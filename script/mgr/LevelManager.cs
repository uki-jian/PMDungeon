using UnityEngine;
using System;
using System.Collections.Generic;

public class CLevelManager : MonoBehaviour, IPipe, IActionQueue, IAreaUpdater, IActionSink
{
    [SerializeField]
    CTerrainCreator m_terrainCreator;
    [SerializeField]
    CCharacterCreator m_characterCreator;
    [SerializeField]
    int m_xLen = 16;
    [SerializeField]
    int m_zLen = 16;

    static CGrid3D m_grid3D;
    static public CGrid3D Grid3D { get { return m_grid3D; } }

    CTerrainEntity[,] m_terrainList; //改造成3维
    CTerrainEntity GetTerrainEntity(Vector3Int new_pos)
    {
        if (new_pos.x < 0 || new_pos.x >= m_xLen) return null;
        if (new_pos.z < 0 || new_pos.z >= m_zLen) return null;
        return m_terrainList[new_pos.x, new_pos.z];
    }
    List<CCharacter> m_characterList;

    // IActionQueue
    static public uint actionQueueMaxSize = 50;
    PriorityQueue<CActionInfo> m_actionQueue;
    IUIActionQueue m_uiActionQueue;
    public PriorityQueue<CActionInfo> ActionQueue { get { return m_actionQueue; } }

    public void ActionInit()
    {
        m_uiActionQueue.OnUIShow();
    }
    public void ActionUninit()
    {
        m_actionQueue.Clear();
        m_uiActionQueue.OnUIHide();
    }
    [ContextMenu("Next Turn")]
    public void OnNextTurn()
    {
        if (m_actionQueue.Empty()) return;
        CActionInfo previous = ActionQueue.Dequeue();
        previous.Obj.OnTurnEnds();

        if (m_actionQueue.Empty()) return;
        CActionInfo current = m_actionQueue.Top;
        current.Obj.OnTurnStarts();

        CLogManager.LogInfo($"由{previous.Obj.Name}的第{previous.Turn}次行动，切换至{current.Obj.Name}的第{current.Turn}次行动");
        m_uiActionQueue.OnUIUpdate();
    }
    /// <summary>
    /// 自动去除重复character
    /// </summary>
    /// <param name="newActions"></param>
    public void OnUpdateQueue(List<CActionInfo> newActions)
    {
        List<CActionInfo> tempActionList;
        if (newActions == null)
            tempActionList = new List<CActionInfo>();
        else
            tempActionList = newActions;

        while (!m_actionQueue.Empty())
        {
            CActionInfo info = m_actionQueue.Dequeue();
            if (info.Obj.Live && info.Obj.InBattle) tempActionList.Add(info);
        }
        
        foreach (CActionInfo info in tempActionList)
        {
            m_actionQueue.Enqueue(info);
        }
        m_uiActionQueue.OnUIUpdate();
    }


    [SerializeField]
    static CEntity m_selectedEntity; //选中的对象
    public static CEntity SelectedEntity
    {
        get { return m_selectedEntity; }
        set
        {
            value.OnSelected();
            m_selectedEntity = value;
        }
    }

    static CCharacter m_activeCharacter; //行动的对象
    static public CCharacter ActiveCharacter
    {
        get { return m_activeCharacter; }
        set
        {
            //value.OnActive();
            if (value == null)
            {
                CLogManager.LogInfo("现在没有人行动");
            }
            else
            {
                CLogManager.LogInfo($"现在{value.Name}开始行动了");
            }
            m_activeCharacter = value;
        }
    }
    void Start()
    {
        //init grid
        Vector3 cellSize = new Vector3(1, 1, 1);
        Vector3 cellGap = new Vector3(0, 0, 0);
        m_grid3D = gameObject.AddComponent<CGrid3D>();
        m_grid3D.Init(cellSize, cellGap);

        //init terrain
        if (m_terrainCreator == null)
        {
            gameObject.AddComponent<CTerrainCreator>();
            CLogManager.LogError("没有添加CTerrainCreator");
        }
        CTerrainCreator.CTerrainInfo info = new CTerrainCreator.CTerrainInfo(m_xLen, m_zLen);
        m_terrainCreator.generateGrids(info, out m_terrainList);

        //init character
        if (m_characterCreator == null)
        {
            gameObject.AddComponent<CCharacterCreator>();
            CLogManager.LogError("没有添加CCharacterCreator");
        }
        m_characterCreator.Init(out m_characterList, out m_actionQueue);
        m_uiActionQueue = GameObject.Find(CGlobal.GamePath.ActionQueue).GetComponent<CUIActionQueue>();
    }

    private void Update()
    {
        //if (CStateManager.CurrentState == CStateManager.EState.MyTeamAct)
        //    ShowMoveableAreaSlice();
    }

    IMove CurrentMove;
    public void OnPreAction(IMove move)
    {
        UpdateAttackableArea(move);
        CStateManager.CurrentState = CStateManager.EState.MyTeamAct_attack;
        CurrentMove = move;
    }
    public void OnAction(Vector3Int pos) //update:aoe
    {
        List<CCharacter> characters_onAttack = new List<CCharacter>();

        CTerrainEntity terrain = GetTerrainEntity(pos);
        if (terrain && terrain.CharacterOn)
        {
            characters_onAttack.Add(terrain.CharacterOn);
        }

        foreach(CCharacter character in characters_onAttack)
        {
            ActiveCharacter.Attack(character, CurrentMove.ID);
        }
        //// take actions
        //if (action.m_obj.Name == "妙蛙种子")
        //    action.m_obj.Attack(m_characterList[1], 0);
        //else
        //    action.m_obj.Attack(m_characterList[0], 0);

        //action.m_obj.OnUpdate();

        /////////////////////

        //if (action.m_obj.Live)
        //{
        //    action.m_turn++;
        //    m_actionQueue.Enqueue(action);
        //}
        OnPostAction();
    }
    public void OnPostAction()
    {
        ClearAllArea(CTerrainEntity.ETerrainStatus.Attackable);
        ClearAllArea(CTerrainEntity.ETerrainStatus.Attackarea);
        CStateManager.CurrentState = CStateManager.EState.MyTeamAct_Standby;
        CurrentMove = null;
    }

    public void TransferData(EMessageType type, object info)
    {
        if (type == EMessageType.GetFocusCellPosition)
        {
            if (!(info is MessageInfo.CellPosition))
            {
                CLogManager.LogError("EMessageType.GetFocusCellPosition error info type");
                return;
            }
            MessageInfo.CellPosition cellPos = (MessageInfo.CellPosition)info;
            //cellPos.pos = m_grid.CellToWorld(m_selectedEntity.m_Pos);
        }
        else if (type == EMessageType.SetSelectedEntity) //鼠标单击
        {
            if (!(info is CEntity))
            {
                CLogManager.LogError("EMessageType.SetSelectedEntity error info type");
                return;
            }
            SelectedEntity = (CEntity)info;
            if (CStateManager.CurrentState == CStateManager.EState.Start) //修改：默认进入MyTeamAct_default
            {
                if (SelectedEntity is CCharacter && ((CCharacter)SelectedEntity).Team == 0)
                {
                    CStateManager.CurrentState = CStateManager.EState.MyTeamAct_Standby;
                    ActiveCharacter = (CCharacter)SelectedEntity;
                }
            }
            else if (CStateManager.CurrentState == CStateManager.EState.MyTeamAct_Standby)
            {
                if (SelectedEntity is CTerrainEntity/* && ((CTerrainEntity)M_selectedEntity).m_moveCost != -1*/)
                {
                    int rest_movement = ActiveCharacter.ActRepo.GetRestPoint(EAction.Movement);
                    if (rest_movement <= 0 || ActiveCharacter.State != CCharacter.CState.EState.Act_Enabled)
                    {
                        return;
                    }
                    if (((CTerrainEntity)SelectedEntity).CharacterOn) return; //有人了
                    List<Vector3Int> path = GetOptimalPath(ActiveCharacter.Pos, SelectedEntity.Pos, rest_movement);

                    //string log = $"{M_ActiveEntity}移动路径:";
                    //foreach (Vector3Int step in path) log += $"({step.x},{step.y},{step.z})";
                    //CLogManager.AddLog(log, CLogManager.ELogLevel.Debug);

                    if (path.Count > 0)
                    {
                        StartCoroutine(ActiveCharacter.MoveOnDelay(path));
                        
                        //if (M_ActiveEntity.MoveOn(((CTerrainEntity)M_selectedEntity).m_Pos + Vector3Int.up))//移动了
                        //{
                        //    foreach (Vector3Int step in path)
                        //    {
                        //        GetTerrainEntity(step).m_gridSlice.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0.9289f, 0.6862f);
                        //    }
                        //}
                    }
                    else
                    {
                        Vector3Int pos = SelectedEntity.Pos;
                        CLogManager.LogDebug($"{ActiveCharacter.Name}无法移动到位置({pos.x}, {pos.z})");
                    }
                }
            }
            else if (CStateManager.CurrentState == CStateManager.EState.MyTeamAct_attack)
            {
                CTerrainEntity terrain;
                if (SelectedEntity is CCharacter)
                    terrain = GetTerrainEntity(((CCharacter)SelectedEntity).Pos);
                else
                    terrain = (CTerrainEntity)SelectedEntity;
                if (terrain.HasTerrainStatus(CTerrainEntity.ETerrainStatus.Attackable))
                {
                    OnAction(terrain.Pos);
                }
            }
        }
        else if(type == EMessageType.HoverEntity)
        {
            if (!(info is CEntity))
            {
                return;
            }
            if (CStateManager.CurrentState == CStateManager.EState.MyTeamAct_Standby)
            {
                if (!(info is CTerrainEntity))
                {
                    return;
                }
                if (ActiveCharacter == null)
                {
                    return;
                }
                if (ActiveCharacter.State != CCharacter.CState.EState.Act_Enabled)
                {
                    ActiveCharacter.RemoveShadow();
                    return;
                }
                if (!IsGridStandable(((CTerrainEntity)info).Pos))
                {
                    //CLogManager.LogInfo($"pos({((CTerrainEntity)info).Pos.x}, {((CTerrainEntity)info).Pos.y})当前不可作为移动目的地");   
                    return;
                }
                int rest_movement = ActiveCharacter.ActRepo.GetRestPoint(EAction.Movement);
                List<Vector3Int> path = GetOptimalPath(ActiveCharacter.Pos, ((CTerrainEntity)info).Pos, rest_movement);
                if (path.Count > 0)
                {
                    ActiveCharacter.MakeShadow(((CTerrainEntity)info).Pos);
                }
            }
            else if (CStateManager.CurrentState == CStateManager.EState.MyTeamAct_attack)
            {
                if (CurrentMove == null)
                {
                    return;
                }
                CTerrainEntity terrain;
                if (info is CCharacter)
                    terrain = GetTerrainEntity(((CCharacter)info).Pos);
                else
                    terrain = (CTerrainEntity)info;
                if (terrain.HasTerrainStatus(CTerrainEntity.ETerrainStatus.Attackable))
                {
                    UpdateAttackTargetArea(terrain.Pos, CurrentMove);
                }
            }
        }
        else
        {
            CLogManager.LogError("CLevelManager.TransferData No this action");
        }
    }

    List<Vector3Int> GetOptimalPath(Vector3Int src, Vector3Int dst, int rest_movement)
    {
        List<Vector3Int> path;
        List<CTerrainEntity> area;
        AstarPathFinding(src, dst, rest_movement, out path, out area);
        return path;
    }
    List<CTerrainEntity> GetArea(Vector3Int src, Vector3Int dst, int rest_movement)
    {
        List<Vector3Int> path;
        List<CTerrainEntity> area;
        AstarPathFinding(src, dst, rest_movement, out path, out area);
        return area;
    }
    void AstarPathFinding(Vector3Int src, Vector3Int dst, int rest_movement, out List<Vector3Int> path, out List<CTerrainEntity> area)
    {
        List<AStarGrid> close = new List<AStarGrid>();  //已求出的最优节点
        List<AStarGrid> open = new List<AStarGrid>();   //备选的最佳节点
        path = new List<Vector3Int>();
        area = new List<CTerrainEntity>();
        {
            AStarGrid src_grid = new AStarGrid(src, 0, Util.ManhattanDistance(src, dst));
            open.Add(src_grid);
        }
        while (open.Count > 0)
        {
            //找到F值最小的
            open.Sort();
            AStarGrid cur_grid = open[0];
            open.RemoveAt(0);
            //CLogManager.AddLog($"open cur: {cur_grid.m_pos.x}, {cur_grid.m_pos.z}", CLogManager.ELogLevel.Debug);

            if (cur_grid.m_pos != src) area.Add(GetTerrainEntity(cur_grid.m_pos));

            if (cur_grid.m_pos.x == dst.x && cur_grid.m_pos.z == dst.z)
            {
                //found
                while (cur_grid != null)
                {
                    path.Add(cur_grid.m_pos);
                    cur_grid = cur_grid.m_parent;
                }
                path.Reverse();
                return;
            }
            if (close.Contains(cur_grid))
            {
                continue;
            }
            close.Add(cur_grid);

            //找F值最小节点的邻接节点
            foreach (Vector3Int offset in AStarGrid.offsets)
            {
                Vector3Int new_pos = cur_grid.m_pos + offset;
                CTerrainEntity new_terrain = GetTerrainEntity(new_pos);
                if (new_terrain == null || new_terrain.MovingCost == -1)
                {
                    continue;
                }

                int new_gcost = cur_grid.m_GCost + new_terrain.MovingCost;
                if (new_terrain.CharacterOn && ActiveCharacter && new_terrain.CharacterOn.Team != ActiveCharacter.Team) //被其他队挡住，条件有待更改
                    continue;
                int new_hcost = Util.ManhattanDistance(new_pos, dst);

                AStarGrid new_grid = new AStarGrid(new_pos, new_gcost, new_hcost);
                new_grid.m_parent = cur_grid;

                if (new_grid.m_GCost > rest_movement)
                {
                    continue;
                }
                if (new_grid.FindInList(close) != null)
                {
                    continue;
                }
                AStarGrid same_grid_in_open_list = new_grid.FindInList(open);
                if (same_grid_in_open_list != null)
                {
                    if (same_grid_in_open_list.m_FCost <= new_grid.m_FCost)
                    {
                        continue;
                    }
                    else
                    {
                        same_grid_in_open_list.m_GCost = new_grid.m_GCost;
                        same_grid_in_open_list.m_HCost = new_grid.m_HCost;
                        same_grid_in_open_list.m_parent = cur_grid;
                    }
                }
                else
                {
                    open.Add(new_grid);
                }
            }
        }
    }
    //CTerrainEntity UpdateCharacterArea(CCharacter character, Vector3Int pos)
    //{
    //    CTerrainEntity terrain = GetTerrainEntity(pos);
    //    if (terrain)
    //    {
    //        terrain.CharacterOn = character;
    //    }
    //    return terrain;
    //}
    //CTerrainEntity OnUnbindCharacterTerrain(CCharacter character, Vector3Int pos)
    //{
    //    CTerrainEntity terrain = GetTerrainEntity(pos);
    //    if (terrain && terrain.CharacterOn == character)
    //    {
    //        terrain.CharacterOn = null;
    //    }
    //    return terrain;
    //}
    public void UpdateCharacterArea(CCharacter character, Vector3Int pos, bool bind)
    {
        CTerrainEntity terrain = GetTerrainEntity(pos);
        if (!terrain) return;
        if (bind)
        {
            if (terrain.CharacterOn != null)
            {
                CLogManager.LogWarning($"UpdateCharacterArea bind, ({pos.x},{pos.z})已经存在角色{terrain.CharacterOn.Name},{character.Name}无法进入");
                return;
            }
            terrain.CharacterOn = character;

            if(character.Team == 0)
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Myteam);
            else if(character.Team > 0)
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Allay);
            else
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Enemy);
        }
        else
        {
            if (terrain.CharacterOn && terrain.CharacterOn != character)
            {
                CLogManager.LogWarning($"UpdateCharacterArea unbind, ({pos.x},{pos.z})上的角色并不是{character.Name},而是{terrain.CharacterOn.Name}");
                return;
            }
            terrain.CharacterOn = null;
            if (character.Team == 0)
                terrain.RemoveTerrainStatus(CTerrainEntity.ETerrainStatus.Myteam);
            else if (character.Team > 0)
                terrain.RemoveTerrainStatus(CTerrainEntity.ETerrainStatus.Allay);
            else
                terrain.RemoveTerrainStatus(CTerrainEntity.ETerrainStatus.Enemy);
        }
    }
    public void UpdateMoveableArea(CCharacter character)
    {
        CLogManager.LogInfo("UpdateMoveableArea");
        if (!character) return;
        ClearAllArea(CTerrainEntity.ETerrainStatus.Moveable);
        int rest_movement = character.ActRepo.GetRestPoint(EAction.Movement);
        List<CTerrainEntity> area = GetArea(character.Pos, new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), rest_movement);
        
        foreach (CTerrainEntity terrain in area)
        {
            if (IsGridStandable(terrain.Pos))
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Moveable);
        }
    }
    public void UpdateAttackableArea(IMove move)
    {
        if (!ActiveCharacter) return;
        ClearAllArea(CTerrainEntity.ETerrainStatus.Attackable);
        List<Vector3Int> range = move.Range;
        foreach(Vector3Int pos in range)
        {
            CTerrainEntity terrain = GetTerrainEntity(ActiveCharacter.Pos + pos);
            if (terrain != null)
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Attackable);
        }
        CLogManager.LogInfo($"{ActiveCharacter.Name}准备使用{move.Name}");
    }
    public void UpdateAttackTargetArea(Vector3Int target, IMove move)
    {
        if (!ActiveCharacter) return;
        ClearAllArea(CTerrainEntity.ETerrainStatus.Attackarea);
        List<Vector3Int> aoe = move.AOE;
        foreach (Vector3Int pos in aoe)
        {
            CTerrainEntity terrain = GetTerrainEntity(target + pos);
            if (terrain != null)
                terrain.AddTerrainStatus(CTerrainEntity.ETerrainStatus.Attackarea);
        }
    }
    public void ClearAllArea(CTerrainEntity.ETerrainStatus status = CTerrainEntity.ETerrainStatus.Common)
    {
        foreach (CTerrainEntity terrain in m_terrainList)
        {
            terrain.RemoveTerrainStatus(status);
        }
    }
    public bool IsGridStandable(Vector3Int targetPos)
    {
        if (GetTerrainEntity(targetPos).MovingCost == -1)
        {
            //CLogManager.LogInfo("该位置不可停留");
            return false;
        }
        foreach(CCharacter character in m_characterList)
        {
            if (character.Pos == targetPos)
            {
                //CLogManager.LogInfo("该位置已经有其他角色");
                return false;
            }
        }

        return true;
    }
}
public interface IAreaUpdater
{
    void UpdateCharacterArea(CCharacter character, Vector3Int pos, bool bind);
    void UpdateMoveableArea(CCharacter character);
    void UpdateAttackableArea(IMove move);//修改输入character
    void UpdateAttackTargetArea(Vector3Int target, IMove move);
    void ClearAllArea(CTerrainEntity.ETerrainStatus status = CTerrainEntity.ETerrainStatus.Common);
}
public interface IActionSink
{
    void OnPreAction(IMove move);
    void OnAction(Vector3Int pos);
    void OnPostAction();

}
public struct CActionInfo : IComparable<CActionInfo>
{
    public int Turn;
    public CCharacter Obj;

    public int CompareTo(CActionInfo other)
    {
        //      如果返回值小于 0，表示当前对象小于另一个对象。小的优先
        if (object.ReferenceEquals(other, null)) return -1;
        if (Turn != other.Turn) return Turn - other.Turn; //回合数小的先动

        return other.Obj.Dex - Obj.Dex; //敏捷值高的先动
    }
}

class AStarGrid : IComparable<AStarGrid>
{
    public Vector3Int m_pos;
    public int m_GCost;//从起点到当前节点的实际代价
    public int m_HCost;//从当前节点到目标节点的预估代价
    public int m_FCost => m_GCost + m_HCost;
    public AStarGrid m_parent;

    public AStarGrid(Vector3Int pos, int GCost, int HCost)
    {
        m_pos = pos;
        m_GCost = GCost;
        m_HCost = HCost;
    }
    public int CompareTo(AStarGrid other)
    {
        return m_FCost.CompareTo(other.m_FCost);
    }
    public AStarGrid FindInList(List<AStarGrid> grid_list)
    {
        if (grid_list == null) return null;
        foreach (AStarGrid grid in grid_list)
        {
            if (grid.m_pos == m_pos) return grid;
        }
        return null;
    }
    static public Vector3Int[] offsets = { Vector3Int.left, Vector3Int.right, Vector3Int.back, Vector3Int.forward };

}

public interface IActionQueue
{
    PriorityQueue<CActionInfo> ActionQueue { get; }
    void ActionInit();
    void ActionUninit();
    void OnNextTurn();
    void OnUpdateQueue(List<CActionInfo> newActions);
}
