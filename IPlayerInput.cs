public interface IPlayerInput
{
    float Horizontal { get; }
    float Vertical { get; }
    bool Jump { get; }
    bool FireHeld { get; }
    bool FireReleased { get; }
    float MouseX { get; }
    float MouseY { get; }
}
