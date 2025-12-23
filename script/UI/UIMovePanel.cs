using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CUIMovePanel : MonoBehaviour, IUIMovePanel
{
    IMoveRepo m_moveRepo;
    IActionSink m_useMoveSink;
    GameObject m_iconPrefab;
    Dictionary<IMove, GameObject> m_icons;
    public int displayMaxRow = 18;
    public int displayMaxCol = 2;
    private void Start()
    {
        m_iconPrefab = Resources.Load<GameObject>(CGlobal.ResPath.Prefab_Image_MoveIcon);
        m_useMoveSink = GameObject.Find(CGlobal.GamePath.Director).GetComponent<CLevelManager>();
        m_icons = new Dictionary<IMove, GameObject>();
        gameObject.SetActive(false);
    }
    public void OnUIShow(IMoveRepo moveRepo)
    {
        foreach (GameObject obj in m_icons.Values) Destroy(obj);
        m_icons.Clear();
        m_moveRepo = moveRepo;
        int displayNum = m_moveRepo.Repo.Count;
        float icon_width = m_iconPrefab.GetComponent<RectTransform>().rect.width;
        float icon_height = m_iconPrefab.GetComponent<RectTransform>().rect.height;
        int idx = 0;
        foreach (IMove move in m_moveRepo.Repo.Values)
        {
            GameObject icon = Instantiate(m_iconPrefab, transform);
            icon.GetComponent<Button>().image.sprite = move.Icon;
            icon.GetComponentInChildren<TMPro.TMP_Text>().text = move.Name.ToString();

            Vector2 offset = Vector2.zero;
            offset.x = (2f * (idx % displayMaxRow) + 1 - displayMaxRow) * icon_width / 2;
            offset.y = (-2f * (idx / displayMaxRow) - 1 + displayMaxCol) * icon_height / 2;
            icon.GetComponent<RectTransform>().anchoredPosition = offset;

            icon.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(move));

            m_icons.Add(move, icon);

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

    }

    void OnButtonClick(IMove move)
    {
        m_useMoveSink.OnPreAction(move);
    }
}
public interface IUIMovePanel
{
    void OnUIShow(IMoveRepo moveRepo);
    void OnUIHide();
    void OnUIUpdate();
}