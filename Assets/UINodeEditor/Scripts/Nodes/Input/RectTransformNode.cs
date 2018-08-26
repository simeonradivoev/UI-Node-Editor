using System;
using NodeEditor;
using NodeEditor.Controls;
using UnityEngine;

namespace UINodeEditor
{
	[Title("Builder","Rect Transform")]
	public class RectTransformNode : AbstractUINode
	{
		[SerializeField] private Vector2 m_AnchorMin = new Vector2(0.5f,0.5f);
		[SerializeField] private Vector2 m_AnchorMax = new Vector2(0.5f,0.5f);
		[SerializeField] private Vector2 m_OffsetMin = new Vector2(0, 0);
		[SerializeField] private Vector2 m_OffsetMax = new Vector2(0, 0);
		[SerializeField] private Vector2 m_Position = new Vector2(0,0);
		[SerializeField] private Vector2 m_Size = new Vector2(100,100);

		[DefaultControl(label = "Anchor Min")]
		public Vector2 anchorMin
		{
			get { return m_AnchorMin; }
			set { m_AnchorMin = value; }
		}

		[DefaultControl(label = "Anchor Max")]
		public Vector2 anchorMax
		{
			get { return m_AnchorMax; }
			set { m_AnchorMax = value; }
		}

		[DefaultControl(label = "Offset Min")]
		public Vector2 offsetMin
		{
			get { return m_OffsetMin; }
			set { m_OffsetMin = value; }
		}

		[DefaultControl(label = "Offset Max")]
		public Vector2 offsetMax
		{
			get { return m_OffsetMax; }
			set { m_OffsetMax = value; }
		}

		/*[DefaultControl(label = "Position")]
		public Vector2 position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		[DefaultControl(label = "Size")]
		public Vector2 size
		{
			get { return m_Size; }
			set { m_Size = value; }
		}*/

		[SerializeField] 
		private ValueSlot<Rect> m_OutRect;
		private ValueSlot<Action<UIEventData>> m_EventOut;

		public RectTransformNode()
		{
			m_EventOut = CreateOutputSlot<ValueSlot<Action<UIEventData>>>("outEvent", "Event").SetValue(CalculateRect);
			m_OutRect = CreateOutputSlot<ValueSlot<Rect>>("outRect","Rect");
		}

		private void CalculateRect(UIEventData eventData)
		{
			if (eventData.EventType == UIEventType.Layout)
			{
				var parentRect = eventData.Rect;
				var outRect = new Rect
				{
					x = m_AnchorMin.x * parentRect.width + m_OffsetMin.x,
					y = m_AnchorMin.y * parentRect.height + m_OffsetMin.y,
					width = m_AnchorMax.x * parentRect.width - m_OffsetMax.x - m_AnchorMin.x * parentRect.width - m_OffsetMin.x,
					height = m_AnchorMax.y * parentRect.height - m_OffsetMax.y - m_AnchorMin.y * parentRect.height - m_OffsetMin.y
				};

				m_OutRect.SetValue(outRect);
			}
		}
	}
}