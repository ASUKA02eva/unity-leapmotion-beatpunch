using UnityEngine;
using System.Collections.Generic;

public class SongDataLoader : MonoBehaviour
{
    [Header("曲谱文件 (请将 JSON 文件拖拽到这里)")]
    public TextAsset jsonFile;

    void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("没有分配 JSON 曲谱文件！请在 Inspector 面板中拖拽文件。");
            return;
        }

        // 1. 使用 Unity 自带的 JsonUtility 解析文本
        SongData songData = JsonUtility.FromJson<SongData>(jsonFile.text);

        // 2. 检查解析是否成功
        if (songData != null && songData.notes != null)
        {
            Debug.Log($"成功读取曲谱！当前 BPM: {songData.bpm}, 总音符数量: {songData.notes.Count}");

            // 3. 将解析出来的数据喂给 GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetupSong(songData.bpm, songData.notes);
            }
            else
            {
                Debug.LogError("场景中没有找到 GameManager！请确保 GameManager 脚本已挂载且启用了单例。");
            }
        }
        else
        {
            Debug.LogError("JSON 解析失败，请检查 SongData 结构是否与 JSON 文件严格对应！");
        }
    }
}