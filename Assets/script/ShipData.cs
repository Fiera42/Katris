using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable

[CreateAssetMenu(fileName = "ShipData", menuName = "ScriptableObjects/ShipData")]
public class ShipData : ScriptableObject
{
    [Header("General")]
    public Sprite sprite;
    public CapsuleDirection2D colliderDirection;
    public Vector2 colliderSize;
    [HideInInspector] public Sprite defaultSprite;

    [Header("Flight param")]
    public float main_thruster_force;
    public float rotation_thruster_force;
    public float rcs_thruster_force;
    
    
    public float patrolSpeed
    {
        get
        {
            return 2 * main_thruster_force;
        }
    }

    public float rotation_duration
    {
        get
        {
            // Time the ship takes to do a 180° rotation ( https://www.desmos.com/calculator/zitqvgfz81 )
            return 2 * Mathf.Sqrt(2 * 180/(rotation_thruster_force + 1e-6f));
        }
    }
}
