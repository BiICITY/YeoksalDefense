using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class StartSceneLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    private bool isLoading = false;

    private void Update()
    {
        if (isLoading) return;

        bool mouseClicked = Mouse.current != null &&
                            Mouse.current.leftButton.wasPressedThisFrame;

        bool keyPressed = Keyboard.current != null &&
                          Keyboard.current.anyKey.wasPressedThisFrame;

        bool touched = Touchscreen.current != null &&
                       Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (mouseClicked || keyPressed || touched)
        {
            isLoading = true;
            SceneManager.LoadScene(gameSceneName);
        }
    }
}