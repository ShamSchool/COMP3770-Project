using UnityEngine;
using UnityEngine.UI;

//Moves main menu background repeatedly
/// <summary>
/// Scrolls a UI RawImage by advancing its texture coordinates each frame.
/// </summary>
/// <remarks>
/// Uses an assigned RawImage for a scrolling background.
/// </remarks>
public class MovingBackground : MonoBehaviour
{

    /// <summary>
    /// UI image whose UV rectangle is scrolled.
    /// </summary>
    [SerializeField] private RawImage image;
    /// <summary>
    /// Multiplier controlling UV-coordinate movement per second.
    /// </summary>
    [SerializeField] private float speed = 0.01f;
    /// <summary>
    /// Horizontal and vertical UV scrolling factors multiplied by speed.
    /// </summary>
    [SerializeField] private float x, y; 

    /// <summary>
    /// Advances the UV position using x, y, speed, and scaled frame time.
    /// </summary>
    private void Update()
    {

        image.uvRect = new Rect(image.uvRect.position + new Vector2(x, y) * speed * Time.deltaTime, image.uvRect.size);

    }

}
/*! \var float MovingBackground::y
 *  \brief Vertical UV scrolling factor multiplied by speed.
 */
