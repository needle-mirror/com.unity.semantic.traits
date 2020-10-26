using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
class ComponentDataHandle<T> where T : struct, IComponentData
{
    public T Data
    {
        get
        {
            if (Entity != Entity.Null && EntityManager != default && EntityManager.World.IsCreated)
            {
                return EntityManager.GetComponentData<T>(Entity);
            }

            return m_Data;
        }
        set
        {
            if (EntityManager != default && EntityManager.World.IsCreated)
            {

                if (Entity == Entity.Null)
                    Entity = EntityManager.CreateEntity(typeof(T));

                if (!EntityManager.HasComponent<T>(Entity))
                    EntityManager.AddComponent<T>(Entity);

                if (!TypeManager.IsZeroSized(TypeManager.GetTypeIndex<T>()))
                    EntityManager.SetComponentData(Entity, value);
            }

            m_Data = value;
        }
    }

    public EntityManager EntityManager { get; private set; }
    public Entity Entity { get; private set; }

    [SerializeField]
    T m_Data;

    public void Initialize(IEntityHandle entityHandle)
    {
        EntityManager = entityHandle.EntityManager;
        Entity = entityHandle.Entity;
        Data = m_Data;

        entityHandle.InspectorUpdated += InspectorUpdated;
    }

    public delegate void UpdateFunc(ref T data);

    public void Update(UpdateFunc updateData, bool propagate = true)
    {
        m_Data = Data;
        updateData(ref m_Data);

        if (propagate)
            Data = m_Data;
    }

    void InspectorUpdated()
    {
        Data = m_Data;
    }
}
