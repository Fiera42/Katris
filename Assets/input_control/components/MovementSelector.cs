using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class MovementSelector : MonoBehaviour
{
    // -------------------------------- EDITABLE
    [SerializeField] protected GameObject zoneDisplayPrefab;

    // -------------------------------- PARAMS
    private InputManager inputManager;
    private GameObject zoneDisplay;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();

        if (inputManager == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no input manager found in scene.");
            return;
        }

        if (zoneDisplayPrefab == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): zoneDisplayPrefab is null.");
            return;
        }

        zoneDisplay = Instantiate(zoneDisplayPrefab, Vector3.zero, Quaternion.identity);
        zoneDisplay.SetActive(false);
    }

    private void OnEnable()
    {
        inputManager.inputController.MovementZone.Enable();
        inputManager.inputController.MovementZone.placePoint.performed += OnPlacePoint;
        inputManager.inputController.MovementZone.cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        inputManager.inputController.MovementZone.placePoint.performed -= OnPlacePoint;
        inputManager.inputController.MovementZone.cancel.performed -= OnCancel;
        inputManager.inputController.MovementZone.Disable();
        zoneDisplay.SetActive(false);
    }

    private void Update()
    {
        if (zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementZone.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition) * 2;
            zoneDisplay.transform.localScale = new Vector2(radius, radius);
        }
    }

    private void OnPlacePoint(InputAction.CallbackContext context)
    {

        // First input -> Set the center of the zone
        if(!zoneDisplay.activeSelf)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementZone.mousePosition.ReadValue<Vector2>());
            zoneDisplay.transform.position = new Vector3(mousePosition.x, mousePosition.y, zoneDisplay.transform.position.z);
            zoneDisplay.SetActive(true);
        }

        // Second input -> Set the radius of the zone
        else
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputManager.inputController.MovementZone.mousePosition.ReadValue<Vector2>());
            float radius = Vector2.Distance((Vector2)zoneDisplay.transform.position, mousePosition);
            zoneDisplay.transform.localScale = new Vector2(radius * 2, radius * 2);

            foreach (ShipStateMachine stateMachine in inputManager.selected_ships)
            {
                stateMachine.targetArea = new Circle(zoneDisplay.transform.position, radius);
            }

            // Close context
            gameObject.SetActive(false);
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        gameObject.SetActive(false);
    }
}
