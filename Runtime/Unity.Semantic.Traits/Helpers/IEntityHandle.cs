using System;
using Unity.Entities;
using UnityEngine;

interface IEntityHandle
{
    EntityManager EntityManager { get; }
    Entity Entity { get; }

    event Action InspectorUpdated;
}
