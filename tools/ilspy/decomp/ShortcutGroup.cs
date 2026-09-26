using System.Collections.Generic;

public class ShortcutGroup : IReflectable
{
	public int Group;

	public Character ControlledCharacter;

	public List<Character> SelectedCharacters = new List<Character>();

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Group);
		reflector.Add(ref ControlledCharacter);
		reflector.AddGameObjectRefList(ref SelectedCharacters);
	}
}
