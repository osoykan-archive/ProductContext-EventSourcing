// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ThreeCards.cs">
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

    /// <summary>
    ///     Proper implementation of a ThreeeCards ValueType since the order of the cards doesn't matter during
    ///     Equality.
    /// </summary>
    public class ThreeCards : ValueType<ThreeCards>
    {
        private readonly HashSet<Card> cards;

        public ThreeCards(string card1Description, string card2Description, string card3Description)
        {
            this.cards = new HashSet<Card>();

            this.cards.Add(Card.Parse(card1Description));
            this.cards.Add(Card.Parse(card2Description));
            this.cards.Add(Card.Parse(card3Description));
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object> { new SetByValue<Card>(this.cards) };
        }
    }
}