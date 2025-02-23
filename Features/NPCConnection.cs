using Mirror;

namespace SwiftNPCs.Features
{
    public class NPCConnection : NetworkConnectionToClient
    {
        public override string address { get; } = "127.0.0.1";

        public static int _idGenerator = 65535;

        public NPCConnection() : base(_idGenerator--) { }
    }
}
