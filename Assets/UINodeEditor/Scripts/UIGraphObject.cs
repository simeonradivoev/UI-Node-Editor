using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UINodeEditor
{
	[CreateAssetMenu]
	public class UIGraphObject : GraphObjectBase, IReferenceTable, ISerializationCallbackReceiver, IPropertyTable
	{
        private Dictionary<Guid, Object> m_References = new Dictionary<Guid, Object>();
		[SerializeField, HideInInspector] private List<ReferenceEntry> m_SerializedReferences = new List<ReferenceEntry>();
		[SerializeField, HideInInspector] private TextAsset m_DataAsset;
		[SerializeField, HideInInspector] private string m_SerializedGraph;

		IGraph m_DeserializedGraph;

		[Serializable]
		public struct ReferenceEntry
		{
			public byte[] guid;
			public Object obj;
		}

		public void SetReferenceValue(Guid id, Object value)
		{
			if (m_References.ContainsKey(id))
			{
				m_References[id] = value;
			}
			else
			{
				m_References.Add(id,value);
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		public Object GetReferenceValue(Guid id, out bool idValid)
		{
			Object val;
			idValid = m_References.TryGetValue(id, out val);
			return val;
		}

		public void ClearReferenceValue(Guid id)
		{
			m_References.Remove(id);
		}

		public T GetPropertyValue<T>(Guid id, out bool validId)
		{
			T val;
			validId = GlobalProperties<T>.TryGetValue(id, out val);
			return val;
		}

		public void ClearPropertyValue<T>(Guid id)
		{
			GlobalProperties<T>.RemoveKey(id);
		}

		public void SetPropertyValue<T>(Guid id, T val)
		{
			GlobalProperties<T>.SetValue(id,val);
		}

		public void OnBeforeSerialize()
		{
			var elementData = SerializationHelper.Serialize(graph);
			m_SerializedGraph = JsonUtility.ToJson(elementData);

			m_SerializedReferences.Clear();
			foreach (var o in m_References)
			{
				m_SerializedReferences.Add(new ReferenceEntry(){guid = o.Key.ToByteArray(),obj = o.Value});
			}
		}

		public void OnAfterDeserialize()
		{
			try
			{
				var deserializedGraph = SerializationHelper.Deserialize<IGraph>(JsonUtility.FromJson<SerializationHelper.JSONSerializedElement>(m_SerializedGraph), null);
				if (graph == null)
					graph = deserializedGraph;
				else
					m_DeserializedGraph = deserializedGraph;
			}
			catch (Exception e)
			{
				graph = new NodeGraph();
			}

			m_References.Clear();
			foreach (var referenceEntry in m_SerializedReferences)
			{
				m_References.Add(new Guid(referenceEntry.guid), referenceEntry.obj);
			}
			m_SerializedReferences.Clear();
		}

		protected override void UndoRedoPerformed()
		{
			if (m_DeserializedGraph != null)
			{
				graph.ReplaceWith(m_DeserializedGraph);
				m_DeserializedGraph = null;
			}
		}

		protected override void Validate()
		{
			foreach (var mReference in m_References.Keys.ToArray())
			{
				if (!graph.ContainsNodeGuid(mReference)) m_References.Remove(mReference);
			}
		}
	}
}