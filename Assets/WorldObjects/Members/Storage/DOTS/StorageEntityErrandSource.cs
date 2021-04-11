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
                validSourceFlags |= ((uint)1) << source.myId;
            }
            validSupplyFlags = ((uint)1) << supplyTypeTarget.myId;
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
            if (tileMem == null)
            {
                Debug.LogError("Storage errand executor has no navigation member. Needed to discern position of actor");
                return null;
            }
            var actorPos = tileMem.CoordinatePosition;

            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var worldToUse = World.DefaultGameObjectInjectionWorld;
            var entityManager = worldToUse.EntityManager;

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

            return new ErrandFromBlackboardDataNode<EntityStoreErrand>(
                    new WaitForECSResponse<StorageSupplyErrandResultComponent, BeginInitializationEntityCommandBufferSystem>(
                        entity,
                        worldToUse,
                        "storageErrandData"
                        ),
                    Blackboard =>
                    {
                        Blackboard.TryGetValueOfType("storageErrandData", out StorageSupplyErrandResultComponent errandResult);
                        return new EntityStoreErrand(errandResult, errandExecutor, this);
                    }
                );
        }

        public void ErrandCompleted(EntityStoreErrand errand)
        {
            Debug.Log("[ERRANDS][STORAGE] Entity errand completed");
        }

        public void ErrandAborted(EntityStoreErrand errand)
        {
            Debug.LogError("[ERRANDS][STORAGE] Entity errand aborted");
        }
    }
}
