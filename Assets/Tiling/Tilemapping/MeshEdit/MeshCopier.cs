using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.MeshEdit
{
    /// <summary>
    /// A utility class used to copy duplicates of one mesh into the target mesh
    ///     currently set up to copy all the vertextes to a set of offsets
    ///     and then selectively copy the triangles
    /// </summary>
    public class MeshCopier
    {
        private Mesh sourceMesh, targetMesh;

        private int sourceSubmeshes;
        private int targetSubmeshes;

        public static readonly Vector3 EjectionVector = new Vector3(1000, 1000, 1000);

        //private Vector3[] offsets;

        public MeshCopier(
            Mesh sourceMesh, int sourceSubmeshCount,
            Mesh targetMesh, int targetSubmeshCount,
            bool append = false)
        {
            this.sourceMesh = sourceMesh;
            sourceSubmeshes = sourceSubmeshCount;
            this.targetMesh = targetMesh;
            targetSubmeshes = targetSubmeshCount;

            targetTrianglesBySubmesh = new List<int>[targetSubmeshCount];
            for (var i = 0; i < targetSubmeshCount; i++)
            {
                targetTrianglesBySubmesh[i] = new List<int>();
            }
            targetVertexes = new List<Vector3>();
            targetColors = new List<Color>();
            targetUVs = new List<Vector2>();

            var sourceVertexes = sourceMesh.vertices;
            sourceVertexCount = sourceVertexes.Length;

            targetMesh.Clear();
            targetMesh.subMeshCount = targetSubmeshes;
        }

        private int sourceVertexCount;
        private List<Vector3> targetVertexes;
        private List<Color> targetColors;
        private List<Vector2> targetUVs;
        private List<int>[] targetTrianglesBySubmesh;

        /// <summary>
        /// assigns all the vertexes, uvs, and colors to the target mesh
        /// </summary>
        public CopiedMeshEditor FinalizeCopy()
        {
            targetMesh.SetVertices(targetVertexes);
            targetMesh.SetColors(targetColors);
            targetMesh.SetUVs(0, targetUVs);

            for (var submesh = 0; submesh < targetSubmeshes; submesh++)
            {
                var submeshTriangles = targetTrianglesBySubmesh[submesh];
                targetMesh.SetTriangles(submeshTriangles, submesh);
            }
            targetMesh.RecalculateNormals();

            return new CopiedMeshEditor(sourceMesh.vertexCount, targetMesh, sourceMesh);
        }

        private int currentDuplicateIndex = -1;

        /// <summary>
        /// creates all the vertexes and uvs for the next copy of the source mesh
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>and index which can be used to modify this specific copy in the mesh post-creation</returns>
        public int NextCopy(
            Vector3 offset = default,
            Color? vertexColor = null,
            Vector2[] UVOverride = null,
            Quaternion localMeshRotation = default,
            IEnumerable<Vector3> vertexOverrides = null)
        {
            if (vertexOverrides != null)
            {
                CopyVertexOverrides(vertexOverrides);
            }
            else
            {
                CopyVertexesToOffset(offset, localMeshRotation);
            }
            CopyOrSetColors(vertexColor);
            CopyUVsToOffsetIndex(UVOverride);

            return ++currentDuplicateIndex;
        }

        public void CopySubmeshTrianglesToOffsetIndex(int sourceSubmesh, int targetSubmesh)
        {
            var sourceTriangles = sourceMesh.GetTriangles(sourceSubmesh);
            var vertexIndexOffset = currentDuplicateIndex * sourceVertexCount;

            var targetSubmeshTrianges = targetTrianglesBySubmesh[targetSubmesh];

            for (var tri = 0; tri < sourceTriangles.Length; tri++)
            {
                targetSubmeshTrianges.Add(sourceTriangles[tri] + vertexIndexOffset);
            }
        }

        private void CopyOrSetColors(Color? colorOverride)
        {
            var sourceColorSize = sourceMesh.vertexCount;
            var sourceColors = sourceMesh.colors;
            if (!colorOverride.HasValue)
            {
                for (var vert = 0; vert < sourceColorSize; vert++)
                {
                    targetColors.Add(sourceColors[vert]);
                }
            }
            else
            {
                for (var vert = 0; vert < sourceColorSize; vert++)
                {
                    targetColors.Add(colorOverride.Value);
                }
            }
        }

        private void CopyVertexOverrides(IEnumerable<Vector3> overrides)
        {
            var preAdd = targetVertexes.Count;
            targetVertexes.AddRange(overrides);
            if (targetVertexes.Count - preAdd != sourceVertexCount)
            {
                throw new System.Exception("Cannot handle a vertex override which is different than the source vertex count");
            }
        }

        private void CopyVertexesToOffset(Vector3 offset, Quaternion rotation)
        {
            var sourceVertexes = sourceMesh.vertices;
            for (var vert = 0; vert < sourceVertexes.Length; vert++)
            {
                var rotatedSource = rotation * sourceVertexes[vert];
                targetVertexes.Add(rotatedSource + offset);
            }
        }

        private void CopyUVsToOffsetIndex(Vector2[] UVOverride)
        {
            if (UVOverride == null)
            {
                var sourceUVs = new List<Vector2>();
                sourceMesh.GetUVs(0, sourceUVs);
                targetUVs.AddRange(sourceUVs);
            }
            else
            {
                if (UVOverride.Length != sourceVertexCount)
                {
                    throw new Exception("UV override length must match number of source vertexes exactly");
                }
                targetUVs.AddRange(UVOverride);
            }
        }
    }
}
