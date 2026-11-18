
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Original level starting position.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Respawn when the player's world Y position falls below this value.")]
    [SerializeField] private float fallThreshold = -12f;

    private Rigidbody2D body;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Current location where the player will respawn
    private Vector3 currentRespawnPosition;
    private Quaternion currentRespawnRotation;

    private bool respawnRequested;

    public static event System.Action PlayerDiedEvent;

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
    public void ResetToStart()
    {
        currentRespawnPosition = initialPosition;
        currentRespawnRotation = initialRotation;

        Debug.Log("Respawn location reset to level start!");
    }

    // Request a player respawn
    public void RequestRespawn()
    {
        if (isActiveAndEnabled)
        {
            respawnRequested = true;
        }
    }

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

    private void OnDisable()
    {
        respawnRequested = false;
    }
}

