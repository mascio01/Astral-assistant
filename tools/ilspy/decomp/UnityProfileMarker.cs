using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct UnityProfileMarker : IDisposable
{
	public UnityProfileMarker(string name)
	{
	}

	public void Dispose()
	{
	}
}
