// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ValueType.cs">
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

namespace Value
{
    /// <summary>
    ///     Base class for any Value Type (i.e. the 'Value Object' oxymoron of DDD).
    ///     All you have to do is to implement the abstract methods:
    ///     <see cref="EquatableByValue{T}.GetAllAttributesToBeUsedForEquality" />
    /// </summary>
    /// <typeparam name="T">Domain type to be 'turned' into a Value Type.</typeparam>
    public abstract class ValueType<T> : EquatableByValue<T>
    {
    }
}
