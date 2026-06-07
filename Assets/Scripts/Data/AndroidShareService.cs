using System.IO;
using UnityEngine;

namespace AREducation.Data
{
    public static class AndroidShareService
    {
        public static bool SharePdf(string filePath, string title)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject file = new AndroidJavaObject("java.io.File", filePath);
                string packageName = activity.Call<string>("getPackageName");
                using AndroidJavaClass reportShare = new AndroidJavaClass("com.areducation.share.ReportShare");
                using AndroidJavaObject uri = reportShare.CallStatic<AndroidJavaObject>(
                    "getUriForFile", activity, packageName + ".fileprovider", file);

                using AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                intent.Call<AndroidJavaObject>("setType", "application/pdf");
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uri);
                intent.Call<AndroidJavaObject>("addFlags", 1);

                using AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                using AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>(
                    "createChooser", intent, string.IsNullOrWhiteSpace(title) ? "Share report" : title);
                activity.Call("startActivity", chooser);
                return true;
            }
            catch (System.Exception ex)
            {
                DiagnosticsLogger.Log($"Unable to share report: {ex.Message}");
                return false;
            }
#else
            DiagnosticsLogger.Log($"Report exported to {filePath}");
            return true;
#endif
        }
    }
}
