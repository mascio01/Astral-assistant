using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class PortraitGeneratorMenu : BaseMenu
{
	public enum GenerateAchievementIconStage
	{
		None,
		Generating,
		Wait
	}

	public enum AchievementIconType
	{
		Steam_256x256_jpg,
		XBox_1920x1080_png,
		Count
	}

	public static PortraitGeneratorMenu Instance;

	private SavedCharacter CurrentSavedCharacter;

	public GenerateAchievementIconStage Stage;

	public Achievement CurrentAchievement = Achievement.Count;

	public AchievementIconType CurrentIconType;

	public CustomRandom Rand;

	private float WaitTimer;

	private RenderTexture[] AchievementRenderTex;

	private RenderTexture OldRenderTex;

	private Color HalftoneCol1;

	private Color HalftoneCol2;

	private float HalftoneAngle;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		Instance = this;
	}

	public override void OnActivate()
	{
		base.gameObject.FindChild("MenuLayout/AchievementIconsButton").SetActive(value: false);
		RandomizePose();
		base.OnActivate();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	private void RandomizePose()
	{
		PortraitPose = (PortraitPose)MathUtil.NonDeterministicRand.Next(1, 49);
	}

	public void OnRandomizePose()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		RandomizePose();
		StartGeneratingPortrait(CurrentSavedCharacter);
	}

	public void OnPrevPose()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		PortraitPose--;
		if (PortraitPose <= PortraitPose.None)
		{
			PortraitPose = PortraitPose.BigHelicopter;
		}
		StartGeneratingPortrait(CurrentSavedCharacter);
	}

	public void OnNextPose()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		PortraitPose++;
		if (PortraitPose >= PortraitPose.Count)
		{
			PortraitPose = PortraitPose.SpikedBatOverShoulder;
		}
		StartGeneratingPortrait(CurrentSavedCharacter);
	}

	public void OnRandomizePortrait()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		CurrentSavedCharacter = null;
		StartGeneratingPortrait();
	}

	public void OnLoadCharacter()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		OpenFileName openFileName = new OpenFileName();
		openFileName.structSize = Marshal.SizeOf(openFileName);
		openFileName.filter = "character files (*.char)\0*.char\0All files (*.*)\0*.*\0\0";
		openFileName.file = new string(new char[256]);
		openFileName.maxFile = openFileName.file.Length;
		openFileName.fileTitle = new string(new char[64]);
		openFileName.maxFileTitle = openFileName.fileTitle.Length;
		openFileName.initialDir = GameImpl.Instance.SaveGamePath + "\\Characters";
		openFileName.title = "Open Character";
		openFileName.defExt = "CHAR";
		openFileName.flags = 530440;
		if (DllTest.GetOpenFileName(openFileName))
		{
			CurrentSavedCharacter = new SavedCharacter();
			if (CurrentSavedCharacter.Load(openFileName.file))
			{
				StartGeneratingPortrait(CurrentSavedCharacter);
			}
		}
	}

	public void OnSavePortrait()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		if (!Ready)
		{
			return;
		}
		Texture2D texture2D = UnityPortrait.texture as Texture2D;
		if (texture2D != null)
		{
			GameObject unityPortraitGeneratorCanvasObj = GameImpl.Instance.UnityPortraitGeneratorCanvasObj;
			unityPortraitGeneratorCanvasObj.SetActive(value: true);
			unityPortraitGeneratorCanvasObj.FindChild("Background").SetActive(value: false);
			unityPortraitGeneratorCanvasObj.FindChild("PortraitImage").SetActive(value: true);
			unityPortraitGeneratorCanvasObj.FindChild("PortraitImage").GetComponent<RawImage>().texture = texture2D;
			GameObject unityPortraitGeneratorCameraObj = GameImpl.Instance.UnityPortraitGeneratorCameraObj;
			unityPortraitGeneratorCameraObj.SetActive(value: true);
			Camera component = unityPortraitGeneratorCameraObj.GetComponent<Camera>();
			if (component.targetTexture == null)
			{
				component.targetTexture = new RenderTexture(texture2D.width, texture2D.height, 24, RenderTextureFormat.ARGB32);
			}
			RenderTexture.active = component.targetTexture;
			component.orthographicSize = (float)texture2D.height * 0.5f;
			unityPortraitGeneratorCameraObj.transform.position = unityPortraitGeneratorCanvasObj.transform.position - Vector3.forward * 100f;
			unityPortraitGeneratorCameraObj.transform.LookAt(unityPortraitGeneratorCanvasObj.transform.position);
			component.Render();
			Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, mipChain: false);
			texture2D2.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			byte[] bytes = texture2D2.EncodeToPNG();
			string text = Application.dataPath + "/Portraits";
			Directory.CreateDirectory(text);
			string text2 = text + "/Portrait";
			int num = 0;
			while (File.Exists(text2 + num + ".png"))
			{
				num++;
			}
			text2 = text2 + num + ".png";
			File.WriteAllBytes(text2, bytes);
			RenderTexture.active = null;
			unityPortraitGeneratorCanvasObj.SetActive(value: false);
			unityPortraitGeneratorCameraObj.SetActive(value: false);
			Object.Destroy(texture2D2);
		}
	}

	public bool IsGeneratingAchievementIcons()
	{
		return CurrentAchievement < Achievement.Count;
	}

	public void OnGenerateAchievementIcons()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		CurrentAchievement = Achievement.Recruit_Refugee;
		Rand = new CustomRandom(69);
		AchievementRenderTex = new RenderTexture[2];
		AchievementRenderTex[0] = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
		AchievementRenderTex[1] = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
	}

	private Color Desaturate(Color col)
	{
		Color.RGBToHSV(col, out var H, out var S, out var V);
		if (H >= 1f / 6f && H < 1f / 3f)
		{
			H -= 1f / 6f;
		}
		if (H >= 1f / 3f && H < 0.5f)
		{
			H += 1f / 6f;
		}
		if (H >= 0.75f && H < 5f / 6f)
		{
			H -= 1f / 12f;
		}
		if (H >= 5f / 6f && H < 11f / 12f)
		{
			H += 1f / 12f;
		}
		S *= 0.375f;
		return Color.HSVToRGB(H, S, V);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (CurrentAchievement >= Achievement.Count)
		{
			return;
		}
		switch (Stage)
		{
		case GenerateAchievementIconStage.None:
		{
			if (!Ready)
			{
				break;
			}
			OldRenderTex = PortraitGallery.Instance.RenderTex;
			FogOfWarBehaviour component6 = GameImpl.Instance.UnityPortraitCameraObj.GetComponent<FogOfWarBehaviour>();
			switch (CurrentIconType)
			{
			default:
				return;
			case AchievementIconType.Steam_256x256_jpg:
				component6.EdgeWidth = 3f;
				break;
			case AchievementIconType.XBox_1920x1080_png:
				component6.EdgeWidth = 0.75f;
				break;
			}
			PortraitGallery.Instance.RenderTex = AchievementRenderTex[(int)CurrentIconType];
			if (CurrentIconType == AchievementIconType.Steam_256x256_jpg)
			{
				TreelinePose = PortraitPose.Treeline;
				string text4 = string.Empty;
				string value3 = string.Empty;
				GenderType genderType = GenderType.Count;
				bool flag = false;
				switch (CurrentAchievement)
				{
				case Achievement.Recruit_Refugee:
					PortraitPose = PortraitPose.Running;
					flag = true;
					break;
				case Achievement.Recruit_Looter:
					PortraitPose = PortraitPose.HandsUp;
					Rand.Seed = 177238351u;
					flag = true;
					break;
				case Achievement.Recruit_Settler:
					PortraitPose = PortraitPose.ChoppingWithAxe;
					Rand.Seed = 3428377406u;
					flag = true;
					break;
				case Achievement.GiftAllBooksInSet:
					PortraitPose = PortraitPose.Reading;
					Rand.Seed = 737422281u;
					flag = true;
					break;
				case Achievement.AdoptAChicken:
					PortraitPose = PortraitPose.Chicken;
					TreelinePose = PortraitPose.TreelineHigh;
					Rand.Seed = 3103157851u;
					break;
				case Achievement.BuildLargeCommunity:
					PortraitPose = PortraitPose.InWatchtower;
					Rand.Seed = 1113695744u;
					flag = true;
					break;
				case Achievement.FormAlliance:
					PortraitPose = PortraitPose.MugShotAggressive;
					Rand.Seed = 2739730028u;
					flag = true;
					break;
				case Achievement.MakeAFriend:
					PortraitPose = PortraitPose.MugShotHappy;
					Rand.Seed = 1906615911u;
					flag = true;
					break;
				case Achievement.StartRelationship:
					PortraitPose = PortraitPose.Hugging;
					genderType = GenderType.Female;
					Rand.Seed = 1092229982u;
					flag = true;
					break;
				case Achievement.PartnersFightOverYou:
					PortraitPose = PortraitPose.OneHandedAttack1CloseUp;
					Rand.Seed = 3379595135u;
					flag = true;
					break;
				case Achievement.InfectWithFood:
					PortraitPose = PortraitPose.ZombieCharging;
					Rand.Seed = 1516889111u;
					break;
				case Achievement.InfectWithWeapon:
					PortraitPose = PortraitPose.ZombieCharging2;
					Rand.Seed = 2819476409u;
					flag = true;
					break;
				case Achievement.VehicleKills:
					PortraitPose = PortraitPose.Ambulance;
					Rand.Seed = 3783231194u;
					break;
				case Achievement.FindInvisibleStrain:
					PortraitPose = PortraitPose.ZombieCharging4;
					Rand.Seed = 1518911695u;
					flag = true;
					break;
				case Achievement.Craft_Cookies:
					text4 = "Kelly Salas";
					PortraitPose = PortraitPose.Eating;
					break;
				case Achievement.Craft_Vodka:
					text4 = "Ayden Washington";
					PortraitPose = PortraitPose.Drinking;
					break;
				case Achievement.Craft_ArmorPiercingAmmo:
					PortraitPose = PortraitPose.PickaxeOverShoulderCloseUp;
					Rand.Seed = 3937252386u;
					flag = true;
					break;
				case Achievement.Story_RitzCreekWar:
					text4 = "Jeremiah Riley";
					PortraitPose = PortraitPose.MugShotAngry;
					break;
				case Achievement.Story_FindRitzvillePasses:
					text4 = "Charlie Gant";
					PortraitPose = PortraitPose.MugShotWistful;
					break;
				case Achievement.Story_FindCabinPeople:
					text4 = "Martin Steele";
					PortraitPose = PortraitPose.MugShotAggressive;
					break;
				case Achievement.Story_FindBrainScanner:
					text4 = "Cooper McClure";
					PortraitPose = PortraitPose.MugShotSuspicious;
					break;
				case Achievement.Story_DumpTruckKills:
					PortraitPose = PortraitPose.DumpTruck;
					break;
				case Achievement.Story_RecruitJoeWheeler:
					text4 = "Joe Wheeler";
					PortraitPose = PortraitPose.RifleIdleCloseUp;
					break;
				case Achievement.Story_EvacEmma:
					text4 = "Emma O'Connor";
					PortraitPose = PortraitPose.InHelicopter;
					break;
				case Achievement.CompleteSandbox_Evac:
					PortraitPose = PortraitPose.InHelicopter;
					Rand.Seed = 3013449225u;
					flag = true;
					break;
				case Achievement.CompleteSandbox_Conquest:
					PortraitPose = PortraitPose.PistolAiming;
					Rand.Seed = 2919124412u;
					flag = true;
					break;
				case Achievement.CompleteSandbox_HitTheRoad:
					PortraitPose = PortraitPose.Taxi;
					break;
				case Achievement.CompleteSandbox_Hard_WithFriends:
					PortraitPose = PortraitPose.Van;
					break;
				case Achievement.CompleteSandbox_Hard_LoneWolf:
					PortraitPose = PortraitPose.RifleIdleCloseUp;
					value3 = "Sunglasses";
					Rand.Seed = 272339619u;
					flag = true;
					break;
				case Achievement.CompleteStory_Evac:
					PortraitPose = PortraitPose.Helicopter;
					break;
				case Achievement.CompleteStory_SuicideMission:
					PortraitPose = PortraitPose.MilitaryVehicle;
					break;
				case Achievement.CompleteStory_HitTheRoad:
					PortraitPose = PortraitPose.Pickup;
					break;
				case Achievement.CompleteStory_Hard:
					text4 = "Emma O'Connor";
					PortraitPose = PortraitPose.MugShotHappy;
					break;
				default:
					PortraitPose = (PortraitPose)Rand.Next(1, 49);
					break;
				}
				Debug.Log(CurrentAchievement.ToString() + ": " + Rand.Seed);
				string text5 = Path.GetDirectoryName(Application.dataPath) + "/Art/Characters";
				if (string.IsNullOrEmpty(text4))
				{
					StartGeneratingPortrait(null, genderType);
					if (flag && PortraitSubject is Human)
					{
						new SavedCharacter((Human)PortraitSubject).Save(text5 + "/" + CurrentAchievement.ToString() + ".char", null);
					}
				}
				else
				{
					text4 = text5 + "/" + text4 + ".char";
					SavedCharacter savedCharacter = new SavedCharacter();
					if (savedCharacter.Load(text4))
					{
						StartGeneratingPortrait(savedCharacter);
					}
				}
				if (!string.IsNullOrEmpty(value3) && PortraitSubject is Character character)
				{
					Equipment equipment = Equipment.Create(GameImpl.Instance.FindEquipmentPrototypeByName(value3));
					character.Inventory.Add(character, equipment);
					equipment.Wear(character);
				}
				HalftoneCol1 = Desaturate(new Color(Rand.RandomFloat(), Rand.RandomFloat(), Rand.RandomFloat()));
				do
				{
					HalftoneCol2 = Desaturate(new Color(Rand.RandomFloat(), Rand.RandomFloat(), Rand.RandomFloat()));
				}
				while (((Vector4)HalftoneCol1 - (Vector4)HalftoneCol2).magnitude < 0.25f);
				HalftoneAngle = Mathf.Lerp(-30f, 30f, Rand.RandomFloat());
			}
			else
			{
				Ready = false;
			}
			Stage = GenerateAchievementIconStage.Generating;
			break;
		}
		case GenerateAchievementIconStage.Generating:
			if (Ready)
			{
				Stage = GenerateAchievementIconStage.Wait;
				WaitTimer = 0f;
			}
			break;
		case GenerateAchievementIconStage.Wait:
		{
			WaitTimer += GameImpl.UnscaledDeltaTime;
			if (!(WaitTimer >= 0.5f))
			{
				break;
			}
			int num;
			int num2;
			switch (CurrentIconType)
			{
			default:
				return;
			case AchievementIconType.Steam_256x256_jpg:
				num = (num2 = 1024);
				break;
			case AchievementIconType.XBox_1920x1080_png:
				num = 1920;
				num2 = 1080;
				break;
			}
			Texture2D texture = UnityPortrait.texture as Texture2D;
			Texture2D texture2 = UnityBackground.texture as Texture2D;
			GameObject unityPortraitGeneratorCanvasObj = GameImpl.Instance.UnityPortraitGeneratorCanvasObj;
			RectTransform rectTransform = (RectTransform)unityPortraitGeneratorCanvasObj.transform;
			rectTransform.sizeDelta = new Vector2(num, num2);
			unityPortraitGeneratorCanvasObj.SetActive(value: true);
			unityPortraitGeneratorCanvasObj.FindChild("Background").SetActive(value: true);
			unityPortraitGeneratorCanvasObj.FindChild("PortraitImage").SetActive(value: false);
			RawImage component = unityPortraitGeneratorCanvasObj.FindChild("Background/FrameOutline/Frame/PortraitImage").GetComponent<RawImage>();
			RawImage component2 = unityPortraitGeneratorCanvasObj.FindChild("Background/FrameOutline/Frame/Background").GetComponent<RawImage>();
			RawImage component3 = unityPortraitGeneratorCanvasObj.FindChild("Background/FrameOutline/Frame/Halftone").GetComponent<RawImage>();
			RawImage component4 = unityPortraitGeneratorCanvasObj.FindChild("Background/FrameOutline").GetComponent<RawImage>();
			component.texture = texture;
			component2.texture = texture2;
			component4.GetComponents<Outline>()[1].effectColor = HalftoneCol1;
			float num3 = (float)(num - num2) * 0.5f;
			component4.rectTransform.offsetMin = new Vector2(num3 + 64f, 64f);
			component4.rectTransform.offsetMax = new Vector2(0f - num3 - 64f, -64f);
			int num4 = num2;
			GameObject unityPortraitGeneratorCameraObj = GameImpl.Instance.UnityPortraitGeneratorCameraObj;
			unityPortraitGeneratorCameraObj.SetActive(value: true);
			Camera component5 = unityPortraitGeneratorCameraObj.GetComponent<Camera>();
			component5.targetTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32);
			RenderTexture.active = component5.targetTexture;
			component5.orthographicSize = rectTransform.rect.height * 0.5f;
			unityPortraitGeneratorCameraObj.transform.position = unityPortraitGeneratorCanvasObj.transform.position - Vector3.forward * 100f;
			unityPortraitGeneratorCameraObj.transform.LookAt(unityPortraitGeneratorCanvasObj.transform.position);
			float value = 0.02f;
			float value2 = 0.02f;
			Material material = component3.material;
			Material material2 = component.material;
			component3.material = new Material(component3.material);
			component3.material.SetFloat("_DotSize", value);
			component3.material.SetColor("_DotColor", HalftoneCol1);
			component3.material.SetColor("_Color", HalftoneCol2);
			component3.material.SetFloat("_Angle", HalftoneAngle);
			component3.uvRect = new Rect(0f, 0f, (float)num4 / (float)num2, 1f);
			component.material = new Material(component.material);
			component.material.SetFloat("_DotSize", value2);
			component5.Render();
			component3.material = material;
			component.material = material2;
			int num5 = num;
			int num6 = num2;
			if (CurrentIconType == AchievementIconType.Steam_256x256_jpg)
			{
				num5 = (num6 = 256);
			}
			RenderTexture source = component5.targetTexture;
			while (num > num5)
			{
				num /= 2;
				num2 /= 2;
				RenderTexture renderTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32);
				Graphics.Blit(source, renderTexture);
				source = renderTexture;
			}
			Texture2D texture2D = new Texture2D(num5, num6, TextureFormat.ARGB32, mipChain: false);
			texture2D.ReadPixels(new Rect(0f, 0f, num5, num6), 0, 0);
			string text = Path.GetDirectoryName(Application.dataPath) + "/Art/AchievementIcons";
			int currentAchievement = (int)CurrentAchievement;
			string text2 = currentAchievement.ToString("D2") + " - " + CurrentAchievement;
			string text3 = null;
			byte[] bytes;
			switch (CurrentIconType)
			{
			default:
				return;
			case AchievementIconType.Steam_256x256_jpg:
				text += "/Steam";
				text2 += ".jpg";
				text3 = text2 + "_Locked.jpg";
				bytes = texture2D.EncodeToJPG(100);
				break;
			case AchievementIconType.XBox_1920x1080_png:
				text += "/XBox";
				text2 += ".png";
				bytes = texture2D.EncodeToPNG();
				break;
			}
			text2 = text + "/" + text2;
			Directory.CreateDirectory(text);
			File.WriteAllBytes(text2, bytes);
			if (!string.IsNullOrEmpty(text3))
			{
				Color[] pixels = texture2D.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					Color.RGBToHSV(pixels[i], out var H, out var S, out var V);
					S = 0f;
					pixels[i] = Color.HSVToRGB(H, S, V);
				}
				texture2D.SetPixels(pixels);
				bytes = texture2D.EncodeToJPG(100);
				text3 = text + "/" + text3;
				File.WriteAllBytes(text3, bytes);
			}
			RenderTexture.active = null;
			unityPortraitGeneratorCanvasObj.SetActive(value: false);
			unityPortraitGeneratorCameraObj.SetActive(value: false);
			Stage = GenerateAchievementIconStage.None;
			CurrentIconType++;
			if (CurrentIconType < AchievementIconType.Count)
			{
				break;
			}
			CurrentIconType = AchievementIconType.Steam_256x256_jpg;
			CurrentAchievement++;
			if (CurrentAchievement >= Achievement.Count)
			{
				GameImpl.Instance.UnityPortraitCameraObj.GetComponent<FogOfWarBehaviour>().EdgeWidth = 0.75f;
				PortraitGallery.Instance.RenderTex = OldRenderTex;
				for (int j = 0; j < AchievementRenderTex.Length; j++)
				{
					AchievementRenderTex[j].Release();
					AchievementRenderTex[j] = null;
				}
			}
			break;
		}
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}
}
