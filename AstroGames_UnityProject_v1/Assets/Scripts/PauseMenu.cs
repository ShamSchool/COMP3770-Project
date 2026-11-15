using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

//Summary:
//Pause menu opened with escape or clickable pause button.

public class PauseMenu : MonoBehaviour
{

    [SerializeField] private GameObject pauseScreen;

    public static bool paused {get; private set;}

    private void Start()
    {

        ResumeGame();

    }

    private void Update()
    {
        //Check keyboard input
        if(Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {

            if (paused)
                ResumeGame();
            else
                PauseGame();

        }

    }

    public void PauseGame()
    {

        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        paused = true;

    }

    public void ResumeGame()
    {

        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        paused = false;

    }

}