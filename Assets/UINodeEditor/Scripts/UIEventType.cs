namespace UINodeEditor
{
	public enum UIEventType
	{
        //Layout event is ran first on main thread.
		Layout,
        //Pre Repaint is ran second on multiple threads
		PreRepaint,
        //Repaint is ran final on main thread.
		Repaint
	}
}