using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    // ================= 供给外部消费的静态信号 =================
    public static bool triggerSwipeLeft { get; private set; }
    public static bool triggerSwipeRight { get; private set; }
    public static bool triggerOK { get; private set; }
    public static bool triggerPunchLeft { get; private set; }
    public static bool triggerPunchRight { get; private set; }

    [Header("追踪目标")]
    public GameObject handL;
    public GameObject handR;

    [Header("1. 滑动检测设置 (X轴)")]
    public float minSwipeDistance = 0.1f;
    public float swipeSpeedThreshold = 0.4f;

    [Header("2. OK确认手势设置")]
    public float okHoldTime = 2.0f;

    [Header("3. 拳击检测设置 (Z轴)")]
    public float minPunchDistance = 0.05f;
    public float punchSpeedThreshold = 0.3f;

    private Vector3 lastPositionL;
    private Vector3 lastPositionR;

    private Vector3 leftSwipeStartPos;
    private bool isSwipingL = false;
    private bool hasSwipedL = false;

    private Vector3 rightSwipeStartPos;
    private bool isSwipingR = false;
    private bool hasSwipedR = false;

    private float currentOKHoldTime = 0f;
    private bool hasConfirmedOK = false;

    private Vector3 leftPunchStartPos;
    private bool isPunchingL = false;
    private bool hasPunchedL = false;

    private Vector3 rightPunchStartPos;
    private bool isPunchingR = false;
    private bool hasPunchedR = false;

    // ================= 核心修复：防止跨场景信号卡死 =================
    void Awake()
    {
        triggerSwipeLeft = false;
        triggerSwipeRight = false;
        triggerOK = false;
        triggerPunchLeft = false;
        triggerPunchRight = false;
    }

    void Start()
    {
        if (handL != null) lastPositionL = handL.transform.position;
        if (handR != null) lastPositionR = handR.transform.position;
    }

    void Update()
    {
        // 【核心修复】：如果正在黑屏过渡，或者没检测到手，直接拒绝处理任何手势，防止射线计算崩溃和误触
        if (TransitionManager.IsFading || handL == null || handR == null) return;

        Vector3 currentLPos = handL.transform.position;
        Vector3 currentRPos = handR.transform.position;

        float speedXL = (currentLPos.x - lastPositionL.x) / Time.deltaTime;
        float speedXR = (currentRPos.x - lastPositionR.x) / Time.deltaTime;
        float speedZL = (currentLPos.z - lastPositionL.z) / Time.deltaTime;
        float speedZR = (currentRPos.z - lastPositionR.z) / Time.deltaTime;

        HandleSwipeLeft(currentLPos, speedXL);
        HandleSwipeRight(currentRPos, speedXR);
        HandleOKGesture();
        HandlePunchLeft(currentLPos, speedZL);
        HandlePunchRight(currentRPos, speedZR);

        lastPositionL = currentLPos;
        lastPositionR = currentRPos;
    }



    void HandleSwipeLeft(Vector3 currentLPos, float speedXL)
    {
        if (speedXL > -0.1f) { isSwipingL = false; hasSwipedL = false; }
        else if (speedXL <= -swipeSpeedThreshold && PoseJudge.isPalmL && !isSwipingL && !hasSwipedL)
        {
            isSwipingL = true; leftSwipeStartPos = currentLPos;
        }

        if (isSwipingL && !hasSwipedL && (currentLPos.x - leftSwipeStartPos.x) <= -minSwipeDistance)
        {
            if (PoseJudge.isPalmL) { hasSwipedL = true; triggerSwipeLeft = true; }
        }
    }

    void HandleSwipeRight(Vector3 currentRPos, float speedXR)
    {
        if (speedXR < 0.1f) { isSwipingR = false; hasSwipedR = false; }
        else if (speedXR >= swipeSpeedThreshold && PoseJudge.isPalmR && !isSwipingR && !hasSwipedR)
        {
            isSwipingR = true; rightSwipeStartPos = currentRPos;
        }

        if (isSwipingR && !hasSwipedR && (currentRPos.x - rightSwipeStartPos.x) >= minSwipeDistance)
        {
            if (PoseJudge.isPalmR) { hasSwipedR = true; triggerSwipeRight = true; }
        }
    }

    void HandleOKGesture()
    {
        if (PoseJudge.isOKR)
        {
            if (!hasConfirmedOK)
            {
                currentOKHoldTime += Time.deltaTime;
                if (currentOKHoldTime >= okHoldTime)
                {
                    triggerOK = true;
                    hasConfirmedOK = true;
                }
            }
        }
        else
        {
            currentOKHoldTime = 0f;
            hasConfirmedOK = false;
        }
    }

    void HandlePunchLeft(Vector3 currentLPos, float speedZL)
    {
        if (speedZL > -0.05f) { isPunchingL = false; hasPunchedL = false; }
        else if (speedZL < -punchSpeedThreshold && PoseJudge.isFistL && !isPunchingL && !hasPunchedL)
        {
            isPunchingL = true; leftPunchStartPos = currentLPos;
        }

        if (isPunchingL && !hasPunchedL && (leftPunchStartPos.z - currentLPos.z) >= minPunchDistance)
        {
            if (PoseJudge.isFistL) { hasPunchedL = true; triggerPunchLeft = true; }
        }
    }

    void HandlePunchRight(Vector3 currentRPos, float speedZR)
    {
        if (speedZR > -0.05f) { isPunchingR = false; hasPunchedR = false; }
        else if (speedZR < -punchSpeedThreshold && PoseJudge.isFistR && !isPunchingR && !hasPunchedR)
        {
            isPunchingR = true; rightPunchStartPos = currentRPos;
        }

        if (isPunchingR && !hasPunchedR && (rightPunchStartPos.z - currentRPos.z) >= minPunchDistance)
        {
            if (PoseJudge.isFistR) { hasPunchedR = true; triggerPunchRight = true; }
        }
    }

    public static bool ConsumeSwipeLeft() { if (triggerSwipeLeft) { triggerSwipeLeft = false; return true; } return false; }
    public static bool ConsumeSwipeRight() { if (triggerSwipeRight) { triggerSwipeRight = false; return true; } return false; }
    public static bool ConsumeOK() { if (triggerOK) { triggerOK = false; return true; } return false; }
    public static bool ConsumePunchLeft() { if (triggerPunchLeft) { triggerPunchLeft = false; return true; } return false; }
    public static bool ConsumePunchRight() { if (triggerPunchRight) { triggerPunchRight = false; return true; } return false; }
}