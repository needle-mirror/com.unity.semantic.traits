using System;
using Unity.Entities;
using Unity.Entities.Hybrid.Extensions;
using UnityEngine;

namespace Unity.Semantic.Traits
{
    /// <summary>
    /// Component used on objects that contain Traits
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu(Constants.MenuName + "/Semantic Object")]
    public class SemanticObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        bool m_EnableTraitInspectors = true;

        /// <summary>
        /// Entity Manager
        /// </summary>
        public EntityManager EntityManager => m_EntityManager;

        /// <summary>
        /// Parent object entity
        /// </summary>
        public Entity Entity => m_Entity;

        internal bool EnableTraitInspectors
        {
            get => m_EnableTraitInspectors;
            set => m_EnableTraitInspectors = value;
        }

        EntityManager m_EntityManager;
        World m_World;
        Entity m_Entity;

        /// <summary>
        /// Convert SemanticObject component to a SemanticObjectData on this object entity
        /// </summary>
        /// <param name="entity">Current Entity</param>
        /// <param name="destinationManager">Current EntityManager</param>
        /// <param name="conversionSystem">System that is used for this conversion</param>
        public void Convert(Entity entity, EntityManager destinationManager, GameObjectConversionSystem conversionSystem)
        {
            if (!destinationManager.HasComponent(entity, typeof(SemanticObjectData)))
            {
                m_Entity = entity;
                m_EntityManager = destinationManager;
                m_World = destinationManager.World;

                destinationManager.AddComponent<SemanticObjectData>(entity);
                destinationManager.AddComponentObject(entity, transform);
            }
        }

        void OnDestroy()
        {
            if (m_World != default &&  m_World.IsCreated)
            {
                m_EntityManager.RemoveComponent<Transform>(m_Entity);
                m_EntityManager.RemoveComponent<SemanticObjectData>(m_Entity);

                if (m_EntityManager.GetComponentCount(m_Entity) == 0)
                    m_EntityManager.DestroyEntity(m_Entity);
            }
        }

        void OnValidate()
        {
            Invoke(nameof(AddEntityConverter), float.Epsilon);
        }

        void AddEntityConverter()
        {
            // Since a developer may want to use ConvertToEntity instead of MirrorAsEntity we can't enforce using [RequireComponent]
            if (!GetComponent<MirrorAsEntity>() && !GetComponent<ConvertToEntity>())
                gameObject.AddComponent<MirrorAsEntity>();
        }
    }
}
