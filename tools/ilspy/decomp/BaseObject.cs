using System;
using System.Text;
using UnityEngine;

public abstract class BaseObject : IReflectable
{
	public int Id;

	public virtual bool Deleted => false;

	public virtual int NameHash => 0;

	public virtual string Category => "";

	public bool IsGhost()
	{
		return Id == 0;
	}

	public abstract BaseObjectType GetBaseObjectType();

	public void AssignId(int id)
	{
		Id = id;
	}

	public virtual void ReflectEarly(Reflector reflector)
	{
	}

	public virtual void Reflect(Reflector reflector)
	{
		if (reflector.IsTextDumping)
		{
			reflector.AddTextDumpLine("============================== " + GetDisplayNameString() + " (" + Id + ") ==============================");
		}
	}

	public virtual void Init()
	{
		if (Id == 0)
		{
			AssignId(BaseObjectManager.Instance.Assign(this));
		}
	}

	public virtual void Delete()
	{
		BaseObjectManager.Instance.Remove(this);
		UnityDelete();
	}

	public virtual bool IsDisappeared()
	{
		return false;
	}

	public virtual void OnSpawn()
	{
		Init();
		if (Session.Instance.State >= SessionState.UnityInited)
		{
			UnityInit();
		}
	}

	public virtual string GetUniqueID()
	{
		return null;
	}

	public virtual void SetUniqueID(string id)
	{
		Debug.LogError("Trying to set a unique id on an object that doesn't support it");
	}

	public virtual void UnityInit()
	{
	}

	public virtual void UnityDelete()
	{
	}

	public virtual void OnStoryReloaded()
	{
	}

	public string GetDisplayNameString(bool noStrangers, bool englishOnly)
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		BuildDisplayName(stringBuilder, noStrangers, englishOnly);
		return stringBuilder.ToString();
	}

	public string GetDisplayNameString()
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		return stringBuilder.ToString();
	}

	public virtual int GetDisplayNameHashIfGeneric(bool noStrangers)
	{
		return 0;
	}

	public virtual void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		sb.Append((NameHash != 0) ? GameImpl.Translate(NameHash, englishOnly) : GetType().Name);
	}

	public virtual void BuildSubDisplayName(StringBuilder sb)
	{
	}

	public virtual GenderType GetGender(Language language = Language.Count)
	{
		return GenderType.Count;
	}

	public virtual bool IsPlural()
	{
		return false;
	}

	public virtual bool IsUncountable()
	{
		return false;
	}

	public virtual bool IsMany()
	{
		return false;
	}

	public virtual bool IsZero()
	{
		return false;
	}

	public virtual bool IsPlayerAvatar()
	{
		return false;
	}

	public virtual Color GetSubDisplayCol2()
	{
		return Color.gray;
	}

	public virtual string GetLootLocation()
	{
		return string.Empty;
	}

	public virtual void SetLootLocation(string lootLocation)
	{
	}

	public virtual BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		return BulletHitEffect.None;
	}

	public virtual Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		mat = null;
		col = Color.white;
		return null;
	}

	public virtual Texture2D GetIconResource()
	{
		return null;
	}

	public virtual Community GetCommunity()
	{
		return null;
	}

	public virtual Character GetAsCharacter()
	{
		return null;
	}

	public virtual TerrainCoord GetTile()
	{
		return TerrainCoord.Invalid;
	}

	public virtual ImpactSusceptibility GetImpactSusceptibility()
	{
		return ImpactSusceptibility.Invulnerable;
	}

	public bool IsSusceptibleToVehicleCollisions(bool juggernaut)
	{
		return (int)GetImpactSusceptibility() >= (juggernaut ? 1 : 2);
	}

	public bool IsSusceptibleToBulletHits()
	{
		return GetImpactSusceptibility() >= ImpactSusceptibility.Medium_BulletHits;
	}

	public virtual float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		return 0f;
	}

	public virtual void OnProjectileHit(Character shooter, Vector3 deterministicDir, Vector3 deterministicHitPos, float damage, bool predicted)
	{
	}

	public virtual bool IsPredicted()
	{
		return false;
	}

	public virtual bool IsAuthoritative()
	{
		return true;
	}

	public virtual float GetMassEstimate()
	{
		return 1000f;
	}

	public virtual void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
	}

	public virtual void PropUpdateRare(ref bool stillNeedUpdating)
	{
	}

	public virtual bool PropWantDelete()
	{
		return false;
	}

	public virtual void CalcApprovalRatingForCharacterOrCommunity(BaseObject obj, out float approval, out float respect)
	{
		CalcApprovalRatingForCharacterOrCommunity(obj, out approval, out respect, Target.Never, TimeSpan.MaxValue);
	}

	public virtual void CalcApprovalRatingForCharacterOrCommunity(BaseObject obj, out float approval, out float respect, TimeSpan afterTime, TimeSpan beforeTime)
	{
		approval = 0f;
		respect = 0f;
	}
}
