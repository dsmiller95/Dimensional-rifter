using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.ThingSelection
{
    /// <summary>
    /// Detects clicks and spawns a new entity with a <see cref="UniversalCoordinatePositionComponent"/> to represent the tile that was clicked
    /// </summary>
    public class ClickDetectorToECS : MonoBehaviour
    {
        private EntityArchetype mouseClickEventArchetype;
        EntityCommandBufferSystem commandBufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            mouseClickEventArchetype = entityManager.CreateArchetype(
                typeof(UniversalCoordinatePositionComponent),
                typeof(ClickEventComponent)
                );
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var posInWorld = MyUtilities.GetMousePos2D();
                var coord = CombinationTileMapManager.instance.GetValidCoordinateFromWorldPosIfExists(posInWorld);
                if (!coord.HasValue) return;

                var commandBuffer = commandBufferSystem.CreateCommandBuffer();
                var clickedEvent = commandBuffer.CreateEntity(mouseClickEventArchetype);
                commandBuffer.SetComponent(clickedEvent, new UniversalCoordinatePositionComponent
                {
                    Value = coord.Value
                });
            }
        }
    }
}