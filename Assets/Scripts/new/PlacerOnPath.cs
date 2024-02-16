using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacerOnPath : MonoBehaviour
{
    public GameObject objToPlace;
    public Cinemachine.CinemachineSmoothPath path;
    [Range(0.0f, 5.0f)]
    public float offsetBetweenObjs = 1.0f;

    private List<GameObject> pathObjs = new List<GameObject>();
    
}
