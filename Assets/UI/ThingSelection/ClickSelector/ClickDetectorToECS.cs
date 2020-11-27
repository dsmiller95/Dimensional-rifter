using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.ThingSelection.ClickSelector
{
    /// <summary>
    /// Detects clicks and spawns a new entity with a <see cref="UniversalCoordinatePositionComponent"/> to represent the tile that was clicked, or dragged
    /// 
    /// On mouse down begin listening, if the mouse-ups on the same tile as it went down create a mouse clicked event
    ///     if the mouse moves to another tile while down, immediately create a new drag component
    /// </summary>
    public class ClickDetectorToECS : MonoBehaviour
    {
        private EntityArchetype mouseClickEventArchetype;
        private EntityArchetype mouseDragEventArchetype;
        EntityCommandBufferSystem commandBufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            mouseClickEventArchetype = entityManager.CreateArchetype(
                typeof(UniversalCoordinatePositionComponent),
                typeof(ClickEventFlagComponent)
                );
            mouseDragEventArchetype = entityManager.CreateArchetype(
                typeof(UniversalCoordinatePositionComponent),
                typeof(DragEventStateComponent),
                typeof(DragEventComponent)
                );
        }

        private Entity draggingEntity;
        private UniversalCoordinate mouseDownPosition = default;
        private UniversalCoordinate lastMousePosition = default;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var coord = MousePosInMap();
                if (!coord.HasValue) return;
                mouseDownPosition = coord.Value;
                lastMousePosition = mouseDownPosition;
            }
            else if (mouseDownPosition.IsValid() && Input.GetMouseButtonUp(0))
            {
                var nextCoord = MousePosInMap();
                var commandBuffer = commandBufferSystem.CreateCommandBuffer();
                if (nextCoord.HasValue)
                {
                    if (draggingEntity == Entity.Null)
                    {
                        if (nextCoord.Value == mouseDownPosition)
                            SpawnClickEvent(mouseDownPosition, commandBuffer);
                        else
                            // haven't registered as dragging yet, but this is a drag. create a completed drag event
                            SpawnCompleteDragEvent(mouseDownPosition, nextCoord.Value, commandBuffer);
                    }
                    else
                    {
                        UpdateActiveDragPosition(nextCoord.Value, commandBuffer);
                    }
                }
                else if (draggingEntity == Entity.Null)
                {
                    SpawnClickEvent(mouseDownPosition, commandBuffer);
                }

                if (draggingEntity != Entity.Null)
                {
                    CompleteDrag(commandBuffer);
                }
                mouseDownPosition = default;
                lastMousePosition = default;
            }
            else if (mouseDownPosition.IsValid() && Input.GetMouseButton(0))
            {
                var nextCoord = MousePosInMap();
                if (!nextCoord.HasValue)
                {
                    lastMousePosition = default;
                    return;
                }

                if (nextCoord != mouseDownPosition && lastMousePosition != nextCoord)
                {
                    lastMousePosition = nextCoord.Value;
                    if (draggingEntity == Entity.Null)
                    {
                        CreatePendingDragEvent(mouseDownPosition, nextCoord.Value);
                    }
                    else
                    {
                        var commandBuffer = commandBufferSystem.CreateCommandBuffer();
                        UpdateActiveDragPosition(nextCoord.Value, commandBuffer);
                    }
                }
            }
        }

        private void CreatePendingDragEvent(UniversalCoordinate origin, UniversalCoordinate pendingDragPoint)
        {
            draggingEntity = entityManager.CreateEntity(mouseDragEventArchetype);
            entityManager.SetComponentData(draggingEntity, new UniversalCoordinatePositionComponent
            {
                Value = origin
            });
            entityManager.SetComponentData(draggingEntity, new DragEventComponent
            {
                dragPos = pendingDragPoint
            });
            entityManager.SetComponentData(draggingEntity, new DragEventStateComponent
            {
                dragDone = false
            });
        }

        private void CompleteDrag(EntityCommandBuffer commandBuffer)
        {
            commandBuffer.SetComponent(draggingEntity, new DragEventStateComponent
            {
                dragDone = true
            });
            draggingEntity = Entity.Null;
        }

        private void UpdateActiveDragPosition(UniversalCoordinate nextPosition, EntityCommandBuffer commandBuffer)
        {
            commandBuffer.SetComponent(draggingEntity, new DragEventComponent
            {
                dragPos = nextPosition
            });
        }

        private void SpawnCompleteDragEvent(UniversalCoordinate origin, UniversalCoordinate finishedPoint, EntityCommandBuffer commandBuffer)
        {
            var draggingEntity = commandBuffer.CreateEntity(mouseDragEventArchetype);
            commandBuffer.SetComponent(draggingEntity, new UniversalCoordinatePositionComponent
            {
                Value = origin
            });
            commandBuffer.SetComponent(draggingEntity, new DragEventComponent
            {
                dragPos = finishedPoint
            });
            commandBuffer.SetComponent(draggingEntity, new DragEventStateComponent
            {
                dragDone = true
            });
            draggingEntity = Entity.Null;
        }

        private void SpawnClickEvent(UniversalCoordinate clickPosition, EntityCommandBuffer? commandBuffer = null)
        {
            if (!commandBuffer.HasValue)
                commandBuffer = commandBufferSystem.CreateCommandBuffer();
            var clickedEvent = commandBuffer.Value.CreateEntity(mouseClickEventArchetype);
            commandBuffer.Value.SetComponent(clickedEvent, new UniversalCoordinatePositionComponent
            {
                Value = clickPosition
            });
        }

        private UniversalCoordinate? MousePosInMap()
        {
            var posInWorld = MyUtilities.GetMousePos2D();
            return CombinationTileMapManager.instance.GetValidCoordinateFromWorldPosIfExists(posInWorld);
        }
    }
}