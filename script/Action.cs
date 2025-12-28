using UnityEngine;
using System.Collections.Generic;

public enum EAction
{
    Default = 0,
    Movement,
    Action,
    BonusAction,
    Magic
}
public interface IAction
{
    EAction ID { get; }
    string Name { get; }
    int FullPoint { get; set; }
    int RestPoint { get; set; }
    Sprite Icon { get; }
    Sprite IconUsed { get; }
}
public class CActionDefault : IAction
{
    public EAction ID { get { return EAction.Default; } }
    public string Name { get { return "默认行动"; } }
    public int FullPoint { get; set; }
    public int RestPoint { get; set; }
    public Sprite Icon { get; }
    public Sprite IconUsed { get; }
}
public class CMovement : IAction
{
    public EAction ID { get { return EAction.Movement; } }
    public string Name { get { return "移动速度"; } }
    int m_fullPoint = m_default_point;
    public int FullPoint { get { return m_fullPoint; } set { m_fullPoint = value; } }
    int m_restPoint = m_default_point;
    public int RestPoint { get { return m_restPoint; } set { m_restPoint = value; } }
    public Sprite Icon { get { return m_icon; } }
    public Sprite IconUsed { get { return m_icon; } }

    static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Movement);
    static Sprite m_icon_used = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Movement_used);
    static int m_default_point = 8;
}
public class CAction : IAction
{
    public EAction ID { get { return EAction.Action; } }
    public string Name { get { return "行动"; } }
    int m_fullPoint = m_default_point;
    public int FullPoint { get { return m_fullPoint; } set { m_fullPoint = value; } }
    int m_restPoint = m_default_point;
    public int RestPoint { get { return m_restPoint; } set { m_restPoint = value; } }
    public Sprite Icon { get { return m_icon; } }
    public Sprite IconUsed { get { return m_icon; } }

    static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Action);
    static Sprite m_icon_used = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Action_used);
    static int m_default_point = 1;
}
public class CBonusAction : IAction
{
    public EAction ID { get { return EAction.BonusAction; } }
    public string Name { get { return "附赠行动"; } }
    int m_fullPoint = m_default_point;
    public int FullPoint { get { return m_fullPoint; } set { m_fullPoint = value; } }
    int m_restPoint = m_default_point;
    public int RestPoint { get { return m_restPoint; } set { m_restPoint = value; } }
    public Sprite Icon { get { return m_icon; } }
    public Sprite IconUsed { get { return m_icon; } }

    static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Bonus_action);
    static Sprite m_icon_used = Resources.Load<Sprite>(CGlobal.ResPath.Icon_Bonus_action_used);
    static int m_default_point = 1;
}
public class CActionCreater
{
    static public IAction CreateAction(EAction id)
    {
        switch (id)
        {
            case EAction.Movement:
                return new CMovement();
            case EAction.Action:
                return new CAction();
            case EAction.BonusAction:
                return new CBonusAction();
            default:
                return new CActionDefault();
        }   

    }
    static public IAction CreateAction(EAction id, int point)
    {
        IAction action;
        switch (id)
        {
            case EAction.Movement:
                action = new CMovement(); break;
            case EAction.Action:
                action = new CAction(); break;
            case EAction.BonusAction:
                action = new CBonusAction(); break;
            default:
                action = new CActionDefault();
                break;
        }
        action.FullPoint = point;
        action.RestPoint = point;
        return action;
    }
}
public interface IActRepo
{
    //int Consume(EAction act, int cost);
    //int GetRestPoint(EAction act);
    //int GetFullPoint(EAction act);
    //void UpdateRepoFull(EAction type, IAction action);
    //void RestoreAll();
    Dictionary<EAction, IAction> Repo { get; }
}
public class CActRepo : IActRepo
{
    Dictionary<EAction, IAction> m_repo;
    public Dictionary<EAction, IAction> Repo { get { return m_repo; } }

    public CActRepo()
    {
        m_repo = new Dictionary<EAction, IAction>();
    }
    public int Consume(EAction act, int cost) //return >=0 for success, -X for X not satisfied
    {
        if (m_repo.ContainsKey(act))
        {
            int rest = m_repo[act].RestPoint - cost;
            if (rest >= 0)
            {
                m_repo[act].RestPoint -= cost;
            }
            return rest;
        }
        else
        {
            CLogManager.LogError($"CAPRepo GetPoint cannot find key:{act}");
            return int.MinValue;
        }
    }
    public int GetRestPoint(EAction act) //return >=0 for success, MinValue for failure
    {
        if (m_repo.ContainsKey(act))
        {
            return m_repo[act].RestPoint;
        }
        else
        {
            CLogManager.LogError($"CAPRepo GetPoint cannot find key:{act}");
            return int.MinValue;
        }
    }
    public int GetFullPoint(EAction act) //return >=0 for success, MinValue for failure
    {
        if (m_repo.ContainsKey(act))
        {
            return m_repo[act].FullPoint;
        }
        else
        {
            CLogManager.LogError($"CAPRepo GetPointFull cannot find key:{act}");
            return -1;
        }
    }
    public void UpdateRepoFull(EAction type, IAction action)
    {
        if (!m_repo.ContainsKey(type))
        {
            m_repo.Add(type, action);
        }
        else
        {
            m_repo[type] = action;
        }

        RestoreAll();//debug
    }
    public void RestoreAll()
    {
        foreach (IAction action in m_repo.Values)
        {
            action.RestPoint = action.FullPoint;
        }
    }
}