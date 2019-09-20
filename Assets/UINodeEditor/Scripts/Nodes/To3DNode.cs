using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// A node that converts a Vector2 into a Vector3
    /// </summary>
	[Title("Conversion","To 3D")]
	public class To3DNode : AbstractNode
	{
		private EmptySlot<Vector2> m_Input;

		public To3DNode()
		{
			m_Input = CreateInputSlot<EmptySlot<Vector2>>("In");
			CreateOutputSlot<GetterSlot<Vector3>>("Out").SetGetter(() => m_Input[this]);
		}
	}
}