using System;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Auth.Models
{
    [PublicAPI]
    public struct RegionData : IEquatable<RegionData>
    {
        public RegionData(string name) => Name = name;

        /// <summary>
        /// Name of the region.
        /// </summary>
        public string Name { get; init; }

        public bool Equals(RegionData other) => Name == other.Name;
    }
}
