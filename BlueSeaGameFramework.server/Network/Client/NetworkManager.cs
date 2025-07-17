
using BlueSeaGameFramework.server.Utils.Singleton;

namespace BlueSeaGameFramework.server.Network.Client
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        private ConcurrentDictionary<int, UClient> clients;
        private static int sessionId;
        public void init()
        {
            clients = new ConcurrentDictionary<int, UClient>();
            sessionId = 1;
        }

        public static int generateId()
        {   
            sessionId++;
            return sessionId;
        }
        
        public void RemoveClient(int sessionid) {
            UClient client;
            if (clients.TryRemove(sessionid,out client))
            {
                client.Close();
                client = null;
            }
        
        }
        
        public UClient GetClient(int sessionid) {

            UClient client;
            if (clients.TryGetValue(sessionid, out client))
            {
                return client;
            }
            return null;
        }
    }
}

