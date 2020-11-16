using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    [Serializable]
    public class StorageErrandEntitySourceSlice : IErrandSource<EntityStoreErrand>
    {
        public StoreErrandType errandType;
        public ItemSourceType[] validSources;
        public SuppliableType supplyTypeTarget;

        public ErrandType ErrandType => errandType;
        private StorageEntityErrandSource baseErrandSource;
        private uint validSourceFlags;
        private uint validSupplyFlags;

        public void Init(StorageEntityErrandSource errandSource)
        {
            baseErrandSource = errandSource;
            validSourceFlags = 0;
            foreach (var source in validSources)
            {
                validSourceFlags |= ((uint)1) << source.ID;
            }
            validSupplyFlags = ((uint)1) << supplyTypeTarget.ID;
        }

        public IErrandSourceNode<EntityStoreErrand> GetErrand(GameObject errandExecutor)
        {
            var request = new StorageSupplyErrandRequestComponent
            {
                SupplyTargetType = validSupplyFlags,
                ItemSourceTypeFlags = validSourceFlags
            };
            return baseErrandSource.GetErrand(errandExecutor, request);
        }
    }

    public class StorageEntityErrandSource : MonoBehaviour, IErrandCompletionReciever<EntityStoreErrand>
    {
        public ErrandBoard errandBoard;
        public StorageErrandEntitySourceSlice[] StorageErrandTypes;

        private EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityArchetype errandRequestArchetype;

        public void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(StorageSupplyErrandRequestComponent),
                typeof(UniversalCoordinatePositionComponent));

            foreach (var errandSource in StorageErrandTypes)
            {
                errandSource.Init(this);
            }
            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            foreach (var errandSource in StorageErrandTypes)
            {
                errandBoard.RegisterErrandSource(errandSource);
            }
        }

        public IErrandSourceNode<EntityStoreErrand> GetErrand(GameObject errandExecutor, StorageSupplyErrandRequestComponent storageRequest)
        {
            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if(tileMem == null)
            {
                Debug.LogError("Storage errand executor has no navigation member. Needed to discern position of actor");
                return new ImmediateErrandSourceNode<EntityStoreErrand>(null);
            }
            var actorPos = tileMem.CoordinatePosition;

            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = entityManager.CreateEntity(errandRequestArchetype);
#if UNITY_EDITOR
            entityManager.SetName(entity, "StorageErrandRequest");
#endif
            storageRequest.DataIsSet = true;
            commandbuffer.SetComponent(entity, storageRequest);
            commandbuffer.SetComponent(entity, new UniversalCoordinatePositionComponent
            {
                Value = actorPos
            });
            return new StorageEntityErrandResultListener(
                this,
                entity,
                errandExecutor);
        }

        public void ErrandCompleted(EntityStoreErrand errand)
        {
            Debug.Log("[ERRANDS] [STORAGE] Entity errand completed");
        }

        public void ErrandAborted(EntityStoreErrand errand)
        {
            Debug.LogError("[ERRANDS] [STORAGE] Entity errand aborted");
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
                if (!entityManager.Exists(errandRequestEntity))
                {
                    Debug.Log("[ERRANDS] [STORAGE] No valid storage errand found");
                    gotResult = true;
                    return NodeStatus.FAILURE;
                }
                if (!entityManager.HasComponent<StorageSupplyErrandResultComponent>(errandRequestEntity))
                {
                    return NodeStatus.RUNNING;
                }

                var resultData = entityManager.GetComponentData<StorageSupplyErrandResultComponent>(errandRequestEntity);
                gotResult = true;

                var commandbuffer = errandSourceInstance.commandbufferSystem.CreateCommandBuffer();
                // TODO: this keeps all the requests in the entities. remove it later
                //commandbuffer.DestroyEntity(errandRequestEntity);

                if (resultData.itemSource == Entity.Null || resultData.supplyTarget == Entity.Null)
                {
                    return NodeStatus.FAILURE;
                }

                ErrandResult = new EntityStoreErrand(resultData, targetActor, errandSourceInstance);
                return NodeStatus.SUCCESS;
            }
            public override void Reset(Blackboard blackboard)
            {
                // TODO: make sure the entity gets cleaned up if the errand gets aborted early
                //   I.E. due to death
                throw new NotImplementedException();
            }
        }
    }
}
