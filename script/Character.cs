using UnityEngine;
using System.Collections.Generic;

public enum EType
{
    Null = 0,
    Normal = 1,
    Fire = 2,
    Water = 3,
    Electric = 4,
    Grass = 5,
    Ice = 6,
    Fighting = 7,
    Poison = 8,
    Ground = 9,
    Flying = 10,
    Psychic = 11,
    Bug = 12,
    Rock = 13,
    Ghost = 14,
    Dragon = 15,
    Dark = 16,
    Steel = 17,
    Fairy = 18
}
//public interface IActPoint
//public class CAPMovement : IActPoint
public enum EActPoint
{
    Movement = 0,
    MainAttack,
    SubAttack,
    Magic
}

public class CCharacter : CEntity
{
    public class CAPRepo
    {
        public Dictionary<EActPoint, int> m_repo;
        public Dictionary<EActPoint, int> m_repoFull;

        public CAPRepo()
        {
            m_repo = new Dictionary<EActPoint, int>();
            m_repoFull = new Dictionary<EActPoint, int>();
        }
        public void UpdateRepoFull(EActPoint type, int count)
        {
            if (!m_repoFull.ContainsKey(type))
            {
                m_repoFull.Add(type, count);
            }
            else
            {
                m_repoFull[type] = count;
            }
            Restore();
        }
        public void Restore()
        {
            foreach(KeyValuePair<EActPoint, int> kvp in m_repoFull)
            {
                if (!m_repoFull.ContainsKey(kvp.Key))
                    m_repo.Add(kvp.Key, kvp.Value);
                else
                    m_repo[kvp.Key] = kvp.Value;
            }
        }
    }
    public CAPRepo m_APRepo;
    public enum EState
    {
        Init = 0,
        Act_Enabled,
        Act_Disabled,
    }
    public string m_name;

    public int m_strength { get; set; } //力量
    public int m_dexterity { get; set; }//敏捷
    public int m_constitution { get; set; }//体力
    public int m_intelligence { get; set; }//智力
    public int m_perception { get; set; }//感知
    public int m_charisma { get; set; }//魅力

    public int m_team;//所属队伍
    EState m_state;//当前状态
    public EState M_state
    {
        get { return m_state; }
        set
        {
            CLogManager.AddLog($"{m_name}由{m_state}状态转移到了{value}状态");
            if (m_state == EState.Act_Disabled && value == EState.Act_Enabled)
            {
                m_APRepo.Restore();
            }
            m_state = value;
        }
    }

    [SerializeField]
    private int m_hp;
    public int m_Hp
    {
        get
        {
            return m_hp;
        }
        set
        {
            m_hp = value;
            if(m_hp <= 0)
            {
                Die();
            }
        }
    }

    [SerializeField]
    private Vector3Int m_pos;
    public Vector3Int m_Pos
    {
        get
        {
            return m_pos;
        }
        set
        {
            if (M_state == EState.Act_Disabled)
            {
                CLogManager.AddLog($"{m_name}处于{M_state}状态，无法移动！");
                return;
            }
            if (!m_APRepo.m_repo.ContainsKey(EActPoint.Movement))
            {
                CLogManager.AddLog($"{m_name}的m_APRepo没有{EActPoint.Movement}属性", CLogManager.ELogLevel.Error);
                return;
            }
            int dist = Util.ManhattanDistance(value, m_pos);
            int rest_movement = m_APRepo.m_repo[EActPoint.Movement];
            if (dist > rest_movement)
            {
                CLogManager.AddLog($"{m_name}超出移动距离，移动距离{dist}，剩余{rest_movement}");
                return;
            }
            m_pos = value;
            rest_movement -= dist;
            m_APRepo.m_repo[EActPoint.Movement] = rest_movement;
            gameObject.transform.position = CLevelManager.m_grid.CellToWorld(m_pos);
            CLogManager.AddLog($"{m_name}移动到了({m_pos.x},{m_pos.z})，移动距离{dist}，剩余{rest_movement}");
        }
    }
    //public List<IEffect> m_effectList; //键值对查询
    public Dictionary<EEffect, IEffect> m_effectDict;
    public List<IMove> m_moveList;

    public EType m_type1;
    public EType m_type2;

    bool m_live;
    public CCharacter()
    {
        m_effectDict = new Dictionary<EEffect, IEffect>();
        m_moveList = new List<IMove>();
        m_APRepo = new CAPRepo();
        m_live = true;
        m_team = 0;
        m_state = EState.Init;
        
    }
    private void Start()
    {
        m_effectDict = new Dictionary<EEffect, IEffect>();
        m_moveList = new List<IMove>();
        m_APRepo = new CAPRepo();
        m_live = true;
        m_team = 0;
        m_state = EState.Init;
        
    }

    private void Update()
    {
        
    }

    public void OnUpdate()
    {
        if(!m_live)
        {
            return;
        }
        foreach(KeyValuePair<EEffect, IEffect> effectPair in m_effectDict)
        {
            effectPair.Value.OnUpdate();
        }
    }
    public void Spawn(Vector3Int pos)
    {
        CLogManager.AddLog($"{m_name}出现了！");
        m_pos = pos;
    }
    public void Die()
    {
        if (!m_live)
        {
            return;
        }
        CLogManager.AddLog($"{m_name}倒下了！");
        m_live = false;
        //Destroy(this);
    }
    public void Attack(CCharacter obj, int slot)
    {
        if (!m_live)
        {
            return;
        }
        if (obj == null || !obj.m_live)
        {
            CLogManager.AddLog($"{m_name}对不存在的对象使用了招式！");
            return;
        }
        if (m_moveList == null || m_moveList.Count < slot)
        {
            CLogManager.AddLog($"{m_name}使用了不存在的招式！");
            return;
        }
        m_moveList[slot].OnAttack(this, obj, new CMoveExtraInfo());
    }
    public bool CheckLive()
    {
        return m_live;
    }

    public override void OnSelected()
    {
        CLogManager.AddLog($"选择了{m_name}", CLogManager.ELogLevel.Debug);
        M_state = EState.Act_Enabled;
    }
}
