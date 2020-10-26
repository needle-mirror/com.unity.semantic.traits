using Unity.Semantic.Traits;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class FloatPropertyDescriptor : TraitPropertyDescriptor<FloatProperty>
    {
        public override TraitPropertyDescriptorData GetData(FloatProperty property)
        {
            return new TraitPropertyDescriptorData(property.Type.ToString(), property.Type.ToString(), $"{property.Value}f");
        }
    }
}
