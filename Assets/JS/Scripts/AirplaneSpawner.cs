using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

public sealed class AirplaneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject airplanePrefab = null;
    [Header("Orbit Parameters")]
    [SerializeField] private uint spawnCount = 100;
    [SerializeField] Vector2 speedRange = new Vector2 { x = 1f, y = 1.2f };
    [SerializeField] Vector2 radiusRange = new Vector2 { x = 7f, y = 10f };
    [SerializeField] float epicenterDeviation = 1f;
    [Header("Elevation Parameters")]
    [SerializeField] Vector2 heightRange = new Vector2 { x = 0f, y = 10f };
    [SerializeField] Vector2 perlinRateRange = new Vector2 { x = 0.1f, y = 0.5f };
    private void OnValidate()
    {
        if (epicenterDeviation < 0f)
            epicenterDeviation = 0f;
        if (speedRange.y < speedRange.x)
            speedRange.y = speedRange.x;
        if (radiusRange.y < radiusRange.x)
            radiusRange.y = radiusRange.x;
        if (heightRange.y < heightRange.x)
            heightRange.y = heightRange.x;
        if (perlinRateRange.y < perlinRateRange.x)
            perlinRateRange.y = perlinRateRange.x;
    }

    private Entity entityPrefab;
    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(airplanePrefab,
            GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null));

        SpawnWave((int)spawnCount);
    }

    private void SpawnWave(int count)
    {
        NativeArray<Entity> newEnemiesArray = new NativeArray<Entity>(count, Allocator.Temp);

        for (int i = 0; i < count; i++)
        {
            newEnemiesArray[i] = entityManager.Instantiate(entityPrefab);
            entityManager.AddComponentData(newEnemiesArray[i], new Orbit
            {
                speed = Random.Range(speedRange.x, speedRange.y),
                angle = Random.Range(0f, 2f * Mathf.PI),
                epicenter = transform.position + new Vector3
                {
                    x = Random.Range(-epicenterDeviation, epicenterDeviation),
                    z = Random.Range(-epicenterDeviation, epicenterDeviation)
                },
                radius = Random.Range(radiusRange.x, radiusRange.y)
            });
            entityManager.AddComponentData(newEnemiesArray[i], new PerlinElevator
            {
                yMin = heightRange.x, yMax = heightRange.y,
                perlinRate = Random.Range(perlinRateRange.x, perlinRateRange.y),
                perlinX = Random.Range(0f, 1000f)
            });
        }

        newEnemiesArray.Dispose();
    }
}
