using UnityEngine;
using System.Collections.Generic;

public class NoteController : MonoBehaviour
{
    public float targetBeat;
    private GameManager gameManager;
    private int pose;

    [Header("步进动画设置")]
    public float pauseFraction = 0.7f;

    // 缓存渲染器，用于视觉隐藏
    private Renderer noteRenderer;

    void Awake()
    {
        noteRenderer = GetComponent<Renderer>();
    }

    public void Initialize(float beat, GameManager gm, int p)
    {
        targetBeat = beat;
        gameManager = gm;
        pose = p;
    }

    void Update()
    {
        if (gameManager == null) return;

        float beatsAway = targetBeat - gameManager.songPositionInBeats;
        float moveFraction = 1f - pauseFraction;

        int discreteBeatsAway = Mathf.FloorToInt(beatsAway);
        float fractionToNextBeat = beatsAway - discreteBeatsAway;

        float finalBeatsAway = 0f;

        if (fractionToNextBeat > moveFraction)
        {
            finalBeatsAway = discreteBeatsAway + 1;
        }
        else
        {
            float t = 1f - (fractionToNextBeat / moveFraction);
            t = Mathf.SmoothStep(0f, 1f, t);
            finalBeatsAway = Mathf.Lerp(discreteBeatsAway + 1, discreteBeatsAway, t);
        }

        float currentY = gameManager.targetY - (finalBeatsAway * gameManager.beatDistance);
        transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

        // --- 视觉隐藏逻辑：当 Y 超过目标线 (0.9f) 时，隐藏渲染 ---
        if (noteRenderer != null)
        {
            noteRenderer.enabled = !(transform.position.y > gameManager.targetY);
        }

        // --- 自然 Miss 判定 ---
        if (transform.position.y > gameManager.greatMaxY + 0.05f)
        {
            HandleNaturalMiss();
        }
    }

    void HandleNaturalMiss()
    {
        Debug.Log("音符彻底漏掉了, Miss!");

        gameManager.TriggerNaturalMiss(transform.position);

        Queue<NoteController> queue = (pose == 0) ? gameManager.leftNotesQueue : gameManager.rightNotesQueue;
        if (queue.Count > 0 && queue.Peek() == this)
        {
            queue.Dequeue();
        }

        Destroy(gameObject);
    }
}