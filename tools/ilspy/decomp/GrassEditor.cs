using UnityEngine;

public class GrassEditor : DebugMenu
{
	private GrassType CurrentGrassType;

	private float Radius = 5f;

	private float Amount = 10f;

	private void SetRadius(float radius)
	{
		Radius = radius;
	}

	private void SetAmount(float amount)
	{
		Amount = amount;
	}

	private float GetRadius()
	{
		return Radius;
	}

	private float GetAmount()
	{
		return Mathf.Min(Amount, GetMaxDensity());
	}

	private float GetMaxDensity()
	{
		return GrassRenderer.GetMaxDensityForGrassType(CurrentGrassType);
	}

	public GrassEditor()
		: base(GameImpl.Translate("DEBUG_GrassEditor"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 10f, GetRadius, SetRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), 0f, GetMaxDensity, GetAmount, SetAmount));
		for (int i = 0; i < 8; i++)
		{
			GrassType grassType = (GrassType)i;
			Items.Add(new DebugMenuItemToggle(GameTerrain.GrassTypeNames[i], () => CurrentGrassType == grassType, delegate
			{
				CurrentGrassType = grassType;
			}));
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (instance3.IsPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				instance2.AddOrRemoveGrass(CurrentGrassType, MathUtil.ToXZ(raycastResult.GetHitPosition()), Radius, GetAmount(), remove: false, apply: true);
				Session.Instance.AchievementsEnabled = false;
			}
		}
		if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance2.AddOrRemoveGrass(CurrentGrassType, MathUtil.ToXZ(raycastResult2.GetHitPosition()), Radius, GetAmount(), remove: true, apply: true);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}
