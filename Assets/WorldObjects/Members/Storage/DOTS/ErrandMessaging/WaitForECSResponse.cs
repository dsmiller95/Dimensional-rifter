using BehaviorTree.Nodes;
using System;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    public class WaitForECSResponse<TResponse, TCommands> : Leaf
        where TResponse : unmanaged, IComponentData
        where TCommands : EntityCommandBufferSystem
    {
        private Entity requestEntity;
        private World executionWorld;
        private string responsePathInBlackboard;

        private bool completed;
        private bool validResult;

        public WaitForECSResponse(Entity requestEntity, World executionWorld, string responsePathInBlackboard)
        {
            this.requestEntity = requestEntity;
            this.executionWorld = executionWorld;
            this.responsePathInBlackboard = responsePathInBlackboard;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (completed)
            {
                return validResult ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
            }
            var entityManager = executionWorld.EntityManager;
            if (!entityManager.Exists(requestEntity))
            {
                completed = true;
                validResult = false;
                return NodeStatus.FAILURE;
            }
            if (!entityManager.HasComponent<TResponse>(requestEntity))
            {
                return NodeStatus.RUNNING;
            }

            var responseData = entityManager.GetComponentData<TResponse>(requestEntity);
            completed = true;
            validResult = false;

            blackboard.SetValue(responsePathInBlackboard, responseData);

            // TODO: this keeps all the requests in the entities, but only in the editor. consider switching if performance in editor gets mucky
#if UNITY_EDITOR
#else
            var commandbuffer = executionWorld.GetOrCreateSystem<TCommands>().CreateCommandBuffer();
            commandbuffer.DestroyEntity(errandRequestEntity);
#endif
            return NodeStatus.SUCCESS;
        }
        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(responsePathInBlackboard);
            // TODO: make sure the entity gets cleaned up if the errand gets aborted early
            //   I.E. due to death
            // Does this ever get called?
            throw new NotImplementedException("ECS response waiter Reset function. It does get called!!");
        }
    }
}
