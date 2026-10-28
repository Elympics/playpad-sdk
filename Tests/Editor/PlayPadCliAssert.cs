#nullable enable

using System;
using Elympics.Editor.UnityCli;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    internal static class PlayPadCliAssert
    {
        public static void HasCodeAndFix(Exception? exception, string code)
        {
            Assert.That(exception, Is.Not.Null, $"Expected {code}.");
            Assert.That(exception!.Message, Does.StartWith(code + ": "));
            Assert.That(exception.Message, Does.Match(ElympicsCliErrors.FixMarker + @"\S"));
        }
    }
}
