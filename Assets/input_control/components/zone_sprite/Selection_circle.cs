using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Selection_circle : MonoBehaviour
{
    public float rotationSpeed;
    //public int subdivisions;
    //public bool drawGizmos; 

    //private LineRenderer myRenderer;

    /*
    private void Start()
    {
        myRenderer = gameObject.GetComponent<LineRenderer>();

        if (myRenderer == null)
        {
            enabled = false;
            Debug.LogError($"{GetType().Name}({name}): no line renderer found in gameObject.");
            return;
        }

        drawCircle();
    }

    private void OnValidate()
    {
        Start();
    }
    */

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    /*
    private void drawCircle()
    {
        if (subdivisions < 3)
        {
            subdivisions = 3;
        }

        float angleStep = 2f * Mathf.PI / subdivisions;

        myRenderer.positionCount = subdivisions;

        for (int i = 0; i < subdivisions; i++)
        {
            float x = Mathf.Cos(angleStep * i);
            float y = Mathf.Sin(angleStep * i);
            Vector2 point = new Vector2(x, y);

            myRenderer.SetPosition(i, point);
        }

        string[] values = myRenderer.sharedMaterial.GetPropertyNames(MaterialPropertyType.Texture);
        
        foreach (string value in values)
        {
            Debug.Log(value);
        }
        myRenderer.sharedMaterial.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
        Debug.Log(myRenderer.sharedMaterial.GetTextureScale("_MainTex"));
    }
    

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, transform.localScale.x/2);
    }
    */
}
