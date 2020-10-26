using System;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class EnumReferencePropertyDescriptor : TraitPropertyDescriptor<EnumReferenceProperty>
    {
        public override TraitPropertyDescriptorData GetData(EnumReferenceProperty property)
        {
            if (property.Reference == null)
                throw new Exception($"Trait property {property.Name} is invalid");

            string typeName = $"{TypeResolver.EnumsQualifier}.{property.Reference.name}";
            return new TraitPropertyDescriptorData(typeName, typeName, $"({typeName}){property.Value}");
        }
    }
}
