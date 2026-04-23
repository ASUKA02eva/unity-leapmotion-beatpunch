using UnityEngine;

// 就是这句！有了它，Unity 才能在面板里把这个类显示出来
[System.Serializable]
public class SongMetaData
{
    [Header("卡片类型设置")]
    public bool isReturnCard = false; // 勾选后，这就是一张返回卡片，而不是歌曲

    [Header("歌曲信息")]
    public string songName;
    public string artistName;
    public string difficulty;
    public Sprite coverArt;        // 歌曲封面
    public TextAsset songJson;     // 对应的曲谱数据
    public AudioClip previewAudio; // 预览音乐片段（用于获取文件名及后续播放试听）
}