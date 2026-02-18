// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;
using SoEx.Hosting.Serializers.NewtonsoftJson;

namespace SoEx.TestSoEx
{
    [TestFixture]
    public class JsonMessageSerializerTests
    {
        private readonly JsonMessageSerializer _serializer = new();

        [TestCase(42)]
        [TestCase(-5)]
        [TestCase(0)]
        public void SerializeDeserialize_Int(int value)
        {
            byte[] bytes = _serializer.Serialize(value);
            int result = _serializer.Deserialize<int>(bytes);
            Assert.That(result, Is.EqualTo(value));
        }

        [TestCase("hello")]
        [TestCase("")]
        [TestCase("JSON Test")]
        public void SerializeDeserialize_String(string value)
        {
            byte[] bytes = _serializer.Serialize(value);
            string? result = _serializer.Deserialize<string>(bytes);
            Assert.That(result, Is.EqualTo(value));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SerializeDeserialize_Bool(bool value)
        {
            byte[] bytes = _serializer.Serialize(value);
            bool result = _serializer.Deserialize<bool>(bytes);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void SerializeDeserialize_NullObject()
        {
            object? value = null;
            byte[] bytes = _serializer.Serialize(value);
            object? result = _serializer.Deserialize<object>(bytes);
            Assert.That(result, Is.Null);
        }

        class SimpleClass
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [Test]
        public void SerializeDeserialize_SimpleClass()
        {
            SimpleClass obj = new SimpleClass { Id = 1, Name = "Test" };
            byte[] bytes = _serializer.Serialize(obj);
            SimpleClass? result = _serializer.Deserialize<SimpleClass>(bytes);
            Assert.Multiple(() =>
            {
                Assert.That(result?.Id, Is.EqualTo(obj.Id));
                Assert.That(result?.Name, Is.EqualTo(obj.Name));
            });
        }

        [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
        [JsonDerivedType(typeof(DerivedClass), nameof(DerivedClass))]
        public class BaseClass
        {
            public string? Type => GetType().Name;
        }

        public class DerivedClass : BaseClass
        {
            public int Value { get; set; }
        }

        [Test]
        public void SerializeDeserialize_PolymorphicObject()
        {
            BaseClass obj = new DerivedClass { Value = 123 };
            byte[] bytes = _serializer.Serialize(obj);
            BaseClass? result = _serializer.Deserialize<BaseClass>(bytes);

            Assert.That(result,Is.Not.Null);
            DerivedClass? derivedClass = result as DerivedClass;
            Assert.That(derivedClass,Is.Not.Null);
            Assert.That(((DerivedClass)obj).Value, Is.EqualTo(derivedClass.Value));
        }

        [Test]
        public void SerializeDeserialize_TypedObject_WithTypeMetadata()
        {
            DerivedClass obj = new DerivedClass { Value = 777 };
            byte[] bytes = _serializer.Serialize((object)obj);
            object? result = _serializer.Deserialize<object>(bytes);

            Assert.That(result, Is.TypeOf<DerivedClass>());
            Assert.That(((DerivedClass)result!).Value, Is.EqualTo(777));
        }

        [Test]
        public void SerializeDeserialize_TypedArray_WithTypeMetadata()
        {
            object array = new int[] { 1, 2, 3 };
            byte[] bytes = _serializer.Serialize(array);
            object? result = _serializer.Deserialize<object>(bytes);

            Assert.That(result, Is.TypeOf<int[]>());
            Assert.That((int[])result!, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void SerializeDeserialize_TypeObject()
        {
            Type type = typeof(string);
            byte[] bytes = _serializer.Serialize(type);
            Type? result = _serializer.Deserialize<Type>(bytes);
            Assert.That(result, Is.EqualTo(type));
        }

        public class GenericClass<T>
        {
            public required T Value { get; set; }
        }

        public class SimpleClassContainingGenericClass
        {
            public required GenericClass<string> Value { get; set; }
        }

        [Test]
        public void SerializeDeserialize_GenericClassString()
        {
            GenericClass<string> genericClass = new GenericClass<string>() { Value = "string" };
            byte[] bytes = _serializer.Serialize(genericClass);
            GenericClass<string>? result = _serializer.Deserialize<GenericClass<string>>(bytes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(genericClass.Value));
        }

        [Test]
        public void SerializeDeserialize_SimpleClassContainingGenericClass()
        {
            SimpleClassContainingGenericClass simpleClassContainingGenericClass = new SimpleClassContainingGenericClass() { Value = new GenericClass<string>() { Value = "string" } };
            byte[] bytes = _serializer.Serialize(simpleClassContainingGenericClass);
            SimpleClassContainingGenericClass? result = _serializer.Deserialize<SimpleClassContainingGenericClass>(bytes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.Value, Is.EqualTo(simpleClassContainingGenericClass.Value.Value));
        }

        public record SimpleRecord(int Int, string String, DateTime DateTime);

        [Test]
        public void SerializeDeserialize_SimpleRecord()
        {
            SimpleRecord simpleRecord = new SimpleRecord(123, "123", DateTime.UtcNow);
            byte[] bytes = _serializer.Serialize(simpleRecord);
            SimpleRecord? result = _serializer.Deserialize<SimpleRecord>(bytes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(simpleRecord));
        }
    }
}
