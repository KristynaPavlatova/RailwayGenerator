using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class BenderToPath : MonoBehaviour
{
    public GameObject objToBend;
    public enum BendType
    {
        FLAT,
        ROUND
    }
    public BendType bendType;
    public Cinemachine.CinemachineSmoothPath path;

    public enum BendingStatus
    {
        WAITING_FOR_ACTION,
        IN_PROGRESS,
        SUCCESS,
        FAILED
    }
    [HideInInspector]
    public BendingStatus bendingStatus = BendingStatus.WAITING_FOR_ACTION;
    [HideInInspector]
    public string bendingStatusOutputMessage = "";
    [Space(10)]
    public string saveMeshesTo = "Assets/Art/Meshes/BenderToPath";
    public string savePrefabTo = "Assets/Prefabs/BenderToPath";

    private List<GameObject> pathObjs = new List<GameObject>();
    
    
    [Range(0.0f, 5.0f)]
    public float offsetBetweenObjs = 1.0f;

    // Gizmos variables
    //private Vector3 _startPoint;
    //private Vector3 _endPoint;

    public async Task Bend()
    {
        bendingStatus = BendingStatus.IN_PROGRESS;
        await Task.Delay(0010);
        if(path != null && objToBend != null)
        {
            placeObjectsOnStraightPath(pathObjs);
            bendingStatus = (pathObjs.Count > 0) ? BendingStatus.SUCCESS :  BendingStatus.FAILED;
            bendingStatus = BendingStatus.SUCCESS;
        }
        else
        {
            bendingStatusOutputMessage = "Bending could not be perform because there is no path assigned or reference to the 'objToBend' is missing!";
            Debug.LogError($"{bendingStatusOutputMessage}");
            bendingStatus = BendingStatus.FAILED;
        }
    }
    public async Task Save()
    {
        bendingStatus = BendingStatus.IN_PROGRESS;
        await Task.Delay(1000);
        bendingStatus = BendingStatus.SUCCESS;
    }
    public async Task Delete()
    {
        bendingStatus = BendingStatus.IN_PROGRESS;
        await Task.Delay(0010);

        //Delete gameobjects and references from list
        ClearAllChildren(this.transform);
        pathObjs.Clear();

        bendingStatus = BendingStatus.WAITING_FOR_ACTION;
    }

    // Bending methods
    private void createVectorOfPathLength()
    {
        //_startPoint = transform.TransformPoint(path.m_Waypoints[0].position);
        //_endPoint = _startPoint + (new Vector3(0,0,1) * path.PathLength);

        bendingStatus = BendingStatus.SUCCESS;
    }
    private void placeObjectsOnStraightPath(List<GameObject> objsOnPathList)
    {
        ClearAllChildren(this.transform);//Clear previous population of path objects
        objsOnPathList.Clear();

        //float meshSizeZ = FindMeshSizes(objToBend.transform.GetChild(0).gameObject).z;
        float meshSizeZ = FindMeshSizeRecursion(objToBend.transform).z;

        // Instantiate copies of objToBend object over the given path
        int objsToPlace = (int)Math.Ceiling(path.PathLength / (meshSizeZ + offsetBetweenObjs));
        for (int i = 0; i < objsToPlace; i++)
        {
            Vector3 pos = path.transform.position + (new Vector3(0,0,meshSizeZ + offsetBetweenObjs) * i);
            objsOnPathList.Add(GameObject.Instantiate(objToBend, pos, Quaternion.identity, this.transform));
        }
    }
    private Vector3 FindMeshSizeRecursion(Transform trans)
    {
        Debug.Log($"Finding mesh renderer inside transform {trans.gameObject.name}:");

        Vector3 meshSize = new Vector3(0,0,0);

        MeshRenderer meshRend = trans.GetComponent<MeshRenderer>();
        if(meshRend)
        {
            meshSize = new Vector3(meshRend.bounds.size.x, meshRend.bounds.size.y, meshRend.bounds.size.z);
            Debug.Log($"Found the mesh size! It is {meshSize}");
        }
        else
        {
            int numOfChildren = trans.childCount;
            if (numOfChildren > 0)
            {
                for (int i = 0; i < numOfChildren; i++)
                {
                    meshSize = FindMeshSizeRecursion(trans.GetChild(i).transform);
                    if (meshSize.z != 0)
                        break;
                }
            }
            else
            {
                Debug.LogError($"the transform {trans.gameObject.name} does not have a Mesh Renderer to find out the size of the mesh (Z axis)!");
            }
        }
        return meshSize;
    }
    private void ClearAllChildren(Transform inTransform)
    {
        if (inTransform.childCount > 0)
        {
            while (inTransform.childCount != 0)
            {
                foreach (Transform child in inTransform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    // Gizmos methods
    /*
    private void OnDrawGizmosSelected()
    {
        //displayGizmosLine(_startPoint, _endPoint, Color.blue);
        if (_startPoint != null && _endPoint != null)
        {
            displayGizmosLine(_startPoint, _endPoint, Color.blue);
            displayGizmoSphere(_startPoint, 0.2f, Color.white);
            displayGizmoSphere(_endPoint, 0.2f, Color.white);
        }
    }
    private void displayGizmosLine(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, to);
    }
    private void displayGizmoSphere(Vector3 position, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(position, radius);
    }
    */
}
