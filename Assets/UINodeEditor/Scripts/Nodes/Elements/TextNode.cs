using System;
using System.Collections.Generic;
using NodeEditor;
using NodeEditor.Controls;
using NodeEditor.Slots;
using UnityEngine;
using UnityEngine.UI;

namespace UINodeEditor.Elements
{
	[Title("Elements","Text")]
	public class TextNode : UIElementNode, IOnAssetEnabled
	{
		private const int DefaultFontSize = 14;

		[SerializeField] private HorizontalWrapMode m_HorizontalWrap;
		[SerializeField] private VerticalWrapMode m_VerticalWrap;
		[SerializeField] private float m_ScaleFactor = 1;
		[SerializeField] private TextAnchor m_Anchor;
		[SerializeField] private FontStyle m_Style;

		[EnumControl(label = "Horizontal Wrap")]
		public HorizontalWrapMode MHorizontalWrap
		{
			get { return m_HorizontalWrap; }
			set
			{
				m_HorizontalWrap = value; 
				OnMeshDirty();
			}
		}

		[EnumControl(label = "Vertical Wrap")]
		public VerticalWrapMode MVerticalWrap
		{
			get { return m_VerticalWrap; }
			set
			{
				m_VerticalWrap = value;
				OnMeshDirty();
			}
		}

		[DefaultControl(label = "Scale Factor")]
		public float scaleFactor
		{
			get { return m_ScaleFactor; }
			set
			{
				m_ScaleFactor = value;
				OnMeshDirty();
			}
		}

		[EnumControl(label = "Text Anchor")]
		public TextAnchor anchor
		{
			get { return m_Anchor; }
			set
			{
				m_Anchor = value;
				OnMeshDirty();
			}
		}

		[EnumControl(label = "Style")]
		public FontStyle style
		{
			get { return m_Style; }
			set
			{
				m_Style = value;
				OnMeshDirty();
			}
		}

		private struct FontData
		{
			public bool hasFont;
			public bool dynamic;
			public int size;
			public Material material;
		}

		private ValueSlot<string> m_Text;
		private DefaultValueSlot<Font> m_Font;
		private ValueSlot<int> m_FontSize;
		private ValueSlot<Vector2> m_Pivot;
		private ValueSlot<Color> m_Color;
		private DefaultValueSlot<Matrix4x4> m_Matrix;
		private EmptySlot<Material> m_Material;
		private MaterialPropertyBlock m_PropertyBlock;
		private Mesh m_Mesh;
		private UIVertexHelper m_VertexHelper;
		private TextGenerationSettings settings;
		private TextGenerator m_TextGenerator;
		private FontData m_FontData;
		private IList<UIVertex> m_TextVerts;

		public TextNode()
		{
			m_Text = CreateInputSlot<ValueSlot<string>>("Text").SetShowControl();
			m_FontSize = CreateInputSlot<ValueSlot<int>>("Size").SetValue(DefaultFontSize).SetShowControl();
			m_Pivot = CreateInputSlot<ValueSlot<Vector2>>("Pivot").SetShowControl();
			m_Color = CreateInputSlot<ValueSlot<Color>>("Color").SetValue(Color.white).SetShowControl();
			m_Font = CreateInputSlot<DefaultValueSlot<Font>>("Font");
			m_Matrix = CreateInputSlot<DefaultValueSlot<Matrix4x4>>("Matrix").SetDefaultValue(Matrix4x4.identity);
			m_Material = CreateInputSlot<EmptySlot<Material>>("Material");
			CreateOutputSlot<ValueSlot<Action<UIEventData>>>("UI Event").SetValue(Execute);
			CreateOutputSlot<GetterSlot<Rect>>("Extents").SetGetter(()=> m_TextGenerator.rectExtents);
		}

		public void OnEnable()
		{
			m_Mesh = new Mesh();
			m_VertexHelper = new UIVertexHelper();
			m_PropertyBlock = new MaterialPropertyBlock();
			m_TextGenerator = new TextGenerator();
			m_Font.SetDefaultValue(Resources.GetBuiltinResource<Font>("Arial.ttf"));
		}

		public override void Dispose()
		{
			base.Dispose();
			UnityEngine.Object.DestroyImmediate(m_Mesh);
			m_TextGenerator = null;
		}

		protected override void Execute(UIEventData eventData,Rect rect)
		{
			if (eventData.EventType == UIEventType.Layout)
			{
				var font = m_Font[this];
				if (font != null)
					m_FontData = new FontData()
					{
						hasFont = true,
						dynamic = font.dynamic,
						size = font.fontSize,
						material = font.material
					};
				else
					m_FontData = new FontData();

				var text = m_Text[this];
				var fontSize = m_FontSize[this];
				var pivot = m_Pivot[this];
				float pixelPerUnit = GetPixelsPerUnit(m_FontData, fontSize);
				m_TextGenerator.Populate(text, GetSettings(font, fontSize, m_Color[this], rect, pivot, pixelPerUnit));
				m_TextVerts = m_TextGenerator.verts;
			}
			else if (eventData.EventType == UIEventType.PreRepaint)
			{
				var fontSize = m_FontSize[this];
				float pixelPerUnit = GetPixelsPerUnit(m_FontData, fontSize);
				lock (m_VertexHelper)
				{
					RebuildMesh(m_VertexHelper, pixelPerUnit);
				}
			}
			else if (eventData.EventType == UIEventType.Repaint)
			{
				var matrix = m_Matrix[this];
				var mat = m_Material[this];
				
				m_VertexHelper.FillMesh(m_Mesh);
				m_VertexHelper.Clear();
				eventData.RenderBuffer.Render(m_Mesh, matrix * Matrix4x4.Translate(rect.position), mat ?? m_FontData.material, m_PropertyBlock);
			}

			base.Execute(eventData,rect);
		}

		private TextGenerationSettings GetSettings(Font font,int fontSize,Color color,Rect rect,Vector2 pivot,float scaleFactor)
		{
			return new TextGenerationSettings()
			{
				font = font,
				fontSize = fontSize,
				horizontalOverflow = m_HorizontalWrap,
				verticalOverflow = m_VerticalWrap,
				color = color,
				generationExtents = rect.size,
				scaleFactor = scaleFactor,
				textAnchor = m_Anchor,
				lineSpacing = 1,
				updateBounds = true,
				pivot = pivot,
				fontStyle = m_Style
			};
		}

		readonly UIVertex[] m_TempVerts = new UIVertex[4];

		private void RebuildMesh(UIVertexHelper toFill,float pixelsPerUnit)
		{
			float unitsPerPixel = 1 / pixelsPerUnit;
			IList<UIVertex> verts = m_TextVerts;
			//Last 4 verts are always a new line... (\n)
			int vertCount = verts.Count - 4;

			for (int i = 0; i < vertCount; ++i)
			{
				int tempVertsIndex = i & 3;
				var vert = verts[i];
				m_TempVerts[tempVertsIndex] = vert;
				m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
				if (tempVertsIndex == 3)
					toFill.AddUIVertexQuad(m_TempVerts);
			}
		}

		private float GetPixelsPerUnit(FontData font,int fontSize)
		{
			// For dynamic fonts, ensure we use one pixel per pixel on the screen.
			if (!font.hasFont || font.dynamic)
				return m_ScaleFactor;
			// For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
			if (fontSize <= 0 || font.size <= 0)
				return 1;
			return font.size / (float)fontSize;
		}
	}
}