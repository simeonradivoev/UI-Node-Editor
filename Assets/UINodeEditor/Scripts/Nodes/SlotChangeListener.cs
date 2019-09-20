using System;
using System.Collections.Generic;
using NodeEditor;

namespace UINodeEditor
{
    /// <summary>
    /// A slot change listener can access a slot value from a node and compare it with an old cached value to see if new values differs. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// Get the slot value from a given node.
        /// </summary>
        /// <param name="node">The node that the slot belongs to.</param>
        /// <returns>The current value of the slot.</returns>
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