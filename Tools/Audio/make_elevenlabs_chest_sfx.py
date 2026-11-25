#!/usr/bin/env python3
"""Tạo âm theo chủ đề cho từng rương bằng ElevenLabs Sound Generation (/v1/sound-generation).

Đọc Tools/Audio/elevenlabs_chest_manifest.json. Mỗi mục có prompt (không phải "reuse") được tạo MỘT lần thành
Assets/_Project/Audio/Chests/<thư mục rương>/<file>.mp3; file đã có thì bỏ qua (tốn credit, không tạo lại).
Vòng nền (loop=true) gửi kèm "loop": true để ElevenLabs làm âm nối liền.

Khoá: biến môi trường ELEVENLABS_API_KEY. Trong phiên cloud có proxy tự chèn khoá cho api.elevenlabs.io thì đặt giá trị
giả bất kỳ (proxy thay bằng khoá thật).

Dùng:
  python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --dry-run          # chỉ liệt kê, không gọi dịch vụ
  python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --chest viet_nam    # một rương
  python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py                     # tất cả
Gọi TUẦN TỰ (gọi song song từng bị lỗi 429), lỗi 429 thì chờ rồi thử lại tối đa 3 lần.
Sau khi tạo, ghi thêm một dòng vào Assets/_Project/Audio/Chests/<thư mục>/README.md (file, giây, mô tả).
"""
import argparse, json, os, sys, time, urllib.request, urllib.error

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MANIFEST = os.path.join(ROOT, "Tools", "Audio", "elevenlabs_chest_manifest.json")
AUDIO = os.path.join(ROOT, "Assets", "_Project", "Audio", "Chests")
URL = "https://api.elevenlabs.io/v1/sound-generation?output_format=mp3_44100_128"
MODEL = "eleven_text_to_sound_v2"


def generate(prompt, seconds, loop, key):
    seconds = max(0.5, float(seconds))   # ElevenLabs nhận 0,5–30 s; tiếng ngắn hơn thì hook/ffmpeg cắt sau
    body = {"text": prompt, "duration_seconds": seconds, "prompt_influence": 0.6, "model_id": MODEL}
    if loop:
        body["loop"] = True
    req = urllib.request.Request(URL, data=json.dumps(body).encode("utf-8"), method="POST",
                                 headers={"xi-api-key": key, "Content-Type": "application/json", "Accept": "audio/mpeg"})
    for attempt in range(4):
        try:
            with urllib.request.urlopen(req, timeout=120) as r:
                return r.read()
        except urllib.error.HTTPError as e:
            if e.code == 429 and attempt < 3:
                time.sleep(5 * (attempt + 1))
                continue
            raise RuntimeError(f"HTTP {e.code}: {e.read()[:300]!r}")
    raise RuntimeError("hết lượt thử")


def note_readme(folder, file, seconds, prompt):
    path = os.path.join(AUDIO, folder, "README.md")
    head = not os.path.exists(path)
    with open(path, "a", encoding="utf-8") as f:
        if head:
            f.write(f"# Âm rương ({folder})\n\nTạo bằng ElevenLabs Sound Generation. CHƯA AI NGHE THỬ.\n\n| File | Dài (s) | Mô tả |\n|---|---|---|\n")
        f.write(f"| `{file}` | {seconds} | {prompt} |\n")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--chest", help="tên thư mục rương, ví dụ viet_nam")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--manifest", default=MANIFEST, help="đường dẫn danh mục (mặc định elevenlabs_chest_manifest.json)")
    a = ap.parse_args()
    manifest = json.load(open(a.manifest, encoding="utf-8"))
    key = os.environ.get("ELEVENLABS_API_KEY", "")
    if not a.dry_run and not key:
        sys.exit("Thiếu ELEVENLABS_API_KEY (đặt giá trị giả nếu proxy chèn khoá).")
    made = skipped = failed = 0
    for folder, chest in manifest.items():
        if folder.startswith("_") or (a.chest and folder != a.chest):
            continue
        for it in chest["items"]:
            if it["prompt"] == "reuse":
                continue
            out = os.path.join(AUDIO, folder, it["file"])
            if os.path.exists(out):
                skipped += 1
                continue
            print(f"{folder}/{it['file']} ({it['seconds']} s{', loop' if it['loop'] else ''}): {it['prompt']}")
            if a.dry_run:
                continue
            try:
                data = generate(it["prompt"], it["seconds"], it["loop"], key)
                os.makedirs(os.path.dirname(out), exist_ok=True)
                with open(out, "wb") as f:
                    f.write(data)
                note_readme(folder, it["file"], it["seconds"], it["prompt"])
                made += 1
            except Exception as e:  # tiếp tục các file khác, báo cuối
                print("  LỖI:", e)
                failed += 1
    print(f"Xong: tạo {made}, bỏ qua (đã có) {skipped}, lỗi {failed}.")


if __name__ == "__main__":
    main()
