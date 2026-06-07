using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AREducation.Editor
{
    /// <summary>
    /// Automatically sets up all scenes before every build so the CI pipeline
    /// produces a fully-populated APK without requiring manual Editor interaction.
    /// </summary>
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[BuildPreprocessor] Setting up AR Education scenes...");
            AREducationSceneSetup.SetupAllScenes(false);
            Debug.Log("[BuildPreprocessor] Scene setup complete.");
        }
    }
}
