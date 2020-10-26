using System;
using System.Reflection;
using Unity.Collections;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class ListIntPropertyDescriptor : ListPropertyDescriptor<ListIntProperty> {}
    class ListBooleanPropertyDescriptor : ListPropertyDescriptor<ListBooleanProperty> {}
    class ListFloatPropertyDescriptor : ListPropertyDescriptor<ListFloatProperty> {}

    abstract class ListPropertyDescriptor<T> : TraitPropertyDescriptor<T> where T : TraitPropertyDefinition
    {
        public override TraitPropertyDescriptorData GetData(T property)
        {
            var fixedListType = ConvertToFixedListType(property.Type);
            return new TraitPropertyDescriptorData(TypeResolver.GetUnmangledName(property.Type),
                TypeResolver.GetUnmangledName(fixedListType),
                string.Empty,
                FixedListCapacity(fixedListType));
        }

        static Type ConvertToFixedListType(Type listType)
        {
            return typeof(FixedList512<>).MakeGenericType(listType.GenericTypeArguments[0]);
        }

        static int FixedListCapacity(Type type)
        {
            var propertyInfo = type.GetProperty(nameof(FixedList32<int>.Capacity),
                BindingFlags.Public | BindingFlags.Instance);

            return (int)propertyInfo.GetValue(Activator.CreateInstance(type));
        }
    }
}
