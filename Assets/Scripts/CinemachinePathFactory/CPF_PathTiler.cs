using System.Collections.Generic;
using UnityEngine;

public class CPF_PathTiler : MonoBehaviour
{
    [Tooltip("Object to be tiled along the path.")]
    public GameObject tileObj;
    [Range(0.0001f, 5.0f)]
    public float offsetBetweenTiles = 0.5f;
    [Space(10)]
    [Header("Rotation limits:")]
    public bool includeRotX = true;
    public bool includeRotY = true;
    public bool includeRoll = true;
    [Space(10)]
    [Header("Random values:")]
    public bool randOffsetBetweenTiles = false;
    public float randOffsetBetweenTilesMin = 0.0f;
    public float randOffsetBetweenTilesMax = 5.0f;
    [Space(5)]
    public bool randRotY = false;
    [Range(-359,0)]
    public int randRotYMin = -90;
    [Range(0, 359)]
    public int randRotYMax = 90;

    private Cinemachine.CinemachineSmoothPath path;
    private List<GameObject> tiles = new List<GameObject>();
    public List<GameObject> GetTile() => tiles;

    private void Start()
    {
        PopulatePathWithTiles();
    }
    public void PopulatePathWithTiles()
    {
        if (!GetPathFromParent())
            return;

        ClearAllChildren(this.transform);

        Vector3 meshSize = FindMeshSizes(tileObj);
        int magicNumberToAddExtraObjectsAtTheEndOfThePathSoItIsFuller = 2;
        int objsToPlace = (int)(path.PathLength / (meshSize.z + offsetBetweenTiles)) + magicNumberToAddExtraObjectsAtTheEndOfThePathSoItIsFuller;

        for (int i = 0; i < objsToPlace; i++)
        {
            if (randOffsetBetweenTiles)
            {
                offsetBetweenTiles = Random.Range(randOffsetBetweenTilesMin, randOffsetBetweenTilesMax);
            }
            Vector3 position = path.EvaluatePositionAtUnit(i * (meshSize.z + offsetBetweenTiles), Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            
            Quaternion orientation = path.EvaluateOrientationAtUnit(i * (meshSize.z + offsetBetweenTiles), Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            if (!includeRoll)
                orientation.z = 0;
            if (!includeRotX)
                orientation.x = 0;
            if (!includeRotY)
                orientation.y = 0;

            if (randRotY)
                orientation *= Quaternion.Euler(Vector3.up * Random.Range(randRotYMin, randRotYMax));
            
            tiles.Add(GameObject.Instantiate(tileObj, position, orientation, this.transform));
        }
    }
    private bool GetPathFromParent()
    {
        path = this.transform.parent.GetComponent<Cinemachine.CinemachineSmoothPath>();
        if (path != null)
        {
            return true;
        }
        else
        {
            Debug.LogError($"Reference to a CinemachineSmoothPath in parent could not be found! Make sure the parent of this object has this component.");
            return false;
        }
    }
    private Vector3 FindMeshSizes(GameObject obj)
    {
        Vector3 meshSize = new Vector3(0, 0, 0);
        MeshRenderer meshRend = obj.GetComponent<MeshRenderer>();

        if (meshRend == null)
        {
            if(obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    meshSize = FindMeshSizes(obj.transform.GetChild(i).gameObject);
                    if (meshSize.z != 0)
                    {
                        //Debug.Log($"MeshSize found in a MeshRenderer of one of the children of {obj.name}.");
                        break;//we got the first available meshSize
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Mesh size of the {obj.name} could not be found due to missing Mesh Renderer! Therefore, path is tiled considering only the offset between tiles for their position along path.");
                return new Vector3(0, 0, 0);
            }
        }
        else
        {
            meshSize = meshRend.bounds.size;
        }
        return new Vector3(meshSize.x, meshSize.y, meshSize.z);
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
}
