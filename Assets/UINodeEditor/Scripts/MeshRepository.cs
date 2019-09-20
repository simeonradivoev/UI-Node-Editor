using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UINodeEditor
{
    /// <summary>
    /// A mesh repository that stores meshes for later rendering.
    /// Each mesh, material property block and UIVertex helper is referenced by a GUID.
    /// It also has an internal pool for meshes, property blocks and vertex helpers.
    /// Used by <see cref="UIGraphRenderer"/>.
    /// As the repository uses an internal mesh pool it needs to be disposed for them to be deleted as well.
    /// </summary>
    public class MeshRepository : IDisposable
    {
        private Dictionary<Guid, Mesh> m_CurrentMeshes = new Dictionary<Guid, Mesh>();

        private Dictionary<Guid, MaterialPropertyBlock> m_CurrentPropertyBlocks = new Dictionary<Guid, MaterialPropertyBlock>();

        private Dictionary<Guid, UIVertexHelper> m_CurrentVertexHelpers = new Dictionary<Guid, UIVertexHelper>();

        private Stack<Mesh> m_MeshPool = new Stack<Mesh>();

        private Stack<MaterialPropertyBlock> m_PropertyBlockPool = new Stack<MaterialPropertyBlock>();

        private ConcurrentStack<UIVertexHelper> m_VertexHelpersPool = new ConcurrentStack<UIVertexHelper>();

        /// <summary>
        /// Get a new or existing mesh with a specified guid.
        /// </summary>
        /// <param name="guid">The guid of the mesh.</param>
        /// <returns>The new or existing mesh.</returns>
        public Mesh GetMesh(Guid guid)
        {
            if (!m_CurrentMeshes.TryGetValue(guid, out Mesh mesh))
            {
                mesh = m_MeshPool.Count > 0 ? m_MeshPool.Pop() : new Mesh();
                m_CurrentMeshes.Add(guid, mesh);
            }

            return mesh;
        }

        /// <summary>
        /// Get a new or existing material property block with a specified guid.
        /// </summary>
        /// <param name="guid">The guid of the material property block.</param>
        /// <returns>The new or existing material property block.</returns>
        public MaterialPropertyBlock GetPropertyBLock(Guid guid)
        {
            if (!m_CurrentPropertyBlocks.TryGetValue(guid, out MaterialPropertyBlock block))
            {
                block = m_PropertyBlockPool.Count > 0 ? m_PropertyBlockPool.Pop() : new MaterialPropertyBlock();
                m_CurrentPropertyBlocks.Add(guid,block);
            }

            return block;
        }

        /// <summary>
        /// Thread safe way of getting a new or existing vertex helper with a specified guid.
        /// </summary>
        /// <param name="guid">The guid of the vertex helper.</param>
        /// <returns>The new or existing vertex helper.</returns>
        public UIVertexHelper GetVertexHelper(Guid guid)
        {
            if (!m_CurrentVertexHelpers.TryGetValue(guid, out UIVertexHelper vertexHelper))
            {
                if (!m_VertexHelpersPool.TryPop(out vertexHelper))
                {
                    vertexHelper = new UIVertexHelper();
                    m_CurrentVertexHelpers.Add(guid,vertexHelper);
                }
            }

            return vertexHelper;
        }

        /// <summary>
        /// Clear all current meshes, property blocks and vertex helpers.
        /// </summary>
        /// <param name="delete">Should pooling be enabled or should all elements be deleted.</param>
        public void ClearCurrent(bool delete = false)
        {
            foreach (var kvp in m_CurrentMeshes)
            {
                if (delete)
                {
                    Object.DestroyImmediate(kvp.Value);
                }
                else
                {
                    m_MeshPool.Push(kvp.Value);
                }
            }

            if (!delete)
            {
                foreach (var kvp in m_CurrentPropertyBlocks)
                {
                    m_PropertyBlockPool.Push(kvp.Value);
                    kvp.Value.Clear();
                }

                foreach (var kvp in m_CurrentPropertyBlocks)
                {
                    m_PropertyBlockPool.Push(kvp.Value);
                    kvp.Value.Clear();
                }
            }

            m_CurrentMeshes.Clear();
            m_CurrentPropertyBlocks.Clear();
            m_CurrentVertexHelpers.Clear();
        }

        /// <summary>
        /// Deletes all current elements as well as pooled ones.
        /// </summary>
        public void Dispose()
        {
            foreach (var kvp in m_CurrentMeshes)
            {
                Object.DestroyImmediate(kvp.Value);
            }

            m_CurrentMeshes.Clear();
            m_CurrentPropertyBlocks.Clear();
            m_CurrentVertexHelpers.Clear();

            foreach (var mesh in m_MeshPool)
            {
                Object.DestroyImmediate(mesh);
            }

            m_MeshPool.Clear();
            m_PropertyBlockPool.Clear();
            m_VertexHelpersPool.Clear();
        }
    }
}