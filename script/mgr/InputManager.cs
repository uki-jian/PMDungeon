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
            do
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    //Vector3 worldPos = hit.point;
                    //m_pipeLevel.TransferData(EMessageType.SetActiveCellPosition, worldPos);

                    GameObject selectedObject = hit.collider.gameObject;
                    CEntity entity = selectedObject.GetComponent<CEntity>();
                    if (entity == null)
                    {
                        CLogManager.AddLog("select a gameobject with no CEntity base", CLogManager.ELogLevel.Warning);
                        break;
                    }
                    m_pipeLevel.TransferData(EMessageType.SetSelectedEntity, entity);

                }
            } while (false);


        //camera input
        {
            MessageInfo.CameraPTZFInfo cameraPTZFInfo = new MessageInfo.CameraPTZFInfo();

            //相机平移
            cameraPTZFInfo.panX = Input.GetAxis("Horizontal");
            cameraPTZFInfo.panY = Input.GetAxis("Vertical");

            //相机远近缩放
            cameraPTZFInfo.zoom = Input.GetAxis("Mouse ScrollWheel");

            ////相机自转
            //float mouseX = 0, mouseY = 0;
            //if (Input.GetMouseButton(2))
            //{
            //    cameraPTZFInfo.tiltX = Input.GetAxis("Mouse X");
            //    cameraPTZFInfo.tiltY = Input.GetAxis("Mouse Y");
            //}

            // 相机公转
            if (Input.GetKey(KeyCode.Q))
            {
                cameraPTZFInfo.rotateDir = 1;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                cameraPTZFInfo.rotateDir = -1;
            }

            //相机聚焦
            if (Input.GetKeyDown(KeyCode.F))
            {
                cameraPTZFInfo.bFocus = true;
            }
            MessageInfo.CellPosition cellPos = new MessageInfo.CellPosition();
            m_pipeLevel.TransferData(EMessageType.GetFocusCellPosition, cellPos);
            cameraPTZFInfo.vFocus = cellPos.pos;

            cameraPTZFInfo.deltaTime = Time.deltaTime;

            m_pipeCamera.TransferData(EMessageType.CameraPTZF, cameraPTZFInfo);
        }
        
    }
}
