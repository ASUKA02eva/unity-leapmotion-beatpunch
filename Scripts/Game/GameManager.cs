using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("音乐设置")]
    public AudioSource musicSource;
    public float bpm;
    private float secPerBeat;
    public float songPositionInBeats { get; private set; }
    private float dspSongTime;

    [Header("音符生成与移动设置")]
    public GameObject notePrefabLeft;
    public GameObject notePrefabRight;
    public float leftTrackX = 0.5f;
    public float rightTrackX = -0.5f;
    public float beatsShownInAdvance = 4f;
    public float beatDistance = 0.2f;
    public float targetY = 0.9f;

    [Header("空间判定区间")]
    public float greatMinY = 0.9f;
    public float greatMaxY = 1.0f;
    public float missMinY = 0.8f;

    [Header("特效与音效")]
    public GameObject greatVFXPrefab;
    public GameObject missVFXPrefab;
    public AudioClip greatSFX;
    public AudioSource sfxSource;

    [Header("判定文字特效")]
    public GameObject greatTextPrefab;
    public GameObject missTextPrefab;
    public float textSpawnOffsetHeight = 0.1f;

    [Header("UI与计分系统")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public UITextPulse scorePulse;
    public UITextPulse comboPulse;
    public int baseScorePerGreat = 100;
    public int comboBonusMultiplier = 10;

    [Header("结算界面设置")]
    public GameObject resultPanel;
    public CanvasGroup resultCanvasGroup;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMaxComboText;
    public TextMeshProUGUI resultGreatCountText;
    public TextMeshProUGUI resultMissCountText;
    public TextMeshProUGUI resultRankText;
    public TextMeshProUGUI resultTipText;      // 结算界面底部的操作提示文字
    public float resultFadeDuration = 0.5f;

    [Header("体感交互状态显示")]
    [Tooltip("可以放一个提示文字，如：双手握拳开始游戏")]
    public TextMeshProUGUI tipText;

    // 内部统计与状态
    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int greatCount = 0;
    private int missCount = 0;
    private int totalNotesCount = 0;

    private enum GameState { Preparing, Playing, Finished }
    private GameState currentState = GameState.Preparing;

    private float fistTimer = 0f;       // 握拳开始计时
    private float palmTimer = 0f;       // 开掌提前结算计时

    private List<NoteData> allNotes = new List<NoteData>();
    private int nextIndex = 0;

    public Queue<NoteController> leftNotesQueue = new Queue<NoteController>();
    public Queue<NoteController> rightNotesQueue = new Queue<NoteController>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 检查是否有选中的歌曲数据
        if (GameDataBridge.SelectedSongData != null)
        {
            LoadSelectedSong(GameDataBridge.SelectedSongData);
        }
        else
        {
            Debug.LogWarning("未检测到选中歌曲，将使用默认设置。");
            secPerBeat = 60f / bpm;
        }

        if (resultPanel != null) resultPanel.SetActive(false);
        UpdateScoreUI();

        if (tipText != null) tipText.text = "双手握拳开始";
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Preparing:
                HandlePreparation();
                break;
            case GameState.Playing:
                HandleGameplay();
                break;
            case GameState.Finished:
                HandleResult();
                break;
        }
    }

    void LoadSelectedSong(SongMetaData data)
    {
        // 1. 设置音频
        if (data.previewAudio != null)
        {
            musicSource.clip = data.previewAudio;
        }

        // 2. 解析 JSON 曲谱数据
        if (data.songJson != null)
        {
            SongData songData = JsonUtility.FromJson<SongData>(data.songJson.text);
            if (songData != null)
            {
                SetupSong(songData.bpm, songData.notes);
                Debug.Log($"成功加载: {data.songName}, BPM: {songData.bpm}");
            }
        }
    }

    void HandlePreparation()
    {
        if (PoseJudge.isFistL && PoseJudge.isFistR)
        {
            fistTimer += Time.deltaTime;
            if (tipText != null) tipText.text = "预备......" + (2f - fistTimer).ToString("F1");

            if (fistTimer >= 2f)
            {
                StartGame();
            }
        }
        else
        {
            fistTimer = 0f;
            if (tipText != null) tipText.text = "双手握拳开始";
        }
    }

    void StartGame()
    {
        currentState = GameState.Playing;

        dspSongTime = (float)AudioSettings.dspTime + beatsShownInAdvance * secPerBeat;

        if (musicSource != null)
            musicSource.PlayScheduled(AudioSettings.dspTime + beatsShownInAdvance * secPerBeat);

        if (tipText != null) tipText.text = "";
    }

    void HandleGameplay()
    {
        float songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats = songPosition / secPerBeat;

        // 音符生成
        if (nextIndex < allNotes.Count)
        {
            if (allNotes[nextIndex].beat <= songPositionInBeats + beatsShownInAdvance)
            {
                SpawnNote(allNotes[nextIndex]);
                nextIndex++;
            }
        }

        // 恢复双手张开2秒结算功能
        if (PoseJudge.isPalmL && PoseJudge.isPalmR)
        {
            palmTimer += Time.deltaTime;
            if (palmTimer >= 2f)
            {
                ShowResult();
            }
        }
        else
        {
            palmTimer = 0f;
        }

        // 音乐自然结束也进入结算
        if (nextIndex >= allNotes.Count && !musicSource.isPlaying)
        {
            if (currentState != GameState.Finished)
                ShowResult();
        }

        // 调试按键
        if (Input.GetKeyDown(KeyCode.F)) TryHitNote(0);
        if (Input.GetKeyDown(KeyCode.J)) TryHitNote(1);
    }

    void HandleResult()
    {
        // 仅保留：检测 OK 手势返回选歌界面
        if (UIController.ConsumeOK())
        {
            ReturnToSelectSong();
        }

        if (resultTipText != null)
        {
            resultTipText.text = "Hold OK 2s to Return";
        }
    }

    void ReturnToSelectSong()
    {
        string sceneName = "SelectSongScene";

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void SetupSong(float songBpm, List<NoteData> notes)
    {
        bpm = songBpm;
        allNotes = notes;
        totalNotesCount = notes.Count;
        secPerBeat = 60f / bpm;
    }

    void SpawnNote(NoteData noteData)
    {
        GameObject prefab = (noteData.pose == 0) ? notePrefabLeft : notePrefabRight;
        float xPos = (noteData.pose == 0) ? leftTrackX : rightTrackX;
        GameObject newNote = Instantiate(prefab, new Vector3(xPos, -10f, 0), Quaternion.identity);
        NoteController controller = newNote.GetComponent<NoteController>();
        controller.Initialize(noteData.beat, this, noteData.pose);
        if (noteData.pose == 0) leftNotesQueue.Enqueue(controller);
        else rightNotesQueue.Enqueue(controller);
    }

    public void TryHitNote(int pose)
    {
        Queue<NoteController> targetQueue = (pose == 0) ? leftNotesQueue : rightNotesQueue;
        if (targetQueue.Count > 0)
        {
            NoteController oldestNote = targetQueue.Peek();
            float noteY = oldestNote.transform.position.y;
            if (noteY >= greatMinY && noteY <= greatMaxY) HandleHitSuccess(targetQueue, oldestNote);
            else if (noteY >= missMinY && noteY < greatMinY) HandleHitMiss(targetQueue, oldestNote);
            else if (noteY > greatMaxY) HandleHitMiss(targetQueue, oldestNote);
        }
    }

    private void HandleHitSuccess(Queue<NoteController> queue, NoteController note)
    {
        currentCombo++;
        greatCount++;
        if (currentCombo > maxCombo) maxCombo = currentCombo;
        currentScore += baseScorePerGreat + (currentCombo * comboBonusMultiplier);
        UpdateScoreUI();
        Vector3 effectPos = new Vector3(note.transform.position.x, targetY, note.transform.position.z);
        PlayEffectAndSound(greatVFXPrefab, greatSFX, effectPos);
        SpawnJudgeText(greatTextPrefab, effectPos);
        queue.Dequeue();
        Destroy(note.gameObject);
    }

    private void HandleHitMiss(Queue<NoteController> queue, NoteController note)
    {
        currentCombo = 0;
        missCount++;
        UpdateScoreUI();
        Vector3 effectPos = new Vector3(note.transform.position.x, targetY, note.transform.position.z);
        PlayEffectAndSound(missVFXPrefab, null, effectPos);
        SpawnJudgeText(missTextPrefab, effectPos);
        queue.Dequeue();
        Destroy(note.gameObject);
    }

    public void TriggerNaturalMiss(Vector3 position)
    {
        currentCombo = 0;
        missCount++;
        UpdateScoreUI();
        PlayEffectAndSound(missVFXPrefab, null, new Vector3(position.x, targetY, position.z));
        SpawnJudgeText(missTextPrefab, new Vector3(position.x, targetY, position.z));
    }

    private void PlayEffectAndSound(GameObject vfxPrefab, AudioClip sfxClip, Vector3 position)
    {
        if (vfxPrefab != null) Instantiate(vfxPrefab, position, Quaternion.identity);
        if (sfxSource != null && sfxClip != null) sfxSource.PlayOneShot(sfxClip);
    }

    private void SpawnJudgeText(GameObject textPrefab, Vector3 basePosition)
    {
        if (textPrefab != null)
        {
            Vector3 spawnPos = basePosition + Vector3.up * textSpawnOffsetHeight;
            Instantiate(textPrefab, spawnPos, Quaternion.Euler(0, 180f, 0));
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString("D5");
        if (comboText != null) comboText.text = currentCombo > 0 ? currentCombo + " Com" : "";
        if (scorePulse != null && currentScore > 0) scorePulse.TriggerPulse();
        if (comboPulse != null && currentCombo > 0) comboPulse.TriggerPulse();
    }

    void ShowResult()
    {
        // 1. 状态锁：防止重复触发结算
        if (currentState == GameState.Finished) return;
        currentState = GameState.Finished;

        // 2. 停止音乐
        musicSource.Stop();

        if (resultPanel == null) return;

        // 3. 填充统计数据并应用推荐的颜色参数
        resultScoreText.text = "Score: " + currentScore.ToString();
        ColorUtility.TryParseHtmlString("#FFEB3B", out Color scoreColor);
        resultScoreText.color = scoreColor;

        resultMaxComboText.text = "Max Combo: " + maxCombo.ToString();

        resultGreatCountText.text = "Great Count: " + greatCount.ToString();
        ColorUtility.TryParseHtmlString("#00FBFF", out Color greatColor);
        resultGreatCountText.color = greatColor;

        resultMissCountText.text = "Miss Count: " + missCount.ToString();
        ColorUtility.TryParseHtmlString("#FF3B3B", out Color missColor);
        resultMissCountText.color = missColor;

        // 4. 计算等级 (Rank) 与动态颜色切换
        if (totalNotesCount > 0)
        {
            float accuracy = (float)greatCount / totalNotesCount;
            Color rankColor = Color.white;

            if (accuracy >= 1.0f)
            {
                resultRankText.text = "SSS";
                ColorUtility.TryParseHtmlString("#FFD700", out rankColor);
            }
            else if (accuracy >= 0.95f)
            {
                resultRankText.text = "SS";
                ColorUtility.TryParseHtmlString("#FFD700", out rankColor);
            }
            else if (accuracy >= 0.90f)
            {
                resultRankText.text = "S";
                ColorUtility.TryParseHtmlString("#FFAC33", out rankColor);
            }
            else if (accuracy >= 0.80f)
            {
                resultRankText.text = "A";
                ColorUtility.TryParseHtmlString("#E29BFF", out rankColor);
            }
            else if (accuracy >= 0.70f)
            {
                resultRankText.text = "B";
                ColorUtility.TryParseHtmlString("#3BB2FF", out rankColor);
            }
            else if (accuracy >= 0.60f)
            {
                resultRankText.text = "C";
                ColorUtility.TryParseHtmlString("#AAAAAA", out rankColor);
            }
            else
            {
                resultRankText.text = "D";
                ColorUtility.TryParseHtmlString("#AAAAAA", out rankColor);
            }

            resultRankText.color = rankColor;
        }
        else
        {
            resultRankText.text = "-";
            resultRankText.color = Color.white;
        }

        // 5. 更新操作提示文字：只保留 OK 手势返回
        if (resultTipText != null)
        {
            resultTipText.text = "Hold OK 2s to Return";
        }

        // 6. 显示面板并执行淡入动画
        resultPanel.SetActive(true);
        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInResultPanel());
        }
    }

    IEnumerator FadeInResultPanel()
    {
        float elapsed = 0f;
        while (elapsed < resultFadeDuration)
        {
            elapsed += Time.deltaTime;
            resultCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / resultFadeDuration);
            yield return null;
        }
        resultCanvasGroup.alpha = 1f;
    }
}