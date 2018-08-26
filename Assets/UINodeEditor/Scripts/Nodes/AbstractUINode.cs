using System;
using NodeEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UINodeEditor
{
	public class AbstractUINode : AbstractNode
	{
		private UIDirtyType m_Dity;

		public AbstractUINode()
		{
			
		}

		public override void SetOwner(IGraph graph)
		{
			base.SetOwner(graph);
			ownerObject = graph.owner as UIGraphObject;
		}

		public SlotChangeListener<T> CreateSlotListener<T>(ISlot slot, Action listener)
		{
			return new SlotChangeListener<T>(slot, listener);
		}

		protected UIGraphObject ownerObject { get; private set; }

		protected void OnMeshDirty()
		{
			m_Dity |= UIDirtyType.Mesh;
		}

		protected void OnAllDirty()
		{
			m_Dity = UIDirtyType.Mesh | UIDirtyType.Layout;
		}

		internal void ClearDirty()
		{
			m_Dity = UIDirtyType.None;
		}

		protected UIDirtyType isDirty
		{
			get { return m_Dity; }
		}
	}
}