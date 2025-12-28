using UnityEngine;
using System.Collections.Generic;

public class CCharacterCreator : MonoBehaviour
{
    public void Init(out List<CCharacter> characterList, out PriorityQueue<CActionInfo> actionQueue)
    {
        characterList = new List<CCharacter>();
        actionQueue = new PriorityQueue<CActionInfo>();

        Init_test1(characterList, actionQueue);
    }
    void Init_test1(List<CCharacter> characterList, PriorityQueue<CActionInfo> actionQueue)
    {
        CCharacter zhongzi = GameObject.Find("Character/0001").GetComponent<CCharacter>();
        zhongzi.Name = "妙蛙种子";
        zhongzi.Hp = 16;
        zhongzi.Dex = 10;
        zhongzi.MoveRepo.UpdateMove(EMove.Tackle, new NSMove.CTackle()); //�Ĺ���ģʽ
        zhongzi.MoveRepo.UpdateMove(EMove.Ember, new NSMove.CEmber()); //�Ĺ���ģʽ
        //zhongzi.StatusRepo.UpdateStatue(EStatus.Type_GrassBug);
        zhongzi.ActRepo.UpdateRepoFull(EAction.Movement, CActionCreater.CreateAction(EAction.Movement, 9));
        zhongzi.ActRepo.UpdateRepoFull(EAction.Action, CActionCreater.CreateAction(EAction.Action, 1));
        zhongzi.ActRepo.UpdateRepoFull(EAction.BonusAction, CActionCreater.CreateAction(EAction.BonusAction, 1));

        zhongzi.Spawn(new Vector3Int(5, 0, 0));



        CCharacter xiaolada = GameObject.Find("Character/0019").GetComponent<CCharacter>();
        xiaolada.Name = "小拉达";
        xiaolada.Hp = 13;
        xiaolada.Dex = 9;
        xiaolada.MoveRepo.UpdateMove(EMove.Tackle, new NSMove.CTackle());
        xiaolada.ActRepo.UpdateRepoFull(EAction.Movement, CActionCreater.CreateAction(EAction.Movement, 10));
        xiaolada.ActRepo.UpdateRepoFull(EAction.Action, CActionCreater.CreateAction(EAction.Action, 1));
        xiaolada.ActRepo.UpdateRepoFull(EAction.BonusAction, CActionCreater.CreateAction(EAction.BonusAction, 1));
        xiaolada.Spawn(new Vector3Int(5, 0, 5));

        characterList.Add(zhongzi);
        characterList.Add(xiaolada);

        foreach (CCharacter item in characterList)
        {
            CActionInfo info = new CActionInfo();
            info.Obj = item;
            info.Turn = 0;
            actionQueue.Enqueue(info);
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
