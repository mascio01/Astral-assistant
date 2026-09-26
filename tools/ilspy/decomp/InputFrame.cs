using System.Collections.Generic;

public class InputFrame : IReflectable
{
	public int Frame;

	public List<InputAction> Actions = new List<InputAction>();

	public static bool DisableActionMerging;

	public void ClearActions(int frame)
	{
		Frame = frame;
		Actions.Clear();
	}

	public void AddAction(InputAction action)
	{
		if (CanActionBeMerged(action.Type))
		{
			int num = FindActionOfType(action.Type);
			if (num != -1)
			{
				if (!DisableActionMerging)
				{
					switch (action.Type)
					{
					case InputActionType.Move:
						Actions[num] = InputAction.Move(Actions[num].Dir + action.Dir);
						break;
					case InputActionType.EscapeHeld:
						Actions[num] = InputAction.EscapeHeld(Actions[num].IntAmount + action.IntAmount);
						break;
					case InputActionType.ChokeHeld:
						Actions[num] = InputAction.ChokeHeld(Actions[num].IntAmount + action.IntAmount);
						break;
					}
				}
				return;
			}
		}
		Actions.Add(action);
	}

	public void CopyActions(InputFrame other)
	{
		Frame = other.Frame;
		Actions.Clear();
		for (int i = 0; i < other.Actions.Count; i++)
		{
			Actions.Add(other.Actions[i]);
		}
	}

	public InputAction? CopyActionOfType(InputActionType actionType)
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Type == actionType)
			{
				return Actions[i];
			}
		}
		return null;
	}

	public int FindActionOfType(InputActionType actionType)
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Type == actionType)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool CanActionBeMerged(InputActionType inputActionType)
	{
		if (inputActionType == InputActionType.Move || (uint)(inputActionType - 134) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsActionWhichYouMustLetGoOfToFireAgain(InputActionType inputActionType)
	{
		switch (inputActionType)
		{
		case InputActionType.Fire:
		case InputActionType.Kick:
		case InputActionType.Vault:
		case InputActionType.Parry:
		case InputActionType.Reload:
		case InputActionType.PickUp:
		case InputActionType.Drop:
		case InputActionType.Surrender:
			return true;
		default:
			return false;
		}
	}

	public bool HasActionsWhichYouMustLetGoOfToFireAgain()
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			if (IsActionWhichYouMustLetGoOfToFireAgain(Actions[i].Type))
			{
				return true;
			}
		}
		return false;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.AddSmallList(ref Actions);
	}
}
