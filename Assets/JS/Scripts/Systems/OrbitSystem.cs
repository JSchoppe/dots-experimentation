using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public sealed class OrbitSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<Orbit>().ForEach((ref Translation trans, ref Rotation rot, ref Orbit orbit) =>
        {
            // Progress the angle of the orbit.
            orbit.angle += Time.DeltaTime * orbit.speed;

            // Calculate new position.
            trans.Value = orbit.epicenter + new float3
                { x = math.cos(orbit.angle) * orbit.radius, z = math.sin(orbit.angle) * orbit.radius };
            // Calculate new rotation.
            rot.Value = quaternion.RotateY(-orbit.angle);
        });
    }
}
