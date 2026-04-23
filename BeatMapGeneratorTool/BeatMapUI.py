import flet as ft
import os
import json
import random
import math
import librosa
import numpy as np


async def main(page: ft.Page):
    # ================= UI 窗口基础设置 =================
    page.title = "BeatMapGenerator"
    page.window.width = 800
    page.window.height = 700
    page.theme_mode = ft.ThemeMode.DARK
    page.padding = 30
    # 强制全局使用微软雅黑，解决中日文混排字体渲染差异问题
    page.theme = ft.Theme(font_family="Microsoft YaHei")

    # ================= 状态变量 =================
    input_path = ft.Text("未选择输入文件夹...", color=ft.Colors.GREY_400)
    output_path = ft.Text("未选择输出文件夹...", color=ft.Colors.GREY_400)

    log_area = ft.ListView(expand=True, spacing=10, auto_scroll=True)

    def log(message, color=ft.Colors.WHITE):
        log_area.controls.append(ft.Text(message, color=color))
        page.update()

    # ================= 核心打谱逻辑 =================
    def start_generation(e):
        in_dir = input_path.value if input_path.value != "未选择输入文件夹..." else ""
        out_dir = output_path.value if output_path.value != "未选择输出文件夹..." else ""

        if not in_dir or not out_dir:
            log("❌ 警告：请先选择输入和输出文件夹！", ft.Colors.RED_400)
            return

        generate_btn.disabled = True
        log(
            f"\n🚀 开始批量生成...\n输入目录: {in_dir}\n输出目录: {out_dir}\n" + "-" * 40,
            ft.Colors.BLUE_300,
        )
        page.update()

        try:
            audio_files = [
                f for f in os.listdir(in_dir) if f.lower().endswith(".ogg")
            ]
            if not audio_files:
                log(f"⚠️ 在输入文件夹中没有找到任何 .ogg 文件。", ft.Colors.YELLOW_400)
                generate_btn.disabled = False
                page.update()
                return

            for filename in audio_files:
                file_path = os.path.join(in_dir, filename)
                base_name = os.path.splitext(filename)[0]
                out_file_path = os.path.join(out_dir, f"{base_name}.json")

                # 解析作者和歌名 (格式: 作者 - 歌名.ogg)
                if "-" in base_name:
                    parts = base_name.split("-", 1)
                    artist = parts[0].strip()
                    song_name = parts[1].strip()
                else:
                    artist = "未知作者"
                    song_name = base_name.strip()

                # --- 1. 读取音频与计算时长 ---
                y, sr = librosa.load(file_path, sr=None)

                duration_sec = librosa.get_duration(y=y, sr=sr)
                mins = int(duration_sec // 60)
                secs = int(duration_sec % 60)
                duration_str = f"{mins:02d}:{secs:02d}"

                # --- 2. 估算 BPM 并计算每拍秒数 ---
                tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
                raw_bpm = (
                    float(tempo[0]) if isinstance(tempo, np.ndarray) else float(tempo)
                )
                final_bpm = math.floor(raw_bpm)
                sec_per_beat = 60.0 / final_bpm

                # --- 3. 提取发声点 (Onset) ---
                onset_frames = librosa.onset.onset_detect(y=y, sr=sr, backtrack=True)
                onset_times = librosa.frames_to_time(onset_frames, sr=sr)

                notes = []
                last_beat_index = -1
                last_pose = -1
                consecutive_count = 0

                # --- 4. 生成与过滤音符 (量化到整数拍) ---
                for t in onset_times:
                    t = float(t)
                    beat_index = round(t / sec_per_beat)

                    if beat_index - last_beat_index >= 1:  # 保证最小间隔1拍
                        if last_pose == -1:
                            pose = random.choice([0, 1])
                        else:
                            if consecutive_count >= 4:  # 防抽筋强制换手
                                pose = 1 - last_pose
                            else:
                                pose = random.choice([0, 1])

                        if pose == last_pose:
                            consecutive_count += 1
                        else:
                            last_pose = pose
                            consecutive_count = 1

                        # 直接写入第几拍和左右手
                        notes.append({"beat": int(beat_index), "pose": pose})
                        last_beat_index = beat_index

                # --- 5. 写入 JSON ---
                beatmap_data = {"bpm": final_bpm, "notes": notes}
                with open(out_file_path, "w", encoding="utf-8") as f:
                    json.dump(beatmap_data, f, indent=4)

                # --- 6. 打印精简格式化日志 ---
                log(
                    f"✅ {song_name} | 作者: {artist} | 时长: {duration_str} | BPM: {final_bpm} | 音符数: {len(notes)}",
                    ft.Colors.GREEN_400,
                )

            log("-" * 40 + "\n🎉 所有谱面处理完毕！", ft.Colors.BLUE_300)

        except Exception as ex:
            log(f"❌ 发生错误: {ex}", ft.Colors.RED_400)

        generate_btn.disabled = False
        page.update()

    # ================= 文件夹选择逻辑 (新版异步 API) =================
    async def pick_input_dir(e):
        directory = await ft.FilePicker().get_directory_path(
            dialog_title="选择 .ogg 音频目录"
        )
        if directory:
            input_path.value = directory
            input_path.color = ft.Colors.WHITE
            log(f"已选择输入目录: {directory}")
            page.update()

    async def pick_output_dir(e):
        directory = await ft.FilePicker().get_directory_path(
            dialog_title="选择 .json 保存目录"
        )
        if directory:
            output_path.value = directory
            output_path.color = ft.Colors.WHITE
            log(f"已设置输出目录: {directory}")
            page.update()

    # ================= UI 布局拼装 =================
    title = ft.Text("批量生成曲谱", size=28, weight=ft.FontWeight.BOLD)

    file_selection_row = ft.Column(
        [
            ft.Row(
                [
                    ft.Button(
                        "选择 .ogg 音频目录",
                        icon=ft.Icons.FOLDER_OPEN,
                        on_click=pick_input_dir,
                    ),
                    input_path,
                ]
            ),
            ft.Row(
                [
                    ft.Button(
                        "选择 .json 保存目录",
                        icon=ft.Icons.SAVE,
                        on_click=pick_output_dir,
                    ),
                    output_path,
                ]
            ),
        ],
        spacing=20,
    )

    # 使用新版 Button 替代 FilledButton，消除废弃警告
    generate_btn = ft.Button(
        "一键生成所有谱面",
        icon=ft.Icons.PLAY_ARROW,
        on_click=start_generation,
        height=50,
        style=ft.ButtonStyle(
            bgcolor=ft.Colors.BLUE_600,
            color=ft.Colors.WHITE,
            shape=ft.RoundedRectangleBorder(radius=8),
        ),
    )

    log_container = ft.Container(
        content=log_area,
        bgcolor=ft.Colors.BLACK87,
        border_radius=10,
        padding=15,
        expand=True,
    )

    # 把所有组件加到页面中
    page.add(
        title,
        ft.Divider(height=30),
        file_selection_row,
        ft.Divider(height=30),
        generate_btn,
        ft.Text("运行日志:", weight=ft.FontWeight.BOLD, size=16),
        log_container,
    )

    # 最后强制将窗口居中显示
    await page.window.center()
    
    # 刷新页面，确保所有更改生效
    page.update()


# ================= 启动 App =================
if __name__ == "__main__":
    ft.run(main)