using NodeEditor;
using UnityEngine;

namespace UINodeEditor.Math
{
    /// <summary>
    /// Matrix4x4 multiplication node.
    /// </summary>
	[Title("Math", "Multiply", "Multiply: Matrix4x4")]
	public class MatrixMultiply : MultiplyNode<Matrix4x4>
	{
		protected override Matrix4x4 Multiply(Matrix4x4 lhs, Matrix4x4 rhs)
		{
			return lhs * rhs;
		}
	}

    /// <summary>
    /// Base class for all multiplications nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public abstract class MultiplyNode<T> : AbstractNode
	{
		private EmptySlot<T> m_Input0;
		private EmptySlot<T> m_Input1;

		protected MultiplyNode()
		{
			m_Input0 = CreateInputSlot<EmptySlot<T>>("0");
			m_Input1 = CreateInputSlot<EmptySlot<T>>("1");
			CreateOutputSlot<GetterSlot<T>>("Out").SetGetter(MultiplyInternal);
		}

		private T MultiplyInternal()
		{
			return Multiply(m_Input0[this], m_Input1[this]);
		}

        /// <summary>
        /// Called when the output is accessed.
        /// </summary>
        /// <param name="lhs">Value 1.</param>
        /// <param name="rhs">Value 2.</param>
        /// <returns>The multiplication of <param name="lhs"> and <param name="rhs"></param></param></returns>
		protected abstract T Multiply(T lhs, T rhs);
	}
}