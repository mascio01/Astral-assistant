using System;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public struct InhabitantSlotDef
{
	[DefaultValue(false)]
	public bool External;

	[XmlIgnore]
	public int NameHash;

	[DefaultValue("")]
	public string NativeName;

	public Vector3 Pos;

	[DefaultValue(0f)]
	public float PipFocusDist;

	[DefaultValue(0f)]
	public float CamDist;

	[DefaultValue(0)]
	public int SightRangeModifier;

	[DefaultValue(0)]
	public int WeaponRangeModifier;

	[DefaultValue(0)]
	public int MinWeaponRange;

	[DefaultValue(0f)]
	public float DefaultAngle;

	[DefaultValue(0f)]
	public float RangeAngle;

	public static InhabitantSlotDef CreateInternal()
	{
		return new InhabitantSlotDef(external: false, Vector3.zero, 0f, 0f, 0, 0, 0, 0f, 0f, string.Empty);
	}

	public static InhabitantSlotDef CreateExternal(Vector3 pos, float pipFocusDist, float camDist, int sightRangeModifier, int weaponRangeModifier, int minWeaponRange)
	{
		return new InhabitantSlotDef(external: true, pos, pipFocusDist, camDist, sightRangeModifier, weaponRangeModifier, minWeaponRange, 0f, 180f, string.Empty);
	}

	private InhabitantSlotDef(bool external, Vector3 pos, float pipFocusDist, float camDist, int sightRangeModifier, int weaponRangeModifier, int minWeaponRange, float defaultAngle, float angleRange, string name)
	{
		External = external;
		NameHash = ((!string.IsNullOrEmpty(name)) ? StringUtil.JenkinsHash(name) : 0);
		NativeName = null;
		Pos = pos;
		PipFocusDist = pipFocusDist;
		CamDist = camDist;
		SightRangeModifier = sightRangeModifier;
		WeaponRangeModifier = weaponRangeModifier;
		MinWeaponRange = minWeaponRange;
		DefaultAngle = defaultAngle;
		RangeAngle = angleRange;
	}

	public bool IsTargetInAngleRange(Building building, Vector2 targetPosXZ)
	{
		if (RangeAngle > 0f && RangeAngle < 180f)
		{
			float angle = MathUtil.WrapAngle(building.GetFacingAngleRad() + DefaultAngle * (MathF.PI / 180f));
			Vector2 vector = MathUtil.ToXZ(building.World.MultiplyPoint(Pos));
			Vector2 dirFromAngle = MathUtil.GetDirFromAngle(angle);
			Vector2 rhs = MathUtil.SafeNormalize(targetPosXZ - vector, dirFromAngle);
			if (Vector2.Dot(dirFromAngle, rhs) < Mathf.Cos(RangeAngle * (MathF.PI / 180f)))
			{
				return false;
			}
		}
		return true;
	}
}
