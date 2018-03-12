// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Amount.cs">
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A quantity of money.
    /// </summary>
    /// <remarks>Value type.</remarks>
    public class Amount : ValueType<Amount>
    {
        private readonly Currency currency;

        private readonly decimal quantity;

        public Amount(decimal quantity, Currency currency)
        {
            this.quantity = quantity;
            this.currency = currency;
        }

        public Currency Currency { get { return this.currency; } }

        public decimal Quantity { get { return this.quantity; } }

        public Amount Add(Amount otherAmount)
        {
            if (this.Currency != otherAmount.Currency)
            {
                throw new InvalidOperationException(
                    string.Format("Can't add amounts with different currencies: Amount= {0} and other amount= {1}.", this, otherAmount));
            }

            return new Amount(this.Quantity + otherAmount.Quantity, this.Currency);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.Quantity, this.Currency);
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object>() { this.quantity, this.currency };
        }
    }
}