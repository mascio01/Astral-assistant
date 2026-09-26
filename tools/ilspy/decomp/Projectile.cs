using System;
using UnityEngine;

public abstract class Projectile : MultiTileObject
{
	public Projectile Predicted;

	public Projectile Authoritative;

	public int PredictedFreshFrame = -1;

	public bool WantContinueAfterAuthoritativeHasBeenDeleted;

	public bool PredictedHasLanded;

	public Character _source;

	public TileObject _target;

	public TimeSpan _startTime;

	public GameObject UnityObj;

	public override bool WantPrediction()
	{
		return !PredictedHasLanded;
	}

	public override bool IsPredicted()
	{
		return PredictedFreshFrame != -1;
	}

	public override bool IsAuthoritative()
	{
		return PredictedFreshFrame == -1;
	}

	public override int GetPredictedFreshFrame()
	{
		return PredictedFreshFrame;
	}

	public override TileObject GetPredicted()
	{
		return Predicted;
	}

	public override TileObject GetAuthoritative()
	{
		return Authoritative;
	}

	public override void SetPredictedFreshFrame(int frame)
	{
		PredictedFreshFrame = frame;
	}

	public override void SetPredicted(TileObject obj)
	{
		Predicted = (Projectile)obj;
	}

	public override void SetAuthoritative(TileObject obj)
	{
		Authoritative = (Projectile)obj;
	}

	public override void InitPredicted()
	{
		if (Authoritative != null && Authoritative.UnityObj != null)
		{
			UnityObj = Authoritative.UnityObj;
			Authoritative.UnityObj = null;
			int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(Authoritative);
			if (num != -1)
			{
				Session.Instance.ActiveMovingUnityObjects[num] = this;
			}
		}
	}

	public override void DeletePredicted()
	{
		if (Authoritative == null || Authoritative.PropWantDelete())
		{
			UnityDelete();
			return;
		}
		Authoritative.UnityObj = UnityObj;
		UnityObj = null;
		int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(this);
		if (num != -1)
		{
			Session.Instance.ActiveMovingUnityObjects[num] = Authoritative;
		}
	}

	public override void PredictedUpdate(TimeSpan dt)
	{
		ProjectileUpdate(dt);
	}

	public override bool PredictedWantDelete()
	{
		return PropWantDelete();
	}

	public override bool PredictedWantContinueAfterAuthoritativeHasBeenDeleted()
	{
		return WantContinueAfterAuthoritativeHasBeenDeleted;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref _source);
		reflector.Add(ref _target);
		reflector.Add(ref _startTime);
	}

	public override void PredictedFixup()
	{
		base.PredictedFixup();
		_source = ((_source != null) ? _source.GetPredictedOrElseThisCharacter() : null);
		_target = ((_target != null) ? _target.GetPredictedOrElseThis() : null);
	}

	public override void Init()
	{
		base.Init();
		AddToTerrain();
		Session.Instance.PropManager.Projectiles.Add(this);
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
		Session.Instance.PropManager.Projectiles.Remove(this);
		RemoveFromTerrain();
		base.Delete();
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		ProjectileUpdate(dt);
		stillNeedUpdating = true;
	}

	public virtual void ProjectileUpdate(TimeSpan dt)
	{
	}

	public virtual void UpdateUnityTransform()
	{
	}

	public override void UnityInit()
	{
		base.UnityInit();
		if (UnityObj == null)
		{
			PrefabResource unityModel = GetUnityModel();
			if (unityModel != null && unityModel.GetAsset() != null)
			{
				UnityObj = unityModel.InstantiatePrefab(GameTerrain.Instance.UnityTerrainObj.transform);
				UpdateUnityTransform();
			}
		}
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: false);
		}
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: true);
		}
	}

	public override void UnityDeactivate()
	{
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: false);
		}
		base.UnityDeactivate();
	}

	public override void UnityDelete()
	{
		base.UnityDelete();
		if (UnityObj != null)
		{
			UnityEngine.Object.Destroy(UnityObj);
			UnityObj = null;
		}
	}

	public override void UnityUpdate()
	{
		if (!IsBeingPredicted())
		{
			base.UnityUpdate();
			UpdateUnityTransform();
		}
	}

	public override bool HasUnityObject()
	{
		return UnityObj != null;
	}

	public override bool IsUnityObjectActive()
	{
		if (UnityObj != null)
		{
			return UnityObj.activeSelf;
		}
		return false;
	}

	public override bool IsUnityObjectStatic()
	{
		return false;
	}

	public override bool WantUnityObjectToStayActive()
	{
		return true;
	}
}
