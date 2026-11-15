using UnityEngine;

public class MovingProps : MonoBehaviour
{

    [SerializeField] private float speed = 10f;
    [SerializeField] private float loopX = 1100f;
    [SerializeField] private float loopY = 650f;
    [SerializeField] private Vector2 direction = new Vector2(-1, -0.5f);

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