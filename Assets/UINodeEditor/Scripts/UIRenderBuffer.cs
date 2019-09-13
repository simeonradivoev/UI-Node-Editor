using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UINodeEditor
{
    /// <summary>
    /// Render buffer is a middle step between the node calculations and the final command buffer that is used by unity.
    /// It holds info on meshes to be rendered.
    /// </summary>
	public class UIRenderBuffer
	{
		private struct RenderElement
		{
			public Mesh Mesh;
			public Material Material;
			public Matrix4x4 Matrix;
			public MaterialPropertyBlock PropertyBlock;
		}

		private Queue<RenderElement> m_RenderElements = new Queue<RenderElement>();

		public void Render(Mesh mesh, Vector2 pos, Material material, MaterialPropertyBlock propertyBlock)
		{
			m_RenderElements.Enqueue(new RenderElement() { Mesh = mesh, Material = material, Matrix = Matrix4x4.Translate(pos), PropertyBlock = propertyBlock });
		}

		public void Render(Mesh mesh, Vector2 pos, Material material)
		{
			m_RenderElements.Enqueue(new RenderElement() { Mesh = mesh, Material = material, Matrix = Matrix4x4.Translate(pos) });
		}

		public void Render(Mesh mesh, Matrix4x4 matrix, Material material, MaterialPropertyBlock propertyBlock)
		{
			m_RenderElements.Enqueue(new RenderElement(){Mesh = mesh,Material = material,Matrix = matrix,PropertyBlock = propertyBlock});
		}

		public void Render(Mesh mesh, Matrix4x4 matrix, Material material)
		{
			m_RenderElements.Enqueue(new RenderElement() { Mesh = mesh, Material = material, Matrix = matrix});
		}

        /// <summary>
        /// Populate a command buffer used by unity with all the render actions enqueued.
        /// </summary>
        /// <remarks>Note that the command buffer is not cleared and should be done so before calling this method.</remarks>
        /// <param name="commandBuffer">The command buffer to populate.</param>
        /// <param name="matrix">The global matrix that will multiple all render actions.</param>
		public void Populate(CommandBuffer commandBuffer,Matrix4x4 matrix)
		{
			foreach (var element in m_RenderElements)
			{
				commandBuffer.DrawMesh(element.Mesh, matrix * element.Matrix, element.Material,0,0,element.PropertyBlock);
			}
		}

        /// <summary>
        /// Clear all queued render actions.
        /// </summary>
		public void Clear()
		{
			m_RenderElements.Clear();
		}
	}
}