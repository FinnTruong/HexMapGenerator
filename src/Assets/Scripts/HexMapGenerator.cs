using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab;

    [SerializeField] int mapWidth = 25;
    [SerializeField] int mapHeight = 12;

    public int seed;
    public float tileSize;
    [Range(0,1)]
    public float refinement;
    public float minTileHeight;
    public float heightMultiplier = 1f;
    [Range(0, 1)]
    public float outlinePercent = 0f;

    public float tileXOffset = 1.5f;
    public float tileZOffset = 1.725f;

    public float hexMaskRadius = 10f;
    private List<Vector3> hexVertices = new List<Vector3>();
    private List<GameObject> tileMap = new List<GameObject>();
    private List<GameObject> treeMap = new List<GameObject>();
    private Transform mapHolder;

    [Header("Terrain Variables")]
    public Material waterMat;
    public Material sandMat;
    public Material grassMat;
    public Material forestMat;
    public Material mountainMat;
    public Material snowMat;

    [Range(0,1)]
    public float waterThreshold = 0.4f;
    [Range(0,1)]
    public float sandThreshold = 1f;
    [Range(0,1)]
    public float grassThreshold = 2f;
    [Range(0,1)]
    public float foresetThreshold = 3f;
    [Range(0,1)]
    public float mountainThreshold = 5f;

    [Header("Water Surface")]

    public float waterMeshSize = 0.85f;
    public float waterDepth = 0;

    [Header("Trees")]
    public GameObject[] treesPrefab;
    [Range(0, 1)]
    public float treeMinThreshold;
    [Range(0, 1)]
    public float treeMaxThreshold = 1;
    public Vector2 treeScale;
    [Range(0,1)]
    public float treeDensity;
    public Material treeMat;

    public float rotateSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    private void Init()
    {
        tileMap.Clear();
        treeMap.Clear();
        GetHexMask();
        string holderName = "Generated Tile";
        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        mapHolder.transform.localPosition = Vector3.zero;
        mapHolder.gameObject.AddComponent<Rotate>().rotateSpeed = rotateSpeed;


    }
    public void GenerateMap()
    {
        Init();
        System.Random pseudoRNG = new System.Random(seed);
        GenerateWaterMesh();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                var tileCoord = CoordToPosition(z, x) + transform.position;
                if (IsInsidePolygon(tileCoord, hexVertices.ToArray()))
                {
                    //float randHeight = Mathf.Lerp(minTileHeight, maxTileHeight, (float)pseudoRNG.NextDouble());
                    float randHeight = Mathf.PerlinNoise((x + seed) * refinement, (z+seed) * refinement);
                    //Generate Tile
                    if (randHeight >= waterThreshold)
                    {
                        GameObject newTile = Instantiate(hexTilePrefab, mapHolder);
                        newTile.GetComponent<Renderer>().material = GetTerrainFromHeight(randHeight);
                        newTile.name = $"{x},{z}";
                        newTile.transform.localPosition = CoordToPosition(z, x) + Vector3.up * newTile.transform.localScale.z + Vector3.up * ((randHeight*heightMultiplier) - 1) * newTile.transform.localScale.z;
                        newTile.transform.localScale = new Vector3((1 - outlinePercent) * tileSize * newTile.transform.localScale.x,
                            (1 - outlinePercent) * tileSize * newTile.transform.localScale.y,
                            randHeight * heightMultiplier * newTile.transform.localScale.z);
                        tileMap.Add(newTile);


                        if (randHeight < treeDensity)
                        {
                            var treeIndex = Random.Range(0, treesPrefab.Length - 1);
                            GameObject tree = Instantiate(treesPrefab[treeIndex], mapHolder);
                            tree.name = $"Tree: {x},{z}";
                            tree.transform.localPosition = newTile.transform.localPosition + Vector3.up * newTile.transform.localScale.z;
                            tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                            tree.transform.localScale = Vector3.one * Random.Range(treeScale.x, treeScale.y) * Mathf.Abs(tileSize);
                            tree.GetComponent<Renderer>().sharedMaterial = treeMat;
                            treeMap.Add(tree);
                        }

                        //if (randHeight >= treeMinThreshold && randHeight <= treeMaxThreshold)
                        //{
                           
                        //}
                    }

                }
            }
        }
    }

    public Vector3 CoordToPosition(int x, int z)
    {
        var xOffset = -((mapWidth - 1) * tileXOffset) / 2f;
        var zOffset = -((mapHeight - 1) * tileZOffset) / 2f;

        if (z % 2 == 0)
        {
            return new Vector3(x * tileXOffset + xOffset, 0, z * tileZOffset + zOffset) * tileSize;
        }
        else
        {
            return new Vector3(x * tileXOffset + tileXOffset / 2 + xOffset, 0, z * tileZOffset + zOffset) * tileSize;
        }
    }
    private void GetHexMask()
    {
        hexVertices.Clear();
        for (int i = 0; i < 6; i++)
        {
            float angle_deg = 60 * i;
            float angle_rad = Mathf.PI / 180f * angle_deg;
            var point = new Vector3(hexMaskRadius  * Mathf.Cos(angle_rad), 0, hexMaskRadius  * Mathf.Sin(angle_rad)) + transform.position;
            hexVertices.Add(point);
        }
    }

    private bool IsInsidePolygon(Vector3 v, Vector3[] p)
    {
        int j = p.Length - 1;
        bool c = false;
        for (int i = 0; i < p.Length; j = i++)
        {
            c ^= p[i].z > v.z ^ p[j].z > v.z && v.x < (p[j].x - p[i].x) * (v.z - p[i].z) / (p[j].z - p[i].z) + p[i].x;
        }
        return c;
    }

    private void OnDrawGizmos()
    {
        GetHexMask();
        //Gizmos.DrawSphere(transform.position, circleMaskRadius);
        for (int i = 0; i < hexVertices.Count - 1; i++)
        {
            Gizmos.DrawLine(hexVertices[i], hexVertices[i + 1]);
        }
        Gizmos.DrawLine(hexVertices[0], hexVertices[hexVertices.Count - 1]);

    }

    private Material GetTerrainFromHeight(float height)
    {
        if (height <= waterThreshold)
            return waterMat;
        else if (height <= sandThreshold)
            return sandMat;
        else if (height <= grassThreshold)
            return grassMat;
        else if (height <= foresetThreshold)
            return forestMat;
        else if (height <= mountainThreshold)
            return mountainMat;
        else
            return snowMat;
    }

    #region Hex Generation
    private void GenerateWaterMesh()
    {
        DrawMesh();
    }

    private void DrawMesh()
    {
        List<Face> faces = new List<Face>();
        //Bottom Face
        for (int i = 0; i < 6; i++)
        {
            faces.Add(CreateFace(0, hexMaskRadius, -waterDepth, -waterDepth, i));
        }
        //Top Face
        float waterHeight = waterThreshold * heightMultiplier * 0.15f;
        for (int i = 0; i < 6; i++)
        {
            faces.Add(CreateFace(0, hexMaskRadius, waterHeight, waterHeight, i));
        }

        //Outer Face
        for (int i = 0; i < 6; i++)
        {
            faces.Add(CreateFace(hexMaskRadius, hexMaskRadius, waterHeight, -waterDepth, i));
        }
        //Inner Face
        for (int i = 0; i < 6; i++)
        {
            faces.Add(CreateFace(0, 0, waterHeight, -waterDepth, i));
        }
        CombineFaces(faces);
    }

    private void CombineFaces(List<Face>faces)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < faces.Count; i++)
        {
            //Add vertices
            vertices.AddRange(faces[i].vertices);
            uvs.AddRange(faces[i].uvs);
            //Offset Triangles
            int offset = (4 * i);
            foreach (int triangles in faces[i].triangles)
            {
                tris.Add(triangles + offset);
            }
        }

        GameObject waterMesh = new GameObject("Water Surface");
        waterMesh.transform.parent = mapHolder;
        waterMesh.transform.localPosition = Vector3.zero;
        waterMesh.AddComponent<MeshRenderer>().material = waterMat;
        MeshFilter meshFilter = waterMesh.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    private Face CreateFace(float innerRadius, float outerRadius, float heightA, float heightB, int point)
    {
        Vector3 pointA = GetPoint(innerRadius, heightA, point);
        Vector3 pointB = GetPoint(innerRadius, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRadius, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRadius, heightB, point);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        return new Face(vertices, triangles, uvs);
    }

    public Vector3 GetPoint(float size, float height, int index)
    {
        float angle_deg = 60 * index;
        float angle_rad = Mathf.PI / 180f * angle_deg;
        return new Vector3(size * Mathf.Cos(angle_rad) * waterMeshSize, height, size * Mathf.Sin(angle_rad) * waterMeshSize);
    }   
}


public struct Face
{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
    #endregion
}
