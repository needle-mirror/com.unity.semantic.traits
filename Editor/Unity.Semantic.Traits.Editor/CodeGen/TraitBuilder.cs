using System;
using System.IO;
using System.Linq;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using UnityEditor.Semantic.Traits.Utility;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.CodeGen
{
    /// <summary>
    /// Class that handles automatic and manual build of trait assets into generated-code
    /// </summary>
    public static class TraitBuilder
    {
        /// <summary>
        /// An event triggered after enumeration assets are built
        /// </summary>
        public static event Action<EnumDefinition> enumerationBuilt;

        /// <summary>
        /// An event triggered after trait assets are built
        /// </summary>
        public static event Action<TraitDefinition> traitBuilt;

        /// <summary>
        /// An event triggered after a definition was deleted
        /// </summary>
        public static event Action<string> cleanedUp;

        /// <summary>
        /// An event triggered after a definition was moved
        /// </summary>
        public static event Action<string, string> moved;

        const string k_BuildMenuTitle = "Semantic/Traits/Build";

        const string k_BuildPath = "Packages";
        static readonly string k_GeneratedPackagesPath = Path.Combine(k_BuildPath, TypeResolver.TraitsQualifier.ToLower());

        [InitializeOnLoadMethod]
        static void AttachAutoBuild()
        {
            if (!Application.isBatchMode)
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            if (SemanticTraitsPreferences.GetOrCreatePreferences().BuildOnEnteredPlayMode)
            {
                if (playMode == PlayModeStateChange.EnteredPlayMode && EditorApplication.isCompiling)
                {
                    // If we're still compiling the domain will reload and cause an error, so as a safeguard simply exit play mode
                    EditorApplication.ExitPlaymode();
                    return;
                }

                if (playMode == PlayModeStateChange.ExitingEditMode)
                {
                    var traitsAssembly = CompilationPipeline.GetAssemblies(AssembliesType.Player).FirstOrDefault(a =>
                        a.name == TypeResolver.TraitsQualifier);
                    DateTime lastBuildTime = DateTime.MinValue;
                    if (traitsAssembly != null)
                        lastBuildTime = File.GetLastWriteTimeUtc(traitsAssembly.outputPath);

                    var assetTypes = new[]
                    {
                        nameof(TraitDefinition),
                        nameof(EnumDefinition),
                    };

                    var filter = string.Join(" ", assetTypes.Select(t => $"t:{t}"));
                    var assets = AssetDatabase.FindAssets(filter);
                    foreach (var a in assets)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(a);
                        var assetLastWriteTime = File.GetLastWriteTimeUtc(assetPath);
                        if (assetLastWriteTime.CompareTo(lastBuildTime) > 1)
                        {
                            Debug.Log($"Rebuilding Semantic Traits assemblies because {assetPath} is newer");
                            EditorApplication.ExitPlaymode();
                            Build();
                            CompilationPipeline.compilationFinished += context =>  EditorApplication.EnterPlaymode();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trigger a code-gen build of all trait assets
        /// </summary>
        [MenuItem(k_BuildMenuTitle)]
        public static void Build()
        {
            TraitAssetDatabase.Refresh();

            var validator = new AssetValidator();
            validator.errorLogged += LogError;

            if (!validator.TraitAssetsAreValid())
            {
                Debug.LogError("All trait asset errors have to be fixed in order to generate the Semantic Traits assembly.");
                return;
            }

            var codeGenerator = new CodeGenerator();
            try
            {
                AssetDatabase.StartAssetEditing();
                codeGenerator.GeneratePackage(k_GeneratedPackagesPath, TypeResolver.TraitsDisplayName);
                codeGenerator.GenerateAsmRef(k_GeneratedPackagesPath, TypeResolver.TraitsQualifier);

                foreach (var enumeration in TraitAssetDatabase.EnumDefinitions)
                {
                    BuildInternalBatch(enumeration, codeGenerator);
                }

                foreach (var trait in TraitAssetDatabase.TraitDefinitions)
                {
                    BuildInternalBatch(trait, codeGenerator);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        [MenuItem(k_BuildMenuTitle, true)]
        static bool BuildMenuValidate()
        {
            return !EditorApplication.isCompiling;
        }

        internal static void Build(EnumDefinition enumDefinition)
        {
            Debug.Log("Build " + enumDefinition.name);
            BuildInternal(enumDefinition);
        }

        internal static void Build(TraitDefinition traitDefinition)
        {
            Debug.Log("Build " + traitDefinition.name);
            BuildInternal(traitDefinition);
        }

        static void LogError(string errorMessage, ScriptableObject asset)
        {
            Debug.LogError($"<b>{AssetDatabase.GetAssetPath(asset)}</b>: {errorMessage}", asset);
        }

        static void BuildInternal<T>(T definition)
        {
            var validator = new AssetValidator();
            validator.errorLogged += LogError;

            if (((IAssetValidator<T>)validator).IsAssetValid(definition))
            {
                var codeGenerator = new CodeGenerator();
                codeGenerator.GeneratePackage(k_GeneratedPackagesPath, TypeResolver.TraitsDisplayName);
                codeGenerator.GenerateAsmRef(k_GeneratedPackagesPath, TypeResolver.TraitsQualifier);

                BuildInternalBatch(definition, codeGenerator);
            }
        }

        static void BuildInternalBatch<T>(T definition, CodeGenerator codeGenerator)
        {
            var definitionGenerator = (ICodeGenerator<T>)codeGenerator;
            var filePaths = definitionGenerator.Generate(k_GeneratedPackagesPath, definition);

            foreach (var filePath in filePaths)
            {
                if (!string.IsNullOrEmpty(filePath))
                    AssetDatabase.ImportAsset(filePath);
            }

            RaiseBuiltEvent(definition);
        }

        static void RaiseBuiltEvent<T>(T definition)
        {
            if (definition is TraitDefinition traitDefinition)
                traitBuilt?.Invoke(traitDefinition);
            else if (definition is EnumDefinition enumDefinition)
                enumerationBuilt?.Invoke(enumDefinition);
        }

        internal static void CleanUp(string deletedFilePath)
        {
            if (!Directory.Exists(k_GeneratedPackagesPath))
                return;

            var definition = TypeResolver.ToTypeNameCase(Path.GetFileNameWithoutExtension(deletedFilePath));

            var files = Directory.GetFiles(k_GeneratedPackagesPath, $"{definition}*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                switch (Path.GetFileNameWithoutExtension(file))
                {
                    case var traitDataFile when traitDataFile == $"{definition}{TypeResolver.ComponentDataSuffix}":
                    case var traitFile when traitFile == $"{definition}":
                        break;

                    default:
                        continue;
                }

                if (!AssetDatabase.IsOpenForEdit(file))
                    AssetDatabase.MakeEditable(file);

                Debug.Log($"Cleaning up moved/deleted definition {file}");
                AssetDatabase.DeleteAsset(file);
            }

            cleanedUp?.Invoke(definition);
        }

        internal static void Move(string movedFromPath, string movedToPath)
        {
            var movedFromDefinition = TypeResolver.ToTypeNameCase(Path.GetFileNameWithoutExtension(movedFromPath));
            var movedToDefinition = TypeResolver.ToTypeNameCase(Path.GetFileNameWithoutExtension(movedToPath));

            var files = Directory.GetFiles(k_GeneratedPackagesPath, $"{movedFromDefinition}*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string newFilePath;
                switch (Path.GetFileNameWithoutExtension(file))
                {
                    case var traitDataFile when traitDataFile == $"{movedFromDefinition}{TypeResolver.ComponentDataSuffix}":
                        newFilePath = file.Replace($"{movedFromDefinition}{TypeResolver.ComponentDataSuffix}.cs", $"{movedToDefinition}{TypeResolver.ComponentDataSuffix}.cs");
                        break;

                    case var traitFile when traitFile == $"{movedFromDefinition}":
                        newFilePath = file.Replace($"{movedFromDefinition}.cs", $"{movedToDefinition}.cs");
                        break;

                    default:
                        continue;
                }

                var oldFileName = Path.GetFileNameWithoutExtension(file);
                var newFileName = Path.GetFileNameWithoutExtension(newFilePath);

                // File name is not changing, so no move necessary
                if (oldFileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!AssetDatabase.IsOpenForEdit(file))
                    AssetDatabase.MakeEditable(file);

                var errorMessage = AssetDatabase.ValidateMoveAsset(file, newFilePath);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    Debug.Log($"Moved definition {oldFileName} to {newFileName}");
                    AssetDatabase.MoveAsset(file, newFilePath);
                }
                else
                {
                    Debug.LogError(errorMessage);
                }
            }

            moved?.Invoke(movedFromDefinition, movedToDefinition);
        }
    }
}
