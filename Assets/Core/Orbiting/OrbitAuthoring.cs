using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

public class OrbitingAuthoring : MonoBehaviour
{
    [field:SerializeField]
    public float Offset { get; private set; }
    [field: SerializeField]
    public float MoveSpeed { get; private set; }
    [field: SerializeField]
    public float OrbitRadius { get; private set; }
}

public struct OrbitData : IComponentData
{
    public Vector3 CenterPosition;
    public float Offset;
    public float MoveSpeed;
    public float OrbitRadius;
}
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct OrbitingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<OrbitData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new OrbitingJob()
        {
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime
        }.ScheduleParallel(state.Dependency);
    }
}
[BurstCompile]
public partial struct OrbitingJob : IJobEntity
{
    public float ElapsedTime;
    void Execute(ref LocalTransform transform, ref OrbitData data)
    {
        if (data.CenterPosition == new Vector3(-0.1f, -0.1f, -0.1f))
        {
            data.CenterPosition = transform.Position;
        }

        Vector3 newPosition = data.CenterPosition + new Vector3(math.sin((ElapsedTime + data.Offset) * data.MoveSpeed) * data.OrbitRadius, 0, math.cos((ElapsedTime + data.Offset) * data.MoveSpeed) * data.OrbitRadius);

        transform.Position = newPosition;
    }
}

public class OrbitingBaker : Baker<OrbitingAuthoring>
{
    public override void Bake(OrbitingAuthoring authoring)
    {
        var rand = new System.Random();

        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new OrbitData
        {
            Offset =(float) rand.NextDouble() * authoring.Offset,
            MoveSpeed = authoring.MoveSpeed,
            OrbitRadius = authoring.OrbitRadius,
            CenterPosition = new Vector3(-0.1f, -0.1f, -0.1f),
        });
    }
}