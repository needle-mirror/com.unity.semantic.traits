using Unity.Semantic.Traits;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class IntPropertyDescriptor : TraitPropertyDescriptor<IntProperty>
    {
        public override TraitPropertyDescriptorData GetData(IntProperty property)
        {
            return new TraitPropertyDescriptorData(property.Type.ToString(), property.Type.ToString(), $"{property.Value}");
        }
    }
}
