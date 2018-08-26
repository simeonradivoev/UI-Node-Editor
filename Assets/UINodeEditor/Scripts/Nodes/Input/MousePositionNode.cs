using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
	[Title("Input","Mouse Position")]
	public class MousePositionNode : AbstractNode
	{
		public MousePositionNode()
		{
			name = "Mouse Position";
			CreateOutputSlot<GetterSlot<Vector2>>("2D").SetGetter(() => Input.mousePosition);
			CreateOutputSlot<GetterSlot<Vector3>>("3D").SetGetter(() => Input.mousePosition);
		}
	}
}