using System;

namespace UINodeEditor
{
	[Flags]
	public enum UIDirtyType
	{
		None = 0,
		Mesh = 1,
		Layout = 2
	}
}