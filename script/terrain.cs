using UnityEngine;
using System.Collections.Generic;

enum ETerrainType
{
    TerrainType_Void = 0,
    TerrainType_Plain = 1,
    TerrainType_River = 2,
    TerrainType_Mount
}
public class CTerrain
{
    public struct CTerrainInfo
    {
        public int m_size_x;
        public int m_size_z;

        public CTerrainInfo(int size_x=16, int size_z=16)
        {
            m_size_x = size_x;
            m_size_z = size_z;
        }
    }
    List<Material> m_materialList;
    List<Sprite> m_spriteList;

    public Vector3Int m_minPos;
    public Vector3Int m_maxPos;

    Vector3Int m_focusPos;

    public Vector3Int M_focusPos
    {
        get
        {
            return m_focusPos;
        }
        set
        {
            //if(value.x < 0)
            m_focusPos = value;
            CLogManager.AddLog($"选择了位置({m_focusPos.x},{m_focusPos.y},{m_focusPos.z})");
        }
    }

    public CTerrain(List<Material> materialList, List<Sprite> spriteList)
    {
        m_materialList = materialList;
        m_spriteList = spriteList;

        m_focusPos = new Vector3Int(0, 0, 0);
    }

    public void generateGrids(CTerrainInfo info, out CTerrainCell[,] gridList)
    {
        int width = info.m_size_x, height = info.m_size_z;
        float PNxStart = 10f, PNzStart = 10f;
        float PNxSampleRate = 16f, PNzSampleRate = 16f;
        float yMin = 0f, yMax = 15f, yStage = 0.333f;
        gridList = new CTerrainCell [width, height];
        for(int x=0; x<width; x++)
        {
            for(int z=0; z<height; z++)
            {   
                float yOrigin = (float)PerlinNoise2D.Noise(PNxStart + (width/PNxSampleRate) / (width - 1) * x, PNzStart + (height/PNzSampleRate) / (height - 1) * z); //0-1
                int stage = (int)(yOrigin * (yMax - yMin) / yStage);
                float y = yMin + stage * yStage;

                ETerrainType terrainType = ETerrainType.TerrainType_Void;
                if (y >= 0.70 * yMax) terrainType = ETerrainType.TerrainType_Mount;
                else if (y >= 0.30 * yMax) terrainType = ETerrainType.TerrainType_Plain;
                else terrainType = ETerrainType.TerrainType_River;

                Material material = m_materialList[(int)terrainType];
                Sprite slicedSprite = m_spriteList[(int)terrainType];

                
                for(int i= 0/*Mathf.FloorToInt(yMin)*/; i<= 0/*Mathf.FloorToInt(y)*/; i++)
                {
                    CTerrainCell grid = new CTerrainCell(material, slicedSprite);
                    Vector3 worldPosition = CLevelManager.m_grid.CellToWorld(new Vector3Int(x, i, z));
                    grid.Spawn(worldPosition);
                }
                
            }
        }
        

        
        
    }
}

public class CTerrainCell
{
    public Material m_material;
    public Sprite m_slicedSprite;
    public GameObject m_instance;
    public CTerrainCell(Material material, Sprite slicedSprite)
    {
        m_material = material;
        m_slicedSprite = slicedSprite;
    }
    public void Spawn(Vector3 position)
    {
        // 创建一个新的立方体游戏对象
        m_instance = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 获取立方体的MeshRenderer组件，用于渲染立方体
        MeshRenderer meshRenderer = m_instance.GetComponent<MeshRenderer>();

        // 创建一个新的材质
        //Material material = new Material(Shader.Find("Standard"));


        Texture2D texture = m_slicedSprite.texture;
        //slicedSprite.texture.wrapMode = TextureWrapMode.Repeat;

        Rect uvRect = m_slicedSprite.textureRect;
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(uvRect.xMin / texture.width, uvRect.yMin / texture.height);
        uv[1] = new Vector2(uvRect.xMax / texture.width, uvRect.yMin / texture.height);
        uv[2] = new Vector2(uvRect.xMax / texture.width, uvRect.yMax / texture.height);
        uv[3] = new Vector2(uvRect.xMin / texture.width, uvRect.yMax / texture.height);
        //for (int i = 0; i < 4; i++) Debug.Log(uv[i]);
        MeshFilter meshFilter = m_instance.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        // 立方体每个面的顶点索引
        int[][] faceIndices = new int[6][]
        {
            new int[] { 0, 1, 2, 3 }, // 前面
            new int[] { 5, 4, 7, 6 }, // 后面
            new int[] { 3, 2, 6, 7 }, // 顶面
            new int[] { 4, 5, 1, 0 }, // 底面
            new int[] { 4, 0, 3, 7 }, // 左面
            new int[] { 1, 5, 6, 2 }  // 右面
        };

        Vector2[] newUV = new Vector2[mesh.uv.Length];
        // 更新每个面的UV坐标
        for (int i = 0; i < mesh.uv.Length; i++)
        {
            if (mesh.uv[i].x == 0f)
            {
                newUV[i].x = uvRect.xMin / texture.width;
            }
            else
            {
                newUV[i].x = uvRect.xMax / texture.width;
            }
            if (mesh.uv[i].y == 0f)
            {
                newUV[i].y = uvRect.yMin / texture.height;
            }
            else
            {
                newUV[i].y = uvRect.yMax / texture.height;
            }
            //Debug.Log($"mesh.uv{newUV[i]}");
        }
        mesh.uv = newUV;

        // 将指定的贴图赋值给材质的主纹理
        m_material.mainTexture = texture;

        // 将新创建的材质赋值给立方体的MeshRenderer组件
        meshRenderer.material = m_material;

        // 设置立方体的位置
        m_instance.transform.position = position;
        m_instance.transform.localScale = new Vector3(1f, 1f, 1f);
        //cube.transform.rotation = Quaternion.Euler(new Vector3(45, 45, 45));
    }
    
}

