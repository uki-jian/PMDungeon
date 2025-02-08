using UnityEngine;
using System.Collections.Generic;

public enum EEffect
{
    Null = 0,
    Burn = 1,
}
public interface IEffect
{
    string GetName();
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void Refresh();
}
public abstract class CEffect
{
    protected EEffect m_id { get; set; }
    protected string m_name { get; set; }
    protected int m_remain_turn { get; set; }
    protected CCharacter m_obj { get; set; }
}
public class CBurn : CEffect, IEffect
{
    bool m_spawn = false;

    public string GetName()
    {
        return m_name;
    }
    public void OnEnter()
    {
        CLogManager.AddLog($"{m_obj.m_name}陷入了{m_name}状态");
    }
    public void OnExit()
    {
        CLogManager.AddLog($"{m_obj.m_name}解除了{m_name}状态");
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

        m_obj.m_Hp -= 2;
        m_remain_turn--;
        CLogManager.AddLog($"{m_obj.m_name}由于{m_name}状态，受到了{2}点伤害，剩余{m_obj.m_Hp}HP");
        CLogManager.AddLog($"{m_name}状态还剩{m_remain_turn}回合");
    }
    public void Refresh()
    {
        m_remain_turn = 3;
    }
    public CBurn(CCharacter obj)
    {
        m_id = EEffect.Burn;
        m_name = "燃烧";
        m_remain_turn = 3;
        m_spawn = false;
        m_obj = obj;
    }
}