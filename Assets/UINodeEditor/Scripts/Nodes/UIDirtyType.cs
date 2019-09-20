using System;

namespace UINodeEditor
{
    /// <summary>
    /// Flags that indicated by what kind of operation was a UI dirtied by.
    /// </summary>
	[Flags]
	public enum UIDirtyType
	{
		None = 0,
		Mesh = 1,
		Layout = 2
	}
}