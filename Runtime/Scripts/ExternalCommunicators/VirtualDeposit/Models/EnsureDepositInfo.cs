#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElympicsPlayPad
{
    public readonly struct EnsureDepositInfo
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
    }
}
