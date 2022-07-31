using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab;

    [SerializeField] int mapWidth = 25;
    [SerializeField] int mapHeight = 12;

    public float circleMaskRadius = 10f;
    public float hexMaskRadius = 10f;
    public List<Vector3> hexVertices = new List<Vector3>();

    public float tileXOffset = 1.5f;
    public float tileZOffset = 1.725f;

    private Transform mapHolder;
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    private void Init()
    {
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

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                var tileCoord = CoordToPosition(x, z) + transform.position;
                if (IsInsidePolygon(tileCoord, hexVertices.ToArray()))
                {
                    GameObject temp = Instantiate(hexTilePrefab, mapHolder);
                    temp.name = $"{x},{z}";
                    temp.transform.localPosition = CoordToPosition(x, z);
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
            return new Vector3(x * tileXOffset + xOffset, 0, z * tileZOffset + zOffset);
        }
        else
        {
            return new Vector3(x * tileXOffset + tileXOffset / 2 + xOffset, 0, z * tileZOffset + zOffset);
        }
    }

    public bool IsInCircle(Vector3 pos)        
    {

        return Vector3.Distance(transform.position, pos) < circleMaskRadius;
    }

    private bool IsInHex(Vector3 pos)
    {
        for (int i = 0; i <6; i++)
        {
            float angle_deg = 60 * i;
            float angle_rad = Mathf.PI / 180f * angle_deg;
            var point = new Vector3(Mathf.Cos(angle_rad), 0, Mathf.Sin(angle_rad)) + pos;
            if (!IsInCircle(point))
                return false;
        }
        return true;
    }

    private void GetHexMask()
    {
        hexVertices.Clear();
        for (int i = 0; i < 6; i++)
        {
            float angle_deg = 60 * i;
            float angle_rad = Mathf.PI / 180f * angle_deg;
            var point = new Vector3(hexMaskRadius * Mathf.Cos(angle_rad), 0, hexMaskRadius * Mathf.Sin(angle_rad)) + transform.position;
            hexVertices.Add(point);
        }
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
    public bool IsInsidePolygon(Vector3 v, Vector3[] p)
    {
        int j = p.Length - 1;
        bool c = false;
        for (int i = 0; i < p.Length; j = i++)
        {
            c ^= p[i].z > v.z ^ p[j].z > v.z && v.x < (p[j].x - p[i].x) * (v.z - p[i].z) / (p[j].z - p[i].z) + p[i].x;
        }
        return c;
    }

}
