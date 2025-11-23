using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    // ---------- Movement ----------
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float arriveRadius = 0.6f;

    // ---------- Dialogue ----------
    [Header("Interaction")]
    public float talkRadius = 4f;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [TextArea] public string customerOrder = "Can you make me a hearty meal with lots of meat?";
    [TextArea] public string satisfiedText = "That was so delicious!";
    [TextArea] public string wrongItemText = "That's not what I ordered...";

    // ---------- Targets ----------
    [Header("Targets")]
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform player;

    // ---------- Auto-wire by tags ----------
    [Header("Auto-Wire Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string waypointTag = "Waypoint1";
    [SerializeField] private string waypoint2Tag = "Waypoint2";
    [SerializeField] private string dialoguePanelTag = "DialoguePanel";
    [SerializeField] private string dialogueTextTag = "DialogueText";

    private UnityEngine.AI.NavMeshAgent agent;
    private bool isTalking = false;
    private bool hasEaten = false;
    private bool canInteract = true;

    private enum State { Arriving, Waiting, Leaving }
    private State state = State.Arriving;

    // ---------- Random Requests Pool ----------
    private readonly string[] randomRequests = new string[]
    {
        "Can you cook me something warm and comforting? Maybe with a bit of spice?",
        "I'm starving! Got any fresh vegetables or soup?",
        "Give me your best dish — surprise me!",
        "Could you make something super duper spicy?",
        "I'm craving something crunchy and a little salty.",
        "I'd love a stew — earthy flavors, lots of umami!",
        "Do you have anything sweet? I could use dessert.",
        "I want something that smells and tastes fishy.",
        "Make me something hearty — the last customer said that was the best!",
        "I'll take something that's elegant."
    };

#if UNITY_EDITOR
    private void Reset() { TryAutoWire(true); }
    [ContextMenu("Auto-Wire Now")] private void AutoWireMenu() { TryAutoWire(true); }
#endif

    private void Awake()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.speed = moveSpeed;

        TryAutoWire(false);

        customerOrder = randomRequests[Random.Range(0, randomRequests.Length)];
    }

    private void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);

        if (waypoint2) agent.Warp(waypoint2.position);

        if (waypoint1)
            StartCoroutine(ArriveThenWait());
        else
            Debug.LogWarning("[CustomerAI] No Waypoint1 assigned!");
    }

    private IEnumerator ArriveThenWait()
    {
        state = State.Arriving;
        canInteract = false;
        yield return MoveTo(waypoint1);
        state = State.Waiting;
        canInteract = true;
    }

    private void Update()
    {
        if (!player || !canInteract) return;

        // Only allow interaction in Waiting state
        if (state != State.Waiting) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= talkRadius && Input.GetKeyDown(KeyCode.E))
        {
            if (!isTalking)
            {
                // Check if player is holding a completed dish
                if (TryGetHeldDish(out CompletedDish dish))
                {
                    CustomerSatisfiedWithDish(dish);
                }
                else if (TryGetHeldItem(out Transform held))
                {
                    // Player gave us something else (wrong item)
                    CustomerReceivedWrongItem(held);
                }
                else
                {
                    // No item held, show order dialogue
                    OpenDialogue(customerOrder);
                }
            }
            else
            {
                CloseDialogue();
            }
        }
    }

    // ---------- Helpers ----------
    private void TryAutoWire(bool editorOnly)
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }
        if (!waypoint1)
        {
            var w = GameObject.FindGameObjectWithTag(waypointTag);
            if (w) waypoint1 = w.transform;
        }
        if (!waypoint2)
        {
            var w2 = GameObject.FindGameObjectWithTag(waypoint2Tag);
            if (w2) waypoint2 = w2.transform;
        }
        if (!dialoguePanel)
        {
            var dp = GameObject.FindGameObjectWithTag(dialoguePanelTag);
            if (dp) dialoguePanel = dp;
        }
        if (!dialogueText)
        {
            var dt = GameObject.FindGameObjectWithTag(dialogueTextTag);
            if (dt) dialogueText = dt.GetComponent<TextMeshProUGUI>();
        }
    }

    // === Check for completed dish (burger) ===
    private bool TryGetHeldDish(out CompletedDish heldDish)
    {
        heldDish = null;
        if (!player) return false;

        var playerInteraction = player.GetComponent<IngredientInteraction>();
        if (!playerInteraction) return false;

        heldDish = playerInteraction.GetHeldDish();
        return heldDish != null;
    }

    // === ANY held item detection (for wrong items) ===
    private bool TryGetHeldItem(out Transform heldItem)
    {
        heldItem = null;
        if (!player) return false;

        Transform holdPoint = player.Find("Rig/root/hips/spine/chest/upperarm.r/lowerarm.r/wrist.r/hand.r/handslot.r/2H_Staff/hold_point");
        if (!holdPoint) return false;

        if (holdPoint.childCount > 0)
        {
            heldItem = holdPoint.GetChild(0);
            return heldItem && heldItem.gameObject.activeInHierarchy;
        }
        return false;
    }

    private void OpenDialogue(string message)
    {
        if (!dialoguePanel || !dialogueText) return;
        dialoguePanel.SetActive(true);
        dialogueText.text = message;
        isTalking = true;

        var pm = player ? player.GetComponent<PlayerMovement>() : null;
        if (pm) pm.canMove = false;
    }

    private void CloseDialogue()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        isTalking = false;

        var pm = player ? player.GetComponent<PlayerMovement>() : null;
        if (pm) pm.canMove = true;
    }

    // ---------- Customer receives correct dish (burger) ----------
    private void CustomerSatisfiedWithDish(CompletedDish dish)
    {
        if (hasEaten) return;
        hasEaten = true;
        canInteract = false;

        // Get points from the dish
        int points = dish.GetPointValue();

        // Destroy the dish
        if (dish != null)
        {
            Destroy(dish.gameObject);
        }

        // Clear the player's held dish reference
        var playerInteraction = player.GetComponent<IngredientInteraction>();
        if (playerInteraction)
        {
            playerInteraction.ClearHeldDish();
        }

        // Add points
        ScoreManager.Instance?.AddPoints(points);
        Debug.Log($"Customer satisfied! Awarded {points} points!");

        OpenDialogue(satisfiedText);
        StartCoroutine(LeaveSequence());
    }

    // ---------- Customer receives wrong item ----------
    private void CustomerReceivedWrongItem(Transform heldItem)
    {
        // Optionally destroy the wrong item or just reject it
        // For now, just show a message
        OpenDialogue(wrongItemText);
        Debug.Log("Customer didn't want this item!");
    }

    // ---------- Legacy method for raw ingredients (backwards compatibility) ----------
    private void CustomerSatisfied(Transform heldItem)
    {
        if (hasEaten) return;
        hasEaten = true;
        canInteract = false;

        if (heldItem) Destroy(heldItem.gameObject);

        ScoreManager.Instance?.AddPoints(30);
        OpenDialogue(satisfiedText);

        StartCoroutine(LeaveSequence());
    }

    private IEnumerator LeaveSequence()
    {
        yield return new WaitForSeconds(2f);
        CloseDialogue();

        if (waypoint2)
        {
            state = State.Leaving;
            canInteract = false;
            yield return MoveTo(waypoint2);
        }
        else
        {
            Debug.LogWarning("[CustomerAI] No Waypoint2 assigned!");
        }

        Destroy(gameObject);
    }

    private IEnumerator MoveTo(Transform target)
    {
        if (!target) yield break;

        agent.isStopped = false;
        agent.SetDestination(target.position);
        while (agent.pathPending) yield return null;

        float stop = Mathf.Max(agent.stoppingDistance, arriveRadius);
        while (agent.remainingDistance > stop) yield return null;

        agent.isStopped = true;
    }

    public void Initialize(Transform playerRef, Transform wp1, Transform wp2,
                           GameObject panel, TextMeshProUGUI text)
    {
        player = playerRef;
        waypoint1 = wp1;
        waypoint2 = wp2;
        dialoguePanel = panel;
        dialogueText = text;

        if (dialoguePanel && dialoguePanel.activeSelf) dialoguePanel.SetActive(false);
    }
}