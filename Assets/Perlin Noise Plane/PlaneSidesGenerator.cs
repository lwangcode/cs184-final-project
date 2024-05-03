using UnityEngine;

public class PlaneSidesGenerator : MonoBehaviour
{
    public Vector3[] edgeVertices; // List of vertices defining the edge of the plane

    int counter = 0;
    void Start()
    {

        //GenerateSidesMesh();
    }

    void Update()
    {
        if (counter == 1)
        {
            GenerateSidesMesh();
        }
        counter++;
    }

    void GenerateSidesMesh()
    {
        if (plane_generator.vertex_nums == null || plane_generator.vertex_nums.Length < 4)
        {
            Debug.LogError("Not enough edge vertices provided.");
            return;
        }

        // Copy the edge vertices from plane_generator
        edgeVertices = new Vector3[plane_generator.vertex_nums.Length];
        for (int i = 0; i < plane_generator.vertex_nums.Length; i++)
        {
            edgeVertices[i] = plane_generator.vertices[plane_generator.vertex_nums[i]];
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] vertices = new Vector3[edgeVertices.Length * 2]; // Double the vertices for sides
        int[] triangles = new int[(edgeVertices.Length - 1) * 6 + 6]; // Each segment has 2 triangles (6 vertices)

        // Create vertices for the sides
        for (int i = 0; i < edgeVertices.Length; i++)
        {
            vertices[i] = edgeVertices[i]; // Top vertices
            vertices[i + edgeVertices.Length] = new Vector3(edgeVertices[i].x, -1f, edgeVertices[i].z); // Bottom vertices
        }

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < edgeVertices.Length - 1; i++)
        {
            int topLeft = vertIndex;
            int topRight = vertIndex + 1;
            int bottomLeft = vertIndex + edgeVertices.Length;
            int bottomRight = vertIndex + edgeVertices.Length + 1;
            if (false)  // (i > edgeVertices.Length / 4 || i < 3 * edgeVertices.Length / 4)
            {
                triangles[triIndex] = topLeft;
                triangles[triIndex + 1] = bottomLeft;
                triangles[triIndex + 2] = topRight;
                triangles[triIndex + 3] = topRight;
                triangles[triIndex + 4] = bottomLeft;
                triangles[triIndex + 5] = bottomRight;
            } else
            {
                triangles[triIndex] = topLeft;
                triangles[triIndex + 1] = topRight;  // Reversed
                triangles[triIndex + 2] = bottomLeft;  // Reversed
                triangles[triIndex + 3] = bottomRight;
                triangles[triIndex + 4] = bottomLeft;  // Reversed
                triangles[triIndex + 5] = topRight;  // Reversed
            }

            triIndex += 6;
            vertIndex++;
        }
        triangles[triIndex] = 0 +edgeVertices.Length;
        triangles[triIndex + 1] = plane_generator.vertex_nums.Length/4 + edgeVertices.Length;
        triangles[triIndex + 2] = 2 * plane_generator.vertex_nums.Length / 4 + edgeVertices.Length;
        triangles[triIndex + 3] = 0;// plane_generator.vertex_nums.Length / 4 + edgeVertices.Length;
        triangles[triIndex + 4] = 1;// 2 * plane_generator.vertex_nums.Length / 4 + edgeVertices.Length;
        triangles[triIndex + 5] = 2;// 3 * plane_generator.vertex_nums.Length / 4 + edgeVertices.Length;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
