using System;
using System.Collections.Generic;
using System.Threading;
using NodeEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UINodeEditor
{
    public class UIGraphRenderer : IDisposable
    {
        private List<Action<UIEventData>> m_ValuesTmp = new List<Action<UIEventData>>();

        private UIRenderBuffer m_RenderBuffer = new UIRenderBuffer();

        private MeshRepository m_MeshRepository = new MeshRepository();

        private UIThreadWaitHandle m_WaitHandle = new UIThreadWaitHandle();

        /// <summary>
        /// Calculates values and meshes for all nodes.
        /// </summary>
        /// <param name="renderBuffer">The render buffer that will hold all render instructions.</param>
        /// <param name="commandBuffer">The command buffer that will have all the final mesh rendering instruction that were extracted from a <see cref="RenderBuffer"/>.</param>
        private void ExecuteMasterNode(bool threaded, Rect rect,Matrix4x4 globalMatrix, UIMasterNode masterNode,CommandBuffer commandBuffer)
        {
            masterNode.GetInputValues(m_ValuesTmp);

            Rect localRect = new Rect(Vector2.zero, rect.size);

            //can only be ran on main thread
            foreach (var value in m_ValuesTmp)
            {
                value.Invoke(new UIEventData() { EventType = UIEventType.Layout, Rect = localRect });
            }

            //can run on multiple threads
            if (threaded)
            {
                m_WaitHandle.Reset(m_ValuesTmp.Count);

                foreach (var a in m_ValuesTmp)
                {
                    var a1 = a;
                    ThreadPool.QueueUserWorkItem(c => a1.Invoke((UIEventData)c), new UIEventData() { EventType = UIEventType.PreRepaint, Rect = localRect, RenderBuffer = m_RenderBuffer, WaitHandle = m_WaitHandle });
                }

                m_WaitHandle.WaitAll();
            }
            else
            {
                foreach (var action in m_ValuesTmp)
                {
                    action.Invoke(new UIEventData() { EventType = UIEventType.PreRepaint, Rect = localRect, RenderBuffer = m_RenderBuffer,MeshRepository = m_MeshRepository });
                }
            }

            //can run only on main thread.
            foreach (var action in m_ValuesTmp)
            {
                action.Invoke(new UIEventData() { EventType = UIEventType.Repaint, Rect = localRect, RenderBuffer = m_RenderBuffer,MeshRepository = m_MeshRepository });
            }

            var matrix = globalMatrix * Matrix4x4.Translate(rect.position);
            m_RenderBuffer.Populate(commandBuffer, matrix);

            m_ValuesTmp.Clear();
        }

        public void PopulateCommandBuffer(bool threaded, Rect rect, Matrix4x4 globalMatrix, IEnumerable<INode> nodes, CommandBuffer commandBuffer)
        {
            foreach (var node in nodes)
            {
                if (node is UIMasterNode masterNode)
                {
                    commandBuffer.Clear();
                    try
                    {
                        ExecuteMasterNode(threaded, rect, globalMatrix, masterNode, commandBuffer);
                    }
                    finally
                    {
                        m_RenderBuffer.Clear();
                        m_MeshRepository.ClearCurrent();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Calculates all master nodes and fills the given command buffer.
        /// </summary>
        /// <param name="threaded">Should <see cref="UIEventType.PreRepaint"/> ne executed on multiple threads.</param>
        /// <param name="nodes">All the nodes to go over.</param>
        /// <param name="renderBuffer"></param>
        /// <param name="commandBuffer">The command buffer to execute.</param>
        public void PopulateCommandBuffer(bool threaded,IEnumerable<INode> nodes,CommandBuffer commandBuffer)
        {
            foreach (var node in nodes)
            {
                if (node is UIMasterNode masterNode)
                {
                    commandBuffer.Clear();
                    try
                    {
                        ExecuteMasterNode(threaded, masterNode.rect, masterNode.matrix, masterNode, commandBuffer);
                    }
                    finally
                    {
                        m_RenderBuffer.Clear();
                        m_MeshRepository.ClearCurrent();
                    }
                    break;
                }
            }
        }

        public void Dispose()
        {
            m_ValuesTmp.Clear();
            m_RenderBuffer.Clear();
            m_WaitHandle.MarkDone();
        }
    }
}