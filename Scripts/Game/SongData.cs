using System.Collections.Generic;

[System.Serializable]
public class SongData
{
    public float bpm; // 使用 float 更严谨，有些歌的 BPM 带小数
    public List<NoteData> notes;
}

[System.Serializable]
public class NoteData
{
    public int beat; // 第几个节拍
    public int pose; // 0 左拳，1 右拳
}