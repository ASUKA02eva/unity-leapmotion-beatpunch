using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    // 【新增】：一个全局状态锁，用来告诉其他脚本“现在正在黑屏，别乱动”
    public static bool IsFading { get; private set; }

    [Header("UI 引用")]
    public Image fadeImage;

    [Header("过渡设置")]
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        Instance = this;

        // 【新增】：进入新场景时强行先解锁，防止上一场景残留的 true 导致手势失效
        IsFading = false;

        if (fadeImage != null)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadRoutine(sceneName));
    }

    private IEnumerator FadeInRoutine()
    {
        IsFading = true; // 开始淡入，上锁

        float timer = fadeDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);

        IsFading = false; // 淡入结束，解锁
    }

    private IEnumerator FadeOutAndLoadRoutine(string sceneName)
    {
        IsFading = true; // 开始黑屏，上锁

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1);

        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // 【新增】：退出游戏函数
    // ==========================================
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 如果在 Unity 编辑器中运行，则停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 如果是打包后的正式游戏，则直接退出程序
        UnityEngine.Application.Quit();
#endif
    }
}