#nullable enable

using System;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    internal static class PlayPadCliAssert
    {
        /// <summary>Also asserts a non-empty fix hint.</summary>
        public static void HasCodeAndFix(Exception? exception, string code)
        {
            Assert.That(exception, Is.Not.Null, $"Expected {code}.");
            Assert.That(exception!.Message, Does.StartWith(code + ": "));
            Assert.That(exception.Message, Does.Match(@" Fix: \S"));
        }
    }
}
