using System;
using System.IO;
using System.Linq;
using Unity.Semantic.Traits;
using UnityEditor.Semantic.Traits.CodeGen;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.Importers
{
    class DefinitionImporter : AssetPostprocessor
    {
        static bool IsDefinitionAsset(string filePath)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(filePath);
            return Path.GetExtension(filePath) == ".asset"
                && (mainAssetType == null || mainAssetType == typeof(TraitDefinition) || mainAssetType == typeof(EnumDefinition));
        }

        static void BuildFromAsset(string filePath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(filePath);
            if (asset is TraitDefinition traitDefinition)
                TraitBuilder.Build(traitDefinition);
            else if (asset is EnumDefinition enumDefinition)
                TraitBuilder.Build(enumDefinition);
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    AssetDatabase.StartAssetEditing();

                    deletedAssets.Where(IsDefinitionAsset)
                        .ForEach(TraitBuilder.CleanUp);

                    for (var i = 0; i < movedAssets.Length; i++)
                    {
                        var movedFromPath = movedFromAssetPaths[i];
                        var movedToPath = movedAssets[i];

                        if (IsDefinitionAsset(movedFromPath))
                            TraitBuilder.Move(movedFromPath, movedToPath);
                    }

                    if (SemanticTraitsPreferences.GetOrCreatePreferences().BuildOnAssetChanged)
                    {
                        importedAssets.Where(IsDefinitionAsset)
                            .Where(a => !movedAssets.Contains(a))
                            .ForEach(BuildFromAsset);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                }
            };
        }
    }
}
