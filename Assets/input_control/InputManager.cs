using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public List<ShipStateMachine> selected_ships = new();


    // -------------------------------- PARAMS
    private MovementSelector movementSelector;

    private void Awake()
    {
        inputController = new InputController();
        movementSelector = GetComponent<MovementSelector>();

        if (movementSelector == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no patrolMovementSelector found in gameObject.");
            return;
        }
    }

    private void OnEnable()
    {
        movementSelector.isPatrolOrder = false;
    }

    private void OnDisable()
    {
        // Reset all components
        movementSelector.enabled = false;

        // Garbage collection
        inputController.Disable();
    }
}
