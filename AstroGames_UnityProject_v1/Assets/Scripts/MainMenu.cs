using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Summary:
//Main menu with a level select screen
//Creates a button for each level with a preview image and number

/// <summary>
/// Displays the main and level-selection panels and creates buttons for numbered level scenes.
/// </summary>
/// <remarks>
/// Creates LevelButton instances from a prefab and enables buttons for available scenes.
/// </remarks>
/// <seealso cref="LevelButton"/>
/// <seealso cref="MovingBackground"/>
/// <seealso cref="MovingProps"/>
public class MainMenu: MonoBehaviour
{

    /// <summary>
    /// Number of level-selection buttons to create, including unavailable levels.
    /// </summary>
    private int levelCount = 10;
    /// <summary>
    /// Prefix combined with each level number to form a scene name.
    /// </summary>
    private string prefix = "Level_";

    /// <summary>
    /// Root panel shown on the main menu screen.
    /// </summary>
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    /// <summary>
    /// Root panel containing the level selector.
    /// </summary>
    [SerializeField] private GameObject selectLevelPanel;

    /// <summary>
    /// Parent Transform into which level-button instances are created.
    /// </summary>
    [Header("Buttons")]
    [SerializeField] private Transform levelButtonGrid;
    /// <summary>
    /// Prefab providing a LevelButton and its referenced UI controls.
    /// </summary>
    [SerializeField] private LevelButton levelButtonPrefab;

    /// <summary>
    /// Level preview sprites, indexed by level number minus one.
    /// </summary>
    [Header("Previews")]
    [SerializeField] private Sprite[] levelPreviews;
    /// <summary>
    /// Fallback image for levels without an assigned preview.
    /// </summary>
    [SerializeField] private Sprite placeholderPreview;

    /// <summary>
    /// Creates the level buttons and initially shows the main panel.
    /// </summary>
    private void Start()
    {

        CreateLevelButtons();
        BackButton();

    }

    /// <summary>
    /// Creates numbered level buttons, assigns previews, and disables buttons for scenes that cannot be loaded.
    /// </summary>
    /// <seealso cref="LevelButton.LoadLevel"/>
    private void CreateLevelButtons()
    {

        for(int i = 1; i <= levelCount; i++)
        {

            string sceneName = prefix + i;

            //Create button and set number
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelButtonGrid);
            levelButton.number.text = i.ToString();


            //Use assigned image or placeholder if none is assigned
            Sprite previewSprite = i - 1 < levelPreviews.Length ? levelPreviews[i - 1] : null;
            levelButton.preview.sprite = previewSprite != null ? previewSprite : placeholderPreview;


            //Disable buttons for missing levels
            //LATER: Add system to lock levels until player completes previous one.
            bool levelAvailable = Application.CanStreamedLevelBeLoaded(sceneName);
            levelButton.button.interactable = levelAvailable;

            //Darken image for unavailable levels
            levelButton.preview.color = levelAvailable ? Color.white : Color.gray;

            //Load Level
            levelButton.sceneName = sceneName;
            levelButton.button.onClick.AddListener(levelButton.LoadLevel);

        }

    }

    //Play Button
    /// <summary>
    /// Hides the main panel and shows the level-selection panel.
    /// </summary>
    public void PlayButton()
    {

        mainPanel.SetActive(false);
        selectLevelPanel.SetActive(true);

    }


    //Back Button
    /// <summary>
    /// Hides the level-selection panel and shows the main panel.
    /// </summary>
    public void BackButton()
    {

        mainPanel.SetActive(true);
        selectLevelPanel.SetActive(false);

    }

    //Quit Button
    /// <summary>
    /// Requests application exit through Unity Application.Quit.
    /// </summary>
    public void QuitButton()
    {

        Application.Quit();

    }

}
