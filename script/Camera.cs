using UnityEngine;
using System;

public class CCameraManager : MonoBehaviour, IPipe
{
    // 相机移动速度
    public float m_moveSpeed = 80f;
    // 相机旋转速度
    public float m_rotateSpeed = 2f;
    // 相机缩放速度
    public float m_zoomSpeed = 50f;
    // 平滑时间
    public float m_smoothTime = 0.1f;
    // 默认相机高度
    public float m_defaultCameraHeight = 5f;

    private Vector3 moveVelocity;
    private Vector3 zoomVelocity;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
        Focus(new Vector3(5f, 1f, 0f));
    }

    void Pan(float x, float z)
    {
        //Vector3 moveDirection = (Vector3.right * x + Vector3.forward * z).normalized;
        Vector3 movement = (transform.right * x + transform.forward * z) * m_moveSpeed * Time.deltaTime;
        movement.y = 0f;
        Vector3 targetMovePosition = transform.position + movement;
        transform.position = Vector3.SmoothDamp(transform.position, targetMovePosition, ref moveVelocity, m_smoothTime);
    }
    void Tilt(float x, float y)
    {
        targetRotation *= Quaternion.Euler(0f/*-y * m_rotateSpeed*/, x * m_rotateSpeed * Time.deltaTime, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_smoothTime);
    }
    void Zoom(float z)
    {
        Vector3 targetPosition = transform.position + transform.forward * z * m_zoomSpeed * Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, m_smoothTime);
    }
    void Focus(Vector3 targetPosition)
    {
        Vector3 cameraForward = transform.forward;
        Vector3 directionToTarget = targetPosition - transform.position;

        Vector3 newPosition = targetPosition - cameraForward * m_defaultCameraHeight;

        transform.position = newPosition;
    }
    void FocusRotate(int rotateDir)
    {
        Vector3 axis = Vector3.up;
        Vector3 targetPosition;
        Ray ray = new Ray(transform.position , transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = hit.point;
            //Debug.Log("交点位置: " + targetPosition);
        }
        else
        {
            Func<float, Vector3> GetIntersectionPointWithPlane = (y) =>
            {
                Vector3 origin = ray.origin;
                // 射线的方向向量
                Vector3 direction = ray.direction;

                // 计算参数 t
                float t = (y - origin.y) / direction.y;

                // 计算交点坐标
                Vector3 intersection = origin + t * direction;

                return intersection;
            };
            targetPosition = GetIntersectionPointWithPlane(1f);
            //Debug.Log("射线未与物体相交" + targetPosition);
        }
        transform.RotateAround(targetPosition, axis * rotateDir, m_rotateSpeed * Time.deltaTime);

        // 恢复相机的 Y 坐标，确保高度不变
        //transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
    }
    void Update()
    {
        
    }

    public void TransferData(EMessageType type, object info)
    {
        if (type == EMessageType.CameraPTZF)
        {
            Pan(((MessageInfo.CameraPTZFInfo)info).panX, ((MessageInfo.CameraPTZFInfo)info).panY);
            //Tilt(((MessageInfo.CameraPTZFInfo)info).tiltX, ((MessageInfo.CameraPTZFInfo)info).tiltY);
            Zoom(((MessageInfo.CameraPTZFInfo)info).zoom);
            if(((MessageInfo.CameraPTZFInfo)info).bFocus)
            {
                Focus(((MessageInfo.CameraPTZFInfo)info).vFocus);
            }
            if(((MessageInfo.CameraPTZFInfo)info).rotateDir != 0)
            {
                FocusRotate(((MessageInfo.CameraPTZFInfo)info).rotateDir);
            }
        }

    }
}
