using System.Collections.Generic;

namespace Value
{
    /// <summary>
    ///     Support a by-Value Equality and Unicity where order of the elements that belongs
    ///     to the Unicity/Equality doesn't matter.
    /// </summary>
    /// <typeparam name="T">Type of the elements.</typeparam>
    public abstract class EquatableByValueWithoutOrder<T> : EquatableByValue<T>
    {
        protected override bool EqualsImpl(EquatableByValue<T> other)
        {
            var otherEquatable = (EquatableByValueWithoutOrder<T>)other;

            return EqualsWithoutOrderImpl(otherEquatable);
        }

        public override bool Equals(object obj) => EqualsImpl(obj as EquatableByValue<T>);

        // Force all derived types to implement a specific equal implementation
        protected abstract bool EqualsWithoutOrderImpl(EquatableByValueWithoutOrder<T> obj);

        public override int GetHashCode()
        {
            if (hashCode == Undefined)
            {
                var code = 0;

                // Two instances with same elements added in different order must return the same hashcode
                // Let's compute and sort hashcodes of all elements (always in the same order)
                var sortedHashCodes = new SortedSet<int>();

                foreach (var element in GetAllAttributesToBeUsedForEquality())
                {
                    sortedHashCodes.Add(element.GetHashCode());
                }

                foreach (var elementHashCode in sortedHashCodes)
                {
                    code = (code * 397) ^ elementHashCode;
                }

                // Cache the result in a field
                hashCode = code;
            }

            return hashCode;
        }
    }
}
