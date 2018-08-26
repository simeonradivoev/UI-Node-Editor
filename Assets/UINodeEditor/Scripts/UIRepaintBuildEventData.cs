using UnityEngine;
using UnityEngine.UI;

namespace UINodeEditor
{
	public struct UIEventData
	{
		public Rect Rect;
		public UIEventType EventType;
		public UIThreadWaitHandle WaitHandle;
		public UIRenderBuffer RenderBuffer;
		public VertexHelper VertexHelper;

		public Rect GetRect(Rect child)
		{
			child.position += Rect.position;
			return child;
		}
	}
}