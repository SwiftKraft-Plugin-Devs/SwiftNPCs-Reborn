using CommandSystem;
using LabApi.Features.Wrappers;
using SwiftNPCs.NavGeometry;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(NavGeometryParentCommand))]
    public class NavGeometryLoadCommand : ICommand
    {
        public string Command => "load";

        public string[] Aliases => ["spawn"];

        public string Description => "Loads the current room.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission([PlayerPermissions.RoundEvents]))
            {
                response = "No permission! ";
                return false;
            }

            if (!Player.TryGet(sender, out Player p))
            {
                response = "Failed to identify sender, only a player can execute this command.";
                return false;
            }

            NavGeometryManager.LoadNavGeometry(p.Room);

            response = "Loaded room.";
            return true;
        }
    }
}
