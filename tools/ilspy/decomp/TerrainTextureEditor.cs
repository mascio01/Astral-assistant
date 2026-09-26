public class TerrainTextureEditor : DebugMenu
{
	private TerrainTex CurrentTex;

	private float Radius = 5f;

	private float Amount = 1f;

	private float Limit = 1f;

	private void SetRadius(float radius)
	{
		Radius = radius;
	}

	private void SetAmount(float amount)
	{
		Amount = amount;
	}

	private void SetLimit(float limit)
	{
		Limit = limit;
	}

	private float GetRadius()
	{
		return Radius;
	}

	private float GetAmount()
	{
		return Amount;
	}

	private float GetLimit()
	{
		return Limit;
	}

	public TerrainTextureEditor()
		: base(GameImpl.Translate("DEBUG_TextureEditor"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 10f, GetRadius, SetRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), -10f, 10f, GetAmount, SetAmount));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Limit"), 0f, 1f, GetLimit, SetLimit));
		for (int i = 0; i < 8; i++)
		{
			TerrainTex localTex = (TerrainTex)i;
			if (GameTerrain.GetTerrainTypeForTex(localTex) != TerrainType.Rock)
			{
				Items.Add(new DebugMenuItemToggle(GameTerrain.TerrainTexNames[i], () => CurrentTex == localTex, delegate
				{
					CurrentTex = localTex;
				}, GameTerrain.Instance.UnityTerrain.terrainData.terrainLayers[i].diffuseTexture));
			}
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
				instance2.SetTerrainTexture(CurrentTex, MathUtil.ToXZ(raycastResult.GetHitPosition()), Radius, Amount * GameImpl.UnscaledDeltaTime, Limit, apply: true);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}
