using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plane_generator_sriram : MonoBehaviour
{
    static public GameObject[] createdObjects;

    [Header("Efficiency settings:")]
    public bool update = false; public int instances;
    public Mesh mesh2;
    public Material[] materials;
    private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
    private List<Vector3> vectorList = new List<Vector3>();
    private List<bool> vectorList_toggle = new List<bool>();


    [Header("3D Perlin Noise Settings")]
    public int width = 64;
    public int height = 64;
    public int depth = 64;
    public float planeSize = 16f;
    public float x_off = 0f;
    public float y_off = 0f;
    public float z_off = 0f;
    [Range(0.0f, 1.0f)]
    public float frequency = 1f;
    [Range(0.0f, 1.0f)]
    public float amplitude = 1f;

    public GameObject gameObj;

    private void RenderBatches()

    {
        int counter = 0;
        foreach (var batch in batches)
        {
            for (int i = 0; i < mesh2.subMeshCount; i++)
            {
                //Debug.Log("reached");
                Graphics.DrawMeshInstanced(mesh2, i, materials[i], batch);

                /*if (vectorList_toggle[counter])
                {
                    Graphics.DrawMeshInstanced(mesh2, i, materials[i], batch);
                }*/
                counter++;
            }
        }
    }

    private void Start()
    {
        PerlinUpdate();
        DrawPerlin();
        

        // create instances
        int add_matrices = 0;
        batches.Add(new List<Matrix4x4>());

        for (int i = 0; i < vectorList.Count; i++)
        {
            if (vectorList_toggle[i])
            {
                if (add_matrices < 1000)
                {
                    batches[batches.Count - 1].Add(item:
                                        Matrix4x4.TRS(
                                                pos: vectorList[i],
                                                q: Quaternion.Euler(0.0f, 0.0f, 0.0f),
                                                s: new Vector3(x: 1.0f, y: 1.0f, z: 1.0f))
                                            );
                    add_matrices++;
                }
                else
                {
                    batches.Add(new List<Matrix4x4>());
                    add_matrices = 0;
                }
            }
        }
    }

    private void Update()
    {
        RenderBatches();
        if (update)
        {
            // PerlinUpdate();
        }
    }

    private void DrawPerlin()
    {
        //createdObjects = new GameObject[width * height * depth];
        int count = 0;

        for (float x = -1f * (float)width / 2f; x < (float)width / 2f; x += 1f)
        {
            for (float y = -1f * (float)height / 2f; y < (float)height / 2f; y += 1f)
            {
                for (float z = -1f * (float)depth / 2f; z < (float)depth / 2f; z += 1f)
                {
                    vectorList.Add(new Vector3(x, y, z));
                    vectorList_toggle.Add(Perlin3D(x, y, z) >= .5f);
                    /*createdObjects[count] = Instantiate(gameObj, new Vector3(x, y, z), Quaternion.identity);
                    if (Perlin3D(x, y, z) >= .5f) {
                        createdObjects[count].SetActive(true);
                    } else {
                        createdObjects[count].SetActive(false);
                    }*/
                    count++;
                }
            }
        }
    }

    private void PerlinUpdate()
    {
        for (int i = 0; i < vectorList.Count; i++)
        {
            Vector3 pos = vectorList[i];
            vectorList_toggle[i] = (Perlin3D(pos.x, pos.y, pos.z) >= .5f);
        }
    }

    private float Perlin3D(float x, float y, float z)
    {
        float a, b, c, d, e, f;
        a = Mathf.PerlinNoise((x + x_off) * frequency, (y + y_off) * frequency) / amplitude;
        b = Mathf.PerlinNoise((y + y_off) * frequency, (z + z_off) * frequency) / amplitude;
        c = Mathf.PerlinNoise((z + z_off) * frequency, (x + x_off) * frequency) / amplitude;
        d = Mathf.PerlinNoise((x + x_off) * frequency, (z + z_off) * frequency) / amplitude;
        e = Mathf.PerlinNoise((y + y_off) * frequency, (x + x_off) * frequency) / amplitude;
        f = Mathf.PerlinNoise((z + z_off) * frequency, (y + y_off) * frequency) / amplitude;

        return (a + b + c + d + e + f) / 6f;
    }
}