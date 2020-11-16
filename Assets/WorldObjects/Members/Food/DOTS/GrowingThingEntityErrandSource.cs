using Assets.Behaviors.Errands.Scripts;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    public class GrowingThingEntityErrandSource : MonoBehaviour, IErrandSource<HarvestEntityErrand>, IErrandCompletionReciever<HarvestEntityErrand>
    {
        public HarvestErrandType harvestErrandType;
        public ErrandType ErrandType => harvestErrandType;
        public ErrandBoard errandBoard;


        private EntityQuery ErrandTargetQuery;
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConnectivityEntitySystem>();
        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ErrandTargetQuery = entityManager.CreateEntityQuery(
                typeof(ErrandClaimComponent),
                ComponentType.ReadOnly<GrowingThingComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<HarvestEntityErrand> GetErrand(GameObject errandExecutor)
        {
            var connectionSystem = ConnectivitySystem;
            if (!connectionSystem.HasRegionMaps)
            {
                return new ImmediateErrandSourceNode<HarvestEntityErrand>(null);
            }
            var regionMap = connectionSystem.Regions;
            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Harvest errand executor has no navigation member. Needed to discern position of actor");
                return new ImmediateErrandSourceNode<HarvestEntityErrand>(null);
            }
            var actorPos = tileMem.CoordinatePosition;
            if(!regionMap.TryGetValue(actorPos, out var actorRegion))
            {
                Debug.LogError("actor not included in region map");
                return new ImmediateErrandSourceNode<HarvestEntityErrand>(null);
            }

            Entity targetEntity = Entity.Null;
            HarvestEntityErrand resultErrand = null;
            using (var targets = ErrandTargetQuery.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var claimed = ErrandTargetQuery.ToComponentDataArray<ErrandClaimComponent>(Unity.Collections.Allocator.TempJob))
            using (var positions = ErrandTargetQuery.ToComponentDataArray<UniversalCoordinatePositionComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var claimedData = claimed[i];
                    if (claimedData.Claimed)
                    {
                        continue;
                    }
                    var targetPos = positions[i].Value;
                    if(!regionMap.TryGetValue(targetPos, out var targetRegion) || (targetRegion & actorRegion) == 0)
                    {
                        continue;
                    }
                    targetEntity = targets[i];
                    resultErrand = new HarvestEntityErrand(
                        World.DefaultGameObjectInjectionWorld,
                        harvestErrandType,
                        targetEntity,
                        errandExecutor,
                        this);
                    break;
                }
            }
            if (targetEntity != Entity.Null)
            {
                // can't use a command buffer here, if multiple actors ask for a harvest errand on the same frame
                // they should not be given the same errand
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(targetEntity, new ErrandClaimComponent
                {
                    Claimed = true
                });
            }

            return new ImmediateErrandSourceNode<HarvestEntityErrand>(resultErrand);
        }

        public void ErrandAborted(HarvestEntityErrand errand)
        {
            Debug.LogError("[ERRANDS] Entity harvest errand aborted");
            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            commandbuffer.SetComponent(errand.targetEntity, new ErrandClaimComponent
            {
                Claimed = false
            });
        }

        public void ErrandCompleted(HarvestEntityErrand errand)
        {
            Debug.Log("[ERRANDS] Entity harvest errand completed");
            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            commandbuffer.RemoveComponent<ErrandClaimComponent>(errand.targetEntity);
        }

    }
}
