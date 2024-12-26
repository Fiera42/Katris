using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private InputManager inputManager;
    private Vector2? origin;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no input manager found in scene.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        inputManager.inputController.CameraMovement.Enable();
        inputManager.inputController.CameraMovement.DragMove.started += OnDragMove;
        inputManager.inputController.CameraMovement.DragMove.canceled += OnDragMove;
        inputManager.inputController.General.mousePosition.performed += OnMoveCamera;
    }

    private void OnDragMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(context.phase == UnityEngine.InputSystem.InputActionPhase.Started)
        {
            Vector2 mousePosition = inputManager.inputController.General.mousePosition.ReadValue<Vector2>();
            origin = Camera.main.ScreenToWorldPoint(mousePosition);
        }
        if(context.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
        {
            origin = null;
        }
    }

    private void OnMoveCamera(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (origin == null) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(
            inputManager.inputController.General.mousePosition.ReadValue<Vector2>()
            );

        // A = Camera - origin
        // CameraPos = A + mouse - origin
        Vector2 originToCam = mousePosition - (Vector2)Camera.main.transform.position;
        Vector2 dif = (Vector2)(origin - originToCam);
        Camera.main.transform.position = new Vector3(dif.x, dif.y, Camera.main.transform.position.z);
    }
}
