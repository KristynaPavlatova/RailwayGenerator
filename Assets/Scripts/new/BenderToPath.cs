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
    public bool instantiateObjOverPathLength = true;
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

    // Gizmos variables
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    public async Task Bend()
    {
        bendingStatus = BendingStatus.IN_PROGRESS;
        await Task.Delay(0010);
        if(path != null && objToBend != null)
        {
            pathObjs = populatePathWithObjs(instantiateObjOverPathLength);
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
        bendingStatus = BendingStatus.WAITING_FOR_ACTION;
    }

    // Bending methods
    private void createVectorOfPathLength()
    {
        _startPoint = transform.TransformPoint(path.m_Waypoints[0].position);
        _endPoint = _startPoint + (new Vector3(0,0,1) * path.PathLength);

        bendingStatus = BendingStatus.SUCCESS;
    }
    private List<GameObject> populatePathWithObjs(bool shouldPopulateEntirePath)
    {
        ClearAllChildren(this.transform);//Clear previous population

        List<GameObject> objsOnPath = new List<GameObject>() { objToBend };

        if (shouldPopulateEntirePath)
        {
            objsOnPath.Clear();
            //instantiate objToBend over the path
            // create vector with length of the path
            float meshSizeZ = FindMeshSizes(objToBend.transform.GetChild(0).gameObject).z;
        
            int objsToPlace = (int)(path.PathLength / meshSizeZ);
            for (int i = 0; i < objsToPlace; i++)
            {
                Vector3 pos = path.transform.position + (new Vector3(0,0,meshSizeZ) * i);
                objsOnPath.Add(GameObject.Instantiate(objToBend, pos, Quaternion.identity, this.transform));
            }
        }
        return objsOnPath;
    }
    private Vector3 FindMeshSizes(GameObject obj)
    {
        //TO DO: Get mesh size from the obj that has the renderer on it --> you have to find out!
        MeshRenderer meshRend = obj.GetComponent<MeshRenderer>();        
        return new Vector3(meshRend.bounds.size.x, meshRend.bounds.size.y, meshRend.bounds.size.z);
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
}
