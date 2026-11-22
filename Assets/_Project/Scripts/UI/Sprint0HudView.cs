using UnityEngine;
using UnityEngine.UI;

namespace ChoDoLa.UI
{
    /// <summary>
    /// Minimal HUD view for Sprint 0. Displays crosshair, interaction prompt,
    /// and basic run state placeholders ready to bind to Codex's data contract.
    /// </summary>
    public class Sprint0HudView : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text interactPromptText;

        private int _day = 1;
        private int _cash = 1000;
        private int _debtDue = 200;
        private int _boxesBought = 0;
        private int _maxDailyBoxes = 3;

        private void Start()
        {
            RefreshStatus();
            SetInteractPrompt(string.Empty);
        }

        public void UpdateRunSummary(int day, int cash, int debtDue, int boxesBought, int maxDailyBoxes)
        {
            _day = day;
            _cash = cash;
            _debtDue = debtDue;
            _boxesBought = boxesBought;
            _maxDailyBoxes = maxDailyBoxes;
            RefreshStatus();
        }

        public void SetInteractPrompt(string prompt)
        {
            if (interactPromptText == null) return;
            interactPromptText.text = prompt;
            interactPromptText.enabled = !string.IsNullOrEmpty(prompt);
        }

        private void RefreshStatus()
        {
            if (statusText == null) return;
            statusText.text = $"Ngày {_day}/7  |  Tiền: {_cash} xu  |  Nợ hôm nay: {_debtDue} xu  |  Hộp: {_boxesBought}/{_maxDailyBoxes}";
        }
    }
}
