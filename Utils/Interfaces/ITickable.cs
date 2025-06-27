namespace SwiftNPCs.Utils.Interfaces
{
    /// <summary>
    /// An interface for objects that have a Tick() function.
    /// </summary>
    public interface ITickable
    {
        public float Tick(float deltaTime);
    }
}
