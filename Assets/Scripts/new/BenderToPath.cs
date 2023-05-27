using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class BenderToPath : MonoBehaviour
{
    public enum BendType
    {
        FLAT,
        ROUND
    }
    public BendType bendType;

    public enum BendingStatus
    {
        WAITING_FOR_ACTION,
        IN_PROGRESS,
        SUCCESS,
        FAILED
    }
    [HideInInspector]
    public BendingStatus bendingStatus = BendingStatus.WAITING_FOR_ACTION;
    [Space(10)]
    public string saveMeshesTo = "Assets/Art/Meshes/BenderToPath";
    public string savePrefabTo = "Assets/Prefabs/BenderToPath";

    [Space(20)]
    public GameObject flatPrefab;
    public GameObject roundPrefab;

    [Space(20)]
    public GameObject railwayObj;
    public Cinemachine.CinemachineSmoothPath path;

    [Space(20)]
    public bool bendingFailed = false;

    // Gizmos variables
    private Vector3 _startPoint;
    private Vector3 _endPoint;


    public async Task Bend()
    {
        //bendingStatus = BendingStatus.IN_PROGRESS;
        //await Task.Delay(0010);
        /*if(railwayObj != null)
        {
            railwayObj.GetComponent<RailGenerator>().GenerateRailway();
            bendingStatus = BendingStatus.SUCCESS;
        }
        else
        {
            Debug.LogError($"{this.name} could not perform bending because there is no path assigned!");
            bendingStatus = BendingStatus.FAILED;
        }*/

        //if(path != null)
        //{
        //    createVectorOfPathLength();
        //}
        //
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
        railwayObj.GetComponent<RailGenerator>().DeleteRailway();
        bendingStatus = BendingStatus.WAITING_FOR_ACTION;
    }

    // Bending methods
    private void createVectorOfPathLength()
    {
        _startPoint = transform.TransformPoint(path.m_Waypoints[0].position);
        _endPoint = _startPoint + (new Vector3(0,0,1) * path.PathLength);

        bendingStatus = BendingStatus.SUCCESS;
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
