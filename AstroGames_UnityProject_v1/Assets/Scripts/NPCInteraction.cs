
using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialoguePage
{
    [TextArea(3, 5)]
    public string dialogueText;

    public AudioClip dialogueAudio;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Station Engineer";

    [Header("Dialogue Pages")]
    public DialoguePage[] dialoguePages;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text npcNameText;
    public TMP_Text continuePrompt;
    public TMP_Text pageIndicator;
    public GameObject interactionPrompt;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;

    private bool playerInRange = false;
    private bool dialogueOpen = false;
    private bool isTyping = false;

    private int currentPage = 0;
    private Coroutine typingCoroutine;

    void Start()
    {
        dialoguePanel.SetActive(false);
        interactionPrompt.SetActive(false);
    }

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



