using UnityEngine;

public class CInputManager : MonoBehaviour
{
    public IPipe m_pipeLevel;
    public IPipe m_pipeCamera;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 worldPos = hit.point;
                m_pipeLevel.TransferData(EMessageType.SetActiveCellPosition, worldPos);
                

            }
        }

        //相机远近缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        //相机平移
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //相机旋转
        float mouseX = 0, mouseY = 0;
        if (Input.GetMouseButton(2))
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        //相机聚焦
        bool bFocus = false;
        Vector3 vFocus = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.F))
        {
            bFocus = true;
            MessageInfo.CellPosition cellPos = new MessageInfo.CellPosition();
            m_pipeLevel.TransferData(EMessageType.GetFocusCellPosition, cellPos);
            vFocus = cellPos.pos;
        }

        MessageInfo.CameraPTZFInfo cameraPTZFInfo = new MessageInfo.CameraPTZFInfo();
        cameraPTZFInfo.panX = horizontal;
        cameraPTZFInfo.panY = vertical;
        cameraPTZFInfo.tiltX = mouseX;
        cameraPTZFInfo.tiltY = mouseY;
        cameraPTZFInfo.zoom = scroll;
        cameraPTZFInfo.bFocus = bFocus;
        cameraPTZFInfo.vFocus = vFocus;

        m_pipeCamera.TransferData(EMessageType.CameraPTZF, cameraPTZFInfo);
    }
}
