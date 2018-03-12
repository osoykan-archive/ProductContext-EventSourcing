using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using Value.Shared;

namespace Value.Tests
{
    [TestFixture]
    public class DictionaryByValueTests
    {
        [Test]
        public void Should_consider_two_instances_with_same_elements_inserted_in_same_order_Equals()
        {
            var dico1 = new Dictionary<int, string>() { {1, "uno" }, { 4, "quatro" }, { 3, "tres" } };
            var dico2 = new Dictionary<int, string>() { { 1, "uno" }, { 4, "quatro" }, { 3, "tres" } };

            var byValue1 = new DictionaryByValue<int, string>(dico1);
            var byValue2 = new DictionaryByValue<int, string>(dico2);

            Check.That(dico1).IsNotEqualTo(dico2);
            Check.That(byValue1).IsEqualTo(byValue2);
        }

        [Test]
        public void Should_consider_two_instances_with_same_elements_inserted_in_different_order_Equals()
        {
            var dico1 = new Dictionary<int, string>() { {1, "uno" }, { 4, "quatro" }, { 3, "tres" } };
            var dico2 = new Dictionary<int, string>() { { 1, "uno" }, { 3, "tres" }, { 4, "quatro" } };

            var byValue1 = new DictionaryByValue<int, string>(dico1);
            var byValue2 = new DictionaryByValue<int, string>(dico2);

            Check.That(dico1).IsNotEqualTo(dico2);
            Check.That(byValue1).IsEqualTo(byValue2);
        }

        [Test]
        public void Should_consider_two_instances_with_different_elements_nott_Equals()
        {
            var dico1 = new Dictionary<int, string>() { { 1, "uno" }, { 4, "quatro" }, { 3, "tres" } };
            var dico2 = new Dictionary<int, string>() { { 1, "uno" }, { 79, "setenta y nueve" }, { 4, "quatro" } };

            var byValue1 = new DictionaryByValue<int, string>(dico1);
            var byValue2 = new DictionaryByValue<int, string>(dico2);

            Check.That(byValue1).IsNotEqualTo(byValue2);
        }

        [Test]
        public void Should_consider_an_instance_not_equals_with_SetByValue_instance()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 4, "quatro" }, { 3, "tres" } });
            var set = new SetByValue<KeyValuePair<int, string>>() { new KeyValuePair<int, string>(1, "uno"), new KeyValuePair<int, string>(4, "quatro"), new KeyValuePair<int, string>(3, "tres") };

            Check.That(dico).IsNotEqualTo(set);
        }

        [Test]
        public void Should_change_its_hashcode_everytime_the_dictionary_is_updated()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 4, "quatro" }, { 3, "tres" } });

            var previousHashcode = dico.GetHashCode();
            dico.Add(79, "Setenta y nueve");
            var currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = dico.GetHashCode();
            dico.Remove(79);
            currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = dico.GetHashCode();
            var keyValuePair = new KeyValuePair<int, string>(42, "quarenta y dos");
            dico.Add(keyValuePair);
            currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = dico.GetHashCode();
            dico.Remove(keyValuePair);
            currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = dico.GetHashCode();
            dico[33] = "trenta y tres";
            currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);

            previousHashcode = dico.GetHashCode();
            dico.Clear();
            currentHashcode = dico.GetHashCode();
            Check.That(currentHashcode).IsNotEqualTo(previousHashcode);
        }

        [Test]
        public void Should_properly_expose_Contains()
        {
            var firstDate = DateTime.ParseExact("2017-05-04", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var secondDate = DateTime.ParseExact("2017-05-27", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var dico = new DictionaryByValue<DateTime, string>(new Dictionary<DateTime, string>() { { firstDate, "uno" }, { secondDate, "quatro" } });

            Check.That(dico.Contains(new KeyValuePair<DateTime, string>(firstDate, "uno")));
        }

        [Test]
        public void Should_properly_expose_ContainsKey()
        {
            var firstDate = DateTime.ParseExact("2017-05-04", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var secondDate = DateTime.ParseExact("2017-05-27", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var missingDate = DateTime.ParseExact("1974-12-24", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var dico = new DictionaryByValue<DateTime, string>(new Dictionary<DateTime, string>() { { firstDate, "uno" }, { secondDate, "quatro" } });

            Check.That(dico.ContainsKey(secondDate)).IsTrue();
            Check.That(dico.ContainsKey(missingDate)).IsFalse();
        }

        [Test]
        public void Should_properly_expose_Count()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            Check.That(dico.Count).IsEqualTo(2);
        }

        [Test]
        public void Should_properly_expose_Keys()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            Check.That(dico.Keys).ContainsExactly(1, 2);
        }

        [Test]
        public void Should_properly_expose_Values()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            Check.That(dico.Values).ContainsExactly("uno", "quatro");
        }

        [Test]
        public void Should_properly_expose_Indexer()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            Check.That(dico[1]).IsEqualTo("uno");
            
            dico[42] = "quarenta y dos";
            Check.That(dico[42]).IsEqualTo("quarenta y dos");
        }

        [Test]
        public void Should_properly_expose_TryGetValue()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            string result;
            dico.TryGetValue(1, out result);
            Check.That(result).IsEqualTo("uno");
        }

        [Test]
        public void Should_properly_expose_CopyTo()
        {
            var dico = new DictionaryByValue<int, string>(new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } });

            KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[2];
            dico.CopyTo(array, 0);

            Check.That(array).ContainsExactly(new KeyValuePair<int, string>(1, "uno"), new KeyValuePair<int, string>(2, "quatro"));
        }

        [Test]
        public void Should_properly_expose_IsReadOnly()
        {
            var dictionary = new Dictionary<int, string>() { { 1, "uno" }, { 2, "quatro" } };
            var wrappedDictionary = new DictionaryByValue<int, string>(dictionary);

            Check.That(wrappedDictionary.IsReadOnly).IsEqualTo(((ICollection<KeyValuePair<int, string>>)dictionary).IsReadOnly);
        }
    }
}
