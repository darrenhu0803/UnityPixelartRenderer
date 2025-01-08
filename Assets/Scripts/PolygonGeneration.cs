using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PolygonGeneration: MonoBehaviour {

    public Material material;

    private List<Polygon> polygons;

    private class Polygon {
        public List<Vector2> vertices;
        public Vector2 offset;

        public Polygon(List<Vector2> verties, Vector2 offset) {
            this.vertices = verties;
            this.offset = offset;
        }
        public Polygon() {
            this.vertices = new List<Vector2>();
            this.offset = new Vector2();
        }

        public void SortByAngle() {
            // Calculate centroid (center of mass)
            float cx = vertices.Average(p => p.x);
            float cy = vertices.Average(p => p.y);

            // Define a function to calculate the angle to each point
            float Angle(Vector2 p) {
                return Mathf.Atan2(p.y - cy, p.x - cx);
            }

            // Sort points by the calculated angle
            vertices = vertices.OrderBy(Angle).ToList();
        }

    }

    private class Fall: MonoBehaviour {

        public Vector2 speed = Vector2.zero;

        private Vector2 center = Vector3.zero;
        private Vector3 pos = Vector3.zero;
        private float rotation = 0f;
        private float downSpeed = 0.01f;

        public List<Vector2> vertices;

        public void CalculatePolygonCenter() {
            if (vertices.Count == 0) {
                Debug.LogWarning("Polygon vertices list is empty.");
                return;
            }

            // Calculate the centroid
            Vector2 sum = Vector2.zero;
            foreach (var vertex in vertices) {
                sum += vertex;
            }
            center = sum / vertices.Count;
            speed = new Vector2((center.x - pos.x) / 150f + Random.Range(-0.04f, 0.04f), (center.y - pos.y) / 150f + Random.Range(-0.04f, 0.04f));
            rotation = -8f * Mathf.Clamp((center.x - pos.x) / Mathf.Abs(center.y - pos.y), -1f, 1f) + Random.Range(-3f, 3f);
        }

        public void Update() {
            Vector3 offset = new Vector3(center.x, center.y, 0);
            transform.RotateAround(pos + offset, Vector3.forward, rotation * Time.deltaTime);
            transform.position += new Vector3(speed.x, speed.y, 0) * Time.deltaTime * 60f;
            transform.position -= new Vector3(0, downSpeed, 0);
            pos += new Vector3(speed.x, speed.y, 0) * Time.deltaTime * 60f;
            pos -= new Vector3(0, downSpeed, 0);
            //speed = Vector2.Lerp(speed, Vector2.zero, Time.deltaTime * 0.5f);
            //rotation = Mathf.Lerp(rotation, 0, Time.deltaTime * 0.25f);
            speed /= 1 + Time.deltaTime * 0.2f;
            rotation /= 1 + Time.deltaTime * 0.01f;


        }
    }


    void Start() {

        Refresh();

    }


    public void Cut(Vector2 point, float angle, Vector2 off1, Vector2 off2) {
        Cut(Line(point, angle), off1, off2);
    }


    public void Cut(Vector2[] line, Vector2 off1, Vector2 off2) {
        List<Polygon> cuttedPolygons = new List<Polygon>();
        foreach (Polygon polygon in polygons) {
            (Polygon polygon1, Polygon polygon2) = SplitPolygon(polygon, line);

            polygon1.offset = new Vector2(polygon1.offset.x * 0.1f * 0.9f + off1.x * 0.1f, polygon1.offset.y * 0.1f * 0.9f + off1.y * 0.1f);
            polygon2.offset = new Vector2(polygon2.offset.x * 0.1f * 0.9f + off2.x * 0.1f, polygon2.offset.y * 0.1f * 0.9f + off2.y * 0.1f);

            if (polygon1.vertices.Count >= 3) {
                cuttedPolygons.Add(polygon1);
            }
            if (polygon2.vertices.Count >= 3) {
                cuttedPolygons.Add(polygon2);
            }
        }
        polygons = new List<Polygon>(cuttedPolygons);
    }

    public void RegenerateChildren() {
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }
        foreach (Polygon polygon in polygons) {
            if (polygon.vertices.Count >= 3) {
                polygon.SortByAngle();
                GenerateChild(polygon);
            } else {
                Debug.LogError("Polygon must have at least 3 vertices.");
            }
        }
    }

    public void Refresh() {
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }
        Polygon p = new Polygon(new List<Vector2>() {
            new Vector2(12, 7),
            new Vector2(12, -7),
            new Vector2(-12, -7),
            new Vector2(-12, 7)
        }, new Vector2(0f, 0f));

        p.SortByAngle();

        polygons = new List<Polygon>() { p };
    }

    private Vector2[] Line(Vector2 point, float angle) {
        float length = 40;
        float x1 = point.x - length * Mathf.Cos(Mathf.Deg2Rad * angle);
        float y1 = point.y - length * Mathf.Sin(Mathf.Deg2Rad * angle);
        float x2 = point.x + length * Mathf.Cos(Mathf.Deg2Rad * angle);
        float y2 = point.y + length * Mathf.Sin(Mathf.Deg2Rad * angle);
        return new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2) };
    }


    private bool IsLeft(Vector2 point, Vector2[] line) {
        return (line[1].x - line[0].x) * (point.y - line[0].y) > (line[1].y - line[0].y) * (point.x - line[0].x);
    }

    private Vector2 Intersection(Vector2 point1, Vector2 point2, Vector2[] line) {
        float A1 = point2.y - point1.y;
        float B1 = point1.x - point2.x;
        float C1 = A1 * point1.x + B1 * point1.y;

        float A2 = line[1].y - line[0].y;
        float B2 = line[0].x - line[1].x;
        float C2 = A2 * line[0].x + B2 * line[0].y;

        float det = A1 * B2 - A2 * B1;
        if (det == 0) {
            return new Vector2(99999, 99999);
        }

        return new Vector2((B2 * C1 - B1 * C2) / det, (A1 * C2 - A2 * C1) / det);

    }

    private (Polygon, Polygon) SplitPolygon(Polygon polygon, Vector2[] line) {
        Polygon polygon1 = new Polygon();
        Polygon polygon2 = new Polygon();

        polygon1.offset = polygon.offset;
        polygon2.offset = polygon.offset;

        for (int i = 0; i < polygon.vertices.Count; i++) {
            Vector2 P = polygon.vertices[i];
            Vector2 Q = polygon.vertices[(i + 1) % polygon.vertices.Count];

            if (IsLeft(P, line)) {
                polygon1.vertices.Add(P);
            } else {
                polygon2.vertices.Add(P);
            }

            if (IsLeft(P, line) != IsLeft(Q, line)) {
                Vector2 intersect = Intersection(P, Q, line);
                if (!Vector2.Equals(intersect, new Vector2(99999, 99999))) {
                    polygon1.vertices.Add(intersect);
                    polygon2.vertices.Add(intersect);
                }
            }
        }

        return (polygon1, polygon2);
    }


    private void GenerateChild(Polygon polygon) {
        //TODO 
        //big + medium + small instead of random
        if (Random.Range(0f, 1f) > 0.5f) {
            return;
        }

        GameObject polygonObject = new GameObject("Polygon");
        polygonObject.layer = LayerMask.NameToLayer("Distortion");
        polygonObject.transform.parent = this.transform;
        polygonObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = polygonObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = polygonObject.AddComponent<MeshRenderer>();

        Fall fall = polygonObject.AddComponent < Fall > ();

        fall.vertices = polygon.vertices;
        fall.CalculatePolygonCenter();

        GenerateMesh(meshFilter, polygon);

        Material _material = Material.Instantiate(material);
        _material.SetFloat("_Alpha", 0.2f);
        _material.SetFloat("_Power", 0.3f);
        //_material.SetVector("_Offset", polygon.offset);

        _material.SetVector("_Offset", new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)));
        meshRenderer.material = _material;


    }

    void GenerateMesh(MeshFilter meshFilter, Polygon polygon) {


        Mesh mesh = new Mesh();
        int[] triangles = new int[(polygon.vertices.Count - 2) * 3];

        for (int i = 0; i < polygon.vertices.Count - 2; i++) {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.Clear();
        mesh.vertices = polygon.vertices.Select(v => new Vector3(v.x, v.y, 0)).ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
}
