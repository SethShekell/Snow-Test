using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;


public class NetworkRelay : MonoBehaviour
{
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private TMP_InputField _joinInput;
    [SerializeField] private Button _createGameSvrButton;
    [SerializeField] private Button _createGameHostButton;
    [SerializeField] private Button _joinGameButton;


    private UnityTransport _transport;
    private const int MaxPlayers = 5;


    private async void Awake()
    {
        // We programmatically attach event handlers to the buttons
        _createGameHostButton.onClick.AddListener(() => { CreateGameHost(); });
        _createGameSvrButton.onClick.AddListener(() => { CreateGameServer(); });
        _joinGameButton.onClick.AddListener(() => { JoinGame(); });


        // Programatically find the transport
        _transport = FindObjectOfType<UnityTransport>();


        //_buttons.SetActive(false);
        await Authenticate();
        //_buttons.SetActive(true);
    }


    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    public async void CreateGameServer()
    {
        if (_transport == null) { Debug.Log("Unity Transport object is null."); }  // Check for invalid transport object


        // Disable buttons but one chosen
        //_createGameSvrButton.gameObject.SetActive(false);
        _createGameHostButton.gameObject.SetActive(false);
        _joinGameButton.gameObject.SetActive(false);


        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        _joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        NetworkManager.Singleton.StartServer();
    }


    public async void CreateGameHost()
    {
        if (_transport == null) { Debug.Log("Unity Transport object is null."); }  // Check for invalid transport object


        _createGameSvrButton.gameObject.SetActive(false);
        //_createGameHostButton.gameObject.SetActive(false);
        _joinGameButton.gameObject.SetActive(false);


        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        _joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        NetworkManager.Singleton.StartHost();
    }


    public async void JoinGame()
    {
        _createGameSvrButton.gameObject.SetActive(false);
        _createGameHostButton.gameObject.SetActive(false);
        //_joinGameButton.gameObject.SetActive(false);


        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(_joinInput.text);
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        NetworkManager.Singleton.StartClient();
    }
}
