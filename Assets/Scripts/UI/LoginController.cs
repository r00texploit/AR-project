using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.UI
{
    /// <summary>
    /// Login/signup screen with Firebase Auth integration.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button btnSignIn;
        [SerializeField] private Button btnSignUp;
        [SerializeField] private Button btnDemoMode;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text toggleAuthText; // "Need account?" / "Have account?"
        [SerializeField] private GameObject signInPanel;
        [SerializeField] private GameObject signUpPanel;

        private bool _isSignUpMode = false;

        void Start()
        {
            btnSignIn?.onClick.AddListener(SignIn);
            btnSignUp?.onClick.AddListener(SignUp);
            btnDemoMode?.onClick.AddListener(DemoMode);

            if (FirebaseRestService.Instance != null)
                FirebaseRestService.Instance.OnAuthResult.AddListener(OnAuthResult);

            SetPanel(false); // Start in sign-in mode
        }

        private void SignIn()
        {
            if (string.IsNullOrWhiteSpace(emailInput.text) ||
                string.IsNullOrWhiteSpace(passwordInput.text))
            {
                SetStatus("Enter email and password.");
                return;
            }

            SetStatus("Signing in...");
            btnSignIn.interactable = false;
            FirebaseRestService.Instance.SignIn(emailInput.text, passwordInput.text);
        }

        private void SignUp()
        {
            if (string.IsNullOrWhiteSpace(emailInput.text) ||
                string.IsNullOrWhiteSpace(passwordInput.text))
            {
                SetStatus("Enter email and password.");
                return;
            }

            if (passwordInput.text.Length < 6)
            {
                SetStatus("Password must be at least 6 characters.");
                return;
            }

            SetStatus("Signing up...");
            btnSignUp.interactable = false;
            FirebaseRestService.Instance.SignUp(emailInput.text, passwordInput.text);
        }

        private void DemoMode()
        {
            SetStatus("Demo mode: no Firebase required.");
            FirebaseRestService.Instance.CurrentUserId = "demo_user";
            FirebaseRestService.Instance.CurrentUserEmail = "demo@example.com";
            Invoke(nameof(GoToMainMenu), 0.5f);
        }

        private void OnAuthResult(bool success, string message)
        {
            btnSignIn.interactable = true;
            btnSignUp.interactable = true;
            SetStatus(message);

            if (success)
                Invoke(nameof(GoToMainMenu), 1f);
        }

        private void GoToMainMenu()
        {
            SceneLoader.Instance?.LoadScene(SceneLoader.SceneMainMenu);
        }

        private void SetPanel(bool isSignUp)
        {
            _isSignUpMode = isSignUp;
            if (signInPanel != null) signInPanel.SetActive(!isSignUp);
            if (signUpPanel != null) signUpPanel.SetActive(isSignUp);
            if (toggleAuthText != null)
                toggleAuthText.text = isSignUp ? "Already have an account?" : "Need an account?";
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        void OnDestroy()
        {
            if (FirebaseRestService.Instance != null)
                FirebaseRestService.Instance.OnAuthResult.RemoveListener(OnAuthResult);
        }
    }
}
