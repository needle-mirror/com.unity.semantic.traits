using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities.Conversion;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using static Unity.Debug;

[assembly: InternalsVisibleTo("Unity.Semantic.Traits")]

namespace Unity.Entities.Hybrid.Extensions
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("DOTS/Mirror As Entity")]
    class MirrorAsEntity : MonoBehaviour
    {
        [NonSerialized]
        bool m_Mirrored;

        void OnEnable()
        {
            Mirror();
        }

        void Mirror()
        {
            if (m_Mirrored)
                return;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var system = world.GetOrCreateSystem<MirrorAsEntitySystem>();
                system.AddToBeMirrored(world, this);
                m_Mirrored = true;
            }
            else if (Application.isPlaying)
            {
                LogWarning($"{nameof(MirrorAsEntity)} failed because there is no {nameof(World.DefaultGameObjectInjectionWorld)}", this);
            }
#if UNITY_EDITOR
            else
            {
                // When transitioning back from play mode to edit mode the default editor world isn't immediately available
                DefaultWorldInitialization.DefaultLazyEditModeInitialize();
                EditorApplication.delayCall += Mirror;
            }
#endif
        }
    }

    [ExecuteAlways]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    class MirrorAsEntitySystem : ComponentSystem
    {
        Dictionary<World, List<MirrorAsEntity>> m_ToBeMirrored = new Dictionary<World, List<MirrorAsEntity>>();

        public BlobAssetStore BlobAssetStore { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            BlobAssetStore = new BlobAssetStore();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (BlobAssetStore != null)
            {
                BlobAssetStore.Dispose();
                BlobAssetStore = null;
            }
        }

        // using `this.World` is a sign of a problem - that World is only needed so that this system will update, but
        // adding entities to it directly is wrong (must be directed via m_ToBeConverted).
        // ReSharper disable once UnusedMember.Local
        new World World => throw new InvalidOperationException($"Do not use `this.World` directly (use {nameof(m_ToBeMirrored)})");

        protected override void OnUpdate()
        {
            if (m_ToBeMirrored.Count != 0)
                Convert();
        }

        public void AddToBeMirrored(World world, MirrorAsEntity mirrorAsEntity)
        {
            if (!m_ToBeMirrored.TryGetValue(world, out var list))
            {
                list = new List<MirrorAsEntity>();
                m_ToBeMirrored.Add(world, list);
            }
            list.Add(mirrorAsEntity);
        }

        static void AddRecurse(EntityManager manager, Transform transform, HashSet<Transform> toBeDetached)
        {
            if (transform.GetComponent<StopConvertToEntity>() != null)
            {
                toBeDetached.Add(transform);
                return;
            }

            GameObjectEntity.AddToEntityManager(manager, transform.gameObject);

            foreach (Transform child in transform)
                AddRecurse(manager, child, toBeDetached);
        }

        void Convert()
        {
            var toBeDetached = new HashSet<Transform>();
            var conversionRoots = new HashSet<GameObject>();

            try
            {
                foreach (var convertToWorld in m_ToBeMirrored)
                {
                    var toBeConverted = convertToWorld.Value;
                    toBeConverted.RemoveAll(m => m == null);
                    if (toBeConverted.Count == 0)
                        continue;

                    var settings = new GameObjectConversionSettings(
                        convertToWorld.Key,
                        GameObjectConversionUtility.ConversionFlags.AssignName)
                    {
                        Systems = new List<Type>() { typeof(ConvertGameObjectToEntitySystem) }
                    };

                    settings.BlobAssetStore = BlobAssetStore;

                    using (var gameObjectWorld = settings.CreateConversionWorld())
                    {
                        toBeConverted.RemoveAll(convert =>
                        {
                            if (convert.GetComponent<StopConvertToEntity>() != null)
                            {
                                LogWarning(
                                    $"{nameof(MirrorAsEntity)} will be ignored because of a {nameof(StopConvertToEntity)} on the same GameObject",
                                    convert.gameObject);
                                return true;
                            }

                            var parent = convert.transform.parent;
                            var remove = parent != null && parent.GetComponentInParent<MirrorAsEntity>() != null;
                            if (remove && parent.GetComponentInParent<StopConvertToEntity>() != null)
                            {
                                LogWarning(
                                    $"{nameof(MirrorAsEntity)} will be ignored because of a {nameof(StopConvertToEntity)} higher in the hierarchy",
                                    convert.gameObject);
                            }

                            return remove;
                        });

                        foreach (var convert in toBeConverted)
                            AddRecurse(gameObjectWorld.EntityManager, convert.transform, toBeDetached);

                        foreach (var convert in toBeConverted)
                        {
                            conversionRoots.Add(convert.gameObject);
                            toBeDetached.Remove(convert.transform);
                        }

                        GameObjectConversionUtility.Convert(gameObjectWorld);
                    }
                }
            }
            finally
            {
                m_ToBeMirrored.Clear();

                foreach (var transform in toBeDetached)
                    transform.parent = null;
            }
        }
    }
}
