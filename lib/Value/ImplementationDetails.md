# Implementation details about *Value*

(*may be interesting if you want to understand the library before you use it, or if you want to contribute to the project*)

![Value](https://github.com/tpierrain/Value/blob/master/Value-small.jpg?raw=true)

## External-design drivers

The *Value* library:

1. __must be very easy and straightforward to use__ (must follow [the pit-of-success](https://blog.codinghorror.com/falling-into-the-pit-of-success/))
2. __must not add accidental complexity(1) to the value types of the developers using the library__ (called *Devs* hereafter). Thus, it must not force the Devs to add boiler-plate or complicated code (like Equality or Hashcode implementations) in their own Value Types.
	- (1) *accidental complexity* means plumbering or technical code that has nothing to deal with the domain logic
3. __packages must be available for most of the .NET platforms and versions__ ( >= .NET 4.0, >= dotnet standard 1.3 and thus dotnet core compliant)

## Internal-design drivers

Here are some drivers and insights about the implementation design of the *Value* library:

- 100% tests coverage for the entire library
- __*ByValue* collections (e.g. ListByValue<T>, DictionaryByValue<T>,..) are NOT *ValueTypes*__ (since we can modify them). Reason why they must NOT inherit from ValueType<T>
- ValueType<T> and all *ByValue* collections __MUST accept struct as well as reference types__ (i.e. classes)
- All *ByValue* collections should behave the same way as the collection they are decorating/wrapping (by delegation). Reason why they are derived from them.
- Hashcode of EquatableByValue types must not be computed more than once (thus be cached) for performance purpose. Nonetheless, the resulted hashcode of all the *ByValue* collections must be reset every time the collection is changed (e.g. Add, Remove, etc).

- TODO: All the implementation must follow the StyleCop rules provided with the project
- Whenever possible (and if it doesn't violate the External design drivers for the library), __foster *Aggregation* over *Inheritance*__. If you can't, try to minimize the level of inheritance.
- All implementation types should be easily copied-pasted for those that want to use the library in a .NET platform or version that aren't cover yet by *Value*.


## Misc implementation details/insights

- I tried to foster aggregation (with an EquatableStrategy) over inheritance for the implementation of the library, but it forced me to publicly expose stuffs (like ResetHashcode() method) in the EquatableByValue<T> type in order for the aggregated strategy (pattern) to be able to use it (and thus making it appeared in the ValueType<T>) that I didn't want to expose for all Value types at the end of the day (since it violated the rule #2: *don't add accidental complexity to the library's users value types* )
	- Thus, I rollbacked the implementation to a classic inheritance things. Sad Panda. I have other ideas to explore in order to retry getting rid of this inheritance but I just need time ;)


 