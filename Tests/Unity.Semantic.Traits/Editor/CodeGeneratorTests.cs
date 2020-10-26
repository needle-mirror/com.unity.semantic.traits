using System.IO;
using NUnit.Framework;
using Unity.Semantic.Traits;
using Unity.Semantic.Traits.Utility;
using UnityEditor.Semantic.Traits.CodeGen;
using UnityEditor.Semantic.Traits.Utility;
using UnityEngine;

namespace UnityEditor.Semantic.Traits.Tests
{
    class CodeGeneratorTestFixture
    {
        static readonly string k_AssetsPath = Path.Combine("Assets", "Temp");
        static readonly string k_TraitAssetsPath = Path.Combine(k_AssetsPath, "Traits");
        static readonly string k_EnumAssetsPath = Path.Combine(k_AssetsPath, "Enums");
        protected static readonly string k_OutputPath = Path.Combine("Temp", "TestTraitAssembly");

        protected CodeGenerator m_CodeGenerator = new CodeGenerator();

        protected TraitDefinition m_TraitDefinition;
        protected EnumDefinition m_EnumDefinition;

        [OneTimeSetUp]
        public virtual void Setup()
        {
            m_TraitDefinition = ScriptableObject.CreateInstance<TraitDefinition>();
            m_TraitDefinition.Properties = new TraitPropertyDefinition[]
            {
                new BooleanProperty { Name = "PropertyA", Id = 1 },
                new IntProperty { Name = "PropertyB", Id = 2 },
                new FloatProperty { Name = "PropertyC", Id = 3 },
                new StringProperty { Name = "PropertyD", Id = 4 },
                new Vector2Property { Name = "PropertyE", Id = 5 },
                new Vector3Property { Name = "PropertyF", Id = 6 },
            };
            SaveAsset(m_TraitDefinition, Path.Combine(k_TraitAssetsPath, "TraitA.asset"));

            m_EnumDefinition = ScriptableObject.CreateInstance<EnumDefinition>();
            m_EnumDefinition.Elements = new[]
            {
                new EnumElementDefinition("ValueA", 1)
            };
            SaveAsset(m_EnumDefinition, Path.Combine(k_EnumAssetsPath, "EnumA.asset"));
        }

        void SaveAsset(Object asset, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(asset, path);
        }

        [OneTimeTearDown]
        public virtual void TearDown()
        {
            CleanupFiles();
            AssetDatabase.Refresh();
            TraitAssetDatabase.Refresh();
        }

        static void CleanupFiles()
        {
            if (Directory.Exists(k_AssetsPath))
                Directory.Delete(k_AssetsPath, true);

            if (Directory.Exists(k_OutputPath))
                Directory.Delete(k_OutputPath, true);
        }
    }

    [TestFixture]
    class CodeGeneratorTests : CodeGeneratorTestFixture
    {
        [Test]
        public void TraitIsGenerated()
        {
            m_CodeGenerator.Generate(k_OutputPath, m_TraitDefinition);
            Assert.IsTrue(File.Exists(Path.Combine(k_OutputPath, TypeResolver.TraitsQualifier, "Traits", "TraitA.cs")));
            Assert.IsTrue(File.Exists(Path.Combine(k_OutputPath, TypeResolver.TraitsQualifier, "Traits", $"TraitA{TypeResolver.ComponentDataSuffix}.cs")));
        }

        [Test]
        public void EnumIsGenerated()
        {
            m_CodeGenerator.Generate(k_OutputPath, m_EnumDefinition);
            Assert.IsTrue(File.Exists(Path.Combine(k_OutputPath, TypeResolver.TraitsQualifier, "Traits", "EnumA.cs")));
        }
    }
}
