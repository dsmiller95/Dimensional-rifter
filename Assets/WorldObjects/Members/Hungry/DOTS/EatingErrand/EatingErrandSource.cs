using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry.DOTS.EatingErrand
{
    public class EatingErrandSource : MonoBehaviour, IErrandSource<EatingEntityErrand>, IErrandCompletionReciever<EatingEntityErrand>
    {
        public ErrandType eatingErrandType;
        public ErrandType ErrandType => eatingErrandType;
        public ErrandBoard errandBoard;

        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityArchetype errandRequestArchetype;

        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(SpecificResourceConsumeRequestComponent),
                typeof(UniversalCoordinatePositionComponent));

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<EatingEntityErrand> GetErrand(GameObject errandExecutor)
        {
            var hungryBoi = errandExecutor.GetComponent<Hungry>();

            var eatingErrandRequest = new SpecificResourceConsumeRequestComponent
            {
                DataIsSet = true,
                ItemSourceTypeFlags = uint.MaxValue,
                resourceToConsume = Resource.FOOD,
                maxResourceConsume = hungryBoi.MaxAmountCanBeEatenOfResource(Resource.FOOD)
            };

            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Harvest errand executor has no navigation member. Needed to discern position of actor");
                return null;
            }
            var actorPos = tileMem.CoordinatePosition;

            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var worldToUse = World.DefaultGameObjectInjectionWorld;
            var entityManager = worldToUse.EntityManager;
            var entity = entityManager.CreateEntity(errandRequestArchetype);
#if UNITY_EDITOR
            entityManager.SetName(entity, "EatingErrandRequest");
#endif
            commandbuffer.SetComponent(entity, eatingErrandRequest);
            commandbuffer.SetComponent(entity, new UniversalCoordinatePositionComponent
            {
                Value = actorPos
            });
            return new ErrandFromBlackboardDataNode<EatingEntityErrand>(
                    new WaitForECSResponse<SpecificResourceErrandResultComponent, BeginInitializationEntityCommandBufferSystem>(
                        entity,
                        worldToUse,
                        "eatingErrandData"
                        ),
                    Blackboard =>
                    {
                        Blackboard.TryGetValueOfType("eatingErrandData", out SpecificResourceErrandResultComponent errandResult);
                        return new EatingEntityErrand(errandResult, worldToUse, errandExecutor, this);
                    }
                );
        }

        public void ErrandAborted(EatingEntityErrand errand)
        {
            Debug.LogError("[ERRANDS][EAT] errand aborted");
        }

        public void ErrandCompleted(EatingEntityErrand errand)
        {
            Debug.Log("[ERRANDS][EAT] errand completed");
        }
    }
}
