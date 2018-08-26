using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodeEditor;
using NodeEditor.Slots;
using NodeEditor.Util;
using UINodeEditor.Elements;
using UnityEngine;
using UnityEngine.Rendering;

namespace UINodeEditor
{
	[Title("UI Master Node")]
	public class UIMasterNode : AbstractNode, IOnAssetEnabled
	{
		private EmptySlot<Action<UIEventData>> m_Input;
		private DefaultValueSlot<Matrix4x4> m_Matrix;
		private ValueSlot<Rect> m_Rect;
		private List<KeyValuePair<ISlot, INode>> m_TmpSlots;
		private Mesh mesh;
		private List<Action<UIEventData>> m_ValuesTmp = new List<Action<UIEventData>>();
		private UIThreadWaitHandle waitHandle = new UIThreadWaitHandle();

		public UIMasterNode()
		{
			name = "UI Master Node";
			m_Input = CreateInputSlot<EmptySlot<Action<UIEventData>>>("UI Event");
			m_Input.SetAllowMultipleConnections(true);
			m_Rect = CreateInputSlot<ValueSlot<Rect>>("Rect").SetShowControl();
			m_Matrix = CreateInputSlot<DefaultValueSlot<Matrix4x4>>("Matrix").SetDefaultValue(Matrix4x4.identity);
		}

		public void OnEnable()
		{
			mesh = new Mesh();
		}

		public override void Dispose()
		{
			base.Dispose();
			UnityEngine.Object.DestroyImmediate(mesh);
		}

		public void Execute(UIRenderBuffer renderBuffer,CommandBuffer commandBuffer)
		{
			GetSlotValues(m_Input, m_ValuesTmp);

			Rect rect = m_Rect[this];
			Rect localRect = new Rect(Vector2.zero, rect.size);

			foreach (var value in m_ValuesTmp)
			{
				value.Invoke(new UIEventData(){EventType = UIEventType.Layout,Rect = localRect });
			}

			if (((UIGraphObject) owner.owner).threaded)
			{
				waitHandle.Reset(m_ValuesTmp.Count);

				foreach (var a in m_ValuesTmp)
				{
					var a1 = a;
					ThreadPool.QueueUserWorkItem(c => a1.Invoke((UIEventData)c), new UIEventData() { EventType = UIEventType.PreRepaint, Rect = localRect, RenderBuffer = renderBuffer, WaitHandle = waitHandle });
				}

				waitHandle.WaitAll();
			}
			else
			{
				foreach (var action in m_ValuesTmp)
				{
					action.Invoke(new UIEventData() { EventType = UIEventType.PreRepaint, Rect = localRect, RenderBuffer = renderBuffer});
				}
			}
			

			foreach (var value in m_ValuesTmp)
			{
				try
				{
					value.Invoke(new UIEventData() { EventType = UIEventType.Repaint, Rect = localRect, RenderBuffer = renderBuffer });
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			var matrix = m_Matrix[this] * Matrix4x4.Translate(rect.position);
			renderBuffer.Populate(commandBuffer, matrix);

			m_ValuesTmp.Clear();
		}
	}
}