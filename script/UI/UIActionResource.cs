using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CUIActionResource : MonoBehaviour, IUIActionResource
{
    IActRepo m_actRepo;
    GameObject m_iconPrefab;
    Dictionary<IAction, GameObject> m_icons;
    public uint displayMaxNum = 10u;
    private void Start()
    {
        m_iconPrefab = Resources.Load<GameObject>(CGlobal.ResPath.Prefab_Image_ActionIcon);
        m_icons = new Dictionary<IAction, GameObject>();
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (m_actRepo != null)
        {
            OnUIUpdate();
        }
    }
    public void OnUIShow(IActRepo actRepo)
    {
        m_actRepo = actRepo;
        foreach (GameObject icon in m_icons.Values) Destroy(icon);
        m_icons.Clear();
        int displayNum = m_actRepo.Repo.Count;
        float icon_width = m_iconPrefab.GetComponent<RectTransform>().rect.width;
        int idx = 0;
        foreach (IAction action in m_actRepo.Repo.Values)
        {
            GameObject icon = Instantiate(m_iconPrefab, transform);
            icon.GetComponent<Image>().sprite = action.Icon;
            icon.GetComponentInChildren<TMPro.TMP_Text>().text = action.RestPoint.ToString();

            Vector2 offset = Vector2.zero;
            offset.x = (2f * idx + 1 - displayNum) * icon_width / 2;
            icon.GetComponent<RectTransform>().anchoredPosition = offset;

            m_icons.Add(action, icon);
            idx++;
        }
        gameObject.SetActive(true);
    }
    public void OnUIHide()
    {
        gameObject.SetActive(false);
    }
    public void OnUIUpdate()
    {
        foreach (IAction action in m_actRepo.Repo.Values)
        {
            GameObject icon = m_icons[action];
            icon.GetComponentInChildren<TMPro.TMP_Text>().text = action.RestPoint.ToString();
        }
    }
}

public interface IUIActionResource
{
    void OnUIShow(IActRepo actRepo);
    void OnUIHide();
    void OnUIUpdate();
}