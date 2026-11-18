using UnityEngine;
using TMPro;

/// <summary>
/// Lets a nearby player press F once to save a checkpoint or restore the level-start respawn location.
/// </summary>
/// <remarks>
/// Uses a 2D trigger and the Player tag to find PlayerRespawn. Optional UI references display interaction and activation messages.
/// </remarks>
/// <seealso cref="PlayerRespawn"/>
public class CheckpointTerminal : MonoBehaviour
{
    /// <summary>
    /// True to restore the original respawn location; false to save a checkpoint.
    /// </summary>
    [Header("Terminal Type")]
    [SerializeField] private bool resetToStartTerminal = false;

    /// <summary>
    /// Position and rotation saved by a regular checkpoint terminal.
    /// </summary>
    [Header("Checkpoint Settings")]
    [SerializeField] private Transform respawnPoint;

    /// <summary>
    /// Optional UI object displaying the interaction prompt.
    /// </summary>
    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    /// <summary>
    /// Optional text label displaying the prompt or activation confirmation.
    /// </summary>
    [SerializeField] private TMP_Text promptText;

    /// <summary>
    /// Whether a Player-tagged collider is inside the terminal trigger.
    /// </summary>
    private bool playerInRange = false;
    /// <summary>
    /// Whether this terminal has already been activated.
    /// </summary>
    private bool terminalActivated = false;

    /// <summary>
    /// PlayerRespawn component found on the entering player collider or its parents.
    /// </summary>
    private PlayerRespawn playerRespawn;

    /// <summary>
    /// Hides the interaction prompt when assigned.
    /// </summary>
    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Activates an unused terminal when the nearby player presses F.
    /// </summary>
    private void Update()
    {
        if (playerInRange && !terminalActivated &&
            Input.GetKeyDown(KeyCode.F))
        {
            ActivateTerminal();
        }
    }

    /// <summary>
    /// Finds the entering player and displays the appropriate interaction prompt.
    /// </summary>
    /// <param name="other">Collider entering the terminal trigger; only the Player tag is accepted.</param>
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

    /// <summary>
    /// Clears the range flag and hides the prompt when the player leaves.
    /// </summary>
    /// <param name="other">Collider leaving the terminal trigger; only the Player tag is accepted.</param>
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

    /// <summary>
    /// Updates PlayerRespawn with the selected spawn location and shows confirmation.
    /// </summary>
    /// <seealso cref="PlayerRespawn.SetCheckpoint"/>
    /// <seealso cref="PlayerRespawn.ResetToStart"/>
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