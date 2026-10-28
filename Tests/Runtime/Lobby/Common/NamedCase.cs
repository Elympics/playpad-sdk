#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace ElympicsPlayPad.Tests.Lobby.Common
{
    /// <summary>
    /// A test case argument whose display name is <see cref="Name"/>. NUnit names a case from its arguments' <c>ToString()</c>,
    /// and a PlayMode run lists the cases again before running them. Arguments that hold timestamps or random GUIDs then get a different name,
    /// and NUnit silently drops the case.
    /// </summary>
    public sealed record NamedCase<T>(string Name, T Value)
    {
        public override string ToString() => Name;
    }

    public static class NamedCase
    {
        public static List<NamedCase<T>> ByIndex<T>(IEnumerable<T> cases) =>
            cases.Select((value, index) => new NamedCase<T>($"#{index}", value)).ToList();
    }
}
