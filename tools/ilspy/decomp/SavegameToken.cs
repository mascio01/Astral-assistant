public class SavegameToken : Equipment
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.SavegameToken;
	}

	public override void OnUsedFromInfoScreen(PlayerRecord playerRecord, Character character, TileObject carrier)
	{
		OnUseDepletableEquipment(carrier, playerRecord.IsLocal);
		if (playerRecord.IsLocal && InfoScreen.Instance.Active)
		{
			InfoScreen.Instance.WantClose = true;
		}
		if (character.EquippedItem == this)
		{
			character.EquippedItem = (character.DesiredEquippedItem = null);
		}
		Session.Instance.WantTokenSave = true;
		Session.Instance.PlaySoundAfterTokenSave = true;
		Session.Instance.TokenSaveCameFromCarrierId = carrier.Id;
		Session.Instance.TokenSaveProto = Prototype;
	}
}
