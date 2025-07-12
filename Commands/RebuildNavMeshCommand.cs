using CommandSystem;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RebuildNavMeshCommand : ICommand
    {
        public string Command => "rebuildnavmesh";

        public string[] Aliases => ["navbuild", "rnvmesh", "buildnav"];

        public string Description => "Rebuilds the nav mesh. ";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission([PlayerPermissions.RoundEvents]))
            {
                response = "No permission! ";
                return false;
            }

            Core.BuildNavMesh();

            response = "Rebuilt nav mesh.";
            return true;
        }
    }
}
