using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public abstract class ErrandSourceObject<T> : ScriptableObject, IErrandSource<T> where T : IErrand
    {
        public abstract ErrandType ErrandType { get; }

        public abstract T GetErrand(GameObject errandExecutor);
    }
}
