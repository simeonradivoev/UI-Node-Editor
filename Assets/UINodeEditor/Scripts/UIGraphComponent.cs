using System;
using NodeEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UINodeEditor
{
	[RequireComponent(typeof(Camera))]
	[ImageEffectAllowedInSceneView]
	public class UIGraphComponent : MonoBehaviour
	{
		[SerializeField,Range(0,600)] private float m_Health;
		[SerializeField] private UIGraphObject m_Graph;
        [SerializeField] private bool m_Threaded;
		private Guid m_HealthId = StringToGUID.Get("health");
		private CommandBuffer m_CommandBuffer;
        private UIGraphRenderer m_Renderer;

        private void Awake()
		{
            m_CommandBuffer = new CommandBuffer {name = name};
            m_Renderer = new UIGraphRenderer();
        }

		private void OnEnable()
		{
			GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterForwardAlpha,m_CommandBuffer);
		}

		private void OnDisable()
		{
			GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, m_CommandBuffer);
		}

		private void OnDestroy()
		{
			m_CommandBuffer.Dispose();
            m_Renderer.Dispose();
		}

		private void Update()
		{
			UpdateInternal();
		}

		private void UpdateInternal()
		{
			GlobalProperties<float>.SetValue(m_HealthId, m_Health);

			Profiler.BeginSample("UIGraph");
			Profiler.BeginSample("UIGraph Tick");
			var nodes = m_Graph.graph.GetNodes();
			foreach (var node in nodes)
				(node as ITickableNode)?.Tick();
			Profiler.EndSample();
            m_Renderer.PopulateCommandBuffer(m_Threaded, nodes,m_CommandBuffer);
            Profiler.EndSample();
			foreach (var node in nodes)
				(node as AbstractUINode)?.ClearDirty();
		}
	}
}