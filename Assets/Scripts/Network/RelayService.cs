using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class RelayServiceGame : MonoBehaviour
{

    public static RelayServiceGame Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Player signed in: {AuthenticationService.Instance.PlayerId}");
        };


        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
    }



    public async Task<string> CreateRelay()
    {

        try
        {
           Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            Debug.Log($"Relay allocation created successfully. Join code: {joinCode}");

            return joinCode;

        }

        catch (RelayServiceException ex)
        {
            Debug.LogError($"Failed to create relay allocation: {ex}");
            return null;
        }


    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"Attempting to join relay allocation with join code: {joinCode}");
             JoinAllocation joinAllocation =
            await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            Debug.Log($"Successfully joined relay allocation with join code: {joinCode}");
        }

        catch (RelayServiceException ex)
        {
            Debug.LogError($"Failed to join relay allocation: {ex}");
        }
    }
}
