#nullable enable
using UnityEngine;

namespace ElympicsPlayPad.Tournament.Data
{
    public struct Prize
    {
        public string? Name { get; init; }
        public int? Amount { get; init; }
        public int Position { get; init; }
        public Texture2D? Texture { get; init; }
        public Prize[]? Prizes { get; init; }
    }
}
