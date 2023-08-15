using TagwizzQASniffer.Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Network
{
    public class NetworkBehaviour : MonoBehaviour
    {
        private HubClient _client;
        private void Start()
        {
            _client = new HubClient();
        }

        public void Connect(string ip, string port)
        {
            _client.StartClient(ip,int.Parse(port));
        }
        
    }
}
