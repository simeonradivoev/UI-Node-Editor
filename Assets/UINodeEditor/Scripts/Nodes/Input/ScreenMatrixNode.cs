using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// A matrix node that provides a screen matrix that can be used in a <see cref="UIMasterNode"/> to draw elements in screen space.
    /// With a rect as big as the camera's pixels.
    /// It uses the main camera from <see cref="Camera.main"/>.
    /// </summary>
	[Title("Input","Screen Matrix")]
	public class ScreenMatrixNode : AbstractNode, ITickableNode
	{
		[SerializeField] private float m_Distance = 1;
		private ValueSlot<Matrix4x4> m_Output;
		private GetterSlot<Camera> m_Camera;
		private ValueSlot<Rect> m_ScreenRect;
		private Camera m_CachedCamera;

        /// <summary>
        /// The distance of the UI elements from the camera.
        /// Elements are scaled based on this distance so they appear the same size in screen space.
        /// </summary>
		[DefaultControl(label = "Distance")]
		public float distance
		{
			get { return m_Distance; }
			set
			{
				m_Distance = value;
				Dirty(ModificationScope.Node);
			}
		}

		public ScreenMatrixNode()
		{
			m_Camera = CreateOutputSlot<GetterSlot<Camera>>("Camera").SetGetter(()=> m_CachedCamera);
			m_ScreenRect = CreateOutputSlot<ValueSlot<Rect>>("ScreenRect").SetValue(new Rect(0, 0, 256, 256));
			m_Output = CreateOutputSlot<ValueSlot<Matrix4x4>>("Mat").SetValue(Matrix4x4.identity);
		}

		private Matrix4x4 CalculateMatrix()
		{
			var frustumHeight = 2.0f * m_Distance * Mathf.Tan(m_CachedCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			var frustumWidth = frustumHeight * m_CachedCamera.aspect;
			return m_CachedCamera.cameraToWorldMatrix * Matrix4x4.Translate(new Vector3(-frustumWidth * 0.5f, -frustumHeight * 0.5f, -m_Distance)) * Matrix4x4.Scale(new Vector3(frustumWidth / m_CachedCamera.pixelWidth, frustumHeight / m_CachedCamera.pixelHeight));
		}

		public void Tick()
		{
			if(m_CachedCamera == null) m_CachedCamera = Camera.main;
			m_Output.SetValue(CalculateMatrix());
			m_ScreenRect.SetValue(m_CachedCamera.pixelRect);
		}
	}
}