using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class AssetValidator : IAssetValidator<TraitDefinition>, IAssetValidator<EnumDefinition>
    {
        public event Action<string, ScriptableObject> errorLogged;

        internal bool TraitAssetsAreValid()
        {
            bool assetValid = true;

            foreach (var enumeration in TraitAssetDatabase.EnumDefinitions)
            {
                assetValid &= IsAssetValid(enumeration);
            }
            foreach (var trait in TraitAssetDatabase.TraitDefinitions)
            {
                assetValid &= IsAssetValid(trait);
            }

            return assetValid;
        }

        public bool IsAssetValid(EnumDefinition enumeration)
        {
            bool enumValid = true;

            // Check for duplicate enum values
            var declaredValueNames = new List<string>();
            var declaredValueIds = new List<int>();
            foreach (var value in enumeration.Elements)
            {
                if (declaredValueNames.Contains(value.Name))
                {
                    errorLogged?.Invoke($"{value.Name} is a duplicated value name.", enumeration);
                    enumValid = false;
                }
                else
                    declaredValueNames.Add(value.Name);

                if (declaredValueIds.Contains(value.Id))
                {
                    errorLogged?.Invoke($"{value.Id} is a duplicated value id.", enumeration);
                    enumValid = false;
                }
                else
                    declaredValueIds.Add(value.Id);
            }
            return enumValid;
        }

        public bool IsAssetValid(TraitDefinition trait)
        {
            bool traitValid = true;

            // Check for duplicate field names
            var declaredPropertyNames = new List<string>();
            var declaredPropertyIds = new List<int>();
            foreach (var property in trait.Properties)
            {
                if (property == null)
                    continue;

                if (declaredPropertyNames.Contains(property.Name))
                {
                    errorLogged?.Invoke($"{property.Name} is a duplicated property name.", trait);
                    traitValid = false;
                }
                else
                    declaredPropertyNames.Add(property.Name);

                if (declaredPropertyIds.Contains(property.Id))
                {
                    errorLogged?.Invoke($"{property.Id} is a duplicated property id.", trait);
                    traitValid = false;
                }
                else
                    declaredPropertyIds.Add(property.Id);
            }
            return traitValid;
        }
    }
}
