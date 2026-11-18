using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

//Summary:
//Used for LevelButton prefab. Holds references to the button's children so LevelSelectMenu doesn't find them by name.
//Loads level when clicked

/// <summary>
/// Stores the controls and scene name for one level-selection button.
/// </summary>
/// <remarks>
/// MainMenu creates this component, assigns the level details, and connects LoadLevel to the button click.
/// </remarks>
/// <seealso cref="MainMenu"/>
public class LevelButton : MonoBehaviour
{
    /// <summary>
    /// Clickable UI button whose availability and click listener are configured by MainMenu.
    /// </summary>
    public Button button;
    /// <summary>
    /// Image used to display the level preview or its placeholder.
    /// </summary>
    public Image preview;
    /// <summary>
    /// Text label displaying the level number.
    /// </summary>
    public TMP_Text number;

    /// <summary>
    /// Scene name passed to SceneManager when this level is selected.
    /// </summary>
    public string sceneName;
    
    /// <summary>
    /// Loads the scene named by sceneName.
    /// </summary>
    public void LoadLevel()
    {

        SceneManager.LoadScene(sceneName);

    }

}