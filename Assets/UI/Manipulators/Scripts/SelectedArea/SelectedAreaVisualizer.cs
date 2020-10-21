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
    [RequireComponent(typeof(MeshRenderer))]
    public class SelectedAreaVisualizer : MonoBehaviour
    {


        public void RenderRange(SquareCoordinateRange range, short coordinatePlaneId)
        {
            var rootCoordinate = UniversalCoordinate.From(range.coord0, coordinatePlaneId);
            var root = CombinationTileMapManager.instance.PositionInRealWorld(rootCoordinate);
            var extentCoordinate = UniversalCoordinate.From(range.MaximumBound, coordinatePlaneId);
            var extent = CombinationTileMapManager.instance.PositionInRealWorld(extentCoordinate);

            Vector3 center = (root + extent) / 2;
            center.z = transform.position.z;
            Vector3 scale = extent - root + Vector2.one;
            scale.z = 1;

            transform.position = center;
            transform.localScale = scale;
        }
    }
}
