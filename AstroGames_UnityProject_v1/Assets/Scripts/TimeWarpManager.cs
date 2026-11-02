using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TimeWarpManager : MonoBehaviour
{

    [Header("Era Objects")]
    [SerializeField] private GameObject[] presentObjects;
    [SerializeField] private GameObject[] pastObjects;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap[] presentSolidTilemaps;
    [SerializeField] private Tilemap[] pastSolidTilemaps;

    [Header("Settings")]
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private bool startInPast = false;

    public bool InPast { get; private set; }

    private float lastSwapTime = -99f;

    public static event System.Action<bool> TimeSwappedEvent;

    //Set correct time when game starts
    private void Start()
    {

        InPast = startInPast;
        SwapObjects();

    }

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
    public void SwapInput(InputAction.CallbackContext context)
    {

        if (context.performed && Time.time - lastSwapTime >= cooldown)
        {

            SwapCheck();

        }

    }

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

    public bool PlayerTargetTimeCollision(Tilemap map)
    {

        Bounds bound = playerCollider.bounds;

        //Shrink bound so standing on a floor doesn't cause swapping to fail
        bound.Expand(-0.05f);

        //Find grid cells and the bottom-left and top-right corners of the player's collider
        Vector3Int bottomLeftCell = Vector3Int.FloorToInt(map.transform.InverseTransformPoint(bound.min));
        Vector3Int topRightCell = Vector3Int.FloorToInt(map.transform.InverseTransformPoint(bound.max));

        //Iterate through every cell the player covers and check for collision
        for (int x = bottomLeftCell.x; x <= topRightCell.x; x++)
        {

            for (int y = bottomLeftCell.y; y <= topRightCell.y; y++)
            {
                
                //Check cell for a tile
                if(map.HasTile(new Vector3Int(x, y, 0)))
                {

                    return true;

                }

            }

        }

        Debug.Log(map.name + ": checked cells " + bottomLeftCell + " to " + topRightCell + ", no tile found");
        return false;
    }

}