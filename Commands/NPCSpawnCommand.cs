using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SwiftNPCs.Features;
using SwiftNPCs.Features.Personalities;
using System;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(NPCParentCommand))]
    public class NPCSpawnCommand : ICommand
    {
        public string Command => "spawn";

        public string[] Aliases => ["add", "spwn", "sp"];

        public string Description => "Spawns an NPC. Usage: spawn <Role>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission([PlayerPermissions.PlayersManagement]))
            {
                response = "No permission! ";
                return false;
            }

            if (!Player.TryGet(sender, out Player p))
            {
                response = "Failed to identify sender, only a player can execute this command.";
                return false;
            }

            if (arguments.Count < 1 || !arguments.Array[2].ToLower().TryGetRoleFromString(out RoleTypeId role))
            {
                response = arguments.Count < 1 ? "Please input a role! (ie. Scp173, ClassD)" : "\"" + arguments.Array[2] + "\" is not a valid role! (ie. Scp173, ClassD)";
                return false;
            }

            NPC npc = new(p.Position, role);
            npc.Core.SetPersonality<NPCPersonalityHuman>();

            response = "Spawned " + role + " NPC at " + p.Position;
            return true;
        }
    }
}
