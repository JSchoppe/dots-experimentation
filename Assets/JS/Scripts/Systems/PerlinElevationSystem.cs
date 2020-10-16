using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class PerlinElevationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        Entities.WithAll<PerlinElevator>().ForEach((ref Translation trans, ref PerlinElevator elevator) =>
        {
            // Progress the distance along the noise.
            elevator.perlinX += dt * elevator.perlinRate;
            // Set the y elevation of this entity.
            trans.Value.y = math.lerp(
                elevator.yMin, elevator.yMax,
                noise.snoise(new float2 { x = elevator.perlinX })
            );
        });
    }
}
