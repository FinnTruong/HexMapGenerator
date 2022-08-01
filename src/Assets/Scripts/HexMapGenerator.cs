using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab;

    [SerializeField] int mapWidth = 25;
    [SerializeField] int mapHeight = 12;

    public int seed;
    public float tileSize;
    public float refinement;
    public float minTileHeight;
    public float maxTileHeight = 1f;
    [Range(0, 1)]
    public float outlinePercent = 0f;

    public float tileXOffset = 1.5f;
    public float tileZOffset = 1.725f;

    public float hexMaskRadius = 10f;
    private List<Vector3> hexVertices = new List<Vector3>();
    private List<GameObject> tileMap = new List<GameObject>();
    private Transform mapHolder;
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    private void Init()
    {
        tileMap.Clear();
        GetHexMask();
        string holderName = "Generated Tile";
        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        mapHolder.transform.localPosition = Vector3.zero;


    }
    public void GenerateMap()
    {
        Init();
        System.Random pseudoRNG = new System.Random(seed);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                var tileCoord = CoordToPosition(z, x) + transform.position;
                if (IsInsidePolygon(tileCoord, hexVertices.ToArray()))
                {
                    //float randHeight = Mathf.Lerp(minTileHeight, maxTileHeight, (float)pseudoRNG.NextDouble());
                    float randHeight = Mathf.PerlinNoise(x * refinement, z * refinement) * maxTileHeight;
                    GameObject newTile = Instantiate(hexTilePrefab, mapHolder);
                    newTile.name = $"{x},{z}";
                    newTile.transform.localPosition = CoordToPosition(z, x) + Vector3.up * newTile.transform.localScale.z + Vector3.up * (randHeight - 1)* newTile.transform.localScale.z;
                    newTile.transform.localScale = new Vector3((1 - outlinePercent) * tileSize * newTile.transform.localScale.x,
                        (1 - outlinePercent) * tileSize * newTile.transform.localScale.y,
                        randHeight * newTile.transform.localScale.z);
                    tileMap.Add(newTile);
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


}
