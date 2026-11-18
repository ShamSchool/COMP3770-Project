using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float speed = 1f;
    [SerializeField] private int startDirection = 1;
    private int currentDirection;
    private float halfWidth;
    private float halfHeight;
    private Vector2 movement;

    private void Start()
    {

        halfWidth = spriteRenderer.bounds.extents.x;
        halfHeight = spriteRenderer.bounds.extents.y;
        currentDirection = startDirection;
        spriteRenderer.flipX = startDirection == 1 ? false : true;

    }

    private void FixedUpdate()
    {

        movement.x = speed * currentDirection;
        movement.y = rigidBody.linearVelocity.y;
        rigidBody.linearVelocity = movement;

        SetDirection();

    }

    private void SetDirection()
    {
        Vector2 center = spriteRenderer.bounds.center;
        Vector2 rightPosition = center;
        Vector2 leftPosition = center;

        rightPosition.x += halfWidth;
        leftPosition.x -= halfWidth;

        //Create box as tall as the enemy to detect obstacles at its head and feet
        Vector2 boxSize = new Vector2(0.05f, halfHeight * 2 - 0.2f);
        Vector2 boxCenter = center + new Vector2(0, 0.1f);


        //Check movement direction
        if (rigidBody.linearVelocity.x > 0)
        {

            //Draw a box from the center of the enemy, pointing in the direction of movement and check if it is intersecting with a wall.
            if (Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.right, halfWidth + 0.1f, LayerMask.GetMask("Ground", "Hazard")))
            {

                //Flip direction
                currentDirection *= -1;
                spriteRenderer.flipX = true;

            }
            //Check for a ledge and switch directions
            else if (!Physics2D.Raycast(rightPosition, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Ground", "Hazard"))){

                currentDirection *= -1;
                spriteRenderer.flipX = true;

            }

        }
        else if (rigidBody.linearVelocity.x < 0)
        {

            //Draw a box from the center of the enemy, pointing in the direction of movement and check if it is intersecting with a wall.
            if (Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.left, halfWidth + 0.1f, LayerMask.GetMask("Ground", "Hazard")))
            {

                //Flip direction
                currentDirection *= -1;
                spriteRenderer.flipX = false;

            }
            //Check for a ledge and switch directions
            else if (!Physics2D.Raycast(leftPosition, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Ground", "Hazard")))
            {

                currentDirection *= -1;
                spriteRenderer.flipX = false;

            }

        }

    }

}
