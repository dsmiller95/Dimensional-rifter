using Assets.Behaviors.Errands.Scripts;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.ErrandClaims
{
    public abstract class BasicErrand<ComponentResult, T> : IErrand
        where ComponentResult : unmanaged, IComponentData
        where T : BasicErrand<ComponentResult, T>
    {
        public ComponentResult errandResult;
        public GameObject actor;

        protected World entityWorld;
        protected EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        protected IErrandCompletionReciever<T> completionReciever;
        protected bool BehaviorCompleted = false;

        public BasicErrand(
            ComponentResult errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<T> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandResult = errandResult;
            this.actor = actor;
            this.completionReciever = completionReciever;

            ErrandBehaviorTreeRoot = SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        protected abstract BehaviorNode SetupBehavior();

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }
        public virtual void OnErrandFailToComplete() { }
        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                OnErrandFailToComplete();
                completionReciever.ErrandAborted((T)this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
