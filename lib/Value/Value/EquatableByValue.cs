// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EquatableByValue.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Value
{
    /// <summary>
    ///     Support a by-Value Equality and Unicity.
    /// </summary>
    /// <remarks>
    ///     This latest implementation has been inspired from Scott Millett's book (Patterns, Principles, and Practices of
    ///     Domain-Driven Design).
    /// </remarks>
    /// <typeparam name="T">Type of the elements.</typeparam>
    public abstract class EquatableByValue<T> : IEquatable<T>
    {
        protected const int Undefined = -1;

        protected volatile int hashCode = Undefined;

        public bool Equals(T other)
        {
            var otherEquatable = other as EquatableByValue<T>;
            if (otherEquatable == null)
            {
                return false;
            }

            return EqualsImpl(otherEquatable);
        }

        protected void ResetHashCode()
        {
            hashCode = Undefined;
        }

        public static bool operator ==(EquatableByValue<T> x, EquatableByValue<T> y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(EquatableByValue<T> x, EquatableByValue<T> y) => !(x == y);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            T other;

            try
            {
                // we use a static cast here since we can't use the 'as' operator for structs and other value type primitives
                other = (T)obj;
            }
            catch (InvalidCastException)
            {
                return false;
            }

            return Equals(other);
        }

        protected abstract IEnumerable<object> GetAllAttributesToBeUsedForEquality();

        protected virtual bool EqualsImpl(EquatableByValue<T> otherEquatable) => GetAllAttributesToBeUsedForEquality().SequenceEqual(otherEquatable.GetAllAttributesToBeUsedForEquality());

        public override int GetHashCode()
        {
            // Implementation where orders of the elements matters.
            if (hashCode == Undefined)
            {
                var code = 0;

                foreach (var attribute in GetAllAttributesToBeUsedForEquality())
                {
                    code = (code * 397) ^ (attribute == null ? 0 : attribute.GetHashCode());
                }

                hashCode = code;
            }

            return hashCode;
        }
    }
}
