// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="AmountTests.cs">
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
    using System;
    using NFluent;
    using NUnit.Framework;
    using Value.Tests.Samples;

    [TestFixture]
    internal class AmountTests
    {
        [Test]
        public void Should_Add_Amount_instances_in_a_Closure_of_operations_style()
        {
            var firstAmount = new Amount(new decimal(1), Currency.Euro);
            var secondAmount = new Amount(new decimal(1), Currency.Euro);

            var thirdAmount = firstAmount.Add(secondAmount);
            Check.That(thirdAmount.Quantity).IsEqualTo(new decimal(2));
        }

        [Test]
        public void Should_throw_exception_when_trying_to_add_Amounts_with_different_currencies()
        {
            var firstAmount = new Amount(new decimal(1), Currency.Euro);
            var secondAmount = new Amount(new decimal(1), Currency.Yuan);

            Check.ThatCode(() => firstAmount.Add(secondAmount))
                .Throws<InvalidOperationException>()
                .WithMessage("Can't add amounts with different currencies: Amount= 1 Euro and other amount= 1 Yuan.");
        }
    }
}