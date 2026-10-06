#!/bin/bash
# Regenerates Runtime/Plugins/MessagePackGenerated.cs (MessagePack.Generator 2.5.124, `dotnet tool install -g MessagePack.Generator`).
# The input is ElympicsPlayPad.Lobby.csproj: mpc collects root types only from the input project's own sources, and the lobby
# and room DTOs live there (types they reference, like ElympicsUserDTO, are followed into other assemblies).
# Run from anywhere inside the host Unity project; the Editor must have generated its csprojs.
set -e
PACKAGE="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
HOST="$PACKAGE"
while [ "$HOST" != "/" ] && ! { [ -d "$HOST/ProjectSettings" ] && [ -f "$HOST/Packages/manifest.json" ]; }; do HOST="$(dirname "$HOST")"; done
[ "$HOST" = "/" ] && HOST="$(pwd)"
[ -f "$HOST/ElympicsPlayPad.Lobby.csproj" ] || { echo "ElympicsPlayPad.Lobby.csproj not found in $HOST. Open the project in the Unity Editor first, or run from the host project directory." >&2; exit 1; }
# Opening the project evaluates it with MSBuild; keep its intermediate files out of the host project.
SCRATCH="$(mktemp -d)"
trap 'rm -rf "$SCRATCH"' EXIT
OBJ="$SCRATCH/obj/"
command -v cygpath >/dev/null && OBJ="$(cygpath -w "$OBJ")"
export BaseIntermediateOutputPath="$OBJ"
mpc -i "$HOST/ElympicsPlayPad.Lobby.csproj" -o "$PACKAGE/Runtime/Plugins/MessagePackGenerated.cs" -r PlayPadGeneratedResolver -n ElympicsPlayPad.Serialization
