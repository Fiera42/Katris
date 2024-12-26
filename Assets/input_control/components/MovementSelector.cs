using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class MovementSelector : MonoBehaviour 
{ 

    // -------------------------------- EDITABLE
    [SerializeField] protected GameObject zoneDisplayPrefab;
    [SerializeField] protected float nonPatrolOrderMinCircle;
    public bool isPatrolOrder;

    // -------------------------------- REFERENCES
    private InputManager inputManager;
    private GameObject zoneDisplay;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no input manager found in scene.");
            enabled = false;
            return;
        }

        if (zoneDisplayPrefab == null)
        {
            Debug.LogError($"{GetType().Name}({name}): zoneDisplayPrefab is null.");
            enabled = false;
            return;
        }

        zoneDisplay = Instantiate(zoneDisplayPrefab, transform);
        zoneDisplay.SetActive(false);
    }

    private void OnEnable()
    {
        zoneDisplay.SetActive(false);
        inputManager.inputController.MovementSelection.Enable();
        inputManager.inputController.MovementSelection.placePoint.performed += OnPlacePoint;
        inputManager.inputController.MovementSelection.cancel.performed += OnCancel;
        
    }

    private void OnDisable()
    {
        inputManager.inputController.MovementSelection.placePoint.performed -= OnPlacePoint;
        inputManager.inputController.MovementSelection.cancel.performed -= OnCancel;
        inputManager.inputController.General.mousePosition.performed -= UpdateZoneRadius;
        inputManager.inputController.MovementSelection.Disable();
        if(zoneDisplay != null) zoneDisplay.SetActive(false);
    }

    private void UpdateZoneRadius(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.General.mousePosition.ReadValue<Vector2>());
        float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition) * 2;
        zoneDisplay.transform.localScale = new Vector2(radius, radius);
    }
    public void OnPlacePoint(InputAction.CallbackContext context)
    {

        // No patrol -> No zone
        if(!isPatrolOrder)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.General.mousePosition.ReadValue<Vector2>());
            float radius = nonPatrolOrderMinCircle + 0.5f * inputManager.selected_ships.Count;

            foreach (ShipStateMachine ship in inputManager.selected_ships)
            {
                if (ship.CompareTag("Player 1"))
                {
                    ship.mustPatrolArea = false;
                    ship.targetArea = new Circle(new Vector2(mousePosition.x, mousePosition.y), radius);
                } 
            }

            inputManager.ResetManager();
            return;
        }

        // Patrol -> two click to select a zone

        // First input -> Set the center of the zone
        if(!zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.General.mousePosition.ReadValue<Vector2>());
            zoneDisplay.transform.position = new Vector3(mousePosition.x, mousePosition.y, zoneDisplay.transform.position.z);
            zoneDisplay.transform.localScale = new Vector2(0, 0);
            zoneDisplay.SetActive(true);
            inputManager.inputController.General.mousePosition.performed += UpdateZoneRadius;
        }

        // Second input -> Set the radius of the zone
        else
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.General.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition);
            
            if(radius < 0.5f)
            {
                return;
            }

            zoneDisplay.transform.localScale = new Vector2(radius * 2, radius * 2);

            // Send the selected movement to the selected units
            foreach (ShipStateMachine ship in inputManager.selected_ships)
            {
                if (ship.CompareTag("Player 1"))
                {
                    ship.mustPatrolArea = true;
                    ship.targetArea = new Circle(zoneDisplay.transform.position, radius);
                }
            }

            inputManager.ResetManager();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        inputManager.ResetManager();
    }
}
