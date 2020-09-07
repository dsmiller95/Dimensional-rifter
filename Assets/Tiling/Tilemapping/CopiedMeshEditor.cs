using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MapGen
{
    public class CopiedMeshEditor
    {
        private Mesh sourceMesh;
        public Mesh targetMesh;
        private int sourceMeshVertexSize;

        public CopiedMeshEditor(
            int sourceMeshVertexSize,
            Mesh targetMesh,
            Mesh sourceMesh)
        {
            this.sourceMesh = sourceMesh;
            this.targetMesh = targetMesh;
            this.sourceMeshVertexSize = sourceMeshVertexSize;
        }

        public void SetUVForVertexesAtDuplicate(int duplicateIndex, Vector2[] uvs)
        {
            if (uvs.Length != sourceMeshVertexSize)
            {
                throw new Exception("UV override length must match number of source vertexes exactly");
            }

            var newUvs = targetMesh.uv;

            var beginningColorIndex = duplicateIndex * sourceMeshVertexSize;
            Array.Copy(uvs, 0, newUvs, beginningColorIndex, sourceMeshVertexSize);

            targetMesh.uv = newUvs;
        }


        /// <summary>
        /// find the vertexes the were created as <paramref name="duplicateIndex"/> and set them to <paramref name="color"/>
        /// </summary>
        /// <param name="duplicateIndex"></param>
        /// <param name="color"></param>
        public void SetColorOnVertexesAtDuplicate(int duplicateIndex, Color32 color)
        {
            var newColors = targetMesh.colors32;

            SetColorOnColorArray(newColors, duplicateIndex, color);

            targetMesh.colors32 = newColors;
        }

        public void SetColorsOnVertexesAtDuplicates(IEnumerable<(int, Color32)> duplicateColors)
        {
            var newColors = targetMesh.colors32;

            foreach (var duplicate in duplicateColors)
            {
                SetColorOnColorArray(newColors, duplicate.Item1, duplicate.Item2);
            }

            targetMesh.colors32 = newColors;
        }

        private void SetColorOnColorArray(Color32[] colorArray, int duplicateIndex, Color32 color)
        {
            var beginningColorIndex = duplicateIndex * sourceMeshVertexSize;
            var endingColorIndex = beginningColorIndex + sourceMeshVertexSize;
            for (var i = beginningColorIndex; i < endingColorIndex; i++)
            {
                colorArray[i] = color;
            }
        }
        public void DisableGeometryAtDuplicate(int duplicateIndex)
        {
            AddToVectorsAtDuplicate(duplicateIndex, MeshCopier.EjectionVector);
        }

        public void EnableGeometryAtDuplicate(int duplicateIndex)
        {
            AddToVectorsAtDuplicate(duplicateIndex, -MeshCopier.EjectionVector);
        }

        private void AddToVectorsAtDuplicate(int duplicate, Vector3 offset)
        {
            var vertices = targetMesh.vertices;

            var beginningColorIndex = duplicate * sourceMeshVertexSize;
            var endingColorIndex = beginningColorIndex + sourceMeshVertexSize;
            for (var i = beginningColorIndex; i < endingColorIndex; i++)
            {
                vertices[i] += offset;
            }

            targetMesh.vertices = vertices;
        }
    }
}
