using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.UI.Manipulators.Scripts.SelectedArea;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts
{
    public class SelectedAreaVisualizer : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private float zPos = -400;


        public void RenderRange(SquareCoordinateRange range, short coordinatePlaneId)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var squareTileArchetype = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(SelectedTile)
            );
            entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(SelectedTile)));
            //clear all selection area objects

            var newCoordinateNum = range.TotalCoordinateContents();
            NativeArray<Entity> entityArray = new NativeArray<Entity>(newCoordinateNum, Allocator.Temp);
            entityManager.CreateEntity(squareTileArchetype, entityArray);
            var bigManager = CombinationTileMapManager.instance;
            var coordGenerator = range.GetEnumerator();
            for (int i = 0; i < newCoordinateNum; i++)
            {
                if (!coordGenerator.MoveNext())
                {
                    Debug.LogError("Not enough coordinates");
                    break;
                }
                var nextCoordSquare = coordGenerator.Current;
                var nextUniversalCoord = UniversalCoordinate.From(nextCoordSquare, coordinatePlaneId);
                Entity entity = entityArray[i];
                var coordPosition = bigManager.PositionInRealWorld(nextUniversalCoord);
                var newTranslate = new float3(coordPosition.x, coordPosition.y, zPos);
                Debug.Log($"setting position to {newTranslate}");
                entityManager.SetComponentData(entity,
                    new Translation
                    {
                        Value = newTranslate
                    }
                );
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = mesh,
                    material = material,
                });
            }

            Debug.Log($"Created {newCoordinateNum} entities");

            entityArray.Dispose();
        }
    }
}
