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
		private Guid m_HealthId = StringToGUID.Get("health");
		private CommandBuffer m_CommandBuffer;
		private UIRenderBuffer m_RenderBuffer;

		private void Awake()
		{
			m_CommandBuffer = new CommandBuffer();
			m_RenderBuffer = new UIRenderBuffer();
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
			foreach (var node in nodes)
			{
				var masterNode = node as UIMasterNode;
				if (masterNode != null)
				{
					m_CommandBuffer.Clear();
					try
					{
						masterNode.Execute(m_RenderBuffer, m_CommandBuffer);
					}
					finally
					{
						m_RenderBuffer.Clear();
					}
					break;
				}
			}
			Profiler.EndSample();
			foreach (var node in nodes)
				(node as AbstractUINode)?.ClearDirty();
		}
	}
}