using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    /*
     * Handle the inputController of the player
     * Allow every components to use the same controller to allow meaningfull interactions
     * For example : Select units -> if(only ally unit selected) -> enable unit control inputs -> "patrol" button -> enable movementZone inputs
     * -> right click -> disable movementZone inputs
     */
    public InputController inputController
    {
        // note : to get all the inputs of the controller, use :
        // foreach(InputAction inputAction in inputController.asset)
        get;
        private set;
    }

    // -------------------------------- PUBLIC
    [HideInInspector] public List<ShipStateMachine> selected_ships = new();
    public bool updateActionWheelPosition;
    public bool freezeActionWheelOnMouseOver;
    public bool keepActionWheelOnScreen;

    // -------------------------------- PARAMS
    public MovementSelector movementSelector { get; private set; }
    public UnitSelector unitSelector { get; private set; }
    public ActionWheel actionWheelScript { get; private set; }
    public CameraMovement cameraMovement { get; private set; }

    private void Awake()
    {
        inputController = new InputController();
        movementSelector = GetComponent<MovementSelector>();
        unitSelector = GetComponent<UnitSelector>();
        actionWheelScript = GetComponent<ActionWheel>();
        cameraMovement = GetComponent<CameraMovement>();

        if (movementSelector == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no movementSelector found in gameObject.");
            return;
        }

        if (unitSelector == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no unitSelector found in gameObject.");
            return;
        }

        if (actionWheelScript == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no actionWheel found in gameObject.");
            return;
        }

        if (cameraMovement == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no cameraMovement found in gameObject.");
            return;
        }
    }

    private void OnEnable()
    {
        inputController.General.Enable();
    }

    private void OnDisable()
    {
        ResetManager();

        // Garbage collection
        inputController.Disable();

        foreach (ShipStateMachine ship in selected_ships)
        {
            if (ship != null) ship.SelectionCircle.gameObject.SetActive(false);
        }
        selected_ships.Clear();
    }

    public void Update()
    {
        if(updateActionWheelPosition && actionWheelScript.enabled)
        {
            if(freezeActionWheelOnMouseOver)
            {
                if(!actionWheelScript.wheelButtons.isHover)
                {
                    UpdateActionWheel();
                }
            }
            else
            {
                UpdateActionWheel();
            }
        }
    }

    public void ResetManager()
    {
        // Garbage collection
        inputController.Disable();
        inputController.General.Enable();

        foreach (ShipStateMachine ship in selected_ships.ToArray())
        {
            if(ship == null)
            {
                selected_ships.Remove(ship);
            }
        }

        // Reset all components
        movementSelector.enabled = false;
        unitSelector.enabled = false;
        actionWheelScript.enabled = false;
        cameraMovement.enabled = false;

        // Activate default components
        unitSelector.enabled = true;
        cameraMovement.enabled = true;

        UpdateActionWheel();
    }

    public void UpdateActionWheel()
    {
        if (selected_ships.Count == 0)
        {
            HideActionWheel();
        }

        else
        {
            ShowActionWheel();
        }
    }

    public void ShowActionWheel()
    {
        if (selected_ships.Count == 0) {
            return;
        }

        Vector2 center = Vector2.zero;
        float biggestY = float.MinValue;
        int allyShipCount = 0;
        float colliderOffset = 0;
        foreach (ShipStateMachine ship in selected_ships)
        {
            if(ship.CompareTag("Player 1"))
            {
                if(ship.transform.position.y > biggestY)
                {
                    colliderOffset = Mathf.Max(ship.shipData.colliderSize.x, ship.shipData.colliderSize.y);
                    biggestY = ship.transform.position.y;
                }

                center += (Vector2)ship.transform.position;
                allyShipCount++;
            }
        }

        if (allyShipCount < 1)
        {
            return;
        }

        center /= allyShipCount;
        center.y = biggestY;
        center.y += colliderOffset;
        center = Camera.main.WorldToScreenPoint(center);
        center.y += actionWheelScript.ActionWheelObject.sizeDelta.y / 2;

        if(keepActionWheelOnScreen)
        {
            float radius = actionWheelScript.ActionWheelObject.sizeDelta.x / 2;

            if (center.x + radius > Screen.width) center.x = Screen.width - radius;
            if (center.x - radius < 0) center.x = radius;
            if (center.y + radius > Screen.height) center.y = Screen.height - radius;
            if (center.y - radius < 0) center.y = radius;
        }

        actionWheelScript.ActionWheelObject.position = center;
        actionWheelScript.enabled = true;
    }

    public void HideActionWheel()
    {
        actionWheelScript.enabled = false;
    }

    public void MoveOrder()
    {
        ResetManager();
        unitSelector.enabled = false;
        HideActionWheel();
        movementSelector.isPatrolOrder = false;
        movementSelector.enabled = true;
    }

    public void PatrolOrder()
    {
        ResetManager();
        unitSelector.enabled = false;
        HideActionWheel();
        movementSelector.isPatrolOrder = true;
        movementSelector.enabled = true;
    }

    public void ScoutOrder()
    {
        Debug.Log("scout");
    }

    public void AttackOrder()
    {
        Debug.Log("attack");
    }
}
