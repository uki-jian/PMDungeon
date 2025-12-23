using System.Collections.Generic;
using UnityEngine;

public struct CMoveExtraInfo
{

}

public enum EMove
{
    Plain = 0,
    Tackle,
    Ember,
    WaterGun,
    Cry
}

public interface IMove
{
    EMove ID { get; }
    string Name { get; }
    //CActRepo Cost { get; set; }
    //int Power { get; set; }
    List<Vector3Int> Range { get; set; }
    List<Vector3Int> AOE { get; set; }
    Sprite Icon { get; }
    void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info);
}
namespace NSMove
{
    public class CMovePlain : IMove
    {
        public EMove ID { get { return EMove.Plain; } }
        public string Name { get { return "无技能"; } }
        public List<Vector3Int> Range { get { return CRange.Range_Front; } set { } }
        public List<Vector3Int> AOE { get { return CRange.Range_Self; } set { } }
        public Sprite Icon { get; }
        public void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info) { }
    }



    public class CTackle : IMove
    {
        public EMove ID { get { return EMove.Tackle; } }
        public string Name { get { return "撞击"; } }
        List<Vector3Int> m_range = CRange.Range_Front;
        public List<Vector3Int> Range { get { return m_range; } set { m_range = value; } }
        List<Vector3Int> m_aoe = CRange.Range_Self;
        public List<Vector3Int> AOE { get { return m_aoe; } set { m_aoe = value; } }
        public Sprite Icon { get { return m_icon; } }
        public void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info)
        {
            dst.Hp -= 5;
            CLogManager.AddLog($"{src.Name}使用了{Name}，{dst.Name}受到了{5}点伤害，剩余{dst.Hp}HP！");
        }

        static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Move_Tackle);
    }
    public class CEmber : IMove
    {
        public EMove ID { get { return EMove.Ember; } }
        public string Name { get { return "火花"; } }
        List<Vector3Int> m_range = CRange.Range_Radiant_8;
        public List<Vector3Int> Range { get { return m_range; } set { m_range = value; } }
        List<Vector3Int> m_aoe = CRange.Range_Self_Front;
        public List<Vector3Int> AOE { get { return m_aoe; } set { m_aoe = value; } }
        public Sprite Icon { get { return m_icon; } }
        public void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info)
        {
            dst.Hp -= 3;
            CLogManager.AddLog($"{src.Name}使用了{Name}，{dst.Name}受到了{3}点伤害，剩余{dst.Hp}HP！");
            IStatus status;
            if (dst.StatusRepo.GetStatus(EStatus.Type_Fire, out status))
            {
                CLogManager.AddLog($"{dst.Name}的火属性使燃烧状态无效了！");
                return;
            }
            //同种状态应该更新

            if (dst.StatusRepo.GetStatus(EStatus.Condition_Burn, out status))
            {
                CLogManager.AddLog($"{dst.Name}已经陷入了燃烧状态");
                status.Refresh();
            }
            else
            {
                dst.StatusRepo.UpdateStatue(EStatus.Condition_Burn, new CBurn(dst));
            }
        }
        static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Move_Ember);
    }
}


public static class CRange
{
    public static List<Vector3Int> Range_Self { get; }
    public static List<Vector3Int> Range_Front { get; }
    public static List<Vector3Int> Range_Self_Front { get; }

    public static List<Vector3Int> Range_Radiant_8 { get; }

    static CRange()
    {
        Range_Self = new List<Vector3Int>();
        Range_Self.Add(Vector3Int.zero);

        Range_Front = new List<Vector3Int>();
        Range_Front.Add(new Vector3Int(0, 0, 1));
        Range_Front.Add(new Vector3Int(0, 0, -1));
        Range_Front.Add(new Vector3Int(1, 0, 0));
        Range_Front.Add(new Vector3Int(-1, 0, 0));

        Range_Self_Front = new List<Vector3Int>();
        Range_Self_Front.AddRange(Range_Self);
        Range_Self_Front.AddRange(Range_Front);

        Range_Radiant_8 = Range_Radiant(8);


    }
    static List<Vector3Int> Range_Radiant(int dist)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        for (int i = -dist; i <= dist; i++)
        {
            for (int j = System.Math.Abs(i) - dist; j <= dist - System.Math.Abs(i); j++)
            {
                if (i == 0 && j == 0) continue;
                list.Add(new Vector3Int(i, 0, j));
            }
        }
        return list;
    }
}
//public static class CMoveList
//{
//    public static CMove Move_Tackle { get; }
//    public static CMove Move_Ember { get; }
//    public static CMove Move_WaterGun { get; }
//    public static CMove Move_Cry { get; }
//    static CMoveList()
//    {
//        Move_Tackle = new CMove(EMove.Tackle, "撞击", power: 5, CRangeList.Range_Front, 100);

//        Move_Ember = new CMove(EMove.Ember, "火花", power: 3, CRangeList.Range_Ray_4, 100);
//        //Move_Ember.onMoveEffect = CMoveEffect

//        Move_WaterGun = new CMove(EMove.WaterGun, "水枪", power: 3, CRangeList.Range_Ray_4, 100);

//        Move_Cry = new CMove(EMove.Cry, "叫声", power: 0, CRangeList.Range_Front, 100);
//        Move_Cry.onMoveEffect = CMoveEffect.OnAtkD1;
//    }
//}
public interface IMoveRepo
{
    Dictionary<EMove, IMove> Repo { get; }
}
public class CMoveRepo : IMoveRepo
{
    Dictionary<EMove, IMove> m_moves;
    public Dictionary<EMove, IMove> Repo { get { return m_moves; } }
    public CMoveRepo()
    {
        m_moves = new Dictionary<EMove, IMove>();
    }
    public void UpdateMove(EMove id, IMove move)
    {
        if (m_moves.ContainsKey(id))
        {
            m_moves.Remove(id);
        }
        m_moves.Add(id, move);
    }
    public bool GetMove(EMove id, out IMove move)
    {
        if (m_moves.ContainsKey(id))
        {
            move = m_moves[id];
            return true;
        }
        move = null;
        return false;
    }
    public void OnAttack(CCharacter attacker, CCharacter defencer, EMove id, CMoveExtraInfo extraInfo)
    {
        if (!m_moves.ContainsKey(id)) return;
        m_moves[id].OnAttack(attacker, defencer, extraInfo);
    }
}