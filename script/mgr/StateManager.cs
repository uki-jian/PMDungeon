using UnityEngine;
using System.Collections.Generic;

public class CStateManager : MonoBehaviour //special
{
    public enum EState
    {
        Undefined,
        Start,
        MyTeamAct_Standby, //我方行动，可以移动、选择招式
        MyTeamAct_attack, //我方行动，点击招式后，选择对象
        OtherAct,
    }
    static CStateManager()
    {
        stateMap = new Dictionary<EState, HashSet<EState>>();
        stateMap.Add(EState.Undefined, new HashSet<EState>());
        stateMap.Add(EState.Start, new HashSet<EState>());
        stateMap.Add(EState.MyTeamAct_Standby, new HashSet<EState>());
        stateMap.Add(EState.MyTeamAct_attack, new HashSet<EState>());
        stateMap.Add(EState.OtherAct, new HashSet<EState>());

        stateMap[EState.Undefined].Add(EState.Start);
        stateMap[EState.Start].Add(EState.MyTeamAct_Standby);
        stateMap[EState.Start].Add(EState.OtherAct);
        stateMap[EState.MyTeamAct_Standby].Add(EState.OtherAct);
        stateMap[EState.MyTeamAct_Standby].Add(EState.MyTeamAct_attack);
        stateMap[EState.MyTeamAct_attack].Add(EState.MyTeamAct_Standby);
        stateMap[EState.OtherAct].Add(EState.MyTeamAct_Standby);

        currentState = EState.Undefined;
    }
    static Dictionary<EState, HashSet<EState>> stateMap;

    
    static EState currentState;
    static public EState CurrentState
    {
        get { return currentState; }
        set
        {
            if (!stateMap.ContainsKey(currentState))
            {
                CLogManager.LogError($"不存在{currentState}状态!");
                return;
            }
            if (stateMap[currentState].Contains(value))
            {
                CLogManager.LogDebug($"从{currentState}{value}状态");
                Transit(currentState, value);
                currentState = value;
            }
            else
            {
                CLogManager.LogWarning($"{currentState}无法转换到{value}状态");
            }
        }
    }
    static void Transit(EState before, EState after)
    {

    }

}


public class CState
{
    public enum EState
    {
        Undefined,
    }
    public CState()
    {
        stateMap = new Dictionary<EState, HashSet<EState>>();
        currentState = EState.Undefined;
    }
    public Dictionary<EState, HashSet<EState>> stateMap;


    protected EState currentState;
    public EState CurrentState
    {
        get { return currentState; }
        set
        {
            if (!stateMap.ContainsKey(currentState))
            {
                CLogManager.LogError($"不存在{currentState}状态!");
                return;
            }
            if (stateMap[currentState].Contains(value))
            {
                CLogManager.LogDebug($"从{currentState}转换到{value}状态");
                Transit(currentState, value);
                currentState = value;
            }
            else
            {
                CLogManager.LogWarning($"{currentState}无法转换到{value}状态");
            }
        }
    }
    public virtual void Transit(EState before, EState after)
    {

    }

}
