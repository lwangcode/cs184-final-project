using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typof(MeshFilter))]
public class plane_generator_cubes : MonoBehaviour
{
    Mesh mesh;
    private List<Vector3> vertices;
    Vector3[] mesh_vertices;
    private List<Triangle> triangles;
    int[] mesh_triangles;
    Vector2[] mesh_uvs;

    [Header("Efficiency settings:")]
    public bool realtime_update = false; // Adjust this for depth/thickness
    [Header("Mesh Settings")]
    public int gridSize = 64; // Change this to set the number of grid cells
    public float planeSize = 16f;
    public float thickness = 1.0f; // Adjust this for depth/thickness
    //public Gradient gradient;
    [Range(0.0f, 10.0f)]
    public float lowerbound = 0.5f;
    [Range(0.0f, 10.0f)]
    public float upperbound = 2f;

    [Header("Perlin Noise Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency = 0.5f;
    [Range(0.1f, 10.0f)]
    public float heightScale = 2f;
    public float p_offset_x = 8f;
    public float p_offset_z = 8f;
    [Range(0.0f, 1.0f)]
    public float scroll_speed = 0.0f;


    [Header("Perlin Noise 2 Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency2 = 0.5f;
    [Range(0.1f, 10.0f)]
    public float heightScale2 = 2f;
    public float p_offset_x2 = 8f;
    public float p_offset_z2 = 8f;

    Color[] colors;

    private Tables tables;
    Vector3[] Grid;

    float isoLevel = 0.5f;

    Texture2D texture;

    public Gradient worldColor;

    struct Triangle
    {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //CreatePlane();
        // SetPerlin();

        tables = new Tables();
        InitGrid();
        MarchCubes();
        SetColors();
    }

    void InitGrid()
    {
        Grid = new Vector3[gridSize * gridSize * gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    float xPos = (float)x / gridSize * planeSize - planeSize / 2.0f;
                    float zPos = (float)z / gridSize * planeSize - planeSize / 2.0f;
                    float yPos = (float)y / gridSize * planeSize - planeSize / 2.0f;
                    Grid[x * gridSize * gridSize + y * gridSize + z] = new Vector3(xPos, yPos, zPos);
                }
            }
        }
    }

    float Noise3D(float x, float y, float z, float frequency = 0.1f, float amplitude = 10, float persistence = 0.1f, int octave = 1, int seed = 11)
    {
        // float noise = 0.0f;

        // for (int i = 0; i < octave; ++i)
        // {
        // 	// Get all permutations of noise for each individual axis
        // 	float noiseXY = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed) * amplitude;
        // 	float noiseXZ = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;
        // 	float noiseYZ = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed) * amplitude;

        // 	// Reverse of the permutations of noise for each individual axis
        // 	float noiseYX = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed) * amplitude;
        // 	float noiseZX = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed) * amplitude;
        // 	float noiseZY = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed) * amplitude;

        // 	// Use the average of the noise functions
        // 	noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;

        // 	amplitude *= persistence;
        // 	frequency *= 2.0f;
        // }

        // // Use the average of all octaves
        // return noise / octave;

        int x_off = 100;
        int y_off = 3493;
        int z_off = -23423;

        float xy = Mathf.PerlinNoise((x + x_off) * frequency , (y + y_off) * frequency);
        float xz = Mathf.PerlinNoise((x + x_off) * frequency, (z + z_off) * frequency);
        float yz = Mathf.PerlinNoise((y + y_off) * frequency , (z + z_off) * frequency);
        float yx = Mathf.PerlinNoise((y + y_off) * frequency, (x + x_off) * frequency);
        float zx = Mathf.PerlinNoise((z + z_off) * frequency , (x + x_off) * frequency);
        float zy = Mathf.PerlinNoise((z + z_off) * frequency, (y + y_off) * frequency);

        return (xy + xz + yz + yx + zx + zy) / 6.0f;
    }

    void MarchCubes()
    {
        Debug.Log("Marching cubes");

        triangles = new List<plane_generator_cubes.Triangle>();
        vertices = new List<Vector3>();

        // int triIndex = 0;

        for (int x = 0; x < gridSize - 1; x++) // TODO: this is hacky, fix
        {
            for (int y = 0; y < gridSize - 1; y++)
            {
                for (int z = 0; z < gridSize - 1; z++)
                {
                    List<plane_generator_cubes.Triangle> tris = MarchCube(x, y, z);
                    // add tris to triangles
                    if (tris != null)
                    {
                        foreach (Triangle tri in tris)
                        {
                            triangles.Add(tri);
                        }
                    }
                }
            }
        }
        ProcessTriangles();
    }

    List<plane_generator_cubes.Triangle> MarchCube(int x, int y, int z)
    {
        int cubeIndex = 0;
        Vector3[] cubeCorners = new Vector3[8];
        float[] cubeValues = new float[8];

        double[,] cubeCoords = tables.cubeCoords;

        for (int i = 0; i < 8; i++)
        {
            cubeCorners[i] = Grid[(x + (int)cubeCoords[i, 0]) * gridSize * gridSize + (y + (int)cubeCoords[i, 1]) * gridSize + z + (int)cubeCoords[i, 2]];
        }

        // cubeCorners[0] = Grid[x * gridSize * gridSize + y * gridSize + z]; // 0, 0, 0
        // cubeCorners[1] = Grid[x * gridSize * gridSize + y * gridSize + z + 1]; // 0, 0, 1
        // cubeCorners[2] = Grid[(x + 1) * gridSize * gridSize + y * gridSize + z + 1]; // 1, 0, 1
        // cubeCorners[3] = Grid[(x + 1) * gridSize * gridSize + y * gridSize + z]; // 1, 0, 0
        // cubeCorners[4] = Grid[x * gridSize * gridSize + (y + 1) * gridSize + z]; // 0, 1, 0
        // cubeCorners[5] = Grid[x * gridSize * gridSize + (y + 1) * gridSize + z + 1]; // 0, 1, 1
        // cubeCorners[6] = Grid[(x + 1) * gridSize * gridSize + (y + 1) * gridSize + z + 1]; // 1, 1, 1
        // cubeCorners[7] = Grid[(x + 1) * gridSize * gridSize + (y + 1) * gridSize + z]; // 1, 1, 0

        for (int i = 0; i < 8; i++)
        {
            float p = planeSize / 2 - 1;
            if (Mathf.Abs(cubeCorners[i].x) > p - 1 || Mathf.Abs(cubeCorners[i].z) > p - 1 || Mathf.Abs(cubeCorners[i].y) > p-1)
            {
                //cubeValues[i] = Noise3D(cubeCorners[i].x, cubeCorners[i].y, cubeCorners[i].z);
                cubeValues[i] = 0f;
            }
            else
            {
                cubeValues[i] = Noise3D(cubeCorners[i].x, cubeCorners[i].y, cubeCorners[i].z);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            if (cubeValues[i] < isoLevel) cubeIndex |= 1 << i;
        }

        // if (cubeValues[0] < isoLevel) cubeIndex |= 1;
        // if (cubeValues[1] < isoLevel) cubeIndex |= 2;
        // if (cubeValues[2] < isoLevel) cubeIndex |= 4;
        // if (cubeValues[3] < isoLevel) cubeIndex |= 8;
        // if (cubeValues[4] < isoLevel) cubeIndex |= 16;
        // if (cubeValues[5] < isoLevel) cubeIndex |= 32;
        // if (cubeValues[6] < isoLevel) cubeIndex |= 64;
        // if (cubeValues[7] < isoLevel) cubeIndex |= 128;



        int edgeIndex = tables.getFromEdgeTable(cubeIndex);
        if (edgeIndex == 0) return new List<plane_generator_cubes.Triangle>();

        Vector3[] edgeVertices = new Vector3[12];

        // 0, 0, 0 + 0, 0, 1
        if ((edgeIndex & 1) == 1) edgeVertices[0] = VertexInterp(cubeCorners[0], cubeValues[0], cubeCorners[1], cubeValues[1]);
        // 0, 0, 1 + 1, 0, 1
        if ((edgeIndex & 2) == 2) edgeVertices[1] = VertexInterp(cubeCorners[1], cubeValues[1], cubeCorners[2], cubeValues[2]);
        // 1, 0, 1 + 1, 0, 0
        if ((edgeIndex & 4) == 4) edgeVertices[2] = VertexInterp(cubeCorners[2], cubeValues[2], cubeCorners[3], cubeValues[3]);
        // 1, 0, 0 + 0, 0, 0
        if ((edgeIndex & 8) == 8) edgeVertices[3] = VertexInterp(cubeCorners[3], cubeValues[3], cubeCorners[0], cubeValues[0]);
        // 0, 1, 0 + 0, 1, 1
        if ((edgeIndex & 16) == 16) edgeVertices[4] = VertexInterp(cubeCorners[4], cubeValues[4], cubeCorners[5], cubeValues[5]);
        // 0, 1, 1 + 1, 1, 1
        if ((edgeIndex & 32) == 32) edgeVertices[5] = VertexInterp(cubeCorners[5], cubeValues[5], cubeCorners[6], cubeValues[6]);
        // 1, 1, 1 + 1, 1, 0
        if ((edgeIndex & 64) == 64) edgeVertices[6] = VertexInterp(cubeCorners[6], cubeValues[6], cubeCorners[7], cubeValues[7]);
        // 1, 1, 0 + 0, 1, 0
        if ((edgeIndex & 128) == 128) edgeVertices[7] = VertexInterp(cubeCorners[7], cubeValues[7], cubeCorners[4], cubeValues[4]);
        // 0, 0, 0 + 0, 1, 0
        if ((edgeIndex & 256) == 256) edgeVertices[8] = VertexInterp(cubeCorners[0], cubeValues[0], cubeCorners[4], cubeValues[4]);
        // 0, 0, 1 + 0, 1, 1
        if ((edgeIndex & 512) == 512) edgeVertices[9] = VertexInterp(cubeCorners[1], cubeValues[1], cubeCorners[5], cubeValues[5]);
        // 1, 0, 1 + 1, 1, 1
        if ((edgeIndex & 1024) == 1024) edgeVertices[10] = VertexInterp(cubeCorners[2], cubeValues[2], cubeCorners[6], cubeValues[6]);
        // 1, 0, 0 + 1, 1, 0
        if ((edgeIndex & 2048) == 2048) edgeVertices[11] = VertexInterp(cubeCorners[3], cubeValues[3], cubeCorners[7], cubeValues[7]);

        List<plane_generator_cubes.Triangle> tris = new List<plane_generator_cubes.Triangle>();

        for (int i = 0; tables.getFromTriTable(cubeIndex, i) != -1; i += 3)
        {
            tris.Add(new Triangle
            {
                v1 = edgeVertices[tables.getFromTriTable(cubeIndex, i)],
                v2 = edgeVertices[tables.getFromTriTable(cubeIndex, i + 1)],
                v3 = edgeVertices[tables.getFromTriTable(cubeIndex, i + 2)]
            });
        }

        return tris;
    }

    // implement interpolate
    Vector3 VertexInterp(Vector3 p1, float v1, Vector3 p2, float v2)
    {
        if (Mathf.Abs((float)isoLevel - v1) < 0.0001) return p1;
        if (Mathf.Abs((float)isoLevel - v2) < 0.0001) return p2;
        if (Mathf.Abs(v1 - v2) < 0.0001) return p1;

        float mu = ((float)isoLevel - v1) / (v2 - v1);
        return new Vector3(p1.x + mu * (p2.x - p1.x), p1.y + mu * (p2.y - p1.y), p1.z + mu * (p2.z - p1.z));
    }

    void ProcessTriangles()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        mesh_vertices = new Vector3[triangles.Count * 3];
        mesh_triangles = new int[triangles.Count * 3];
        mesh_uvs = new Vector2[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++)
        {

            mesh_vertices[i * 3].Set(triangles[i].v1.x, triangles[i].v1.y, triangles[i].v1.z);
            mesh_vertices[i * 3 + 1].Set(triangles[i].v2.x, triangles[i].v2.y, triangles[i].v2.z);
            mesh_vertices[i * 3 + 2].Set(triangles[i].v3.x, triangles[i].v3.y, triangles[i].v3.z);

            mesh_uvs[i * 3] = new Vector2(0, 0);
            mesh_uvs[i * 3 + 1] = new Vector2(0, 1);
            mesh_uvs[i * 3 + 2] = new Vector2(1, 0);

            // mesh_triangles[i * 3] = i * 3 + 2;
            // mesh_triangles[i * 3 + 1] = i * 3 + 1;
            // mesh_triangles[i * 3 + 2] = i * 3;

            mesh_triangles[i * 3] = i * 3;
            mesh_triangles[i * 3 + 1] = i * 3 + 1;
            mesh_triangles[i * 3 + 2] = i * 3 + 2;

        }


        // note mesh gets set in SetColor()
    }

    void Update()
    {
        if (realtime_update)
        {
            CreatePlane();
            MarchCubes();
            // p_offset_z += scroll_speed;
        }
        SetColors();
        transform.RotateAround(new Vector3(0,0,0), Vector3.up, 20 * Time.deltaTime);
    }

    void CreatePlane()
    {
        vertices = new List<Vector3>();
        triangles = new List<plane_generator_cubes.Triangle>();
    }

    void SetColors()
    {
        colors = new Color[triangles.Count * 3];
        for (int i = 0; i < mesh_vertices.Length; i++)
        {
            //float normalizedHeight = vertices[i].y / heightScale; // Normalize height to range [0, 1]
            float normalizedHeight = Mathf.InverseLerp(0f, 32f, mesh_vertices[i].y);

            colors[i] = worldColor.Evaluate(Mathf.Clamp(normalizedHeight, 0.0f, 1.0f));
            //colors[i] = new Color(mesh_vertices.Length / (i+1), mesh_vertices.Length / (i + 1), mesh_vertices.Length / (i + 1));
            //colors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
        //Debug.Log
        mesh.vertices = mesh_vertices;
        mesh.triangles = mesh_triangles;
        //mesh.uv = mesh_uvs;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}