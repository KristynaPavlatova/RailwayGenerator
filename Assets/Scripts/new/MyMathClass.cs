using UnityEngine;

public class MyMathClass : MonoBehaviour
{
    public static MyMathClass Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Calculated variables
    private Vector3 _fromThePointToStartPoint;
    private float _sideALength;
    private float _sideBLength;
    private float _sideCLength;
    public Vector3 findClosestPointOnLineToPoint(Vector3 lineStartPoint, Vector3 lineEndPoint, Vector3 givenPoint)
    {
        // Calculations based on trigonometry considering:
        // 1) Basic right triangle, where the right angle is at the vertex B.
        // 2) Vertex C is where the point we are trying to find the closest point on line is.
        // 3) Vertex A is one of the ends of the line.
        // 4) Vertex B is the resulting closest point on the line to the given point/vertex C.
        //
        //      C
        //     /|
        //  b / | a
        //   /  |
        //  /   |
        // A ---B
        //    c

        // Line (AB)
        Vector3 line = lineEndPoint - lineStartPoint;
        float lineLength = line.magnitude;

        // Side b
        Vector3 sideB = givenPoint - lineStartPoint;// Could be lineEndPoint as well.
        float sideBLength = sideB.magnitude;
        _sideBLength = sideBLength;

        // Triangle angles
        float alphaAngle = Vector3.Angle(sideB.normalized, line.normalized);
        float betaAngle = 90;
        float gamaAngle = 180 - (alphaAngle + betaAngle);

        // Side c
        // length: cos(Alpha) = c / b;
        float sideCLength = Mathf.Cos(Mathf.Deg2Rad * alphaAngle) * sideBLength;
        _sideCLength = sideCLength;

        // Side a
        // length: sin(Alpha) = a / b;
        float sideALength = Mathf.Sin(Mathf.Deg2Rad * alphaAngle) * sideBLength;
        _sideALength = sideALength;


        if (alphaAngle > 90)
        {
            return lineStartPoint;
        }
        else
        if (sideCLength > lineLength)
        {
            return lineEndPoint;
        }
        else
        {
            return (lineStartPoint + new Vector3(0, 0, sideCLength));
        }
    }
}
