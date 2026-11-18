using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// Switches configured objects between past and present after checking target-era tile occupancy.
/// </summary>
/// <remarks>
/// Uses the player collider and target-era tilemaps. Successful swaps notify WarpAudio through TimeSwappedEvent.
/// </remarks>
/// <seealso cref="WarpAudio"/>
/// <seealso cref="PlayerMovement"/>
public class TimeWarpManager : MonoBehaviour
{

    /// <summary>
    /// Objects enabled in the present and disabled in the past.
    /// </summary>
    [Header("Era Objects")]
    [SerializeField] private GameObject[] presentObjects;
    /// <summary>
    /// Objects enabled in the past and disabled in the present.
    /// </summary>
    [SerializeField] private GameObject[] pastObjects;

    /// <summary>
    /// Present-era tilemaps checked before entering the present.
    /// </summary>
    [Header("Tilemaps")]
    [SerializeField] private Tilemap[] presentSolidTilemaps;
    /// <summary>
    /// Past-era tilemaps checked before entering the past.
    /// </summary>
    [SerializeField] private Tilemap[] pastSolidTilemaps;

    /// <summary>
    /// Player collider whose bounds define the target-era occupancy check.
    /// </summary>
    [Header("Settings")]
    [SerializeField] private Collider2D playerCollider;
    /// <summary>
    /// Minimum seconds between successful swaps initiated through SwapInput.
    /// </summary>
    [SerializeField] private float cooldown = 0.5f;
    /// <summary>
    /// Whether the initial timeline is the past.
    /// </summary>
    [SerializeField] private bool startInPast = false;

    /// <summary>
    /// True when the manager is in the past; false when it is in the present.
    /// </summary>
    public bool InPast { get; private set; }

    /// <summary>
    /// Time.time value recorded at the last successful swap.
    /// </summary>
    private float lastSwapTime = -99f;

    /// <summary>
    /// Raised after a successful swap with the new timeline state.
    /// </summary>
    /// <seealso cref="WarpAudio.Warp"/>
    public static event System.Action<bool> TimeSwappedEvent;

    //Set correct time when game starts
    /// <summary>
    /// Sets the initial timeline and applies its object activation states.
    /// </summary>
    private void Start()
    {

        InPast = startInPast;
        SwapObjects();

    }

    /// <summary>
    /// Enables objects belonging to the current timeline and disables objects in the other timeline.
    /// </summary>
    private void SwapObjects()
    {

        foreach (GameObject obj in presentObjects)
        {
            //Turn on if not in the past, off if in past
            if (obj != null)
            {

                obj.SetActive(!InPast);

            }

        }

        foreach (GameObject obj in pastObjects)
        {
            //Turn on if in the past, off if not in past
            if (obj != null)
            {

                obj.SetActive(InPast);

            }

        }

    }

    //Keyboard Input
    /// <summary>
    /// Attempts a swap when the input action is performed and its cooldown has elapsed.
    /// </summary>
    /// <param name="context">Unity Input System callback containing the swap action phase.</param>
    public void SwapInput(InputAction.CallbackContext context)
    {

        if (context.performed && Time.time - lastSwapTime >= cooldown)
        {

            SwapCheck();

        }

    }

    /// <summary>
    /// Attempts a timeline swap after checking target-era tiles; direct calls do not enforce the input cooldown.
    /// </summary>
    public void SwapCheck()
    {

        //Select objects of the time being swapped into
        Tilemap[] targetTimeObjects = InPast ? presentSolidTilemaps : pastSolidTilemaps;

        foreach (Tilemap map in targetTimeObjects)
        {

            //If player would collide with and object in the other time
            if(map != null && PlayerTargetTimeCollision(map))
            {

                Debug.Log("Time swap failed: player is in an unsafe position in the other world. Add message to game screen to inform player later.");
                return;


            }

        }

        //Safety Check passed (swap)
        InPast = !InPast;
        lastSwapTime = Time.time;
        SwapObjects();

        //Notify other scripts time change (screen animation and other scripts to come later)
        TimeSwappedEvent?.Invoke(InPast);

    }

    /// <summary>
    /// Vertical offset added to the lower checked bound to reduce floor interference.
    /// </summary>
    [SerializeField] private float feetIgnore = 0.6f;
    /// <summary>
    /// Checks whether the adjusted player bounds cover occupied cells in a target-era tilemap.
    /// </summary>
    /// <param name="map">Target-era tilemap to inspect for occupied cells.</param>
    /// <returns>True if any checked cell contains a tile; otherwise false.</returns>
    public bool PlayerTargetTimeCollision(Tilemap map)
    {

        Bounds bound = playerCollider.bounds;

        //Shrink bound so standing on a floor doesn't cause swapping to fail
        bound.Expand(-0.05f);
        Vector3 checkFloorCollision = bound.min + new Vector3(0, feetIgnore, 0);

        //Find grid cells and the bottom-left and top-right corners of the player's collider
        Vector3Int bottomLeftCell = Vector3Int.FloorToInt(map.transform.InverseTransformPoint(checkFloorCollision));
        Vector3Int topRightCell = Vector3Int.FloorToInt(map.transform.InverseTransformPoint(bound.max));

        //Iterate through every cell the player covers and check for collision
        for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
        {

            for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
            {
                
                //Check cell for a tile
                if(map.HasTile(new Vector3Int(x, y, 0)))
                {
                    Debug.Log("Blocked by " + map.name + " at cell (" + x + ", " + y + "), checking " + bottomLeftCell + " to " + topRightCell);
                    return true;

                }

            }

        }

        Debug.Log(map.name + ": checked cells " + bottomLeftCell + " to " + topRightCell + ", no tile found");
        return false;
    }

}