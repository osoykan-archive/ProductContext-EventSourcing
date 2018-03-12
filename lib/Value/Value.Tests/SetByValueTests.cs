// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="SetByValueTests.cs">
// //     Copyright 2016
// //           Thomas PIERRAIN (@tpierrain)    
// //     Licensed under the Apache License, Version 2.0 (the "License");
// //     you may not use this file except in compliance with the License.
// //     You may obtain a copy of the License at
// //         http://www.apache.org/licenses/LICENSE-2.0
// //     Unless required by applicable law or agreed to in writing, software
// //     distributed under the License is distributed on an "AS IS" BASIS,
// //     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //     See the License for the specific language governing permissions and
// //     limitations under the License.b 
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------
namespace Value.Tests.Samples
{
    using System.Collections.Generic;
    using NFluent;
    using NUnit.Framework;

    [TestFixture]
    public class SetByValueTests
    {
        [Test]
        public void Should_consider_two_sets_with_same_items_equals()
        {
            var set1 = new SetByValue<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Achille", "Anton", "Maxime" };

            Check.That(set2).IsEqualTo(set1);
        }

        [Test]
        public void Should_consider_two_sets_with_same_items_in_different_order_equals()
        {
            var set1 = new SetByValue<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Maxime", "Anton", "Achille" };

            Check.That(set2).IsEqualTo(set1);
        }

        [Test]
        public void Should_not_consider_a_classic_hashSet_and_a_HashSetByValue_Equals()
        {
            var set1 = new HashSet<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Achille", "Anton", "Maxime" };

            Check.That(set2).IsNotEqualTo(set1);
        }

        [Test]
        public void Should_consider_equals_two_ValueType_instances_that_aggregates_equivalent_SetByValue()
        {
            var threeCards = new ThreeCards("AS", "QD", "2H");
            var sameThreeCards = new ThreeCards("2H", "QD", "AS");

            Check.That(threeCards).IsEqualTo(sameThreeCards);
        }

        [Test]
        public void Should_consider_Not_equals_two_badly_implemented_ValueType_instances_that_aggregates_equivalent_HashSet()
        {
            var threeCards = new ThreeCardsBadlyImplementedAsValueType("AS", "QD", "2H");
            var sameThreeCards = new ThreeCardsBadlyImplementedAsValueType("2H", "QD", "AS");

            Check.That(threeCards).IsNotEqualTo(sameThreeCards);
        }

        [Test]
        public void Should_change_its_hashcode_everytime_the_set_is_updated()
        {
            var set = new SetByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var firstHashCode = set.GetHashCode();

            set.Add(Card.Parse("3H")); // ---update the set ---
            var afterAddHash = set.GetHashCode();
            Check.That(firstHashCode).IsNotEqualTo(afterAddHash);

            set.Clear();
            var afterClearHash = set.GetHashCode();
            Check.That(afterClearHash).IsNotEqualTo(afterAddHash);

            set.Add(Card.Parse("1C"));
            set.Add(Card.Parse("2C"));
            set.Add(Card.Parse("KD"));
            set.Add(Card.Parse("AC"));

            afterAddHash = set.GetHashCode();

            set.ExceptWith(new List<Card>() { Card.Parse("AC") });
            var afterExceptWithHash = set.GetHashCode();
            Check.That(afterExceptWithHash).IsNotEqualTo(afterAddHash);

            set.IntersectWith(new[] { Card.Parse("2C"), Card.Parse("1C") });
            var afterIntersectWithHash = set.GetHashCode();
            Check.That(afterIntersectWithHash).IsNotEqualTo(afterExceptWithHash);

            Check.That(set).ContainsExactly(Card.Parse("1C"), Card.Parse("2C"));
            set.Remove(Card.Parse("1C"));
            var afterRemoveHash = set.GetHashCode();
            Check.That(afterRemoveHash).IsNotEqualTo(afterIntersectWithHash);

            ((ISet<Card>)set).Add(Card.Parse("AD"));
            ((ISet<Card>)set).Add(Card.Parse("AS"));
            afterAddHash = set.GetHashCode();
            Check.That(afterAddHash).IsNotEqualTo(afterRemoveHash);

            set.UnionWith(new[] { Card.Parse("7H") });
            var afterUnionWithHash = set.GetHashCode();
            Check.That(afterUnionWithHash).IsNotEqualTo(afterAddHash);

            set.SymmetricExceptWith(new[] { Card.Parse("AD") });
            var afterSymetricExceptWithHash = set.GetHashCode();
            Check.That(afterSymetricExceptWithHash).IsNotEqualTo(afterUnionWithHash);
        }

        [Test]
        public void Should_properly_expose_Contains()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.Contains(Card.Parse("QC"))).IsEqualTo(originalSet.Contains(Card.Parse("QC")));
        }

        [Test]
        public void Should_properly_expose_CopyTo()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            var firstCards = new Card[originalSet.Count];
            var secondCards = new Card[originalSet.Count];
            originalSet.CopyTo(firstCards, 0);
            byValueSet.CopyTo(secondCards, 0);

            Check.That(secondCards).ContainsExactly(firstCards);
        }

        [Test]
        public void Should_properly_expose_Count()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.Count).IsEqualTo(originalSet.Count);
        }

        [Test]
        public void Should_properly_expose_IsProperSubsetOf()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.IsProperSubsetOf(new[] { Card.Parse("QC") })).IsEqualTo(originalSet.IsProperSubsetOf(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_properly_expose_IsProperSupersetOf()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.IsProperSupersetOf(new[] { Card.Parse("QC") }))
                .IsTrue()
                .And.IsEqualTo(originalSet.IsProperSupersetOf(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_properly_expose_IsReadOnly()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.IsReadOnly).IsEqualTo(((ICollection<Card>)originalSet).IsReadOnly);
        }

        [Test]
        public void Should_properly_expose_IsSubsetOf()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.IsSubsetOf(new[] { Card.Parse("QC") })).IsEqualTo(originalSet.IsSubsetOf(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_properly_expose_IsSupersetOf()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.IsSupersetOf(new[] { Card.Parse("QC") })).IsEqualTo(originalSet.IsSupersetOf(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_properly_expose_Overlaps()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.Overlaps(new[] { Card.Parse("QC") })).IsEqualTo(originalSet.Overlaps(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_properly_expose_SetEquals()
        {
            var originalSet = new HashSet<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var byValueSet = new SetByValue<Card>(originalSet);

            Check.That(byValueSet.SetEquals(new[] { Card.Parse("QC") })).IsEqualTo(originalSet.SetEquals(new[] { Card.Parse("QC") }));
        }

        [Test]
        public void Should_provide_different_GetHashCode_for_two_different_sets()
        {
            var set1 = new SetByValue<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Hendrix", "De Lucia", "Reinhart" };

            Check.That(set2.GetHashCode()).IsNotEqualTo(set1.GetHashCode());
        }

        [Test]
        public void Should_provide_same_GetHashCode_from_two_sets_with_same_values()
        {
            var set1 = new SetByValue<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Achille", "Anton", "Maxime" };

            Check.That(set2.GetHashCode()).IsEqualTo(set1.GetHashCode());
        }

        [Test]
        public void Should_provide_same_GetHashCode_from_two_sets_with_same_values_in_different_order()
        {
            var set1 = new SetByValue<string> { "Achille", "Anton", "Maxime" };
            var set2 = new SetByValue<string> { "Maxime", "Achille", "Anton" };

            Check.That(set2.GetHashCode()).IsEqualTo(set1.GetHashCode());
        }
    }
}