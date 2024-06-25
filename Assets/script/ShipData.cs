using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "ScriptableObjects/ShipData")]
public class ShipData : ScriptableObject
{
    [Header("General")]
    public Sprite sprite;

    [Header("Flight param")]
    public float main_thruster_force;
    public float rotation_thruster_force;
    public float rcs_thruster_force;
    [HideInInspector] public float patrolSpeed = 10; // Should be the same for every ship type

    public float rotation_duration
    {
        get
        {
            // Time the ship takes to do a 180° rotation ( https://www.desmos.com/calculator/zitqvgfz81 )
            return 2 * Mathf.Sqrt(180/(rotation_thruster_force + 1e-6f));
        }
    }
}
