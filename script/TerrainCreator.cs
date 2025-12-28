using UnityEngine;
using System.Collections.Generic;

enum ETerrainType
{
    TerrainType_Void = 0,
    TerrainType_Plain = 1,
    TerrainType_River = 2,
    TerrainType_Mount
}
public class CTerrainCreator : MonoBehaviour
{
    public struct CTerrainInfo
    {
        public int m_size_x;
        public int m_size_z;

        public CTerrainInfo(int size_x = 16, int size_z = 16)
        {
            m_size_x = size_x;
            m_size_z = size_z;
        }
    }
    [SerializeField]
    List<Material> m_materialList;
    [SerializeField]
    List<Sprite> m_spriteList;

    public Vector3Int m_minPos;
    public Vector3Int m_maxPos;

    private void Start()
    {
        if (m_materialList == null)
        {
            m_materialList = new List<Material>();
            CLogManager.LogError("没有添加m_materialList");
        }
        if (m_spriteList == null)
        {
            m_spriteList = new List<Sprite>();
            CLogManager.LogError("没有添加m_spriteList");
        }


        //for (int i = 0; i < 4; i++) m_materialList.Add(Resources.Load<Material>("testMaterial1"));
        //Sprite[] allSprites = Resources.LoadAll<Sprite>("terrain/AppleWoods/tileset_0");
        //m_spriteList.Add(allSprites[19]);
        //m_spriteList.Add(allSprites[19]);
        //m_spriteList.Add(allSprites[25]);
        //m_spriteList.Add(allSprites[31]);

        //m_focusPos = Vector3Int.zero;
    }

    public void generateGrids(CTerrainInfo info, out CTerrainEntity[,] gridList)
    {
        // 预加载prefab，避免在循环中重复加载
        GameObject slice_prefab = Resources.Load<GameObject>("prefab/gridSlice");
        GameObject terrain_obj = GameObject.Find(CGlobal.GamePath.Terrain);
        
        int width = info.m_size_x, height = info.m_size_z;
        float PNxStart = 17f, PNzStart = 11f;
        float PNxSampleRate = 16f, PNzSampleRate = 16f;
        float yMin = 0f, yMax = 15f, yStage = 0.333f;
        gridList = new CTerrainEntity[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float yOrigin = (float)PerlinNoise2D.Noise(PNxStart + (width / PNxSampleRate) / (width - 1) * x, PNzStart + (height / PNzSampleRate) / (height - 1) * z); //0-1
                int stage = (int)(yOrigin * (yMax - yMin) / yStage);
                float y = yMin + stage * yStage;

                ETerrainType terrainType = ETerrainType.TerrainType_Void;
                if (y >= 0.70 * yMax) terrainType = ETerrainType.TerrainType_Mount;
                else if (y >= 0.30 * yMax) terrainType = ETerrainType.TerrainType_Plain;
                else terrainType = ETerrainType.TerrainType_River;

                Material material = m_materialList[(int)terrainType];
                Sprite slicedSprite = m_spriteList[(int)terrainType];


                for (int i = 0/*Mathf.FloorToInt(yMin)*/; i <= 0/*Mathf.FloorToInt(y)*/; i++)
                {
                    GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);   
                    instance.transform.SetParent(terrain_obj.transform);
                    CTerrainEntity grid = instance.AddComponent<CTerrainEntity>();
                    Vector3Int cellPos = new Vector3Int(x, i, z);
                    Vector3 worldPosition = CLevelManager.Grid3D.CellToWorld(cellPos);
                    grid.Init(material, slicedSprite, cellPos, slice_prefab);
                    grid.Spawn(worldPosition);

                    if (terrainType == ETerrainType.TerrainType_Mount) grid.MovingCost = -1;

                    gridList[x, z] = grid;
                }

            }
        }




    }
}
