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
    public ShipData shipData;

    // -------------------------------- PARAMS
    protected SpriteRenderer myRenderer;
    protected Rigidbody2D myBody;
    protected Thrusters myThruster;
    protected ShipStateMachine myStateMachine;
    protected CapsuleCollider2D myCollider;

    protected void OnValidate()
    {
        myRenderer = gameObject.GetComponent<SpriteRenderer>();
        myCollider = gameObject.GetComponent<CapsuleCollider2D>();

        if (myRenderer == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no sprite renderer found in gameObject.");
            enabled = false;
            return;
        }

        if (myCollider == null)
        {
            Debug.LogError($"{GetType().Name}({name}): no CapsuleCollider2D found in gameObject.");
            enabled = false;
            return;
        }

        myCollider.direction = shipData.colliderDirection;
        myCollider.size = shipData.colliderSize;
    }

    // -------------------------------- LOGIC

    protected void Awake()
    {
        // Get components
        myRenderer = gameObject.GetComponent<SpriteRenderer>();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        myThruster = gameObject.GetComponent<Thrusters>();
        myStateMachine = gameObject.GetComponent<ShipStateMachine>();
        myCollider = gameObject.GetComponent<CapsuleCollider2D>();

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

        if (myCollider == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no CapsuleCollider2D found in gameObject.");
            return;
        }

        // Start method itself
        myRenderer.sprite = shipData.sprite;
        myCollider.direction = shipData.colliderDirection;
        myCollider.size = shipData.colliderSize;

        // -Temp-
        /*
        float maxNoiseMagnitude = 50;
        float noiseX = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);
        float noiseY = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);

        myBody.velocity += new Vector2(noiseX, noiseY);

        float maxRotationNoise = 50;
        myBody.angularVelocity = Random.Range(-maxRotationNoise, maxRotationNoise);
        */
        // -Temp-
    }

    protected void FixedUpdate()
    {
        // ------- Obstacle avoidance
        Vector2 collision_vector = GetCollisionVector(1 << LayerMask.NameToLayer("Obstacle"));

        if (collision_vector != Vector2.zero)
        {
            myThruster.target_velocity = collision_vector;
            myThruster.target_orientation = collision_vector;
            return;
        }

        // ------- Boid simulation
        // TODO

        // ------- Ship control
        switch (myStateMachine.State)
        {
            case ShipStateMachine.IDLING: // --------------------------------------------------------------------------------------

                if (myBody.velocity == Vector2.zero) // True idle
                {
                    myThruster.target_velocity = null;
                    myThruster.target_orientation = null;
                    break;
                }

                if (myBody.velocity.magnitude > shipData.rcs_thruster_force) // Too fast -> slow down
                {
                    myThruster.target_velocity = -myBody.velocity;
                    myThruster.target_orientation = -myBody.velocity;
                }
                else // Else chill
                {
                    myThruster.target_velocity = Vector2.zero;
                    myThruster.target_orientation = (Vector2) transform.up;
                }
                
                break;

            case ShipStateMachine.PATROLING: // --------------------------------------------------------------------------------------
                Vector2 area_collision_vector = GetAreaCollisionVector((Circle)myStateMachine.targetArea);

                if (area_collision_vector != Vector2.zero)
                {
                    myThruster.target_velocity = area_collision_vector;
                    myThruster.target_orientation = area_collision_vector;
                }
                else
                {
                    myThruster.target_velocity = transform.up * (((Circle)myStateMachine.targetArea).radius * 2) / shipData.patrolDuration;
                    myThruster.target_orientation = null;
                }

                break;

            case ShipStateMachine.MOVING_TO_TARGET_AREA: // --------------------------------------------------------------------------------------
                float break_distance = GetBreakDistance();
                Vector2 break_point = (Vector2)transform.position + myBody.velocity.normalized * break_distance;
                int zone_intersect_count = ((Circle)myStateMachine.targetArea).GetIntersectCount(transform.position, break_point);

                // Debug.DrawLine(transform.position, break_point, Color.black);

                switch (zone_intersect_count)
                {
                    case 0: // Outside the target area
                        Vector2 infinite_break_point = (Vector2)transform.position + myBody.velocity.normalized * 1000000;
                        int infinite_intersect_count = ((Circle)myStateMachine.targetArea).GetIntersectCount(transform.position, infinite_break_point);
                        if (infinite_intersect_count == 0) // We head toward the wrong direction
                        {
                            if (myBody.velocity.magnitude > 2 * shipData.rcs_thruster_force) // Too fast to steer
                            {
                                myThruster.target_velocity = -myBody.velocity;
                                myThruster.target_orientation = -myBody.velocity;
                            }
                            else // Steer
                            {
                                Vector2 vector = (((Circle)myStateMachine.targetArea).center - (Vector2)transform.position).normalized;
                                myThruster.target_velocity = vector;
                                myThruster.target_orientation = vector;
                            }
                            
                        }
                        else // We're too slow
                        {
                            {
                                Vector2 middle_direction = (((Circle)myStateMachine.targetArea).center - (Vector2)transform.position).normalized;
                                // 0.75 of current speed + 0.25 of going to the center of the target, x10
                                myThruster.target_velocity = (0.75f * (myBody.velocity) + 0.25f * (middle_direction * myBody.velocity.magnitude)) * 10;
                                myThruster.target_orientation = null;
                            }
                        }
                        break;

                    case 1: // Inside the target area
                        {
                            Vector2 middle_direction = (((Circle)myStateMachine.targetArea).center - (Vector2)transform.position).normalized;
                            // 0.75 of current speed + 0.25 of going to the center of the target
                            myThruster.target_velocity = 0.75f * myBody.velocity + 0.25f * (middle_direction * myBody.velocity.magnitude);
                            myThruster.target_orientation = (Vector2)transform.up;
                            break;
                        }

                    case 2: // going past the target area
                        myThruster.target_velocity = -myBody.velocity;
                        myThruster.target_orientation = -myBody.velocity;
                        break;

                    default: // should never happen unless "GetIntersectCount" is f*cked up
                        Debug.LogError($"{GetType().Name}({name}): 'GetIntersectCount' returned {zone_intersect_count} intersection, wich is not normal, 'zone_intersect_count' should be either 0, 1 or 2");
                        break;
                }
                break;

            default: // --------------------------------------------------------------------------------------
                Debug.LogError($"{GetType().Name}({name}): State {myStateMachine.State} is not implemented");
                break;
        }

        if(myThruster.target_velocity != null)
        {
            Debug.DrawRay(transform.position, (Vector3)myThruster.target_velocity, Color.blue);
        }
        if (myThruster.target_orientation != null)
        {
            Debug.DrawRay(transform.position, (Vector3)myThruster.target_orientation, Color.cyan);
        }

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

    protected Vector2 GetCollisionVector(int layermask)
    {
        float raycastRadius = Mathf.Max(myRenderer.bounds.size.x, myRenderer.bounds.size.y) / 2;
        float break_distance = GetBreakDistance();

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

    protected Vector2 GetAreaCollisionVector(Circle area)
    {
        float break_distance = GetBreakDistance();
        Vector2 stop_position = transform.position;
        stop_position += myBody.velocity.normalized * break_distance;

        //Debug.DrawLine(transform.position, stop_position, Color.red);

        if(Vector2.Distance(area.center, stop_position) > area.radius)
        {
            Vector2 stop_vector = (stop_position - area.center).normalized * area.radius;
            //stop_position = stop_vector + area.center;
            //Debug.DrawRay(stop_position, -stop_vector, Color.yellow);
            return -stop_vector;
        }

        return Vector2.zero;
    }

    // -------------------------------- UTILS

    protected float GetBreakDistance()
    {
        // break offset = time to turn in the right direction + time to compensate angular velocity (hack)
        float break_offset = (shipData.rotation_duration) + (Mathf.Abs(myBody.angularVelocity) / (shipData.rotation_thruster_force + 1e-6f));

        // break distance = distance passed while turning + distance passed while breaking
        float break_distance = (break_offset * myBody.velocity.magnitude) + ((myBody.velocity.magnitude * myBody.velocity.magnitude) / (2 * (shipData.main_thruster_force + shipData.rcs_thruster_force + 1e-6f)));
        return break_distance;
    }

    [ContextMenu("Update collider size")]
    protected void GetColliderSizeEstimation()
    {

        if (myRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no sprite renderer found in gameObject.");
            return;
        }

        if (myCollider == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no CapsuleCollider2D found in gameObject.");
            return;
        }

        myRenderer.sprite = shipData.sprite;

        // Rotate the transform, otherwise the bounding box is not accurate
        Quaternion temp_rotation = transform.rotation;
        transform.rotation = Quaternion.identity;
        float x = myRenderer.bounds.size.x;
        float y = myRenderer.bounds.size.y;
        transform.rotation = temp_rotation;

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

        Debug.Log($"{GetType().Name}({name}): 'Update collider size': estimated direction: {myCollider.direction} estimated size: {myCollider.size}");
        shipData.colliderDirection = myCollider.direction;
        shipData.colliderSize = myCollider.size;
    }

    [ContextMenu("Reset sprite")]
    protected void ResetSprite()
    {
        if (myRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no sprite renderer found in gameObject.");
            return;
        }

        myRenderer.sprite = shipData.defaultSprite;
        Debug.Log($"{GetType().Name}({name}): 'Reset sprite': sprite successfully changed");
    }
}
