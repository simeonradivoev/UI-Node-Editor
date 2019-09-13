using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;
using UnityEngine.Sprites;

namespace UINodeEditor.Elements
{
	[Title("Elements","Image (Sliced)")]
	public class SlicedImageNode : GraphicNode
	{
		[SerializeField] private bool m_FillCenter = true;

		private struct SpriteData
		{
			public bool hasSprite;
			public Vector4 padding;
			public Vector4 outerUv;
			public Vector4 innerUv;
			public Vector4 border;
		}

		[DefaultControl(label = "Fill Center")]
		public bool fillCenter
		{
			get { return m_FillCenter; }
			set
			{
				m_FillCenter = value;
				OnMeshDirty();
			}
		}

		private SlotChangeListener<Sprite> m_SpriteInput;
		private SpriteData m_SpriteData;

		public SlicedImageNode()
		{
			name = "Sliced Image";
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
						outerUv = DataUtility.GetOuterUV(sprite),
						innerUv = DataUtility.GetInnerUV(sprite),
						border = sprite.border,
						hasSprite = true
					};
				else
					m_SpriteData = new SpriteData();
			}
			else if (eData.EventType == UIEventType.PreRepaint)
			{
				var color = m_Color[this];
                var vertexHelper = eData.MeshRepository.GetVertexHelper(guid);
                GenerateSlicedSprite(vertexHelper, m_SpriteData, rect, color);
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

				eData.RenderBuffer.Render(mesh, matrix * Matrix4x4.Translate(new Vector3(0, 0, m_ZOffset[this])), mat, propertyBlock);
			}
			base.Execute(eData, rect);
		}

		private readonly Vector2[] s_VertScratch = new Vector2[4];
		private readonly Vector2[] s_UVScratch = new Vector2[4];

		private void GenerateSlicedSprite(UIVertexHelper vertexHelper, SpriteData sprite, Rect rect, Color color)
		{
			Vector4 outer = sprite.outerUv;
			Vector4 inner = sprite.innerUv;
			Vector4 padding = sprite.padding;
			Vector4 border = sprite.border;

			Vector4 adjustedBorders = border;

			s_VertScratch[0] = new Vector2(padding.x, padding.y);
			s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

			s_VertScratch[1].x = adjustedBorders.x;
			s_VertScratch[1].y = adjustedBorders.y;

			s_VertScratch[2].x = rect.width - adjustedBorders.z;
			s_VertScratch[2].y = rect.height - adjustedBorders.w;

			for (int i = 0; i < 4; ++i)
			{
				s_VertScratch[i].x += rect.x;
				s_VertScratch[i].y += rect.y;
			}

			s_UVScratch[0] = new Vector2(outer.x, outer.y);
			s_UVScratch[1] = new Vector2(inner.x, inner.y);
			s_UVScratch[2] = new Vector2(inner.z, inner.w);
			s_UVScratch[3] = new Vector2(outer.z, outer.w);

			vertexHelper.Clear();

			for (int x = 0; x < 3; ++x)
			{
				int x2 = x + 1;

				for (int y = 0; y < 3; ++y)
				{
					if (!m_FillCenter && x == 1 && y == 1)
						continue;

					int y2 = y + 1;


					AddQuad(vertexHelper,
						new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
						new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
						color,
						new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
						new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
				}
			}
		}
	}
}