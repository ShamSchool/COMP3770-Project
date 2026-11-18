
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Stores one dialogue page and its optional audio clip for an NPC conversation.
/// </summary>
/// <remarks>
/// Reads DialoguePage entries and displays them using TextMesh Pro. Detects a Player-tagged collider and handles F input.
/// </remarks>
/// <seealso cref="NPCInteraction"/>
[System.Serializable]
public class DialoguePage
{
    /// <summary>
    /// Text displayed for this page of the conversation.
    /// </summary>
    [TextArea(3, 5)]
    public string dialogueText;

    /// <summary>
    /// Optional audio clip played while this page is displayed.
    /// </summary>
    public AudioClip dialogueAudio;
}

/// <summary>
/// Shows paged NPC dialogue when a nearby player presses F.
/// </summary>
/// <remarks>
/// Reads DialoguePage entries and displays them using TextMesh Pro. Detects a Player-tagged collider and handles F input.
/// </remarks>
/// <seealso cref="DialoguePage"/>
public class NPCInteraction : MonoBehaviour
{
    /// <summary>
    /// NPC name displayed in the conversation UI.
    /// </summary>
    [Header("NPC Settings")]
    public string npcName = "Station Engineer";

    /// <summary>
    /// Ordered dialogue pages shown during each conversation.
    /// </summary>
    [Header("Dialogue Pages")]
    public DialoguePage[] dialoguePages;

    /// <summary>
    /// Root GameObject for the dialogue UI.
    /// </summary>
    [Header("UI References")]
    public GameObject dialoguePanel;
    /// <summary>
    /// TextMesh Pro label used to reveal the current page character by character.
    /// </summary>
    public TMP_Text dialogueText;
    /// <summary>
    /// TextMesh Pro label displaying npcName.
    /// </summary>
    public TMP_Text npcNameText;
    /// <summary>
    /// Prompt shown once the current page has finished revealing.
    /// </summary>
    public TMP_Text continuePrompt;
    /// <summary>
    /// Label showing the current page number and total page count.
    /// </summary>
    public TMP_Text pageIndicator;
    /// <summary>
    /// Prompt shown when the player is nearby and no conversation is open.
    /// </summary>
    public GameObject interactionPrompt;

    /// <summary>
    /// Optional audio source used for the dialogue page audio clip.
    /// </summary>
    [Header("Audio")]
    public AudioSource audioSource;

    /// <summary>
    /// Scaled seconds to wait between revealing successive dialogue characters.
    /// </summary>
    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;

    /// <summary>
    /// Whether a Player-tagged collider has entered the interaction trigger.
    /// </summary>
    private bool playerInRange = false;
    /// <summary>
    /// Whether the conversation is currently open.
    /// </summary>
    private bool dialogueOpen = false;
    /// <summary>
    /// Whether the current page is still being revealed.
    /// </summary>
    private bool isTyping = false;

    /// <summary>
    /// Zero-based index of the dialogue page being displayed.
    /// </summary>
    private int currentPage = 0;
    /// <summary>
    /// Handle for the active character-reveal coroutine.
    /// </summary>
    private Coroutine typingCoroutine;

    /// <summary>
    /// Initially hides the dialogue panel and interaction prompt.
    /// </summary>
    void Start()
    {
        dialoguePanel.SetActive(false);
        interactionPrompt.SetActive(false);
    }

    /// <summary>
    /// Handles F while in range to open dialogue, reveal the page immediately, or advance to the next page.
    /// </summary>
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (!dialogueOpen)
            {
                StartDialogue();
            }
            else if (isTyping)
            {
                FinishTyping();
            }
            else
            {
                NextDialoguePage();
            }
        }
    }

    /// <summary>
    /// Enables interaction when a Player-tagged collider enters the trigger.
    /// </summary>
    /// <param name="other">Collider entering the interaction area.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!dialogueOpen)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Hides the interaction prompt and closes dialogue when a Player-tagged collider exits.
    /// </summary>
    /// <param name="other">Collider leaving the interaction area.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactionPrompt.SetActive(false);

            if (dialogueOpen)
            {
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Starts at the first dialogue page and shows the NPC name, or logs a warning when no pages are assigned.
    /// </summary>
    void StartDialogue()
    {
        if (dialoguePages == null || dialoguePages.Length == 0)
        {
            Debug.LogWarning("No dialogue pages assigned to this NPC!");
            return;
        }

        dialogueOpen = true;
        currentPage = 0;

        interactionPrompt.SetActive(false);
        dialoguePanel.SetActive(true);

        npcNameText.text = npcName;

        DisplayCurrentPage();
    }

    /// <summary>
    /// Advances to the next page or closes the conversation after the final page.
    /// </summary>
    void NextDialoguePage()
    {
        currentPage++;

        if (currentPage >= dialoguePages.Length)
        {
            EndDialogue();
            return;
        }

        DisplayCurrentPage();
    }

    /// <summary>
    /// Sets the current text and page indicator, plays optional audio, and starts character reveal.
    /// </summary>
    void DisplayCurrentPage()
    {
        DialoguePage page = dialoguePages[currentPage];

        dialogueText.text = page.dialogueText;
        dialogueText.maxVisibleCharacters = 0;

        pageIndicator.text = "Page " +
            (currentPage + 1) + "/" + dialoguePages.Length;

        continuePrompt.gameObject.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Stop();

            if (page.dialogueAudio != null)
            {
                audioSource.clip = page.dialogueAudio;
                audioSource.Play();
            }
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    /// <summary>
    /// Reveals the current page one character at a time and then enables the continue prompt.
    /// </summary>
    /// <returns>Coroutine iterator yielding a scaled wait between characters.</returns>
    IEnumerator TypeDialogue()
    {
        isTyping = true;

        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(typingSpeed);
        }

        FinishTyping();
    }

    /// <summary>
    /// Stops character reveal, shows all characters on the page, and enables the continue prompt.
    /// </summary>
    void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;

        continuePrompt.gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the conversation, stops character reveal and audio, and restores the interaction prompt if still in range.
    /// </summary>
    void EndDialogue()
    {
        dialogueOpen = false;
        isTyping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePanel.SetActive(false);

        if (playerInRange)
        {
            interactionPrompt.SetActive(true);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}



