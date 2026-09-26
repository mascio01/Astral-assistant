using UnityEngine;

public struct Half3
{
	public ushort x;

	public ushort y;

	public ushort z;

	public static Half3 zero = new Half3(0, 0, 0);

	public Half3(ushort ix, ushort iy, ushort iz)
	{
		x = ix;
		y = iy;
		z = iz;
	}

	public Half3(Vector3 i)
	{
		x = MathUtil.EncodeLowPrecisionFloat(i.x);
		y = MathUtil.EncodeLowPrecisionFloat(i.y);
		z = MathUtil.EncodeLowPrecisionFloat(i.z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(MathUtil.DecodeLowPrecisionFloat(x), MathUtil.DecodeLowPrecisionFloat(y), MathUtil.DecodeLowPrecisionFloat(z));
	}
}
