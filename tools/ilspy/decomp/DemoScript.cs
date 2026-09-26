using System;
using System.Runtime.InteropServices;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Party;
using UnityEngine;
using UnityEngine.UI;

public class DemoScript : MonoBehaviour
{
	public InputField networkIdTextBox;

	public Text output;

	private void Start()
	{
		PlayFabMultiplayerManager playFabMultiplayerManager = PlayFabMultiplayerManager.Get();
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
		{
			CustomId = UnityEngine.Random.value.ToString(),
			CreateAccount = true
		}, OnLoginSuccess, OnLoginFailure);
		playFabMultiplayerManager.OnNetworkJoined += OnNetworkJoined;
		playFabMultiplayerManager.OnDataMessageReceived += OnDataMessageReceived;
		playFabMultiplayerManager.OnDataMessageNoCopyReceived += OnDataMessageNoCopyReceived;
	}

	public void CreateAndJoinToNetwork()
	{
		PlayFabMultiplayerManager.Get().CreateAndJoinNetwork();
	}

	public void JoinNetwork()
	{
		PlayFabMultiplayerManager.Get().JoinNetwork(networkIdTextBox.text);
	}

	private void OnDataMessageReceived(object sender, PlayFabPlayer from, byte[] buffer)
	{
		Debug.Log("Got a message (simple).");
		output.text += "\r\n Got a message (simple).";
	}

	private void OnDataMessageNoCopyReceived(object sender, PlayFabPlayer from, IntPtr buffer, uint bufferSize)
	{
		Debug.Log("Got a message (no copy).");
		output.text += "\r\n Got a message (no copy).";
	}

	private void OnNetworkJoined(object sender, string networkId)
	{
		Debug.Log("Joined the network.");
		output.text += "\r\n Joined the network.";
		networkIdTextBox.text = networkId;
		byte[] bytes = Encoding.ASCII.GetBytes("Hello world (simple message).");
		PlayFabMultiplayerManager.Get().SendDataMessageToAllPlayers(bytes);
		byte[] bytes2 = Encoding.ASCII.GetBytes("Hello world (no garbage collection method).");
		IntPtr intPtr = Marshal.AllocHGlobal(bytes2.Length);
		Marshal.Copy(bytes2, 0, intPtr, bytes2.Length);
		PlayFabMultiplayerManager.Get().SendDataMessage(intPtr, (uint)bytes2.Length, PlayFabMultiplayerManager.Get().RemotePlayers, DeliveryOption.BestEffort);
		Marshal.FreeHGlobal(intPtr);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		Debug.Log("Logged into PlayFab.");
		output.text += "\r\n Logged into PlayFab.";
	}

	private void OnLoginFailure(PlayFabError error)
	{
		Debug.Log("Error logging into PlayFab: " + error.ErrorMessage);
		Text text = output;
		text.text = text.text + "\r\n Error logging into PlayFab: " + error.ErrorMessage;
	}

	private void Update()
	{
	}
}
