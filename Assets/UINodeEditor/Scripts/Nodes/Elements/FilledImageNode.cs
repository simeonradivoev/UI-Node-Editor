using NodeEditor;
using NodeEditor.Controls;
using NodeEditor.Slots;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UINodeEditor.Elements
{
    /// <summary>
    /// A filled image similar to the <see cref="Image"/> with filled image mode enabled.
    /// It can draw a graphic that can be filled from a value.
    /// </summary>
	[Title("Elements","Image (Filled)")]
	public class FilledImageNode : GraphicNode
	{
		private struct SpriteData
		{
			public bool hasSprite;
			public Vector4 padding;
			public Rect rect;
			public Vector4 outerUv;
			public Vector4 innerUv;
			public bool hasBorder;
			public Vector4 border;
			public bool packed;
			public TextureWrapMode wrapMode;
		}

		[SerializeField] private Image.FillMethod m_FillMethod;
		[SerializeField] private int m_FillOrigin;
		[SerializeField] private bool m_FillClockwise;
		[SerializeField] private bool m_PreserveAspect;

        /// <summary>
        /// Preserve the aspect of the sprite.
        /// </summary>
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

		[EnumControl(label = "Fill Method")]
		public Image.FillMethod fillMethod
		{
			get { return m_FillMethod; }
			set
			{
				m_FillMethod = value;
				OnMeshDirty();
			}
		}

		[DefaultControl(label = "Fill Origin")]
		public int fillOrigin
		{
			get { return m_FillOrigin; }
			set
			{
				m_FillOrigin = value;
				OnMeshDirty();
			}
		}

		[DefaultControl(label = "Fill Clockwise")]
		public bool fillClockwise
		{
			get { return m_FillClockwise; }
			set
			{
				m_FillClockwise = value;
				OnMeshDirty();
			}
		}

		private SlotChangeListener<Sprite> m_SpriteInput;
		private SpriteData m_SpriteData;
		private SlotChangeListener<float> m_FillAmount;

		public FilledImageNode()
		{
			name = "Filled Image";
			m_SpriteInput = CreateSlotListener<Sprite>(CreateInputSlot<EmptySlot<Sprite>>("Sprite"), OnMeshDirty);
			m_FillAmount = CreateSlotListener<float>(CreateInputSlot<SliderSlot>("Fill Amount").SetValue(1).SetRange(0, 1), OnMeshDirty);
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
						outerUv = DataUtility.GetOuterUV(sprite),
						innerUv = DataUtility.GetInnerUV(sprite),
						hasBorder = sprite.border.sqrMagnitude > 0,
						border = sprite.border,
						packed = sprite.packed,
						wrapMode = sprite.texture.wrapMode,
						hasSprite = true
					};
				else
					m_SpriteData = new SpriteData()
					{
						rect = new Rect(0, 0, 100, 100)
					};
			}
			else if (eData.EventType == UIEventType.PreRepaint)
			{
				var fillAmount = Mathf.Clamp01(m_FillAmount[this]);
				var color = m_Color[this];
				var pivot = m_Pivot[this];
                Rect localRect = new Rect(-pivot.x * rect.width,-pivot.y * rect.height,rect.width,rect.height);

                var vertexHelper = eData.MeshRepository.GetVertexHelper(guid);
                GenerateFilledSprite(vertexHelper, m_SpriteData, localRect, pivot, color, fillAmount);
            }
			else if (eData.EventType == UIEventType.Repaint)
			{
                var pivot = m_Pivot[this];
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
				eData.RenderBuffer.Render(mesh, Matrix4x4.Translate(new Vector3(rect.x + pivot.x * rect.width, rect.y + pivot.y * rect.height, m_ZOffset[this])) * matrix, mat, propertyBlock);
			}
			base.Execute(eData, rect);
		}

		private readonly Vector3[] s_Xy = new Vector3[4];
		private readonly Vector3[] s_Uv = new Vector3[4];

		void GenerateFilledSprite(UIVertexHelper toFill, SpriteData sprite, Rect rect,Vector2 pivot, Color color, float fillAmount)
		{
			toFill.Clear();

			if (fillAmount < 0.001f)
				return;

			Vector4 v = GetDrawingDimensions(sprite.padding,sprite.rect.size, pivot, rect, preserveAspect);
			Vector4 outer = sprite.outerUv;
			UIVertex uiv = UIVertex.simpleVert;
			uiv.color = color;

			float tx0 = outer.x;
			float ty0 = outer.y;
			float tx1 = outer.z;
			float ty1 = outer.w;

			// Horizontal and vertical filled sprites are simple -- just end the Image prematurely
			if (m_FillMethod == Image.FillMethod.Horizontal || m_FillMethod == Image.FillMethod.Vertical)
			{
				if (m_FillMethod == Image.FillMethod.Horizontal)
				{
					float fill = (tx1 - tx0) * fillAmount;

					if (m_FillOrigin == 1)
					{
						v.x = v.z - (v.z - v.x) * fillAmount;
						tx0 = tx1 - fill;
					}
					else
					{
						v.z = v.x + (v.z - v.x) * fillAmount;
						tx1 = tx0 + fill;
					}
				}
				else if (fillMethod == Image.FillMethod.Vertical)
				{
					float fill = (ty1 - ty0) * fillAmount;

					if (m_FillOrigin == 1)
					{
						v.y = v.w - (v.w - v.y) * fillAmount;
						ty0 = ty1 - fill;
					}
					else
					{
						v.w = v.y + (v.w - v.y) * fillAmount;
						ty1 = ty0 + fill;
					}
				}
			}

			s_Xy[0] = new Vector2(v.x, v.y);
			s_Xy[1] = new Vector2(v.x, v.w);
			s_Xy[2] = new Vector2(v.z, v.w);
			s_Xy[3] = new Vector2(v.z, v.y);

			s_Uv[0] = new Vector2(tx0, ty0);
			s_Uv[1] = new Vector2(tx0, ty1);
			s_Uv[2] = new Vector2(tx1, ty1);
			s_Uv[3] = new Vector2(tx1, ty0);

			{
				if (fillAmount < 1f && m_FillMethod != Image.FillMethod.Horizontal && m_FillMethod != Image.FillMethod.Vertical)
				{
					if (fillMethod == Image.FillMethod.Radial90)
					{
						if (RadialCut(s_Xy, s_Uv, fillAmount, m_FillClockwise, m_FillOrigin))
							AddQuad(toFill, s_Xy, color, s_Uv);
					}
					else if (fillMethod == Image.FillMethod.Radial180)
					{
						for (int side = 0; side < 2; ++side)
						{
							float fx0, fx1, fy0, fy1;
							int even = m_FillOrigin > 1 ? 1 : 0;

							if (m_FillOrigin == 0 || m_FillOrigin == 2)
							{
								fy0 = 0f;
								fy1 = 1f;
								if (side == even)
								{
									fx0 = 0f;
									fx1 = 0.5f;
								}
								else
								{
									fx0 = 0.5f;
									fx1 = 1f;
								}
							}
							else
							{
								fx0 = 0f;
								fx1 = 1f;
								if (side == even)
								{
									fy0 = 0.5f;
									fy1 = 1f;
								}
								else
								{
									fy0 = 0f;
									fy1 = 0.5f;
								}
							}

							s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
							s_Xy[1].x = s_Xy[0].x;
							s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
							s_Xy[3].x = s_Xy[2].x;

							s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
							s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
							s_Xy[2].y = s_Xy[1].y;
							s_Xy[3].y = s_Xy[0].y;

							s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
							s_Uv[1].x = s_Uv[0].x;
							s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
							s_Uv[3].x = s_Uv[2].x;

							s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
							s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
							s_Uv[2].y = s_Uv[1].y;
							s_Uv[3].y = s_Uv[0].y;

							float val = m_FillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

							if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((side + m_FillOrigin + 3) % 4)))
							{
								AddQuad(toFill, s_Xy, color, s_Uv);
							}
						}
					}
					else if (fillMethod == Image.FillMethod.Radial360)
					{
						for (int corner = 0; corner < 4; ++corner)
						{
							float fx0, fx1, fy0, fy1;

							if (corner < 2)
							{
								fx0 = 0f;
								fx1 = 0.5f;
							}
							else
							{
								fx0 = 0.5f;
								fx1 = 1f;
							}

							if (corner == 0 || corner == 3)
							{
								fy0 = 0f;
								fy1 = 0.5f;
							}
							else
							{
								fy0 = 0.5f;
								fy1 = 1f;
							}

							s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
							s_Xy[1].x = s_Xy[0].x;
							s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
							s_Xy[3].x = s_Xy[2].x;

							s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
							s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
							s_Xy[2].y = s_Xy[1].y;
							s_Xy[3].y = s_Xy[0].y;

							s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
							s_Uv[1].x = s_Uv[0].x;
							s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
							s_Uv[3].x = s_Uv[2].x;

							s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
							s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
							s_Uv[2].y = s_Uv[1].y;
							s_Uv[3].y = s_Uv[0].y;

							float val = m_FillClockwise ?
								fillAmount * 4f - ((corner + m_FillOrigin) % 4) :
								fillAmount * 4f - (3 - ((corner + m_FillOrigin) % 4));

							if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((corner + 2) % 4)))
								AddQuad(toFill, s_Xy, color, s_Uv);
						}
					}
				}
				else
				{
					AddQuad(toFill, s_Xy, color, s_Uv);
				}
			}
		}

		static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
		{
			// Nothing to fill
			if (fill < 0.001f) return false;

			// Even corners invert the fill direction
			if ((corner & 1) == 1) invert = !invert;

			// Nothing to adjust
			if (!invert && fill > 0.999f) return true;

			// Convert 0-1 value into 0 to 90 degrees angle in radians
			float angle = Mathf.Clamp01(fill);
			if (invert) angle = 1f - angle;
			angle *= 90f * Mathf.Deg2Rad;

			// Calculate the effective X and Y factors
			float cos = Mathf.Cos(angle);
			float sin = Mathf.Sin(angle);

			RadialCut(xy, cos, sin, invert, corner);
			RadialCut(uv, cos, sin, invert, corner);
			return true;
		}

		/// <summary>
		/// Adjust the specified quad, making it be radially filled instead.
		/// </summary>

		static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
		{
			int i0 = corner;
			int i1 = ((corner + 1) % 4);
			int i2 = ((corner + 2) % 4);
			int i3 = ((corner + 3) % 4);

			if ((corner & 1) == 1)
			{
				if (sin > cos)
				{
					cos /= sin;
					sin = 1f;

					if (invert)
					{
						xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
						xy[i2].x = xy[i1].x;
					}
				}
				else if (cos > sin)
				{
					sin /= cos;
					cos = 1f;

					if (!invert)
					{
						xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
						xy[i3].y = xy[i2].y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
				else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
			}
			else
			{
				if (cos > sin)
				{
					sin /= cos;
					cos = 1f;

					if (!invert)
					{
						xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
						xy[i2].y = xy[i1].y;
					}
				}
				else if (sin > cos)
				{
					cos /= sin;
					sin = 1f;

					if (invert)
					{
						xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
						xy[i3].x = xy[i2].x;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
				else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
			}
		}
	}
}