using Assets.Behaviors.Errands.Scripts;
using Assets.WorldObjects;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.ErrandClaims
{
    public abstract class BasicErrandSource<E, Request, Response> :
        MonoBehaviour, IErrandSource<E>, IErrandCompletionReciever<E>
        where E: IErrand
        where Request : unmanaged, IComponentData
        where Response : unmanaged, IComponentData
    {
        public ErrandType errandSourceType;
        public ErrandType ErrandType => errandSourceType;
        public ErrandBoard errandBoard;

        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityArchetype errandRequestArchetype;

        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            errandRequestArchetype = entityManager.CreateArchetype(
                typeof(Request),
                typeof(UniversalCoordinatePositionComponent));

            SaveSystemHooks.Instance.PostLoad += RegisterSelfAsErrandSource;
        }

        private void RegisterSelfAsErrandSource()
        {
            errandBoard.RegisterErrandSource((IErrandSource<IErrand>)this);
        }

        protected abstract Request GenerateRequestComponent(GameObject errandExecutor);
        protected abstract E GenerateErrandFromResponse(Response response, GameObject errandExecutor);

        public IErrandSourceNode<E> GetErrand(GameObject errandExecutor)
        {
            var errandRequestData = GenerateRequestComponent(errandExecutor);

            var tileMem = errandExecutor.GetComponent<TileMapNavigationMember>();
            if (tileMem == null)
            {
                Debug.LogError("Errand executor has no navigation member. Needed to discern position of actor");
                return null;
            }
            var actorPos = tileMem.CoordinatePosition;

            var commandbuffer = commandbufferSystem.CreateCommandBuffer();
            var worldToUse = World.DefaultGameObjectInjectionWorld;
            var entityManager = worldToUse.EntityManager;
            var entity = entityManager.CreateEntity(errandRequestArchetype);
#if UNITY_EDITOR
            entityManager.SetName(entity, "ErrandRequest");
#endif
            commandbuffer.SetComponent(entity, errandRequestData);
            commandbuffer.SetComponent(entity, new UniversalCoordinatePositionComponent
            {
                Value = actorPos
            });
            return new ErrandFromBlackboardDataNode<E>(
                    new WaitForECSResponse<Response, BeginInitializationEntityCommandBufferSystem>(
                        entity,
                        worldToUse,
                        "errandData"
                        ),
                    Blackboard =>
                    {
                        Blackboard.TryGetValueOfType("errandData", out Response errandResult);
                        return GenerateErrandFromResponse(errandResult, errandExecutor);
                    }
                );
        }

        public void ErrandAborted(E errand)
        {
            Debug.LogError("[ERRANDS] errand aborted");
        }

        public void ErrandCompleted(E errand)
        {
            Debug.Log("[ERRANDS] errand completed");
        }
    }
}
