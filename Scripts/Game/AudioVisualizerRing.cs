using UnityEngine;

public class AudioVisualizerRing : MonoBehaviour
{
    [Header("音乐源 (拖入 GameManager 中的 Music Source)")]
    public AudioSource audioSource;

    [Header("预制体与生成设置")]
    [Tooltip("拖入你刚刚制作的细长条 Image 预制体")]
    public GameObject barPrefab;
    [Tooltip("圆环由多少个长条组成（建议 64 或 128）")]
    public int numberOfBars = 64;
    [Tooltip("圆环的半径大小（已调大以容纳两行文字）")]
    public float radius = 250f;

    [Header("跳动效果设置")]
    [Tooltip("跳动的最大缩放倍数")]
    public float maxScale = 200f;
    [Tooltip("动画平滑度，数值越大跳动越硬，越小越平滑")]
    public float smoothSpeed = 15f;
    [Tooltip("高频补偿（高音能量通常较低，用此参数放大高音部分的跳动）")]
    public float highFrequencyMultiplier = 2f;

    private GameObject[] bars;
    private RectTransform[] barRects;
    private float[] spectrumData = new float[512];

    void Start()
    {
        // 自动寻找 GameManager 中的音乐源
        if (audioSource == null && GameManager.Instance != null)
        {
            audioSource = GameManager.Instance.musicSource;
        }

        bars = new GameObject[numberOfBars];
        barRects = new RectTransform[numberOfBars];

        // 沿圆周生成律动条
        float angleStep = 360f / numberOfBars;

        for (int i = 0; i < numberOfBars; i++)
        {
            // 计算当前条的角度和坐标
            float angle = i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            // 因为是 UI，所以用 2D 坐标 (x, y)
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            // 实例化并设置为本物体的子节点
            GameObject bar = Instantiate(barPrefab, transform);
            RectTransform rect = bar.GetComponent<RectTransform>();

            rect.anchoredPosition = pos;
            // 旋转长条，使其一端朝向圆心
            rect.localRotation = Quaternion.Euler(0, 0, angle - 90);

            bars[i] = bar;
            barRects[i] = rect;
        }
    }

    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        // 获取当前帧的音频频谱数据
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        for (int i = 0; i < numberOfBars; i++)
        {
            float rawIntensity = spectrumData[i];

            // 恢复为你想要的初始线性计算方式
            float modifiedIntensity = rawIntensity * maxScale * (1 + i * highFrequencyMultiplier * 0.05f);

            // 恢复最初的 10f 硬性上限限制，让波形跳动更加整齐
            modifiedIntensity = Mathf.Clamp(modifiedIntensity, 0, 10f);

            // 获取当前的缩放比例
            Vector3 currentScale = barRects[i].localScale;
            // 目标缩放比例（Y 轴拉长）
            Vector3 targetScale = new Vector3(currentScale.x, 1f + modifiedIntensity, currentScale.z);

            // 使用 Lerp 平滑过渡
            barRects[i].localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed);
        }
    }
}