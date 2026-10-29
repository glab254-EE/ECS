using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    [field: SerializeField]
    public GameObject prefab { get; private set; }
    [field: SerializeField]
    public Vector3 RowOffset { get; private set; }
    [field: SerializeField]
    public Vector3 CollumnOffset { get; private set; }
    [field: SerializeField]
    public int RowCount { get; private set; }
    [field: SerializeField]
    public int CollumnCount { get; private set; }
}

public struct SpawnerData : IComponentData
{
    public Entity prefab;
    public Vector3 RowOffset;
    public Vector3 CollumnOffset;
    public int RowCount;
    public int CollumnCount;
}
//public partial struct PrepareSpawningSystem: ISystem
//{
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<SpawnerData>();
//    }
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        state.Enabled = false;
//        var data = SystemAPI.GetSingleton<SpawnerData>();
//        var entity = SystemAPI.GetSingletonEntity<SpawnerData>();

//        state.EntityManager.AddComponentData(entity, new RequestEntityPrefabLoaded
//        {
//            Prefab = data.prefab,
//        });
//    }
//}
public partial struct SpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerData>();
        //state.RequireForUpdate<PrefabLoadResult>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var data = SystemAPI.GetSingleton<SpawnerData>();
        var entity = SystemAPI.GetSingletonEntity<SpawnerData>();

        //if (!SystemAPI.HasComponent<PrefabLoadResult>(entity))
        //{
        //    return;
        //}

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        //var prefabLoadResult = SystemAPI.GetComponent<PrefabLoadResult>(entity);
        for (int row = 0; row < data.RowCount; row++)
        {
            for (int i = 0; i < data.CollumnCount; i++)
            {
                float3 newPos = data.RowOffset * row + data.CollumnOffset * i;
                var newEntity = ecb.Instantiate(data.prefab);
                ecb.SetComponent(newEntity, LocalTransform.FromPosition(newPos));
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        state.Enabled = false;
    }
}
public class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

        AddComponent(entity, new SpawnerData()
        {
            prefab = GetEntity(authoring.prefab,TransformUsageFlags.Dynamic),
            RowCount = authoring.RowCount,
            CollumnCount = authoring.CollumnCount,
            RowOffset = authoring.RowOffset,
            CollumnOffset = authoring.CollumnOffset,
        });
    }
}