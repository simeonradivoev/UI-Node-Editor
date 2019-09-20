using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;
using UnityEngine.Sprites;

namespace UINodeEditor.Elements
{
    /// <summary>
    /// Tiled image is similar to <see cref="UnityEngine.UI.Image"/> with the tiled image option enabled.
    /// It draws a sprite that can be tiled.
    /// </summary>
	[Title("Elements","Image (Tiled)")]
	public class TiledImageNode : GraphicNode
	{
		[SerializeField] private bool m_FillCenter = true;

		private struct SpriteData
		{
			public bool hasSprite;
			public Rect rect;
			public Vector4 outerUv;
			public Vector4 innerUv;
			public bool hasBorder;
			public Vector4 border;
			public bool packed;
			public TextureWrapMode wrapMode;
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

		public TiledImageNode()
		{
			name = "Tiled Image";
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
				var color = m_Color[this];
                var vertexHelper = eData.MeshRepository.GetVertexHelper(guid);
                GenerateTiledSprite(vertexHelper, m_SpriteData, rect, color);
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

		private void GenerateTiledSprite(UIVertexHelper toFill, SpriteData sprite, Rect rect, Color color)
		{
			bool hasBorder = sprite.hasBorder;
			Vector4 outer = sprite.outerUv;
			Vector4 inner = sprite.innerUv;
			Vector4 border = sprite.border;
			Vector2 spriteSize = sprite.rect.size;

			float tileWidth = (spriteSize.x - border.x - border.z);
			float tileHeight = (spriteSize.y - border.y - border.w);

			var uvMin = new Vector2(inner.x, inner.y);
			var uvMax = new Vector2(inner.z, inner.w);

			// Min to max max range for tiled region in coordinates relative to lower left corner.
			float xMin = border.x;
			float xMax = rect.width - border.z;
			float yMin = border.y;
			float yMax = rect.height - border.w;

			toFill.Clear();
			var clipped = uvMax;

			// if either width is zero we cant tile so just assume it was the full width.
			if (tileWidth <= 0)
				tileWidth = xMax - xMin;

			if (tileHeight <= 0)
				tileHeight = yMax - yMin;

			if (sprite.hasSprite && (hasBorder || sprite.packed || sprite.wrapMode != TextureWrapMode.Repeat))
			{
				// Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
				// We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

				// Evaluate how many vertices we will generate. Limit this number to something sane,
				// especially since meshes can not have more than 65000 vertices.

				long nTilesW = 0;
				long nTilesH = 0;
				if (m_FillCenter)
				{
					nTilesW = (long)System.Math.Ceiling((xMax - xMin) / tileWidth);
					nTilesH = (long)System.Math.Ceiling((yMax - yMin) / tileHeight);

					double nVertices = 0;
					if (hasBorder)
					{
						nVertices = (nTilesW + 2.0) * (nTilesH + 2.0) * 4.0; // 4 vertices per tile
					}
					else
					{
						nVertices = nTilesW * nTilesH * 4.0; // 4 vertices per tile
					}

					if (nVertices > 65000.0)
					{
						Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.");

						double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
						double imageRatio;
						if (hasBorder)
						{
							imageRatio = (nTilesW + 2.0) / (nTilesH + 2.0);
						}
						else
						{
							imageRatio = (double)nTilesW / nTilesH;
						}

						double targetTilesW = System.Math.Sqrt(maxTiles / imageRatio);
						double targetTilesH = targetTilesW * imageRatio;
						if (hasBorder)
						{
							targetTilesW -= 2;
							targetTilesH -= 2;
						}

						nTilesW = (long)System.Math.Floor(targetTilesW);
						nTilesH = (long)System.Math.Floor(targetTilesH);
						tileWidth = (xMax - xMin) / nTilesW;
						tileHeight = (yMax - yMin) / nTilesH;
					}
				}
				else
				{
					if (hasBorder)
					{
						// Texture on the border is repeated only in one direction.
						nTilesW = (long)System.Math.Ceiling((xMax - xMin) / tileWidth);
						nTilesH = (long)System.Math.Ceiling((yMax - yMin) / tileHeight);
						double nVertices = (nTilesH + nTilesW + 2.0 /*corners*/) * 2.0 /*sides*/ * 4.0 /*vertices per tile*/;
						if (nVertices > 65000.0)
						{
							Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.");

							double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
							double imageRatio = (double)nTilesW / nTilesH;
							double targetTilesW = (maxTiles - 4 /*corners*/) / (2 * (1.0 + imageRatio));
							double targetTilesH = targetTilesW * imageRatio;

							nTilesW = (long)System.Math.Floor(targetTilesW);
							nTilesH = (long)System.Math.Floor(targetTilesH);
							tileWidth = (xMax - xMin) / nTilesW;
							tileHeight = (yMax - yMin) / nTilesH;
						}
					}
					else
					{
						nTilesH = nTilesW = 0;
					}
				}

				if (m_FillCenter)
				{
					// TODO: we could share vertices between quads. If vertex sharing is implemented. update the computation for the number of vertices accordingly.
					for (long j = 0; j < nTilesH; j++)
					{
						float y1 = yMin + j * tileHeight;
						float y2 = yMin + (j + 1) * tileHeight;
						if (y2 > yMax)
						{
							clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
							y2 = yMax;
						}
						clipped.x = uvMax.x;
						for (long i = 0; i < nTilesW; i++)
						{
							float x1 = xMin + i * tileWidth;
							float x2 = xMin + (i + 1) * tileWidth;
							if (x2 > xMax)
							{
								clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
								x2 = xMax;
							}
							AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin, clipped);
						}
					}
				}
				if (hasBorder)
				{
					clipped = uvMax;
					for (long j = 0; j < nTilesH; j++)
					{
						float y1 = yMin + j * tileHeight;
						float y2 = yMin + (j + 1) * tileHeight;
						if (y2 > yMax)
						{
							clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
							y2 = yMax;
						}
						AddQuad(toFill,
							new Vector2(0, y1) + rect.position,
							new Vector2(xMin, y2) + rect.position,
							color,
							new Vector2(outer.x, uvMin.y),
							new Vector2(uvMin.x, clipped.y));
						AddQuad(toFill,
							new Vector2(xMax, y1) + rect.position,
							new Vector2(rect.width, y2) + rect.position,
							color,
							new Vector2(uvMax.x, uvMin.y),
							new Vector2(outer.z, clipped.y));
					}

					// Bottom and top tiled border
					clipped = uvMax;
					for (long i = 0; i < nTilesW; i++)
					{
						float x1 = xMin + i * tileWidth;
						float x2 = xMin + (i + 1) * tileWidth;
						if (x2 > xMax)
						{
							clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
							x2 = xMax;
						}
						AddQuad(toFill,
							new Vector2(x1, 0) + rect.position,
							new Vector2(x2, yMin) + rect.position,
							color,
							new Vector2(uvMin.x, outer.y),
							new Vector2(clipped.x, uvMin.y));
						AddQuad(toFill,
							new Vector2(x1, yMax) + rect.position,
							new Vector2(x2, rect.height) + rect.position,
							color,
							new Vector2(uvMin.x, uvMax.y),
							new Vector2(clipped.x, outer.w));
					}

					// Corners
					AddQuad(toFill,
						new Vector2(0, 0) + rect.position,
						new Vector2(xMin, yMin) + rect.position,
						color,
						new Vector2(outer.x, outer.y),
						new Vector2(uvMin.x, uvMin.y));
					AddQuad(toFill,
						new Vector2(xMax, 0) + rect.position,
						new Vector2(rect.width, yMin) + rect.position,
						color,
						new Vector2(uvMax.x, outer.y),
						new Vector2(outer.z, uvMin.y));
					AddQuad(toFill,
						new Vector2(0, yMax) + rect.position,
						new Vector2(xMin, rect.height) + rect.position,
						color,
						new Vector2(outer.x, uvMax.y),
						new Vector2(uvMin.x, outer.w));
					AddQuad(toFill,
						new Vector2(xMax, yMax) + rect.position,
						new Vector2(rect.width, rect.height) + rect.position,
						color,
						new Vector2(uvMax.x, uvMax.y),
						new Vector2(outer.z, outer.w));
				}
			}
			else
			{
				// Texture has no border, is in repeat mode and not packed. Use texture tiling.
				Vector2 uvScale = new Vector2((xMax - xMin) / tileWidth, (yMax - yMin) / tileHeight);

				if (m_FillCenter)
				{
					AddQuad(toFill, new Vector2(xMin, yMin) + rect.position, new Vector2(xMax, yMax) + rect.position, color, Vector2.Scale(uvMin, uvScale), Vector2.Scale(uvMax, uvScale));
				}
			}
		}
	}
}