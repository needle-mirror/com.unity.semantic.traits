using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Semantic.Traits.Queries.Tests.Unit
{
    [Category("Unit")]
    class TraitQueryTests : ECSTestsFixture
    {
        struct Alpha : ITraitData
        {
        }

        struct Beta : ITraitData
        {
        }

        struct Gamma : ITraitData
        {
            public float Value;
        }

        [Test]
        public void DefaultConstructorUsageThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using (var query = new TraitQuery())
                {
                    Assert.AreEqual(0, query.Count());
                }
            });
        }

        [Test]
        public void EmptyQueryReturnsAllEntities()
        {
            var unity = m_Manager.CreateEntity(typeof(SemanticObjectData));
            using (var query = new TraitQuery(m_Manager))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(1, entities.Length);
                Assert.AreEqual(1, query.Count());
                Assert.IsTrue(query.First() == unity);
            }
        }

        [Test]
        public void InvalidatedQueryThrows()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha)))
                using (var alternateQuery = query.Or().WithTraitTypes(typeof(Beta)))
                using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
                {
                    Assert.AreEqual(2, entities.Length);
                    Assert.AreEqual(1, query.Count()); // This should throw
                    Assert.AreEqual(2, alternateQuery.Count());
                }
            });
        }

        [Test]
        public void QueryUseAfterDisposeThrows()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            Assert.Throws<InvalidOperationException>(() =>
            {
                var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha));
                using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
                {
                    Assert.AreEqual(2, entities.Length);
                    Assert.AreEqual(1, query.Count());
                }
                query.Dispose();
                Assert.AreEqual(1, query.Count());
            });
        }


        [Test]
        public void ClonedQueryUsesSeparateResults()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha)))
            using (var alternateQuery = query.Clone().Or().WithTraitTypes(typeof(Beta)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(1, query.m_Query.m_QueryGroups.Count);
                Assert.AreEqual(2, alternateQuery.m_Query.m_QueryGroups.Count);
                Assert.AreEqual(1, query.Count());
                Assert.AreEqual(2, alternateQuery.Count());
            }
        }

        [Test]
        public void QueryResetAllowsAnotherQuery()
        {
            var alpha = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(1, query.Count());

                m_Manager.DestroyEntity(alpha);
                query.Reset();
                Assert.AreEqual(0, query.Count());
            }
        }

        [Test]
        public void WorldChangesDontUpdateQueryIfNotReset()
        {
            var alpha = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(1, query.Count());

                m_Manager.DestroyEntity(alpha);
                Assert.AreEqual(1, query.Count());
            }
        }

        [Test]
        public void WithTraitTypeFilterReturnsSubset()
        {
            var alpha = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Alpha)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(1, query.Count());
                Assert.IsTrue(query.First() == alpha);
            }
        }

        [Test]
        public void WithTraitTypeFilterReturnsNone()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes(typeof(Gamma)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2,entities.Length);
                Assert.AreEqual(0, query.Count());
            }
        }

        [Test]
        public void WithoutTraitTypeFilterReturnsSubset()
        {
            var alpha = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));

            using(var query = new TraitQuery(m_Manager).WithoutTraitTypes(typeof(Beta)))
            using(var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(1,query.Count());
                Assert.IsTrue(query.First() == alpha);
            }
        }

        [Test]
        public void OrFilterReturnsNone()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Gamma));

            using (var query = new TraitQuery(m_Manager).WithTraitTypes<Alpha>().Or().WithTraitTypes<Beta>())
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(1,entities.Length);
                Assert.AreEqual(0, query.Count());
            }
        }

        [Test]
        public void OrFilterReturnsSubset()
        {
            var alpha = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            var beta = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Gamma));

            using(var query = new TraitQuery(m_Manager).WithTraitTypes<Alpha>().Or().WithTraitTypes<Beta>())
            using(var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(3, entities.Length);
                Assert.AreEqual(2,query.Count());
                Assert.IsTrue(query.Contains(alpha));
                Assert.IsTrue(query.Contains(beta));
            }
        }

        [Test]
        public void WithoutTraitTypeFilterReturnsNone()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));

            using (var query = new TraitQuery(m_Manager).WithoutTraitTypes(typeof(Alpha)))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(0, query.Count());
            }
        }

        [Test]
        public void TraitConditionReturnsSubset()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));
            var gamma = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Gamma));
            var gammaData = m_Manager.GetComponentData<Gamma>(gamma);
            gammaData.Value = 3.14f;
            m_Manager.SetComponentData(gamma, gammaData);

            using (var query = new TraitQuery(m_Manager).Where(new Gamma { Value = (float)Math.PI },
                (ref Gamma other, ref Gamma reference) => Math.Abs(other.Value - reference.Value) <= 0.01f))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(3, entities.Length);
                Assert.AreEqual(1, query.Count());
                Assert.IsTrue(query.First() == gamma);
            }
        }

        [Test]
        public void TraitConditionReturnsNone()
        {
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Alpha));
            m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Beta));
            var gamma = m_Manager.CreateEntity(typeof(SemanticObjectData), typeof(Gamma));
            var gammaData = m_Manager.GetComponentData<Gamma>(gamma);
            gammaData.Value = 3f;
            m_Manager.SetComponentData(gamma, gammaData);

            using (var query = new TraitQuery(m_Manager).Where(new Gamma { Value = (float)Math.PI },
                (ref Gamma other, ref Gamma reference) => Math.Abs(other.Value - reference.Value) <= 0.01f))
            using (var entities = m_Manager.GetAllEntities(Allocator.TempJob))
            {
                Assert.AreEqual(3, entities.Length);
                Assert.AreEqual(0, query.Count());
            }
        }
    }
}
