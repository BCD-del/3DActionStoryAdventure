namespace Assets._Project.Develop.Runtime.InputFeature
{
    public interface IPCPlayerInput
    {
        float Horizontal { get; }
        bool Jump { get; }
        float MouseX { get; }
        float MouseY { get; }
        float Vertical { get; }
    }
}