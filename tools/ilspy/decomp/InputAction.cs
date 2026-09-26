using System.Collections.Generic;
using UnityEngine;

public struct InputAction : IReflectable
{
	public InputActionType Type;

	public bool IsDoubleClick;

	public bool Value;

	public byte ByteAmount;

	public int IntAmount;

	public float FloatAmount;

	public float Angle;

	public Vector2 PosXZ;

	public Vector2 Dir;

	public Vector3 Pos;

	public Vector3 RelativeVelocity;

	public Half3 HalfPos;

	public Quaternion Rot;

	public TerrainCoord Tile;

	public TerrainRect Rect;

	public int ObjectId;

	public int FromId;

	public int ToId;

	public int ReplyToReferringToId;

	public BaseObjectType ObjectType;

	public EquipmentPrototype Prototype;

	public PropPrototype PropPrototype;

	public LiquidPrototype Liquid;

	public Recipe Recipe;

	public TargettableBodyLocation TargettableBodyLocation;

	public ActionAnim ActionAnim;

	public Prop.OrientationType OrientationType;

	public InfectionType InfectionType;

	public PlaySpeed PlaySpeed;

	public SortBy SortBy;

	public SortOrder SortOrder;

	public SortCharactersBy SortCharactersBy;

	public GatePolicy GatePolicy;

	public MineralType MineralType;

	public InteractionType InteractionType;

	public Role Role;

	public Speech Speech;

	public Speech ReplyTo;

	public string ChatSpeech;

	public CharacterCreationSettings CharacterCreationSettings;

	public HintType HintType;

	public Hint Hint;

	public MapMarkerType MapMarkerType;

	public int CharacterPolicy;

	public float CharacterTargetAmount;

	public int CommunityPolicy;

	public float CommunityTargetAmount;

	public List<int> CharacterIds;

	public List<PendingTrade> PendingTrades;

	public MemoryParam SpeechParam;

	public MemoryParam ReplyToParam;

	public InputAction(InputActionType type)
	{
		Type = type;
		IsDoubleClick = false;
		Value = false;
		ByteAmount = 0;
		IntAmount = 0;
		FloatAmount = 0f;
		Angle = 0f;
		PosXZ = Vector2.zero;
		Dir = Vector2.zero;
		Pos = Vector3.zero;
		HalfPos = Half3.zero;
		RelativeVelocity = Vector3.zero;
		Rot = Quaternion.identity;
		Tile = TerrainCoord.Invalid;
		Rect = TerrainRect.Invalid;
		ObjectId = 0;
		FromId = 0;
		ToId = 0;
		ReplyToReferringToId = 0;
		ObjectType = BaseObjectType.Invalid;
		Prototype = null;
		PropPrototype = null;
		Role = Role.None;
		Liquid = null;
		Recipe = null;
		TargettableBodyLocation = TargettableBodyLocation.Torso;
		ActionAnim = ActionAnim.None;
		OrientationType = Prop.OrientationType.Deg0;
		InfectionType = InfectionType.None;
		PlaySpeed = PlaySpeed.Normal;
		SortBy = SortBy.Time;
		SortCharactersBy = SortCharactersBy.TimeJoined;
		SortOrder = SortOrder.Ascending;
		GatePolicy = GatePolicy.OpenableByFriendsExceptWhenInsideAndUnderAttack;
		MineralType = MineralType.None;
		InteractionType = InteractionType.Invalid;
		Speech = null;
		ReplyTo = null;
		ChatSpeech = null;
		CharacterCreationSettings = null;
		HintType = HintType.Invalid;
		Hint = default(Hint);
		MapMarkerType = MapMarkerType.Red;
		CharacterPolicy = 0;
		CommunityPolicy = 0;
		CharacterTargetAmount = 0f;
		CommunityTargetAmount = 0f;
		CharacterIds = null;
		PendingTrades = null;
		SpeechParam = default(MemoryParam);
		ReplyToParam = default(MemoryParam);
	}

	public void SetObject(BaseObject obj)
	{
		ObjectId = obj?.Id ?? 0;
	}

	public void SetFrom(BaseObject obj)
	{
		FromId = obj?.Id ?? 0;
	}

	public void SetTo(BaseObject obj)
	{
		ToId = obj?.Id ?? 0;
	}

	public void SetReplyToReferringTo(BaseObject obj)
	{
		ReplyToReferringToId = obj?.Id ?? 0;
	}

	public BaseObject GetObject(Session session)
	{
		if (ObjectId == 0)
		{
			return null;
		}
		return session.BaseObjectManager.FindBaseObjectByID(ObjectId);
	}

	public BaseObject GetFrom(Session session)
	{
		if (FromId == 0)
		{
			return null;
		}
		return session.BaseObjectManager.FindBaseObjectByID(FromId);
	}

	public BaseObject GetTo(Session session)
	{
		if (ToId == 0)
		{
			return null;
		}
		return session.BaseObjectManager.FindBaseObjectByID(ToId);
	}

	public BaseObject GetReplyToReferringTo(Session session)
	{
		if (ReplyToReferringToId == 0)
		{
			return null;
		}
		return session.BaseObjectManager.FindBaseObjectByID(ReplyToReferringToId);
	}

	public static InputAction Move(Vector2 moveDir)
	{
		InputAction result = new InputAction(InputActionType.Move);
		result.Dir = moveDir;
		return result;
	}

	public static InputAction Vault(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Vault);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction PickUp(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.PickUp);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Drop()
	{
		return new InputAction(InputActionType.Drop);
	}

	public static InputAction Grab(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Grab);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Bury(Character body, Prop grave, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Bury);
		result.SetObject(body);
		result.SetTo(grave);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Eulogy(Grave grave, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Eulogy);
		result.SetObject(grave);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction VisitGrave(Grave grave, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.VisitGrave);
		result.SetObject(grave);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Parry(Character attacker, ActionAnim actionAnim)
	{
		InputAction result = new InputAction(InputActionType.Parry);
		result.SetObject(attacker);
		result.ActionAnim = actionAnim;
		return result;
	}

	public static InputAction EscapeHeld(int frames)
	{
		InputAction result = new InputAction(InputActionType.EscapeHeld);
		result.IntAmount = frames;
		return result;
	}

	public static InputAction ChokeHeld(int frames)
	{
		InputAction result = new InputAction(InputActionType.ChokeHeld);
		result.IntAmount = frames;
		return result;
	}

	public static InputAction SetControlledCharacter(Character character)
	{
		InputAction result = new InputAction(InputActionType.SetControlledCharacter);
		result.SetObject(character);
		return result;
	}

	public static InputAction SetDesiredWeapon(Equipment weapon, EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		InputAction result = new InputAction(InputActionType.SetDesiredWeapon);
		result.SetObject(weapon);
		result.Prototype = ammoType;
		result.InfectionType = infectedWith;
		return result;
	}

	public static InputAction SetTargetObject(TileObject obj, TargettableBodyLocation targettableBodyLocation)
	{
		InputAction result = new InputAction(InputActionType.SetTargetObject);
		result.SetObject(obj);
		result.TargettableBodyLocation = targettableBodyLocation;
		return result;
	}

	public static InputAction SetTargetPos(Vector3 pos)
	{
		InputAction result = new InputAction(InputActionType.SetTargetPos);
		result.HalfPos = new Half3(pos);
		return result;
	}

	public static InputAction SetThrowAngle(float throwAngle, float throwSpeed)
	{
		InputAction result = new InputAction(InputActionType.SetThrowAngle);
		result.Angle = throwAngle;
		result.FloatAmount = throwSpeed;
		return result;
	}

	public static InputAction SetTargetBodyLocationToAimFor(TargettableBodyLocation location)
	{
		InputAction result = new InputAction(InputActionType.SetTargetBodyLocationToAimFor);
		result.TargettableBodyLocation = location;
		return result;
	}

	public static InputAction SetSyncedCamPosXZ(Vector2 posXZ)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedCamPosXZ);
		result.PosXZ = posXZ;
		return result;
	}

	public static InputAction SetSyncedCamAngle(float angle)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedCamAngle);
		result.Angle = angle;
		return result;
	}

	public static InputAction SetSyncedCamZoom(float dist)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedCamZoom);
		result.FloatAmount = dist;
		return result;
	}

	public static InputAction SetSyncedPipObj(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedPipObj);
		result.SetObject(obj);
		return result;
	}

	public static InputAction SetSyncedIsNavigatingMenus(bool value)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedIsNavigatingMenus);
		result.Value = value;
		return result;
	}

	public static InputAction SetSyncedInInfoScreen(bool value)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedInInfoScreen);
		result.Value = value;
		return result;
	}

	public static InputAction SetPlaySpeed(PlaySpeed playSpeed)
	{
		InputAction result = new InputAction(InputActionType.SetPlaySpeed);
		result.PlaySpeed = playSpeed;
		return result;
	}

	public static InputAction SetCanFastForwardEvenInCombat(bool val)
	{
		InputAction result = new InputAction(InputActionType.SetCanFastForwardEvenInCombat);
		result.Value = val;
		return result;
	}

	public static InputAction GoTo(TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.GoTo);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction GoToAndEnterBuilding(Building building, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.GoToAndEnterBuilding);
		result.SetObject(building);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction ChangeBuildingSlot(int slotIndex)
	{
		InputAction result = new InputAction(InputActionType.ChangeBuildingSlot);
		result.IntAmount = slotIndex;
		return result;
	}

	public static InputAction ExitBuilding(int entranceIndex, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ExitBuilding);
		result.IntAmount = entranceIndex;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction EnterBuilding(Building building, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.EnterBuilding);
		result.SetObject(building);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction FollowMe(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.FollowMe);
		result.SetObject(obj);
		return result;
	}

	public static InputAction StopFollowingMe(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.StopFollowingMe);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Attack(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Attack);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction RagdollMove(TileObject obj, Vector2 posXZ, float facingAngle)
	{
		InputAction result = new InputAction(InputActionType.RagdollMove);
		result.SetObject(obj);
		result.PosXZ = posXZ;
		result.Angle = facingAngle;
		return result;
	}

	public static InputAction RagdollStopMoving(TileObject obj, Vector2 posXZ, bool back, float facingAngle)
	{
		InputAction result = new InputAction(InputActionType.RagdollStopMoving);
		result.SetObject(obj);
		result.PosXZ = posXZ;
		result.Value = back;
		result.Angle = facingAngle;
		return result;
	}

	public static InputAction MarkInvestigated(TileObject obj, Character investigator)
	{
		InputAction result = new InputAction(InputActionType.MarkInvestigated);
		result.SetObject(obj);
		result.SetFrom(investigator);
		return result;
	}

	public static InputAction SetSyncedInventorySortBy(SortBy sortyBy)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedInventorySortBy);
		result.SortBy = sortyBy;
		return result;
	}

	public static InputAction SetSyncedInventorySortOrder(SortOrder sortyOrder)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedInventorySortOrder);
		result.SortOrder = sortyOrder;
		return result;
	}

	public static InputAction SetSyncedCharactersSortBy(SortCharactersBy sortCharactersBy)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedCharactersSortBy);
		result.SortCharactersBy = sortCharactersBy;
		return result;
	}

	public static InputAction SetSyncedCharactersSortOrder(SortOrder sortyOrder)
	{
		InputAction result = new InputAction(InputActionType.SetSyncedCharactersSortOrder);
		result.SortOrder = sortyOrder;
		return result;
	}

	public static InputAction EquipmentTransfer(Equipment equipment, TileObject from, TileObject to, int amount)
	{
		InputAction result = new InputAction(InputActionType.EquipmentTransfer);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(to);
		result.IntAmount = amount;
		return result;
	}

	public static InputAction EquipmentTrade(Equipment equipment, TileObject from, TileObject to, int amount)
	{
		InputAction result = new InputAction(InputActionType.EquipmentTrade);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(to);
		result.IntAmount = amount;
		return result;
	}

	public static InputAction EquipmentPickpocket(Equipment equipment, TileObject from, TileObject to, int amount, float detection)
	{
		InputAction result = new InputAction(InputActionType.EquipmentPickpocket);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(to);
		result.IntAmount = amount;
		result.FloatAmount = detection;
		return result;
	}

	public static InputAction ConfirmTrade(List<PendingTrade> pendingTrades, TileObject from, TileObject to, bool evenIfNotEnoughGold)
	{
		InputAction result = new InputAction(InputActionType.ConfirmTrade);
		result.SetFrom(from);
		result.SetTo(to);
		result.PendingTrades = new List<PendingTrade>();
		pendingTrades.CopyToList(result.PendingTrades);
		result.Value = evenIfNotEnoughGold;
		return result;
	}

	public static InputAction EquipmentDestroy(Equipment equipment, TileObject from, Character destroyer, int amount)
	{
		InputAction result = new InputAction(InputActionType.EquipmentDestroy);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(destroyer);
		result.IntAmount = amount;
		return result;
	}

	public static InputAction EquipmentPourAway(Equipment equipment, TileObject from, Character pourer)
	{
		InputAction result = new InputAction(InputActionType.EquipmentPourAway);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(pourer);
		return result;
	}

	public static InputAction EquipmentPourInto(Equipment from, Equipment to, Character pourer)
	{
		InputAction result = new InputAction(InputActionType.EquipmentPourInto);
		result.SetObject(pourer);
		result.SetFrom(from);
		result.SetTo(to);
		return result;
	}

	public static InputAction EquipmentUse(Equipment equipment, TileObject from, Character user)
	{
		InputAction result = new InputAction(InputActionType.EquipmentUse);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.SetTo(user);
		return result;
	}

	public static InputAction EquipmentEquip(Equipment equipment, Character from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentEquip);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentUnequip(Equipment equipment, Character from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentUnequip);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentWear(Equipment equipment, Character from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentWear);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentStrip(Equipment equipment, Character from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentStrip);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentUnloadAmmo(Equipment equipment, TileObject from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentUnloadAmmo);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentLoadAmmo(Equipment equipment, TileObject from, EquipmentPrototype ammoType)
	{
		InputAction result = new InputAction(InputActionType.EquipmentLoadAmmo);
		result.SetObject(equipment);
		result.SetFrom(from);
		result.Prototype = ammoType;
		return result;
	}

	public static InputAction EquipmentLightFuse(Equipment equipment, TileObject from)
	{
		InputAction result = new InputAction(InputActionType.EquipmentLightFuse);
		result.SetObject(equipment);
		result.SetFrom(from);
		return result;
	}

	public static InputAction EquipmentSpawn(TileObject obj, EquipmentPrototype proto)
	{
		InputAction result = new InputAction(InputActionType.EquipmentSpawn);
		result.SetObject(obj);
		result.Prototype = proto;
		return result;
	}

	public static InputAction Craft(Recipe recipe, Character crafter, Equipment item, bool recurring, int amount, TerrainCoord tile, Prop.OrientationType orientation, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Craft);
		result.Recipe = recipe;
		result.SetFrom(crafter);
		result.SetObject(item);
		result.Value = recurring;
		result.IntAmount = amount;
		result.Tile = tile;
		result.OrientationType = orientation;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction ResumeCrafting(CraftingProp craftingProp, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ResumeCrafting);
		result.SetObject(craftingProp);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Take(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Take);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction TakeAll(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.TakeAll);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Open(Gate obj)
	{
		InputAction result = new InputAction(InputActionType.Open);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Close(Gate obj)
	{
		InputAction result = new InputAction(InputActionType.Close);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Knock(Gate obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Knock);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Unlock(Gate obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Unlock);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Demolish(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Demolish);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Abandon(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Abandon);
		result.SetObject(obj);
		return result;
	}

	public static InputAction VehiclePark(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.VehiclePark);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Farm(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Farm);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Gather(Prop obj, EquipmentPrototype gatherType)
	{
		InputAction result = new InputAction(InputActionType.Gather);
		result.SetObject(obj);
		result.Prototype = gatherType;
		return result;
	}

	public static InputAction Guard(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Guard);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Repair(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Repair);
		result.SetObject(obj);
		return result;
	}

	public static InputAction TakeOver(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.TakeOver);
		result.SetObject(obj);
		return result;
	}

	public static InputAction BuildHere(Recipe buildingType, TerrainCoord tile, Prop.OrientationType orientation, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.BuildHere);
		result.Recipe = buildingType;
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		result.OrientationType = orientation;
		return result;
	}

	public static InputAction ResumeBuilding(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ResumeBuilding);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Harvest(TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Harvest);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction ChopTree(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ChopTree);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction ChopLog(TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ChopLog);
		result.SetObject(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Lumberjack(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Lumberjack);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Mine(TileObject obj, MineralType mineralType, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Mine);
		result.SetObject(obj);
		result.MineralType = mineralType;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SetMiner(TileObject obj, MineralType mineralType)
	{
		InputAction result = new InputAction(InputActionType.SetMiner);
		result.SetObject(obj);
		result.MineralType = mineralType;
		return result;
	}

	public static InputAction SetTrapper(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.SetTrapper);
		result.SetObject(obj);
		return result;
	}

	public static InputAction SetAnimalFeeder()
	{
		return new InputAction(InputActionType.SetAnimalFeeder);
	}

	public static InputAction SetOrganizer()
	{
		return new InputAction(InputActionType.SetOrganizer);
	}

	public static InputAction Cook(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.Cook);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Fill(Equipment liquidContainer, TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Fill);
		result.SetObject(liquidContainer);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Plant(Equipment seeds, TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Plant);
		result.SetObject(seeds);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SpeakTo(Character listener, BaseObject speechObject, MemoryParam speechParam, Speech speech, Speech replyTo, BaseObject replyToReferringTo, MemoryParam replyToParam, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.SpeakTo);
		result.SetTo(listener);
		result.SetObject(speechObject);
		result.SpeechParam = speechParam;
		result.Speech = speech;
		result.ReplyTo = replyTo;
		result.SetReplyToReferringTo(replyToReferringTo);
		result.ReplyToParam = replyToParam;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction GiveGift(Character recipient, Equipment item, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.GiveGift);
		result.SetTo(recipient);
		result.SetObject(item);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction TalkToInhabitant(Building building, Character talkTo)
	{
		InputAction result = new InputAction(InputActionType.TalkToInhabitant);
		result.SetObject(building);
		result.SetTo(talkTo);
		return result;
	}

	public static InputAction Chat(string chatSpeech)
	{
		InputAction result = new InputAction(InputActionType.Chat);
		result.ChatSpeech = chatSpeech;
		return result;
	}

	public static InputAction SkipConversation(Character target)
	{
		InputAction result = new InputAction(InputActionType.SkipConversation);
		result.SetObject(target);
		return result;
	}

	public static InputAction LightFire(Equipment equipment, TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.LightFire);
		result.SetObject(equipment);
		result.SetTo(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SitByFire(Campfire campfire, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.SitByFire);
		result.SetObject(campfire);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction RepairArmor(WorkBench workBench, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.RepairArmor);
		result.SetObject(workBench);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Skin(Character character, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Skin);
		result.SetObject(character);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Eat(Equipment item)
	{
		InputAction result = new InputAction(InputActionType.Eat);
		result.SetObject(item);
		return result;
	}

	public static InputAction Drink(Equipment item)
	{
		InputAction result = new InputAction(InputActionType.Drink);
		result.SetObject(item);
		return result;
	}

	public static InputAction DrinkFromRiver(TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.DrinkFromRiver);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SetRecentActivityTalkToMe(TileObject obj)
	{
		InputAction result = new InputAction(InputActionType.SetRecentActivityTalkToMe);
		result.SetObject(obj);
		return result;
	}

	public static InputAction Use(Equipment item)
	{
		InputAction result = new InputAction(InputActionType.Use);
		result.SetObject(item);
		return result;
	}

	public static InputAction AddMaterialToFire(Equipment item, TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.AddMaterialToFire);
		result.SetObject(item);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction PourInto(Equipment liquidContainer, TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.PourInto);
		result.SetObject(liquidContainer);
		result.SetTo(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction PourOnto(Equipment liquidContainer, TileObject obj, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.PourOnto);
		result.SetObject(liquidContainer);
		result.SetTo(obj);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction ChokeHold(Character target, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ChokeHold);
		result.SetObject(target);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SlitThroat(Character target, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.SlitThroat);
		result.SetObject(target);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Restrain(Character target, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Restrain);
		result.SetObject(target);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction BludgeonUnconscious(Character target, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.BludgeonUnconscious);
		result.SetObject(target);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction KillUnconscious(Character target, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.KillUnconscious);
		result.SetObject(target);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction Interact(TileObject target, InteractionType interactionType, Equipment equipment, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.Interact);
		result.SetTo(target);
		result.InteractionType = interactionType;
		result.SetObject(equipment);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SetCropsPatch(TerrainCoord tile, PropPrototype cropType)
	{
		InputAction result = new InputAction(InputActionType.SetCropsPatch);
		result.Tile = tile;
		result.PropPrototype = cropType;
		return result;
	}

	public static InputAction SetCraftItemLimit(Community community, EquipmentPrototype proto, int limit)
	{
		InputAction result = new InputAction(InputActionType.SetCraftItemLimit);
		result.SetObject(community);
		result.Prototype = proto;
		result.IntAmount = limit;
		return result;
	}

	public static InputAction SetCraftLiquidLimit(Community community, LiquidPrototype liquid, int limit)
	{
		InputAction result = new InputAction(InputActionType.SetCraftLiquidLimit);
		result.SetObject(community);
		result.Liquid = liquid;
		result.IntAmount = limit;
		return result;
	}

	public static InputAction SetCurrentQuest(QuestInstance quest)
	{
		InputAction result = new InputAction(InputActionType.SetCurrentQuest);
		result.SetObject(quest);
		return result;
	}

	public static InputAction CancelRole(Character character, Role role, EquipmentPrototype resourceType, Recipe recipe, TerrainCoord targetLocation)
	{
		InputAction result = new InputAction(InputActionType.CancelRole);
		result.SetObject(character);
		result.Role = role;
		result.Prototype = resourceType;
		result.Recipe = recipe;
		result.Tile = targetLocation;
		return result;
	}

	public static InputAction PauseRole(Character character, Role role, EquipmentPrototype resourceType, Recipe recipe, TerrainCoord targetLocation)
	{
		InputAction result = new InputAction(InputActionType.PauseRole);
		result.SetObject(character);
		result.Role = role;
		result.Prototype = resourceType;
		result.Recipe = recipe;
		result.Tile = targetLocation;
		return result;
	}

	public static InputAction ResumeRole(Character character, Role role, EquipmentPrototype resourceType, Recipe recipe, TerrainCoord targetLocation)
	{
		InputAction result = new InputAction(InputActionType.ResumeRole);
		result.SetObject(character);
		result.Role = role;
		result.Prototype = resourceType;
		result.Recipe = recipe;
		result.Tile = targetLocation;
		return result;
	}

	public static InputAction ChangeRolePriority(Character character, Role role, EquipmentPrototype resourceType, Recipe recipe, TerrainCoord targetLocation, int dir)
	{
		InputAction result = new InputAction(InputActionType.ChangeRolePriority);
		result.SetObject(character);
		result.Role = role;
		result.Prototype = resourceType;
		result.Recipe = recipe;
		result.Tile = targetLocation;
		result.IntAmount = dir;
		return result;
	}

	public static InputAction SetRoleUrgent(Character character, Role role, EquipmentPrototype resourceType, Recipe recipe, TerrainCoord targetLocation, bool urgent)
	{
		InputAction result = new InputAction(InputActionType.SetRoleUrgent);
		result.SetObject(character);
		result.Role = role;
		result.Prototype = resourceType;
		result.Recipe = recipe;
		result.Tile = targetLocation;
		result.Value = urgent;
		return result;
	}

	public static InputAction ResumeAllRoles(Character character)
	{
		InputAction result = new InputAction(InputActionType.ResumeAll);
		result.SetObject(character);
		return result;
	}

	public static InputAction AddSelectedCharacter(Character character)
	{
		InputAction result = new InputAction(InputActionType.AddSelectedCharacter);
		result.SetObject(character);
		return result;
	}

	public static InputAction RemoveSelectedCharacter(Character character)
	{
		InputAction result = new InputAction(InputActionType.RemoveSelectedCharacter);
		result.SetObject(character);
		return result;
	}

	public static InputAction SetGatePolicy(Gate gate, GatePolicy gatePolicy)
	{
		InputAction result = new InputAction(InputActionType.SetGatePolicy);
		result.SetObject(gate);
		result.GatePolicy = gatePolicy;
		return result;
	}

	public static InputAction SetAllGatesPolicy(Gate gate, GatePolicy gatePolicy)
	{
		InputAction result = new InputAction(InputActionType.SetAllGatesPolicy);
		result.SetObject(gate);
		result.GatePolicy = gatePolicy;
		return result;
	}

	public static InputAction SetBlockAnimals(Gate gate, bool on)
	{
		InputAction result = new InputAction(InputActionType.SetBlockAnimals);
		result.SetObject(gate);
		result.Value = on;
		return result;
	}

	public static InputAction SetPropName(TileObject prop, string name)
	{
		InputAction result = new InputAction(InputActionType.SetPropName);
		result.SetObject(prop);
		result.ChatSpeech = name;
		return result;
	}

	public static InputAction SetMapMarkerTile(MapMarkerType type, TerrainCoord tile)
	{
		InputAction result = new InputAction(InputActionType.SetMapMarkerTile);
		result.MapMarkerType = type;
		result.Tile = tile;
		return result;
	}

	public static InputAction ClearMapMarkerTile(MapMarkerType type, TerrainCoord tile)
	{
		InputAction result = new InputAction(InputActionType.ClearMapMarkerTile);
		result.MapMarkerType = type;
		result.Tile = tile;
		return result;
	}

	public static InputAction SetGeologicalMap(MineralType mineralType)
	{
		InputAction result = new InputAction(InputActionType.SetGeologicalMap);
		result.MineralType = mineralType;
		return result;
	}

	public static InputAction SetMapCamPos(Vector3 pos)
	{
		InputAction result = new InputAction(InputActionType.SetMapCamPos);
		result.Pos = pos;
		return result;
	}

	public static InputAction CreateCharacter(CharacterCreationSettings settings)
	{
		InputAction result = new InputAction(InputActionType.CreateCharacter);
		result.CharacterCreationSettings = settings;
		return result;
	}

	public static InputAction RigidBodyMove(TileObject obj, Vector3 pos, Quaternion rot)
	{
		InputAction result = new InputAction(InputActionType.RigidBodyMove);
		result.SetObject(obj);
		result.Pos = pos;
		result.Rot = rot;
		return result;
	}

	public static InputAction VehicleMove(TileObject obj, Vector3 pos, Quaternion rot, float wheelRPM)
	{
		InputAction result = new InputAction(InputActionType.VehicleMove);
		result.SetObject(obj);
		result.Pos = pos;
		result.Rot = rot;
		result.FloatAmount = wheelRPM;
		return result;
	}

	public static InputAction VehicleCollision(EnterableVehicle vehicle, BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint)
	{
		InputAction result = new InputAction(InputActionType.VehicleCollision);
		result.SetFrom(vehicle);
		result.SetTo(other);
		result.RelativeVelocity = relativeVelocity;
		result.Pos = contactPoint;
		return result;
	}

	public static InputAction RigidBodyStopMoving(TileObject obj, Vector3 pos, Quaternion rot)
	{
		InputAction result = new InputAction(InputActionType.RigidBodyStopMoving);
		result.SetObject(obj);
		result.Pos = pos;
		result.Rot = rot;
		return result;
	}

	public static InputAction ResetTrap(TileObject trap, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ResetTrap);
		result.SetObject(trap);
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SetHint(HintType hintType, Hint hint)
	{
		InputAction result = new InputAction(InputActionType.SetHint);
		result.HintType = hintType;
		result.Hint = hint;
		return result;
	}

	public static InputAction SetForceFullSnapshot(bool on)
	{
		InputAction result = new InputAction(InputActionType.SetForceFullSnapshot);
		result.Value = on;
		return result;
	}

	public static InputAction SetEquipmentPolicy(Character character, int characterPolicy, float characterTargetAmount, int communityPolicy, float communityTargetAmount, bool characterSameAsCommunity, EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		InputAction result = new InputAction(InputActionType.SetEquipmentPolicy);
		result.SetObject(character);
		result.CharacterPolicy = characterPolicy;
		result.CharacterTargetAmount = characterTargetAmount;
		result.CommunityPolicy = communityPolicy;
		result.CommunityTargetAmount = communityTargetAmount;
		result.Value = characterSameAsCommunity;
		result.Prototype = proto;
		result.Liquid = liquid;
		result.InfectionType = infectionType;
		return result;
	}

	public static InputAction SetMovementZone(TerrainRect zone)
	{
		InputAction result = new InputAction(InputActionType.SetMovementZone);
		result.Rect = zone;
		return result;
	}

	public static InputAction SetShortcutGroup(int group, Character controlledCharacter, List<Character> selectedCharacters)
	{
		InputAction result = new InputAction(InputActionType.SetShortcutGroup);
		result.IntAmount = group;
		result.SetObject(controlledCharacter);
		result.CharacterIds = new List<int>();
		foreach (Character selectedCharacter in selectedCharacters)
		{
			result.CharacterIds.Add(selectedCharacter.Id);
		}
		return result;
	}

	public static InputAction ScoopSnow(TerrainCoord tile, bool isDoubleClick)
	{
		InputAction result = new InputAction(InputActionType.ScoopSnow);
		result.Tile = tile;
		result.IsDoubleClick = isDoubleClick;
		return result;
	}

	public static InputAction SetStoragePolicy(Prop prop, EquipmentPrototype proto, LiquidPrototype liquid, bool on)
	{
		InputAction result = new InputAction(InputActionType.SetStoragePolicy);
		result.SetObject(prop);
		result.Prototype = proto;
		result.Liquid = liquid;
		result.Value = on;
		return result;
	}

	public static InputAction BrainScan(Character character)
	{
		InputAction result = new InputAction(InputActionType.BrainScan);
		result.SetObject(character);
		return result;
	}

	public static InputAction PowerNap(Character character)
	{
		InputAction result = new InputAction(InputActionType.PowerNap);
		result.SetObject(character);
		return result;
	}

	public static InputAction SetDesignatedLiquid(Equipment item, LiquidPrototype liquid)
	{
		InputAction result = new InputAction(InputActionType.SetDesignatedLiquid);
		result.SetObject(item);
		result.Liquid = liquid;
		return result;
	}

	public static InputAction LoadNewMap(BaseObject vehicle)
	{
		InputAction result = new InputAction(InputActionType.LoadNewMap);
		result.SetObject(vehicle);
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		switch (Type)
		{
		case InputActionType.Move:
			reflector.Add(ref Dir);
			break;
		case InputActionType.EscapeHeld:
		case InputActionType.ChokeHeld:
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.Vault:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.PickUp:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Grab:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Bury:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ToId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Eulogy:
		case InputActionType.VisitGrave:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Parry:
		case InputActionType.ResetTrap:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ActionAnim);
			break;
		case InputActionType.SetControlledCharacter:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetDesiredWeapon:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Prototype);
			reflector.Add(ref InfectionType);
			break;
		case InputActionType.SetTargetObject:
			reflector.Add(ref ObjectId);
			reflector.Add(ref TargettableBodyLocation);
			break;
		case InputActionType.SetTargetPos:
			reflector.Add(ref HalfPos);
			break;
		case InputActionType.SetThrowAngle:
			reflector.Add(ref Angle);
			reflector.Add(ref FloatAmount);
			break;
		case InputActionType.SetTargetBodyLocationToAimFor:
			reflector.Add(ref TargettableBodyLocation);
			break;
		case InputActionType.SetSyncedCamPosXZ:
			reflector.Add(ref PosXZ);
			break;
		case InputActionType.SetSyncedCamAngle:
			reflector.Add(ref Angle);
			break;
		case InputActionType.SetSyncedCamZoom:
			reflector.Add(ref FloatAmount);
			break;
		case InputActionType.SetSyncedPipObj:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetSyncedIsNavigatingMenus:
		case InputActionType.SetSyncedInInfoScreen:
			reflector.Add(ref Value);
			break;
		case InputActionType.SetPlaySpeed:
			reflector.Add(ref PlaySpeed);
			break;
		case InputActionType.SetCanFastForwardEvenInCombat:
			reflector.Add(ref Value);
			break;
		case InputActionType.GoTo:
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.GoToAndEnterBuilding:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.ChangeBuildingSlot:
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.ExitBuilding:
			reflector.Add(ref IntAmount);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.EnterBuilding:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.FollowMe:
		case InputActionType.StopFollowingMe:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.RagdollMove:
			reflector.Add(ref ObjectId);
			reflector.Add(ref PosXZ);
			reflector.Add(ref Angle);
			break;
		case InputActionType.RagdollStopMoving:
			reflector.Add(ref ObjectId);
			reflector.Add(ref PosXZ);
			reflector.Add(ref Value);
			reflector.Add(ref Angle);
			break;
		case InputActionType.MarkInvestigated:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			break;
		case InputActionType.SetSyncedInventorySortBy:
			reflector.Add(ref SortBy);
			break;
		case InputActionType.SetSyncedInventorySortOrder:
			reflector.Add(ref SortOrder);
			break;
		case InputActionType.SetSyncedCharactersSortBy:
			reflector.Add(ref SortCharactersBy);
			break;
		case InputActionType.SetSyncedCharactersSortOrder:
			reflector.Add(ref SortOrder);
			break;
		case InputActionType.EquipmentTransfer:
		case InputActionType.EquipmentTrade:
		case InputActionType.EquipmentDestroy:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.EquipmentPickpocket:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			reflector.Add(ref IntAmount);
			reflector.Add(ref FloatAmount);
			break;
		case InputActionType.ConfirmTrade:
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			reflector.Add(ref PendingTrades);
			reflector.Add(ref Value);
			break;
		case InputActionType.EquipmentEquip:
		case InputActionType.EquipmentUnequip:
		case InputActionType.EquipmentWear:
		case InputActionType.EquipmentStrip:
		case InputActionType.EquipmentUnloadAmmo:
		case InputActionType.EquipmentLightFuse:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			break;
		case InputActionType.EquipmentLoadAmmo:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			reflector.Add(ref Prototype);
			break;
		case InputActionType.EquipmentSpawn:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Prototype);
			break;
		case InputActionType.EquipmentUse:
		case InputActionType.EquipmentPourAway:
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			break;
		case InputActionType.Craft:
			reflector.Add(ref Recipe);
			reflector.Add(ref ObjectId);
			reflector.Add(ref FromId);
			reflector.Add(ref Value);
			reflector.Add(ref IntAmount);
			reflector.Add(ref Tile);
			reflector.Add(ref OrientationType);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Take:
		case InputActionType.TakeAll:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Open:
		case InputActionType.Close:
		case InputActionType.Demolish:
		case InputActionType.Abandon:
		case InputActionType.Farm:
		case InputActionType.Guard:
		case InputActionType.Repair:
		case InputActionType.TakeOver:
		case InputActionType.Lumberjack:
		case InputActionType.Cook:
		case InputActionType.SetTrapper:
		case InputActionType.VehiclePark:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetMiner:
			reflector.Add(ref ObjectId);
			reflector.Add(ref MineralType);
			break;
		case InputActionType.ChopTree:
		case InputActionType.ChopLog:
		case InputActionType.Knock:
		case InputActionType.Unlock:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Mine:
			reflector.Add(ref ObjectId);
			reflector.Add(ref MineralType);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Gather:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Prototype);
			break;
		case InputActionType.CancelRole:
		case InputActionType.PauseRole:
		case InputActionType.ResumeRole:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Role);
			reflector.Add(ref Prototype);
			reflector.Add(ref Recipe);
			reflector.Add(ref Tile);
			break;
		case InputActionType.ChangeRolePriority:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Role);
			reflector.Add(ref Prototype);
			reflector.Add(ref Recipe);
			reflector.Add(ref Tile);
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.SetRoleUrgent:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Role);
			reflector.Add(ref Prototype);
			reflector.Add(ref Recipe);
			reflector.Add(ref Tile);
			reflector.Add(ref Value);
			break;
		case InputActionType.ResumeAll:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.BuildHere:
			reflector.Add(ref Tile);
			reflector.Add(ref Recipe);
			reflector.Add(ref OrientationType);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.ResumeBuilding:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.PourInto:
		case InputActionType.PourOnto:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ToId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.AddMaterialToFire:
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.Harvest:
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Eat:
		case InputActionType.Drink:
		case InputActionType.Use:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.DrinkFromRiver:
		case InputActionType.ScoopSnow:
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Fill:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.Plant:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Tile);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SpeakTo:
			reflector.Add(ref ToId);
			reflector.Add(ref ObjectId);
			SpeechParam.Reflect(reflector);
			reflector.Add(ref Speech);
			reflector.Add(ref ReplyTo);
			reflector.Add(ref ReplyToReferringToId);
			ReplyToParam.Reflect(reflector);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.GiveGift:
			reflector.Add(ref ToId);
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SkipConversation:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.TalkToInhabitant:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ToId);
			break;
		case InputActionType.Chat:
			reflector.Add(ref ChatSpeech);
			break;
		case InputActionType.LightFire:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ToId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SitByFire:
		case InputActionType.Skin:
		case InputActionType.RepairArmor:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SetRecentActivityTalkToMe:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.ChokeHold:
		case InputActionType.SlitThroat:
		case InputActionType.BludgeonUnconscious:
		case InputActionType.KillUnconscious:
		case InputActionType.Restrain:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SetCropsPatch:
			reflector.Add(ref Tile);
			reflector.Add(ref PropPrototype);
			break;
		case InputActionType.SetCraftItemLimit:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Prototype);
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.SetCraftLiquidLimit:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Liquid);
			reflector.Add(ref IntAmount);
			break;
		case InputActionType.SetCurrentQuest:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetMapMarkerTile:
		case InputActionType.ClearMapMarkerTile:
			reflector.Add(ref Tile);
			reflector.Add(ref MapMarkerType);
			break;
		case InputActionType.SetGeologicalMap:
			reflector.Add(ref MineralType);
			break;
		case InputActionType.SetMapCamPos:
			reflector.Add(ref Pos);
			break;
		case InputActionType.Attack:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.ResumeCrafting:
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.EquipmentPourInto:
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			break;
		case InputActionType.AddSelectedCharacter:
		case InputActionType.RemoveSelectedCharacter:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetGatePolicy:
			reflector.Add(ref ObjectId);
			reflector.Add(ref GatePolicy);
			break;
		case InputActionType.SetAllGatesPolicy:
			reflector.Add(ref ObjectId);
			reflector.Add(ref GatePolicy);
			break;
		case InputActionType.SetBlockAnimals:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Value);
			break;
		case InputActionType.SetPropName:
			reflector.Add(ref ObjectId);
			reflector.Add(ref ChatSpeech);
			break;
		case InputActionType.CreateCharacter:
			if (CharacterCreationSettings == null)
			{
				CharacterCreationSettings = new CharacterCreationSettings();
			}
			reflector.Add(CharacterCreationSettings);
			break;
		case InputActionType.RigidBodyMove:
		case InputActionType.RigidBodyStopMoving:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Pos);
			reflector.Add(ref Rot);
			break;
		case InputActionType.VehicleMove:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Pos);
			reflector.Add(ref Rot);
			reflector.Add(ref FloatAmount);
			break;
		case InputActionType.VehicleCollision:
			reflector.Add(ref FromId);
			reflector.Add(ref ToId);
			reflector.Add(ref RelativeVelocity);
			reflector.Add(ref Pos);
			break;
		case InputActionType.SetHint:
			reflector.Add(ref HintType);
			Hint.Reflect(reflector);
			break;
		case InputActionType.SetForceFullSnapshot:
			reflector.Add(ref Value);
			break;
		case InputActionType.SetEquipmentPolicy:
			reflector.Add(ref ObjectId);
			reflector.Add(ref CharacterPolicy);
			reflector.Add(ref CharacterTargetAmount);
			reflector.Add(ref CommunityPolicy);
			reflector.Add(ref CommunityTargetAmount);
			reflector.Add(ref Value);
			reflector.Add(ref Prototype);
			reflector.Add(ref Liquid);
			reflector.Add(ref InfectionType);
			break;
		case InputActionType.SetMovementZone:
			Rect.Reflect(reflector);
			break;
		case InputActionType.SetShortcutGroup:
			reflector.Add(ref IntAmount);
			reflector.Add(ref ObjectId);
			reflector.AddIntList(ref CharacterIds);
			break;
		case InputActionType.Interact:
			reflector.Add(ref ToId);
			reflector.Add(ref InteractionType);
			reflector.Add(ref ObjectId);
			reflector.Add(ref IsDoubleClick);
			break;
		case InputActionType.SetStoragePolicy:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Prototype);
			reflector.Add(ref Liquid);
			reflector.Add(ref Value);
			break;
		case InputActionType.BrainScan:
		case InputActionType.PowerNap:
		case InputActionType.LoadNewMap:
			reflector.Add(ref ObjectId);
			break;
		case InputActionType.SetDesignatedLiquid:
			reflector.Add(ref ObjectId);
			reflector.Add(ref Liquid);
			break;
		case InputActionType.EnableSprint:
		case InputActionType.Fire:
		case InputActionType.Kick:
		case InputActionType.Escape:
		case InputActionType.Reload:
		case InputActionType.Crouch:
		case InputActionType.SetWantLockOnTarget:
		case InputActionType.ClearWantLockOnTarget:
		case InputActionType.EnterFlyMode:
		case InputActionType.LeaveFlyMode:
		case InputActionType.IncreasePlaySpeed:
		case InputActionType.DecreasePlaySpeed:
		case InputActionType.ScavengingFinished:
		case InputActionType.SetGatherDepot_DEPRECATED:
		case InputActionType.StopBuilding:
		case InputActionType.Drop:
		case InputActionType.Choke:
		case InputActionType.SetDirectControlled:
		case InputActionType.QuickSaveUsingToken:
		case InputActionType.ContinueGame:
		case InputActionType.DisableSprint:
		case InputActionType.PauseMovementZone:
		case InputActionType.ResumeMovementZone:
		case InputActionType.SetAnimalFeeder:
		case InputActionType.Surrender:
		case InputActionType.SetOrganizer:
		case InputActionType.EnableSprintAutomatic:
		case InputActionType.Horn:
		case InputActionType.HandBrake:
		case InputActionType.GroupFollowMe:
		case InputActionType.GroupStopFollowingMe:
		case InputActionType.ClearSelectedCharacters:
		case InputActionType.AddFollowersToSelection:
		case InputActionType.SelectFollowers:
		case InputActionType.SelectEveryone:
		case InputActionType.CopyMovementZone:
		case InputActionType.SetMedic:
			break;
		}
	}
}
