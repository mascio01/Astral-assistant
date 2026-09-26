using System;

public class Snowman : Prop
{
	public SnowmanState SnowmanState;

	public float Meltedness;

	public TimeSpan LastUpdateTime;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Snowman;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref SnowmanState, 435);
		reflector.AddAfter(ref Meltedness, 435);
		reflector.AddAfter(ref LastUpdateTime, 435);
	}

	public override void OnSpawn()
	{
		LastUpdateTime = Session.Instance.PlayTime;
		base.OnSpawn();
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		TimeSpan timeSpan = Session.Instance.PlayTime - LastUpdateTime;
		LastUpdateTime = Session.Instance.PlayTime;
		Meltedness += Math.Max(0f, Session.Instance.Weather.TemperatureInCelsius / 10f) * (float)timeSpan.TotalSeconds / Sun.DayLengthSecs;
		SnowmanState snowmanState = SnowmanState.Happy;
		for (int i = 0; i < 3; i++)
		{
			if (Meltedness >= (float)i / 3f)
			{
				snowmanState = (SnowmanState)i;
			}
		}
		if (snowmanState > SnowmanState)
		{
			bool num = IsUnityObjectActive();
			if (num)
			{
				UnityDeactivate();
			}
			UnityDelete();
			SnowmanState = snowmanState;
			UnityInit();
			if (num)
			{
				UnityActivate();
			}
		}
		stillNeedUpdating = true;
	}

	public override PrefabResource GetUnityModel()
	{
		switch (SnowmanState)
		{
		case SnowmanState.Happy:
			if (Prototype == null || UnityModelIndexFromPrototype >= Prototype.Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Prefabs[UnityModelIndexFromPrototype];
		case SnowmanState.Melting:
			if (Prototype == null || Prototype.Stage1Prefabs == null || UnityModelIndexFromPrototype >= Prototype.Stage1Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Stage1Prefabs[UnityModelIndexFromPrototype];
		case SnowmanState.Melted:
			if (Prototype == null || Prototype.Stage2Prefabs == null || UnityModelIndexFromPrototype >= Prototype.Stage2Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Stage2Prefabs[UnityModelIndexFromPrototype];
		default:
			return null;
		}
	}

	public override bool PropWantDelete()
	{
		if (!(Meltedness >= 1f))
		{
			return base.PropWantDelete();
		}
		return true;
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		base.Delete();
	}
}
