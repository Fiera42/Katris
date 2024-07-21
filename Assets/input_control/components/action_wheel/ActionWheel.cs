using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class ActionWheel : MonoBehaviour
{

    // -------------------------------- EDITABLE
    [SerializeField] protected GameObject actionWheelPrefab;

    // -------------------------------- REFERENCES
    private InputManager inputManager;
    public RectTransform ActionWheelObject { get; private set; }
    public WheelButtons wheelButtons { get; private set; }

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no input manager found in scene.");
            enabled = false;
            return;
        }

        if (actionWheelPrefab == null)
        {
            Debug.LogError($"{GetType().Name}({name}): actionWheelPrefab is null.");
            enabled = false;
            return;
        }

        ActionWheelObject = Instantiate(actionWheelPrefab, transform).GetComponent<RectTransform>();

        if (ActionWheelObject == null)
        {
            Debug.LogError($"{GetType().Name}({name}): actionWheel is not a rectTransform.");
            enabled = false;
            return;
        }

        ActionWheelObject.gameObject.SetActive(false);
        wheelButtons = ActionWheelObject.gameObject.GetComponent<WheelButtons>();

        if (wheelButtons == null)
        {
            Debug.LogError($"{GetType().Name}({name}): actionWheel does not contain buttons.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        ActionWheelObject.gameObject.SetActive(true);
        inputManager.inputController.ActionWheelShortcuts.Enable();
        inputManager.inputController.ActionWheelShortcuts.moveInteract.performed += OnMovePressed;
        inputManager.inputController.ActionWheelShortcuts.patrol.performed += OnPatrolPressed;
        inputManager.inputController.ActionWheelShortcuts.scout.performed += OnScoutPressed;
        inputManager.inputController.ActionWheelShortcuts.attack.performed += OnAttackPressed;
        wheelButtons.moveButton.onClick.AddListener(() => { OnMovePressed(new InputAction.CallbackContext()); });
        wheelButtons.patrolButton.onClick.AddListener(() => { OnPatrolPressed(new InputAction.CallbackContext()); });
        wheelButtons.scoutButton.onClick.AddListener(() => { OnScoutPressed(new InputAction.CallbackContext()); });
        wheelButtons.attackButton.onClick.AddListener(() => { OnAttackPressed(new InputAction.CallbackContext()); });
    }

    private void OnDisable()
    {
        inputManager.inputController.ActionWheelShortcuts.moveInteract.performed -= OnMovePressed;
        inputManager.inputController.ActionWheelShortcuts.patrol.performed -= OnPatrolPressed;
        inputManager.inputController.ActionWheelShortcuts.scout.performed -= OnScoutPressed;
        inputManager.inputController.ActionWheelShortcuts.attack.performed -= OnAttackPressed;
        wheelButtons.moveButton.onClick.RemoveAllListeners();
        wheelButtons.patrolButton.onClick.RemoveAllListeners();
        wheelButtons.scoutButton.onClick.RemoveAllListeners();
        wheelButtons.attackButton.onClick.RemoveAllListeners();
        inputManager.inputController.ActionWheelShortcuts.Disable();
        if (ActionWheelObject != null) ActionWheelObject.gameObject.SetActive(false);
    }

    public void OnMovePressed(InputAction.CallbackContext context)
    {
        inputManager.MoveOrder();
    }

    public void OnPatrolPressed(InputAction.CallbackContext context)
    {
        inputManager.PatrolOrder();
    }

    public void OnScoutPressed(InputAction.CallbackContext context)
    {
        inputManager.ScoutOrder();
    }

    public void OnAttackPressed(InputAction.CallbackContext context)
    {
        inputManager.AttackOrder();
    }
}
