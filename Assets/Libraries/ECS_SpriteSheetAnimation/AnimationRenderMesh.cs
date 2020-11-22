using System;
using Unity.Entities;

namespace ECS_SpriteSheetAnimation
{
    public struct AnimationRenderMesh : ISharedComponentData, IEquatable<AnimationRenderMesh>
    {
        public UnityEngine.Mesh mesh;
        public UnityEngine.Material material;
        public int frameCountX;
        public int frameCountY;
        public int totalFrames;

        public bool Equals(AnimationRenderMesh other)
        {
            return mesh == other.mesh &&
                material == other.material &&
                frameCountX == other.frameCountX &&
                frameCountY == other.frameCountY;
        }

        public override int GetHashCode()
        {
            var hash = new Hash128(
                (uint)mesh.GetHashCode(),
                (uint)material.GetHashCode(),
                (uint)frameCountX,
                (uint)frameCountY);

            return hash.Value.GetHashCode();
        }

        /// <summary>
        /// Animation index is 0 at top-left, increases left-to-right, top-to-bottom
        /// </summary>
        public UnityEngine.Vector4 GetUVAtIndex(int index)
        {
            var indexX = index % frameCountX;
            // index progresses top-to-bottom
            var indexY = (frameCountY - 1) - (index / frameCountX);
            float uvPerX = 1f / frameCountX;
            float uvPerY = 1f / frameCountY;
            return new UnityEngine.Vector4(
                uvPerX,
                uvPerY,
                uvPerX * indexX,
                uvPerY * indexY);
        }
    }
}
