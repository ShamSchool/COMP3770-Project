using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Summary:
//Main menu with a level select screen
//Creates a button for each level with a preview image and number

public class MainMenu: MonoBehaviour
{

    private int levelCount = 10;
    private string prefix = "Level_";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject selectLevelPanel;

    [Header("Buttons")]
    [SerializeField] private Transform levelButtonGrid;
    [SerializeField] private LevelButton levelButtonPrefab;

    [Header("Previews")]
    [SerializeField] private Sprite[] levelPreviews;
    [SerializeField] private Sprite placeholderPreview;

    private void Start()
    {

        CreateLevelButtons();
        BackButton();

    }

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
    public void PlayButton()
    {

        mainPanel.SetActive(false);
        selectLevelPanel.SetActive(true);

    }


    //Back Button
    public void BackButton()
    {

        mainPanel.SetActive(true);
        selectLevelPanel.SetActive(false);

    }

    //Quit Button
    public void QuitButton()
    {

        Application.Quit();

    }

}
