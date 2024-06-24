using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // -------------------------------- EDITABLE
    [SerializeField] protected ShipData shipData;

    // -------------------------------- PARAMS
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D myBody;
    protected Thrusters myThruster;
    protected Vector2 boidSize;

    public float speed;

    protected void OnValidate()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        updateColliderSize();
    }

    protected void Start()
    {
        // Get components
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        myBody = myBody = gameObject.GetComponent<Rigidbody2D>();
        myThruster = gameObject.GetComponent<Thrusters>();

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

        if (myBody == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no rigidBody found in gameObject.");
            return;
        }

        if (spriteRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no sprite renderer found in gameObject.");
            return;
        }

        // Start method itself
        spriteRenderer.sprite = shipData.sprite;
        updateColliderSize();
    }

    protected void FixedUpdate()
    {
        Vector2 target_velocity = transform.up * speed;

        float maxNoiseMagnitude = speed * 0.1f;
        float noiseX = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);
        float noiseY = Random.Range(-maxNoiseMagnitude, maxNoiseMagnitude);
        
        target_velocity += new Vector2(noiseX, noiseY);

        Vector2? collision_vector = getCollisionVector();

        myThruster.target_velocity = (collision_vector == Vector2.zero) ? target_velocity : collision_vector;
        myThruster.target_orientation = (collision_vector == Vector2.zero) ? target_velocity : collision_vector;
    }

    protected Vector2 getCollisionVector()
    {
        float raycastRadius = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) / 2;

        // break offset = time to turn in the right direction + time to compensate angular velocity
        float break_offset = (shipData.rotation_duration) + (Mathf.Abs(myBody.angularVelocity) / shipData.rotation_thruster_force);

        // break distance = distance passed while turning + distance passed while breaking
        float break_distance = (break_offset * myBody.velocity.magnitude) + ((myBody.velocity.magnitude * myBody.velocity.magnitude) / (2 * (shipData.main_thruster_force + shipData.rcs_thruster_force)));

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, raycastRadius, myBody.velocity.normalized, break_distance, 1 << LayerMask.NameToLayer("Obstacle"));

        //Debug.DrawRay(transform.position, myBody.velocity.normalized * break_distance, Color.red);

        if (hit.collider != null)
        {
            Vector2 collisionVector = hit.centroid - hit.point;
            //Debug.DrawRay(hit.point, collisionVector.normalized * break_distance, Color.yellow);
            return collisionVector.normalized * break_distance;
        }

        return Vector2.zero;
    }

    // -------------------------------- UTILS
    protected void updateColliderSize()
    {
        CapsuleCollider2D myCollider = gameObject.GetComponent<CapsuleCollider2D>();

        if(myCollider == null || spriteRenderer == null)
        {
            return;
        }

        float x = spriteRenderer.bounds.size.x;
        float y = spriteRenderer.bounds.size.y;

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
