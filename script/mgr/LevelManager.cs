using UnityEngine;
using System;
using System.Collections.Generic;




public class CLevelManager : MonoBehaviour, IPipe
{
    public struct CActionInfo : IComparable<CActionInfo>
    {
        public int m_turn;
        public CEntity m_obj;

        public int CompareTo(CActionInfo other)
        {
            //      如果返回值小于 0，表示当前对象小于另一个对象。小的优先
            if (object.ReferenceEquals(other, null)) return -1;
            if (m_turn != other.m_turn) return m_turn - other.m_turn; //回合数小的先动

            return other.m_obj.m_dexterity - m_obj.m_dexterity; //敏捷值高的先动
        }
    }

    public CTerrain m_terrain;
    public List<Material> materialList;
    public List<Sprite> spriteList;

    public int m_x;
    public int m_z;

    public CTerrainCell[,] m_gridList;
    public List<CEntity> m_entityList;
    public PriorityQueue<CActionInfo> m_actionQueue;

    static public CGrid3D m_grid;

    public CEntity hero;

    void Start()
    {
        initmat_test1();

        Vector3 cellSize = new Vector3(1, 1, 1);
        Vector3 cellGap = new Vector3(0, 0, 0);
        m_grid = gameObject.AddComponent<CGrid3D>();
        m_grid.Init(cellSize, cellGap);

        //init terrain
        m_terrain = new CTerrain(materialList, spriteList);
        CTerrain.CTerrainInfo info = new CTerrain.CTerrainInfo(m_x, m_z);
        m_terrain.generateGrids(info, out m_gridList);

        //init character
        m_entityList = new List<CEntity>();
        m_actionQueue = new PriorityQueue<CActionInfo>();

        Init_test1();
    }

    void initmat_test1()
    {
        m_x = 16;
        m_z = 16;

        materialList = new List<Material>();
        spriteList = new List<Sprite>();
        for (int i = 0; i < 4; i++) materialList.Add(Resources.Load<Material>("testMaterial1"));
        Sprite[] allSprites = Resources.LoadAll<Sprite>("entity/AppleWoods/tileset_0");
        spriteList.Add(allSprites[19]);
        spriteList.Add(allSprites[19]);
        spriteList.Add(allSprites[25]);
        spriteList.Add(allSprites[31]);
    }
    void Init_test1()
    {
        hero = GameObject.Find("testCharacter").GetComponent<CEntity>();
        CEntity zhongzi = hero;
        zhongzi.m_name = "妙蛙种子";
        zhongzi.m_Hp = 16;
        zhongzi.m_dexterity = 5;
        zhongzi.m_moveList.Add(new CTackle()); //改工厂模式
        zhongzi.m_type1 = EType.Grass;
        zhongzi.m_type2 = EType.Poison;
        zhongzi.Spawn(new Vector3Int(1, 0, 1));
        zhongzi.m_Pos = new Vector3Int(1, 1, 1);

        CEntity huolong = new CEntity();
        huolong.m_name = "小火龙";
        huolong.m_Hp = 13;
        huolong.m_dexterity = 6;
        huolong.m_moveList.Add(new CEmber());
        huolong.m_type1 = EType.Fire;
        huolong.m_type2 = EType.Null;
        huolong.Spawn(new Vector3Int(8, 0, 8));

        m_entityList.Add(zhongzi);
        m_entityList.Add(huolong);

        foreach (CEntity item in m_entityList)
        {
            CActionInfo info = new CActionInfo();
            info.m_obj = item;
            info.m_turn = 0;
            m_actionQueue.Enqueue(info);
        }

        hero = zhongzi;
    }
    private void Update()
    {
        //OnAction();
    }

    void OnPreAction()
    {

    }
    void OnAction()
    {
        if (m_actionQueue.Empty()) return;
        CActionInfo action = m_actionQueue.Dequeue();
        if (!action.m_obj.CheckLive()) return;

        CLogManager.AddLog($"现在是{action.m_obj.m_name}的第{action.m_turn}次行动！");

        // take actions
        if (action.m_obj.m_name == "妙蛙种子")
            action.m_obj.Attack(m_entityList[1], 0);
        else
            action.m_obj.Attack(m_entityList[0], 0);

        action.m_obj.OnUpdate();

        ///////////////////

        if (action.m_obj.CheckLive())
        {
            action.m_turn++;
            m_actionQueue.Enqueue(action);
        }
    }
    void OnPostAction()
    {

    }

    public void TransferData(EMessageType type, object info)
    {
        if(type == EMessageType.SetActiveCellPosition)
        {
            Vector3? worldPos = info as Vector3?;
            
            if (!worldPos.HasValue) return; //改返回值
            m_terrain.M_focusPos = m_grid.WorldToCell((Vector3)worldPos);
        }
        else if(type == EMessageType.GetFocusCellPosition)
        {
            if (!(info is MessageInfo.CellPosition))
            {
                CLogManager.AddLog("leixingcuowu");
                return;
            }
            MessageInfo.CellPosition cellPos = (MessageInfo.CellPosition)info;
            cellPos.pos = m_grid.CellToWorld(hero.m_Pos);


        }
        
    }

}
