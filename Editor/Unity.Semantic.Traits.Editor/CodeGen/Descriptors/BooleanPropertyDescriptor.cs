using Unity.Semantic.Traits;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class BooleanPropertyDescriptor : TraitPropertyDescriptor<BooleanProperty>
    {
        public override TraitPropertyDescriptorData GetData(BooleanProperty property)
        {
            return new TraitPropertyDescriptorData(property.Type.ToString(), property.Type.ToString(), property.Value.ToString().ToLower());
        }
    }
}
