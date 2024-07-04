using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class PatrolMovementSelector : MonoBehaviour 
{ 

    // -------------------------------- PREFAB
    [SerializeField] protected GameObject zoneDisplayPrefab;

    // -------------------------------- PARAMS
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
        inputManager.inputController.PatrolMovementSelection.Enable();
        inputManager.inputController.PatrolMovementSelection.placePoint.performed += OnPlacePoint;
        inputManager.inputController.PatrolMovementSelection.cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        inputManager.inputController.PatrolMovementSelection.placePoint.performed -= OnPlacePoint;
        inputManager.inputController.PatrolMovementSelection.cancel.performed -= OnCancel;
        inputManager.inputController.PatrolMovementSelection.Disable();
        zoneDisplay.SetActive(false);
    }

    private void Update()
    {
        if (zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.PatrolMovementSelection.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition) * 2;
            zoneDisplay.transform.localScale = new Vector2(radius, radius);
        }
    }

    private void OnPlacePoint(InputAction.CallbackContext context)
    {

        // First input -> Set the center of the zone
        if(!zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.PatrolMovementSelection.mousePosition.ReadValue<Vector2>());
            zoneDisplay.transform.position = new Vector3(mousePosition.x, mousePosition.y, zoneDisplay.transform.position.z);
            zoneDisplay.SetActive(true);
        }

        // Second input -> Set the radius of the zone
        else
        {
            // Send the selected movement to the selected units
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.PatrolMovementSelection.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition);
            zoneDisplay.transform.localScale = new Vector2(radius * 2, radius * 2);

            foreach (ShipStateMachine stateMachine in inputManager.selected_ships)
            {
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
