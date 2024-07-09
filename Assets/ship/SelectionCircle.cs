using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle : MonoBehaviour
{
    // -------------------------------- VALUES
    public Color displayColor;
    public ShipData shipData;

    // -------------------------------- PARAMS

    protected GameObject parent;
    protected SpriteRenderer myRenderer;

    private void Awake()
    {
        parent = transform.parent.gameObject;
        myRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (parent == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no parent GameObject found.");
            return;
        }

        if (myRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no SpriteRenderer found in gameObject.");
            return;
        }
    }

    private void OnEnable()
    {
        float width = Mathf.Max(shipData.colliderSize.x, shipData.colliderSize.y);
        transform.localScale = new Vector3(width, width, 1);
        myRenderer.color = displayColor;
    }

}
