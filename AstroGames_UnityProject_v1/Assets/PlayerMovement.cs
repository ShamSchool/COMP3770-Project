using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls horizontal movement, jumping, ground detection, and falling gravity for the player.
/// </summary>
/// <remarks>
/// Uses Rigidbody2D and Unity Input System actions. Checks PauseMenu.paused when handling a jump.
/// </remarks>
/// <seealso cref="PauseMenu"/>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Physics body whose velocity and gravity scale are controlled by this component.
    /// </summary>
    public Rigidbody2D rb;
    /// <summary>
    /// Horizontal speed applied to the physics body in world units per second.
    /// </summary>
    [Header("Movement")]
    public float moveSpeed = 5f;
    /// <summary>
    /// Latest horizontal input value read by Move.
    /// </summary>
    float horizontalMovement;

    /// <summary>
    /// Upward velocity assigned when a jump action is performed.
    /// </summary>
    [Header("Jumping")]
    public float jumpPower = 10f;
    /// <summary>
    /// Jump allowance restored whenever the ground check detects ground.
    /// </summary>
    public int maxJumps = 2;
    /// <summary>
    /// Remaining jump allowance; the current implementation decrements it on handled jump presses and releases.
    /// </summary>
    int jumpsRemaining;

    /// <summary>
    /// Transform marking the centre of the ground-detection overlap box.
    /// </summary>
    [Header("GroundCheck")]
    public Transform groundCheckPos;
    /// <summary>
    /// Width and height of the ground-detection box in world units.
    /// </summary>
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    /// <summary>
    /// Layers accepted as ground by the overlap-box check.
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// Gravity scale used while the player is not falling.
    /// </summary>
    [Header("Gravity")]
    public float baseGravity = 2;
    /// <summary>
    /// Maximum downward speed enforced while falling.
    /// </summary>
    public float maxFallSpeed = 18f;
    /// <summary>
    /// Multiplier applied to baseGravity when vertical velocity is negative.
    /// </summary>
    public float fallSpeedMultiplier = 2f;

    /// <summary>
    /// Applies horizontal velocity and refreshes ground detection and falling gravity each frame.
    /// </summary>
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        GroundCheck();
        Gravity();
    }

    /// <summary>
    /// Applies stronger gravity while falling and limits downward velocity; otherwise restores baseGravity.
    /// </summary>
    public void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // fall increasingly faster 
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    /// <summary>
    /// Reads horizontal movement from a Vector2 input action.
    /// </summary>
    /// <param name="context">Unity Input System callback containing the movement action value.</param>
    public void Move(InputAction.CallbackContext context)
    {
        // movement on the x axis aka horizontal 
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    /// <summary>
    /// Handles jump presses and releases while jump allowance remains and the game is not paused.
    /// </summary>
    /// <param name="context">Unity Input System callback identifying the jump action phase.</param>
    /// <seealso cref="PauseMenu"/>
    public void Jump(InputAction.CallbackContext context)
    {
        if (PauseMenu.paused) 
            return;
        if (jumpsRemaining > 0)
        {
            if (context.performed)
            {
                // hold down jump button = full jump height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining--;
            }
            else if (context.canceled)
            {
                // light tap of jump button = half of the height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
            }
        }
    }

    /// <summary>
    /// Restores the jump allowance when the ground-check box overlaps a collider on groundLayer.
    /// </summary>
    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
        }
    }

    /// <summary>
    /// Draws the ground-check box in the Scene view when this object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}
