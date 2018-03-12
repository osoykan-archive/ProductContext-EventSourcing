// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ItemPrice.cs">
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
    using System.Linq;

    /// <summary>
    /// Price for an item.
    /// </summary>
    public class ItemPrice : Amount
    {
        private readonly string itemName;

        public ItemPrice(string itemName, decimal quantity, Currency currency) : base(quantity, currency)
        {
            this.itemName = itemName;
        }

        public string ItemName { get { return this.itemName; } }

        public override string ToString()
        {
            return string.Format("{0} - price: {1} {2}.", this.ItemName, this.Quantity, this.Currency);
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return base.GetAllAttributesToBeUsedForEquality().Concat(new List<object>() { this.ItemName });
        }
    }
}