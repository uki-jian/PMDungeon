using UnityEngine;

public class testBasic : MonoBehaviour
{
    public CEntity zhongzi;
    public CEntity huolong;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        zhongzi.m_moveList.Add(new CTackle()); //改工厂模式
        huolong.m_moveList.Add(new CEmber());

        huolong.Spawn(huolong.m_Pos);
        zhongzi.Spawn(zhongzi.m_Pos);
    }

    // Update is called once per frame
    void Update()
    {
        huolong.Attack(zhongzi, 0);
        zhongzi.Attack(huolong, 0);

        huolong.OnUpdate();
        zhongzi.OnUpdate();
    }
}
