using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class SettingOption
{
    public string optionName;
    public Sprite optionIcon;
    public UnityEvent onConfirm;
}

public class SettingManager : MonoBehaviour
{
    [Header("设置选项数据")]
    public List<SettingOption> settingOptions;

    [Header("UI 引用")]
    public GameObject cardPrefab;
    public Transform cardContainer;
    public TextMeshProUGUI optionNameText;
    public TextMeshProUGUI optionValueText;

    [Header("封面流视觉参数")]
    public float cardSpacing = 800f;
    public float scaleDownFactor = 0.6f;
    public float alphaDownFactor = 0.4f;
    public float lerpSpeed = 10f;

    private List<RectTransform> cardRects = new List<RectTransform>();
    private List<Image> cardImages = new List<Image>();
    private int currentIndex = 0;

    private int currentVolume = 5;
    private int currentResIndex = 2;
    private Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440)
    };

    void Start()
    {
        if (settingOptions == null || settingOptions.Count == 0) return;

        currentVolume = PlayerPrefs.GetInt("GameVolume", 5);
        currentResIndex = PlayerPrefs.GetInt("GameResIndex", 2);

        ApplySettings();
        InitializeCards();
        UpdateUIInfo();
    }

    void InitializeCards()
    {
        for (int i = 0; i < settingOptions.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            Image img = cardObj.GetComponent<Image>();

            if (settingOptions[i].optionIcon != null) img.sprite = settingOptions[i].optionIcon;

            float offset = i - currentIndex;
            rect.anchoredPosition = new Vector2(offset * cardSpacing, 0);
            float targetScale = (i == currentIndex) ? 1f : scaleDownFactor;
            rect.localScale = new Vector3(targetScale, targetScale, 1f);
            float targetAlpha = (i == currentIndex) ? 1f : alphaDownFactor;
            Color currentColor = img.color;
            img.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            cardRects.Add(rect);
            cardImages.Add(img);
        }

        if (cardRects.Count > 0 && currentIndex < cardRects.Count) cardRects[currentIndex].SetAsLastSibling();
    }

    void Update()
    {
        if (settingOptions == null || settingOptions.Count == 0) return;

        HandleInput();
        AnimateCards();
    }

    void HandleInput()
    {
        // 1. 滑动切换 (手掌)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || UIController.ConsumeSwipeLeft())
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = 0;
            UpdateUIInfo();
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || UIController.ConsumeSwipeRight())
        {
            currentIndex++;
            if (currentIndex >= settingOptions.Count) currentIndex = settingOptions.Count - 1;
            UpdateUIInfo();
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }

        // 2. 确认执行
        if (Input.GetKeyDown(KeyCode.Return) || UIController.ConsumeOK())
        {
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlayConfirm();
            ConfirmSelection();
        }

        // 3. 调节参数 (拳击)
        if (Input.GetKeyDown(KeyCode.A) || UIController.ConsumePunchLeft())
        {
            AdjustCurrentSetting(-1);
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlayPunch();
        }
        else if (Input.GetKeyDown(KeyCode.D) || UIController.ConsumePunchRight())
        {
            AdjustCurrentSetting(1);
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlayPunch();
        }
    }

    void AdjustCurrentSetting(int direction)
    {
        string currentOption = settingOptions[currentIndex].optionName;

        if (currentOption == "音量")
        {
            currentVolume = Mathf.Clamp(currentVolume + direction, 0, 10);
            PlayerPrefs.SetInt("GameVolume", currentVolume);
            ApplySettings();
        }
        else if (currentOption == "分辨率")
        {
            currentResIndex = Mathf.Clamp(currentResIndex + direction, 0, resolutions.Length - 1);
            PlayerPrefs.SetInt("GameResIndex", currentResIndex);
            ApplySettings();
        }
        UpdateUIInfo();
    }

    void ApplySettings()
    {
        AudioListener.volume = currentVolume / 10f;
        Vector2Int res = resolutions[currentResIndex];
        Screen.SetResolution(res.x, res.y, FullScreenMode.FullScreenWindow);
    }

    void AnimateCards()
    {
        for (int i = 0; i < cardRects.Count; i++)
        {
            float offset = i - currentIndex;
            Vector2 targetPosition = new Vector2(offset * cardSpacing, 0);
            float targetScale = (i == currentIndex) ? 1f : scaleDownFactor;
            float targetAlpha = (i == currentIndex) ? 1f : alphaDownFactor;

            cardRects[i].anchoredPosition = Vector2.Lerp(cardRects[i].anchoredPosition, targetPosition, Time.deltaTime * lerpSpeed);
            cardRects[i].localScale = Vector3.Lerp(cardRects[i].localScale, new Vector3(targetScale, targetScale, 1f), Time.deltaTime * lerpSpeed);

            Color currentColor = cardImages[i].color;
            Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
            cardImages[i].color = Color.Lerp(currentColor, targetColor, Time.deltaTime * lerpSpeed);

            if (i == currentIndex) cardRects[i].SetAsLastSibling();
        }
    }

    void UpdateUIInfo()
    {
        if (settingOptions == null || settingOptions.Count == 0 || currentIndex >= settingOptions.Count) return;

        string currentOption = settingOptions[currentIndex].optionName;
        if (optionNameText != null) optionNameText.text = currentOption;

        if (optionValueText != null)
        {
            if (currentOption == "音量") optionValueText.text = (currentVolume * 10).ToString() + "%";
            else if (currentOption == "分辨率") optionValueText.text = resolutions[currentResIndex].x + " x " + resolutions[currentResIndex].y;
            else optionValueText.text = "";
        }
    }

    void ConfirmSelection()
    {
        settingOptions[currentIndex].onConfirm?.Invoke();
    }
}