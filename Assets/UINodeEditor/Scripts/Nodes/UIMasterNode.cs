using System;
using System.Collections.Generic;
using NodeEditor;
using NodeEditor.Slots;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// Master node is the main node in each graph. It acts like a canvas for the UI.
    /// It executes the calculations and gets the values from all children connected to it.
    /// It also has the rect and matrix of the "canvas".
    /// </summary>
	[Title("UI Master Node")]
	public class UIMasterNode : AbstractNode
	{
		private EmptySlot<Action<UIEventData>> m_Input;
		private DefaultValueSlot<Matrix4x4> m_Matrix;
		private ValueSlot<Rect> m_Rect;
		private List<KeyValuePair<ISlot, INode>> m_TmpSlots;

        public Matrix4x4 matrix => m_Matrix[this];
        public Rect rect => m_Rect[this];

        public UIMasterNode()
		{
			name = "UI Master Node";
			m_Input = CreateInputSlot<EmptySlot<Action<UIEventData>>>("UI Event");
			m_Input.SetAllowMultipleConnections(true);
			m_Rect = CreateInputSlot<ValueSlot<Rect>>("Rect").SetShowControl();
			m_Matrix = CreateInputSlot<DefaultValueSlot<Matrix4x4>>("Matrix").SetDefaultValue(Matrix4x4.identity);
		}

        public void GetInputValues(IList<Action<UIEventData>> values)
        {
            GetSlotValues(m_Input, values);
        }
    }
}