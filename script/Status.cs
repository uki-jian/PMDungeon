using UnityEngine;
using System.Collections.Generic;

public enum EStatus
{
    Null = 0,
    Condition_Burn,
    Type_Fire,
    Type_GrassBug,
    Type_DarkPoision,
    Ethnic_Humanlike,
}
public interface IStatus
{
    string GetName();
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void Refresh();
}
public abstract class CStatus
{
    protected EStatus m_id { get; set; }
    protected string m_name { get; set; }
    protected int m_remain_turn { get; set; }
    protected CCharacter m_obj { get; set; }
}
public class CBurn : CStatus, IStatus
{
    bool m_spawn = false;

    public string GetName()
    {
        return m_name;
    }
    public void OnEnter()
    {
        CLogManager.LogInfo($"{m_obj.Name}陷入了{m_name}状态");
    }
    public void OnExit()
    {
        CLogManager.LogInfo($"{m_obj.Name}解除了{m_name}状态");
    }
    public void OnUpdate()
    {
        if(m_remain_turn <= 0)
        {
            OnExit();
            return;
        }
        if(!m_spawn)
        {
            OnEnter();
            m_spawn = true;
        }

        m_obj.Hp -= 2;
        m_remain_turn--;
        CLogManager.LogInfo($"{m_obj.Name}由于{m_name}状态，受到了{2}点伤害，剩余{m_obj.Hp}HP");
        CLogManager.LogInfo($"{m_name}状态还剩{m_remain_turn}回合");
    }
    public void Refresh()
    {
        m_remain_turn = 3;
    }
    public CBurn(CCharacter obj)
    {
        m_id = EStatus.Condition_Burn;
        m_name = "燃烧";
        m_remain_turn = 3;
        m_spawn = false;
        m_obj = obj;
    }
}