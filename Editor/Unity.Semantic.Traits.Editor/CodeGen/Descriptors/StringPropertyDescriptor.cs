using Unity.Collections;
using Unity.Semantic.Traits;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class StringPropertyDescriptor : TraitPropertyDescriptor<StringProperty>
    {
        public override TraitPropertyDescriptorData GetData(StringProperty property)
        {
            return new TraitPropertyDescriptorData(typeof(string).ToString(), typeof(FixedString32).ToString(), $"{property.Value}", FixedString32.UTF8MaxLengthInBytes);
        }
    }
}
