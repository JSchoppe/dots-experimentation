using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

// Creates and manages the waves in the scene.
public class WaveGenerator : MonoBehaviour
{
    // The instance of the job that stores job variables.
    UpdateMeshJob meshModificationJob;
    // Provides info about the state of the job.
    JobHandle meshModificationJobHandle;

    [BurstCompile] // Compiles this job with platform specific technology.
    private struct UpdateMeshJob : IJobParallelFor
    {
        // Fast unmanaged memory for a series of Vector3 structs.
        public NativeArray<Vector3> vertices;
        [ReadOnly] // Performance boost from promising we will not modify this data.
        public NativeArray<Vector3> normals;
        // These act as the "parameters" of the job.
        public float offsetSpeed;
        public float scale;
        public float height;
        public float time;
        // This is the implementation of the job.
        // It is called for each vertex on the mesh.
        public void Execute(int i)
        {
            // Make sure this vertex is on the top surface.
            // TODO seems like something that could be precalculated in this scenario.
            if (normals[i].z > 0f)
            {
                // You must pull the value out, modify it,
                // and then put it back in.
                Vector3 vertex = vertices[i];

                // Uses perlin noise to generate an elevation.
                // TODO seems like this could be dryer (pass in offsetSpeed * time)
                float noiseValue =
                    Noise(vertex.x * scale + offsetSpeed * time, vertex.y * scale +
                offsetSpeed * time);

                vertices[i] =
                    new Vector3(vertex.x, vertex.y, noiseValue * height + 0.3f);
            }
        }
        private float Noise(float x, float y)
        {
            float2 pos = math.float2(x, y);
            // The low level code is c++ like (elitist) in its naming.
            return noise.snoise(pos);
        }
    }

    // Inspector references for controlling the behavior.
    [Header("Wave Parameters")]
    public float waveScale;
    public float waveOffsetSpeed;
    public float waveHeight;
    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    private Mesh waterMesh;

    // Persistent fast-access storage for each of our vertices.
    private NativeArray<Vector3> waterVertices;
    private NativeArray<Vector3> waterNormals;

    private void Start()
    {
        waterMesh = waterMeshFilter.mesh;
        // Tells Unity we will update this mesh very often.
        waterMesh.MarkDynamic();
        // Initialize our fast memory to match the inital mesh state.
        waterVertices =
            new NativeArray<Vector3>(waterMesh.vertices, Allocator.Persistent);
        waterNormals =
            new NativeArray<Vector3>(waterMesh.normals, Allocator.Persistent);
    }

    private void Update()
    {
        // Pass the arguments into the job struct.
        meshModificationJob = new UpdateMeshJob()
        {
            vertices = waterVertices,
            normals = waterNormals,
            offsetSpeed = waveOffsetSpeed,
            time = Time.time,
            scale = waveScale,
            height = waveHeight
        };
        // Tell the jobs manager to distribute this task among 64 batches.
        // This job will be executed on each vertex in the mesh.
        meshModificationJobHandle =
            meshModificationJob.Schedule(waterVertices.Length, 64);
    }

    private void LateUpdate()
    {
        // Wait to ensure all threads have finished their tasks.
        meshModificationJobHandle.Complete();
        // Apply the results returned by the job.
        waterMesh.SetVertices(meshModificationJob.vertices);
        // Update mesh lighting.
        waterMesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        // ALL native types must be disposed at the end of
        // the objects lifestyle! Otherwise there will be leaks!
        waterVertices.Dispose();
        waterNormals.Dispose();
    }
}