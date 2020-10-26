using NUnit.Framework;
using Unity.Semantic.Traits;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.Semantic.Traits.CodeGen;

namespace UnityEditor.Semantic.Traits.Tests
{
    class AssetValidatorTestFixture
    {
        protected AssetValidator m_AssetValidator = new AssetValidator();
    }

    [TestFixture]
    class AssetValidatorTests : AssetValidatorTestFixture
    {
        [Test]
        public void TraitPropertiesDuplicationChecked()
        {
            var traitDefinition = ScriptableObject.CreateInstance<TraitDefinition>();
            traitDefinition.Properties = new[]
            {
                new BooleanProperty { Name = "PropertyA", Id = 1 },
                new BooleanProperty { Name = "PropertyA", Id = 2 }
            };

            Assert.IsFalse(m_AssetValidator.IsAssetValid(traitDefinition));

            traitDefinition = ScriptableObject.CreateInstance<TraitDefinition>();
            traitDefinition.Properties = new[]
            {
                new StringProperty { Name = "PropertyA", Id = 1 },
                new StringProperty { Name = "PropertyB", Id = 1 }
            };

            Assert.IsFalse(m_AssetValidator.IsAssetValid(traitDefinition));
        }

        [Test]
        public void EnumValuesDuplicationChecked()
        {
            var enumDefinition = ScriptableObject.CreateInstance<EnumDefinition>();
            enumDefinition.Elements = new[]
            {
                new EnumElementDefinition("ElementA", 1),
                new EnumElementDefinition("ElementB", 1),
            };

            Assert.IsFalse(m_AssetValidator.IsAssetValid(enumDefinition));

            enumDefinition = ScriptableObject.CreateInstance<EnumDefinition>();
            enumDefinition.Elements = new[]
            {
                new EnumElementDefinition("ElementA", 1),
                new EnumElementDefinition("ElementA", 2),
            };

            Assert.IsFalse(m_AssetValidator.IsAssetValid(enumDefinition));
        }
    }
}
