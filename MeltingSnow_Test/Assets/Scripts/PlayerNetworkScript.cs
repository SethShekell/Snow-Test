using UnityEngine;
using Unity.Netcode;
using Unity.Collections;


public class PlayerNetwork : NetworkBehaviour
{
    // Prefab for bullet to spawn and shoot
    public GameObject spawnedObjectPrefab;
    public Transform spawnedBulletPos;


    public float moveSpeed = 15f;
    public float rotStep = 0.5f; // in degrees
    public float fireSpeed = 10f;


    private Vector3 rotVector = new Vector3(0, 0, 0);


    ////////////////////////////////////////////
    // Network Variable Samples
    ////////////////////////////////////////////


    // NOTE: We can ONLY use Value types (structs, int, floats, etc.) NOT Classes including String. \
    //       We have to use FixedString instead.
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData { _int = 56, _bool = true, }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Must update Write Permission to Owner for each client to modify value


    // Since we are using a custom struct datatype, we have to implement INetorkSerializable and the 
    // NetworkSerialize<>() method to specify how to serialize each component of the struct on the network buffer.
    //
    // I THINK - We would use the to maintain other state values for a Player to share across all copies, 
    // such as hitpoints, power up statues, etc.
    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;  // This requires   using Unity.Collections;
                                             // FixedString<num> is deprecated.  Use FixedString<num>Bytes instead 


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }


    // Set up eventhandler to subscribe to OnValueChanged event so we don't need to poll this value in every Update() call
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + ";  " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }
    ////////////////////////////////////////////
    void Update()
    {
        if (!IsOwner) return;  // Only check for object movement for the actual owner player instance


        // Network Variable Samples
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = new MyCustomData
            {
                _int = 10,
                _bool = false,
                message = "All your base are belong to us!"
            };
        }


        ///////////////////////////////////////////////////////
        // Handle Movement
        ///////////////////////////////////////////////////////
        Vector3 moveDir = gameObject.transform.forward; // new Vector3(0, 0, 0);


        // Clear Velocity so it ONLY moves when key is down
        gameObject.GetComponent<Rigidbody>().velocity = moveDir * 0;




        // Check for Movement commands
        if (Input.GetKey(KeyCode.W))
        {
            gameObject.GetComponent<Rigidbody>().velocity = moveDir * moveSpeed;
        }


        if (Input.GetKey(KeyCode.S))
        {
            gameObject.GetComponent<Rigidbody>().velocity = -moveDir * moveSpeed;
        }


        if (Input.GetKey(KeyCode.A))
        {
            rotVector.y -= rotStep;
            gameObject.transform.eulerAngles = rotVector;
        }


        if (Input.GetKey(KeyCode.D))
        {
            rotVector.y += rotStep;
            gameObject.transform.eulerAngles = rotVector;
        }




        ///////////////////////////////////////////////////////
        // Handle Firing
        ///////////////////////////////////////////////////////


        // Check for Fire Bullet Command 
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (IsServer)
            {
                ExecuteShoot();  // Fire immediately if this client is the server or host
            }
            else
            {
                RequestFireServerRpc();  // Send off the request for the server to fire the projectile
            }
        }
    }




    ///////////////////////////////////////////////////////////
    // Chain of methods to allow owning client to request
    // server to pass that method call to all clients
    ///////////////////////////////////////////////////////////


    // Server RPC  --  Projectile must be fired from the server in order to behave correctly
    [ServerRpc]
    private void RequestFireServerRpc()
    {
        ExecuteShoot();
    }


    // Fire and spawn projectile
    private void ExecuteShoot()
    {
        Vector3 dir = transform.forward;
        Vector3 firePos = spawnedBulletPos.transform.position;  // We use the SpawnedBulletPos GO to allow us to place 
                                                                // the firing point visibly with the Player prefab


        var projectile = Instantiate(spawnedObjectPrefab, firePos, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(); // This gives the server ownership over the projectile,
                                                          // allowing it to run the physics simulation
        projectile.GetComponent<Rigidbody>().velocity = (dir * moveSpeed);
    }


}
