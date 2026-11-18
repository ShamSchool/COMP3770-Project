
using UnityEngine;

/// <summary>
/// Respawns the player at the level start or a saved checkpoint after a hazard contact or fall.
/// </summary>
/// <remarks>
/// Uses Rigidbody2D. HazardContact requests respawning, CheckpointTerminal updates the spawn location, and WarpAudio listens for deaths.
/// </remarks>
/// <seealso cref="HazardContact"/>
/// <seealso cref="CheckpointTerminal"/>
/// <seealso cref="WarpAudio"/>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    /// <summary>
    /// Optional marker used to record the original level-start position and rotation.
    /// </summary>
    [Header("Spawn Settings")]
    [Tooltip("Original level starting position.")]
    [SerializeField] private Transform spawnPoint;

    /// <summary>
    /// World Y position below which the player respawns.
    /// </summary>
    [Tooltip("Respawn when the player's world Y position falls below this value.")]
    [SerializeField] private float fallThreshold = -12f;

    /// <summary>
    /// Player physics body used to reset position and velocity.
    /// </summary>
    private Rigidbody2D body;

    /// <summary>
    /// Original level-start position recorded in Awake.
    /// </summary>
    private Vector3 initialPosition;
    /// <summary>
    /// Original level-start rotation recorded in Awake.
    /// </summary>
    private Quaternion initialRotation;

    // Current location where the player will respawn
    /// <summary>
    /// Saved position for the next respawn.
    /// </summary>
    private Vector3 currentRespawnPosition;
    /// <summary>
    /// Saved rotation for the next respawn.
    /// </summary>
    private Quaternion currentRespawnRotation;

    /// <summary>
    /// Whether a respawn is queued for the next physics update.
    /// </summary>
    private bool respawnRequested;

    /// <summary>
    /// Notifies listeners immediately before the player respawns.
    /// </summary>
    /// <seealso cref="WarpAudio.PlayDeath"/>
    public static event System.Action PlayerDiedEvent;

    /// <summary>
    /// Caches Rigidbody2D and records the starting pose as the first respawn location.
    /// </summary>
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        initialPosition = spawnPoint != null
            ? spawnPoint.position
            : transform.position;

        initialRotation = spawnPoint != null
            ? spawnPoint.rotation
            : transform.rotation;

        // Initially, respawn at the level start
        currentRespawnPosition = initialPosition;
        currentRespawnRotation = initialRotation;
    }

    // Save a new checkpoint
    /// <summary>
    /// Saves a checkpoint position and rotation for future respawns.
    /// </summary>
    /// <param name="checkpoint">Transform to copy as the new respawn pose; null is rejected.</param>
    /// <seealso cref="CheckpointTerminal"/>
    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
        {
            Debug.LogWarning("Checkpoint position is missing!");
            return;
        }

        currentRespawnPosition = checkpoint.position;
        currentRespawnRotation = checkpoint.rotation;

        Debug.Log("Checkpoint saved!");
    }

    // Reset respawn location to the beginning of the level
    /// <summary>
    /// Sets future respawns to the original level-start pose without immediately moving the player.
    /// </summary>
    /// <seealso cref="CheckpointTerminal"/>
    public void ResetToStart()
    {
        currentRespawnPosition = initialPosition;
        currentRespawnRotation = initialRotation;

        Debug.Log("Respawn location reset to level start!");
    }

    // Request a player respawn
    /// <summary>
    /// Queues a respawn when this component is active and enabled.
    /// </summary>
    /// <seealso cref="HazardContact"/>
    public void RequestRespawn()
    {
        if (isActiveAndEnabled)
        {
            respawnRequested = true;
        }
    }

    /// <summary>
    /// Handles queued respawns or falls, announces death, clears velocity, and restores the saved pose.
    /// </summary>
    private void FixedUpdate()
    {
        if (!respawnRequested && body.position.y >= fallThreshold)
        {
            return;
        }

        respawnRequested = false;

        // Notify listeners that the player died
        PlayerDiedEvent?.Invoke();

        // Stop all player movement
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        // Move player to the current respawn position
        body.position = currentRespawnPosition;
        body.rotation = currentRespawnRotation.eulerAngles.z;

        transform.SetPositionAndRotation(
            currentRespawnPosition,
            currentRespawnRotation
        );

        body.WakeUp();
    }

    /// <summary>
    /// Clears any pending respawn request.
    /// </summary>
    private void OnDisable()
    {
        respawnRequested = false;
    }
}

