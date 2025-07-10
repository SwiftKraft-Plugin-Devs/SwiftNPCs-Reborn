using LabApi.Events.Handlers;
using SwiftNPCs.Features.Components;

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
        }

        public override void End()
        {
            base.End();
            PlayerEvents.SendingVoiceMessage -= OnSendingVoiceMessage;
        }

        private void OnSendingVoiceMessage(LabApi.Events.Arguments.PlayerEvents.PlayerSendingVoiceMessageEventArgs ev)
        {
            if (ev.Player.IsAlive && !ev.Player.IsEnemy(WrapperPlayer))
            {
                var follow = FollowPersonality;
                Core.SetPersonality(follow);
                follow.FollowTarget = ev.Player;
            }
        }
    }
}
