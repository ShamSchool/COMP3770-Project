using UnityEngine;

/// <summary>
/// Requests player respawning when a collider touches or remains inside this hazard.
/// </summary>
/// <remarks>
/// Finds PlayerRespawn on the contacting collider or its parents and supports both collisions and triggers.
/// </remarks>
/// <seealso cref="PlayerRespawn"/>
[DisallowMultipleComponent]
public class HazardContact : MonoBehaviour
{
    /// <summary>
    /// Checks for a player when a physical collision begins.
    /// </summary>
    /// <param name="collision">Collision containing the other collider.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        RespawnPlayer(collision.collider);
    }

    /// <summary>
    /// Checks for a player while physical contact continues.
    /// </summary>
    /// <param name="collision">Ongoing collision containing the other collider.</param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        RespawnPlayer(collision.collider);
    }

    /// <summary>
    /// Checks for a player when another collider enters the hazard trigger.
    /// </summary>
    /// <param name="other">Collider entering the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        RespawnPlayer(other);
    }

    /// <summary>
    /// Checks for a player while another collider remains in the hazard trigger.
    /// </summary>
    /// <param name="other">Collider remaining inside the trigger.</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        RespawnPlayer(other);
    }

    /// <summary>
    /// Finds PlayerRespawn in the other collider hierarchy and requests a respawn if present.
    /// </summary>
    /// <param name="other">Collider to search, including its parent objects.</param>
    /// <seealso cref="PlayerRespawn.RequestRespawn"/>
    private static void RespawnPlayer(Collider2D other)
    {
        // Use a component lookup so the player does not need a tag or layer change.
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player != null)
        {
            player.RequestRespawn();
        }
    }
}
