using NodeEditor;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// The camera node provides properties from the main camera.
    /// </summary>
	[Title("Input","Camera")]
	public class CameraNode : AbstractNode
	{
		public CameraNode()
		{
			CreateOutputSlot<GetterSlot<Vector3>>("Position").SetGetter(() => Camera.main.transform.position);
			CreateOutputSlot<GetterSlot<Quaternion>>("Rotation").SetGetter(() => Camera.main.transform.rotation);
			CreateOutputSlot<GetterSlot<Matrix4x4>>("CameraToWorld").SetGetter(() => Camera.main.cameraToWorldMatrix);
			CreateOutputSlot<GetterSlot<Matrix4x4>>("WorldToCamera").SetGetter(() => Camera.main.worldToCameraMatrix);
		}
	}
}