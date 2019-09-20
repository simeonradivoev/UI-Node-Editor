using System;
using System.Collections.Generic;
using System.Threading;
using NodeEditor;
using NodeEditor.Slots;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// A node that acts like a filter for UI elements. If the filter input is false then no children will be executed.
    /// </summary>
	[Title("Util","Execution Filter")]
	public class ExecutionFilterNode : AbstractNode
	{
		private EmptySlot<Action<UIEventData>> m_Event;
		private ValueSlot<bool> m_Filter;
		private List<Action<UIEventData>> m_ValuesTmp = new List<Action<UIEventData>>();

		public ExecutionFilterNode()
		{
			m_Event = CreateInputSlot<EmptySlot<Action<UIEventData>>>("Event");
			m_Event.SetAllowMultipleConnections(true);
			m_Filter = CreateInputSlot<ValueSlot<bool>>("Filter").SetShowControl();
			CreateOutputSlot<DefaultValueSlot<Action<UIEventData>>>("UI Event").SetDefaultValue(Execute);
		}

		private void Execute(UIEventData eventData)
		{
			try
			{
				Execute(eventData, eventData.Rect);
			}
			finally
			{
				if (eventData.WaitHandle != null) eventData.WaitHandle.MarkDone();
			}
		}

		protected virtual void Execute(UIEventData eventData, Rect rect)
		{
			if(!m_Filter[this]) return;

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