using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindClosestPointOnLineToPoint : MonoBehaviour
{
    // Gizmos variables
    public float lineLength = 10;
    public Vector3 thePoint;

    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // Calculated variables
    private Vector3 _fromThePointToStartPoint;
    private float _sideCLength;
    private float _sideALength;

    private void findClosestPontOnLineToPoint()
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
        Vector3 line = _endPoint - _startPoint;
        float lineLength = line.magnitude;
        if (lineLength < 0)
        {
            lineLength *= -1;
        }

        // Side b
        Vector3 sideB = thePoint - _startPoint;
        float sideBLength = sideB.magnitude;

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
    }

    // Gizmos methods
    private void OnDrawGizmosSelected()
    {
        //displayGizmosLine(_startPoint, _endPoint, Color.blue);
        if (lineLength > 0)
        {
            drawTheBaseLine(Color.green);
            drawLineFromThePointToStartPoint(Color.yellow);
            drawTheClosestPoint(Color.red);
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

    private void drawTheBaseLine(Color color)
    {
        _startPoint = this.transform.position;
        _endPoint = _startPoint + (new Vector3(0, 0, 1) * lineLength);
        drawGizmosLine(_startPoint, _endPoint, color);
        drawGizmoSphere(_startPoint, 0.2f, color);
        drawGizmoSphere(_endPoint, 0.2f, color);
    }
    private void drawLineFromThePointToStartPoint(Color color)
    {
        drawGizmoSphere(thePoint, 0.2f, color);
        drawGizmosLine(thePoint, _startPoint, color);
    }
    private void drawTheClosestPoint(Color color)
    {
        findClosestPontOnLineToPoint();

        Vector3 toLine = thePoint + new Vector3(_sideALength, 0, 0); 
        drawGizmosLine(thePoint, toLine, color);
        
        Vector3 pointPos = _startPoint + new Vector3(0, 0, _sideCLength);
        drawGizmoSphere(pointPos, 0.1f, color);
    }
}
