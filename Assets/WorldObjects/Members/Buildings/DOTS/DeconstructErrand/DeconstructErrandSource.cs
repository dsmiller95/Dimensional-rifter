using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.DeconstructErrand
{
    public class DeconstructErrandSource : MonoBehaviour, IErrandSource<DeconstructErrand>, IErrandCompletionReciever<DeconstructErrand>
    {
        public ErrandType deconstructErrandSource;
        public ErrandType ErrandType => deconstructErrandSource;
        public ErrandBoard errandBoard;

        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityArchetype errandRequestArchetype;

        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(DeconstructErrandRequestComponent),
                typeof(UniversalCoordinatePositionComponent));

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<DeconstructErrand> GetErrand(GameObject errandExecutor)
        {
            var eatingErrandRequest = new DeconstructErrandRequestComponent
            {
                DataIsSet = true,
            };

            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Deconstruct errand executor has no navigation member. Needed to discern position of actor");
                return null;
            }
            var actorPos = tileMem.CoordinatePosition;

            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var worldToUse = World.DefaultGameObjectInjectionWorld;
            var entityManager = worldToUse.EntityManager;
            var entity = entityManager.CreateEntity(errandRequestArchetype);
#if UNITY_EDITOR
            entityManager.SetName(entity, "DeconstructErrandRequest");
#endif
            commandbuffer.SetComponent(entity, eatingErrandRequest);
            commandbuffer.SetComponent(entity, new UniversalCoordinatePositionComponent
            {
                Value = actorPos
            });
            return new ErrandFromBlackboardDataNode<DeconstructErrand>(
                    new WaitForECSResponse<DeconstructErrandResultComponent, BeginInitializationEntityCommandBufferSystem>(
                        entity,
                        worldToUse,
                        "deconstructErrandData"
                        ),
                    Blackboard =>
                    {
                        Blackboard.TryGetValueOfType("deconstructErrandData", out DeconstructErrandResultComponent errandResult);
                        return new DeconstructErrand(errandResult, worldToUse, errandExecutor, this);
                    }
                );
        }

        public void ErrandAborted(DeconstructErrand errand)
        {
            Debug.LogError("[ERRANDS][DECONSTRUCT] errand aborted");
        }

        public void ErrandCompleted(DeconstructErrand errand)
        {
            Debug.Log("[ERRANDS][DECONSTRUCT] errand completed");
        }
    }
}
