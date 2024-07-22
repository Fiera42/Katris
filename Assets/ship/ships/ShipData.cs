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
    public static float patrolDuration = 1;

    public float Rotation_duration
    {
        get
        {
            // Time the ship takes to do a 180° rotation ( https://www.desmos.com/calculator/zitqvgfz81 )
            return 2 * Mathf.Sqrt(2 * 180 / (rotation_thruster_force + 1e-6f));
        }
    }

    // -------------------------------- LIST OF ALL SHIPS
    private static readonly Dictionary<ShipData, List<ShipStateMachine>> AllShips = new();
    
    public List<ShipStateMachine> GetAllShipOfType()
    {
        return AllShips.GetValueOrDefault(this);
    }

    public void RegisterShip(ShipStateMachine ship)
    {
        if(ship == null)
        {
            return;
        }

        List<ShipStateMachine> ships = AllShips.GetValueOrDefault(this);

        if(ships != null)
        {
            ships.Add(ship);
        }
        else
        {
            AllShips.Add(this, new());
            AllShips.GetValueOrDefault(this).Add(ship);
        }
        
    }

    public void ForgetShip(ShipStateMachine ship)
    {
        if (ship == null)
        {
            return;
        }

        List < ShipStateMachine > ships = AllShips.GetValueOrDefault(this);
        if (ships == null)
        {
            return;
        }

        ships.Remove(ship);
        if(ships.Count < 1)
        {
            AllShips.Remove(this);
        }
    }
}
