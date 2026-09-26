using System;
using UnityEngine;

public class TiltedProp : Prop
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
		return BaseObjectType.TiltedProp;
	}

	public override bool IsTiltableProp()
	{
		return true;
	}

	public static float GetTerrainHeightAtWheelPos(Vector2 wheelOffset, Vector2 wheelPos, Matrix4x4 worldFromLocal, float x, float z, bool inTerrain)
	{
		Vector3 vector = worldFromLocal.MultiplyPoint(new Vector3(wheelOffset.x + wheelPos.x * x, 0f, wheelOffset.y + wheelPos.y * z));
		if (!inTerrain)
		{
			return 0f;
		}
		return GameTerrain.Instance.GetTileHeightAtPos(vector.x, vector.z, ignoreIce: true, ignoreRoadCamber: false);
	}

	public static Matrix4x4 GetWorldFromLocalWithoutTilt(TerrainCoord tile, float maxYaw, Matrix4x4 world, Vector3 modelOffset, bool inTerrain)
	{
		float angle = MathF.PI / 180f * (MathUtil.RandomFloat(tile.GetHashCode()) * 2f - 1f) * maxYaw;
		return world * MathUtil.CreateTranslation(modelOffset) * MathUtil.CreateRotationY(angle);
	}

	public static void CalcPitchAndRoll(Vector2 wheelOffset, Vector2 wheelPos, float extraFrontWheelSeparation, TerrainCoord tile, float maxYaw, Matrix4x4 world, Vector3 modelOffset, bool inTerrain, out Matrix4x4 worldFromLocal, out float pitch, out float roll, out float mid)
	{
		worldFromLocal = GetWorldFromLocalWithoutTilt(tile, maxYaw, world, modelOffset, inTerrain);
		float terrainHeightAtWheelPos = GetTerrainHeightAtWheelPos(wheelOffset, wheelPos + new Vector2(extraFrontWheelSeparation, 0f), worldFromLocal, -1f, -1f, inTerrain);
		float terrainHeightAtWheelPos2 = GetTerrainHeightAtWheelPos(wheelOffset, wheelPos + new Vector2(extraFrontWheelSeparation, 0f), worldFromLocal, 1f, -1f, inTerrain);
		float terrainHeightAtWheelPos3 = GetTerrainHeightAtWheelPos(wheelOffset, wheelPos, worldFromLocal, -1f, 1f, inTerrain);
		float terrainHeightAtWheelPos4 = GetTerrainHeightAtWheelPos(wheelOffset, wheelPos, worldFromLocal, 1f, 1f, inTerrain);
		float num = Mathf.Lerp(terrainHeightAtWheelPos, terrainHeightAtWheelPos2, 0.5f);
		float num2 = Mathf.Lerp(terrainHeightAtWheelPos3, terrainHeightAtWheelPos4, 0.5f);
		float num3 = Mathf.Lerp(terrainHeightAtWheelPos, terrainHeightAtWheelPos3, 0.5f);
		float num4 = Mathf.Lerp(terrainHeightAtWheelPos2, terrainHeightAtWheelPos4, 0.5f);
		pitch = (float)Math.Atan2(num - num2, wheelPos.y * 2f);
		roll = (float)Math.Atan2(num4 - num3, (wheelPos.x + extraFrontWheelSeparation * 0.5f) * 2f);
		mid = Math.Min(Mathf.Lerp(num2, num, 0.5f), Mathf.Lerp(num3, num4, 0.5f));
	}

	public static Matrix4x4 CalcModelTransform(PrefabResource prefab, Vector2 wheelOffset, Vector2 wheelPos, float extraFrontWheelSeparation, TerrainCoord tile, float maxYaw, Matrix4x4 world, Vector3 modelOffset, bool inTerrain, float demolitionTransition)
	{
		CalcPitchAndRoll(wheelOffset, wheelPos, extraFrontWheelSeparation, tile, maxYaw, world, modelOffset, inTerrain, out var worldFromLocal, out var pitch, out var roll, out var mid);
		float y = worldFromLocal.Translation().y;
		worldFromLocal = worldFromLocal * MathUtil.CreateTranslation(0f, mid - y + modelOffset.y - demolitionTransition, 0f) * MathUtil.CreateTranslation(MathUtil.ToX0Y(wheelOffset)) * MathUtil.CreateRotationX(pitch) * MathUtil.CreateRotationZ(roll) * MathUtil.CreateTranslation(MathUtil.ToX0Y(-wheelOffset));
		Matrix4x4 mat = prefab?.LocalToWorldMatrix ?? Matrix4x4.identity;
		MathUtil.SetTranslation(ref mat, Vector3.zero);
		return worldFromLocal * mat;
	}

	public static void DebugDrawWheels(Vector2 wheelOffset, Vector2 wheelPos, float extraFrontWheelSeparation, TerrainCoord tile, float maxYaw, Matrix4x4 world, Vector3 modelOffset)
	{
		Matrix4x4 worldFromLocalWithoutTilt = GetWorldFromLocalWithoutTilt(tile, maxYaw, world, modelOffset, inTerrain: true);
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		for (int i = -1; i <= 1; i += 2)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				float num = ((j == -1) ? extraFrontWheelSeparation : 0f);
				Vector3 pos = worldFromLocalWithoutTilt.MultiplyPoint(new Vector3(wheelOffset.x + (wheelPos.x + num) * (float)i, 0f, wheelOffset.y + wheelPos.y * (float)j));
				pos = GameTerrain.Instance.ClampPosToSurface(pos);
				DebugGraphics.DrawLine(pos, pos + Vector3.up * 3f, Color.red);
				DebugGraphics.DrawBox(pos, Vector3.one * 0.1f, Color.red);
			}
		}
		DebugGraphics.EndDrawLines();
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return CalcModelTransform(GetUnityModel(), WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset, Id != 0 || this == Hud.GetGhostBuilding(), DemolitionTransition);
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (PropEditor.ShowWheelPositions)
		{
			DebugDrawWheels(WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset);
		}
	}
}
