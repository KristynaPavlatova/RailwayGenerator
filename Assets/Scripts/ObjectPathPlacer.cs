using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class ObjectPathPlacer : MonoBehaviour
{
    //Script to be placed on an object that will holld all the placed objects.
    
    private Cinemachine.CinemachineSmoothPath path;
    private List<GameObject> objsToPlace;
    [Header("Placing properties:")]
    [Tooltip("How densely to place the objects. 0 - none, 100 - max density")]
    private float distanceBetween;
    private float objectSize;
    [Tooltip("When placing objs on path give them either +/- random rotation offset.")]
    private float rotationOffsetY;
    private int numOfObjects;

    public void GenerateObjectsOnPath(Cinemachine.CinemachineSmoothPath path, List<GameObject> objsToPlace, float objectSize, float distanceBetweenObjs, float rotationOffsetY)
    {   
        this.path = path;
        this.objsToPlace = objsToPlace;
        this.distanceBetween = distanceBetweenObjs;
        this.objectSize = objectSize;
        this.rotationOffsetY = rotationOffsetY;
        numOfObjects = (int)Math.Ceiling(path.PathLength / (objectSize + distanceBetween));

        checkReferences();
        PlaceObjectsOnPath();

        DestroyImmediate(this);
    }
    private void PlaceObjectsOnPath()
    {
        for (int i = 0; i < numOfObjects; i++)
        {
            Vector3 pos = path.EvaluatePositionAtUnit((objectSize + distanceBetween) * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            //Rotation + Roll:
            Quaternion orientationAtUnit = path.EvaluateOrientationAtUnit((objectSize + distanceBetween) * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            //TODO: HERE ADJUST ROLL BY INTERPOLATION
            
            //Add random Y rotation offset:
            float randOffsetY = UnityEngine.Random.Range(-rotationOffsetY, rotationOffsetY);
            if (i == 0)
            {
                randOffsetY = 0;//so,the rail properly fits on the wood
            }
            orientationAtUnit *= Quaternion.Euler(Vector3.up * randOffsetY);

            //Random chose from the list of objects
            GameObject.Instantiate(objsToPlace[UnityEngine.Random.Range(0, objsToPlace.Count)], pos, orientationAtUnit, this.transform);
        }
    }
    private void checkReferences()
    {
        Debug.Assert(objsToPlace[0], $"{this.name}: No wood will be placed because there are no object references in the list!");
    }
}
