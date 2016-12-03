﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Ipfs
{

    [TestClass]
    public class MultiAddressTest
    {
        const string somewhere = "/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";
        const string nowhere = "/ip4/10.1.10.11/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";

        [TestMethod]
        public void Parsing()
        {
            var a = new MultiAddress(somewhere);
            Assert.AreEqual(3, a.Protocols.Count);
            Assert.AreEqual("ip4", a.Protocols[0].Name);
            Assert.AreEqual("10.1.10.10", a.Protocols[0].Value);
            Assert.AreEqual("tcp", a.Protocols[1].Name);
            Assert.AreEqual("29087", a.Protocols[1].Value);
            Assert.AreEqual("ipfs", a.Protocols[2].Name);
            Assert.AreEqual("QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC", a.Protocols[2].Value);

            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress((string)null));
            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress(""));
            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress("   "));
        }

        [TestMethod]
        public void Unknown_Protocol_Name()
        {
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/foobar/123"));
        }

        [TestMethod]
        public void Missing_Protocol_Name()
        {
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/"));
        }

        [TestMethod]
        public new void ToString()
        {
            Assert.AreEqual(somewhere, new MultiAddress(somewhere).ToString());
        }

        [TestMethod]
        public void Value_Equality()
        {
            var a0 = new MultiAddress(somewhere);
            var a1 = new MultiAddress(somewhere);
            var b = new MultiAddress(nowhere);
            MultiAddress c = null;
            MultiAddress d = null;

            Assert.IsTrue(c == d);
            Assert.IsFalse(c == b);
            Assert.IsFalse(b == c);

            Assert.IsFalse(c != d);
            Assert.IsTrue(c != b);
            Assert.IsTrue(b != c);

#pragma warning disable 1718
            Assert.IsTrue(a0 == a0);
            Assert.IsTrue(a0 == a1);
            Assert.IsFalse(a0 == b);

#pragma warning disable 1718
            Assert.IsFalse(a0 != a0);
            Assert.IsFalse(a0 != a1);
            Assert.IsTrue(a0 != b);

            Assert.IsTrue(a0.Equals(a0));
            Assert.IsTrue(a0.Equals(a1));
            Assert.IsFalse(a0.Equals(b));

            Assert.AreEqual(a0, a0);
            Assert.AreEqual(a0, a1);
            Assert.AreNotEqual(a0, b);

            Assert.AreEqual<MultiAddress>(a0, a0);
            Assert.AreEqual<MultiAddress>(a0, a1);
            Assert.AreNotEqual<MultiAddress>(a0, b);

            Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
            Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
            Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        public void Bad_Port()
        {
            var tcp = new MultiAddress("/tcp/65535");
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/tcp/x"));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/tcp/65536"));

            var udp = new MultiAddress("/udp/65535");
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/upd/x"));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/udp/65536"));
        }

        [TestMethod]
        public void Bad_IPAddress()
        {
            var ipv4 = new MultiAddress("/ip4/127.0.0.1");
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip4/x"));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip4/127."));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip4/::1"));

            var ipv6 = new MultiAddress("/ip6/::1");
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip6/x"));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip6/03:"));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("/ip6/127.0.0.1"));
        }

        [TestMethod]
        public void RoundTripping()
        {
            var addresses = new[]
            {
                somewhere,
                "/ip4/1.2.3.4/tcp/80/http",
                "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/tcp/443/https",
                "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/udp/8001",
                "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/sctp/8001",
                "/ip6/3ffe:1900:4545:3:200:f8ff:fe21:67cf/dccp/8001",
            };
            foreach (var a in addresses)
            {
                var ma0 = new MultiAddress(a);

                var ms = new MemoryStream();
                ma0.Write(ms);
                ms.Position = 0;
                var ma1 = new MultiAddress(ms);
                Assert.AreEqual<MultiAddress>(ma0, ma1);

                var ma2 = new MultiAddress(ma0.ToString());
                Assert.AreEqual<MultiAddress>(ma0, ma2);

                var ma3 = new MultiAddress(ma0.ToArray());
                Assert.AreEqual<MultiAddress>(ma0, ma3);
            }
        }

        [TestMethod]
        public void Reading_Invalid_Code()
        {
            ExceptionAssert.Throws<InvalidDataException>(() => new MultiAddress(new byte[] { 0x7F }));
        }

        [TestMethod]
        public void Reading_Empty()
        {
            ExceptionAssert.Throws<Exception>(() => new MultiAddress(new byte[0]));
        }

        [TestMethod]
        public void Reading_Invalid_Text()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress((string)null));
            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress(""));
            ExceptionAssert.Throws<ArgumentNullException>(() => new MultiAddress("  "));
            ExceptionAssert.Throws<FormatException>(() => new MultiAddress("tcp/80"));
        }

        [TestMethod]
        public void Implicit_Conversion_From_String()
        {
            MultiAddress a = somewhere;
            Assert.IsInstanceOfType(a, typeof(MultiAddress));
        }
    }
}

