using System.Threading;

namespace UINodeEditor
{
    /// <summary>
    /// A wait handle for UI element nodes.
    /// </summary>
	public class UIThreadWaitHandle
	{
		private int m_ActiveCount;

		public void Reset(int activeCount)
		{
			m_ActiveCount = activeCount;
		}

		public void MarkDone()
		{
			Interlocked.Decrement(ref m_ActiveCount);
		}

		public void AddWait()
		{
			Interlocked.Increment(ref m_ActiveCount);
		}

		public void WaitAll()
		{
			while (m_ActiveCount > 0)
			{
				Thread.SpinWait(0);
			}
		}
	}
}