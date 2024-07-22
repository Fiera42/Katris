using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class UnitSelector : MonoBehaviour
{
    // -------------------------------- EDITABLE
    [SerializeField] protected GameObject selectionBoxPrefab;
    public bool selectAllyInPriority;

    // -------------------------------- REFERENCES
    private InputManager inputManager;
    private RectTransform selectionBox;

    // -------------------------------- VARIABLE
    private Vector2 selectionStartPosition;
    private float selectionStartTime;
    private Vector2 multitapStartPosition;
    private float multiTapStartTime;
    private bool isPointerOverUI;
    private IEnumerator WaitBeforeShowingActionWheel_coroutine;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no input manager found in scene.");
            enabled = false;
            return;
        }

        if (selectionBoxPrefab == null)
        {
            Debug.LogError($"{GetType().Name}({name}): selectionBoxPrefab is null.");
            enabled = false;
            return;
        }

        selectionBox = Instantiate(selectionBoxPrefab, transform).GetComponent<RectTransform>();

        if (selectionBox == null)
        {
            Debug.LogError($"{GetType().Name}({name}): selectionBox is not a rectTransform.");
            enabled = false;
            return;
        }

        selectionBox.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        selectionBox.gameObject.SetActive(false);
        inputManager.inputController.UnitSelection.Enable();
        inputManager.inputController.UnitSelection.select.started += OnStartSelection;
    }

    private void OnDisable()
    {
        inputManager.inputController.UnitSelection.select.started -= OnStartSelection;
        inputManager.inputController.UnitSelection.select.canceled -= OnStopSelection;
        inputManager.inputController.General.mousePosition.performed -= UpdateSelectionBox;
        inputManager.inputController.UnitSelection.Disable();
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void OnStartSelection(InputAction.CallbackContext context)
    {
        if(isPointerOverUI)
        {
            return;
        }

        selectionStartTime = Time.time;
        selectionStartPosition = inputManager.inputController.General.mousePosition.ReadValue<Vector2>();
        inputManager.inputController.UnitSelection.select.canceled += OnStopSelection;
        inputManager.inputController.General.mousePosition.performed += UpdateSelectionBox;
        UpdateSelectionBox(new InputAction.CallbackContext());
        selectionBox.gameObject.SetActive(true);
        inputManager.HideActionWheel();
    }

    private void OnStopSelection(InputAction.CallbackContext context)
    {
        inputManager.inputController.UnitSelection.select.canceled -= OnStopSelection;
        inputManager.inputController.General.mousePosition.performed -= UpdateSelectionBox;
        selectionBox.gameObject.SetActive(false);

        Stop_ShowActionWheelAfterMultiTapDelay_Coroutine();

        // If the mouse travelled too much, do not bother with multitap
        Vector2 mousePosition = inputManager.inputController.General.mousePosition.ReadValue<Vector2>();

        if (Vector2.Distance(mousePosition, selectionStartPosition) > 10)
        {
            inputManager.UpdateActionWheel();
            WaitBeforeShowingActionWheel_coroutine = null;
        }
        else
        {
            if(Vector2.Distance(mousePosition, multitapStartPosition) <= 10 && (Time.time - multiTapStartTime) < InputSystem.settings.multiTapDelayTime)
            {
                multiTapStartTime = 0;
                multitapStartPosition = new Vector2(-10000, -10000);
                OnSelectAllShipOfType(context);
            }
            else
            {
                multitapStartPosition = selectionStartPosition;
                multiTapStartTime = selectionStartTime;
                WaitBeforeShowingActionWheel_coroutine = WaitBeforeShowingActionWheel();
                StartCoroutine(WaitBeforeShowingActionWheel_coroutine);
            }
        }
    }

    private void Stop_ShowActionWheelAfterMultiTapDelay_Coroutine()
    {
        if (WaitBeforeShowingActionWheel_coroutine != null)
        {
            StopCoroutine(WaitBeforeShowingActionWheel_coroutine);
            WaitBeforeShowingActionWheel_coroutine = null;
        }
    }

    public IEnumerator WaitBeforeShowingActionWheel()
    {
        yield return new WaitForSeconds(InputSystem.settings.multiTapDelayTime - (Time.time - selectionStartTime));
        inputManager.UpdateActionWheel();
        WaitBeforeShowingActionWheel_coroutine = null;
    }

    private void OnSelectAllShipOfType(InputAction.CallbackContext context)
    {
        if(inputManager.selected_ships.Count > 0)
        {
            Stop_ShowActionWheelAfterMultiTapDelay_Coroutine();

            Debug.Log("Selecting all ships of type " + inputManager.selected_ships[0].shipData.name);
            inputManager.UpdateActionWheel();
        }
    }

    private void UpdateSelectionBox(InputAction.CallbackContext context)
    {
        // Update the selection box hud
        Vector2 mousePosition = inputManager.inputController.General.mousePosition.ReadValue<Vector2>();
        Vector2 size = mousePosition - selectionStartPosition;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        selectionBox.anchoredPosition = (selectionStartPosition + mousePosition) / 2;

        // Project the hud rectangle into a worldspace rectangle
        Vector3 projectedStartPosition = Camera.main.ScreenToWorldPoint(selectionStartPosition);
        Vector3 projectedMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        size = projectedMousePosition - projectedStartPosition;
        Vector2 raycastBoxSize = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        Vector2 raycastBoxCenter = (projectedStartPosition + projectedMousePosition) / 2;

        CastSelectionBox(raycastBoxCenter, raycastBoxSize);
    }

    protected void CastSelectionBox(Vector2 raycastBoxCenter, Vector2 raycastBoxSize)
    {
        // OverlapBox to find the units

        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(raycastBoxCenter, raycastBoxSize, 0, 1 << LayerMask.NameToLayer("Ship"));

        List<ShipStateMachine> allyShips = new();
        List<ShipStateMachine> otherShips = new();

        foreach (Collider2D collider in overlappingColliders)
        {
            ShipStateMachine ship = collider.gameObject.GetComponent<ShipStateMachine>();
            if (ship != null)
            {
                if (ship.CompareTag("Player 1"))
                {
                    allyShips.Add(ship);
                }
                else
                {
                    otherShips.Add(ship);
                }
            }
        }

        List<ShipStateMachine> selectedShips;
        if(selectAllyInPriority)
        {
            selectedShips = (allyShips.Count != 0) ? allyShips : otherShips;
        }
        else
        {
            selectedShips = new();
            selectedShips.AddRange(allyShips);
            selectedShips.AddRange(otherShips);
        }

        // Update the selected units

        // ------------------------------------------------------------------------- Add new unit to the selection
        if (inputManager.inputController.UnitSelection.add.ReadValue<float>() == 1)
        {
            // ------------------------- Select any
            if(!selectAllyInPriority)
            {
                foreach (ShipStateMachine ship in selectedShips)
                {
                    if (inputManager.selected_ships.Contains(ship)) continue;
                    ship.SelectionCircle.gameObject.SetActive(true);
                    inputManager.selected_ships.Add(ship);
                }
                return;
            }

            // ------------------------- Select ally in priority

            // When old selection does not have ally ships but the new selection does, clear the old selection
            if (inputManager.selected_ships.Count != 0 && !inputManager.selected_ships[0].CompareTag("Player 1") && allyShips.Count != 0)
            {
                foreach (ShipStateMachine ship in inputManager.selected_ships)
                {
                    ship.SelectionCircle.gameObject.SetActive(false);
                }
                inputManager.selected_ships.Clear();
            }

            // Old selection contain ally but the new one does not
            if (inputManager.selected_ships.Count != 0 && inputManager.selected_ships[0].CompareTag("Player 1") && allyShips.Count == 0)
            {
                return;
            }

            foreach (ShipStateMachine ship in selectedShips)
            {
                if (inputManager.selected_ships.Contains(ship)) continue;
                ship.SelectionCircle.gameObject.SetActive(true);
                inputManager.selected_ships.Add(ship);
            }
        }

        // ------------------------------------------------------------------------- Remove unit from the selection
        else if (inputManager.inputController.UnitSelection.remove.ReadValue<float>() == 1)
        {
            foreach (ShipStateMachine ship in allyShips)
            {
                ship.SelectionCircle.gameObject.SetActive(false);
                inputManager.selected_ships.Remove(ship);
            }
            foreach (ShipStateMachine ship in otherShips)
            {
                ship.SelectionCircle.gameObject.SetActive(false);
                inputManager.selected_ships.Remove(ship);
            }
        }

        // ------------------------------------------------------------------------- Replace the old selection
        else
        {
            foreach (ShipStateMachine ship in inputManager.selected_ships)
            {
                ship.SelectionCircle.gameObject.SetActive(false);
            }

            inputManager.selected_ships = selectedShips;

            foreach (ShipStateMachine ship in selectedShips)
            {
                ship.SelectionCircle.gameObject.SetActive(true);
            }
        }
    }
}
