namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityWanderHuman : NPCPersonalityWander
    {
        public override NPCPersonalityBase CombatPersonality => new NPCPersonalityHumanCombat();
    }
}
