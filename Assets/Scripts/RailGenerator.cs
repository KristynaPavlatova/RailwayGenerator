using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class RailGenerator : MonoBehaviour
{
    private bool saveObjectAsPrefab = true;
    [Header("Saving properties:")]
    public string savePrefabDirectory = "Assets/_Project/Prefabs/";
    [Tooltip("To what directory in Assets should the generated tunnels be saved.")]
    public string saveMeshesDirectory = "Assets/_Project/Models/Level5/RailsGenerated";
    private bool wasPrefabUnpacked = false;

    [Header("Cinemachine path references:")]
    [Tooltip("Cinemachine Smooth path that is curved and that the railway will get generated on it.")]
    [Space(10)]
    public Cinemachine.CinemachineSmoothPath pathCurved;
    [Tooltip("Cinemachine Smooth path that is straight and serves for correct generation of the railway.")]
    public Cinemachine.CinemachineSmoothPath pathStraight;
    public bool useCustomLength;
    [Tooltip("Legth of the cinemachine smooth path. Make shure this corresponds with the legth of the 'pathCurved'.")]
    public float customPathLength;

    [Header("Prefab/Model references:")]
    [Tooltip("Prefab pro kovove casti koleji.")]
    [Space(10)]
    [SerializeField] private GameObject railMetalPrefab;
    [Tooltip("Prefab pro dreva pod koleje.")]
    [SerializeField] private List<GameObject> woodPrefabs = new List<GameObject>();

    [Header("Wood properties:")]
    public bool includeRoll = true;
    [Range(0, 360)]
    [Tooltip("When placing objs on path give them either +/- random rotation offset.")]
    public float rotationOffsetY = 15;
    [Tooltip("How much space to have between each wood prefabs.")]
    public float distanceBetweenWoods = 0.5f;

    [Header("Debug Gizmos:")]
    [Tooltip("Determines if scripts from single rails should be deleted because they are needed for gizmos.")]
    [HideInInspector]public bool showGizmos = true;
    [HideInInspector] public bool moveByVecsOn;
    [Header("Straight Path:")]
    [HideInInspector] public bool cpsPathStraightOn;
    [Header("Curve Path:")]
    [HideInInspector]public bool cpsPathOn;
    [HideInInspector]public bool finalNormalsOn;
    [HideInInspector] public bool newVertsOn;
    [Tooltip("Shows gizmo vector from closestPoint to its closest waypoint on the path.")]
    [HideInInspector] public bool cpToClosestWPOn;

    private GameObject railsHolder;
    private GameObject woodHolder;
    private float railSize;
    private float woodDepth;


    //SAVING FUNCTIONS ---------------------------------------------
    //private void Awake()
    //{
    //    wasPrefabUnpacked = false;
    //}
    /// <summary>
    /// Saves both rails and wood under. The rail meshes will be saved into its own folder and there will be a prefab of this object.
    /// </summary>
    public void SaveRailway()
    {
        AssetDatabase.Refresh();

        if (railsHolder.transform.childCount > 0)//prevnting creating an empty folder with no meshes
        {
            //Delete mesh FILES inside directory folder
            string customFolder = this.gameObject.name.ToString() + "_" + SceneManager.GetActiveScene().name.ToString();
            string saveDir = saveMeshesDirectory;
            string lastChar = saveMeshesDirectory.Substring(saveMeshesDirectory.Length - 1);//get last character of the string
            if (lastChar != "/") saveDir += "/";//make it a valid directory
            string fullPath = saveDir + customFolder;
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                DeleteFilesInDirectory(fullPath);
            }
            else
            {
                if (lastChar == "/")
                {
                    //make it a valid directory
                    saveDir = saveMeshesDirectory.Remove(saveMeshesDirectory.Length - 1);
                }
                AssetDatabase.CreateFolder(saveDir, customFolder);
            }

            //Save Meshes
            CheckRailsHolderReference();
            bool savingStatus = true;
            for (int i = 0; i < railsHolder.transform.childCount; i++)//railPrefab
            {
                for (int j = 1; j <= railsHolder.transform.GetChild(i).transform.childCount; j++)//rail L and R
                {
                    //railsHolder -> each railPrefab -> each single rail object
                    MeshBenderToPath railMeshBender = railsHolder.transform.GetChild(i).transform.GetChild(j - 1).GetComponent<MeshBenderToPath>();
                    Debug.Assert(railMeshBender, $"{this.name}: MeshBenderToPath script component could not be found! Regenerate the railway or just rails to save the railway. NOTE: once the railway is saved the 'MeshBenderToPath' component is deleted from all objects inside the 'RailsHolder' to have a clean prefab.");
                    savingStatus = railMeshBender.SaveRailMesh(fullPath, i.ToString() + j.ToString());

                    if (savingStatus)
                    {
                        if (i == (railsHolder.transform.childCount - 1) && j == 2)
                        {
                            Debug.Log($"{i * 2} Rail meshes saved. Directory: '{fullPath}'");
                        }
                    }
                    else break;
                }
                if (!savingStatus) break;
            }
            if (!savingStatus) Debug.LogError($"Rail meshes saving failed! Directory: '{fullPath}'");
        }
        else Debug.LogWarning($"{this.name}: Could not save any meshes because {this.gameObject.name} has no children under the RailsHolder parent.");
        
        //Save Prefab
        if (saveObjectAsPrefab)
        {
            PrefabUtility.SaveAsPrefabAsset(this.gameObject, savePrefabDirectory + $"{this.gameObject.name}.prefab", out bool result);
            if (result)
            {
                Debug.Log($"Prefab {this.gameObject.name.ToString()} saved into directory: '{savePrefabDirectory + this.gameObject.name + ".prefab"}'. NOTE: If there was a prefab with the same name it is now replaced with this prefab!");
            }
            else Debug.LogError($"Prefab {this.gameObject.name.ToString()} saving failed! Directory: '{savePrefabDirectory + this.gameObject.name + ".prefab"}'");
            wasPrefabUnpacked = false;//next time this (if is a) prefab can be unpack and (if any) children singleRail objects can be deleted
        }
    }
    private void DeleteSavedRails()
    {
        //Delete mesh FILES inside directory folder
        string customFolder = this.gameObject.name.ToString() + "_" + SceneManager.GetActiveScene().name.ToString();//Make sure it is the same as in the SaveRailway function
        string lastChar = saveMeshesDirectory.Substring(saveMeshesDirectory.Length - 1);//get last character of the string
        if (lastChar != "/") saveMeshesDirectory += "/";
        string fullPath = saveMeshesDirectory + customFolder;// + "/"
        if (AssetDatabase.IsValidFolder(fullPath))
        {
            DeleteFilesInDirectory(fullPath);
            FileUtil.DeleteFileOrDirectory(fullPath);
        }

        AssetDatabase.Refresh();
    }
    /// <summary>
    /// If is prefab, unpack it to delete children
    /// </summary>
    private void UnpackThisPrefab()
    {
        if (PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(this.gameObject) != null && PrefabUtility.GetPrefabInstanceHandle(this.gameObject.transform) != null)
        {
            PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
    }
    private void DeleteFilesInDirectory(string path)
    {
        //delete folder insides
        var files = Directory.GetFiles(path);
        if (files.Length != 0)
        {
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
        }
    }
    
    //BUTTON FUNCTIONS ---------------------------------------------
    public void GenerateRailway()
    {
        InitRailway();
        GenerateWood();
        GenerateRails();
    }
    public void DeleteRailway()
    {
        DeleteRails();
        DeleteWood();
    }

    public void GenerateWood()
    {
        InitRailway();
        if (woodHolder.transform.childCount != 0) DeleteWood();

        //Make sure the script exists and can be called --> script gets destroyed after generation
        if (woodHolder.GetComponent<ObjectPathPlacer>() == null)
            woodHolder.AddComponent<ObjectPathPlacer>();

        woodHolder.GetComponent<ObjectPathPlacer>().GenerateObjectsOnPath(pathCurved, woodPrefabs, woodDepth, distanceBetweenWoods, rotationOffsetY);

    }
    public void DeleteWood()
    {
        UnpackThisPrefab();
        CheckWoodHolderReference();
        if (woodHolder.transform.childCount != 0)
        {
            while (woodHolder.transform.childCount != 0)
            {
                foreach (Transform child in woodHolder.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning($"{this.name}: Could not delete any wood because {this.gameObject.name} has no children under the WoodHolder parent.");
        }
    }

    public void GenerateRails()
    {
        InitRailway();

        
        PlaceRailsOnPathStraight();
        BendRailMeshes();
    }
    public void DeleteRails()
    {
        CreateObjectHolders();
        CheckReferences();

        DeleteSavedRails();
        //Delete children objects
        if (!wasPrefabUnpacked)
        {
            UnpackThisPrefab();
            wasPrefabUnpacked = true;
        }
        if (railsHolder.transform.childCount > 0)
        {
            while (railsHolder.transform.childCount != 0)
            {
                foreach (Transform child in railsHolder.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }else Debug.LogWarning($"{this.name}: Could not delete any rails because {this.gameObject.name} has no children under the RailsHolder parent.");
    }

    public void DeleteObjectFromScene()
    {
        DeleteRailway();
        DestroyImmediate(this.gameObject);
    }

    //OTHER ---------------------------------------------
    private void InitRailway()
    {
        CheckReferences();
        CreateObjectHolders();
        FindMeshSizes();

        pathStraight.m_Waypoints[1].position.z = pathCurved.PathLength;
        Debug.Log($"TIP: If generated rails have spaces between them, check if the straight path in the 'DONT_TOUCH' obj has the correct PathLegth. If not, pass the correct value to the Z value of its last Waypoint. Repeat until the 'PathLength' says the correct value. (Sorry for the inconvenience, I am not sure why it is not updating automatically.)");

    }
    private void CheckReferences()
    {
        Debug.Assert(pathStraight, $"{this.name}: pathStraight Cinemachine smooth path reference is not defined!");
        Debug.Assert(pathCurved, $"{this.name}: pathCurved Cinemachine smooth path reference is not defined!");
        Debug.Assert(railMetalPrefab, $"{this.name}: railPrefab reference is not defined!");
        Debug.Assert(woodPrefabs[0], $"{this.name}: woodPrefab reference is not defined!");
    }
    private void CreateObjectHolders()
    {
        CheckRailsHolderReference();
        CheckWoodHolderReference();
    }
    private void CheckWoodHolderReference()
    {
        if (woodHolder == null)
        {
            woodHolder = this.transform.GetChild(1).gameObject;
            Debug.Assert(woodHolder, $"{this.name}: There is no children attached to this gameObject!");
        }
    }
    private void CheckRailsHolderReference()
    {
        if (railsHolder == null)
        {
            railsHolder = this.transform.GetChild(0).gameObject;
            Debug.Assert(railsHolder, $"{this.name}: There is no children attached to this gameObject!");
        }
    }
    private void FindMeshSizes()
    {
        woodDepth = woodPrefabs[0].GetComponentInChildren<MeshRenderer>().bounds.size.z;
        railSize = railMetalPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.size.z;
    }
    
    //GENERATION FUNCTIONS ---------------------------------------------
    private void PlaceRailsOnPathStraight()
    {
        if (this.transform.childCount != 0) DeleteRails();//clean previous
        CheckReferences();

        int railsToPlace = (int)(pathStraight.PathLength / railSize);
        for (int i = 0; i < railsToPlace; i++)
        {
            Vector3 pos = pathStraight.EvaluatePositionAtUnit(railSize * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            //Rotation + Roll:
            Quaternion orientationAtUnit = pathStraight.EvaluateOrientationAtUnit(railSize * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);

            GameObject.Instantiate(railMetalPrefab, pos, orientationAtUnit, railsHolder.transform);
        }
    }
    private void BendRailMeshes()
    {
        //For each railPrefab
        for (int i = 0; i < railsHolder.transform.childCount; i++)
        {
            //Position of the tunnel on the curved paths
            Vector3 parentPosCurved = pathCurved.EvaluatePositionAtUnit(railSize * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
            Vector3 parentPosStraight = pathStraight.EvaluatePositionAtUnit(railSize * i, Cinemachine.CinemachinePathBase.PositionUnits.Distance);

            for (int j = 0; j < railsHolder.transform.GetChild(i).transform.childCount; j++)//rail L and R
            {
                //railsHolder -> each railPrefab -> each single rail object
                railsHolder.transform.GetChild(i).transform.GetChild(j).GetComponent<MeshBenderToPath>().BendMeshAroundPath(pathStraight, pathCurved, showGizmos, includeRoll, parentPosStraight, parentPosCurved);
                if(showGizmos) railsHolder.transform.GetChild(i).transform.GetChild(j).GetComponent<MeshBenderToPath>().SetGizmos(cpsPathOn, cpsPathStraightOn, moveByVecsOn, finalNormalsOn, newVertsOn, cpToClosestWPOn);
            }
        }
    }
}
