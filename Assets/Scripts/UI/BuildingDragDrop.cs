using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TGS;
using UnityEngine.EventSystems;

public class BuildingDragDrop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public GameObject prefabToSpawn;
    [SerializeField]
    public float buildingTimeInSeconds = 2;
    [SerializeField]
    public int currencyCost = 500;
    [SerializeField]
    public Text txt_currency;
    [SerializeField]
    public LayerMask mask;

    private ScrollRect scrollRect;
    private bool cursorIsOverTowerSelectionButton;
    private bool isInDragProcess = false;
    private TerrainGridSystem tgs;
    private Transform spawnedObjectToDragTransform;
    private Camera mainCamera;

    /// <summary>
    /// Called when the curser enters the area of the UI element this scriped is attached to
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorIsOverTowerSelectionButton = true;
    }

    /// <summary>
    /// Called when the curser leaves the area of the UI element this scriped is attached to
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        cursorIsOverTowerSelectionButton = false;
    }

    /// <summary>
    /// Called at game start
    /// </summary>
    private void Start()
    {
        // Get the main camera and the grid
        mainCamera = Camera.main;
        tgs = FindObjectOfType<TerrainGridSystem>();
        
        // Get the scrollView of the ui element the element this scriped is attached to is part of
        scrollRect = Utils.GetFirstFoundComponentFromAllParents<ScrollRect>(this.transform);
    }

    /// <summary>
    /// Executed every frame
    /// <br>Checks if a new drag process should start</br>
    /// <br>Handles activ drag processes</br>
    /// <br>Ends or cancles a activ drag process</br>
    /// </summary>
    private void Update()
    {
        // Make sure the currency text in the UI is white if no drag process is activ
        if(!isInDragProcess && txt_currency.color != Color.white)
        {
            txt_currency.color = Color.white;
        }

        // true if the left mouse button is currently pressed and hold
        bool holdLeftMouseBtn = Input.GetKey(KeyCode.Mouse0);

        // Checks if all requirements are set to start a new drag process
        if (cursorIsOverTowerSelectionButton && holdLeftMouseBtn && !isInDragProcess) 
        {
            // Start new drag process
            StartDragProcess();
        }
        // If already a activ drag process is running and the cursor is not on the tower selection button anymore but the left mouse key is not pressed
        else if (!cursorIsOverTowerSelectionButton && !holdLeftMouseBtn && isInDragProcess) 
        {
            // Check if the user is able to pay for the currently dragged object
            if (CanPayForBuilding())
            {
                // Place the currently dragged object and end the drag process
                PlaceTowerAndEndDragProcess();
            }
            else
            {
                // Cancle the current drag process without placing anything
                CancleDragProcess();
            }
        }
        // If the cursor is on the tower selection button, the left mouse key is not pressd or if the user performs a right klick
        else if((cursorIsOverTowerSelectionButton && !holdLeftMouseBtn && isInDragProcess) || !cursorIsOverTowerSelectionButton && Input.GetKey(KeyCode.Mouse1))
        {
            CancleDragProcess();
        }
        // If cursor is not on the tower selection button and there is a activ drag process
        else if(!cursorIsOverTowerSelectionButton && isInDragProcess)
        {
            // Keep the drag process executing
            DragProcess();
        }
    }

    /// <summary>
    /// Starts a new drag process with
    /// <br> - Blocking the selection menu scrolling</br>
    /// <br> - Spawning the selected drag object</br>
    /// <br> - Blocking all activities the draged object can perform</br>
    /// </summary>
    private void StartDragProcess()
    {
        // Set process bool and block selection menu scrolling
        isInDragProcess = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        // Spawn the draged object and store its Transform
        spawnedObjectToDragTransform = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity).GetComponent<Transform>();
        
        // Block all activities of the draged object to prevent execution during the drag process
        spawnedObjectToDragTransform.GetComponent<IActivityBlock>().blockAllActivities = true;
    }

    /// <summary>
    /// Ends a drag process successfully with
    /// <br> - Placing the draged object in the current selected cell</br>
    /// <br> - Reseting all variables that are used during the process</br>
    /// <br> - Activateing the placed objects functionality</br>
    /// <br> - Updateing the remaining currency</br>
    /// </summary>
    private void PlaceTowerAndEndDragProcess()
    {
        // Update drag process information
        isInDragProcess = false;
        
        // Allow scrolling in selection menu
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        // Activate the construction timer and after its construction time the functionality of the now placed object
        spawnedObjectToDragTransform.GetComponent<IActivityBlock>().StartConstructionTimer();

        ObjectCollections.Towers.Add(spawnedObjectToDragTransform.gameObject);

        // Reset currently draged object
        spawnedObjectToDragTransform = null;
       
        // Update displayed currency
        txt_currency.text = $"{GetCurrentAvailabeCurrency() - currencyCost}";
    }

    /// <summary>
    /// Ends a drag pocess not successfully with
    /// <br> - Destroying the draged object</br>
    /// <br> - Reseting all variables that are used during the process</br>
    /// </summary>
    private void CancleDragProcess()
    {
        // If something is currently draged
        if (spawnedObjectToDragTransform != null)
        {
            // Destroy the draged object without placeing it
            Destroy(spawnedObjectToDragTransform.gameObject);
        }

        // Allow scrolling in selection menu again and end drag process
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        isInDragProcess = false;
    }

    /// <summary>
    /// Is called once per frame during a activ drag process
    /// <br>Handles the drag process by managing the displayed currency color depending of the cost of the draged object and the amound of currency the player has</br>
    /// <br>Handels also the detection if the cell the draged object is currently inside</br>
    /// </summary>
    private void DragProcess()
    {
        // Paint curreny text red if the dragt object costs more than the player has
        if(CanPayForBuilding() == false)
        {
            txt_currency.color = Color.red;
        }

        // Checks if the cursor hovers something
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Get the nearest grid cell to the mouse position
            Vector3 nearestCell = Vector3.zero;
            float nearestDist = int.MaxValue;
            for (int i = 0; i < tgs.cells.Count; i++)
            {
                // Get distance between grid cell and cursor position and set new nearest cell if it is nearer to the cursor than the current nearest found
                float dist = Vector3.Distance(hit.point, tgs.CellGetPosition(i));
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestCell = tgs.CellGetPosition(i);
                }
            }

            // Set the position of the draged object to the center of the grid cell nearest to the cursor
            spawnedObjectToDragTransform.position = nearestCell;
        }
    }

    /// <summary>
    /// Checks if amount of currency the player has is enough to bay the currently draged object
    /// </summary>
    /// <returns>true if the player has at least enough currency to buy the currently draged object
    ///           <br>false otherwise</br></returns>
    private bool CanPayForBuilding()
    {
        // Return if the player has enough currency to pay for the currently draged object
        return GetCurrentAvailabeCurrency() >= currencyCost;
    }

    /// <summary>
    /// Get the current amount of currency available for the player 
    /// </summary>
    /// <returns>Current amount of currency available</returns>
    private int GetCurrentAvailabeCurrency()
    {
        // Get the in UI displayed currency text
        string availableCurrencyString = txt_currency.text;
        
        // Delete all characters to make the text easier to read for the player to create a formate convertable to int
        availableCurrencyString = availableCurrencyString.Replace(".", "").Replace("+", "").Replace(" ", "");
        
        // Parse the currency string to an integer value
        return int.Parse(availableCurrencyString);
    }
}
