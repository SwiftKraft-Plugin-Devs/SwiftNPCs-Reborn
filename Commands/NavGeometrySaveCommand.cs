using CommandSystem;
using LabApi.Features.Wrappers;
using SwiftNPCs.NavGeometry;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(NavGeometryParentCommand))]
    public class NavGeometrySaveCommand : ICommand
    {
        public string Command => "save";

        public string[] Aliases => ["sav"];

        public string Description => "Saves the current room.";

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

            NavGeometryManager.SaveNavGeometry(p.Room);

            response = "Saved room.";
            return true;
        }
    }
}
