using UnityEngine;

public abstract class Weapon : Equipment
{
	public override bool AllowLockOn(Character character)
	{
		return true;
	}

	public virtual float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		bestInfectedWith = InfectionType.None;
		return 0f;
	}

	public Vector3 GetMuzzleWorldPos(Character character, bool deterministic)
	{
		if (character.IsUnityObjectActive() && !deterministic)
		{
			Vector3 muzzleLocalPos = GetMuzzleLocalPos();
			Matrix4x4 unityBoneTransform = character.GetUnityBoneTransform(GetEquippedBone());
			Matrix4x4 matrix4x = Matrix4x4.TRS(GetEquippedLocalPos(), Quaternion.Euler(GetEquippedLocalRotation()), GetEquippedLocalScale());
			return (unityBoneTransform * matrix4x).MultiplyPoint(muzzleLocalPos);
		}
		Vector3 gunPosition = character.GunPosition;
		if (character.InsideBuilding != null && character.InsideBuilding.IsGuardPost())
		{
			gunPosition += character.Forward * RangedAttack.WatchTowerOffsetHack;
		}
		return gunPosition;
	}
}
