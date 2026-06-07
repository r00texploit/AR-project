using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AREducation.Editor
{
    public static class AndroidApkBuilder
    {
        private static readonly NamedBuildTarget AndroidNamedBuildTarget = NamedBuildTarget.Android;

        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/ARLesson.unity",
            "Assets/Scenes/Quiz.unity",
            "Assets/Scenes/TeacherDashboard.unity"
        };

        public static void BuildFromCommandLine()
        {
            string apkPath = GetArgument("-apkPath");
            if (string.IsNullOrWhiteSpace(apkPath))
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                apkPath = Path.Combine(desktop, "AR-Education.apk");
            }

            Build(apkPath);
        }

        public static void Build(string apkPath)
        {
            if (string.IsNullOrWhiteSpace(apkPath))
                throw new ArgumentException("APK path is required.", nameof(apkPath));

            apkPath = Path.GetFullPath(apkPath);
            Directory.CreateDirectory(Path.GetDirectoryName(apkPath) ?? ".");

            ValidateScenes();
            ApplyAndroidSettings();

            Debug.Log("[AndroidApkBuilder] Setting up generated scenes and model assets...");
            AREducationSceneSetup.SetupAllScenes();

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[AndroidApkBuilder] Building APK: {apkPath}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    $"Android APK build failed: {summary.result}. Errors: {summary.totalErrors}, warnings: {summary.totalWarnings}");
            }

            Debug.Log($"[AndroidApkBuilder] APK created successfully: {apkPath}");
        }

        private static void ApplyAndroidSettings()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;

            PlayerSettings.productName = "AR Education";
            PlayerSettings.SetApplicationIdentifier(AndroidNamedBuildTarget, "com.areducation.app");
            PlayerSettings.SetScriptingBackend(AndroidNamedBuildTarget, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        }

        private static void ValidateScenes()
        {
            string missing = string.Join(", ", ScenePaths.Where(path => !File.Exists(path)));
            if (!string.IsNullOrEmpty(missing))
                throw new FileNotFoundException($"Missing build scenes: {missing}");
        }

        private static string GetArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }

            return null;
        }
    }
}
