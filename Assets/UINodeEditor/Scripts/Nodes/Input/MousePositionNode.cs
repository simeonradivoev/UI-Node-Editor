using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// Node that provides the 2D and 3D mouse position gotten straight from <see cref="Input"/>.
    /// </summary>
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