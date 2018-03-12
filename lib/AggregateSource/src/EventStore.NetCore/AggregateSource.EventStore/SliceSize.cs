using System;

namespace AggregateSource.EventStore
{
    /// <summary>
    ///     Represent the size of a slice to read from the event store.
    /// </summary>
    public struct SliceSize
    {
        private readonly int _value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SliceSize" /> struct.
        /// </summary>
        /// <param name="value">The size of the slice to read from the event store.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Throw when the value is less than 1.</exception>
        public SliceSize(int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("value", Resources.SliceSize_GreaterThanOne);
            }
            _value = value;
        }

        /// <summary>
        ///     Converts the specified size to an integer.
        /// </summary>
        /// <param name="size">The slice size.</param>
        /// <returns>The <see cref="int" /> value of the slice to read.</returns>
        public static implicit operator int(SliceSize size) => size._value;
    }
}
