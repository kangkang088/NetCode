using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private NetworkVariable<bool> networkIsActive = new(true);

    public override void OnNetworkSpawn()
    {
        networkIsActive.OnValueChanged += (oldValue, newValue) =>
        {
            gameObject.SetActive(newValue);
        };
        gameObject.SetActive(networkIsActive.Value);
    }

    public void SetActive(bool active)
    {
        if (IsServer)
            networkIsActive.Value = active;
        else if (IsClient)
        {
            SetNetworkActiveServerRpc(active);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNetworkActiveServerRpc(bool active)
    {
        networkIsActive.Value = active;
    }
}
