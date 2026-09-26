using System;
using UnityEngine;

public abstract class PropProjectile : BaseThrownProjectile
{
	public Quaternion Rot;

	public TimeSpan LandedTime;

	public bool RigidBodyStoppedMoving;

	public bool SentRigidBodyStopMoving;

	private static float PhysicsStopMovingTolerance = 0.015f;

	private static TimeSpan MaxPhysicsTime = TimeSpan.FromSeconds(5.0);

	private static float FloorTolerance = 0.1f;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version >= 302 || !(this is PipeBombProjectile))
		{
			reflector.Add(ref Rot);
			reflector.Add(ref LandedTime);
			reflector.Add(ref RigidBodyStoppedMoving);
		}
	}

	public override bool PropWantDelete()
	{
		if (Landed)
		{
			return RigidBodyStoppedMoving;
		}
		return false;
	}

	public override void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		Landed = true;
		LandedTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (UnityObj != null && UnityObj.GetComponent<Rigidbody>() == null)
		{
			Rigidbody rigidbody = UnityObj.AddComponent<Rigidbody>();
			rigidbody.isKinematic = false;
			rigidbody.linearVelocity = _speed * dir * Mathf.Clamp01((_startPos - _targetPos).magnitude / 16f);
			UnityObj.layer = Character.DefaultLayer;
		}
		OnHitObjectNonExplosive(hitObject, dir, 0f, hitNormal);
		if (!IsAuthoritative())
		{
			return;
		}
		if (Predicted != null)
		{
			Predicted.WantContinueAfterAuthoritativeHasBeenDeleted = true;
		}
		if (Secret != SecrecyMode.OnlyKnownToSubjectCommunity && Secret != SecrecyMode.OnlyKnownToObject && Secret != SecrecyMode.OnlyKnownToSubject)
		{
			TileObject tileObject = _target;
			if (tileObject == null)
			{
				tileObject = hitObject;
			}
			Session.Instance.AISoundManager.AddSound(new AISound((HitDamage > 0f) ? AISoundType.Suspicious : AISoundType.Interesting, Position, 16f, 16f, this, _source, tileObject, _target));
		}
	}

	public override void OnRigidBodyMove(Vector3 pos, Quaternion rot, float wheelRPM)
	{
		Position = pos;
		Rot = rot;
	}

	public override void OnRigidBodyStopMoving(Vector3 pos, Quaternion rot)
	{
		RigidBodyStoppedMoving = true;
		Position = pos;
		Rot = rot;
		if (UnityObj != null)
		{
			Rigidbody component = UnityObj.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = true;
			}
		}
	}

	public override void UnitySendPhysicsStateToClients(InputFrame inputFrame)
	{
		if (UnityObj != null && Landed && !SentRigidBodyStopMoving)
		{
			if (!Session.Instance.IsPartyLeader())
			{
				return;
			}
			UnityObj.transform.position = GameTerrain.Instance.ClampPosWithinBounds(UnityObj.transform.position, 1f);
			float tileHeightAtPos = GameTerrain.Instance.GetTileHeightAtPos(UnityObj.transform.position.x, UnityObj.transform.position.z);
			if (UnityObj.transform.position.y < tileHeightAtPos - 1f)
			{
				Debug.LogWarning(GetDisplayNameString() + " fell through terrain at: " + UnityObj.transform.position.ToString());
				UnityObj.transform.position = new Vector3(UnityObj.transform.position.x, tileHeightAtPos + 0.01f, UnityObj.transform.position.z);
			}
			Rigidbody component = UnityObj.GetComponent<Rigidbody>();
			if (component == null || component.linearVelocity.sqrMagnitude < MathUtil.Squared(PhysicsStopMovingTolerance) || Session.Instance.PlayTime - LandedTime >= MaxPhysicsTime)
			{
				inputFrame.AddAction(InputAction.RigidBodyStopMoving(this, UnityObj.transform.position, UnityObj.transform.rotation));
				SentRigidBodyStopMoving = true;
				return;
			}
			inputFrame.AddAction(InputAction.RigidBodyMove(this, UnityObj.transform.position, UnityObj.transform.rotation));
		}
		if (Predicted != null)
		{
			Predicted.UnitySendPhysicsStateToClients(inputFrame);
		}
	}

	public override void UpdateUnityTransform()
	{
		if (UnityObj != null)
		{
			GameTerrain instance = GameTerrain.Instance;
			Vector3 position = UnityObj.transform.position;
			float tileHeightAtPos = instance.GetTileHeightAtPos(position.x, position.z);
			if (position.y < tileHeightAtPos - FloorTolerance)
			{
				position.y = tileHeightAtPos + FloorTolerance;
			}
			position.x = Mathf.Clamp(position.x, 0f - instance.HalfSize, instance.HalfSize);
			position.z = Mathf.Clamp(position.z, 0f - instance.HalfSize, instance.HalfSize);
			UnityObj.transform.position = position;
		}
		if (RigidBodyStoppedMoving)
		{
			if (UnityObj != null)
			{
				UnityObj.transform.position = Position;
				UnityObj.transform.rotation = Rot;
			}
		}
		else if (!Landed)
		{
			base.UpdateUnityTransform();
		}
	}
}
