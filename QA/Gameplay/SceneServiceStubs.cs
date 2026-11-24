// Minimal substitutes for Unity lifecycle dependencies in the headless checks.
namespace UnityEngine
{
    public class MonoBehaviour
    {
        protected T GetComponent<T>() where T : class => null;
        public T AddComponent<T>() where T : new() => new T();
        protected MonoBehaviour gameObject => this;
    }
}

namespace ChoDoLa.Gameplay
{
    public sealed class MarketRunHost
    {
        public MarketRun Run { get; set; }
        public bool TimedDaysEnabled { get; private set; }
        public void EnableTimedDays() => TimedDaysEnabled = true;
        public event System.Action ClockPhaseChanged { add { } remove { } }
        public bool StartNextRun() => false;
    }
}
