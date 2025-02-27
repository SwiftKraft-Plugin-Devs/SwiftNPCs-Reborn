namespace SwiftNPCs.Features
{
    public abstract class NPCComponent
    {
        public NPCCore Core { get; set; }

        public abstract void Begin();
        public abstract void Tick();
    }
}
