
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField IPAddressInp;
    [SerializeField] private TMP_Text currentIPAddressText;

    private void Awake()
    {
        Color selectedColor = Color.green;
        string localIPAddress = GetLocalIPAddress();
        IPAddressInp.GetComponent<TMP_InputField>().text = "";
        currentIPAddressText.GetComponent<TMP_Text>().text = localIPAddress;

        serverBtn.onClick.AddListener(() => {

            // Get IP address from screen and set it before starting the Host
            string IPAddress = IPAddressInp.GetComponent<TMP_InputField>().text;

            Debug.Log("{" + IPAddress + "}");

            SetNetworkIPAddress(IPAddress);

            NetworkManager.Singleton.StartServer();
            Debug.Log("Server started.");

            serverBtn.GetComponent<Image>().color = selectedColor;

            //serverBtn.gameObject.SetActive(false);
            hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
        });

        hostBtn.onClick.AddListener(() => {
            // Get IP address from screen and set it before starting the Host
            string IPAddress = IPAddressInp.GetComponent<TMP_InputField>().text;
            Debug.Log("{" + IPAddress + "}");

            SetNetworkIPAddress(IPAddress);

            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started.");

            hostBtn.GetComponent<Image>().color = selectedColor;

            serverBtn.gameObject.SetActive(false);
            //hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
        });

        clientBtn.onClick.AddListener(() => {
            // Get IP address from screen and set it before starting the Host
            string IPAddress = IPAddressInp.GetComponent<TMP_InputField>().text;
            Debug.Log("{" + IPAddress + "}");

            SetNetworkIPAddress(IPAddress);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started.");

            clientBtn.GetComponent<Image>().color = selectedColor;

            serverBtn.gameObject.SetActive(false);
            hostBtn.gameObject.SetActive(false);
            //clientBtn.gameObject.SetActive(false);

        });


    }

    public void SetNetworkIPAddress(string IPAddress)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                                IPAddress,  // The IP address is a string
                                (ushort)7777, // The port number is an unsigned short
                                "" // The server listen address is a string.
                                );
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
