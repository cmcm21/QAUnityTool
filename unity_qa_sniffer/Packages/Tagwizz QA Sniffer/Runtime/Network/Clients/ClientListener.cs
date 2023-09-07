
using System.Collections.Generic;

namespace TagwizzQASniffer.Network.Clients
{
    public interface IClientListener
    {
        public void Connected();
        public void Disconnected();
        public void ExceptionThrown();
    }

    public class ClientObserver
    {
        private List<IClientListener> _clients;


        public void Subscribe(IClientListener clientListener)
        {
            _clients ??= new List<IClientListener>();
            _clients.Add(clientListener);
        }

        public void ConnectedNotify()
        {
            foreach (var client in _clients)
                client.Connected();
        }

        public void DisconnectedNotify()
        {
            foreach(var client in _clients)
                client.Disconnected();
        }

        public void ExceptionThrownNotify()
        {
            foreach(var client in _clients)
                client.ExceptionThrown();
        }

        public void Unsubscribe(IClientListener clientListener)
        {
            _clients ??= new List<IClientListener>();
            _clients.Remove(clientListener);
        }
    } 
}
