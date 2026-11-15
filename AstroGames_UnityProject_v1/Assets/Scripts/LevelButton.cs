using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

//Summary:
//Used for LevelButton prefab. Holds references to the button's children so LevelSelectMenu doesn't find them by name.
//Loads level when clicked

public class LevelButton : MonoBehaviour
{
    public Button button;
    public Image preview;
    public TMP_Text number;

    public string sceneName;
    
    public void LoadLevel()
    {

        SceneManager.LoadScene(sceneName);

    }

}