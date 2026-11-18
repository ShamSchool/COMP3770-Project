using UnityEngine;

/// <summary>
/// Moves a UI prop and wraps it to the opposite side when it crosses configured bounds.
/// </summary>
/// <remarks>
/// Uses a RectTransform to move and wrap a UI prop.
/// </remarks>
public class MovingProps : MonoBehaviour
{

    /// <summary>
    /// Movement speed in anchored UI units per second.
    /// </summary>
    [SerializeField] private float speed = 10f;
    /// <summary>
    /// Positive horizontal boundary used for left/right wrapping.
    /// </summary>
    [SerializeField] private float loopX = 1100f;
    /// <summary>
    /// Positive vertical boundary used for top/bottom wrapping.
    /// </summary>
    [SerializeField] private float loopY = 650f;
    /// <summary>
    /// Direction normalized before calculating the movement offset.
    /// </summary>
    [SerializeField] private Vector2 direction = new Vector2(-1, -0.5f);

    /// <summary>
    /// Moves the RectTransform in anchored space and wraps coordinates that cross the configured bounds.
    /// </summary>
    private void Update()
    {

        RectTransform rectangle = (RectTransform)transform;

        //Movement
        Vector2 position = rectangle.anchoredPosition + direction.normalized * speed * Time.deltaTime;

        //Loop movement
        if (position.x < -loopX)
            position.x = loopX;

        if (position.x > loopX)
            position.x = -loopX;

        if (position.y < -loopY)
            position.y = loopY;

        if (position.y > loopY)
            position.y = -loopY;

        rectangle.anchoredPosition = position;

    }
}