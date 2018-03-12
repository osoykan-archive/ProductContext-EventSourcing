// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ListByValueTests.cs">
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
namespace Value.Tests
{
    using System.Collections.Generic;
    using NFluent;
    using NUnit.Framework;
    using Value.Tests.Samples;

    [TestFixture]
    public class ListByValueTests
    {
        [Test]
        public void Should_accept_list_as_constructor_argument()
        {
            var list = new List<int>() { 1, 2, 3 };

            Check.That(new ListByValue<int>(list)).ContainsExactly(1, 2, 3);
        }

        [Test]
        public void Should_change_its_hashcode_everytime_the_list_is_updated()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };

            var previousHashcode = list.GetHashCode();
            list.Add(Card.Parse("3H")); // ---update the list ---
            var currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);


            previousHashcode = list.GetHashCode();
            list.Remove(Card.Parse("QC")); // ---update the list ---
            currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = list.GetHashCode();
            list.Clear(); // ---update the list ---
            currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = list.GetHashCode();
            list.Insert(0, Card.Parse("AS")); // ---update the list ---
            currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = list.GetHashCode();
            list[0] = Card.Parse("QH");
            currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = list.GetHashCode();
            list.RemoveAt(0);
            currentHashcode = list.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);
        }

        [Test]
        public void Should_consider_Equals_two_instances_with_same_reference_types_elements_in_same_order()
        {
            var firstElement = new object();
            var secondElement = new object();

            var listA = new ListByValue<object>() { firstElement, secondElement };
            var listB = new ListByValue<object>() { firstElement, secondElement };

            Check.That(listA).IsEqualTo(listB).And.ContainsExactly(firstElement, secondElement);
        }

        [Test]
        public void Should_consider_Equals_two_instances_with_same_ValueType_elements_in_same_order()
        {
            var listA = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var listB = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };

            Check.That(listA).IsEqualTo(listB).And.ContainsExactly(Card.Parse("QC"), Card.Parse("TS"));
        }

        [Test]
        public void Should_consider_Not_Equals_two_instances_with_same_elements_in_different_order()
        {
            var firstElement = new object();
            var secondElement = new object();

            var listA = new ListByValue<object>() { firstElement, secondElement };
            var listB = new ListByValue<object>() { secondElement, firstElement };

            Check.That(listB).IsNotEqualTo(listA).And.ContainsExactly(secondElement, firstElement);
        }

        [Test]
        public void Should_properly_expose_Count()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            Check.That(list.Count).IsEqualTo(2);

            list.Add(Card.Parse("4D"));
            Check.That(list.Count).IsEqualTo(3);
        }

        [Test]
        public void Should_properly_expose_Contains()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            Check.That(list.Contains(Card.Parse("TS"))).IsTrue();
            Check.That(list.Contains(Card.Parse("4D"))).IsFalse();
        }

        [Test]
        public void Should_properly_expose_CopyTo()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            var cards = new Card[5];
            list.CopyTo(cards, 2);

            Check.That(cards).ContainsExactly(null, null, Card.Parse("QC"), Card.Parse("TS"), null);
        }

        [Test]
        public void Should_properly_expose_IEnumerable()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };

            foreach (var card in list)
            {
                Check.That(card == Card.Parse("QC") || card == Card.Parse("TS")).IsTrue();
            }
        }

        [Test]
        public void Should_properly_expose_indexer()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            Check.That(list[0]).IsEqualTo(Card.Parse("QC"));
            Check.That(list[1]).IsEqualTo(Card.Parse("TS"));
        }

        [Test]
        public void Should_properly_expose_IndexOf()
        {
            var list = new ListByValue<Card>() { Card.Parse("QC"), Card.Parse("TS") };
            Check.That(list.IndexOf(Card.Parse("QC"))).IsEqualTo(0);
            Check.That(list.IndexOf(Card.Parse("TS"))).IsEqualTo(1);
        }

        [Test]
        public void Should_properly_expose_IsReadOnly()
        {
            var originalList = new List<int>() { 0, 1, 2 };
            var listByValue = new ListByValue<int>(originalList);

            ICollection<int> original = originalList;
            Check.That(listByValue.IsReadOnly).IsEqualTo(original.IsReadOnly);
        }
    }
}