using UnityEngine;
using TMPro;

public class FindClosestPointOnPath_ShowcaseUI : MonoBehaviour
{
    public float pathLength = 7.5f;
    [Tooltip("The point to which we are calculating the closest point on the line.")]
    public Vector3 thePoint;

    [Space(10)]
    [Header("Showcase animation:")]
    [Tooltip("Turn on animated showcase where the point is moving around based on predefined positions.")]
    public bool turnOnShowcase = false;
    [Tooltip("Size of the points in the triangle.")]
    public float gizmosSphereRadius = 0.1f;
    [Tooltip("The speed of the animation.")]
    [Range(0.0f,5.0f)]
    public float lerpSpeed = 2.0f;
    [Space(10)]
    [Header("UI elements:")]
    public TextMeshPro A;
    public TextMeshPro B;
    public TextMeshPro C;
    [Space(10)]
    public TextMeshPro sideALengthUI;
    public TextMeshPro sideBLengthUI;
    public TextMeshPro sideCLengthUI;
    [Space(10)]    
    private int _currentPosOfThePoint = 0;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private Vector3 _closestPointOnLine;

    // Calculated variables
    private Vector3 _fromThePointToStartPoint;
    private float _sideALength;
    private float _sideBLength;
    private float _sideCLength;

    public Vector3 findClosestPointOnLineToAPoint(Vector3 lineStartPoint, Vector3 lineEndPoint, Vector3 givenPoint)
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

    private void Update()
    {
        if (turnOnShowcase)
        {
            Vector3 lerpPositionOfThePoint = new Vector3(0, 0, 0);
            Vector3 lerpEndPosition = new Vector3(0, 0, 0);
            switch (_currentPosOfThePoint)
            {
                case 0:
                    lerpEndPosition = new Vector3(-2, 0, 2);
                    break;
                case 1:
                    lerpEndPosition = new Vector3(-3, 0, -2);
                    break;
                case 2:
                    lerpEndPosition = new Vector3(-1, 0, 5);
                    break;
                case 3:
                    lerpEndPosition = new Vector3(1.5f, 0, 2);
                    break;
                case 4:
                    lerpEndPosition = new Vector3(1, 0, 5);
                    break;
                case 5:
                    lerpEndPosition = new Vector3(0, 0, 5);
                    break;
            }

            if (new Vector3(thePoint.x, thePoint.y, thePoint.z) != lerpEndPosition)
            {
                lerpPositionOfThePoint = Vector3.Lerp(new Vector3(thePoint.x, thePoint.y, thePoint.z), lerpEndPosition, (lerpSpeed / 100));
                thePoint.x = lerpPositionOfThePoint.x;
                thePoint.y = lerpPositionOfThePoint.y;
                thePoint.z = lerpPositionOfThePoint.z;
            }
            else
            {
                if (_currentPosOfThePoint < 5)
                {
                    _currentPosOfThePoint++;
                }
                else if (_currentPosOfThePoint == 5)
                {
                    _currentPosOfThePoint = 0;
                }

            }
        }

    }

    // Gizmos methods
    private void OnDrawGizmosSelected()
    {
        //displayGizmosLine(_startPoint, _endPoint, Color.blue);
        if (pathLength > 0)
        {
            drawTheBaseLine(Color.blue, gizmosSphereRadius);
            drawLineFromThePointToStartPoint(Color.yellow, gizmosSphereRadius);
            drawTheClosestPoint(Color.red, gizmosSphereRadius);
            drawLineFromStartToClosestPoint(_startPoint, _closestPointOnLine, Color.green);
            drawLineFromPointToClosestPoint(thePoint, _closestPointOnLine, Color.red);

            positionTextAtVerteces();
            updateTextUI();
        }
    }
    private void drawGizmosLine(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, to);
    }
    private void drawGizmoSphere(Vector3 position, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(position, radius);
    }

    private void drawTheBaseLine(Color color, float radius)
    {
        _startPoint = new Vector3(0, 0, 0);
        _endPoint = _startPoint + (new Vector3(0, 0, 1) * pathLength);
        drawGizmosLine(_startPoint, _endPoint, color);
        drawGizmoSphere(_startPoint, radius, color);
        drawGizmoSphere(_endPoint, radius, color);
    }
    private void drawLineFromThePointToStartPoint(Color color, float radius)
    {
        drawGizmoSphere(thePoint, radius, color);
        drawGizmosLine(thePoint, _startPoint, color);
    }
    private void drawTheClosestPoint(Color color, float radius)
    {
        _closestPointOnLine = findClosestPointOnLineToAPoint(_startPoint, _endPoint, thePoint);
        drawGizmoSphere(_closestPointOnLine, radius, color);
    }
    private void drawLineFromPointToClosestPoint(Vector3 point, Vector3 closestPoint, Color color)
    {
        drawGizmosLine(point, closestPoint, color);
    }
    private void drawLineFromStartToClosestPoint(Vector3 from, Vector3 to, Color color)
    {
        drawGizmosLine(from, to, color);
    }

    private void positionTextAtVerteces()
    {
        A.transform.position = new Vector3(0, 0, 0);
        B.transform.position = _closestPointOnLine;
        C.transform.position = thePoint;
    }
    private void updateTextUI()
    {
        sideALengthUI.text = $"a = {_sideALength.ToString("f2")}";
        sideBLengthUI.text = $"b = {_sideBLength.ToString("f2")}";
        sideCLengthUI.text = $"c = {_sideCLength.ToString("f2")}";
    }
}
