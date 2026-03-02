using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 8f, -10f);
    public float smoothTime = 0.15f;

    [Header("Look")]
    public bool lookAtTarget = true;
    public Vector3 lookOffset = new Vector3(0f, 1.0f, 0f);

    Vector3 _vel;

    void LateUpdate()
    {
        if (target == null) return;

        // 平滑跟随位置
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, smoothTime);

        // 看向目标（不抖）
        if (lookAtTarget)
        {
            Vector3 lookPos = target.position + lookOffset;
            transform.rotation = Quaternion.LookRotation(lookPos - transform.position, Vector3.up);
        }
    }
}