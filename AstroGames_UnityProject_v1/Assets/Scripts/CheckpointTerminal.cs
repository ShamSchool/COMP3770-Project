using UnityEngine;
using TMPro;

public class CheckpointTerminal : MonoBehaviour
{
    [Header("Terminal Type")]
    [SerializeField] private bool resetToStartTerminal = false;

    [Header("Checkpoint Settings")]
    [SerializeField] private Transform respawnPoint;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;

    private bool playerInRange = false;
    private bool terminalActivated = false;

    private PlayerRespawn playerRespawn;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && !terminalActivated &&
            Input.GetKeyDown(KeyCode.F))
        {
            ActivateTerminal();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        playerRespawn = other.GetComponent<PlayerRespawn>();

        if (playerRespawn == null)
        {
            playerRespawn = other.GetComponentInParent<PlayerRespawn>();
        }

        if (!terminalActivated && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);

            if (promptText != null)
            {
                if (resetToStartTerminal)
                {
                    promptText.text = "Press F to return respawn to level start";
                }
                else
                {
                    promptText.text = "Press F to activate checkpoint";
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void ActivateTerminal()
    {
        if (playerRespawn == null)
        {
            Debug.LogWarning("Could not find PlayerRespawn on the player!");
            return;
        }

        // Special terminal: reset the respawn location to the level start
        if (resetToStartTerminal)
        {
            playerRespawn.ResetToStart();

            Debug.Log("Respawn location reset to level start!");
        }
        else
        {
            // Regular terminal: save this terminal as a checkpoint
            if (respawnPoint == null)
            {
                Debug.LogWarning("Checkpoint RespawnPoint has not been assigned!");
                return;
            }

            playerRespawn.SetCheckpoint(respawnPoint);

            Debug.Log("Checkpoint activated successfully!");
        }

        terminalActivated = true;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        if (promptText != null)
        {
            if (resetToStartTerminal)
            {
                promptText.text = "Level start respawn restored!";
            }
            else
            {
                promptText.text = "Checkpoint activated!";
            }
        }
    }
}