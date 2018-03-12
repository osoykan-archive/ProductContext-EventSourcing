[![Build status](https://ci.appveyor.com/api/projects/status/ju5m6t3fm2xsl0o9/branch/master?svg=true)](https://ci.appveyor.com/project/tpierrain/value/branch/master)

# Value

is a pico library (or code snippets shed) to help you to __easily implement Value Types__ in your C# projects without making errors nor polluting your domain logic with boiler-plate code.

![Value](https://github.com/tpierrain/Value/blob/master/Value-small.jpg?raw=true)

## Value Types?
__Domain Driven Design (DDD)__'s *Value Object* being an oxymoron (indeed, an object has a changing state by nature), we now rather use the "*Value Type*" (instance) terminology. But the concept is the same as described within Eric Evan's Blue book.

__A Value Type:__
 - is __immutable__ (every field must be read-only after the Value Type instantiation; no 'setter' is allowed)
 - is __rich with domain logic and behaviours__. The idea is to swallow (and encapsulate) most of our business complexity within those classes
 - embraces the __Ubiquitous Language__ of our business context: cure to primitive obsession, the usage of Value Types is an opportunity for us to embrace the language of our business within our code base
 - __exposes, uses and combines functions__ to provide business (domain) value. Functions usually return new instance(s) of Value Types ('*closure of operations*' describing an operation whose return type is the same as the type of its argument(s))

```c#
    // e.g.: the following Add function does not change any existing Amount instance, it just returns a new one
    public Amount Add(Amount otherAmount) 
```

 - __considers ALL its attributes for Equality and Uniqueness__ (and "all" *is-a-must* here)
 - is __auto-validating__ (via *transactional* constructors __with business validation inside__ and throwing exception if necessary)


### Reverse the trend! (we need more Value Types, and fewer Entities)

An *Entity* is an object that has a changeable state (often made by combining Value Objects) for which we care about its identity. Whether to use an Entity or Value Object will strongly depend on your business context (*there is no silver bullet*). Here are some basic samples to better grasp the difference between *Value Types* and *Entities*:
 
 - __*Value types*__: cards of a poker hand, a speed of 10 mph, a bank note of 10 euros (unless you are working for a Central Bank which need then to trace every bank note --> Entity), a user address

 - __*Entities*__: a user account, a customer's basket with items, a customer, ...

Our OO code bases are usually full of types with states and contain very few Value Type instances.
DDD advises us to reverse the trend by not having *Entities* created by default, and to strongly increase our __usage of Value Types__. 

This __will helps us to reduce side-effects within our OO base code__. A simple reflex, for great benefits.

### Side effects, you said?

Yes, because one of the problem we face when we code with Object Oriented (OO) languages like C# or java is the presence of __side-effects__. Indead, the ability for object instances to have their own state changed by other threads or by a specific combination of previous method calls (temporal coupling) __makes our reasoning harder__. Doing Test Driven Development helps a lot, but is not enough to ease the reasoning about our code.

Being inspired by functional programming (FP) languages, __DDD suggests us to make our OO design more FP oriented in order to reduce those painful side-effects__. They are many things we can do for it. E.g.: 
 - to use and combine __functions__ instead of methods (that impact object states)
 - to embrace __CQS pattern__ for *Entity* objects (i.e. a paradigm where read methods never change state and write methods never return data)
 - to implement *Closure of Operations* whenever it's possible (to reduce coupling with other types)
 - to use __*Value Types*__ by default and to keep *Entity* objects only when needed.


### You are not alone

Since there is no first-class citizen for immutability and *Value Types* in C#, the goal of this pico library is to help you easily implement Value Types without caring too much on the boiler-plate code. 

__Yeah, let's focus on our business value now!__

--- 

## What's inside the box?

 - __ValueType<T>__: making all your Value Types deriving from this base class will allow you to __properly implement Equality__ (IEquatable) __and Unicity__ (GetHashCode()) on ALL your fields __in 1 line of code__. Very Handy!
```c#
    // 1. You inherit from ValueType<T> like this:
	public class Amount : ValueType<Amount>
    {
        private readonly decimal quantity;
        private readonly Currency currency;
		
		public decimal Quantity { get { return this.quantity; } }
        public Currency Currency { get { return this.currency; } }

		...

		// 2. You (are then forced to) implement the abstract method returning the list of all your fields
		protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new List<object>() { this.quantity, this.currency }; // The line of code I was talking about
        }

		// And that's all folks!
    }


```

 - __ListByValue<T>__: A list with equality based on its content and not on the list's reference (i.e.: 2 different instances containing the same items in the same order will be equals). This collection decorator is __very useful for any ValueType that aggregates a list__

 ```c#
      // when one of your ValueType aggregates a IList like this
      private readonly List<Card> cards;

      //...

      protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
      {
          // here, you can use the ListByValue decorator to ensure a "ByValue" equality of your Type.
          return new List<object>() { new ListByValue<Card>(this.cards) };
      }
```

 - __SetByValue<T>__: A Set with equality based on its content and not on the Set's reference (i.e.: 2 different instances containing the same items will be equals whatever their storage order). This collection decorator is __very useful for any ValueType that aggregates a set__

```c#
      // when one of your ValueType aggregates a Set like this
      private readonly HashSet<Card> cards;
      
      //...
     
      protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
      {
          // here, you can use the SetByValue decorator to ensure the "ByValue" equality of your Type.
          return new List<object>() { new SetByValue<Card>(this.cards) };
      }
```

--- 

## Usage samples of ValueTypes

__Disclaimer:__ for the sake of clarity, the following code samples don't have behaviours to only focus here on the Equality concern. __Of course, a ValueType in DDD must embed behaviours to swallow complexity (it's not just a DTO or a POCO without responsibilities).__

Code Sample of a properly implemented ValueType:

```c#
    /// <summary>
    /// Proper implementation of a ThreeeCards ValueType since the order of the cards doesn't matter during
    /// Equality. Note: the Card type is also a ValueType.
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
            // we decorate our standard HashSet with the SetByValue helper class.
            return new List<object>() { new SetByValue<Card>(this.cards) };
        }
    }
```

Code Sample of a bad ValueType implementation: 

```c#
    /// <summary>
    /// Bad ValueType implementation of ThreeCards since the GetAllAttributesToBeUsedForEquality() method 
    /// returns the set directly, without decoring it with the SetByValue helper.
    /// </summary>
    public class ThreeCardsBadlyImplementedAsValueType : ValueType<ThreeCards>
    {
        private readonly HashSet<Card> cards;

        public ThreeCardsBadlyImplementedAsValueType(string card1Description, string card2Description, string card3Description)
        {
            this.cards = new HashSet<Card>();

            this.cards.Add(Card.Parse(card1Description));
            this.cards.Add(Card.Parse(card2Description));
            this.cards.Add(Card.Parse(card3Description));
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            // BAD IMPLEMENTATION HERE: should have returned "new SetByValue<Card>(this.cards)" instead of "this.cards"
            return new List<object>() { this.cards };
        }
    }
```

Now, let's have a look a 2 tests that clarify the impact of those 2 implementations:

```c#
        [Test]
        public void Should_consider_equals_two_ValueType_instances_that_aggregates_equivalent_SetByValue()
        {
            var threeCards = new ThreeCards("AS", "QD", "2H");
            var sameThreeCards = new ThreeCards("2H", "QD", "AS");

            Check.That(threeCards).IsEqualTo(sameThreeCards);
        }

        [Test]
        public void Should_consider_Not_equals_two_badly_implemented_ValueType_instances_that_aggregates_equivalent_HashSet()
        {
            var threeCards = new ThreeCardsBadlyImplementedAsValueType("AS", "QD", "2H");
            var sameThreeCards = new ThreeCardsBadlyImplementedAsValueType("2H", "QD", "AS");

            Check.That(threeCards).IsNotEqualTo(sameThreeCards);
        }
```
 
