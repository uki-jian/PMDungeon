using UnityEngine;

public class CCameraManager : MonoBehaviour, IPipe
{
    // 相机移动速度
    public float moveSpeed = 5f;
    // 相机旋转速度
    public float rotateSpeed = 2f;
    // 相机缩放速度
    public float zoomSpeed = 5f;
    // 平滑时间
    public float smoothTime = 0.1f;

    private Vector3 moveVelocity;
    private Vector3 zoomVelocity;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
        Focus(new Vector3(7f, 0f, 7f));
    }

    void Pan(float x, float y)
    {
        Vector3 moveDirection = (transform.right * x + transform.forward * y).normalized;
        Vector3 targetMovePosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, targetMovePosition, ref moveVelocity, smoothTime);
    }
    void Tilt(float x, float y)
    {
        targetRotation *= Quaternion.Euler(-y * rotateSpeed, x * rotateSpeed, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothTime);
    }
    void Zoom(float z)
    {
        Vector3 targetPosition = transform.position + transform.forward * z * zoomSpeed;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }
    void Focus(Vector3 targetPosition)
    {
        // 获取相机的前向向量
        Vector3 cameraForward = transform.forward;
        // 计算相机到目标位置的向量
        Vector3 directionToTarget = targetPosition - transform.position;

        // 计算相机到目标位置在相机前向向量上的投影距离
        float distance = Vector3.Dot(directionToTarget, cameraForward);

        // 计算相机应该移动到的新位置
        Vector3 newPosition = targetPosition - cameraForward * distance;

        // 设置相机的新位置
        transform.position = newPosition;
    }
    void Update()
    {
        
    }

    public void TransferData(EMessageType type, object info)
    {
        if (type == EMessageType.CameraPTZF)
        {
            Pan(((MessageInfo.CameraPTZFInfo)info).panX, ((MessageInfo.CameraPTZFInfo)info).panY);
            Tilt(((MessageInfo.CameraPTZFInfo)info).tiltX, ((MessageInfo.CameraPTZFInfo)info).tiltY);
            Zoom(((MessageInfo.CameraPTZFInfo)info).zoom);
            if(((MessageInfo.CameraPTZFInfo)info).bFocus)
            {
                Focus(((MessageInfo.CameraPTZFInfo)info).vFocus);
            }
        }

    }
}
