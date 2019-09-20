using UnityEngine;
using UnityEngine.UI;

namespace UINodeEditor
{
    /// <summary>
    /// Event data used by all UI nodes when rendering, or during layout.
    /// </summary>
	public struct UIEventData
	{
        /// <summary>
        /// The global world rect of the current event.
        /// </summary>
		public Rect Rect;

        /// <summary>
        /// The current type of the event.
        /// </summary>
		public UIEventType EventType;

        /// <summary>
        /// The global wait handle for the master node.
        /// Each node should add a wait.
        /// </summary>
		public UIThreadWaitHandle WaitHandle;

        /// <summary>
        /// The global render buffer for the current graph.
        /// </summary>
		public UIRenderBuffer RenderBuffer;

        /// <summary>
        /// The global mesh repository for the current graph.
        /// </summary>
        public MeshRepository MeshRepository;

        public Rect GetRect(Rect child)
		{
			child.position += Rect.position;
			return child;
		}
	}
}