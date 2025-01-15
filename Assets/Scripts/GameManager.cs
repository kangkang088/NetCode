using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinCount = 4;

    private void Start()
    {
        //CreatePlayer();
        //CreateCoin();

        NetworkManager.Singleton.OnClientConnectedCallback += id => { Debug.Log("A new client connected,id = " + id); };

        NetworkManager.Singleton.OnClientDisconnectCallback += id => { Debug.Log("A new client disconnected,id = " + id); };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("Server started");
            CreateCoin();
        };
    }

    // private void CreatePlayer()
    // {
    //     Instantiate(playerPrefab, new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(-4.5f, 4.5f)), Quaternion.identity);
    // }

    private void CreateCoin()
    {
        for (var i = 0; i < coinCount; i++)
        {
            var obj = Instantiate(coinPrefab, new Vector3(Random.Range(-9.5f, 9.5f), 5f, Random.Range(-9.5f, 9.5f)), Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void OnStartServerButtonClicked()
    {
        Debug.Log(NetworkManager.Singleton.StartServer() ? "Start Server Success" : "Start Server Failed");
    }

    public void OnStartClientButtonClicked()
    {
        Debug.Log(NetworkManager.Singleton.StartClient() ? "Start Client Success" : "Start Client Failed");
    }

    public void OnStartHostButtonClicked()
    {
        Debug.Log(NetworkManager.Singleton.StartHost() ? "Start Host Success" : "Start Host Failed");
    }

    public void OnShutdownButtonClicked()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("Shut Down Success");
    }
}
