using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class MenuOption
{
    public string optionName;
    public Sprite optionIcon;
    public UnityEvent onConfirm;
}

public class MainMenuManager : MonoBehaviour
{
    [Header("菜单数据")]
    public List<MenuOption> menuOptions;

    [Header("UI 引用")]
    public GameObject cardPrefab;
    public Transform cardContainer;
    public TextMeshProUGUI optionNameText;

    [Header("轮播图设置")]
    public float cardSpacing = 800f;
    public float scaleDownFactor = 0.6f;
    public float alphaDownFactor = 0.4f;
    public float lerpSpeed = 10f;

    private List<RectTransform> cardRects = new List<RectTransform>();
    private List<Image> cardImages = new List<Image>();
    private int currentIndex = 0;

    void Start()
    {
        if (menuOptions == null || menuOptions.Count == 0) return;
        InitializeCards();
        UpdateUIInfo();
    }

    void InitializeCards()
    {
        for (int i = 0; i < menuOptions.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            Image img = cardObj.GetComponent<Image>();

            if (menuOptions[i].optionIcon != null) img.sprite = menuOptions[i].optionIcon;

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
        if (menuOptions == null || menuOptions.Count == 0) return;

        HandleInput();
        AnimateCards();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || UIController.ConsumeSwipeLeft())
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = 0;
            UpdateUIInfo();
            // 播放滑动音效
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || UIController.ConsumeSwipeRight())
        {
            currentIndex++;
            if (currentIndex >= menuOptions.Count) currentIndex = menuOptions.Count - 1;
            UpdateUIInfo();
            // 播放滑动音效
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }

        if (Input.GetKeyDown(KeyCode.Return) || UIController.ConsumeOK())
        {
            // 播放确认音效
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlayConfirm();
            ConfirmSelection();
        }
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
        if (menuOptions == null || menuOptions.Count == 0 || currentIndex >= menuOptions.Count) return;
        if (optionNameText != null) optionNameText.text = menuOptions[currentIndex].optionName;
    }

    void ConfirmSelection()
    {
        menuOptions[currentIndex].onConfirm?.Invoke();
    }
}