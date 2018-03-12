// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Card.cs">
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
    /// One of the 52 cards from a poker deck. Every card has a value, a suit and a name.
    /// </summary>
    public class Card : ValueType<Card>, IComparable<Card>
    {
        // TODO: make cards ranking dependant on poker game type (high-low, omaha, etc)?
        private const int AceValue = 14;

        private const int JackValue = 11;

        private const int KingValue = 13;

        private const int QueenValue = 12;

        private Card(string cardDescription, int value, Suit suit)
        {
            this.CardDescription = cardDescription;
            this.Value = value;
            this.Suit = suit;
        }

        public string CardDescription { get; private set; }

        public string DisplayName
        {
            get
            {
                var displayName = this.Value.ToString();

                if (this.Value == AceValue)
                {
                    displayName = "Ace";
                }
                else if (this.Value == KingValue)
                {
                    displayName = "King";
                }
                else if (this.Value == QueenValue)
                {
                    displayName = "Queen";
                }
                else if (this.Value == JackValue)
                {
                    displayName = "Jack";
                }

                return displayName;
            }
        }

        public Suit Suit { get; private set; }

        public int Value { get; private set; }

        public static Card Parse(string cardDescription)
        {
            return new Card(cardDescription, ExtractValue(cardDescription), ExtractSuit(cardDescription));
        }

        public int CompareTo(Card other)
        {
            return this.Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return $"{this.CardDescription}";
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object>() { this.Value, this.CardDescription, this.DisplayName, this.Suit };
        }

        private static Suit ExtractSuit(string cardDescription)
        {
            switch (cardDescription[1])
            {
                case 'C':
                    return Suit.C;

                case 'D':
                    return Suit.D;

                case 'H':
                    return Suit.H;

                case 'S':
                    return Suit.S;

                default:
                    throw new InvalidOperationException();
            }
        }

        private static int ExtractValue(string cardDescription)
        {
            int result;
            var valueOrFace = cardDescription[0];
            if (!int.TryParse(valueOrFace.ToString(), out result))
            {
                switch (valueOrFace)
                {
                    case 'T':
                        result = 10;
                        break;

                    case 'J':
                        return JackValue;

                        break;

                    case 'Q':
                        return QueenValue;

                        break;

                    case 'K':
                        return KingValue;

                        break;

                    case 'A':
                        return AceValue;

                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            return result;
        }
    }
}