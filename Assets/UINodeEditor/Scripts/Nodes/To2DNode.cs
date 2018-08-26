using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
	[Title("Conversion", "To 2D")]
	public class To2DNode : AbstractNode
	{
		public To2DNode()
		{
			var input = CreateInputSlot<EmptySlot<Vector3>>("In");
			CreateOutputSlot<GetterSlot<Vector2>>("Out").SetGetter(() => input[this]);
		}
	}
}