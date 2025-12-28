using UnityEngine;
using System.Collections.Generic;

public class CTerrainEntity : CEntity
{
    [SerializeField]
    private Vector3Int m_pos;
    public override Vector3Int Pos
    {
        get { return m_pos; }
        set { m_pos = value; }
    }

    CStatusRepo m_statusRepo;
    public CStatusRepo StatusRepo { get { return m_statusRepo; } }
    [SerializeField]
    int m_moveCost = 1; //-1 for unstandable todo:不同对象的移动率不同
    public int MovingCost { get { return m_moveCost; } set { m_moveCost = value; } }

    Material m_material;
    Sprite m_slicedSprite;
    class CGridSlice
    {
        GameObject m_obj;
        uint m_status;
        public void Init(GameObject prefab, Transform transform)
        {
            m_status = 0;
            m_obj = Instantiate(prefab);
            m_obj.transform.SetParent(transform);
            m_obj.transform.localPosition = pos_offset;
            m_obj.transform.localScale = scale;
            m_obj.SetActive(true); //debug
        }
        public void Render()
        {
            Color color = Color_Common;
            //顺序决定显示的颜色
            do
            {
                if ((m_status & (uint)ETerrainStatus.Attackarea) > 0)
                { color = Color_AttackArea; break; }

                if ((m_status & (uint)ETerrainStatus.Attackable) > 0)
                { color = Color_AttackAble; break; }

                if ((m_status & (uint)ETerrainStatus.Moveable) > 0)
                { color = Color_Moveable; break; }

                if ((m_status & (uint)ETerrainStatus.Myteam) > 0)
                { color = Color_MyTeam; break; }

                if ((m_status & (uint)ETerrainStatus.Allay) > 0)
                { color = Color_Allay; break; }

                if ((m_status & (uint)ETerrainStatus.Enemy) > 0)
                { color = Color_Enemy; break; }

                if ((m_status & (uint)ETerrainStatus.Common) > 0)
                { color = Color_Common; break; }
            } while (false);

            m_obj.GetComponent<MeshRenderer>().material.color = color;
        }
        public void AddStatus(ETerrainStatus status)
        {
            m_status |= (uint)status;
            Render();
        }
        public void RemoveStatus(ETerrainStatus status)
        {
            m_status &= ~(uint)status;
            Render();
        }
        public bool HasStatus(ETerrainStatus status)
        {
            return (m_status & (uint)status) > 0;
        }
        public void Show()
        {
            m_obj.SetActive(true);
        }
        public void Hide()
        {
            m_obj.SetActive(false);
        }
        static byte color_a = 200;
        static Vector3 pos_offset = new Vector3(0f, 0.6f, 0f);
        static Vector3 scale = new Vector3(0.9f, 0.05f, 0.9f);
        static public Color Color_Common = new Color32(0, 117, 255, color_a);
        static public Color Color_MyTeam = new Color32(39, 255, 0, color_a);
        static public Color Color_Allay = new Color32();
        static public Color Color_Enemy = new Color32(255, 0, 0, color_a);
        static public Color Color_Moveable = new Color32(117, 117, 255, color_a);
        static public Color Color_AttackAble = new Color32(230, 255, 0, color_a);
        static public Color Color_AttackArea = new Color32(230, 0, 255, color_a);
    }
    public enum ETerrainStatus
    {
        Common = 1 << 1,
        Myteam = 1 << 2,
        Allay = 1 << 3,
        Enemy = 1 << 4,
        Moveable = 1 << 5,
        Attackable = 1 << 6,
        Attackarea = 1 << 7,
    }

    CGridSlice m_gridSlice;

    CCharacter m_characterOn;
    public CCharacter CharacterOn
    {
        get { return m_characterOn; }
        set
        {
            //if (value) CLogManager.AddLog($"{value.Name}在({m_pos.x},{m_pos.z})");
            //else CLogManager.AddLog($"{m_characterOn.Name}离开了({m_pos.x},{m_pos.z})");
            m_characterOn = value;
        }
    }

    public void Init(Material material, Sprite slicedSprite, Vector3Int cellPos, GameObject slice_prefab)
    {
        m_gridSlice = new CGridSlice();

        m_material = material;
        m_slicedSprite = slicedSprite;
        m_pos = cellPos;
        m_gridSlice.Init(slice_prefab, transform);
    }
    public void Spawn(Vector3 position)
    {
        // 获取立方体的MeshRenderer组件，用于渲染立方体
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

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
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
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
        gameObject.transform.position = position;
        gameObject.transform.localScale = Vector3.one;
        //cube.transform.rotation = Quaternion.Euler(new Vector3(45, 45, 45));
        m_gridSlice.AddStatus(ETerrainStatus.Common);
    }

    private void Start()
    {
        //m_standable = 1;
        //m_moveCost = 1;
    }
    public override void OnSelected()
    {
        //CLogManager.AddLog($"选择了{gameObject.name}", CLogManager.ELogLevel.Debug);
    }

    public void AddTerrainStatus(ETerrainStatus status)
    {
        m_gridSlice.AddStatus(status);
    }
    public void RemoveTerrainStatus(ETerrainStatus status)
    {
        m_gridSlice.RemoveStatus(status);
    }
    public bool HasTerrainStatus(ETerrainStatus status)
    {
        return m_gridSlice.HasStatus(status);
    }
    public void UnShowGridSlice()
    {
        m_gridSlice.Hide();
    }
}

