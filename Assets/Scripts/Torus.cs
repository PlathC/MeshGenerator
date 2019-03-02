using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torus : MonoBehaviour, IDynamicMesh {

    // For help :https://gamedev.stackexchange.com/questions/16845/how-do-i-generate-a-torus-mesh

    public float firstCircleDiameter = 14f;
    public float secondCircleDiameter = 5.5f;
    public int nbPointsPerCircle = 80;
    public Vector3 center = new Vector3(0, 0, 50);
    public Material meshMaterial = null;
    public Color color = new Color(0, 0, 0);

    private Dictionary<float, Dictionary<float, int>> pointsDictionnary = new Dictionary<float, Dictionary<float, int>>();
    private List<float> us = new List<float>();
    private List<float> vs = new List<float>();
    private int pointsIncrements = 0;
    private bool completeVs = false;

    // Use this for initialization
    void Start()
    {
        PrintMesh();
    }

    public void OnValidate()
    {
        // Generate the mesh for each change of the Editor
        PrintMesh();
    }

    /// <summary>
    /// Return first circle axes
    /// </summary>
    /// <param name="u">The current angle of the first circle</param>
    /// <returns>A vector3 that contains the W axes.</returns>
    private Vector3 GetW(float u)
    {
        return (new Vector3(Mathf.Cos(u), Mathf.Sin(u), 0));
    }

    /// <summary>
    /// Return a points of the second circle
    /// using the first circle axe.
    /// </summary>
    /// <param name="u">First circle angle</param>
    /// <param name="v">Second circle angle</param>
    /// <returns>A Vector3 according to u and v</returns>
    private Vector3 GetQ(float u, float v)
    {
        // Saving points to create mesh.
        if (!pointsDictionnary.ContainsKey(u))
            pointsDictionnary[u] = new Dictionary<float, int>();
        pointsDictionnary[u][v] = pointsIncrements++;

        Vector3 res = center + (firstCircleDiameter * GetW(u)) + (secondCircleDiameter * Mathf.Cos(v) * GetW(u)) + new Vector3(0, 0, secondCircleDiameter * Mathf.Sin(v));
        return res;
    }

    public Mesh GenerateMesh()
    {
        Mesh finalMesh = new Mesh();
        us.Clear();
        vs.Clear();
        completeVs = false;
        List<Vector3> meshPoints = new List<Vector3>();
        List<int> triangles = new List<int>();
        int nbPoints = nbPointsPerCircle;
        float angle = 0;
        pointsDictionnary.Clear();
        pointsIncrements = 0;
        float angleSteps = ((2 * Mathf.PI) / nbPoints);
        Debug.Assert(nbPoints > 0, "Le nombre de points doit être supérieur à 0.");

        for (int i = 0; i < nbPoints; i++)
        {
            us.Add(angle);
            float secondAngle = 0;
            for (int j = 0; j < nbPoints; j++)
            {
                if (!completeVs)
                    vs.Add(secondAngle);
                meshPoints.Add(GetQ(angle, secondAngle));

                secondAngle += angleSteps;
            }
            completeVs = true;
            angle += angleSteps;

        }

        for (int i = 0; i < us.Count; i++)
        {
            for (int j = 0; j < vs.Count; j++)
            {

                // ---------------------- First triangle

                // (u, v)
                triangles.Add(pointsDictionnary[us[i]][vs[j]]);

                // (u, v+1)
                if (j == vs.Count - 1)
                {

                    triangles.Add(pointsDictionnary[us[i]][vs[0]]);

                }
                else
                {
                    triangles.Add(pointsDictionnary[us[i]][vs[j + 1]]);
                }

                // (u - 1, v)
                if (i == 0)
                {
                    triangles.Add(pointsDictionnary[us[us.Count - 1]][vs[j]]);
                }
                else
                {
                    triangles.Add(pointsDictionnary[us[i - 1]][vs[j]]);
                }

                // ---------------------- Second triangle

                // (u-1, v)
                if (i == 0)
                {
                    triangles.Add(pointsDictionnary[us[us.Count - 1]][vs[j]]);
                }
                else
                {
                    triangles.Add(pointsDictionnary[us[i - 1]][vs[j]]);
                }

                // (u, v+1)
                if (j == vs.Count - 1)
                {
                    triangles.Add(pointsDictionnary[us[i]][vs[0]]);
                }
                else
                {
                    triangles.Add(pointsDictionnary[us[i]][vs[j + 1]]);
                }

                // (u-1, v+1)
                float uValue;
                float vValue;
                if (i == 0)
                {
                    uValue = us[us.Count - 1];
                }
                else
                {
                    uValue = us[i - 1];
                }

                if (j == vs.Count - 1)
                {
                    vValue = vs[0];
                }
                else
                {
                    vValue = vs[j + 1];
                }

                triangles.Add(pointsDictionnary[uValue][vValue]);

            }
        }

        Vector2[] uvs = new Vector2[finalMesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(finalMesh.vertices[i].x, finalMesh.vertices[i].z);
        }

        finalMesh.vertices = meshPoints.ToArray();
        finalMesh.uv = uvs;
        finalMesh.MarkDynamic();
        finalMesh.triangles = triangles.ToArray();
        finalMesh.RecalculateBounds();
        finalMesh.RecalculateNormals();
        return finalMesh;
    }

    /// <summary>
    /// Allow to separate the creation of the mesh and its assignation
    /// to the object.
    /// </summary>
    public void PrintMesh()
    {

        Mesh finalMesh = GenerateMesh();

        if (this.gameObject.GetComponent<MeshRenderer>() == null)
            this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;

        Material material;
        if (meshMaterial)
            material = meshMaterial;
        else
        {
            material = new Material(Shader.Find("Standard"));
        }
        material.color = this.color;
        this.gameObject.GetComponent<MeshRenderer>().material = material;

        if (this.gameObject.GetComponent<MeshFilter>() == null)
            this.gameObject.AddComponent<MeshFilter>();
        this.gameObject.GetComponent<MeshFilter>().mesh = finalMesh;
    }
}
