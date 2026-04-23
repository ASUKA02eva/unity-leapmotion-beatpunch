// 放在 Assets/Scripts 文件夹下
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseDetector : MonoBehaviour
{
    // 专门给动画机用的视觉触发器
    public static bool triggerAnimL { get; private set; }
    public static bool triggerAnimR { get; private set; }

    [Header("出拳检测设置")]
    [Tooltip("高速曲目建议调小距离，高速连打时动作幅度往往较小")]
    public float minPunchDistance = 0.05f;
    public float punchSpeedThreshold = 0.3f;

    private Vector3 leftPunchStartPos;
    private Vector3 rightPunchStartPos;
    private bool leftPunchPreparing = false;
    private bool rightPunchPreparing = false;

    // 机械锁：防止一次伸臂被算作打好几拳
    private bool hasPunchedL = false;
    private bool hasPunchedR = false;

    public GameObject handL;
    public GameObject handR;

    private Vector3 lastPositionL;
    private Vector3 lastPositionR;

    void Start()
    {
        if (handL != null) lastPositionL = handL.transform.position;
        if (handR != null) lastPositionR = handR.transform.position;
    }

    void Update()
    {
        if (handL == null || handR == null) return;

        Vector3 currentLPos = handL.transform.position;
        Vector3 currentRPos = handR.transform.position;

        float punchSpeedL = (currentLPos.z - lastPositionL.z) / Time.deltaTime;
        float punchSpeedR = (currentRPos.z - lastPositionR.z) / Time.deltaTime;

        // ================= 左拳逻辑与直接判定 =================
        if (punchSpeedL > -0.05f)
        {
            leftPunchPreparing = false;
            hasPunchedL = false;
        }
        // 【新增】：只有速度达标，且当前姿势是“左手握拳”，才开始蓄力计算
        else if (punchSpeedL < -punchSpeedThreshold && PoseJudge.isFistL)
        {
            if (!leftPunchPreparing && !hasPunchedL)
            {
                leftPunchPreparing = true;
                leftPunchStartPos = currentLPos;
            }
        }

        if (leftPunchPreparing && (leftPunchStartPos.z - currentLPos.z) >= minPunchDistance)
        {
            leftPunchPreparing = false;
            hasPunchedL = true;

            // 【新增】：到达判定距离时，再次确认依然是握拳状态
            if (PoseJudge.isFistL)
            {
                // 【核心质变】：绕过动画机，当场直接触发音符判定！0延迟！
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TryHitNote(0);
                }
                // 通知动画机播放动画
                triggerAnimL = true;
            }
            else
            {
                Debug.Log("左手挥出了距离，但手势不是握拳，判定无效！");
            }
        }

        // ================= 右拳逻辑与直接判定 =================
        if (punchSpeedR > -0.05f)
        {
            rightPunchPreparing = false;
            hasPunchedR = false;
        }
        // 【新增】：只有速度达标，且当前姿势是“右手握拳”，才开始蓄力计算
        else if (punchSpeedR < -punchSpeedThreshold && PoseJudge.isFistR)
        {
            if (!rightPunchPreparing && !hasPunchedR)
            {
                rightPunchPreparing = true;
                rightPunchStartPos = currentRPos;
            }
        }

        if (rightPunchPreparing && (rightPunchStartPos.z - currentRPos.z) >= minPunchDistance)
        {
            rightPunchPreparing = false;
            hasPunchedR = true;

            // 【新增】：到达判定距离时，再次确认依然是握拳状态
            if (PoseJudge.isFistR)
            {
                // 【核心质变】：当场判定右拳！
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TryHitNote(1);
                }
                // 通知动画机
                triggerAnimR = true;
            }
            else
            {
                Debug.Log("右手挥出了距离，但手势不是握拳，判定无效！");
            }
        }

        lastPositionL = currentLPos;
        lastPositionR = currentRPos;
    }

    // --- 动画机消费信号的方法 ---
    public static bool ConsumeAnimL()
    {
        if (triggerAnimL)
        {
            triggerAnimL = false;
            return true;
        }
        return false;
    }

    public static bool ConsumeAnimR()
    {
        if (triggerAnimR)
        {
            triggerAnimR = false;
            return true;
        }
        return false;
    }
}