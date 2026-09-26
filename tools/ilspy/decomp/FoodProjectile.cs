using System;
using UnityEngine;

public class FoodProjectile : PropProjectile
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FoodProjectile;
	}

	public override void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		if (!PredictedHasLanded && _source != null && _source.CheckFrontmostPrediction(PredictedEventType.BottleHitSound))
		{
			if (Array.IndexOf(EquipmentPrototype.Meat, Proto) != -1)
			{
				SoundManager.PlaySound3DFromList(SoundManager.SplatSounds, Position);
			}
			else
			{
				SoundManager.PlaySound3DFromList(SoundManager.LightImpactSounds, Position);
			}
		}
		base.Land(hitObject, dir, hitNormal);
	}

	public override void OnRigidBodyStopMoving(Vector3 pos, Quaternion rot)
	{
		base.OnRigidBodyStopMoving(pos, rot);
		if (IsAuthoritative())
		{
			FoodProp.Spawn(Proto, pos, rot, InfectedWith, _source);
		}
	}
}
