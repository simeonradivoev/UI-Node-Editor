using System;
using System.Collections.Generic;
using NodeEditor;
using UINodeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(UIGraphObject))]
public class UIGraphObjectInspector : Editor
{
    private Vector2 scrollPos;

    private SerializedProperty m_SerializedReferences;

    private UIGraphRenderer m_Renderer;
    private CommandBuffer m_CommandBuffer;

    private void OnEnable()
    {
        m_SerializedReferences = serializedObject.FindProperty("m_SerializedReferences");

        m_Renderer = new UIGraphRenderer();
        m_CommandBuffer = new CommandBuffer(){name = target.name + " Preview"};
    }

    private void OnDisable()
    {
        m_CommandBuffer.Dispose();
        m_Renderer.Dispose();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        EditorGUI.BeginDisabledGroup(true);
        for (int i = 0; i < m_SerializedReferences.arraySize; i++)
        {
            var element = m_SerializedReferences.GetArrayElementAtIndex(i);

            var guid = element.FindPropertyRelative("guid");
            byte[] guidArray = new byte[guid.arraySize];
            for (int j = 0; j < guid.arraySize; j++)
            {
                guidArray[j] = (byte)guid.GetArrayElementAtIndex(j).intValue;
            }
            var guidValue = new Guid(guidArray);

            
            EditorGUILayout.PropertyField(element.FindPropertyRelative("obj"), new GUIContent(guidValue.ToString()));
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.EndScrollView();
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Event.current.type == EventType.Repaint)
        {
            var graphObject = (UIGraphObject) target;

            GUI.BeginClip(r);
            Matrix4x4 rotateMatrix = Matrix4x4.TRS(new Vector3(0, r.height), Quaternion.Euler(0, 0, 0), new Vector3(1, -1, 1));

            m_CommandBuffer.Clear();
            m_Renderer.PopulateCommandBuffer(false,r, rotateMatrix, graphObject.graph.GetNodes(), m_CommandBuffer);

            Graphics.ExecuteCommandBuffer(m_CommandBuffer);
            GUI.EndClip();
        }
    }
}
