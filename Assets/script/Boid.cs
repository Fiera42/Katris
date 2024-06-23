using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // -------------------------------- EDITABLE

    // -------------------------------- PARAMS
    private Thrusters myThruster;
    public float rotation;
    public float speed;

    void Start()
    {
        myThruster = gameObject.GetComponent<Thrusters>();
        if (myThruster == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no Thruster script found in gameObject.");
        }
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
        Vector2 target_orientation = new Vector2(x, y).normalized * speed;

        myThruster.target_orientation = target_orientation;
        myThruster.target_velocity = target_orientation;
    }
}
