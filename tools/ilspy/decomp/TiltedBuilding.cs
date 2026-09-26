using UnityEngine;

public class TiltedBuilding : Building
{
	public virtual Vector2 WheelPos
	{
		get
		{
			if (Prototype == null)
			{
				return Vector2.zero;
			}
			return Prototype.WheelPos;
		}
	}

	public virtual Vector2 WheelOffset
	{
		get
		{
			if (Prototype == null)
			{
				return Vector2.zero;
			}
			return Prototype.WheelOffset;
		}
	}

	public virtual float ExtraFrontWheelSeparation
	{
		get
		{
			if (Prototype == null)
			{
				return 0f;
			}
			return Prototype.ExtraFrontWheelSeparation;
		}
	}

	public virtual float MaxYaw
	{
		get
		{
			if (Prototype == null)
			{
				return 15f;
			}
			return Prototype.MaxYaw;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TiltedBuilding;
	}

	public override bool IsTiltableProp()
	{
		return true;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return TiltedProp.CalcModelTransform(GetUnityModel(), WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset, Id != 0 || this == Hud.GetGhostBuilding(), DemolitionTransition);
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (PropEditor.ShowWheelPositions)
		{
			TiltedProp.DebugDrawWheels(WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset);
		}
	}
}
