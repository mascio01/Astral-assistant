public interface ITrap
{
	void TriggerTrap(Character character);

	void ResetTrap(Character character, bool isGathering);

	bool CanTriggerTrap(Character character);

	bool CanResetTrap();

	EquipmentPrototype GetEquipmentNeededForReset();
}
