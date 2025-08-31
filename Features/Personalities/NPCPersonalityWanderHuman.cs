using InventorySystem.Items;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using SwiftNPCs.Features.Targettables;
using System.Collections.Generic;
using System.Linq;
using Utils.NonAllocLINQ;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityWanderHuman : NPCPersonalityWander
    {
        public override NPCPersonalityBase CombatPersonality => new NPCPersonalityHumanCombat();

        public virtual NPCPersonalityFollow FollowPersonality => new();

        public override void Begin()
        {
            base.Begin();
            PlayerEvents.SendingVoiceMessage += OnSendingVoiceMessage;
            PlayerEvents.Cuffed += OnCuffed;
            Core.Scanner.OnBeingAttacked += OnBeingAttacked;
        }

        public override void Tick()
        {
            base.Tick();

            if (IsThreat && !IsArmed && GetWeapon(out ItemBase item, out _))
                Core.Inventory.EquipItem(item);

            Core.Motor.MoveState = Core.HasTarget && (!IsArmed || !IsThreat) ? PlayerRoles.FirstPersonControl.PlayerMovementState.Sprinting : PlayerRoles.FirstPersonControl.PlayerMovementState.Walking;

            if (IsCivilian && !WrapperPlayer.IsDisarmed && !IsArmed && Core.Scanner.TryGetFriendlies(out List<Player> players))
            {
                Player p = players.FirstOrDefault((p) => p.Faction == WrapperPlayer.Faction && (p.Team == PlayerRoles.Team.FoundationForces || p.Team == PlayerRoles.Team.ChaosInsurgency));
                if (p != null)
                    StartFollow(p);
            }
        }

        public override void End()
        {
            base.End();
            PlayerEvents.SendingVoiceMessage -= OnSendingVoiceMessage;
            PlayerEvents.Cuffed -= OnCuffed;
            Core.Scanner.OnBeingAttacked -= OnBeingAttacked;
        }

        private void OnCuffed(LabApi.Events.Arguments.PlayerEvents.PlayerCuffedEventArgs ev)
        {
            if (ev.Target != WrapperPlayer)
                return;

            Core.Target = null;
            StartFollow(ev.Player);
        }

        private void OnBeingAttacked(Player obj)
        {
            if (!GetWeapon(out ItemBase item, out _))
                return;

            Core.Target = new TargetablePlayer(obj);
            Core.Inventory.EquipItem(item);
            Core.SetPersonality(CombatPersonality);
        }

        private void OnSendingVoiceMessage(LabApi.Events.Arguments.PlayerEvents.PlayerSendingVoiceMessageEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive || ev.Player.Faction != WrapperPlayer.Faction || (ev.Player.Position - Core.Position).sqrMagnitude >= 100f)
                return;

            StartFollow(ev.Player);
        }

        public void StartFollow(Player player)
        {
            var follow = FollowPersonality;
            Core.SetPersonality(follow);
            follow.FollowTarget = player;
        }
    }
}
