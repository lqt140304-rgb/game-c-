using System.Linq;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed partial class MarketRun
    {
        public string ActiveHeirloomId => state.HeirloomId;
        public string ActiveHeirloomName => (string)FindHeirloom(state.HeirloomId)?["name"];
        public int CarryDebt => state.CarryDebt;
        public bool PostponeAvailable => Status == "active" && CanPostpone(state, PostponeEffect(state));

        private JToken FindHeirloom(string id) =>
            content["heirlooms"].FirstOrDefault(item => (string)item["id"] == id);

        private string InitialHeirloom(int seed)
        {
            using var random = new OfferRandom(seed, "starting-heirloom");
            var definitions = (JArray)content["heirlooms"];
            return (string)definitions[random.Index(definitions.Count)]["id"];
        }

        private JToken PostponeEffect(State current) => FindHeirloom(current.HeirloomId)?["effects"]
            .FirstOrDefault(effect => (string)effect["type"] == "postpone_debt");

        private static bool CanPostpone(State current, JToken effect) =>
            effect != null && current.Day != (int)effect["not_on_day"] &&
            current.PostponementsUsed < (int)effect["uses_per_run"];

        private int HeirloomFreeClues(string boxType) =>
            FindHeirloom(state.HeirloomId)?["effects"]
                .Where(effect => (string)effect["type"] == "extra_free_clue" && (string)effect["box"] == boxType)
                .Sum(effect => (int)effect["count"]) ?? 0;

        private bool ValidHeirloomSave(State loaded)
        {
            if (loaded.HeirloomId == null)
                return loaded.CarryDebt == 0 && loaded.PostponementsUsed == 0;

            var effect = PostponeEffect(loaded);
            return FindHeirloom(loaded.HeirloomId) != null && loaded.CarryDebt >= 0 &&
                loaded.PostponementsUsed >= 0 && loaded.PostponementsUsed <= ((int?)effect?["uses_per_run"] ?? 0) &&
                (loaded.CarryDebt == 0 || (loaded.PostponementsUsed > 0 && loaded.Day > 1));
        }
    }
}
