using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UINodeEditor
{
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

		public void Populate(CommandBuffer commandBuffer,Matrix4x4 matrix)
		{
			foreach (var element in m_RenderElements)
			{
				commandBuffer.DrawMesh(element.Mesh, matrix * element.Matrix, element.Material,0,0,element.PropertyBlock);
			}
		}

		public void Clear()
		{
			m_RenderElements.Clear();
		}
	}
}