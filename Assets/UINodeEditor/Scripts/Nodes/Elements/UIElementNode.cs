using System;
using System.Collections.Generic;
using System.Threading;
using NodeEditor;
using UnityEngine;

namespace UINodeEditor.Elements
{
	public class UIElementNode : AbstractUINode
	{
		private EmptySlot<Action<UIEventData>> m_Event;
		protected ValueSlot<Vector2> m_Position;
		protected ValueSlot<Vector2> m_Size;
		protected ValueSlot<Vector2> m_OffsetMin;
		protected ValueSlot<Vector2> m_OffsetMax;
		protected ValueSlot<Vector2> m_AnchorMin;
		protected ValueSlot<Vector2> m_AnchorMax;
		private List<Action<UIEventData>> m_ValuesTmp = new List<Action<UIEventData>>();

		public UIElementNode()
		{
			m_Event = CreateInputSlot<EmptySlot<Action<UIEventData>>>("Event");
			m_Event.SetAllowMultipleConnections(true);
			m_Position = CreateInputSlot<ValueSlot<Vector2>>("pos","Position").SetShowControl();
			m_Size = CreateInputSlot<ValueSlot<Vector2>>("size","Size").SetValue(new Vector2(100, 100)).SetShowControl();
			m_OffsetMin = CreateInputSlot<ValueSlot<Vector2>>("minOffset","Offset Min").SetShowControl();
			m_OffsetMax = CreateInputSlot<ValueSlot<Vector2>>("maxOffset","Offset Max").SetShowControl();
			m_AnchorMin = CreateInputSlot<ValueSlot<Vector2>>("minAnchor","Anchor Min %").SetValue(new Vector2(0.5f,0.5f)).SetShowControl();
			m_AnchorMax = CreateInputSlot<ValueSlot<Vector2>>("maxAnchor","Anchor Max %").SetValue(new Vector2(0.5f, 0.5f)).SetShowControl();
		}

		protected void Execute(UIEventData eventData)
		{
			try
			{
				var position = m_Position[this];
				var size = m_Size[this];
				var offsetMin = m_OffsetMin[this];
				var offsetMax = m_OffsetMax[this];
				var anchorMin = m_AnchorMin[this];
				var anchorMax = m_AnchorMax[this];

				var parentRect = eventData.Rect;
				Vector2 min = new Vector2();
				Vector2 max = new Vector2();

				min.x = parentRect.x + position.x + anchorMin.x * parentRect.width + offsetMin.x;
				min.y = parentRect.y + position.y + anchorMin.y * parentRect.height + offsetMin.y;
				max.x = parentRect.x + position.x + size.x + parentRect.width * anchorMax.x - offsetMax.x;
				max.y = parentRect.y + position.y + size.y + parentRect.height * anchorMax.y - offsetMax.y;

				Execute(eventData, new Rect(min.x,min.y,max.x - min.x,max.y - min.y));
			}
			finally
			{
				if (eventData.WaitHandle != null) eventData.WaitHandle.MarkDone();
			}
		}

		protected virtual void Execute(UIEventData eventData,Rect rect)
		{
			eventData.Rect = new Rect(rect.position, rect.size);
			try
			{
				GetSlotValues(m_Event, m_ValuesTmp);
				if (eventData.WaitHandle != null)
				{
					foreach (var action in m_ValuesTmp)
					{
						eventData.WaitHandle.AddWait();
						ThreadPool.QueueUserWorkItem(c => action.Invoke((UIEventData)c), eventData);
					}
				}
				else
				{
					foreach (var action in m_ValuesTmp)
					{
						action.Invoke(eventData);
					}
				}
			}
			finally
			{
				m_ValuesTmp.Clear();
			}
		}
	}
}