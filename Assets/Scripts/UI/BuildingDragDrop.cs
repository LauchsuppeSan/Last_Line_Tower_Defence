using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TGS;
using UnityEngine.EventSystems;

public class BuildingDragDrop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isOver { get; private set; }
    public GameObject prefabToSpawn;
    public int currencyCost = 500;
    public Text txt_currency;
    public LayerMask mask;

    private bool isInDragProcess = false;
    private TerrainGridSystem tgs;
    private Transform spawnedObjectTDragTransform;
    private Camera mainCamera;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOver = false;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        tgs = FindObjectOfType<TerrainGridSystem>();
    }

    private void Update()
    {
        Debug.Log("Was soll der scheiss");

        if(!isInDragProcess && txt_currency.color != Color.white)
        {
            txt_currency.color = Color.white;
        }

        bool holdLeftMouseBtn = Input.GetKey(KeyCode.Mouse0);
        
        if (isOver && holdLeftMouseBtn && !isInDragProcess) 
        {
            StartDragProcess();
        }
        else if(!isOver && !holdLeftMouseBtn && isInDragProcess) 
        {
            if(CanPayForBuilding())
            {
                PlaceTowerAndEndDragProcess();
            }
            else
            {
                CancleDragProcess();
            }
        }
        else if((isOver && !holdLeftMouseBtn && isInDragProcess) || !isOver && Input.GetKey(KeyCode.Mouse1))
        {
            CancleDragProcess();
        }
        else if(!isOver && isInDragProcess)
        {
            DragProcess();
        }
    }

    private void StartDragProcess()
    {
        isInDragProcess = true;
        spawnedObjectTDragTransform = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity).GetComponent<Transform>();
    }

    private void PlaceTowerAndEndDragProcess()
    {
        isInDragProcess = false;
        spawnedObjectTDragTransform = null;
        txt_currency.text = $"{GetCurrentAvailabeCurrency() - currencyCost}";
    }

    private void CancleDragProcess()
    {
        if (spawnedObjectTDragTransform != null)
        {
            Destroy(spawnedObjectTDragTransform.gameObject);
        }

        isInDragProcess = false;
    }

    private void DragProcess()
    {
        if(CanPayForBuilding() == false)
        {
            txt_currency.color = Color.red;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 nearestCell = Vector3.zero;
            float nearestDist = int.MaxValue;

            for (int i = 0; i < tgs.cells.Count; i++)
            {
                float dist = Vector3.Distance(hit.point, tgs.CellGetPosition(i));
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestCell = tgs.CellGetPosition(i);
                }
            }

            spawnedObjectTDragTransform.position = nearestCell;
        }

        //Vector3 mousePosition = Input.mousePosition;
        //mousePosition.z = 10f;
        //mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //Debug.DrawRay(Camera.main.transform.position, mousePosition - Camera.main.transform.position, Color.red);
        //
        //Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        //RaycastHit hit;
        //
        //if (Physics.Raycast(ray, out hit, int.MaxValue))
        //{
        //    // Plaziert an stellen bei dennen ich keine ahnung habe wie der darauf kommt
        //    Cell selectedCell = tgs.CellGetAtPosition(hit.point);
        //    Vector3 cellCenter = tgs.CellGetPosition(selectedCell);
        //    spawnedObjectTDragTransform.position = hit.point;
        //}
    }

    private bool CanPayForBuilding()
    {
        if (GetCurrentAvailabeCurrency() >= currencyCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int GetCurrentAvailabeCurrency()
    {
        string availableCurrencyString = txt_currency.text;
        availableCurrencyString = availableCurrencyString.Replace(".", "").Replace("+", "").Replace(" ", "");
        return int.Parse(availableCurrencyString);
    }
}
