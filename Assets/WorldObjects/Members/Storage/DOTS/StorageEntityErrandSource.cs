using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    public class StorageEntityErrandSource : MonoBehaviour, IErrandSource<EntityStoreErrand>, IErrandCompletionReciever<EntityStoreErrand>
    {
        public StoreErrandType errandType;
        public ErrandBoard errandBoard;

        public ErrandType ErrandType => errandType;
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

        public EntityArchetype errandRequestArchetype;

        public void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(StorageSupplyErrandRequestComponent));
            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<EntityStoreErrand> GetErrand(GameObject errandExecutor)
        {
            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = entityManager.CreateEntity(errandRequestArchetype);
#if UNITY_EDITOR
            entityManager.SetName(entity, "StorageErrandRequest");
#endif
            commandbuffer.SetComponent(entity, new StorageSupplyErrandRequestComponent());
            return new StorageEntityErrandResultListener(
                this,
                entity,
                errandExecutor);
        }

        public void ErrandCompleted(EntityStoreErrand errand)
        {
            Debug.Log("[ERRANDS] Entity storage errand completed");
        }

        public void ErrandAborted(EntityStoreErrand errand)
        {
            Debug.LogError("[ERRANDS] Entity storage errand aborted");
        }

        class StorageEntityErrandResultListener : ErrandSourceNode<EntityStoreErrand>
        {
            private StorageEntityErrandSource errandSourceInstance;
            private Entity errandRequestEntity;
            private GameObject targetActor;

            private bool gotResult;

            public StorageEntityErrandResultListener(
                StorageEntityErrandSource errandSourceInstance,
                Entity requestEntity,
                GameObject targetActor)
            {
                this.errandSourceInstance = errandSourceInstance;
                errandRequestEntity = requestEntity;
                this.targetActor = targetActor;
            }

            protected override NodeStatus OnEvaluate(Blackboard blackboard)
            {
                if (gotResult)
                {
                    return ErrandResult == null ? NodeStatus.FAILURE : NodeStatus.SUCCESS;
                }
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (!entityManager.HasComponent<StorageSupplyErrandResultComponent>(errandRequestEntity))
                {
                    return NodeStatus.RUNNING;
                }

                var resultData = entityManager.GetComponentData<StorageSupplyErrandResultComponent>(errandRequestEntity);
                gotResult = true;

                var commandbuffer = errandSourceInstance.commandbufferSystem.CreateCommandBuffer();
                commandbuffer.DestroyEntity(errandRequestEntity);

                if (resultData.itemSource == Entity.Null || resultData.supplyTarget == Entity.Null)
                {
                    return NodeStatus.FAILURE;
                }

                ErrandResult = new EntityStoreErrand(resultData, targetActor, errandSourceInstance);
                return NodeStatus.SUCCESS;
            }
            public override void Reset(Blackboard blackboard)
            {
                throw new NotImplementedException();
            }
        }
    }
}
