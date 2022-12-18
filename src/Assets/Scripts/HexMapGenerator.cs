using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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

    [Range(0, 100)]
    public float hexMaskRadius = 10f;
    private List<Vector3> hexVertices = new List<Vector3>();
    private List<GameObject> tileMap = new List<GameObject>();
    private List<Vector3> tileMapPos = new List<Vector3>();
    private List<Vector3> tileMapScale = new List<Vector3>();
    private List<GameObject> treeMap = new List<GameObject>();
    private List<Vector3> treeScaleMap = new List<Vector3>();
    private Transform mapHolder;

    [Header("Terrain Variables")]
    public Material waterMat;
    public Material sandMat;
    public Material grassMat;
    public Material forestMat;
    public Material mountainMat;
    public Material snowMat;

    public Color sandColor;
    public Color grassColor;
    public Color forestColor;
    public Color mountainColor;
    public Color snowColor;

    [Range(0,1)]
    public float waterThreshold = 0.4f;
    [Range(0,1)]
    public float sandThreshold = 1f;
    [Range(0,1)]
    public float grassThreshold = 2f;
    [Range(0,1)]
    public float forestThreshold = 3f;
    [Range(0,1)]
    public float mountainThreshold = 5f;

    [Header("Water Surface")]

    public GameObject waterMesh;
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


    public GameObject cloud;

    private Sequence spawnSequence;
    private Sequence waveSequence;

    public Preset defautPreset;

    //Island Slider
    public Slider seedSlider, refinementSlider, tileSizeSlider, tileHeightSlider, mapRadiusSlider;
    //Ocean Slider
    public Slider waterThresholdSlider, waterDepthSlider, waterRadiusSlider;
    //Tree Slider
    public Slider treeDensitySlider, minTreeScaleSlider, maxTreeScaleSlider;
    //Terrain Slider
    public Slider sandSlider, grassSlider, forestSlider, mountainSlider;

    // Start is called before the first frame update
    void Start()
    {
        SetDefaultValue();
        //MapSliderValue();
    }

    public void SetDefaultValue()
    {

        seed = defautPreset.seed;
        refinement = defautPreset.refinement;
        tileSize = defautPreset.tileSize;
        heightMultiplier = defautPreset.tileHeight;
        hexMaskRadius = defautPreset.radius;

        waterThreshold = defautPreset.waterThreshold;
        sandThreshold = defautPreset.sandThreshold;
        grassThreshold = defautPreset.grassThreshold;
        forestThreshold = defautPreset.forestThreshold;
        mountainThreshold = defautPreset.mountainThreshold;

        waterMeshSize = defautPreset.waterMeshSize;
        waterDepth = defautPreset.waterDepth;

        treeDensity = defautPreset.treeDensity;
        treeScale = defautPreset.treeScale;

        //MapSliderValue();
        GenerateMap();
        PlaySpawnAnimation();
    }


    private void MapSliderValue()
    {
        seedSlider.value = seed;
        refinementSlider.value = refinement;
        tileSizeSlider.value = tileSize;
        tileHeightSlider.value = heightMultiplier;
        mapRadiusSlider.value = hexMaskRadius;

        waterThresholdSlider.value = waterThreshold;
        waterDepthSlider.value = waterDepth;
        waterRadiusSlider.value = waterMeshSize;

        treeDensitySlider.value = treeDensity;
        minTreeScaleSlider.value = treeScale.x;
        maxTreeScaleSlider.value = treeScale.y;

        sandSlider.value = sandThreshold;
        grassSlider.value = grassThreshold;
        forestSlider.value = forestThreshold;
        mountainSlider.value = mountainThreshold;
    }

    private void Init()
    {
        tileMap.Clear();
        tileMapPos.Clear();
        tileMapScale.Clear();
        treeMap.Clear();
        treeScaleMap.Clear();
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
                        newTile.GetComponent<Renderer>().material.color = GetColorFromHeight(randHeight);
                        newTile.name = $"{x},{z}";
                        var tilePos = CoordToPosition(z, x) + Vector3.up * newTile.transform.localScale.z;
                        var yOffset = Vector3.up * ((randHeight * heightMultiplier) - 1) * newTile.transform.localScale.z;
                        newTile.transform.localPosition = tilePos + yOffset;
                        newTile.transform.localScale = new Vector3((1 - outlinePercent) * tileSize * newTile.transform.localScale.x,
                            (1 - outlinePercent) * tileSize * newTile.transform.localScale.y,
                            randHeight * heightMultiplier * newTile.transform.localScale.z);

                        tileMap.Add(newTile);
                        tileMapPos.Add(tilePos + yOffset);
                        tileMapScale.Add(newTile.transform.localScale);
                        var tileIndex = tileMap.Count;
                        //newTile.transform.DOLocalMove(tilePos + yOffset, 0.3f).SetDelay(tileIndex * 0.0025f);


                        if (randHeight < treeDensity)
                        {
                            var treeIndex = UnityEngine.Random.Range(0, treesPrefab.Length);
                            GameObject tree = Instantiate(treesPrefab[treeIndex], mapHolder);
                            tree.name = $"Tree: {x},{z}";

                            tree.transform.localPosition = newTile.transform.localPosition + Vector3.up * newTile.transform.localScale.z;
                            //tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                            tree.transform.localScale = Vector3.one * Mathf.Lerp(treeScale.x, treeScale.y,(float)pseudoRNG.NextDouble()) * Mathf.Abs(tileSize);
                            tree.transform.parent = newTile.transform;


                            treeMap.Add(tree);
                            treeScaleMap.Add(tree.transform.localScale);
                        }

                        //if (randHeight >= treeMinThreshold && randHeight <= treeMaxThreshold)
                        //{
                           
                        //}
                    }

                }
            }
        }
    }

    public void PlaySpawnAnimation()
    {
        waveSequence.Kill(true);
        spawnSequence.Kill(true);
        spawnSequence = DOTween.Sequence();
        spawnSequence.AppendCallback(() =>
        {
            waterMesh.transform.localScale = Vector3.zero;
            for (int i = 0; i < tileMap.Count; i++)
            {
                tileMap[i].transform.DOComplete();
                tileMap[i].transform.localScale = Vector3.zero;
                //tileMap[i].transform.localPosition = tileMapPos[i] - (Vector3.up * 2f);
            }
            for (int i = 0; i < treeMap.Count; i++)
            {
                treeMap[i].transform.DOComplete();
                treeMap[i].transform.localScale = Vector3.zero;
            }
        });
        spawnSequence.Append(waterMesh.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        spawnSequence.AppendCallback(() =>
        {
            for (int i = 0; i < tileMap.Count; i++)
            {                
                tileMap[i].transform.DOScale(tileMapScale[i], 0.3f).SetDelay(i * 0.0025f).OnStart(()=>
                {
                    tileMap[i].transform.localScale = new Vector3(tileMapScale[i].x, tileMapScale[i].y, 0);
                });
                //tileMap[i].transform.DOLocalMoveY(tileMapPos[i].y, 0.3f).SetDelay(i * 0.0025f);
            }
        });
        spawnSequence.AppendCallback(() =>
        {
            for (int i = 0; i < treeMap.Count; i++)
            {
                treeMap[i].transform.DOScale(treeScaleMap[i], 0.5f).SetDelay(tileMap.Count * 0.0025f + 0.2f + 0 * 0.0025f).SetEase(Ease.OutBack);
            }
        });

    }

    public void MakeWave(bool isRepeat = false)
    {
        waveSequence.Kill(true);
        waveSequence = DOTween.Sequence();
        waveSequence.AppendCallback(() =>
        {
            for (int i = 0; i < tileMap.Count; i++)
            {
                tileMap[i].transform.DOComplete();
                //waveSequence.AppendInterval(i * 0.0025f);
                //waveSequence.Append(tileMap[i].transform.DOLocalJump(tileMap[i].transform.localPosition, 1f, 1, 0.5f));
                tileMap[i].transform.DOLocalJump(tileMap[i].transform.localPosition,1f,1, 0.5f).SetDelay(i * 0.005f);
            }
        });
        waveSequence.AppendInterval(tileMap.Count * 0.005f + 0.5f);
        if(isRepeat)
        {
            waveSequence.SetLoops(-1);
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
        else if (height <= forestThreshold)
            return forestMat;
        else if (height <= mountainThreshold)
            return mountainMat;
        else
            return snowMat;
    }

    private Color GetColorFromHeight(float height)
    {
        if (height <= sandThreshold)
            return sandColor;
        else if (height <= grassThreshold)
            return grassColor;
        else if (height <= forestThreshold)
            return forestColor;
        else if (height <= mountainThreshold)
            return mountainColor;
        else
            return snowColor;
    }
    #region UI Controller Mapping

    //Map Settings
    public void SetSeed(Slider slider)
    {
        seed = (int)slider.value;
        GenerateMap();
    }

    public void SetRefinement(Slider slider)
    {
        refinement = slider.value;
        GenerateMap();
    }

    public void SetTileSize(Slider slider)
    {
        tileSize = slider.value;
        GenerateMap();
    }

    public void SetTileHeight(Slider slider)
    {
        heightMultiplier = slider.value;
        GenerateMap();
    }

    public void SetRadius(Slider slider)
    {
        hexMaskRadius = slider.value;
        GenerateMap();
    }


    //Water Settings
    public void SetWaterSize(Slider slider)
    {
        waterMeshSize = slider.value;
        GenerateMap();
    }

    public void SetWaterDepth(Slider slider)
    {
        waterDepth = slider.value;
        GenerateMap();
    }
    public void SetWaterHeight(Slider slider)
    {
        waterThreshold = slider.value;
        GenerateMap();
    }

    //Foliage Settings
    public void SetTreeDensity(Slider slider)
    {
        treeDensity = slider.value;
        GenerateMap();
    }

    public void SetTreeMinScale(Slider slider)
    {
        treeScale.x = slider.value;
        GenerateMap();
    }    

    public void SetTreeMaxScale(Slider slider)
    {
        treeScale.y = slider.value;
        GenerateMap();
    }

    //Terrain Settings
    public void SetSandThreshold(Slider slider)
    {
        sandThreshold = slider.value;
        GenerateMap();
    }

    public void SetGrassThreshold(Slider slider)
    {
        grassThreshold = slider.value;
        GenerateMap();
    }

    public void SetForestThreshold(Slider slider)
    {
        forestThreshold = slider.value;
        GenerateMap();
    }

    public void SetMountainThreshold(Slider slider)
    {
        mountainThreshold = slider.value;
        GenerateMap();
    }

    public void SetPeakThreshold(Slider slider)
    {
        //snowTjh
    }
    #endregion

    //Palette Settings

    //Effect Settings
    public void EnableCloud()
    {
        cloud.SetActive(!cloud.activeInHierarchy);
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

        waterMesh = new GameObject("Water Surface");
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

    }

    #endregion

}

[Serializable]
public struct Preset
{
    public int seed;
    public float refinement;
    public float tileSize;
    public float tileHeight;
    public float radius;

    [Range(0, 1)]
    public float waterThreshold, sandThreshold, grassThreshold, forestThreshold, mountainThreshold;

    public float waterMeshSize;
    public float waterDepth;

    public float treeDensity;
    public Vector2 treeScale;
}
