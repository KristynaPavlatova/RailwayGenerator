using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FindClosestPointOnLineToPoint : MonoBehaviour
{
    // Gizmos variables
    public float lineLength = 10;
    public Vector3 thePoint;
    [Space(10)]
    public float gizmosSphereRadius = 0.1f;
    [Space(10)]
    public TextMeshPro A;
    public TextMeshPro B;
    public TextMeshPro C;
    [Space(10)]
    public TextMeshPro sidesLengthUI;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private Vector3 _closestPointOnLine;

    // Calculated variables
    private Vector3 _fromThePointToStartPoint;
    private float _sideALength;
    private float _sideBLength;
    private float _sideCLength;

    private Vector3 findClosestPointOnLineToAPoint(Vector3 lineStartPoint, Vector3 lineEndPoint, Vector3 point)
    {
        //TODO: consider when point is before the start or after the end of the line
        // Calculations based on trigonometry considering...
        // basic right triangle with right angle at the vertex B:
        //      C
        //     /|
        //  b / | a
        //   /  |
        //  /   |
        // A ---B
        //   c

        // Line
        Vector3 line = lineEndPoint - lineStartPoint;
        float lineLength = line.magnitude;

        // Side b
        Vector3 sideB = point - lineStartPoint;
        float sideBLength = sideB.magnitude;
        _sideBLength = sideBLength;

        // Alpha angle
        float alphaAngle = Vector3.Angle(sideB.normalized, line.normalized);
        // Beta angle
        float betaAngle = 90;
        // Gama angle
        float gamaAngle = 180 - (alphaAngle + betaAngle);


        // Side c
        // length: cos(Alpha) = c / b;
        float sideCLength = Mathf.Cos(Mathf.Deg2Rad * alphaAngle) * sideBLength;
        _sideCLength = sideCLength;

        // Side a
        // length: sin(Alpha) = a / b;
        float sideALength = Mathf.Sin(Mathf.Deg2Rad * alphaAngle) * sideBLength;
        _sideALength = sideALength;

        Debug.Log($"Alpha = {alphaAngle}, Beta = {betaAngle}, Game = {gamaAngle}, a = {sideALength}, b = {sideBLength}, c = {lineLength}");

        if(alphaAngle > 90)
        {
            return lineStartPoint;
        }else if (sideCLength > lineLength)
        {
            return lineEndPoint;
        }
        else
        {
            return (lineStartPoint + new Vector3(0, 0, sideCLength));
        }
    }

    // Gizmos methods
    private void OnDrawGizmosSelected()
    {
        //displayGizmosLine(_startPoint, _endPoint, Color.blue);
        if (lineLength > 0)
        {
            drawTheBaseLine(Color.blue, gizmosSphereRadius);
            drawLineFromThePointToStartPoint(Color.yellow, gizmosSphereRadius);
            drawTheClosestPoint(Color.red, gizmosSphereRadius);
            drawLineFromStartToClosestPoint(_startPoint, _closestPointOnLine, Color.green);

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
        _startPoint = new Vector3(0,0,0);
        _endPoint = _startPoint + (new Vector3(0, 0, 1) * lineLength);
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
        sidesLengthUI.text = $"Sides length \n a = {_sideALength}\n b = {_sideBLength}\n c = {_sideCLength}\n";
    }
}
