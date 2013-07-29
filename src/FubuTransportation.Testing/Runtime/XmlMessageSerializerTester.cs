﻿using System;
using System.Collections.Generic;
using System.IO;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime
{
    //Copied from RSB for backwards compatibility
    [TestFixture]
    public class XmlMessageSerializerTester
    {
        private readonly Order sample = new Order
        {
            Url = new Uri("msmq://www.ayende.com/"),
            At = DateTime.Today,
            Count = 5,
            OrderId = new Guid("1909994f-8173-452c-a651-14725bb09cb6"),
            OrderLines = new[]
            {
                new OrderLine
                {
                    Product = "milk",
                    Fubar = new List<int> {1, 2, 3}
                },
                new OrderLine
                {
                    Product = "butter",
                    Fubar = new List<int> {4, 5, 6}
                }
            },
            TimeToDelivery = TimeSpan.FromDays(1),
        };

        private readonly Order sample2 = new Order
        {
            Url = new Uri("msmq://www.ayende.com/"),
            At = DateTime.Today,
            Count = 5,
            OrderId = new Guid("99B5810B-CC5A-4728-B20E-66D6B0022D64"),
            OrderLines = new[]
            {
                new OrderLine
                {
                    Product = "milk",
                    Fubar = new List<int> {1, 2, 3}
                },
                new OrderLine
                {
                    Product = "butter",
                    Fubar = new List<int> {4, 5, 6}
                }
            },
            TimeToDelivery = TimeSpan.FromDays(1),
        };

        [Test]
        public void can_round_trip_object_array()
        {
            var messages = new object[] {sample, sample2, new Address {City = "SLC", State = "Utah"}};

            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(messages, stream);

            stream.Position = 0;

            var actual = serializer.Deserialize(stream).ShouldBeOfType<object[]>();
            actual[0].ShouldBeOfType<Order>();
            actual[1].ShouldBeOfType<Order>();
            actual[2].ShouldBeOfType<Address>();

        }

        [Test]
        public void can_round_trip_single_message()
        {
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(sample, stream);

            stream.Position = 0;

            var actual = serializer.Deserialize(stream).ShouldBeOfType<Order>();
            actual.OrderId.ShouldEqual(sample.OrderId);
        }

        [Test]
        public void Can_serialize_and_deserialize_primitive()
        {
            long ticks = DateTime.Now.Ticks;
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[] { ticks }, stream);
            stream.Position = 0;
            var actual = (long)serializer.Deserialize(stream).As<long>();
            ticks.ShouldEqual(actual);
        }

        [Test]
        public void Can_serialize_and_deserialize_double()
        {
            double aDouble = 1.12;
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[] { aDouble }, stream);
            stream.Position = 0;
            var actual = (double)serializer.Deserialize(stream).As<double>();
            aDouble.ShouldEqual(actual);
        }

        [Test]
        public void Can_serialize_and_deserialize_float()
        {
            float aFloat = 1.12f;
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[] { aFloat }, stream);
            stream.Position = 0;
            var actual = (float)serializer.Deserialize(stream).As<float>();
            aFloat.ShouldEqual(actual);
        }

        [Test]
        public void Can_serialize_and_deserialize_byte_array()
        {
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[] { new byte[] { 1, 2, 3, 4 } }, stream);
            stream.Position = 0;
            var actual = (byte[])serializer.Deserialize(stream).As<byte[]>();
            new byte[] { 1, 2, 3, 4 }.ShouldEqual(actual);
        }

        [Test]
        public void Can_serialize_and_deserialize_DateTimeOffset()
        {
            var value = DateTimeOffset.Now;
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[] { value }, stream);
            stream.Position = 0;
            var actual = (DateTimeOffset)serializer.Deserialize(stream).As<DateTimeOffset>();
            value.ShouldEqual(actual);
        }

        [Test]
        public void Can_serialize_and_deserialize_array()
        {
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new object[]
            {
                new ClassWithObjectArray
                {
                    Items = new object[] {new OrderLine {Product = "ayende"}}
                }
            }, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize(stream).As<ClassWithObjectArray>();
            "ayende".ShouldEqual(actual.Items[0].As<OrderLine>().Product);
        }

        [Test]
        public void Can_deserialize_complex_object_graph()
        {
            var serializer = new XmlMessageSerializer();
            var stream = new MemoryStream();
            serializer.Serialize(new[] { sample }, stream);
            stream.Position = 0;

            var order = serializer.Deserialize(stream).As<Order>();

            sample.Url.ShouldEqual(order.Url);
            sample.At.ShouldEqual(order.At);
            sample.Count.ShouldEqual(order.Count);
            sample.OrderId.ShouldEqual(order.OrderId);
            sample.TimeToDelivery.ShouldEqual(order.TimeToDelivery);
            order.OrderLines.ShouldHaveCount(2);

            sample.OrderLines[0].Product.ShouldEqual(order.OrderLines[0].Product);
            sample.OrderLines[1].Product.ShouldEqual(order.OrderLines[1].Product);
        }

        #region Nested type: ClassWithObjectArray

        public class ClassWithObjectArray
        {
            public object[] Items { get; set; }
        }

        #endregion

        #region Nested type: Order

        public class Order
        {
            public Uri Url { get; set; }
            public int Count { get; set; }
            public Guid OrderId { get; set; }
            public DateTime At { get; set; }
            public TimeSpan TimeToDelivery { get; set; }

            public OrderLine[] OrderLines { get; set; }
        }

        #endregion

        #region Nested type: OrderLine

        public class OrderLine
        {
            public string Product { get; set; }
            public List<int> Fubar { get; set; }
        }

        #endregion 
    }
}