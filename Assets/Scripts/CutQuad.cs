using UnityEngine;
using System.Collections.Generic;

public class QuadCutter: MonoBehaviour {
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        GenerateQuad();
    }

    void Update() {
        if (Input.GetMouseButtonDown(1))  // Right-click
        {
            CutQuad();
        }
    }

    void GenerateQuad() {
        vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        UpdateMesh();
    }

    void CutQuad() {
        Vector2 randomPoint1 = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        float randomAngle = Random.Range(0f, 360f);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));

        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<int> newTriangles = new List<int>();

        Vector3 cutPoint1 = randomPoint1 - direction * 0.7f;
        Vector3 cutPoint2 = randomPoint1 + direction * 0.7f;

        newVertices.Add(cutPoint1);
        newVertices.Add(cutPoint2);

        newTriangles.AddRange(new int[] { 0, 4, 2 });
        newTriangles.AddRange(new int[] { 4, 5, 2 });
        newTriangles.AddRange(new int[] { 5, 1, 3 });
        newTriangles.AddRange(new int[] { 5, 3, 2 });

        vertices = newVertices.ToArray();
        triangles = newTriangles.ToArray();

        UpdateMesh();
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
