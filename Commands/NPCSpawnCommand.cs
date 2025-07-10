using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SwiftNPCs.Features;
using SwiftNPCs.Features.Personalities;
using System;
using System.Linq;
using System.Text;

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

            StringBuilder sb = new();
            for (int i = 3; i < arguments.Array.Length; i++)
            {
                sb.Append(arguments.Array[i]);
                if (i < arguments.Array.Length - 1)
                    sb.Append(' ');
            }
            string name = sb.ToString();

            NPC npc = new(p.Position, string.IsNullOrWhiteSpace(name) ? NPC.DefaultName : name, role);
            npc.Core.SetPersonality<NPCPersonalityWanderHuman>();

            response = "Spawned " + role + " NPC at " + p.Position + " with name \"" + npc.WrapperPlayer.Nickname + "\"";
            return true;
        }
    }
}
