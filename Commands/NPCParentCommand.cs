using CommandSystem;
using LabApi.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SwiftNPCs.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NPCParentCommand : ParentCommand
    {
        public static string Prompt;

        public static void SetPrompt()
        {
            Prompt = 
                $"""
                
                <b>Welcome to {Core.Instance.Name}!</b>
                <b>----------------------------</b>
                {Core.Instance.Description}
                <b>Author:</b> {Core.Instance.Author}
                <b>Version:</b> {Core.Instance.Version}
                <b>----------------------------</b>
                <b>Required LabAPI Version:</b> {Core.Instance.RequiredApiVersion}
                <b>Current LabAPI Version:</b> {LabApiProperties.CurrentVersion}

                {(Core.Instance.RequiredApiVersion != LabApiProperties.CurrentVersion ?
                "<b><color=#FF0000>WARNING: The current LabAPI version does not match the required LabAPI version; \ninconsistent behaviors or errors may occur.</color></b>"
                : "<b><color=#00FF00>Correct LabAPI version detected, good job!</color></b>")}
                <b>----------------------------</b>
                """;
        }

        public override string Command => "swiftnpcs";

        public override string[] Aliases => ["swiftnpc", "snpcs", "snpc", "npcs", "npc"];

        public override string Description => "Parent command for all SwiftNPCs commands.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = Prompt;
            return true;
        }
    }
}
