using UnityEngine;

namespace Assets.WorldObjects
{
    [CreateAssetMenu(fileName = "OrderingLayer", menuName = "TileMap/OrderingLayer", order = 10)]
    public class TileMemberOrderingLayer : ScriptableObject
    {
        public float zOffsetPerY = 0.1f;
        public float netZOffset = -100f;

        public Vector3 ApplyPositionInOrderingLayer(Vector2 worldPosition)
        {
            return new Vector3(worldPosition.x, worldPosition.y, worldPosition.y * zOffsetPerY + netZOffset);
        }
    }
}
