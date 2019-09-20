using System;
using NodeEditor;
using NodeEditor.Slots;
using UINodeEditor;
using UINodeEditor.Elements;
using UnityEngine;

/// <summary>
/// Base node for all graphic elements.
/// </summary>
public abstract class GraphicNode : UIElementNode, IOnAssetEnabled
{
	private static Material m_DefaultMaterial;

	protected ValueSlot<Vector2> m_Pivot;
	protected ValueSlot<Color> m_Color;
	protected DefaultValueSlot<Matrix4x4> m_Matrix;
	protected DefaultValueSlot<Material> m_Material;
	protected ValueSlot<float> m_ZOffset;

	protected int m_MainTexProp;

	protected GraphicNode()
	{
		m_Pivot = CreateInputSlot<ValueSlot<Vector2>>("Pivot %").SetValue(Vector2.one * 0.5f).SetShowControl();
		m_Color = CreateInputSlot<ValueSlot<Color>>("Color").SetValue(Color.white).SetShowControl();
		m_Matrix = CreateInputSlot<DefaultValueSlot<Matrix4x4>>("Matrix").SetDefaultValue(Matrix4x4.identity);
		m_Material = CreateInputSlot<DefaultValueSlot<Material>>("Material");
		m_ZOffset = CreateInputSlot<ValueSlot<float>>("zOffset","Z Offset").SetShowControl();

		CreateOutputSlot<ValueSlot<Action<UIEventData>>>("UI Event").SetValue(Execute);
	}

	public void OnEnable()
	{
		m_MainTexProp = Shader.PropertyToID("_MainTex");
		if (m_DefaultMaterial == null) m_DefaultMaterial = new Material(Shader.Find("Sprites/Default")){hideFlags = HideFlags.DontSave};
		if (m_Material.value == null) m_Material.SetDefaultValue(m_DefaultMaterial);
	}

	/// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
	protected Vector4 GetDrawingDimensions(Vector4 padding,Vector2 size,Vector2 pivot,Rect rect,bool shouldPreserveAspect)
	{
		Rect r = rect;
		// Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

		int spriteW = Mathf.RoundToInt(size.x);
		int spriteH = Mathf.RoundToInt(size.y);

		var v = new Vector4(
			padding.x / spriteW,
			padding.y / spriteH,
			(spriteW - padding.z) / spriteW,
			(spriteH - padding.w) / spriteH);

		if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
		{
            PreserveSpriteAspectRatio(ref r, size, pivot);
        }

        v = new Vector4(
			r.x + r.width * v.x,
			r.y + r.height * v.y,
			r.x + r.width * v.z,
			r.y + r.height * v.w
		);

		return v;
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


    protected static void AddQuad(UIVertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
	{
		int startIndex = vertexHelper.currentVertCount;

		for (int i = 0; i < 4; ++i)
			vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

		vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
		vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
	}

	protected static void AddQuad(UIVertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
	{
		int startIndex = vertexHelper.currentVertCount;

		vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
		vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

		vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
		vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
	}
}
