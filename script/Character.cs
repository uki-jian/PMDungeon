using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public enum EType
//{
//    Null = 0,
//    Normal = 1,
//    Fire = 2,
//    Water = 3,
//    Electric = 4,
//    Grass = 5,
//    Ice = 6,
//    Fighting = 7,
//    Poison = 8,
//    Ground = 9,
//    Flying = 10,
//    Psychic = 11,
//    Bug = 12,
//    Rock = 13,
//    Ghost = 14,
//    Dragon = 15,
//    Dark = 16,
//    Steel = 17,
//    Fairy = 18
//}
//public interface IActPoint
//public class CAPMovement : IActPoint


public class CCharacter : CEntity
{
    [SerializeField]
    string m_name;
    public string Name { get { return m_name; } set { m_name = value; } }

    [SerializeField]
    Sprite m_portrait;
    public Sprite Portrait { get { return m_portrait; } }

    [SerializeField]
    private int m_hp;
    public int Hp
    {
        get{ return m_hp; }
        set
        {
            m_hp = value;
            if (m_hp <= 0)
            {
                Die();
            }
        }
    }
    [SerializeField]
    int m_strength = 8;
    /// <summary>
    /// 力量
    /// </summary>
    public int Str { get { return m_strength; } set { m_strength = value; } }
    [SerializeField]
    int m_dexterity = 8;
    /// <summary>
    /// 敏捷
    /// </summary>
    public int Dex { get { return m_dexterity; } set { m_dexterity = value; } }
    [SerializeField]
    int m_constitution = 8;
    /// <summary>
    /// 体力
    /// </summary>
    public int Con { get { return m_constitution; } set { m_constitution = value; } }
    [SerializeField]
    int m_intelligence = 8;
    /// <summary>
    /// 智力
    /// </summary>
    public int Int { get { return m_intelligence; } set { m_intelligence = value; } }
    [SerializeField]
    int m_wisdom = 8;
    /// <summary>
    /// 感知
    /// </summary>
    public int Wis { get { return m_wisdom; } set { m_wisdom = value; } }
    [SerializeField]
    int m_charisma = 8;
    /// <summary>
    /// 魅力
    /// </summary>
    public int Cha { get { return m_charisma; } set { m_charisma = value; } }

    [SerializeField]
    int m_team; //所属队伍 0:自己队伍, 1:友军, other:敌军
    public int Team { get { return m_team; } set { m_team = value; } }

    public class CState
    {
        public enum EState
        {
            Init = 0,
            Act_Enabled,
            Act_Enabled_Animating,
            Act_Disabled,
        }
        Dictionary<EState, HashSet<EState>> m_stateMap;
        [SerializeField]
        EState m_currentState;
        public EState CurrentState
        {
            get { return m_currentState; }
            set
            {
                if (!m_stateMap.ContainsKey(m_currentState)) return;
                if (m_stateMap[m_currentState].Contains(value))
                {
                    m_currentState = value;
                }
                else
                {
                    CLogManager.AddLog($"{m_currentState}无法转换到{value}状态", CLogManager.ELogLevel.Warning);
                }
            }
        }
        public CState()
        {
            m_stateMap = new Dictionary<EState, HashSet<EState>>();
            m_stateMap.Add(EState.Init, new HashSet<EState>());
            m_stateMap.Add(EState.Act_Enabled, new HashSet<EState>());
            m_stateMap.Add(EState.Act_Enabled_Animating, new HashSet<EState>());
            m_stateMap.Add(EState.Act_Disabled, new HashSet<EState>());

            m_stateMap[EState.Init].Add(EState.Act_Enabled);
            m_stateMap[EState.Init].Add(EState.Act_Disabled);
            m_stateMap[EState.Act_Enabled].Add(EState.Act_Enabled_Animating);
            m_stateMap[EState.Act_Enabled].Add(EState.Act_Disabled);
            m_stateMap[EState.Act_Enabled_Animating].Add(EState.Act_Enabled);
            m_stateMap[EState.Act_Disabled].Add(EState.Act_Enabled);

            m_currentState = EState.Init;
        }
    }

    [SerializeField]
    CState m_state;//当前状态
    public CState.EState State
    {
        get { return m_state.CurrentState; }
        set
        {
            if (m_state.CurrentState == value) return;
            CLogManager.AddLog($"{Name}由{m_state.CurrentState}状态转移到{value}状态");
            m_state.CurrentState = value;
        }
    }

    [SerializeField]
    Vector3 m_posOffset = Vector3.up;

    [SerializeField]
    Vector3Int m_pos;
    public override Vector3Int Pos
    {
        get { return m_pos; }
        set
        {
            if(m_areaUpdater != null)
            {
                m_areaUpdater.UpdateCharacterArea(this, m_pos, false);
                m_areaUpdater.UpdateCharacterArea(this, value, true);
            }
            m_pos = value;
        }
    }
    Vector3Int m_posTarget; //目标位置

    IAreaUpdater m_areaUpdater;
    CTerrainEntity m_terrainOn;
    public CTerrainEntity TerrainOn { get { return m_terrainOn; } set { m_terrainOn = value; } }

    public IEnumerator MoveOnDelay(List<Vector3Int> path)
    {
        for (int i = 0; i<path.Count;)
        {
            if (MoveOn(path[i]))
            {
                i++;
            }
            yield return null;
        }
        
    }
    public bool MoveOn(Vector3Int target)
    {
        if (State != CState.EState.Act_Enabled)
        {
            //CLogManager.AddLog($"{m_name}处于{M_state}状态，无法移动！");
            return false;
        }
        if (target == Pos)
        {
            return true;
        }

        int dist = Util.ManhattanDistance(target, Pos);
        int rest_movement = m_actRepo.GetRestPoint(EAction.Movement);
        if (rest_movement == int.MinValue)
        {
            CLogManager.AddLog($"{Name}的m_APRepo没有{EAction.Movement}属性", CLogManager.ELogLevel.Error);
            return false;
        }
        if (dist > rest_movement)
        {
            CLogManager.AddLog($"{Name}超出移动距离，移动距离{dist}，剩余{rest_movement}", CLogManager.ELogLevel.Error);
            return false;
        }
        m_actRepo.Consume(EAction.Movement, dist);

        m_posTarget = target;
        //gameObject.transform.position = CLevelManager.m_grid.CellToWorld(m_pos);
        CLogManager.AddLog($"{Name}移动到了({Pos.x},{Pos.z})，移动距离{dist}，剩余{rest_movement-dist}");
        State = CState.EState.Act_Enabled_Animating;

        return true;
    }
    GameObject m_Shadow;
    public bool MakeShadow(Vector3Int target)
    {
        if (m_Shadow)
        {
            Destroy(m_Shadow);
        }
        int dist = Util.ManhattanDistance(target, Pos);
        int rest_movement = m_actRepo.GetRestPoint(EAction.Movement);
        if (dist == 0 || dist > rest_movement)
        {
            return false;
        }
        m_Shadow = Instantiate(gameObject);
        m_Shadow.transform.position = CLevelManager.Grid3D.CellToWorld(target) + m_posOffset;
        m_Shadow.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.3f);
        m_Shadow.GetComponent<CCharacter>().enabled = false;
        m_Shadow.GetComponent<BoxCollider>().enabled = false;
        return true;
    }
    public void RemoveShadow()
    {
        if (m_Shadow)
        {
            Destroy(m_Shadow);
        }
    }

    CActRepo m_actRepo;
    public CActRepo ActRepo { get { return m_actRepo; } }
    IUIActionResource m_uiActionResource;

    CMoveRepo m_moveRepo;
    public CMoveRepo MoveRepo { get { return m_moveRepo; } }
    IUIMovePanel m_uiMovePanel;
    CStatusRepo m_statusRepo;
    public CStatusRepo StatusRepo { get { return m_statusRepo; } }

    bool m_live;
    public bool Live{ get { return m_live; } }

    bool m_inBattle = true;
    public bool InBattle { get { return m_inBattle; } }

    [SerializeField]
    float m_moveSpeed = 5f; //sprite走格子的速度

    public CCharacter()
    {
        m_actRepo = new CActRepo();
        m_moveRepo = new CMoveRepo();
        m_actRepo = new CActRepo();
        m_statusRepo = new CStatusRepo();
        m_state = new CState();
        m_live = true;
        Team = 0;
    }
    private void Start()
    {
        m_uiActionResource = GameObject.Find(CGlobal.GamePath.ActionResource).GetComponent<CUIActionResource>();
        m_uiMovePanel = GameObject.Find(CGlobal.GamePath.MovePanel).GetComponent<CUIMovePanel>();
        m_areaUpdater = GameObject.Find(CGlobal.GamePath.Director).GetComponent<CLevelManager>();
    }
    

    private void Update()
    {
        Vector3 pos_target = CLevelManager.Grid3D.CellToWorld(m_posTarget) + m_posOffset;
        Vector3 pos_diff = pos_target - gameObject.transform.position;
        if (Mathf.Abs(pos_diff.x) + Mathf.Abs(pos_diff.z) > 0.05f)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pos_target, Time.deltaTime * m_moveSpeed);
        }
        else
        {
            if(State == CState.EState.Act_Enabled_Animating)
            {
                State = CState.EState.Act_Enabled;
                Pos = m_posTarget;
            }
        }
    }

    public void OnUpdate()
    {
        if(!m_live)
        {
            return;
        }
        m_statusRepo.OnUpdate(this);
    }
    public void Spawn(Vector3Int pos)
    {
        CLogManager.AddLog($"{Name}出现了！");
        Pos = pos;
        m_posTarget = pos;
        transform.position = CLevelManager.Grid3D.CellToWorld(m_pos) + m_posOffset;
        State = CState.EState.Act_Disabled;
    }
    public void Die()
    {
        if (!m_live)
        {
            return;
        }
        CLogManager.AddLog($"{Name}倒下了！");
        State = CState.EState.Act_Disabled;
        m_areaUpdater.UpdateCharacterArea(this, Pos, false);
        m_live = false;
        //Destroy(this);
    }
    public void Attack(CCharacter obj, EMove slot)
    {
        if (!m_live)
        {
            return;
        }
        if (obj == null || !obj.m_live)
        {
            CLogManager.AddLog($"{Name}对不存在的对象使用了招式！");
            return;
        }
        if (m_moveRepo == null)
        {
            CLogManager.AddLog($"{Name}没有m_moveRepo！", CLogManager.ELogLevel.Error);
            return;
        }
        IMove move;
        if (!m_moveRepo.GetMove(slot, out move))
        {
            CLogManager.AddLog($"{Name}使用了不存在的招式{slot}！");
            return;
        }
        m_moveRepo.OnAttack(this, obj, slot, new CMoveExtraInfo());
    }

    public override void OnSelected()
    {
        m_uiActionResource.OnUIShow(m_actRepo);//debug
        m_uiMovePanel.OnUIShow(m_moveRepo);
        //CLogManager.AddLog($"选择了{Name}", CLogManager.ELogLevel.Debug);
        State = CState.EState.Act_Enabled;
    }
    
    public void OnTurnStarts()
    {
        m_uiActionResource.OnUIShow(m_actRepo);
        m_uiMovePanel.OnUIShow(m_moveRepo);
    }
    public void OnTurnEnds()
    {
        m_uiActionResource.OnUIHide();
        m_uiMovePanel.OnUIHide();
        State = CState.EState.Act_Disabled;
    }
}


public class CStatusRepo
{
    Dictionary<EStatus, IStatus> m_status;
    public CStatusRepo()
    {
        m_status = new Dictionary<EStatus, IStatus>();
    }
    public void UpdateStatue(EStatus id, IStatus status)
    {
        if (m_status.ContainsKey(id))
        {
            m_status.Remove(id);
        }
        m_status.Add(id, status);
    }
    public bool GetStatus(EStatus id, out IStatus status)
    {
        if (m_status.ContainsKey(id))
        {
            status = m_status[id];
            return true;
        }
        status = null;
        return false;
    }
    public void OnUpdate(CCharacter character)
    {
        foreach (KeyValuePair<EStatus, IStatus> effectPair in m_status)
        {
            effectPair.Value.OnUpdate();
        }
    }
}
