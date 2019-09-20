namespace UINodeEditor
{
    /// <summary>
    /// The type of UI event that a <see cref="UIMasterNode"/> will execute.
    /// 
    /// </summary>
	public enum UIEventType
	{
        /// <summary>
        /// Layout event is ran first on main thread.
        /// </summary>
		Layout,
        /// <summary>
        /// Pre Repaint is ran second on multiple threads
        /// </summary>
		PreRepaint,
        /// <summary>
        /// Repaint is ran final on main thread.
        /// </summary>
		Repaint
    }
}