using System.Collections;
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
    // 获取攻击音效
    AudioClip GetAttackSound();
    // 获取受击音效
    AudioClip GetHitSound();
    // 获取动画预制体（如果没有动画则返回null）
    GameObject GetAnimationPrefab();
    // 是否需要播放动画（从攻击者到受击者）
    bool HasAnimation();
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
        public AudioClip GetAttackSound() { return null; }
        public AudioClip GetHitSound() { return null; }
        public GameObject GetAnimationPrefab() { return null; }
        public bool HasAnimation() { return false; }
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
            CLogManager.LogInfo($"{src.Name}使用了{Name}，{dst.Name}受到了{5}点伤害，剩余{dst.Hp}HP！");
        }
        public AudioClip GetAttackSound() { return m_attackSound; }
        public AudioClip GetHitSound() { return null; }
        public GameObject GetAnimationPrefab() { return null; }
        public bool HasAnimation() { return false; }

        static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Move_Tackle);
        static AudioClip m_attackSound = Resources.Load<AudioClip>(CGlobal.ResPath.Move_Sound_Tackle_Attack);
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
            CLogManager.LogInfo($"{src.Name}使用了{Name}，{dst.Name}受到了{3}点伤害，剩余{dst.Hp}HP！");
            IStatus status;
            if (dst.StatusRepo.GetStatus(EStatus.Type_Fire, out status))
            {
                CLogManager.LogInfo($"{dst.Name}的火属性使燃烧状态无效了！");
                return;
            }
            //同种状态应该更新

            if (dst.StatusRepo.GetStatus(EStatus.Condition_Burn, out status))
            {
                CLogManager.LogInfo($"{dst.Name}已经陷入了燃烧状态");
                status.Refresh();
            }
            else
            {
                dst.StatusRepo.UpdateStatue(EStatus.Condition_Burn, new CBurn(dst));
            }
        }
        public AudioClip GetAttackSound() { return m_attackSound; }
        public AudioClip GetHitSound() { return m_hitSound; }
        public GameObject GetAnimationPrefab() { return m_animationPrefab; }
        public bool HasAnimation() { return m_animationPrefab != null; }

        static Sprite m_icon = Resources.Load<Sprite>(CGlobal.ResPath.Move_Ember);
        static AudioClip m_attackSound = Resources.Load<AudioClip>(CGlobal.ResPath.Move_Sound_Ember_Attack);
        static AudioClip m_hitSound = Resources.Load<AudioClip>(CGlobal.ResPath.Move_Sound_Ember_Hit);
        static GameObject m_animationPrefab = Resources.Load<GameObject>(CGlobal.ResPath.Move_Prefab_Ember);
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
        IMove move = m_moves[id];
        
        // 播放攻击音效（在攻击者位置）
        AudioClip attackSound = move.GetAttackSound();
        if (attackSound != null && attacker != null)
        {
            Vector3 attackPos = attacker.transform.position;
            AudioSource.PlayClipAtPoint(attackSound, attackPos);
        }
        
        // 如果有动画，需要等待动画播放完成后再执行攻击和受击音效
        if (move.HasAnimation() && attacker != null)
        {
            // 在Character中启动协程处理动画和攻击逻辑
            attacker.StartCoroutine(CMoveRepo.PlayMoveWithAnimation(move, attacker, defencer, extraInfo));
        }
        else
        {
            // 没有动画，直接执行攻击
            move.OnAttack(attacker, defencer, extraInfo);
            
            // 播放受击音效（在受击者位置）
            AudioClip hitSound = move.GetHitSound();
            if (hitSound != null && defencer != null)
            {
                Vector3 hitPos = defencer.transform.position;
                AudioSource.PlayClipAtPoint(hitSound, hitPos);
            }
        }
    }
    
    // 静态方法，用于在Character中启动协程
    public static IEnumerator PlayMoveWithAnimation(IMove move, CCharacter attacker, CCharacter defencer, CMoveExtraInfo extraInfo)
    {
        // 获取动画预制体
        GameObject animPrefab = move.GetAnimationPrefab();
        if (animPrefab != null && attacker != null && defencer != null)
        {
            // 实例化预制体并挂载到攻击者下
            GameObject animInstance = Object.Instantiate(animPrefab, attacker.transform);
            animInstance.transform.localPosition = Vector3.zero;
            
            // 获取动画时长（优先使用Animator，其次使用Animation组件）
            float animDuration = 1f; // 默认时长
            Animator animator = animInstance.GetComponent<Animator>();
            Animation animation = animInstance.GetComponent<Animation>();
            
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // 使用Animator，获取当前播放的动画时长
                if (animator.GetCurrentAnimatorStateInfo(0).length > 0)
                {
                    animDuration = animator.GetCurrentAnimatorStateInfo(0).length;
                }
            }
            else if (animation != null && animation.clip != null)
            {
                // 使用Animation组件
                animDuration = animation.clip.length;
            }
            
            // 解除父子关系以便使用世界坐标移动
            animInstance.transform.SetParent(null);
            
            // 移动动画对象从攻击者到受击者
            Vector3 startPos = attacker.transform.position;
            Vector3 endPos = defencer.transform.position;
            
            // 移动动画对象
            float elapsed = 0f;
            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animDuration;
                // 使用世界坐标移动
                Vector3 worldPos = Vector3.Lerp(startPos, endPos, t);
                animInstance.transform.position = worldPos;
                yield return null;
            }
            
            // 确保动画对象到达目标位置
            animInstance.transform.position = endPos;
            
            // 等待动画播放完成
            if (animator != null)
            {
                // 等待Animator动画播放完成
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                {
                    yield return null;
                }
            }
            else if (animation != null)
            {
                // 等待Animation组件动画播放完成
                while (animation.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                // 如果没有动画组件，等待默认时长
                yield return new WaitForSeconds(animDuration);
            }
            
            // 动画播放完成后，执行攻击逻辑
            move.OnAttack(attacker, defencer, extraInfo);
            
            // 播放受击音效（在受击者位置）
            AudioClip hitSound = move.GetHitSound();
            if (hitSound != null && defencer != null)
            {
                Vector3 hitPos = defencer.transform.position;
                AudioSource.PlayClipAtPoint(hitSound, hitPos);
            }
            
            // 销毁动画对象实例
            Object.Destroy(animInstance);
        }
        else
        {
            // 如果没有预制体，直接执行攻击
            move.OnAttack(attacker, defencer, extraInfo);
            
            // 播放受击音效
            AudioClip hitSound = move.GetHitSound();
            if (hitSound != null && defencer != null)
            {
                Vector3 hitPos = defencer.transform.position;
                AudioSource.PlayClipAtPoint(hitSound, hitPos);
            }
        }
    }
}
