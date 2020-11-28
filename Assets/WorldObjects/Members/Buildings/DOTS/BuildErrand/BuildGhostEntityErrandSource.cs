using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Wall.DOTS;
using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.BuildErrand
{
    public class BuildGhostEntityErrandSource : MonoBehaviour, IErrandSource<BuildEntityErrand>, IErrandCompletionReciever<BuildEntityErrand>
    {
        public BuildingErrandType buildingErrandType;
        public ErrandType ErrandType => buildingErrandType;
        public ErrandBoard errandBoard;


        private EntityQuery ErrandTargetQuery;
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConnectivityEntitySystem>();
        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ErrandTargetQuery = entityManager.CreateEntityQuery(
                typeof(ErrandClaimComponent),
                ComponentType.ReadOnly<IsNotBuiltFlag>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<BuildEntityErrand> GetErrand(GameObject errandExecutor)
        {
            var connectionSystem = ConnectivitySystem;
            if (!connectionSystem.HasRegionMaps)
            {
                return null;
            }
            var regionMap = connectionSystem.Regions;
            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Build ghost errand executor has no navigation member. Needed to discern position of actor");
                return null;
            }
            var actorPos = tileMem.CoordinatePosition;
            if (!regionMap.TryGetValue(actorPos, out var actorRegion))
            {
                Debug.LogError("actor not included in region map");
                return null;
            }

            Entity targetEntity = Entity.Null;
            BuildEntityErrand resultErrand = null;
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
                    if (!regionMap.TryGetValue(targetPos, out var targetRegion) || (targetRegion & actorRegion) == 0)
                    {
                        continue;
                    }
                    targetEntity = targets[i];
                    resultErrand = new BuildEntityErrand(
                        World.DefaultGameObjectInjectionWorld,
                        buildingErrandType,
                        targetEntity,
                        errandExecutor,
                        this);
                    break;
                }
            }
            if (targetEntity != Entity.Null)
            {
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(targetEntity, new ErrandClaimComponent
                {
                    Claimed = true
                });
            }

            return new ImmediateErrandSourceNode<BuildEntityErrand>(resultErrand);
        }

        public void ErrandAborted(BuildEntityErrand errand)
        {
            Debug.LogError("[ERRANDS] Entity harvest errand aborted");
            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            commandbuffer.SetComponent(errand.toBeBuilt, new ErrandClaimComponent
            {
                Claimed = false
            });
        }

        public void ErrandCompleted(BuildEntityErrand errand)
        {
            Debug.Log("[ERRANDS] Entity harvest errand completed");
            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            commandbuffer.RemoveComponent<ErrandClaimComponent>(errand.toBeBuilt);
        }
    }
}
