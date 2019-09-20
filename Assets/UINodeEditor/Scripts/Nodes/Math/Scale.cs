using NodeEditor;
using UnityEngine;

namespace UINodeEditor.Math
{
    [Title("Math", "4D", "Scale: 4D")]
    public class ScaleVector4 : AbstractNode
    {
        public ScaleVector4()
        {
            name = "Scale Vector4";
            var inVec = CreateInputSlot<EmptySlot<Vector4>>("In");
            var scale = CreateInputSlot<ValueSlot<float>>("Scale").SetValue(1).SetShowControl();
            CreateOutputSlot<GetterSlot<Vector4>>("Out").SetGetter(() => inVec[this] * scale[this]);
        }
    }

    [Title("Math","3D","Scale: 3D")]
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

    [Title("Math", "2D", "Scale: 2D")]
    public class ScaleVector2 : AbstractNode
    {
        public ScaleVector2()
        {
            name = "Scale Vector2";
            var inVec = CreateInputSlot<EmptySlot<Vector2>>("In");
            var scale = CreateInputSlot<ValueSlot<float>>("Scale").SetValue(1).SetShowControl();
            CreateOutputSlot<GetterSlot<Vector2>>("Out").SetGetter(() => inVec[this] * scale[this]);
        }
    }
}