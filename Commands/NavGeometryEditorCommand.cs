using CommandSystem;
using LabApi.Features.Wrappers;
using SwiftNPCs.NavGeometry;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(NavGeometryParentCommand))]
    public class NavGeometryEditorCommand : ICommand
    {
        public string Command => "editor";

        public string[] Aliases => ["edit", "wand", "tool"];

        public string Description => "Gives you the editor tool.";

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

            NavGeometryEditor.GiveEditor(p);

            response = "Gave you the editor!";
            return true;
        }
    }
}
