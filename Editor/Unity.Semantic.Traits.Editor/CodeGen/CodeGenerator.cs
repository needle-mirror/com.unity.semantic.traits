using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using Unity.Entities;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    class CodeGenerator : ICodeGenerator<TraitDefinition>, ICodeGenerator<EnumDefinition>
    {
        CodeRenderer m_CodeRenderer = new CodeRenderer();

        public IEnumerable<string> Generate(string outputPath, TraitDefinition trait)
        {
            if (TypeResolver.TryGetType(trait.name + TypeResolver.ComponentDataSuffix, out var traitType) && typeof(ICustomTraitData).IsAssignableFrom(traitType)) // Custom trait, no codegen needed
                return Enumerable.Empty<string>();

            var fieldsForComponentData = trait.Properties.Select(p =>
            {
                var data = GetTraitDescriptorData(p);

                return new
                {
                    field_id = p.Id,
                    field_name = p.Name,
                    field_type = data?.RuntimeType ?? TypeResolver.GetUnmangledName(p.Type),
                };
            });

            var fieldsForMonoBehaviour = trait.Properties.Select(p =>
            {
                var data = GetTraitDescriptorData(p);

                return new
                {
                    field_id = p.Id,
                    field_name = p.Name,
                    field_type = data?.AuthoringType ?? TypeResolver.GetUnmangledName(p.Type),
                    default_value = data?.DefaultValue,
                    max_length = data?.MaxSize ?? 0
                };
            });

            var traitDataResult = m_CodeRenderer.RenderTemplate(TraitResources.instance.TemplateTraitData, new
            {
                @namespace = TypeResolver.TraitsQualifier,
                name = trait.name,
                fields = fieldsForComponentData,
            });

            var traitResult = m_CodeRenderer.RenderTemplate(TraitResources.instance.TemplateTrait,
                new
                {
                    @namespace = TypeResolver.TraitsQualifier,
                    name = trait.name,
                    fields = fieldsForMonoBehaviour,
                });

            var traitOutputPath = Path.Combine(outputPath, TypeResolver.TraitsQualifier, "Traits");
            var generatedFiles = new []
            {
                SaveToFile(Path.Combine(traitOutputPath, $"{trait.name}{TypeResolver.ComponentDataSuffix}.cs"), traitDataResult),
                SaveToFile(Path.Combine(traitOutputPath, $"{trait.name}.cs"), traitResult),
            };
            return generatedFiles;
        }

        internal static TraitPropertyDescriptorData GetTraitDescriptorData(TraitPropertyDefinition definition)
        {
            var propertyDescriptors = TypeCache.GetTypesDerivedFrom(typeof(TraitPropertyDescriptor<>)).ToList();
            var descriptorType = propertyDescriptors.Find(d => definition.GetType() == d.BaseType?.GetGenericArguments()[0]);

            if (descriptorType != null)
            {
                var methodInfo = descriptorType.GetMethod("GetData");
                if (methodInfo != null)
                {
                    var instance = Activator.CreateInstance(descriptorType);
                    return (TraitPropertyDescriptorData)methodInfo.Invoke(instance, new object[] { definition });
                }
            }

            return null;
        }

        public IEnumerable<string> Generate(string outputPath, EnumDefinition enumDefinition)
        {
            var result = m_CodeRenderer.RenderTemplate(TraitResources.instance.TemplateEnum, new
            {
                @namespace = TypeResolver.EnumsQualifier,
                Name = enumDefinition.name,
                Values = enumDefinition.Elements.Select(p => $"{p.Name} = {p.Id}")
            });

            return new[] { SaveToFile(Path.Combine(outputPath, TypeResolver.TraitsQualifier, "Traits", $"{enumDefinition.name}.cs"), result) };
        }

        public string GenerateAsmRef(string outputPath, string assemblyName)
        {
            var result = m_CodeRenderer.RenderTemplate(
                TraitResources.instance.TemplateAsmRef, new
                {
                    assembly = assemblyName
                });

            return SaveToFile(Path.Combine(outputPath, assemblyName, $"{assemblyName}.asmref"), result);
        }

        internal string GeneratePackage(string outputPath, string displayName)
        {
            string packageName = Path.GetFileName(outputPath);
            outputPath = Path.Combine(outputPath, "package.json");

            var packageJson = m_CodeRenderer.RenderTemplate(TraitResources.instance.TemplatePackage, new
            {
                package_name = packageName,
                display_name = displayName
            });

            return SaveToFile(outputPath, packageJson);
        }

        string SaveToFile(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var originalAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (originalAsset == null || originalAsset.text != text)
            {
                if (!AssetDatabase.IsOpenForEdit(path))
                    AssetDatabase.MakeEditable(path);

                Debug.Log($"Writing out {path}", originalAsset);
                File.WriteAllText(path, text);
                return path;
            }

            return null;
        }
    }
}
