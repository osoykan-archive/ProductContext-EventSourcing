// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ValueTypeTests.cs">
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
    public class ValueTypeTests
    {
        [Test]
        public void Should_distinguish_a_derived_instance_from_a_base_value_type_with_same_sub_common_values()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);
            var itemPrice = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);

            Check.That(amount.Equals((object)itemPrice)).IsFalse();
        }

        [Test]
        public void Should_distinguish_a_value_type_from_null()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);

            Check.That(amount).IsNotEqualTo(null);
            Check.That(amount == null).IsFalse();
            Check.That(amount != null).IsTrue();
            Check.That(amount.Equals((object)null)).IsFalse();
            Check.That(amount.Equals(null)).IsFalse();
        }

        [Test]
        public void Should_distinguish_a_value_type_instance_from_WTF_is_this_other_type()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);
            Check.That(amount.Equals(Card.Parse("QS"))).IsFalse();
        }

        [Test]
        public void Should_distinguish_a_value_type_instances_from_one_of_its_derived_type_instance()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);
            var itemPrice = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);

            Check.That((Amount)itemPrice).IsNotEqualTo(amount);
            Check.That(itemPrice == amount).IsFalse();
            Check.That(itemPrice.Equals(amount)).IsFalse();
            Check.That(itemPrice.Equals((object)amount)).IsFalse();
        }

        [Test]
        public void Should_distinguish_properly_implemented_derived_value_type_instances()
        {
            var itemPrice = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);
            var itemPriceWithOtherValues = new ItemPrice("not a movie", new decimal(50.3), Currency.Dollar);
            var otherPriceInstanceWithSameValues = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);

            Check.That(itemPrice).IsNotEqualTo(itemPriceWithOtherValues);
            Check.That(otherPriceInstanceWithSameValues).IsEqualTo(itemPrice);
        }

        [Test]
        public void Should_find_equals_2_different_instances_of_ValueType_with_different_values_when_equality_is_badly_implemented()
        {
            var itemPrice = new ItemPriceWithBadImplementationForEqualityAndUnicity("movie", new decimal(50.3), Currency.Dollar);
            var differentItemPriceValue = new ItemPriceWithBadImplementationForEqualityAndUnicity("not a movie", new decimal(50.3), Currency.Dollar);

            Check.That(itemPrice).IsEqualTo(differentItemPriceValue); // because bad implementation of equality
        }

        [Test]
        public void Should_find_equals_2_different_instances_of_ValueType_with_same_values()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);
            var sameAmountValue = new Amount(new decimal(50.3), Currency.Dollar);

            Check.That(amount).IsEqualTo(sameAmountValue);
        }

        [Test]
        public void Should_retrieve_properly_implemented_derived_value_type_with_same_values_in_a_set()
        {
            var itemPrice = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);
            var itemPriceWithSameValues = new ItemPrice("movie", new decimal(50.3), Currency.Dollar);

            var set = new HashSet<ItemPrice> { itemPrice };

            Check.That(set).ContainsExactly(itemPriceWithSameValues);
        }

        [Test]
        public void Should_retrieve_value_type_with_same_values_in_a_set()
        {
            var amount = new Amount(new decimal(50.3), Currency.Dollar);
            var amountWithSameValues = new Amount(new decimal(50.3), Currency.Dollar);

            var set = new HashSet<Amount> { amount };

            Check.That(set).ContainsExactly(amountWithSameValues);
        }

        [Test]
        public void Should_retrieve_wrong_instances_from_a_set_when_unicity_is_badly_implemented_on_a_derived_value_type()
        {
            var itemPrice = new ItemPriceWithBadImplementationForEqualityAndUnicity("movie", new decimal(50.3), Currency.Dollar);
            var itemPriceWithOtherValues = new ItemPriceWithBadImplementationForEqualityAndUnicity("XXX movie", new decimal(50.3), Currency.Dollar);

            var set = new HashSet<ItemPriceWithBadImplementationForEqualityAndUnicity> { itemPrice };

            Check.That(set).ContainsExactly(itemPriceWithOtherValues); // because bad implementation of unicity
        }

        [Test]
        public void Should_work_with_null_when_using_equality_operator()
        {
            Amount amount1 = null;
            Amount amount2 = null;

            Check.That(amount1 == amount2).IsTrue();
        }
    }
}