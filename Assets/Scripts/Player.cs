using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float turnSpeed = 100.0f;

    //private NetworkVariable<Vector3> networkPlayerPos = new(Vector3.zero);
    //private NetworkVariable<Quaternion> networkPlayerRot = new(Quaternion.identity);
    private NetworkVariable<int> clientId = new();
    //private NetworkVariable<bool> networkPlayerWalkState = new(false);

    private Color[] playerColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white, Color.cyan };

    private NetworkAnimator animator;
    private bool lastWalkState = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<NetworkAnimator>();
        if (IsClient && IsOwner)
            transform.position = new Vector3(Random.Range(-4.5f, 4.5f), 0.0f, Random.Range(-4.5f, 4.5f));

        nameLabel.text = clientId.Value.ToString();

        var renderer = GetComponent<Renderer>();
        renderer.material.color = playerColors[clientId.Value % playerColors.Length];
    }

    public override void OnNetworkSpawn() //先于Start
    {
        if (IsServer)
        {
            clientId.Value = (int)OwnerClientId;
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxis("Horizontal");

            var pos = GetTargetPos(v);
            var rot = GetTargetRot(h);

            //UpdatePosAndRotServerRpc(pos, rot);

            var walkState = IsTargetWalk(v, h);
            if (lastWalkState != walkState)
            {
                AniWalk(walkState);
                lastWalkState = walkState;
                //UpdateWalkStateServerRpc(walkState);;
            }

            Move(pos);
            Turn(rot);
        }
        else
        {
            //Move(networkPlayerPos.Value);
            //Turn(networkPlayerRot.Value);

            // if (lastWalkState != networkPlayerWalkState.Value)
            // {
            //     AniWalk(networkPlayerWalkState.Value);
            //     lastWalkState = networkPlayerWalkState.Value;
            // }
        }
    }

    // [ServerRpc]
    // private void UpdateWalkStateServerRpc(bool walkState)
    // {
    //     networkPlayerWalkState.Value = walkState;
    // }

    private void AniWalk(bool walkState)
    {
        animator.SetTrigger("IsWalk");
    }

    private bool IsTargetWalk(float v, float h)
    {
        return v != 0;
    }

    // [ServerRpc] //让客户端可以直接调用服务端的函数。NetworkVariable变量的值不能在客户端修改，只能在服务端修改
    // private void UpdatePosAndRotServerRpc(Vector3 pos, Quaternion rot)
    // {
    //     networkPlayerPos.Value = pos;
    //     networkPlayerRot.Value = rot;
    // }

    private void Turn(Quaternion rot)
    {
        rb.MoveRotation(rot);
    }

    private Quaternion GetTargetRot(float h)
    {
        var delta = Quaternion.Euler(0f, h * turnSpeed * Time.deltaTime, 0f);
        var rot = transform.rotation * delta;
        return rot;
    }

    private void Move(Vector3 pos)
    {
        rb.MovePosition(pos);
    }

    private Vector3 GetTargetPos(float v)
    {
        var delta = transform.forward * (v * moveSpeed * Time.deltaTime);
        var pos = rb.position + delta;
        return pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            if (IsOwner)
            {
                var cc = other.gameObject.GetComponent<Coin>();
                cc.SetActive(false);
            }
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            if (IsClient && IsOwner)
            {
                var clientId = other.GetComponent<NetworkObject>().OwnerClientId;
                UpdatePlayerMeetServerRpc(OwnerClientId, clientId);
            }
        }
    }

    [ServerRpc]
    private void UpdatePlayerMeetServerRpc(ulong from, ulong to)
    {
        var p = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { to }
            }
        };

        NotifyPlayerMeetClientRpc(from, p);
    }

    [ClientRpc]
    private void NotifyPlayerMeetClientRpc(ulong from, ClientRpcParams p)
    {
        if (!IsOwner)
        {
            Debug.Log("Meet by player: " + from);
        }
    }
}
