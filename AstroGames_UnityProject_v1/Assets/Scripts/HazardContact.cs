using UnityEngine;

[DisallowMultipleComponent]
public class HazardContact : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        RespawnPlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        RespawnPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RespawnPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        RespawnPlayer(other);
    }

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
