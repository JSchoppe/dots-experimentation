using Unity.Entities;
using Unity.Mathematics;

public struct Orbit : IComponentData
{
    public float speed;
    public float angle;
    public float radius;
    public float3 epicenter;
}
