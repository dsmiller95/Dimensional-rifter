using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public class SleepStationOccupyErrandSource : MonoBehaviour, IErrandSource<SleepStationOccupyErrand>, IErrandCompletionReciever<SleepStationOccupyErrand>
    {
        public ErrandType sleepingErrandSource;
        public ErrandType ErrandType => sleepingErrandSource;
        public ErrandBoard errandBoard;

        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityArchetype errandRequestArchetype;

        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(SleepStationOccupyRequestComponent),
                typeof(UniversalCoordinatePositionComponent));

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource(this);
        }

        public IErrandSourceNode<SleepStationOccupyErrand> GetErrand(GameObject errandExecutor)
        {
            var eatingErrandRequest = new SleepStationOccupyRequestComponent
            {
                DataIsSet = true,
            };

            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Sleep station errand executor has no navigation member. Needed to discern position of actor");
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
            return new ErrandFromBlackboardDataNode<SleepStationOccupyErrand>(
                    new WaitForECSResponse<SleepStationOccupyErrandResultComponent, BeginInitializationEntityCommandBufferSystem>(
                        entity,
                        worldToUse,
                        "sleepingErrandData"
                        ),
                    Blackboard =>
                    {
                        Blackboard.TryGetValueOfType("sleepingErrandData", out SleepStationOccupyErrandResultComponent errandResult);
                        return new SleepStationOccupyErrand(errandResult, worldToUse, errandExecutor, this);
                    }
                );
        }

        public void ErrandAborted(SleepStationOccupyErrand errand)
        {
            Debug.LogError("[ERRANDS][EAT] errand aborted");
        }

        public void ErrandCompleted(SleepStationOccupyErrand errand)
        {
            Debug.Log("[ERRANDS][EAT] errand completed");
        }
    }
}
