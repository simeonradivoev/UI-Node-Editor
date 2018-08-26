using NodeEditor;
using UnityEngine;

namespace UINodeEditor.Math
{
	[Title("Math","Scale: Vector3")]
	public class ScaleVector3 : AbstractNode
	{
		public ScaleVector3()
		{
			name = "Scale Vector3";
			var inVec = CreateInputSlot<EmptySlot<Vector3>>("In");
			var scale = CreateInputSlot<ValueSlot<float>>("Scale").SetValue(1).SetShowControl();
			CreateOutputSlot<GetterSlot<Vector3>>("Out").SetGetter(()=> inVec[this] * scale[this]);
		}
	}
}