using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


public class CUIActionQueue : MonoBehaviour, IUIActionQueue
{
    IActionQueue queue;
    public uint displayMaxNum = 50u;
    bool bShow = true;
    public GameObject actionButton;
    List<GameObject> btnList;

    void Start()
    {
        queue = GameObject.Find(CGlobal.GamePath.Director).GetComponent<CLevelManager>();
        actionButton = Resources.Load<GameObject>(CGlobal.ResPath.Prefab_Button_ActionQueue);
        btnList = new List<GameObject>();
        gameObject.SetActive(false);
        OnUIShow();//debug
    }
    [ContextMenu("TEST_ADD")]
    void testAdd()
    {
        List<CActionInfo> temp = new List<CActionInfo>();
        temp.Add(queue.ActionQueue.Top);
        queue.OnUpdateQueue(temp);
        OnUIShow();
    }
    [ContextMenu("ShowUI")]
    public void OnUIShow()
    {
        bShow = true;
        gameObject.SetActive(true);
        foreach (GameObject obj in btnList) Destroy(obj);
        btnList.Clear();

        int displayNum = (int)Math.Min(queue.ActionQueue.Count, displayMaxNum); //×ÜÏÔÊ¾Êý
        float x = GetComponent<RectTransform>().rect.position.x;
        float y = GetComponent<RectTransform>().rect.position.y;
        float width = GetComponent<RectTransform>().rect.width;
        float height = GetComponent<RectTransform>().rect.height;
        //CLogManager.AddLog("x:" + x + "y:" + y + "width:" + width + "height:" + height);
        float ui_width = actionButton.GetComponent<RectTransform>().rect.width;
        float interval = ui_width/10;
        int thresholdNum = (int)((width + interval) / (ui_width + interval));
        float scale = 1f;
        if(thresholdNum < displayNum)
        {
            scale = (float)thresholdNum / displayNum;
        }
        float unitDistance = (ui_width + interval) / 2 * scale;

        for (int i = 0; i < displayNum; i++)
        {
            GameObject btn = Instantiate(actionButton, transform);
            
            btnList.Add(btn);

            btn.GetComponent<RectTransform>().transform.localScale *= scale;

            Vector2 offset = Vector2.zero;
            offset.x = (2f * i + 1 - displayNum) * unitDistance;
            btn.GetComponent<RectTransform>().anchoredPosition = offset;

            CActionInfo info = queue.ActionQueue.GetItem(i);
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = info.Obj.Name;
            btn.GetComponent<Button>().image.sprite = info.Obj.Portrait;

            btn.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(info));
        }
    }
    [ContextMenu("HideUI")]
    public void OnUIHide()
    {
        bShow = false;
        gameObject.SetActive(false);
        btnList.Clear();
    }
    [ContextMenu("UpdateUI")]
    public void OnUIUpdate()
    {
        if (!bShow) return;
    }
    void OnButtonClick(CActionInfo info)
    {
        CLevelManager.SelectedEntity = info.Obj;
    }
    
}
public interface IUIActionQueue
{
    void OnUIShow();
    void OnUIHide();
    void OnUIUpdate();
}