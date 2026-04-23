import os
import json
import random
import math
import librosa
import numpy as np

def generate_beatmaps(input_folder, output_folder, min_beat_gap=1, max_consecutive=4):
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    audio_files = [f for f in os.listdir(input_folder) if f.lower().endswith('.ogg')]

    if not audio_files:
        print(f"在 {input_folder} 中没有找到任何 .ogg 文件。")
        return

    print(f"总共找到 {len(audio_files)} 首歌曲，准备开始打谱...\n")

    for filename in audio_files:
        file_path = os.path.join(input_folder, filename)
        base_name = os.path.splitext(filename)[0]
        output_path = os.path.join(output_folder, f"{base_name}.json")

        print(f"🎵 正在处理: {filename} ...")

        try:
            y, sr = librosa.load(file_path, sr=None)

            # 1. 取整 BPM
            tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
            raw_bpm = float(tempo[0]) if isinstance(tempo, np.ndarray) else float(tempo)
            final_bpm = math.floor(raw_bpm)
            
            # 2. 一拍的秒数
            sec_per_beat = 60.0 / final_bpm

            onset_frames = librosa.onset.onset_detect(y=y, sr=sr, backtrack=True)
            onset_times = librosa.frames_to_time(onset_frames, sr=sr)

            notes = []
            last_beat_index = -min_beat_gap 
            
            last_pose = -1 
            consecutive_count = 0

            for t in onset_times:
                t = float(t)
                
                # 3. 【核心修改】：算出属于第几个节拍 (四舍五入为整数)
                beat_index = round(t / sec_per_beat)
                
                # 基于拍数过滤
                if beat_index - last_beat_index >= min_beat_gap:
                    
                    if last_pose == -1:
                        pose = random.choice([0, 1])
                    else:
                        if consecutive_count >= max_consecutive:
                            pose = 1 - last_pose
                        else:
                            pose = random.choice([0, 1])
                    
                    if pose == last_pose:
                        consecutive_count += 1
                    else:
                        last_pose = pose
                        consecutive_count = 1

                    # 4. 直接把整数节拍写入 JSON ！！！
                    notes.append({
                        "beat": int(beat_index),  # <-- 改成了 beat 字段，存整数
                        "pose": pose
                    })
                    last_beat_index = beat_index

            beatmap_data = {
                "bpm": final_bpm,
                "notes": notes
            }

            with open(output_path, 'w', encoding='utf-8') as f:
                json.dump(beatmap_data, f, indent=4)

            print(f"   ✅ 生成完毕! (BPM: {final_bpm}, 提取了 {len(notes)} 个基于节拍的音符)")

        except Exception as e:
            print(f"   ❌ 处理失败: {e}")

    print("\n🎉 所有谱面已批量生成结束！")

if __name__ == "__main__":
    INPUT_DIR = r"C:\Users\徐林智\Desktop\AudioProcess"
    OUTPUT_DIR = r"C:\Users\徐林智\Desktop\AudioProcess\Beatmaps"

    generate_beatmaps(INPUT_DIR, OUTPUT_DIR)