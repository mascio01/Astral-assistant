using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
	public static Launcher Instance;

	public CanvasScaler UnityCanvasScaler;

	private string ExceptionToHandle;

	public static string NanColliderWarningMsg = "Infinity or NaN floating point numbers appear when calculating the transform matrix for a Collider";

	public static string ScreenPositionWarningMsg = "Screen position out of view frustum";

	public static string UnsupportedConversionMsg = "Unsupported conversion of vertex data";

	public static string RHidMsg = "<RI.Hid>";

	public static string PhysXMsg = "[Physics.PhysX]";

	public static string Log = ">> ";

	public static string ErrorLoading = "Error loading";

	public static string ErrorUnloading = "Error unloading";

	public static string TheMessageHeader = "The message header is corrupted and for security reasons connection will be terminated.";

	public static string FMODFailed = "FMOD failed to initialize the output device";

	public bool WantSuppressExceptionHandler;

	public int WantSuppressDevConsole;

	public static int SuppressDevConsoleForFrames = 2;

	public void Awake()
	{
		UnityCanvasScaler = GetComponent<CanvasScaler>();
		RendererDataBehaviour.GRABPASS = new ShaderTagId("GRABPASS");
		RendererDataBehaviour.LightMode = new ShaderTagId("LightMode");
	}

	public void LogCallback(string condition, string stackTrace, LogType type)
	{
		if (!WantSuppressExceptionHandler)
		{
			if (condition.StartsWith(RHidMsg) || condition.StartsWith(PhysXMsg) || condition.StartsWith(NanColliderWarningMsg) || condition.StartsWith(ScreenPositionWarningMsg) || condition.StartsWith(UnsupportedConversionMsg) || condition.Contains(TheMessageHeader) || condition.StartsWith(FMODFailed))
			{
				WantSuppressDevConsole = SuppressDevConsoleForFrames;
			}
			else if (!condition.StartsWith(Log) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && !condition.StartsWith(ErrorLoading) && !condition.StartsWith(ErrorUnloading))
			{
				SetExceptionToHandle(type.ToString(), condition, stackTrace);
			}
		}
	}

	public void SetExceptionToHandle(string name, string condition, string stackTrace)
	{
		if (ExceptionToHandle != null)
		{
			return;
		}
		bool flag = OnlineParty.Instance != null && OnlineParty.Instance.IsInMultiplayerGame();
		ExceptionToHandle = name + " (v" + GameImpl.ReleaseVersionAsString + (flag ? " mp" : "") + "): " + condition + " " + stackTrace;
		GameImpl instance = GameImpl.Instance;
		if (instance == null || instance.CurrentStories == null)
		{
			return;
		}
		bool flag2 = true;
		foreach (Story currentStory in instance.CurrentStories)
		{
			string text = ((currentStory.StorySource == null) ? "null" : currentStory.StorySource.ToString());
			ExceptionToHandle += (flag2 ? "Mods: " : ", ");
			ExceptionToHandle += text;
			flag2 = false;
		}
	}

	private void Start()
	{
		Instance = this;
		Application.logMessageReceived += LogCallback;
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			Application.Quit();
		}
		else if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			Application.Quit();
		}
		else if (SteamAPI.RestartAppIfNecessary((AppId_t)GameImpl.AppId))
		{
			Debug.LogError("[Steamworks.NET] RestartAppIfNecessary returned false, quitting.");
			Application.Quit();
		}
		else
		{
			new GameImpl().Start(base.gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (GameImpl.Instance != null)
		{
			GameImpl.Instance.FixedUpdate();
		}
	}

	public void OnAccept(InputFrame inputFrame, string s)
	{
		GUIUtility.systemCopyBuffer = s;
	}

	private void Update()
	{
		if (GameImpl.Instance != null)
		{
			if (ExceptionToHandle != null && GameImpl.Instance.GetState() != GameState.Loading)
			{
				GameImpl.Instance.SetState(GameState.TitleMenu);
				GameImpl.Instance.SetMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("TitleMenuPanel"));
				GameImpl.Instance.ShowInputBox(OnAccept, GameImpl.Translate("MENU_Exception"), ExceptionToHandle, multiline: true, readOnly: true);
				ExceptionToHandle = null;
			}
			GameImpl.Instance.Update();
		}
	}

	private void LateUpdate()
	{
		if (GameImpl.Instance != null)
		{
			GameImpl.Instance.LateUpdate();
		}
		if (WantSuppressDevConsole > 0)
		{
			Debug.developerConsoleVisible = false;
			WantSuppressDevConsole--;
		}
	}

	private void OnGUI()
	{
		if (GameImpl.Instance != null)
		{
			GameImpl.Instance.OnGUI();
		}
	}

	private void OnDrawGizmos()
	{
		if (GameImpl.Instance != null)
		{
			GameImpl.Instance.OnDrawGizmos();
		}
	}

	private void OnApplicationQuit()
	{
		if (GameImpl.Instance != null)
		{
			Debug.Log("QUITTING NOW");
			GameImpl.Instance.Unload();
		}
	}

	private bool WantAutosaveOnLoseFocus()
	{
		Session instance = Session.Instance;
		if (instance != null && instance.State == SessionState.Started && instance.GameFinishedState != GameFinishedState.GameOver && instance.GameFinishedState != GameFinishedState.GameComplete)
		{
			return instance.DifficultySettings.SaveTokensRequired;
		}
		return false;
	}

	private void OnApplicationFocus(bool focus)
	{
		if (GameImpl.Instance != null)
		{
			AudioListener.volume = (focus ? 1f : 0f);
			if (!focus && WantAutosaveOnLoseFocus())
			{
				Session.Instance.AutoSave(SaveGameType.Current, overwriteAllSlots: false);
			}
		}
	}
}
