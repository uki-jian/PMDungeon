using UnityEngine;

namespace MessageInfo
{
    //用class的原因是传引用
    public class CameraPTZFInfo
    {
        public float panX;
        public float panY;
        //public float tiltX;
        //public float tiltY;
        public float zoom;
        public bool bFocus;
        public Vector3 vFocus;
        public int rotateDir;   //>1 顺时针 <1逆时针
        public float deltaTime;
    }
    public class CellPosition
    {
        public Vector3 pos;
    }
}
public enum EMessageType
{
    SetActiveCellPosition = 0, //Vector3
    CameraPTZF,           //MessageInfo.CameraPTZFInfo
    GetFocusCellPosition,    //MessageInfo.CellPosition
    SetSelectedEntity,
}
public interface IPipe
{
    void TransferData(EMessageType type, object info);
    //void TransferDataRef(EMessageType type, ref object info);
}
public class GameDirector : MonoBehaviour
{
    CLogManager logMgr;
    CLevelManager levelMgr;
    CInputManager inputMgr;
    [SerializeField]
    CCameraManager cameraMgr;

    CStateManager stateMgr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logMgr = gameObject.AddComponent<CLogManager>();
        levelMgr = gameObject.AddComponent<CLevelManager>();
        inputMgr = gameObject.AddComponent<CInputManager>();
        stateMgr = gameObject.AddComponent<CStateManager>();

        inputMgr.m_pipeLevel = levelMgr;
        inputMgr.m_pipeCamera = cameraMgr;

        CStateManager.CurrentState = CStateManager.EState.Start;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
