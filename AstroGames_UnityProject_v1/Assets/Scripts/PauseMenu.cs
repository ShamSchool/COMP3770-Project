using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Summary:
//Pause menu opened with escape or clickable pause button.

/// <summary>
/// Controls pausing, resuming, restarting, and returning to the main menu.
/// </summary>
/// <remarks>
/// Uses a pause-screen GameObject and Time.timeScale. PlayerMovement reads the shared paused flag.
/// </remarks>
/// <seealso cref="PlayerMovement"/>
public class PauseMenu : MonoBehaviour
{

    /// <summary>
    /// UI root enabled while paused and hidden when play resumes.
    /// </summary>
    [SerializeField] private GameObject pauseScreen;

    /// <summary>
    /// Shared pause flag set by this component and read by PlayerMovement.Jump.
    /// </summary>
    public static bool paused {get; private set;}

    /// <summary>
    /// Starts the scene in the resumed state with time scale set to one.
    /// </summary>
    private void Start()
    {

        ResumeLevel();

    }

    /// <summary>
    /// Toggles pause on an Escape key press when a keyboard is available.
    /// </summary>
    private void Update()
    {
        //Check keyboard input
        if(Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {

            if (paused)
                ResumeLevel();
            else
                PauseLevel();

        }

    }

    /// <summary>
    /// Shows the pause screen, sets time scale to zero, and marks the game paused.
    /// </summary>
    public void PauseLevel()
    {

        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        paused = true;

    }

    /// <summary>
    /// Hides the pause screen, sets time scale to one, and clears the pause flag.
    /// </summary>
    public void ResumeLevel()
    {

        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        paused = false;

    }

    /// <summary>
    /// Restores normal time and clears pause before reloading the active scene.
    /// </summary>
    public void RestartLevel()
    {

        Time.timeScale = 1;
        paused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    /// <summary>
    /// Restores normal time and clears pause before loading the Main_Menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {

        Time.timeScale = 1;
        paused = false;

        SceneManager.LoadScene("Main_Menu");

    }

}