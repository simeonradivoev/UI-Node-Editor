using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
	[Title("Builder","Rect Matrix")]
	public class RectMatrixBuilderNode : AbstractNode
	{
		private ValueSlot<Vector2> m_Position;
		private ValueSlot<Vector2> m_Size;

		public RectMatrixBuilderNode()
		{
			name = "Rect Matrix";
			m_Position = CreateInputSlot<ValueSlot<Vector2>>("Position").SetShowControl();
			m_Size = CreateInputSlot<ValueSlot<Vector2>>("Size").SetShowControl();
			CreateOutputSlot<GetterSlot<Matrix4x4>>("Out").SetGetter(BuildMatrix);
		}

		private Matrix4x4 BuildMatrix()
		{
			var pos = m_Position[this];
			var size = m_Size[this];

			return Matrix4x4.TRS(pos, Quaternion.identity, size);
		}
	}
}