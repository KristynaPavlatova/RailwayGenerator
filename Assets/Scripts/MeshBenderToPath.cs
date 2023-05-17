using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class MeshBenderToPath : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> meshVertices;
    List<Vector3> newVerts = new List<Vector3>();

    private Cinemachine.CinemachineSmoothPath pathCurved;
    private Cinemachine.CinemachineSmoothPath pathStraight;
    
    private List<float> cpsPathStraight = new List<float>();//closest points on the path for each mesh vertex
    private List<Vector3> cpsCurvedPathPositions = new List<Vector3>();
    private List<Vector3> moveByVecs = new List<Vector3>();
    private List<float> vertexOriginalDistanceToPath = new List<float>();
    private List<List<int>> threeClosestWPsIndices = new List<List<int>>();//roll lerping

    private bool isMeshOnLeftToPath;
    private bool includeRoll;
    private Vector3 parentPosCurved;
    private Vector3 parentPosStraight;

    //Gizmos
    private bool showGizmos;
    private bool cpsPathOn;
    private bool cpsPathStraightOn;
    private bool moveByVecsOn;
    private bool finalNormalsOn;
    private bool newVertsOn;
    private bool cpToClosestWPOn;
    private List<KeyValuePair<int, int>> twoClosestWpsIndices = new List<KeyValuePair<int, int>>();//for Gizmos
    private List<KeyValuePair<float, float>> tsRolls = new List<KeyValuePair<float, float>>();

    
    
    //GENERATION ----------------------------------------------------------------------------
    public void BendMeshAroundPath(Cinemachine.CinemachineSmoothPath pathStraight, Cinemachine.CinemachineSmoothPath path, bool showGizmos, bool includeRoll, Vector3 parentPosStraight, Vector3 parentPosCurved)
    {
        this.includeRoll = includeRoll;
        this.showGizmos = showGizmos;
        this.pathStraight = pathStraight;
        this.pathCurved = path;
        this.parentPosCurved = parentPosCurved;
        this.parentPosStraight = parentPosStraight;

        this.transform.parent.transform.position = parentPosStraight;

        GetMeshAndVertices();
        float cp = pathStraight.FindClosestPoint(meshVertices[0], 0, -1, 20);
        isMeshOnLeftToPath = DecideMeshOnLeft(pathStraight, cp, meshVertices[0]);

        //RAIL GENERATION:
        GetClosestPointsPathStraight();
        this.transform.parent.transform.position = parentPosCurved;

        GetPositionsOnPathFromPathStraight();
        MoveVertices();

        mesh.SetVertices(newVerts.Select(v => transform.InverseTransformPoint(v)).ToList());//world to local
        mesh.RecalculateBounds();
        this.GetComponent<MeshFilter>().mesh = mesh;
    }
    private void GetMeshAndVertices()
    {
        var origMesh = this.GetComponent<MeshFilter>().sharedMesh;
        mesh = new Mesh();
        mesh.vertices = origMesh.vertices;
        mesh.triangles = origMesh.triangles;
        mesh.normals = origMesh.normals;
        mesh.uv = origMesh.uv;

        Debug.Assert(mesh, $"{this.name}: Could not get Mesh from a Tunnel object.");
        meshVertices = mesh.vertices.Select(v => transform.TransformPoint(v)).ToList();//L -> W
    }
    private void GetClosestPointsPathStraight()
    {
        foreach (Vector3 vertex in meshVertices)
        {
            float cp = pathStraight.FindClosestPoint(vertex, 0, -1, 20);
            cpsPathStraight.Add(cp);

            //Store original distance from vertex to its closest point on the straight path.
            vertexOriginalDistanceToPath.Add((pathStraight.EvaluatePosition(cp) - vertex).magnitude);
        }
    }
    private void GetPositionsOnPathFromPathStraight()
    {
        //Get positions of points on the curved path by projecting the points on straigt line onto the curved path.
        //(It is possible because both paths have the same length)
        foreach (float cp in cpsPathStraight)
        {
            Vector3 a = pathCurved.EvaluatePositionAtUnit(cp, Cinemachine.CinemachinePathBase.PositionUnits.Normalized);
            cpsCurvedPathPositions.Add(a);
        }
    }
    private void MoveVertices()
    {
        for (int i = 0; i < meshVertices.Count; i++)
        {
            //Find closest point on the curved path by its position.
            //(Cannot be converted directly, therefore the FindClosestPoint method has to be used for the pointPosition to "find itself" in different units)
            float cp = pathCurved.FindClosestPoint(cpsCurvedPathPositions[i], 0, -1, 20);
            Vector3 normal = GetNormalFromPathPoint(cp);

            if(includeRoll) normal = RotateNormalToReflectRoll(normal, cp, i);
            
            //Decide vertex position:
            //Find final vertex position by moving along the normal of the cp on the curve path.
            //Move by the original distance of the vertex to cp on the straight path.
            Vector3 finalVert = pathCurved.EvaluatePosition(cp) + (normal * vertexOriginalDistanceToPath[i]);
            //Calculate the position of the vertex but without the Y value (preventing generating flat rails).
            //Instead of the vertex Y use the Y of the cp on the straight path.
            //This resolves in a "MoveByVector" that is in the same plane space thus not pointing down and moving the vectors down maing the rails flat.
            Vector3 vertexFlatPosPathStraight = new Vector3(meshVertices[i].x, pathStraight.EvaluatePosition(cpsPathStraight[i]).y, meshVertices[i].z);

            moveByVecs.Add(finalVert - vertexFlatPosPathStraight);
            newVerts.Add(meshVertices[i] + moveByVecs[i]);
        }
    }

    //OTHER ----------------------------------------------------------------------------
    private bool DecideMeshOnLeft(Cinemachine.CinemachineSmoothPath path, float pathPoint, Vector3 vertex)
    {
        //For getting correct normal from a tangent of a point on the path.
        Vector3 tangent = path.EvaluateTangent(pathPoint);
        Vector3 up = new Vector3(0,1,0);
        Vector3 right = (Vector3.Cross(up, tangent)).normalized;
        //Compare cross with vec from pathPoint to vertex
        Vector3 pointPos = path.EvaluatePosition(pathPoint);
        Vector3 pointToVertex = (new Vector3(vertex.x, pointPos.y, vertex.z) - pointPos).normalized;
        float dot = Vector3.Dot(pointToVertex, right);
        if (dot > 0)
        {
            return false;
        }
        else return true;
    }
    private Vector3 GetNormalFromPathPoint(float pathPoint)
    {
        //Rotate tangent +90 or -90 degrees to get path point normal.
        Vector3 normal = pathCurved.EvaluateTangent(pathPoint);
        normal.Normalize();

        if (isMeshOnLeftToPath)
        {
            normal = Rotate90CCW(normal);
        }
        else
        {
            normal = Rotate90CW(normal);
        }
        return normal;
    }
    Vector3 Rotate90CW(Vector3 aDir)
    {
        return new Vector3(aDir.z, 0, -aDir.x);
    }
    Vector3 Rotate90CCW(Vector3 aDir)
    {
        return new Vector3(-aDir.z, 0, aDir.x);
    }
    private List<int> FindIndicesOfThreeClosestWaypoints(Cinemachine.CinemachineSmoothPath path, Vector3 pointOnPath)
    {
        //generate list of 3 closest waypoints for current point of the path

        float dist1 = 1000;//closest wp dist
        float dist2 = 10000;//second closest wp dist
        float dist3 = 100000;
        int index1 = 0;
        int index2 = 0;
        int index3 = 0;

        for (int i = 0; i < path.m_Waypoints.Length; i++)
        {
            float currentDist = (pointOnPath - path.m_Waypoints[i].position).magnitude;
            if (currentDist <= dist1)
            {
                dist2 = dist1;
                index2 = index1;

                dist1 = currentDist;
                index1 = i;
            }
            else if (currentDist < dist2)
            {
                dist3 = dist2;
                index3 = index2;

                dist2 = currentDist;
                index2 = i;
            }else if (currentDist < dist3)
            {
                dist3 = currentDist;
                index3 = i;
            }
        }
        List<int> closestWPs = new List<int>();
        closestWPs.Add(index1);
        closestWPs.Add(index2);
        closestWPs.Add(index3);

        return closestWPs;
    }
    private Vector3 RotateNormalToReflectRoll(Vector3 normal, float cp, int vertexIndex)
    {
        Vector3 cpPosition = pathCurved.EvaluatePosition(cp);
        threeClosestWPsIndices.Add(FindIndicesOfThreeClosestWaypoints(pathCurved, cpPosition));//we know about two closest wps for each point on the curved path
        /* Evaluate which waypoints to use a the two closest waypoints a point on path is between.
         * Done based on the dot product between vectors A = WP1 - pathPoint, B = WP2 - pathPoint
         * Dot < 0
         * true -> take the 1st as wp1, 2nd as wp2
         * false -> take the 1st as wp1, 3rd as wp2
         */
        int firstClosestWPIndex = threeClosestWPsIndices[vertexIndex][0];
        int secondClosestWPIndex = threeClosestWPsIndices[vertexIndex][1];
        int thirdClosestWPIndex = threeClosestWPsIndices[vertexIndex][2];

        Cinemachine.CinemachineSmoothPath.Waypoint lowertWP = pathCurved.m_Waypoints[firstClosestWPIndex];
        Cinemachine.CinemachineSmoothPath.Waypoint higherWP = pathCurved.m_Waypoints[secondClosestWPIndex];
        Vector3 cpToLowerWP = lowertWP.position - cpPosition;
        Vector3 cpToHigherWP = higherWP.position - cpPosition;

        if (Vector3.Dot(cpToLowerWP, cpToHigherWP) > 0)//is actually between two closest points on the path?
        {
            //The point on the curved path is not actually placed between the two WPs on the path.
            //The found WPs need to be switched to get WPs that the path point is actually between on the path.
            secondClosestWPIndex = thirdClosestWPIndex;
            higherWP = pathCurved.m_Waypoints[secondClosestWPIndex];
        }
        if (firstClosestWPIndex > secondClosestWPIndex)//Sorting the fond waypoints by their indices
        {
            //x <- 1
            //1 <- 2
            //2 <- x
            int tempIndex = firstClosestWPIndex;
            firstClosestWPIndex = secondClosestWPIndex;
            secondClosestWPIndex = tempIndex;
        }

        //Define WP0 & WP1:
        lowertWP = pathCurved.m_Waypoints[firstClosestWPIndex];
        higherWP = pathCurved.m_Waypoints[secondClosestWPIndex];
        twoClosestWpsIndices.Add(new KeyValuePair<int, int>(firstClosestWPIndex, secondClosestWPIndex));//Gizmos

        //LERP CALCULATION:
        //A) Based on distance on the path:
        float pathDistLowerWP = pathCurved.FindClosestPoint(lowertWP.position, 0, -1, 20);
        float pathDistHigherWP = pathCurved.FindClosestPoint(higherWP.position, 0, -1, 20);
        float distBetweenWPS = pathDistHigherWP - pathDistLowerWP;
        float distPointToHigherWP = pathDistHigherWP - cp;
        float t = distPointToHigherWP / distBetweenWPS;
        
        //B) Based on simple distance in space between points:
        //float distanceBetweenTwoWPs = (lowertWP.position - higherWP.position).magnitude;
        //Calculate where the current closestPoint is between the two closest WPs for interpolation.
        //float t = (lowertWP.position - cpPosition).magnitude;
        //t /= distanceBetweenTwoWPs;//value between 0 - 1 -> dist between two closestWPs = 10, cp dist to closestWP1 = 3,5 -> 3,5 / 10 = 0.35

        //Calculate roll value by interpolating roll values of the two closest WPs
        float rollLerp = Mathf.Lerp(lowertWP.roll, higherWP.roll, t);
        tsRolls.Add(new KeyValuePair<float, float>(t, rollLerp));//Gizmos
        Vector3 lowerWPAxis = lowertWP.position - cpPosition;
        normal = Quaternion.AngleAxis(-rollLerp, lowerWPAxis) * normal;//Rotate the normal to reflect the roll value.
        return normal;
    }

    //SAVING ----------------------------------------------------------------------------
    public bool SaveRailMesh(string directory, string index)
    {
        if (AssetDatabase.IsValidFolder(directory))
        {
            if (mesh != null)
            {
                AssetDatabase.CreateAsset(mesh, $"{directory}/rail_{index}.mesh");
                AssetDatabase.SaveAssets();
                if (!showGizmos) DestroyImmediate(this);//Clean rail gameObject
                return true;
            }
            else
            {
                Debug.LogError($"{this.transform.parent.name} > {this.name}: The mesh was not assigned! Generate the whole railway or only rails again before trying to save it.");
                return false;
            }
        }
        else
        {
            Debug.LogError($"{this.name}: Could not save rail mesh because the directory {directory} is not valid!");
            return false;
        }
    }
    //DEBUG ----------------------------------------------------------------------------
    public void SetGizmos(bool cpsPathOn, bool cpsPathStraightOn, bool moveByVecsOn, bool finalNormalsOn, bool newVertsOn, bool cpToClosestWPOn)
    {
        this.cpsPathOn = cpsPathOn;
        this.cpsPathStraightOn = cpsPathStraightOn;
        this.moveByVecsOn = moveByVecsOn;
        this.finalNormalsOn = finalNormalsOn;
        this.newVertsOn = newVertsOn;
        this.cpToClosestWPOn = cpToClosestWPOn;
    }
    private void OnDrawGizmosSelected()
    {
        //Closest path points:
        if (cpsPathStraightOn)
        {
            Gizmos.color = Color.yellow;
            foreach (float cp in cpsPathStraight)
            {
                Gizmos.DrawSphere(pathStraight.EvaluatePosition(cp), 0.02f);
            }
        }
        if (cpsPathOn)
        {
            Gizmos.color = Color.red;
            foreach (Vector3 cp in cpsCurvedPathPositions)
            {
                Gizmos.DrawSphere(cp, 0.02f);
            }
        }

        //Move by vectors
        if (moveByVecsOn)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < moveByVecs.Count; i++)
            {
                Gizmos.DrawLine(pathStraight.EvaluatePosition(cpsPathStraight[i]), cpsCurvedPathPositions[i]);
            }
        }

        //new vertices:
        if (newVertsOn)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 newVertex in newVerts)
            {
                Gizmos.DrawSphere(newVertex, 0.005f);
            }
        }

        //Normals:
        if (finalNormalsOn)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < newVerts.Count; i++)
            {
                Gizmos.DrawLine(cpsCurvedPathPositions[i], newVerts[i]);
            }
        }

        //closest waypoint for closestPoints on curve path
        if (cpToClosestWPOn)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < cpsCurvedPathPositions.Count; i++)
            {
                Gizmos.DrawLine(cpsCurvedPathPositions[i], pathCurved.m_Waypoints[twoClosestWpsIndices[i].Key].position);//closest
            }
            Gizmos.color = Color.blue;
            for (int i = 0; i < cpsCurvedPathPositions.Count; i++)
            {
                Gizmos.DrawLine(cpsCurvedPathPositions[i], pathCurved.m_Waypoints[twoClosestWpsIndices[i].Value].position);//second closest
            }
            
        }
        //Debug.Log($"Rail is to the left {isMeshOnLeftToPath}");
    }
}
