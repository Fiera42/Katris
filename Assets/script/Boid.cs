using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour {

    [Header("Flight param")]
    [SerializeField] private float main_thruster_force;
    [SerializeField] private float rotation_thruster_force;
    [SerializeField] private float rcs_thruster_force;

    public float rotation;
    public float speed;

    private Rigidbody2D myBody;

    void Start() 
    {
        myBody = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rotation = Random.Range(-180f, 180f);
        }
    }

    private void FixedUpdate()
    {
        float angleRad = rotation * Mathf.Deg2Rad;

        // Calculate x and y components of the vector
        float x = Mathf.Sin(angleRad);
        float y = Mathf.Cos(angleRad);

        // Return the normalized vector
        Vector2 target_orientation = new Vector2(x, y).normalized;

        thrust(target_orientation * speed);
        rcsThrust(target_orientation * speed);
        rotationThrust(target_orientation * speed);
    }

    private void rotationThrust(Vector2? arg_target_orientation = null)
    {
        Vector2 target_orientation;

        // If no orientation provided, align to the direction of movement (or not if there is no target movement)
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

        // If we already have the right orientation
        if (angle_error == 0 && myBody.angularVelocity == 0) 
        { 
            return;
        }

        float settle_distance = ((MathF.Abs(myBody.angularVelocity) / rotation_thruster_force) * myBody.angularVelocity) / 2;
        float settle_point = Mathf.Repeat(angle_error - settle_distance + 180, 360) - 180;

        // Calculate the angle change in the next frame using current angular velocity and deltaTime
        float nextAngleChange = myBody.angularVelocity * Time.fixedDeltaTime;

        //print(settle_point);

        // If we approach the right angle and can stop within the next frame, we stop
        if (Mathf.Abs(myBody.angularVelocity) <= rotation_thruster_force * Time.fixedDeltaTime + 15 && Mathf.Abs(angle_error) <= Mathf.Abs(nextAngleChange))
        {
            myBody.angularVelocity = 0f;
            myBody.rotation += angle_error;
        }

        // If sign of settle_point == sign of current angular velocity, going faster is the fastest way to reach target
        else if (MathF.Sign(settle_point) == MathF.Sign(myBody.angularVelocity))
        {
            myBody.angularVelocity += rotation_thruster_force * Time.fixedDeltaTime * MathF.Sign(settle_point);
        }

        // If sign of settle_point != sign of current angular velocity, slowing down and reverse is the fastest way
        else
        {
            myBody.angularVelocity -= rotation_thruster_force * Time.fixedDeltaTime * -(MathF.Sign(settle_point));
        }   
    }

    private void rcsThrust(Vector2? target_velocity = null)
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

            float additional_velocity_magnitude = MathF.Min(velocity_difference.magnitude, rcs_thruster_force * Time.fixedDeltaTime);

            Vector2 additional_velocity = direction * additional_velocity_magnitude;
            myBody.velocity += additional_velocity;
        }
    }

    private void thrust(Vector2? target_velocity = null)
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

        // If I want to go faster
        if (myBody.velocity.magnitude < ((Vector2)target_velocity).magnitude)
        {
            float additional_velocity = main_thruster_force * MathF.Max(0, dot_angle) * Time.fixedDeltaTime;

            float magnitude_differential = ((Vector2)target_velocity).magnitude - myBody.velocity.magnitude;
            additional_velocity = MathF.Min(additional_velocity, magnitude_differential);

            myBody.velocity += additional_velocity * (Vector2)transform.up;
        }

        // If I want to go slower
        else
        {
            float additional_velocity = main_thruster_force * MathF.Max(0, -dot_angle) * Time.fixedDeltaTime;

            float magnitude_differential = myBody.velocity.magnitude - ((Vector2)target_velocity).magnitude;
            additional_velocity = MathF.Min(additional_velocity, magnitude_differential);

            myBody.velocity += additional_velocity * (Vector2)transform.up;
        }
    }
}
