using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("Optional spawn marker. If empty, use the player's position when the scene starts.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Respawn when the player's world Y position falls below this value.")]
    [SerializeField] private float fallThreshold = -12f;

    private Rigidbody2D body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool respawnRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void RequestRespawn()
    {
        if (isActiveAndEnabled)
        {
            // Multiple contacts in one physics step should only cause one reset.
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
        Vector3 position = spawnPoint != null ? spawnPoint.position : initialPosition;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : initialRotation;

        // Defer teleporting until outside collision callbacks. Keep the current era.
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.position = position;
        body.rotation = rotation.eulerAngles.z;
        transform.SetPositionAndRotation(position, rotation);
        body.WakeUp();
    }

    private void OnDisable()
    {
        respawnRequested = false;
    }
}
