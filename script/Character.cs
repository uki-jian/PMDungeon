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
public class CEntity : MonoBehaviour
{
    public string m_name;

    public int m_strength { get; set; } //力量
    public int m_dexterity { get; set; }//敏捷
    public int m_constitution { get; set; }//体力
    public int m_intelligence { get; set; }//智力
    public int m_perception { get; set; }//感知
    public int m_charisma { get; set; }//魅力

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
            m_pos = value;
            gameObject.transform.position = CLevelManager.m_grid.CellToWorld(m_pos);
            CLogManager.AddLog($"{m_name}移动到了({m_pos.x},{m_pos.z})");
        }
    }
    //public List<IEffect> m_effectList; //键值对查询
    public Dictionary<EEffect, IEffect> m_effectDict;
    public List<IMove> m_moveList;

    public EType m_type1;
    public EType m_type2;

    bool m_live;
    public CEntity()
    {
        m_effectDict = new Dictionary<EEffect, IEffect>();
        m_moveList = new List<IMove>();
        m_live = true;
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
    public void Attack(CEntity obj, int slot)
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
}
