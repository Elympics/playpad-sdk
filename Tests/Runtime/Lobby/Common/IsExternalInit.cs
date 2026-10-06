using JetBrains.Annotations;

// Required for init-only setters. Elympics and Elympics.Core both expose theirs to this assembly (CS8356), so it needs its own.

namespace System.Runtime.CompilerServices
{
    [UsedImplicitly]
    internal static class IsExternalInit
    { }
}
