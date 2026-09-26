using UnityEngine;

public class IconGenerator
{
	public static IconGenerator Instance;

	public Texture2D[] ObjectTypeIconPool = new Texture2D[244];

	public TexturePool PropIconPool;

	public TexturePool FaceIconPool;

	public TexturePool FullIconPool;

	public TexturePool ClothingIconPool;

	public IconGenerationQueue Queue = new IconGenerationQueue();

	public void Init()
	{
		Instance = this;
		PropIconPool = new TexturePool(256, 4);
		FaceIconPool = new TexturePool(256, 64);
		FullIconPool = new TexturePool(512, 4);
		ClothingIconPool = new TexturePool(256, 128);
	}

	public void Unload()
	{
		if (PropIconPool != null)
		{
			PropIconPool.Release();
		}
		if (FaceIconPool != null)
		{
			FaceIconPool.Release();
		}
		if (FullIconPool != null)
		{
			FullIconPool.Release();
		}
		if (ClothingIconPool != null)
		{
			ClothingIconPool.Release();
		}
	}

	public void OnFinishedSession()
	{
		PropIconPool.Reset();
		FaceIconPool.Reset();
		FullIconPool.Reset();
		ClothingIconPool.Reset();
		Queue.Clear();
	}

	public Texture2D GetIconForObjectType(BaseObjectType type, out Material mat, bool highlighted)
	{
		Texture2D iconResource = BaseObjectManager.PrototypeGameObjects[(int)type].GetIconResource();
		if (iconResource != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			return iconResource;
		}
		iconResource = ObjectTypeIconPool[(int)type];
		if (iconResource == null)
		{
			Queue.Push(IconGenerationRequest.ObjectType(type));
		}
		mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		return iconResource;
	}

	public Texture2D GetIconForObjectType(PropPrototype proto, out Material mat, bool highlighted)
	{
		if (proto == null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			return null;
		}
		Texture2D iconResource = proto.GetIconResource();
		if (iconResource != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			return iconResource;
		}
		if (iconResource == null)
		{
			Queue.Push(IconGenerationRequest.PropPrototype(proto));
		}
		mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		return iconResource;
	}

	public void GenerateIcons()
	{
		Character.DrawIcon();
		if (Queue.Pop())
		{
			switch (Queue.Processing.Type)
			{
			case IconGenerationRequestType.Face:
				((Character)Queue.Processing.Object).TryStartGeneratingIcon(IconType.Face, PortraitPose.None);
				break;
			case IconGenerationRequestType.Full:
				((Character)Queue.Processing.Object).TryStartGeneratingIcon(IconType.Full, PortraitPose.None);
				break;
			case IconGenerationRequestType.Clothing:
				Character.TryStartGeneratingClothingIcon((Equipment)Queue.Processing.Object);
				break;
			case IconGenerationRequestType.Prop:
				((Prop)Queue.Processing.Object).DrawForPropIcon();
				break;
			case IconGenerationRequestType.PropPrototype:
				GeneratePrototypeIcon(BaseObjectType.Invalid, (PropPrototype)Queue.Processing.Object);
				break;
			case IconGenerationRequestType.BaseObjectType:
				GeneratePrototypeIcon(Queue.Processing.BaseObjectType, null);
				break;
			}
		}
	}

	private void GeneratePrototypeIcon(BaseObjectType baseObjectType, PropPrototype propPrototype)
	{
		GameImpl instance = GameImpl.Instance;
		TileObject tileObject = null;
		tileObject = ((baseObjectType == BaseObjectType.Invalid) ? propPrototype.ProtoInstance : (BaseObjectManager.PrototypeGameObjects[(int)baseObjectType] as TileObject));
		if (tileObject == null)
		{
			Queue.FinishedProcessing();
			return;
		}
		if (tileObject.GetUnityObject() == null)
		{
			if (tileObject is BaseFence baseFence)
			{
				baseFence._modelType = FenceModelType.Fence_N_S;
			}
			if (tileObject is Gate gate)
			{
				gate.Close(null);
			}
			if (tileObject is PlantableCrop plantableCrop)
			{
				plantableCrop.GrowthStage = plantableCrop.GetMaxStage();
			}
			if (tileObject is Rabbit rabbit)
			{
				rabbit.Appearance = new RabbitAppearance();
			}
			if (tileObject is SingleTileProp singleTileProp)
			{
				singleTileProp.MarkCachedWorldTransDirty();
			}
			tileObject.UnityInit();
			if (!tileObject.IsUnityObjectActive())
			{
				tileObject.UnityActivate();
			}
			tileObject.UnityUpdate();
			GameObject unityObject = tileObject.GetUnityObject();
			if (unityObject != null && tileObject.GetUnityModel() != null)
			{
				Prop.ReplaceLayerRecursively(unityObject, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
				unityObject.transform.position = Sun.IconGenerationPos;
				unityObject.transform.rotation = tileObject.GetUnityModel().LocalToWorldMatrix.rotation;
			}
			else
			{
				Queue.FinishedProcessing();
			}
			return;
		}
		instance.Sun.SetupLightForTime(0.5f, useMiddayReflectionTex: true);
		Camera component = instance.UnityIconCameraObj.GetComponent<Camera>();
		RenderTexture renderTexture = (component.targetTexture = PropIconPool.RenderTex);
		component.gameObject.SetActive(value: true);
		RenderTexture.active = renderTexture;
		tileObject.SetupPropIconCam(component);
		component.Render();
		Texture2D texture2D = null;
		if (baseObjectType != BaseObjectType.Invalid)
		{
			if (ObjectTypeIconPool[(int)tileObject.GetBaseObjectType()] == null)
			{
				ObjectTypeIconPool[(int)tileObject.GetBaseObjectType()] = new Texture2D(renderTexture.width, renderTexture.height);
			}
			texture2D = ObjectTypeIconPool[(int)tileObject.GetBaseObjectType()];
		}
		else
		{
			if (propPrototype.Tex == null)
			{
				propPrototype.Tex = new Resource<Texture2D>(new Texture2D(renderTexture.width, renderTexture.height));
			}
			texture2D = propPrototype.Tex;
		}
		if (texture2D != null)
		{
			texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply();
		}
		texture2D.wrapMode = TextureWrapMode.Clamp;
		RenderTexture.active = null;
		component.gameObject.SetActive(value: false);
		component.targetTexture = null;
		if (Session.Instance != null)
		{
			instance.Sun.SetupLightForTime(Session.Instance.DaysSinceStart, useMiddayReflectionTex: false);
		}
		if (tileObject.GetUnityObject() != null)
		{
			Prop.ReplaceLayerRecursively(tileObject.GetUnityObject(), Character.IconGimpLayer, Character.DefaultLayer, skipParticleEffects: false);
		}
		tileObject.UnityDeactivate();
		tileObject.UnityDelete();
		Queue.FinishedProcessing();
		EditPropMenu.Instance.WantRepopulate = true;
	}
}
