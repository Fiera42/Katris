using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakePixelsTransparent : MonoBehaviour
{
    [SerializeField] private Image image;
    private void Awake()
    {
        image.alphaHitTestMinimumThreshold = 0.5f;
    }
}
