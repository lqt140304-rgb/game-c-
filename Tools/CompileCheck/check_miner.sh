#!/usr/bin/env bash
# Rương Thợ Mỏ nay có kịch bản đào (MinerDigSequence) dùng Rigidbody/AudioClip.Create… vượt stub nhỏ: chuyển sang bản đầy đủ.
exec "$(cd "$(dirname "$0")" && pwd)/check_miner_dig.sh" "$@"
