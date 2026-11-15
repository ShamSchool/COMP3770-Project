using UnityEngine;
using UnityEngine.UI;

//Moves main menu background repeatedly
public class MovingBackground : MonoBehaviour
{

    [SerializeField] private RawImage image;
    [SerializeField] private float speed = 0.01f;
    [SerializeField] private float x, y; 

    private void Update()
    {

        image.uvRect = new Rect(image.uvRect.position + new Vector2(x, y) * speed * Time.deltaTime, image.uvRect.size);

    }

}