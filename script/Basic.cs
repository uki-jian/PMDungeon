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
    void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info);
}
public class CMove
{
    public EMove m_id { get; set; }
    public string m_name { get; set; }
    public int m_power { get; set; }
    public List<Vector2Int> m_range { get; set; }
    public int m_accurancy;
    public CMove(EMove id, string name, int power, List<Vector2Int> range, int accurancy)
    {
        m_id = id;
        m_name = name;
        m_power = power;
        m_range = range;
        m_accurancy = accurancy;
    }
}

public static class CRangeList
{
    public static List<Vector2Int> Range_Front { get; }
    public static List<Vector2Int> Range_Ray_4 { get; }

    static CRangeList()
    {
        Range_Front = new List<Vector2Int>();
        Range_Front.Add(Vector2Int.up);

        Range_Ray_4 = new List<Vector2Int>();
        for(int i=1; i<=4; i++) Range_Ray_4.Add(new Vector2Int(0,i));
    }
}

public class CTackle : CMove, IMove
{
    public void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info)
    {
        dst.m_Hp -= m_power;
        CLogManager.AddLog($"{src.m_name}使用了{m_name}，{dst.m_name}受到了{m_power}点伤害，剩余{dst.m_Hp}HP！");
    }
    public CTackle() : base(EMove.Tackle, "撞击", power: 5, CRangeList.Range_Front, 100)
    {
        
    }
}
public class CEmber : CMove, IMove
{
    public void OnAttack(CCharacter src, CCharacter dst, CMoveExtraInfo info)
    {
        dst.m_Hp -= m_power;
        CLogManager.AddLog($"{src.m_name}使用了{m_name}，{dst.m_name}受到了{m_power}点伤害，剩余{dst.m_Hp}HP！");
        if (dst.m_type1 == EType.Fire || dst.m_type2 == EType.Fire)
        {
            CLogManager.AddLog($"{dst.m_name}的火属性使燃烧状态无效了！");
            return;
        }
        //同种状态应该更新

        if (dst.m_effectDict.ContainsKey(EEffect.Burn))
        {
            CLogManager.AddLog($"{dst.m_name}已经陷入了燃烧状态");
            dst.m_effectDict[EEffect.Burn].Refresh();
        }
        else
        {
            dst.m_effectDict.Add(EEffect.Burn, new CBurn(dst));
        }
    }
    public CEmber() : base(EMove.Ember, "火花", power: 3, CRangeList.Range_Ray_4, 100)
    {
        
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
