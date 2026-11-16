using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Summary:
//Pause menu opened with escape or clickable pause button.

public class PauseMenu : MonoBehaviour
{

    [SerializeField] private GameObject pauseScreen;

    public static bool paused {get; private set;}

    private void Start()
    {

        ResumeLevel();

    }

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

    public void PauseLevel()
    {

        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        paused = true;

    }

    public void ResumeLevel()
    {

        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        paused = false;

    }

    public void RestartLevel()
    {

        Time.timeScale = 1;
        paused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void ReturnToMainMenu()
    {

        Time.timeScale = 1;
        paused = false;

        SceneManager.LoadScene("Main_Menu");

    }

}