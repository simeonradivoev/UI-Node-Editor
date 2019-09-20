using System;
using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UINodeEditor.Elements
{
    /// <summary>
    /// Simple sprite image node that is similar ot <see cref="Image"/> with simple image option enabled.
    /// It draws a single sprite with no other effects.
    /// </summary>
	[Title("Elements","Image (Simple)")]
	public class SimpleImageNode : GraphicNode
	{
		[SerializeField] private bool m_PreserveAspect;

		private struct SpriteData
		{
			public Vector4 padding;
			public Rect rect;
			public Vector4 outerUv;
            public Vector2 pivot;
            public Bounds bounds;
            public Vector2[] vertices;
            public Vector2[] uvs;
            public UInt16[] triangles;
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
						outerUv = DataUtility.GetOuterUV(sprite),
                        pivot = sprite.pivot,
                        vertices = sprite.vertices,
                        uvs = sprite.uv,
                        triangles = sprite.triangles,
                        bounds = sprite.bounds
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
                var localRect = new Rect(-pivot.x * rect.width, -pivot.y * rect.height, rect.width, rect.height);
                if (m_SpriteData.vertices != null)
                {
                    GenerateSprite(vertexHelper, m_SpriteData, localRect, pivot, color);
                }
                else
                {
                    GenerateSimpleSprite(vertexHelper, m_SpriteData, localRect, pivot, color);
                }
                
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

				eData.RenderBuffer.Render(mesh, Matrix4x4.Translate(new Vector3(rect.x + pivot.x * rect.width, rect.y + pivot.y * rect.width, m_ZOffset[this])) * matrix, mat, propertyBlock);
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

        private void GenerateSprite(UIVertexHelper vh, SpriteData sprite, Rect rect, Vector2 pivot, Color color)
        {
            var spriteSize = new Vector2(sprite.rect.width, sprite.rect.height);

            // Covert sprite pivot into normalized space.
            var spritePivot = sprite.pivot / spriteSize;
            var rectPivot = pivot;
            Rect r = rect;

            if (preserveAspect & spriteSize.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref r, spriteSize,pivot);
            }

            var drawingSize = new Vector2(r.width, r.height);
            var spriteBoundSize = sprite.bounds.size;

            // Calculate the drawing offset based on the difference between the two pivots.
            var drawOffset = (rectPivot - spritePivot) * drawingSize;

            var color32 = color;
            vh.Clear();

            Vector2[] vertices = sprite.vertices;
            Vector2[] uvs = sprite.uvs;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vh.AddVert(new Vector3((vertices[i].x / spriteBoundSize.x) * drawingSize.x - drawOffset.x, (vertices[i].y / spriteBoundSize.y) * drawingSize.y - drawOffset.y), color32, new Vector2(uvs[i].x, uvs[i].y));
            }

            UInt16[] triangles = sprite.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(triangles[i + 0], triangles[i + 1], triangles[i + 2]);
            }
        }

        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize,Vector2 pivot)
        {
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = rect.height;
                rect.height = rect.width * (1.0f / spriteRatio);
                rect.y += (oldHeight - rect.height) * pivot.y;
            }
            else
            {
                var oldWidth = rect.width;
                rect.width = rect.height * spriteRatio;
                rect.x += (oldWidth - rect.width) * pivot.x;
            }
        }
    }
}