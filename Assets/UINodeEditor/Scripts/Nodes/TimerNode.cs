using NodeEditor;
using NodeEditor.Controls;
using NodeEditor.Slots;
using UnityEngine;

namespace UINodeEditor
{
    /// <summary>
    /// A timer node increments a float based on a speed input.
    /// This is used if speed needs to change and the <see cref="NodeEditor.Nodes.Input.TimeNode"/> can't handle variable speed.
    /// </summary>
    [Title("Input", "Timer")]
    public class TimerNode : AbstractNode, ITickableNode
    {
        [SerializeField] private bool m_unscaled;

        /// <summary>
        /// If true timer will be increased by unscaled delta time.
        /// If false timer will be increased by scaled delta time.
        /// </summary>
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

        void ITickableNode.Tick()
        {
            m_Time.SetDefaultValue(m_Time.value + (m_unscaled ? Time.unscaledDeltaTime : Time.deltaTime) * m_Speed[this]);
        }
    }
}