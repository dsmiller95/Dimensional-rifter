using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public interface IErrandSource<out T> where T: IErrand
    {
        ErrandType ErrandType { get; }
        T GetErrand(GameObject errandExecutor);
    }
    public interface IErrandCompletionReciever<in T> where T: IErrand
    {
        void ErrandCompleted(T errand);
        void ErrandAborted(T errand);
    }
}
