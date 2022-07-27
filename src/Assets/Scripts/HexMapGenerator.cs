using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab;

    [SerializeField] int mapWidth = 25;
    [SerializeField] int mapHeight = 12;

    public float hexOuterRadius;
    public float hexInnerRadius;

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
                GameObject temp = Instantiate(hexTilePrefab,mapHolder);
                temp.name = $"{x},{z}";
                temp.transform.localPosition = CoordToPosition(x, z);
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
}
