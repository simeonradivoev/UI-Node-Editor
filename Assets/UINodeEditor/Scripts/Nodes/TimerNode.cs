using NodeEditor;
using NodeEditor.Controls;
using NodeEditor.Slots;
using UnityEngine;

namespace UINodeEditor
{
    [Title("Input", "Timer")]
    public class TimerNode : AbstractNode, ITickableNode
    {
        [SerializeField] private bool m_unscaled;

        [DefaultControl(label = "Unscaled")]
        public bool unscaled
        {
            get => m_unscaled;
            set => m_unscaled = value;
        }

        private ValueSlot<float> m_Speed;
        private DefaultValueSlot<float> m_Time;

        public TimerNode()
        {
            m_Speed = CreateInputSlot<ValueSlot<float>>("Speed").SetValue(1).SetShowControl();
            m_Time = CreateOutputSlot<DefaultValueSlot<float>>("Time");
        }

        public void Tick()
        {
            m_Time.SetDefaultValue(m_Time.value + (m_unscaled ? Time.unscaledDeltaTime : Time.deltaTime) * m_Speed[this]);
        }
    }
}