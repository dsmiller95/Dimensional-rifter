using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    public class BuildGhostEntityErrandSource : MonoBehaviour, IErrandSource<BuildEntityErrand>, IErrandCompletionReciever<BuildEntityErrand>
    {
        public BuildingErrandType buildingErrandType;
        public ErrandType ErrandType => buildingErrandType;
        public ErrandBoard errandBoard;


        private EntityQuery errandTargetQuery;
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandTargetQuery = entityManager.CreateEntityQuery(
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
            Entity targetEntity = Entity.Null;
            BuildEntityErrand resultErrand = null;
            using (var targets = errandTargetQuery.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var claimed = errandTargetQuery.ToComponentDataArray<ErrandClaimComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var claimedData = claimed[i];
                    if (claimedData.Claimed)
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
