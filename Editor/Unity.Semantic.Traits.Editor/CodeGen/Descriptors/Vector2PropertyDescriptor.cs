using Unity.Mathematics;
using Unity.Semantic.Traits;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class Vector2PropertyDescriptor : TraitPropertyDescriptor<Vector2Property>
    {
        public override TraitPropertyDescriptorData GetData(Vector2Property property)
        {
            return new TraitPropertyDescriptorData(typeof(Vector2).ToString(), typeof(float2).ToString(), $"new Vector2({property.Value.x}f, {property.Value.y})");
        }
    }
}
