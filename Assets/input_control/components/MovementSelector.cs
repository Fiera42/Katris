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

        zoneDisplay = Instantiate(zoneDisplayPrefab, Vector3.zero, Quaternion.identity);
        zoneDisplay.SetActive(false);
    }

    private void OnEnable()
    {
        inputManager.inputController.MovementSelection.Enable();
        inputManager.inputController.MovementSelection.placePoint.performed += OnPlacePoint;
        inputManager.inputController.MovementSelection.cancel.performed += OnCancel;
        zoneDisplay.SetActive(false);
    }

    private void OnDisable()
    {
        inputManager.inputController.MovementSelection.placePoint.performed -= OnPlacePoint;
        inputManager.inputController.MovementSelection.cancel.performed -= OnCancel;
        inputManager.inputController.MovementSelection.Disable();
        if(zoneDisplay != null) zoneDisplay.SetActive(false);
    }

    private void Update()
    {
        if (zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementSelection.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition) * 2;
            zoneDisplay.transform.localScale = new Vector2(radius, radius);
        }
    }

    private void OnPlacePoint(InputAction.CallbackContext context)
    {

        // No patrol -> No zone
        if(!isPatrolOrder)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementSelection.mousePosition.ReadValue<Vector2>());
            float radius = nonPatrolOrderMinCircle + 0.5f * inputManager.selected_ships.Count;

            foreach (ShipStateMachine stateMachine in inputManager.selected_ships)
            {
                stateMachine.mustPatrolArea = false;
                stateMachine.targetArea = new Circle(new Vector2(mousePosition.x, mousePosition.y), radius);
            }

            // Close context
            enabled = false;
            return;
        }

        // Patrol -> two click to select a zone

        // First input -> Set the center of the zone
        if(!zoneDisplay.activeSelf)
        {
            
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementSelection.mousePosition.ReadValue<Vector2>());
            Debug.Log(mousePosition);
            zoneDisplay.transform.position = new Vector3(mousePosition.x, mousePosition.y, zoneDisplay.transform.position.z);
            zoneDisplay.SetActive(true);
        }

        // Second input -> Set the radius of the zone
        else
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementSelection.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition);
            zoneDisplay.transform.localScale = new Vector2(radius * 2, radius * 2);

            // Send the selected movement to the selected units
            foreach (ShipStateMachine stateMachine in inputManager.selected_ships)
            {
                stateMachine.mustPatrolArea = true;
                stateMachine.targetArea = new Circle(zoneDisplay.transform.position, radius);
            }

            // Close context
            enabled = false;
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        enabled = false;
    }
}
