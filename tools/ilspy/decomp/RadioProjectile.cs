using UnityEngine;

public class RadioProjectile : PropProjectile
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RadioProjectile;
	}

	public override void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		if (!PredictedHasLanded && _source != null && _source.CheckFrontmostPrediction(PredictedEventType.BottleHitSound))
		{
			SoundManager.PlaySound3DFromList(SoundManager.ImpactSounds, Position);
		}
		base.Land(hitObject, dir, hitNormal);
	}

	public override void OnRigidBodyStopMoving(Vector3 pos, Quaternion rot)
	{
		base.OnRigidBodyStopMoving(pos, rot);
		if (IsAuthoritative())
		{
			RadioProp.Spawn(_source, pos, rot);
		}
	}
}
