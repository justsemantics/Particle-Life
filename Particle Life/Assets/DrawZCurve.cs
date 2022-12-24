using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawZCurve : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float resolution;

    // Start is called before the first frame update
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetPoints(Agent[] agents, float z)
    {
        Vector3[] points = new Vector3[agents.Length];

        for(int i = 0; i < agents.Length; i++)
        {
            Vector2 position = agents[i].position / resolution;
            points[i] = new Vector3(position.x, z, position.y);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public void SetPoints(Vector2[] points, float z)
    {
        Vector3[] vector3Points = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 position = points[i] / resolution;
            vector3Points[i] = new Vector3(position.x, z, position.y);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(vector3Points);
    }

}
