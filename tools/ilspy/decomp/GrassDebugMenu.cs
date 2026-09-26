public class GrassDebugMenu : DebugMenu
{
	private DebugMenuItemText[] GrassText = new DebugMenuItemText[8];

	public GrassDebugMenu()
		: base(GameImpl.Translate("DEBUG_Grass"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_GrassDensity"), 0f, 4f, () => GameImpl.Instance.Settings.GrassDensity, delegate(float v)
		{
			GameImpl.Instance.SetGrassDensity(v);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_GrassRange"), 0f, 48f, () => HudBehaviour.Instance.UnityGrassCameraBehaviour.GrassRangeMax, delegate(float v)
		{
			GameImpl.Instance.SetGrassRange(v);
		}));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_GrassInstancing"), typeof(FlatGrassPatch), "EnableInstancing"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_GrassShadows"), typeof(FlatGrassPatch), "EnableShadows"));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_GrassMapViewer"), typeof(GrassMapViewer)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_HeightMapViewer"), typeof(HeightMapViewer)));
		for (int num = 0; num < 8; num++)
		{
			GrassText[num] = new DebugMenuItemText(((GrassType)num/*cast due to .constrained prefix*/).ToString(), "");
			Items.Add(GrassText[num]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		GrassRenderer mainGrassRenderer = GameImpl.Instance.MainGrassRenderer;
		for (int i = 0; i < 8; i++)
		{
			string text = "";
			for (int j = 0; j < 5; j++)
			{
				FlatGrassPatch flatGrassPatch = mainGrassRenderer.FlatGrassPatches[i, j];
				if (flatGrassPatch.DrawCount > 0)
				{
					text = text + "[" + flatGrassPatch.Density + ": " + flatGrassPatch.DrawCount + "] ";
					flatGrassPatch.DrawCount = 0;
				}
			}
			GrassText[i].Text = text;
		}
		base.HandleInputImpl(inputFrame);
	}
}
