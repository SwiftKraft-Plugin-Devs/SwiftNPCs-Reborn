namespace SwiftNPCs.Utils.Interfaces
{
    /// <summary>
    /// And interface for objects that have a MaxValue and a CurrentValue.
    /// </summary>
    /// <typeparam name="T">The data type for the values</typeparam>
    public interface IValue<T>
    {
        public T MaxValue { get; set; }

        public T CurrentValue { get; }

        public float GetPercentage();
    }
}
