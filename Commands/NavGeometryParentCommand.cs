using CommandSystem;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NavGeometryParentCommand : ParentCommand
    {
        public override string Command => "navgeo";

        public override string[] Aliases => ["ngeo", "navg"];

        public override string Description => "NavGeometry parent command.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "NavGeometry is a way to bypass unbakeable map geometry.";
            return true;
        }
    }
}
