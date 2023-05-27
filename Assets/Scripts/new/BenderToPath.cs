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
        AROUND
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

    [Space(20)]
    public bool bendingFailed = false;

    public async Task Bend()
    {
        bendingStatus = BendingStatus.IN_PROGRESS;
        await Task.Delay(0010);
        railwayObj.GetComponent<RailGenerator>().GenerateRailway();
        bendingStatus = BendingStatus.SUCCESS;
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
}
