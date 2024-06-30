using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour
{
    // -------------------------------- EDITABLE
    [SerializeField] protected ShipData shipData;

    // -------------------------------- PARAMS
    protected SpriteRenderer myRenderer;
    protected Rigidbody2D myBody;
    protected Thrusters myThruster;
    protected ShipStateMachine myStateMachine;

    protected void OnValidate()
    {
        myRenderer = gameObject.GetComponent<SpriteRenderer>();
        updateColliderSize();
    }

    // -------------------------------- LOGIC

    protected void Start()
    {
        // Get components
        myRenderer = gameObject.GetComponent<SpriteRenderer>();
        myBody = myBody = gameObject.GetComponent<Rigidbody2D>();
        myThruster = gameObject.GetComponent<Thrusters>();
        myStateMachine = gameObject.GetComponent<ShipStateMachine>();

        if (shipData == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): shipData is null.");
            return;
        }

        if (myThruster == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no Thruster script found in gameObject.");
            return;
        }

        if (myStateMachine == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no ShipStateMachine script found in gameObject.");
            return;
        }

        if (myBody == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no rigidBody found in gameObject.");
            return;
        }

        if (myRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no sprite renderer found in gameObject.");
            return;
        }

        // Start method itself
        myRenderer.sprite = shipData.sprite;
        updateColliderSize();
    }

    protected void FixedUpdate()
    {
        Vector2 collision_vector = getCollisionVector(1 << LayerMask.NameToLayer("Obstacle"));
        Vector2 state_vector = getStateVector();
        
        Vector2 final_vector = (collision_vector == Vector2.zero) ? state_vector : collision_vector;

        //Debug.DrawRay(transform.position, state_vector, Color.green);

        myThruster.target_velocity = final_vector;
        myThruster.target_orientation = final_vector;
        

        /*
        Vector2 target_velocity = transform.up * shipData.patrolSpeed;

        // TODO : incremental random (each iteration, add random to the prev random and clamp it the desired range, to keep continuity)
        float maxNoiseMagnitude = shipData.patrolSpeed * 0.1f;
        float noiseX = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);
        float noiseY = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);
        
        target_velocity += new Vector2(noiseX, noiseY);

        Vector2? collision_vector = getCollisionVector();

        myThruster.target_velocity = (collision_vector == Vector2.zero) ? target_velocity : collision_vector;
        myThruster.target_orientation = (collision_vector == Vector2.zero) ? target_velocity : collision_vector;
        */
    }

    // -------------------------------- METHODS

    protected Vector2 getCollisionVector(int layermask)
    {
        float raycastRadius = Mathf.Max(myRenderer.bounds.size.x, myRenderer.bounds.size.y) / 2;
        float break_distance = getBreakDistance();

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, raycastRadius, myBody.velocity.normalized, break_distance, layermask);

        //Debug.DrawRay(transform.position, myBody.velocity.normalized * break_distance, Color.red);

        if (hit.collider != null)
        {
            Vector2 collisionVector = hit.centroid - hit.point;
            //Debug.DrawRay(hit.point, collisionVector.normalized * break_distance, Color.yellow);
            return collisionVector.normalized * break_distance;
        }

        return Vector2.zero;
    }

    protected Vector2 getAreaCollisionVector(Circle area)
    {
        float break_distance = getBreakDistance();
        Vector2 stop_position = transform.position;
        stop_position += myBody.velocity.normalized * break_distance;

        Debug.DrawLine(transform.position, stop_position, Color.red);

        if(Vector2.Distance(area.center, stop_position) > area.radius)
        {
            Vector2 stop_vector = (stop_position - area.center).normalized * area.radius;
            stop_position = stop_vector + area.center;
            Debug.DrawRay(stop_position, -stop_vector, Color.yellow);
            return -stop_vector;
        }

        return Vector2.zero;
    }

    protected Vector2 getStateVector()
    {
        switch (myStateMachine.state)
        {
            case ShipStateMachine.IDLING:
                return -myBody.velocity;

            case ShipStateMachine.PATROLING:
                Vector2 collision_vector = getAreaCollisionVector((Circle)myStateMachine.targetArea);

                return (collision_vector != Vector2.zero) ? collision_vector : transform.up * shipData.patrolSpeed;

            case ShipStateMachine.MOVING_TO_TARGET_AREA:
                float break_distance = getBreakDistance();
                Vector2 break_point = (Vector2)transform.position + myBody.velocity.normalized * break_distance;
                int zone_intersect_count = ((Circle)myStateMachine.targetArea).GetIntersectCount(transform.position, break_point);

                Debug.DrawLine(transform.position, break_point, Color.black);

                switch(zone_intersect_count)
                {
                    case 0: // Outside the target area
                        Vector2 infinite_break_point = (Vector2)transform.position + myBody.velocity.normalized * 1000000;
                        int infinite_intersect_count = ((Circle)myStateMachine.targetArea).GetIntersectCount(transform.position, infinite_break_point);
                        if(infinite_intersect_count == 0) // We head toward the wrong direction
                        {
                            return (((Circle)myStateMachine.targetArea).center - (Vector2)transform.position).normalized;
                        }
                        else // We're too slow
                        {
                            return myBody.velocity * 10;
                        }
                    case 1: // Inside the target area
                        return myBody.velocity;
                    case 2: // going past the target area
                        return -myBody.velocity;
                    default: // should never happen unless "GetIntersectCount" is f*cked up
                        return -myBody.velocity;
                }
            default:
                return -myBody.velocity;
        }
    }

    // -------------------------------- UTILS

    protected float getBreakDistance()
    {
        // break offset = time to turn in the right direction + time to compensate angular velocity (hack)
        float break_offset = (shipData.rotation_duration) + (Mathf.Abs(myBody.angularVelocity) / (shipData.rotation_thruster_force + 1e-6f));
        //break_offset = Mathf.Max(break_offset, 1f); // Avoid CHAOS when "rotation_thruster_force" too big

        // break distance = distance passed while turning + distance passed while breaking
        float simulated_velocity = myBody.velocity.magnitude + (shipData.main_thruster_force + shipData.rcs_thruster_force) * Time.fixedDeltaTime;
        float break_distance = (break_offset * myBody.velocity.magnitude) + ((simulated_velocity * simulated_velocity) / (2 * (shipData.main_thruster_force + shipData.rcs_thruster_force + 1e-6f)));
        return break_distance * 1.1f; // safety margin
    }

    protected void updateColliderSize()
    {
        CapsuleCollider2D myCollider = gameObject.GetComponent<CapsuleCollider2D>();

        if(myCollider == null || myRenderer == null)
        {
            return;
        }

        float y = myRenderer.bounds.size.x;
        float x = myRenderer.bounds.size.y;

        if (x > y)
        {
            myCollider.direction = CapsuleDirection2D.Horizontal;
        }
        else
        {
            myCollider.direction = CapsuleDirection2D.Vertical;
        }

        x /= transform.localScale.x;
        y /= transform.localScale.y;

        myCollider.size = new Vector2(x, y);
    }
}
