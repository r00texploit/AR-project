#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using TMPro;
using AREducation.AR;
using AREducation.Data;
using AREducation.Lessons;
using AREducation.Quiz;
using AREducation.Teacher;
using AREducation.UI;
using AREducation.Utils;

/// <summary>
/// Editor utility that builds all four AR Education scenes programmatically.
/// Run from: AR Education > Setup All Scenes
/// </summary>
public class AREducationSceneSetup : Editor
{
    // ─── Entry points ──────────────────────────────────────────────────────────

    [MenuItem("AR Education/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        SetupMainMenuScene();
        SetupARLessonScene();
        SetupQuizScene();
        SetupTeacherDashboardScene();
        SetupBuildSettings();

        EditorUtility.DisplayDialog("AR Education Setup Complete",
            "All four scenes have been created and saved.\n\n" +
            "Next steps:\n" +
            "1. Edit > Project Settings > XR Plug-in Management\n" +
            "   ► Android tab: enable ARCore\n" +
            "   ► iOS tab:     enable ARKit\n" +
            "2. File > Build Settings > Switch Platform > Android\n" +
            "3. Build and Run on an ARCore-capable device.",
            "Got it!");
    }

    [MenuItem("AR Education/1 – Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Persistent singletons ────────────────────────────────────
        CreateSingleton<DataManager>("DataManager");
        var slGO     = CreateSingleton<SceneLoader>("SceneLoader");
        var slCanvas = BuildLoadingCanvas(slGO);
        SetField(slGO.GetComponent<SceneLoader>(), "loadingCanvas", slCanvas.GetComponent<Canvas>());

        // ── EventSystem ──────────────────────────────────────────────
        CreateEventSystem();

        // ── Main Canvas ──────────────────────────────────────────────
        var (canvas, canvasGO) = BuildScreenCanvas("MainMenuCanvas");
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode           = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution   = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight    = 0.5f;

        // Background panel
        var bg = CreatePanel(canvasGO.transform, "Background",
            new Color(0.09f, 0.12f, 0.18f), FullRect());

        // Title
        CreateTMPText(canvasGO.transform, "TitleText", "AR Math & Physics",
            40, FontStyles.Bold, Color.white,
            new Rect(-300, 200, 600, 80));

        // Subtitle
        CreateTMPText(canvasGO.transform, "SubtitleText", "Interactive Learning with AR",
            22, FontStyles.Normal, new Color(0.7f, 0.85f, 1f),
            new Rect(-250, 145, 500, 40));

        // Welcome text
        var welcome = CreateTMPText(canvasGO.transform, "WelcomeText", "Welcome, Student!",
            20, FontStyles.Normal, Color.white,
            new Rect(-250, 90, 500, 35));

        // Buttons
        var btnAR      = CreateButton(canvasGO.transform, "BtnStartAR",      "Start AR Lesson",  -30,  new Color(0.2f,0.6f,1f));
        var btnQuiz    = CreateButton(canvasGO.transform, "BtnQuizzes",       "Take a Quiz",      -110, new Color(0.2f,0.8f,0.4f));
        var btnTeacher = CreateButton(canvasGO.transform, "BtnTeacher",       "Teacher Dashboard",-190, new Color(0.9f,0.5f,0.1f));
        var btnSettings= CreateButton(canvasGO.transform, "BtnSettings",      "Settings",         -270, new Color(0.5f,0.5f,0.6f));

        // Lesson selector panel
        var lessonPanel  = BuildSubPanel(canvasGO.transform, "LessonSelectPanel",
            new Color(0.1f,0.15f,0.25f,0.95f));
        var btnLTri  = CreateButton(lessonPanel.transform, "BtnLessonTriangle", "Triangle Lesson", 60, new Color(0.3f,0.7f,1f));
        var btnLCube = CreateButton(lessonPanel.transform, "BtnLessonCube",     "Cube Lesson",     -20, new Color(0.3f,0.8f,0.5f));
        var btnLPhy  = CreateButton(lessonPanel.transform, "BtnLessonPhysics",  "Physics Lesson",  -100, new Color(1f,0.6f,0.1f));
        var btnCLP   = CreateButton(lessonPanel.transform, "BtnCloseLesson",    "✕ Close",          -170, new Color(0.5f,0.2f,0.2f));
        lessonPanel.SetActive(false);

        // Quiz selector panel
        var quizPanel  = BuildSubPanel(canvasGO.transform, "QuizSelectPanel",
            new Color(0.1f,0.15f,0.25f,0.95f));
        var btnQTri  = CreateButton(quizPanel.transform, "BtnQuizTriangle", "Triangle Quiz", 60,   new Color(0.3f,0.7f,1f));
        var btnQPhy  = CreateButton(quizPanel.transform, "BtnQuizPhysics",  "Physics Quiz",  -20,  new Color(1f,0.6f,0.1f));
        var btnQCube = CreateButton(quizPanel.transform, "BtnQuizCube",     "Cube Quiz",     -100, new Color(0.3f,0.8f,0.5f));
        var btnCQP   = CreateButton(quizPanel.transform, "BtnCloseQuiz",    "✕ Close",        -170, new Color(0.5f,0.2f,0.2f));
        quizPanel.SetActive(false);

        // Settings panel
        var settingsPanel = BuildSubPanel(canvasGO.transform, "SettingsPanel",
            new Color(0.1f,0.15f,0.25f,0.95f));
        CreateTMPText(settingsPanel.transform, "SettingsTitle", "Settings",
            28, FontStyles.Bold, Color.white, new Rect(-200,120,400,50));
        var nameInputGO = CreateInputField(settingsPanel.transform, "StudentNameInput",
            "Enter your name...", 40);
        var arToggleGO  = CreateToggle(settingsPanel.transform, "ARToggle", "Enable AR Mode", -20);
        var btnSave     = CreateButton(settingsPanel.transform, "BtnSaveSettings", "Save", -110, new Color(0.2f,0.7f,0.3f));
        var btnCloseSet = CreateButton(settingsPanel.transform, "BtnCloseSettings","✕ Close", -180, new Color(0.5f,0.2f,0.2f));
        settingsPanel.SetActive(false);

        // ── Wire MainMenuController ─────────────────────────────────
        var ctrlGO  = new GameObject("MainMenuController");
        var ctrl    = ctrlGO.AddComponent<MainMenuController>();
        SetField(ctrl, "btnStartAR",           btnAR.GetComponent<Button>());
        SetField(ctrl, "btnQuizzes",           btnQuiz.GetComponent<Button>());
        SetField(ctrl, "btnTeacher",           btnTeacher.GetComponent<Button>());
        SetField(ctrl, "btnSettings",          btnSettings.GetComponent<Button>());
        SetField(ctrl, "welcomeText",          welcome.GetComponent<TMP_Text>());
        SetField(ctrl, "lessonSelectPanel",    lessonPanel);
        SetField(ctrl, "btnLessonTriangle",    btnLTri.GetComponent<Button>());
        SetField(ctrl, "btnLessonCube",        btnLCube.GetComponent<Button>());
        SetField(ctrl, "btnLessonPhysics",     btnLPhy.GetComponent<Button>());
        SetField(ctrl, "btnCloseLessonPanel",  btnCLP.GetComponent<Button>());
        SetField(ctrl, "quizSelectPanel",      quizPanel);
        SetField(ctrl, "btnQuizTriangle",      btnQTri.GetComponent<Button>());
        SetField(ctrl, "btnQuizPhysics",       btnQPhy.GetComponent<Button>());
        SetField(ctrl, "btnQuizCube",          btnQCube.GetComponent<Button>());
        SetField(ctrl, "btnCloseQuizPanel",    btnCQP.GetComponent<Button>());
        SetField(ctrl, "settingsPanel",        settingsPanel);
        SetField(ctrl, "nameInput",            nameInputGO.GetComponent<TMP_InputField>());
        SetField(ctrl, "arToggle",             arToggleGO.GetComponent<Toggle>());
        SetField(ctrl, "btnSaveSettings",      btnSave.GetComponent<Button>());
        SetField(ctrl, "btnCloseSettings",     btnCloseSet.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[AREducation] MainMenu scene saved.");
    }

    [MenuItem("AR Education/2 – Setup AR Lesson Scene")]
    public static void SetupARLessonScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── AR Core Objects ───────────────────────────────────────────
        var arSessionGO = new GameObject("AR Session");
        arSessionGO.AddComponent<ARSession>();
        arSessionGO.AddComponent<ARInputManager>();

        var xrOriginGO = new GameObject("XR Origin");
        var xrOrigin   = xrOriginGO.AddComponent<XROrigin>();

        // Camera offset
        var camOffsetGO = new GameObject("Camera Offset");
        camOffsetGO.transform.parent = xrOriginGO.transform;

        // AR Camera
        var arCamGO = new GameObject("AR Camera");
        arCamGO.transform.parent = camOffsetGO.transform;
        var arCam  = arCamGO.AddComponent<Camera>();
        arCam.clearFlags      = CameraClearFlags.Color;
        arCam.backgroundColor = Color.black;
        arCam.nearClipPlane   = 0.1f;
        arCam.tag             = "MainCamera";
        arCamGO.AddComponent<ARCameraManager>();
        arCamGO.AddComponent<ARCameraBackground>();
        arCamGO.AddComponent<AudioListener>();

        xrOrigin.Camera      = arCam;
        xrOrigin.CameraFloorOffsetObject = camOffsetGO;

        // AR managers on XR Origin
        var raycastMgr = xrOriginGO.AddComponent<ARRaycastManager>();
        var planeMgr   = xrOriginGO.AddComponent<ARPlaneManager>();

        // ── AR controllers ────────────────────────────────────────────
        var placementGO  = new GameObject("ARPlacementManager");
        var placement    = placementGO.AddComponent<ARPlacementManager>();
        SetField(placement, "raycastManager", raycastMgr);
        SetField(placement, "planeManager",   planeMgr);

        var arSessionCtrl    = arSessionGO.AddComponent<ARSessionController>();
        SetField(arSessionCtrl, "arSession", arSessionGO.GetComponent<ARSession>());

        // ── Lesson Objects ────────────────────────────────────────────
        var triangleRoot = BuildTriangleLessonObject(xrOriginGO.transform);
        var physicsRoot  = BuildPhysicsLessonObject(xrOriginGO.transform);
        var cubeRoot     = BuildCubeLessonObject(xrOriginGO.transform);

        // ── Lesson Manager ─────────────────────────────────────────────
        var lmGO = new GameObject("LessonManager");
        var lm   = lmGO.AddComponent<LessonManager>();
        SetField(lm, "triangleLesson",  triangleRoot.GetComponent<TriangleLessonController>());
        SetField(lm, "cubeLesson",      cubeRoot.GetComponent<CubeLessonController>());
        SetField(lm, "physicsLesson",   physicsRoot.GetComponent<PhysicsLessonController>());
        SetField(lm, "placementManager", placement);

        // ── HUD Canvas ────────────────────────────────────────────────
        var (_, hudCanvasGO) = BuildScreenCanvas("HUDCanvas");
        var hudScaler        = hudCanvasGO.GetComponent<CanvasScaler>();
        hudScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        hudScaler.referenceResolution = new Vector2(1080, 1920);
        hudScaler.matchWidthOrHeight  = 0.5f;

        // Top bar
        var topBar     = CreatePanel(hudCanvasGO.transform, "TopBar",
            new Color(0,0,0,0.6f), new Rect(-540, 830, 1080, 100));
        var titleTxt   = CreateTMPText(topBar.transform, "LessonTitle", "Triangle Lesson",
            26, FontStyles.Bold, Color.white, new Rect(-200, -20, 400, 50));
        var backBtn    = CreateButton(topBar.transform, "BackButton", "< Back", 0,
            new Color(0.6f,0.2f,0.2f), width: 160, height: 55, anchorX: -420, anchorY: 0);

        // Lesson switcher
        var switcher  = CreatePanel(hudCanvasGO.transform, "LessonSwitcher",
            new Color(0,0,0,0.5f), new Rect(-540, 720, 1080, 70));
        var swTriBtn  = CreateSmallButton(switcher.transform, "SwTriangle", "Triangle", -280, 0);
        var swCubeBtn = CreateSmallButton(switcher.transform, "SwCube",     "Cube",      0, 0);
        var swPhyBtn  = CreateSmallButton(switcher.transform, "SwPhysics",  "Physics",  280, 0);

        // Placement hint
        var hintPanel = CreatePanel(hudCanvasGO.transform, "PlacementHint",
            new Color(0,0,0,0.7f), new Rect(-300, 50, 600, 80));
        var hintTxt   = CreateTMPText(hintPanel.transform, "HintText",
            "Scan a flat surface, then tap to place.",
            18, FontStyles.Normal, Color.white, new Rect(-280, -20, 560, 60));

        // Toggle button
        var toggleBtn = CreateButton(hudCanvasGO.transform, "BtnToggleControls",
            "Show Controls", -720, new Color(0.2f,0.4f,0.7f));

        // Quiz button
        var quizBtn = CreateButton(hudCanvasGO.transform, "BtnStartQuiz",
            "Take Quiz", -800, new Color(0.2f,0.7f,0.3f));

        // ── Triangle Control Panel ─────────────────────────────────────
        var triPanel = CreatePanel(hudCanvasGO.transform, "TriangleControlPanel",
            new Color(0.05f,0.1f,0.2f,0.92f), new Rect(240, -50, 580, 700));
        triPanel.SetActive(false);

        CreateTMPText(triPanel.transform, "TriTitle", "Triangle",
            24, FontStyles.Bold, Color.white, new Rect(-240, 295, 480, 45));

        var (sliderA, lblA) = CreateLabeledSlider(triPanel.transform, "SideA", "A = 3.0", 230);
        var (sliderB, lblB) = CreateLabeledSlider(triPanel.transform, "SideB", "B = 4.0", 140);
        var (sliderC, lblC) = CreateLabeledSlider(triPanel.transform, "SideC", "C = 5.0",  50);

        var formulaTri = CreateTMPText(triPanel.transform, "FormulaTri",
            "P = A + B + C", 18, FontStyles.Normal, new Color(0.9f,0.9f,0.5f),
            new Rect(-240, -30, 480, 40));
        var perimTxt   = CreateTMPText(triPanel.transform, "PerimeterTxt",
            "Perimeter = 12.00", 18, FontStyles.Bold, Color.white,
            new Rect(-240, -80, 480, 40));
        var areaTxt    = CreateTMPText(triPanel.transform, "AreaTxt",
            "Area = 6.00", 18, FontStyles.Normal, Color.white,
            new Rect(-240, -130, 480, 40));
        var warnTxt    = CreateTMPText(triPanel.transform, "WarnTxt",
            "Invalid triangle!", 16, FontStyles.Normal, new Color(1f,0.4f,0.4f),
            new Rect(-240, -180, 480, 35));
        warnTxt.gameObject.SetActive(false);
        var explTxt    = CreateTMPText(triPanel.transform, "ExplTxt",
            "When a side grows longer,\nthe perimeter increases.",
            14, FontStyles.Normal, new Color(0.7f,0.9f,0.7f),
            new Rect(-240, -260, 480, 60));

        // Wire TriangleLessonController
        var triCtrl = triangleRoot.GetComponent<TriangleLessonController>();
        SetField(triCtrl, "sliderA",         sliderA.GetComponent<Slider>());
        SetField(triCtrl, "sliderB",         sliderB.GetComponent<Slider>());
        SetField(triCtrl, "sliderC",         sliderC.GetComponent<Slider>());
        SetField(triCtrl, "labelSideA",      lblA.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelSideB",      lblB.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelSideC",      lblC.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelPerimeter",  perimTxt.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelArea",       areaTxt.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelFormula",    formulaTri.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelExplanation",explTxt.GetComponent<TMP_Text>());
        SetField(triCtrl, "labelWarning",    warnTxt.GetComponent<TMP_Text>());
        // MeshFilter on the 3D object
        var triMeshGO = triangleRoot.transform.Find("TriangleMesh");
        if (triMeshGO != null)
        {
            SetField(triCtrl, "triangleMeshFilter", triMeshGO.GetComponent<MeshFilter>());
            var lr = triangleRoot.transform.Find("Outline")?.GetComponent<LineRenderer>();
            if (lr != null) SetField(triCtrl, "perimeterOutline", lr);
        }

        // ── Physics Control Panel ──────────────────────────────────────
        var phyPanel = CreatePanel(hudCanvasGO.transform, "PhysicsControlPanel",
            new Color(0.05f,0.1f,0.2f,0.92f), new Rect(240, -50, 580, 680));
        phyPanel.SetActive(false);
        CreateTMPText(phyPanel.transform, "PhyTitle", "Physics: d = v × t",
            22, FontStyles.Bold, Color.white, new Rect(-240, 295, 480, 45));

        var speedTxt = CreateTMPText(phyPanel.transform, "SpeedTxt", "Speed:   2.0 m/s",
            18, FontStyles.Normal, Color.white, new Rect(-240, 220, 480, 40));
        var timeTxt  = CreateTMPText(phyPanel.transform, "TimeTxt",  "Time:    0.00 s",
            18, FontStyles.Normal, Color.white, new Rect(-240, 170, 480, 40));
        var distTxt  = CreateTMPText(phyPanel.transform, "DistTxt",  "Distance: 0.00 m",
            18, FontStyles.Normal, Color.white, new Rect(-240, 120, 480, 40));
        var formulaPhy = CreateTMPText(phyPanel.transform, "FormulaPhy",
            "d = v × t\nd = 2.0 × 0.00 = 0.00",
            16, FontStyles.Normal, new Color(0.9f,0.9f,0.5f),
            new Rect(-240, 40, 480, 60));

        var btnSS   = CreateButton(phyPanel.transform, "BtnStartStop","Start",  -60, new Color(0.2f,0.7f,0.3f), width:220);
        var btnRst  = CreateButton(phyPanel.transform, "BtnReset",    "Reset", -140, new Color(0.6f,0.3f,0.2f), width:220);
        var btnSUp  = CreateButton(phyPanel.transform, "BtnSpeedUp",  "Speed ＋", -220, new Color(0.3f,0.5f,0.8f), width:220);
        var btnSDwn = CreateButton(phyPanel.transform, "BtnSlowDown", "Speed －", -300, new Color(0.4f,0.4f,0.6f), width:220);

        var phyCtrl = physicsRoot.GetComponent<PhysicsLessonController>();
        SetField(phyCtrl, "labelSpeed",    speedTxt.GetComponent<TMP_Text>());
        SetField(phyCtrl, "labelTime",     timeTxt.GetComponent<TMP_Text>());
        SetField(phyCtrl, "labelDistance", distTxt.GetComponent<TMP_Text>());
        SetField(phyCtrl, "labelFormula",  formulaPhy.GetComponent<TMP_Text>());
        SetField(phyCtrl, "btnStartStop",  btnSS.GetComponent<Button>());
        SetField(phyCtrl, "btnReset",      btnRst.GetComponent<Button>());
        SetField(phyCtrl, "btnSpeedUp",    btnSUp.GetComponent<Button>());
        SetField(phyCtrl, "btnSlowDown",   btnSDwn.GetComponent<Button>());
        SetField(phyCtrl, "btnStartStopLabel",
            btnSS.GetComponentInChildren<TMP_Text>());

        // ── Cube Control Panel ─────────────────────────────────────────
        var cubePanel = CreatePanel(hudCanvasGO.transform, "CubeControlPanel",
            new Color(0.05f,0.1f,0.2f,0.92f), new Rect(240, -50, 580, 500));
        cubePanel.SetActive(false);
        CreateTMPText(cubePanel.transform, "CubeTitle", "Cube Geometry",
            24, FontStyles.Bold, Color.white, new Rect(-240, 210, 480, 45));

        var faceTxt      = CreateTMPText(cubePanel.transform, "FaceInfoTxt",
            "Tap a face to inspect it.",
            18, FontStyles.Normal, Color.white, new Rect(-240, 140, 480, 60));
        var formulaCube  = CreateTMPText(cubePanel.transform, "FormulaCube",
            "SA = 6 × s²\nV = s³",
            18, FontStyles.Normal, new Color(0.9f,0.9f,0.5f),
            new Rect(-240, 50, 480, 60));
        var (sizeSlider, sizeLbl) = CreateLabeledSlider(cubePanel.transform, "SizeSlider", "Size: 1.00", -20);

        var cubeCtrl = cubeRoot.GetComponent<CubeLessonController>();
        SetField(cubeCtrl, "labelFaceInfo", faceTxt.GetComponent<TMP_Text>());
        SetField(cubeCtrl, "labelFormula",  formulaCube.GetComponent<TMP_Text>());
        SetField(cubeCtrl, "labelSize",     sizeLbl.GetComponent<TMP_Text>());
        SetField(cubeCtrl, "sizeSlider",    sizeSlider.GetComponent<Slider>());
        var cubeMeshGO = cubeRoot.transform.Find("CubeMesh");
        if (cubeMeshGO != null)
        {
            SetField(cubeCtrl, "cubeMeshFilter",  cubeMeshGO.GetComponent<MeshFilter>());
            SetField(cubeCtrl, "cubeMeshRenderer",cubeMeshGO.GetComponent<MeshRenderer>());
        }
        var cubeColliderComp = cubeRoot.GetComponent<BoxCollider>();
        if (cubeColliderComp != null) SetField(cubeCtrl, "cubeCollider", cubeColliderComp);

        // ── HUD Controller ─────────────────────────────────────────────
        var hudCtrlGO = new GameObject("ARLessonHUDController");
        var hudCtrl   = hudCtrlGO.AddComponent<ARLessonHUDController>();
        SetField(hudCtrl, "lessonTitleText",       titleTxt.GetComponent<TMP_Text>());
        SetField(hudCtrl, "backButton",            backBtn.GetComponent<Button>());
        SetField(hudCtrl, "btnSelectTriangle",     swTriBtn.GetComponent<Button>());
        SetField(hudCtrl, "btnSelectCube",         swCubeBtn.GetComponent<Button>());
        SetField(hudCtrl, "btnSelectPhysics",      swPhyBtn.GetComponent<Button>());
        SetField(hudCtrl, "placementHintPanel",    hintPanel);
        SetField(hudCtrl, "hintText",              hintTxt.GetComponent<TMP_Text>());
        SetField(hudCtrl, "triangleControlPanel",  triPanel);
        SetField(hudCtrl, "physicsControlPanel",   phyPanel);
        SetField(hudCtrl, "cubeControlPanel",      cubePanel);
        SetField(hudCtrl, "btnStartQuiz",          quizBtn.GetComponent<Button>());
        SetField(hudCtrl, "btnToggleControls",     toggleBtn.GetComponent<Button>());
        SetField(hudCtrl, "btnToggleLabel",        toggleBtn.GetComponentInChildren<TMP_Text>());
        SetField(hudCtrl, "placementManager",      placement);
        SetField(hudCtrl, "lessonManager",         lm);

        // ── EventSystem ───────────────────────────────────────────────
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/ARLesson.unity");
        Debug.Log("[AREducation] ARLesson scene saved.");
    }

    [MenuItem("AR Education/3 – Setup Quiz Scene")]
    public static void SetupQuizScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>().backgroundColor = new Color(0.09f,0.12f,0.18f);
        cam.AddComponent<AudioListener>();
        cam.tag = "MainCamera";

        CreateEventSystem();

        var (_, canvasGO) = BuildScreenCanvas("QuizCanvas");
        var scaler        = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;

        CreatePanel(canvasGO.transform, "Background",
            new Color(0.09f,0.12f,0.18f), FullRect());

        // Lesson select panel
        var selectPanel = BuildSubPanel(canvasGO.transform, "LessonSelectPanel",
            new Color(0.1f,0.15f,0.25f,0.95f));
        CreateTMPText(selectPanel.transform, "SelectTitle", "Choose a Quiz",
            30, FontStyles.Bold, Color.white, new Rect(-250,160,500,60));
        var selTri  = CreateButton(selectPanel.transform, "BtnTriangleQuiz", "Triangle Quiz",  70, new Color(0.3f,0.7f,1f));
        var selPhy  = CreateButton(selectPanel.transform, "BtnPhysicsQuiz",  "Physics Quiz",   -20, new Color(1f,0.6f,0.1f));
        var selCube = CreateButton(selectPanel.transform, "BtnCubeQuiz",     "Cube Quiz",      -110, new Color(0.3f,0.8f,0.5f));

        // Quiz panel
        var quizPanelGO = CreatePanel(canvasGO.transform, "QuizPanel",
            new Color(0.09f,0.12f,0.18f,0.98f), FullRect());
        var progressTxt = CreateTMPText(quizPanelGO.transform, "ProgressText", "Q 1 / 5",
            20, FontStyles.Normal, new Color(0.7f,0.7f,0.7f),
            new Rect(-200, 750, 400, 40));
        var questionTxt = CreateTMPText(quizPanelGO.transform, "QuestionText",
            "What is the perimeter of a triangle with sides 3, 4, 5?",
            24, FontStyles.Bold, Color.white,
            new Rect(-440, 580, 880, 140));

        Color[] optColors = {
            new Color(0.2f,0.4f,0.8f), new Color(0.15f,0.5f,0.7f),
            new Color(0.2f,0.35f,0.7f),new Color(0.15f,0.45f,0.75f)
        };
        var optBtns = new Button[4];
        var optTxts = new TMP_Text[4];
        string[] optLabels = { "A)  12", "B)  10", "C)  14", "D)  8" };
        for (int i = 0; i < 4; i++)
        {
            var ob  = CreateButton(quizPanelGO.transform, $"Option_{i}", optLabels[i],
                380 - i * 120, optColors[i], width: 860, height: 100);
            optBtns[i] = ob.GetComponent<Button>();
            optTxts[i] = ob.GetComponentInChildren<TMP_Text>();
        }

        var feedbackTxt = CreateTMPText(quizPanelGO.transform, "FeedbackText", "Correct!",
            28, FontStyles.Bold, new Color(0.2f,0.9f,0.3f),
            new Rect(-300, -260, 600, 50));
        feedbackTxt.gameObject.SetActive(false);

        var explanationTxt = CreateTMPText(quizPanelGO.transform, "ExplanationText",
            "Perimeter = A + B + C = 3+4+5 = 12",
            18, FontStyles.Normal, Color.white,
            new Rect(-400, -320, 800, 60));
        explanationTxt.gameObject.SetActive(false);

        var nextBtn = CreateButton(quizPanelGO.transform, "NextButton", "Next Question →",
            -420, new Color(0.2f,0.6f,1f));
        nextBtn.GetComponent<Button>().gameObject.SetActive(false);

        // Result panel
        var resultPanelGO = CreatePanel(canvasGO.transform, "ResultPanel",
            new Color(0.07f,0.1f,0.16f,0.98f), FullRect());
        resultPanelGO.SetActive(false);
        CreateTMPText(resultPanelGO.transform, "ResultTitle", "Quiz Complete!",
            34, FontStyles.Bold, Color.white, new Rect(-300, 250, 600, 70));
        var scoreTxt  = CreateTMPText(resultPanelGO.transform, "ScoreText", "You scored 4 / 5!",
            28, FontStyles.Bold, new Color(0.9f,0.9f,0.3f),
            new Rect(-300, 150, 600, 55));
        var ratingTxt = CreateTMPText(resultPanelGO.transform, "RatingText", "Excellent! 🌟",
            24, FontStyles.Normal, Color.white,
            new Rect(-300, 80, 600, 50));
        var retryBtn  = CreateButton(resultPanelGO.transform, "RetryButton", "Retry Quiz",
            -30, new Color(0.2f,0.6f,1f));
        var menuBtn   = CreateButton(resultPanelGO.transform, "MenuButton", "Main Menu",
            -120, new Color(0.4f,0.4f,0.5f));

        // Back button
        var backBtn = CreateButton(canvasGO.transform, "BackButton", "< Back",
            850, new Color(0.4f,0.2f,0.2f), width: 150, height: 60, anchorX: -450, anchorY: 0);

        // QuizManager
        var qmGO = new GameObject("QuizManager");
        qmGO.AddComponent<QuizManager>();

        // QuizUIController
        var quizUIGO = new GameObject("QuizUIController");
        var quizUI   = quizUIGO.AddComponent<QuizUIController>();
        SetField(quizUI, "lessonSelectPanel", selectPanel);
        SetField(quizUI, "btnTriangleQuiz",   selTri.GetComponent<Button>());
        SetField(quizUI, "btnPhysicsQuiz",    selPhy.GetComponent<Button>());
        SetField(quizUI, "btnCubeQuiz",       selCube.GetComponent<Button>());
        SetField(quizUI, "quizPanel",         quizPanelGO);
        SetField(quizUI, "progressText",      progressTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "questionText",      questionTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "optionButtons",     optBtns);
        SetField(quizUI, "optionTexts",       optTxts);
        SetField(quizUI, "feedbackText",      feedbackTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "explanationText",   explanationTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "nextButton",        nextBtn.GetComponent<Button>());
        SetField(quizUI, "resultPanel",       resultPanelGO);
        SetField(quizUI, "scoreText",         scoreTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "ratingText",        ratingTxt.GetComponent<TMP_Text>());
        SetField(quizUI, "retryButton",       retryBtn.GetComponent<Button>());
        SetField(quizUI, "menuButton",        menuBtn.GetComponent<Button>());
        SetField(quizUI, "backButton",        backBtn.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Quiz.unity");
        Debug.Log("[AREducation] Quiz scene saved.");
    }

    [MenuItem("AR Education/4 – Setup Teacher Dashboard Scene")]
    public static void SetupTeacherDashboardScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>().backgroundColor = new Color(0.07f,0.1f,0.16f);
        cam.AddComponent<AudioListener>();
        cam.tag = "MainCamera";

        CreateEventSystem();

        var (_, canvasGO) = BuildScreenCanvas("DashboardCanvas");
        var scaler        = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;

        CreatePanel(canvasGO.transform, "Background",
            new Color(0.07f,0.1f,0.16f), FullRect());

        // Header
        CreateTMPText(canvasGO.transform, "Header", "Teacher Dashboard",
            32, FontStyles.Bold, Color.white, new Rect(-400, 840, 800, 60));

        // Summary texts
        var summaryTxt = CreateTMPText(canvasGO.transform, "SummaryText", "0 results shown",
            18, FontStyles.Normal, new Color(0.7f,0.7f,0.7f),
            new Rect(-400, 780, 500, 35));
        var avgTxt = CreateTMPText(canvasGO.transform, "AverageText", "Average Score: 0%",
            18, FontStyles.Normal, new Color(0.9f,0.9f,0.5f),
            new Rect(-400, 740, 500, 35));

        // Filter buttons
        var filterPanel = CreatePanel(canvasGO.transform, "FilterPanel",
            new Color(0,0,0,0.3f), new Rect(-540, 670, 1080, 65));
        var btnAll  = CreateSmallButton(filterPanel.transform, "BtnAll",      "All",      -390, 0);
        var btnTri  = CreateSmallButton(filterPanel.transform, "BtnTriangle", "Triangle", -130, 0);
        var btnPhy  = CreateSmallButton(filterPanel.transform, "BtnPhysics",  "Physics",   130, 0);
        var btnCube = CreateSmallButton(filterPanel.transform, "BtnCube",     "Cube",      390, 0);

        // Scroll view for result rows
        var scrollGO    = new GameObject("ScrollView");
        scrollGO.transform.SetParent(canvasGO.transform, false);
        var scrollRect  = scrollGO.AddComponent<ScrollRect>();
        var scrollRT    = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin         = new Vector2(0f, 0f);
        scrollRT.anchorMax         = new Vector2(1f, 1f);
        scrollRT.offsetMin         = new Vector2(20, 80);
        scrollRT.offsetMax         = new Vector2(-20, -290);

        var viewportGO  = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRT  = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero; viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero; viewportRT.offsetMax = Vector2.zero;
        var viewportMask = viewportGO.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

        var contentGO   = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRT   = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing           = 4f;
        vlg.childControlHeight  = true;
        vlg.childControlWidth   = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content    = contentRT;
        scrollRect.viewport   = viewportRT;
        scrollRect.horizontal = false;
        scrollRect.vertical   = true;
        scrollRect.scrollSensitivity = 30f;

        // Row prefab (used as template)
        var rowPrefab = BuildResultRowPrefab();

        // Back button
        var backBtn = CreateButton(canvasGO.transform, "BackButton", "< Back",
            -870, new Color(0.4f,0.2f,0.2f));

        // Dashboard controller
        var dctrlGO = new GameObject("TeacherDashboardController");
        var dctrl   = dctrlGO.AddComponent<TeacherDashboardController>();
        SetField(dctrl, "btnAll",           btnAll.GetComponent<Button>());
        SetField(dctrl, "btnTriangle",      btnTri.GetComponent<Button>());
        SetField(dctrl, "btnPhysics",       btnPhy.GetComponent<Button>());
        SetField(dctrl, "btnCube",          btnCube.GetComponent<Button>());
        SetField(dctrl, "listContent",      contentGO.transform);
        SetField(dctrl, "resultRowPrefab",  rowPrefab);
        SetField(dctrl, "summaryText",      summaryTxt.GetComponent<TMP_Text>());
        SetField(dctrl, "averageText",      avgTxt.GetComponent<TMP_Text>());
        SetField(dctrl, "backButton",       backBtn.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TeacherDashboard.unity");
        Debug.Log("[AREducation] TeacherDashboard scene saved.");
    }

    // ─── Lesson 3D Object Builders ──────────────────────────────────────────

    private static GameObject BuildTriangleLessonObject(Transform parent)
    {
        var root = new GameObject("TriangleLesson");
        root.transform.SetParent(parent, false);

        var meshGO = new GameObject("TriangleMesh");
        meshGO.transform.SetParent(root.transform, false);
        meshGO.AddComponent<MeshFilter>();
        var mr   = meshGO.AddComponent<MeshRenderer>();
        var mat  = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.6f, 1f);
        mr.material = mat;

        var outlineGO = new GameObject("Outline");
        outlineGO.transform.SetParent(root.transform, false);
        var lr = outlineGO.AddComponent<LineRenderer>();
        lr.startWidth = 0.008f; lr.endWidth = 0.008f;
        lr.loop = false;
        lr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.white };
        lr.positionCount = 4;

        root.AddComponent<TriangleLessonController>();
        root.AddComponent<ARObjectManipulator>();
        root.SetActive(false);
        return root;
    }

    private static GameObject BuildPhysicsLessonObject(Transform parent)
    {
        var root = new GameObject("PhysicsLesson");
        root.transform.SetParent(parent, false);

        // Track/rail indicator
        var trackGO  = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trackGO.name = "Track";
        trackGO.transform.SetParent(root.transform, false);
        trackGO.transform.localScale = new Vector3(0.02f, 1f, 0.02f);
        trackGO.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        trackGO.transform.localPosition = new Vector3(0.15f, 0f, 0f);
        DestroyImmediate(trackGO.GetComponent<Collider>());
        trackGO.GetComponent<MeshRenderer>().material =
            new Material(Shader.Find("Standard")) { color = new Color(0.4f,0.4f,0.5f) };

        // Ball
        var ballGO   = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballGO.name  = "Ball";
        ballGO.transform.SetParent(root.transform, false);
        ballGO.transform.localScale    = Vector3.one * 0.06f;
        ballGO.transform.localPosition = Vector3.zero;
        ballGO.GetComponent<MeshRenderer>().material =
            new Material(Shader.Find("Standard")) { color = new Color(1f,0.4f,0.1f) };

        var trail = ballGO.AddComponent<TrailRenderer>();
        trail.startWidth = 0.02f; trail.endWidth = 0f; trail.time = 1.5f;
        trail.material = new Material(Shader.Find("Sprites/Default"))
            { color = new Color(1f,0.6f,0.2f,0.5f) };

        var ctrl = root.AddComponent<PhysicsLessonController>();
        SetField(ctrl, "ballTransform", ballGO.transform);
        SetField(ctrl, "ballTrail",     trail);

        root.AddComponent<ARObjectManipulator>();
        root.SetActive(false);
        return root;
    }

    private static GameObject BuildCubeLessonObject(Transform parent)
    {
        var root = new GameObject("CubeLesson");
        root.transform.SetParent(parent, false);

        var meshGO = new GameObject("CubeMesh");
        meshGO.transform.SetParent(root.transform, false);
        meshGO.AddComponent<MeshFilter>();
        var mr  = meshGO.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard")) { color = new Color(0.3f,0.6f,1f) };

        // BoxCollider on root for face tap detection
        var col = root.AddComponent<BoxCollider>();
        col.size = Vector3.one * 0.1f;

        root.AddComponent<CubeLessonController>();
        root.AddComponent<ARObjectManipulator>();
        root.SetActive(false);
        return root;
    }

    // ─── Helper: Result Row Prefab ──────────────────────────────────────────

    private static GameObject BuildResultRowPrefab()
    {
        var row = new GameObject("ResultRow");
        var rowRT = row.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(1040, 70);

        var bg = row.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.17f, 0.25f);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childControlHeight  = true;
        hlg.childControlWidth   = false;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth  = false;

        var rowCtrl = row.AddComponent<ResultRowUI>();

        void AddCell(string n, string txt, int w, ref TMP_Text target)
        {
            var go = new GameObject(n);
            go.transform.SetParent(row.transform, false);
            var cellRT = go.AddComponent<RectTransform>();
            cellRT.sizeDelta = new Vector2(w, 60);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text       = txt;
            t.fontSize   = 16;
            t.color      = Color.white;
            t.alignment  = TextAlignmentOptions.MidlineLeft;
            target = t;
        }

        TMP_Text nameTxt = null, lessonTxt = null, scoreTxt = null, dateTxt = null, gradeTxt = null;
        AddCell("Name",   "Alice",         260, ref nameTxt);
        AddCell("Lesson", "Triangle",      180, ref lessonTxt);
        AddCell("Score",  "4/5 (80%)",     180, ref scoreTxt);
        AddCell("Date",   "2024-01-15",    180, ref dateTxt);
        AddCell("Grade",  "B",              80, ref gradeTxt);

        SetField(rowCtrl, "nameText",   nameTxt);
        SetField(rowCtrl, "lessonText", lessonTxt);
        SetField(rowCtrl, "scoreText",  scoreTxt);
        SetField(rowCtrl, "dateText",   dateTxt);
        SetField(rowCtrl, "gradeText",  gradeTxt);

        return row;
    }

    // ─── UI Helpers ─────────────────────────────────────────────────────────

    private static (Canvas, GameObject) BuildScreenCanvas(string name)
    {
        var go     = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return (canvas, go);
    }

    private static GameObject BuildLoadingCanvas(GameObject parent)
    {
        var go     = new GameObject("LoadingCanvas");
        go.transform.SetParent(parent.transform, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        var bg = CreatePanel(go.transform, "LoadingBG",
            new Color(0,0,0,0.8f), FullRect());

        var txt = CreateTMPText(go.transform, "LoadingText", "Loading...",
            28, FontStyles.Bold, Color.white, new Rect(-200, 50, 400, 60));

        go.SetActive(false);
        return go;
    }

    private static GameObject BuildSubPanel(Transform parent, string name, Color bg)
    {
        var go = CreatePanel(parent, name, bg, new Rect(-450, -300, 900, 700));
        return go;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, Rect rect)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rect.x + rect.width  / 2f,
                                          rect.y + rect.height / 2f);
        rt.sizeDelta        = new Vector2(rect.width, rect.height);
        return go;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        float fontSize, FontStyles style, Color color, Rect rect)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text       = text;
        t.fontSize   = fontSize;
        t.fontStyle  = style;
        t.color      = color;
        t.alignment  = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rect.x + rect.width  / 2f,
                                          rect.y + rect.height / 2f);
        rt.sizeDelta        = new Vector2(rect.width, rect.height);
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        float anchorY, Color color, float width = 480, float height = 75,
        float anchorX = 0, float _ignored = 0)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img  = go.AddComponent<Image>();
        img.color = color;
        var btn  = go.AddComponent<Button>();
        var cb   = btn.colors;
        cb.highlightedColor = Color.Lerp(color, Color.white, 0.3f);
        cb.pressedColor     = Color.Lerp(color, Color.black, 0.3f);
        btn.colors = cb;

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(anchorX, anchorY);
        rt.sizeDelta        = new Vector2(width, height);

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<TextMeshProUGUI>();
        t.text       = label;
        t.fontSize   = 22;
        t.fontStyle  = FontStyles.Bold;
        t.color      = Color.white;
        t.alignment  = TextAlignmentOptions.Center;
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        return go;
    }

    private static GameObject CreateSmallButton(Transform parent, string name, string label,
        float anchorX, float anchorY)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f,0.3f,0.5f);
        go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(anchorX, anchorY);
        rt.sizeDelta        = new Vector2(200, 50);
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = 18; t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        return go;
    }

    private static (GameObject slider, GameObject label) CreateLabeledSlider(
        Transform parent, string name, string labelText, float anchorY)
    {
        var container = new GameObject(name + "Container");
        container.transform.SetParent(parent, false);
        var crt = container.AddComponent<RectTransform>();
        crt.anchoredPosition = new Vector2(0, anchorY);
        crt.sizeDelta        = new Vector2(460, 70);

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(container.transform, false);
        var lbl   = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = labelText;
        lbl.fontSize  = 18;
        lbl.color     = Color.white;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchoredPosition = new Vector2(-120, 18);
        lblRT.sizeDelta        = new Vector2(220, 35);

        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(container.transform, false);
        var slider   = sliderGO.AddComponent<Slider>();
        slider.minValue = 0.5f; slider.maxValue = 10f; slider.value = 3f;
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchoredPosition = new Vector2(60, -15);
        sliderRT.sizeDelta        = new Vector2(350, 30);

        // Fill area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        fillAreaGO.AddComponent<RectTransform>();
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.6f, 1f);
        slider.fillRect = fillGO.GetComponent<RectTransform>();

        // Handle
        var handleSlideAreaGO = new GameObject("Handle Slide Area");
        handleSlideAreaGO.transform.SetParent(sliderGO.transform, false);
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleSlideAreaGO.transform, false);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(25, 25);
        slider.handleRect = handleRT;

        slider.targetGraphic = handleImg;

        return (sliderGO, lblGO);
    }

    private static GameObject CreateInputField(Transform parent, string name,
        string placeholder, float anchorY)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.2f, 0.3f);
        var input = go.AddComponent<TMP_InputField>();
        var rt    = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, anchorY);
        rt.sizeDelta        = new Vector2(440, 60);

        var textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(go.transform, false);
        var phGO  = new GameObject("Placeholder");
        phGO.transform.SetParent(textAreaGO.transform, false);
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        phT.text = placeholder; phT.fontSize = 18;
        phT.color = new Color(0.5f,0.5f,0.5f);
        var inputTextGO = new GameObject("Text");
        inputTextGO.transform.SetParent(textAreaGO.transform, false);
        var inpT = inputTextGO.AddComponent<TextMeshProUGUI>();
        inpT.fontSize = 18; inpT.color = Color.white;

        input.textViewport    = textAreaGO.GetComponent<RectTransform>();
        input.textComponent   = inpT;
        input.placeholder     = phT;
        return go;
    }

    private static GameObject CreateToggle(Transform parent, string name, string label, float anchorY)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var toggle = go.AddComponent<Toggle>();
        var rt     = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, anchorY);
        rt.sizeDelta        = new Vector2(440, 50);

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.3f, 0.5f);
        var checkGO = new GameObject("Checkmark");
        checkGO.transform.SetParent(bgGO.transform, false);
        var checkImg = checkGO.AddComponent<Image>();
        checkImg.color = new Color(0.2f, 0.8f, 0.3f);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var lt = labelGO.AddComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = 18; lt.color = Color.white;

        toggle.targetGraphic = bgImg;
        toggle.graphic       = checkImg;
        toggle.isOn          = true;

        return go;
    }

    private static GameObject CreateSingleton<T>(string name) where T : MonoBehaviour
    {
        var go = new GameObject(name);
        go.AddComponent<T>();
        return go;
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private static Rect FullRect() => new Rect(-540, -960, 1080, 1920);

    // ─── Build Settings ─────────────────────────────────────────────────────

    private static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",        true),
            new EditorBuildSettingsScene("Assets/Scenes/ARLesson.unity",        true),
            new EditorBuildSettingsScene("Assets/Scenes/Quiz.unity",            true),
            new EditorBuildSettingsScene("Assets/Scenes/TeacherDashboard.unity",true),
        };
    }

    // ─── Reflection Helper ──────────────────────────────────────────────────

    private static void SetField(object target, string fieldName, object value)
    {
        if (target == null) return;
        var type = target.GetType();
        FieldInfo fi = null;
        while (fi == null && type != null)
        {
            fi = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            type = type.BaseType;
        }
        if (fi == null)
        {
            Debug.LogWarning($"[AREducationSceneSetup] Field '{fieldName}' not found on {target.GetType().Name}");
            return;
        }
        fi.SetValue(target, value);
    }
}
#endif
