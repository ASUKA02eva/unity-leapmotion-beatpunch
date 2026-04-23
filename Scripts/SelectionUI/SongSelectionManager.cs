using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SongSelectionManager : MonoBehaviour
{
    [Header("歌曲数据")]
    public List<SongMetaData> songList;

    [Header("UI 引用")]
    public GameObject cardPrefab;
    public Transform cardContainer;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI difficultyText;

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
        if (songList == null || songList.Count == 0)
        {
            Debug.LogWarning("歌曲列表为空！请在 Inspector 面板中添加歌曲数据。");
            return;
        }

        InitializeCards();
        UpdateUIInfo();
    }

    void InitializeCards()
    {
        for (int i = 0; i < songList.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            Image img = cardObj.GetComponent<Image>();

            if (songList[i].coverArt != null)
            {
                img.sprite = songList[i].coverArt;
            }

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

        if (cardRects.Count > 0 && currentIndex < cardRects.Count)
        {
            cardRects[currentIndex].SetAsLastSibling();
        }
    }

    void Update()
    {
        if (songList == null || songList.Count == 0) return;

        HandleInput();
        AnimateCards();
    }

    void HandleInput()
    {
        // 1. 向左切换
        if (Input.GetKeyDown(KeyCode.LeftArrow) || UIController.ConsumeSwipeLeft())
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = 0;
            UpdateUIInfo();

            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }
        // 2. 向右切换
        else if (Input.GetKeyDown(KeyCode.RightArrow) || UIController.ConsumeSwipeRight())
        {
            currentIndex++;
            if (currentIndex >= songList.Count) currentIndex = songList.Count - 1;
            UpdateUIInfo();

            if (UISoundManager.Instance != null) UISoundManager.Instance.PlaySwipe();
        }

        // 3. 确认选择
        if (Input.GetKeyDown(KeyCode.Return) || UIController.ConsumeOK())
        {
            if (UISoundManager.Instance != null) UISoundManager.Instance.PlayConfirm();
            SelectSong();
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

            if (i == currentIndex)
            {
                cardRects[i].SetAsLastSibling();
            }
        }
    }

    void UpdateUIInfo()
    {
        if (songList == null || songList.Count == 0 || currentIndex >= songList.Count) return;

        SongMetaData currentSong = songList[currentIndex];

        // 更新文本信息
        if (titleText != null) titleText.text = currentSong.songName;
        if (artistText != null) artistText.text = currentSong.artistName;

        // 如果是返回卡片，清空难度显示；否则正常显示难度颜色
        if (difficultyText != null)
        {
            if (currentSong.isReturnCard)
            {
                difficultyText.text = "";
            }
            else
            {
                difficultyText.text = currentSong.difficulty;
                if (currentSong.difficulty == "Easy") difficultyText.color = Color.green;
                else if (currentSong.difficulty == "Normal") difficultyText.color = Color.yellow;
                else if (currentSong.difficulty == "Hard") difficultyText.color = Color.red;
                else difficultyText.color = Color.white;
            }
        }
    }

    void SelectSong()
    {
        if (songList == null || currentIndex >= songList.Count) return;

        SongMetaData currentSong = songList[currentIndex];

        Debug.Log($"[选歌系统] 选中了: {currentSong.songName}, 返回卡属性: {currentSong.isReturnCard}");

        if (currentSong.isReturnCard)
        {
            Debug.Log("检测到返回卡，正在尝试切换到 MainMenuScene...");

            if (TransitionManager.Instance != null)
            {
                if (TransitionManager.Instance.fadeImage == null)
                {
                    Debug.LogError("TransitionManager 上的 Fade Image 丢失！请在 Inspector 面板中拖入 UI 图片。");
                    SceneManager.LoadScene("MainMenuScene");
                }
                else
                {
                    TransitionManager.Instance.LoadScene("MainMenuScene");
                }
            }
            else
            {
                Debug.LogWarning("未找到 TransitionManager 实例，执行普通跳转。");
                SceneManager.LoadScene("MainMenuScene");
            }
        }
        else
        {
            Debug.Log("检测到歌曲卡，准备进入游戏...");

            // 核心修复：取消了这行注释，将数据成功赋值给中转站
            GameDataBridge.SelectedSongData = currentSong;

            if (TransitionManager.Instance != null)
                TransitionManager.Instance.LoadScene("GameScene");
            else
                SceneManager.LoadScene("GameScene");
        }
    }

    [ContextMenu("一键补全所有信息 (提取文件名 & 计算BPM难度)")]
    private void AutoFillAllSongInfo()
    {
        int count = 0;
        foreach (var song in songList)
        {
            if (song.isReturnCard) continue;

            bool infoUpdated = false;

            if (song.previewAudio != null)
            {
                string fileName = song.previewAudio.name;
                string[] parts = fileName.Split('-');
                if (parts.Length >= 2)
                {
                    song.artistName = parts[0].Trim();
                    string title = parts[1];
                    for (int i = 2; i < parts.Length; i++)
                    {
                        title += "-" + parts[i];
                    }
                    song.songName = title.Trim();
                    infoUpdated = true;
                }
            }
            if (infoUpdated) count++;
        }

        Debug.Log($"提取完成！共成功处理并更新了 {count} 首歌曲的信息。");
    }
}