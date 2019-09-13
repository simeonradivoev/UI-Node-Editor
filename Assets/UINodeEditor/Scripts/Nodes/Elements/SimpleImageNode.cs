using System;
using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;
using UnityEngine.Sprites;

namespace UINodeEditor.Elements
{
	[Title("Elements","Image (Simple)")]
	public class SimpleImageNode : GraphicNode
	{
		[SerializeField] private bool m_PreserveAspect;

		private struct SpriteData
		{
			public Vector4 padding;
			public Rect rect;
			public Vector4 outerUv;
		}

		[DefaultControl(label = "Preserve Aspect")]
		public bool preserveAspect
		{
			get { return m_PreserveAspect; }
			set
			{
				m_PreserveAspect = value;
				OnMeshDirty();
			}
		}

		private SlotChangeListener<Sprite> m_SpriteInput;
		private SpriteData m_SpriteData;

		public SimpleImageNode()
		{
			name = "Simple Image";
			m_SpriteInput = CreateSlotListener<Sprite>(CreateInputSlot<EmptySlot<Sprite>>("Sprite"), OnMeshDirty);
		}

		protected override void Execute(UIEventData eData, Rect rect)
		{
			if (eData.EventType == UIEventType.Layout)
			{
				var sprite = m_SpriteInput[this];
				if (sprite != null)
					m_SpriteData = new SpriteData()
					{
						padding = DataUtility.GetPadding(sprite),
						rect = sprite.rect,
						outerUv = DataUtility.GetOuterUV(sprite)
					};
				else
					m_SpriteData = new SpriteData()
					{
						rect = new Rect(0, 0, 100, 100),
						outerUv = new Vector4(0,0,1,1)
					};
			}
			else if (eData.EventType == UIEventType.PreRepaint)
			{
				var color = m_Color[this];
				var pivot = m_Pivot[this];
                var vertexHelper = eData.MeshRepository.GetVertexHelper(guid);
                GenerateSimpleSprite(vertexHelper, m_SpriteData, rect, pivot, color);
            }
			else if (eData.EventType == UIEventType.Repaint)
			{
				var sprite = m_SpriteInput[this];
				var matrix = m_Matrix[this];
				var mat = m_Material[this];

                var vertexHelper = eData.MeshRepository.GetVertexHelper(guid);
                var mesh = eData.MeshRepository.GetMesh(guid);
                MaterialPropertyBlock propertyBlock = null;
                vertexHelper.FillMesh(mesh);

                if (sprite != null)
                {
                    propertyBlock = eData.MeshRepository.GetPropertyBLock(guid);
                    propertyBlock.SetTexture(m_MainTexProp, sprite.texture);
                }

				eData.RenderBuffer.Render(mesh, matrix * Matrix4x4.Translate(new Vector3(0,0,m_ZOffset[this])), mat, propertyBlock);
			}
			base.Execute(eData, rect);
		}

		private void GenerateSimpleSprite(UIVertexHelper vh, SpriteData sprite, Rect rect,Vector2 pivot, Color color)
		{
			Vector4 v = GetDrawingDimensions(sprite.padding,sprite.rect.size, pivot, rect, preserveAspect);
			var uv = sprite.outerUv;

			var color32 = color;
			vh.Clear();
			vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
			vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
			vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
			vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}
	}
}