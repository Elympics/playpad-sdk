using System.Collections;
using System.Collections.Generic;
using ElympicsPlayPad.Utility;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests.PlayMode
{
    [Category("Wei")]
    public class WeiTests
    {
        [TestCase("1000000000000000000", 18, 1d)]
        [TestCase("100000000000000000", 18, 0.1d)]
        [TestCase("123400000000000000", 18, .1234d)]
        [TestCase("10000000000000000000", 18, 10d)]
        [TestCase("0", 18, 0d)]
        public void TestFromWei(string wei, int decimalUnit, decimal expected)
        {
            var result = WeiConverter.FromWei(wei, decimalUnit);
            Assert.AreEqual(expected, result);
        }

        [TestCase(1d, 18, "1000000000000000000")]
        [TestCase(.1d, 18, "100000000000000000")]
        [TestCase(.1234d, 18, "123400000000000000")]
        [TestCase(10d, 18, "10000000000000000000")]
        [TestCase(0d, 18, "0")]
        public void ToWei(decimal amount, int decimalUnit, string expected)
        {
            var result = WeiConverter.ToWei(amount, decimalUnit);
            Assert.AreEqual(expected, result);
        }
    }
}
