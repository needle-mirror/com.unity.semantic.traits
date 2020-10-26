using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Semantic.Traits.Queries.Tests.Unit
{
    [Category("Unit")]
    class HybridFilterTests
    {
        SemanticObject m_SemanticObject;
        SemanticObject m_OtherSemanticObject;
        GameObject m_StandardGameObject;

        [SetUp]
        public void Setup()
        {
            var gameObject = new GameObject("TBO", typeof(SemanticObject));
            gameObject.transform.position = Vector3.forward * 5f;
            m_SemanticObject = gameObject.GetComponent<SemanticObject>();

            var otherGameObject = new GameObject("OtherTBO", typeof(SemanticObject));
            otherGameObject.transform.position = Vector3.forward * 10f;
            m_OtherSemanticObject = otherGameObject.GetComponent<SemanticObject>();

            m_StandardGameObject = new GameObject();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_SemanticObject)
                Object.Destroy(m_SemanticObject.gameObject);

            if (m_OtherSemanticObject)
                Object.Destroy(m_OtherSemanticObject.gameObject);

            if (m_StandardGameObject)
                Object.Destroy(m_StandardGameObject);
        }

        [UnityTest]
        public IEnumerator DistanceFilterReturnsSubset()
        {
            while (m_SemanticObject.EntityManager == default)
                yield return null;

            var entityManager = m_SemanticObject.EntityManager;
            using (var query = new TraitQuery(entityManager).WithFilter(new DistanceFilter { Radius = 5f }))
            using (var entityQuery = entityManager.CreateEntityQuery(typeof(SemanticObjectData)))
            {
                Assert.AreEqual(2, entityQuery.CalculateEntityCount());
                Assert.AreEqual(1, query.Count());
                Assert.IsTrue(query.First() == m_SemanticObject.Entity);
            }
        }

        [UnityTest]
        public IEnumerator DistanceFilterReturnsNone()
        {
            while (m_SemanticObject.EntityManager == default)
                yield return null;

            var entityManager = m_SemanticObject.EntityManager;
            using (var query = new TraitQuery(entityManager).WithFilter(new DistanceFilter { Radius = 1f }))
            using (var entityQuery = entityManager.CreateEntityQuery(typeof(SemanticObjectData)))
            {
                Assert.AreEqual(2, entityQuery.CalculateEntityCount());
                Assert.AreEqual(0, query.Count());
            }
        }

        [UnityTest]
        public IEnumerator SpecificSemanticObjectFilterReturnsSubset()
        {
            while (m_SemanticObject.EntityManager == default)
                yield return null;

            var entityManager = m_SemanticObject.EntityManager;
            using (var query = new TraitQuery(entityManager).WithFilter(new SpecificSemanticObjectFilter() { ReferenceObject = m_SemanticObject }))
            using (var entityQuery = entityManager.CreateEntityQuery(typeof(SemanticObjectData)))
            {
                Assert.AreEqual(2, entityQuery.CalculateEntityCount());
                Assert.AreEqual(1, query.Count());
            }
        }
    }
}
