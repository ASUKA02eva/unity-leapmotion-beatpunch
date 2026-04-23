using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

public class JudgeTextEffect : MonoBehaviour
{
    [Header("特效动画设置")]
    public float floatSpeed = 0.5f; // 向上漂浮的速度
    public float fadeSpeed = 3f;  // 淡出的速度
    public float lifeTime = 0.8f;   // 存活时间，时间到了自动销毁

    private TextMeshPro textMesh;
    private Color textColor;

    void Start()
    {
        // 获取文字组件并记录初始颜色
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }

        // 设置定时销毁，保持内存干净
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 向上漂浮
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. 逐渐透明淡出
        if (textMesh != null && textColor.a > 0)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}