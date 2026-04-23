using UnityEngine;

public class UITextPulse : MonoBehaviour
{
    [Header("跳动动画设置")]
    [Tooltip("瞬间放大的倍数")]
    public float pulseScale = 1.3f;
    [Tooltip("回弹缩小的平滑速度")]
    public float recoverSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        // 记录 UI 初始的大小
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // 平滑地从当前大小向目标大小（初始大小）过渡
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * recoverSpeed);
        }
    }

    // 暴露给 GameManager 调用的触发方法
    public void TriggerPulse()
    {
        // 瞬间放大
        transform.localScale = originalScale * pulseScale;
        // 目标设回原大小，Update 会负责把它平滑缩回来
        targetScale = originalScale;
    }
}