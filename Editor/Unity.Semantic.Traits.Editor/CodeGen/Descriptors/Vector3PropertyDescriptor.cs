using Unity.Mathematics;
using Unity.Semantic.Traits;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class Vector3PropertyDescriptor : TraitPropertyDescriptor<Vector3Property>
    {
        public override TraitPropertyDescriptorData GetData(Vector3Property property)
        {
            return new TraitPropertyDescriptorData(typeof(Vector3).ToString(), typeof(float3).ToString(), $"new Vector3({property.Value.x}f, {property.Value.y}f, {property.Value.z}f)");
        }
    }
}
