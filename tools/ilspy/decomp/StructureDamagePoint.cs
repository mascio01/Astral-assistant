using System;
using UnityEngine;

public struct StructureDamagePoint : IReflectable
{
	public StructureDamageType Type;

	public Vector3 Pos;

	public Vector3 Normal;

	public float Fuel;

	public bool Burning;

	public float Size;

	public float Angle;

	public Matrix4x4 InvDecalTransform;

	public Character Source;

	public static float FireSpreadRate = 0.1f;

	public static float FireDamageRate = 0.5f;

	public static StructureDamagePoint Create(StructureDamageType type, Vector3 pos, Vector3 normal, float size, Character source, float fuel, bool burning)
	{
		StructureDamagePoint result = default(StructureDamagePoint);
		result.Type = type;
		result.Pos = pos;
		result.Normal = normal;
		result.Fuel = fuel;
		result.Burning = burning;
		result.Size = size;
		result.Angle = MathUtil.RandomFloat((float)(source?.Id ?? 0) + pos.x * 324f + pos.y * 77f + pos.z * 8911f) * (MathF.PI * 2f);
		result.Source = source;
		result.InvDecalTransform = Matrix4x4.identity;
		result.SetupDecal();
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref Pos);
		reflector.Add(ref Normal);
		reflector.Add(ref Fuel);
		reflector.Add(ref Burning);
		reflector.Add(ref Size);
		reflector.Add(ref Angle);
		reflector.Add(ref Source);
		if (reflector.IsDeserialising)
		{
			InvDecalTransform = Matrix4x4.identity;
			SetupDecal();
		}
	}

	public StructureDamagePoint Burn(Prop prop, TimeSpan dt, ref float fireDamage)
	{
		float num = (float)dt.TotalSeconds;
		Size += FireSpreadRate * num;
		fireDamage += Size * FireDamageRate * num;
		Fuel = Math.Max(0f, Fuel - TileObject.FuelBurningRateFlOzPerSecond * num);
		SetupDecal();
		if (Fuel <= 0f && prop.GetFlammability() < Flammability.High)
		{
			Burning = false;
		}
		return this;
	}

	private void SetupDecal()
	{
		Vector3 vector = ((!(Math.Abs(Vector3.Dot(Normal, Vector3.up)) <= 0.9f)) ? Vector3.Normalize(Vector3.Cross(Normal, Vector3.forward)) : Vector3.Normalize(Vector3.Cross(Normal, Vector3.up)));
		Matrix4x4 mat = Matrix4x4.identity;
		MathUtil.SetForward(ref mat, Normal);
		MathUtil.SetRight(ref mat, vector);
		MathUtil.SetUp(ref mat, Vector3.Normalize(Vector3.Cross(Normal, vector)));
		Matrix4x4 mat2 = MathUtil.CreateFromAxisAngle(Normal, Angle) * mat * MathUtil.CreateScale(Size * 2f);
		MathUtil.SetTranslation(ref mat2, Pos - mat2.Up() * 0.5f - mat2.Right() * 0.5f);
		InvDecalTransform = Matrix4x4.Inverse(mat2);
	}
}
