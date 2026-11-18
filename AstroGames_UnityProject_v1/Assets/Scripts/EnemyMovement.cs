using UnityEngine;
/// <summary>
/// Moves an enemy horizontally and reverses direction at walls or ledges.
/// </summary>
/// <remarks>
/// Uses an assigned Rigidbody2D and SpriteRenderer. Ground and Hazard layers are checked to detect obstacles and supporting surfaces.
/// </remarks>
public class EnemyMovement : MonoBehaviour
{
    /// <summary>
    /// Physics body moved by this component.
    /// </summary>
    [SerializeField] private Rigidbody2D rigidBody;
    /// <summary>
    /// Renderer used to measure the enemy and flip its facing direction.
    /// </summary>
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Horizontal movement speed in world units per second.
    /// </summary>
    [SerializeField] private float speed = 1f;
    /// <summary>
    /// Initial movement direction; use 1 for right or -1 for left.
    /// </summary>
    [SerializeField] private int startDirection = 1;
    /// <summary>
    /// Current horizontal direction multiplier.
    /// </summary>
    private int currentDirection;
    /// <summary>
    /// Half the sprite width recorded at startup for obstacle checks.
    /// </summary>
    private float halfWidth;
    /// <summary>
    /// Half the sprite height recorded at startup for obstacle and ledge checks.
    /// </summary>
    private float halfHeight;
    /// <summary>
    /// Velocity applied while preserving the current vertical velocity.
    /// </summary>
    private Vector2 movement;

    /// <summary>
    /// Records sprite dimensions and sets the starting direction and facing.
    /// </summary>
    private void Start()
    {

        halfWidth = spriteRenderer.bounds.extents.x;
        halfHeight = spriteRenderer.bounds.extents.y;
        currentDirection = startDirection;
        spriteRenderer.flipX = startDirection == 1 ? false : true;

    }

    /// <summary>
    /// Applies horizontal velocity and checks whether to reverse direction.
    /// </summary>
    private void FixedUpdate()
    {

        movement.x = speed * currentDirection;
        movement.y = rigidBody.linearVelocity.y;
        rigidBody.linearVelocity = movement;

        SetDirection();

    }

    /// <summary>
    /// Uses a box cast and downward raycast to turn at obstacles or missing ground.
    /// </summary>
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
