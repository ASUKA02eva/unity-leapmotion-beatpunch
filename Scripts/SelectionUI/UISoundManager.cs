using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("音效播放器")]
    public AudioSource audioSource;

    [Header("UI 音效片段")]
    public AudioClip swipeSound;
    public AudioClip confirmSound;
    public AudioClip punchSound;
    public AudioClip errorSound;

    private void Awake()
    {
        // 仅限本场景，不再跨场景存活
        Instance = this;
    }

    public void PlaySwipe() { if (audioSource && swipeSound) audioSource.PlayOneShot(swipeSound); }
    public void PlayConfirm() { if (audioSource && confirmSound) audioSource.PlayOneShot(confirmSound); }
    public void PlayPunch() { if (audioSource && punchSound) audioSource.PlayOneShot(punchSound); }
    public void PlayError() { if (audioSource && errorSound) audioSource.PlayOneShot(errorSound); }
}