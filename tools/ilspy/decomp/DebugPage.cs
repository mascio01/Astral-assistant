public class DebugPage
{
	protected DebugPage Parent;

	protected DebugPage ActiveChild;

	public DebugPageState State;

	protected string Name;

	protected string TitleString;

	protected DebugPage(string name)
	{
		TitleString = (Name = name);
	}

	public void SetState(DebugPageState state, DebugPage child)
	{
		if (state != State || child != ActiveChild)
		{
			switch (State)
			{
			case DebugPageState.Active:
				DeactivateImpl();
				break;
			case DebugPageState.ChildActive:
				ActiveChild.SetState(DebugPageState.Inactive, null);
				ActiveChild.Parent = null;
				ActiveChild = null;
				break;
			}
			State = state;
			switch (State)
			{
			case DebugPageState.Active:
				ActivateImpl();
				break;
			case DebugPageState.ChildActive:
				ActiveChild = child;
				ActiveChild.Parent = this;
				ActiveChild.TitleString = TitleString + "/" + ActiveChild.Name;
				ActiveChild.SetState(DebugPageState.Active, null);
				break;
			}
		}
	}

	public void HandleInput(InputFrame inputFrame)
	{
		switch (State)
		{
		case DebugPageState.Active:
			HandleInputImpl(inputFrame);
			break;
		case DebugPageState.ChildActive:
			ActiveChild.HandleInput(inputFrame);
			if (ActiveChild.State == DebugPageState.Inactive)
			{
				SetState(DebugPageState.Active, null);
			}
			break;
		}
	}

	public void OnGUI()
	{
		switch (State)
		{
		case DebugPageState.Active:
			OnGUIImpl();
			break;
		case DebugPageState.ChildActive:
			ActiveChild.OnGUI();
			if (ActiveChild.State == DebugPageState.Inactive)
			{
				SetState(DebugPageState.Active, null);
			}
			break;
		}
	}

	public void OnDrawGizmos()
	{
		switch (State)
		{
		case DebugPageState.Active:
			OnDrawGizmosImpl();
			break;
		case DebugPageState.ChildActive:
			ActiveChild.OnDrawGizmos();
			break;
		}
	}

	public void OnPostRender()
	{
		switch (State)
		{
		case DebugPageState.Active:
			OnPostRenderImpl();
			break;
		case DebugPageState.ChildActive:
			ActiveChild.OnPostRender();
			break;
		}
	}

	public virtual void ActivateImpl()
	{
	}

	public virtual void DeactivateImpl()
	{
	}

	public virtual void OnGUIImpl()
	{
	}

	public virtual void OnDrawGizmosImpl()
	{
	}

	public virtual void OnPostRenderImpl()
	{
	}

	public virtual void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			ClosePage();
		}
	}

	public void OpenChildPage(DebugPage page)
	{
		SetState(DebugPageState.ChildActive, page);
	}

	public void ClosePage()
	{
		SetState(DebugPageState.Inactive, null);
	}
}
