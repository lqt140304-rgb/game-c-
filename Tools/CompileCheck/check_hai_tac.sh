#!/usr/bin/env bash
# Chest_HaiTac nay dùng kịch bản đại bác (HaiTacSeqHook cần stub đầy đủ): chuyển sang check_hai_tac_seq.sh.
exec "$(dirname "$0")/check_hai_tac_seq.sh" "$@"
