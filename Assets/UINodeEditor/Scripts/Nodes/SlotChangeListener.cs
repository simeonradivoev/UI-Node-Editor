using System;
using System.Collections.Generic;
using NodeEditor;

namespace UINodeEditor
{
	public class SlotChangeListener<T>
	{
		private ISlot m_Slot;
		private Action m_ChangeListener;
		private T m_OldValue;

		public SlotChangeListener(ISlot slot, Action action)
		{
			m_Slot = slot;
			m_ChangeListener = action;
		}

		public T this[AbstractNode node]
		{
			get
			{
				var newValue = node.GetSlotValue<T>(m_Slot);
				if (!EqualityComparer<T>.Default.Equals(newValue,m_OldValue))
				{
					m_OldValue = newValue;
					if(m_ChangeListener != null) m_ChangeListener.Invoke();
				}
				return newValue;

			}
		}
	}
}