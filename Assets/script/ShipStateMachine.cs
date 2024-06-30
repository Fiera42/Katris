using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShipStateMachine : MonoBehaviour
{

    // -------------------------------- VALUES

    [HideInInspector] public Circle? targetArea = null;
    [HideInInspector] public bool mustPatrolArea;

    public GameObject areaPrefab; // TEMP

    // -------------------------------- STATES

    [HideInInspector] public int state { get; private set; } = IDLING;

    public const int IDLING = 0;
    public const int PATROLING = 1;
    public const int MOVING_TO_TARGET_AREA = 2;
    

    void Start()
    {
        targetArea = new Circle(new Vector3(0,0,0), 10);
        mustPatrolArea = true;
    }

    // -------------------------------- STATE MACHINE

    private void FixedUpdate()
    {
        // Declaration of stuff so the ide doesn't explode
        bool isInArea;

        switch (state)
        {
            case IDLING:
                if (targetArea == null) { state = IDLING; break; }
                isInArea = ((Circle)targetArea).radius > Vector2.Distance(transform.position, ((Circle)targetArea).center);
                if (!isInArea) { state = MOVING_TO_TARGET_AREA; break; }
                if (isInArea && !mustPatrolArea) { state = IDLING; targetArea = null;  break; }
                if (isInArea && mustPatrolArea) { state = PATROLING; break; }
                break;

            case PATROLING:
                if (targetArea == null) { state = IDLING; break; }
                isInArea = ((Circle)targetArea).radius > Vector2.Distance(transform.position, ((Circle)targetArea).center);
                if (!isInArea) { state = MOVING_TO_TARGET_AREA; break; }
                if (isInArea && !mustPatrolArea) { state = IDLING; targetArea = null; break; }
                if (isInArea && mustPatrolArea) { state = PATROLING; break; }
                break;

            case MOVING_TO_TARGET_AREA:
                if (targetArea == null) { state = IDLING; break; }
                isInArea = ((Circle)targetArea).radius > Vector2.Distance(transform.position, ((Circle)targetArea).center);
                if (!isInArea) { state = MOVING_TO_TARGET_AREA; break; }
                if (isInArea && !mustPatrolArea) { state = IDLING; targetArea = null; break; }
                if (isInArea && mustPatrolArea) { state = PATROLING; break; }
                break;

            default:
                break;
        }
    }

    // ------------------------- Gizmos

    public void OnDrawGizmos()
    {
        if (targetArea == null) return;

        Color color = Color.cyan;
        color.a = 0.1f;
        Gizmos.color = color;
        Gizmos.DrawSphere(new Vector3(((Circle)targetArea).center.x, ((Circle)targetArea).center.y, transform.position.y), ((Circle)targetArea).radius);
    }
}
