using Unity.Entities;

public struct PerlinElevator : IComponentData
{
    public float yMin;
    public float yMax;
    public float perlinRate;
    public float perlinX;
}
