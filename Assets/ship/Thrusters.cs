using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Thrusters : MonoBehaviour {

    // -------------------------------- EDITABLE
    public ShipData shipData;

    // -------------------------------- PARAMS
    protected Rigidbody2D myBody;

    public Vector2? target_velocity;
    public Vector2? target_orientation;

    protected void Awake() 
    {
        // Get components
        myBody = gameObject.GetComponent<Rigidbody2D>();

        if (shipData == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): shipData is null.");
            return;
        }

        if (myBody == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no rigidBody found in gameObject.");
            return;
        }
    }

    protected void FixedUpdate()
    {
        thrust(target_velocity);
        rcsThrust(target_velocity);
        rotationThrust(target_orientation);
    }
    protected void rotationThrust(Vector2? arg_target_orientation = null)
    {
        Vector2 target_orientation;

        // If no orientation provided, align to the direction of movement (or not if there is no movement)
        if (arg_target_orientation is null)
        {
            Vector2 normalized_target_velocity = myBody.velocity.normalized;
            target_orientation = (normalized_target_velocity == Vector2.zero) ? transform.up : normalized_target_velocity;
        }
        else
        {
            target_orientation = (Vector2)arg_target_orientation;
        }

        // Estimate the point where we will stop if we use our thrusters to slow down and stop
        float angle_error = Vector2.SignedAngle(transform.up, target_orientation);

        // If we already have the right orientation, do nothing
        if (angle_error == 0 && myBody.angularVelocity == 0) 
        { 
            return;
        }

        // Calculate the distance we would take to stop completly the rotation
        float settle_distance = ((MathF.Abs(myBody.angularVelocity) / (shipData.rotation_thruster_force + 1e-6f)) * myBody.angularVelocity) / 2;
        float settle_point = Mathf.Repeat(angle_error - settle_distance + 180, 360) - 180;

        // Calculate the angle change in the next frame using current angular velocity and deltaTime
        float nextAngleChange = myBody.angularVelocity * Time.fixedDeltaTime;

        // If we approach the right angle and can stop within the next frame, we stop
        if (Mathf.Abs(myBody.angularVelocity) <= shipData.rotation_thruster_force * Time.fixedDeltaTime + 15 && Mathf.Abs(angle_error) <= Mathf.Abs(nextAngleChange))
        {
            myBody.angularVelocity = 0f;
            myBody.rotation += angle_error;
        }

        // If sign of settle_point == sign of current angular velocity, going faster is the fastest way to reach target
        else if (MathF.Sign(settle_point) == MathF.Sign(myBody.angularVelocity))
        {
            myBody.angularVelocity += shipData.rotation_thruster_force * Time.fixedDeltaTime * MathF.Sign(settle_point);
        }

        // If sign of settle_point != sign of current angular velocity, slowing down and reverse is the fastest way
        else
        {
            myBody.angularVelocity -= shipData.rotation_thruster_force * Time.fixedDeltaTime * -(MathF.Sign(settle_point));
        }   
    }

    protected void rcsThrust(Vector2? target_velocity = null)
    {
        if (target_velocity is null)
        {
            target_velocity = Vector2.zero;
        }

        // RCS speed correction
        if (myBody.velocity != target_velocity)
        {
            Vector2 velocity_difference = (Vector2)target_velocity - myBody.velocity;
            Vector2 direction = velocity_difference.normalized;

            float additional_velocity_magnitude = MathF.Min(velocity_difference.magnitude, shipData.rcs_thruster_force * Time.fixedDeltaTime);

            Vector2 additional_velocity = direction * additional_velocity_magnitude;
            myBody.velocity += additional_velocity;
        }
    }

    protected void thrust(Vector2? target_velocity = null)
    {
        if (target_velocity is null)
        {
            target_velocity = Vector2.zero;
        }

        // If I already have the right speed
        if (myBody.velocity == target_velocity)
        {
            return;
        }

        // Dot product -> use thrust only when facing the right direction
        Vector2 target_direction = (target_velocity == Vector2.zero) ? myBody.velocity.normalized : ((Vector2)target_velocity).normalized;
        float dot_angle = Vector2.Dot(target_direction, transform.up);

        
        if(Mathf.Abs(dot_angle) < 0.9f) // We are facing a weird direction, dont thrust
        {
            return;
        }

        // Check our actual speed in the wanted direction
        Vector2 actual_speed = ((Vector2)target_velocity).normalized * myBody.velocity;
        float actual_speed_magnitude = actual_speed.magnitude * MathF.Sign(Vector2.Dot(myBody.velocity, (Vector2)target_velocity));

        /*
        Debug.DrawRay(transform.position, myBody.velocity, Color.red);
        Debug.DrawRay(transform.position, ((Vector2)target_velocity).normalized * actual_speed_magnitude, Color.yellow);
        Debug.DrawRay(transform.position, ((Vector2)target_velocity).normalized, Color.green);
        Debug.Log(actual_speed_magnitude);
        */

        // If I want to go faster
        if (actual_speed_magnitude < ((Vector2)target_velocity).magnitude)
        {
            float additional_velocity = shipData.main_thruster_force * MathF.Max(0, dot_angle) * Time.fixedDeltaTime;

            float magnitude_differential = ((Vector2)target_velocity).magnitude - actual_speed_magnitude;
            additional_velocity = MathF.Min(additional_velocity, magnitude_differential);

            myBody.velocity += additional_velocity * (Vector2)transform.up;
        }

        // If I want to go slower
        else
        {
            float additional_velocity = shipData.main_thruster_force * MathF.Max(0, -dot_angle) * Time.fixedDeltaTime;

            float magnitude_differential = actual_speed_magnitude - ((Vector2)target_velocity).magnitude;
            additional_velocity = MathF.Min(additional_velocity, magnitude_differential);

            myBody.velocity += additional_velocity * (Vector2)transform.up;
        }
    }
}
