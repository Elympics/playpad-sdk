#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
#nullable enable

using System;
using System.Collections.Generic;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.Lobby;

namespace ElympicsPlayPad.Wrappers.Connection
{
    internal interface IRegionValidator
    {
        bool IsRegionValid(RegionData regionData);
        List<string> GetAvailableRegions { get; }
    }

    internal class StandardRegionValidator : IRegionValidator
    {
        public StandardRegionValidator(List<string> availableRegions) => GetAvailableRegions = availableRegions;
        public bool IsRegionValid(RegionData regionData) => regionData.Name == string.Empty || GetAvailableRegions.Contains(regionData.Name);
        public List<string> GetAvailableRegions { get; }
    }

    internal class SessionConnectionFactory
    {
        private readonly IRegionValidator _regionValidator;
        public SessionConnectionFactory(IRegionValidator regionValidator) => _regionValidator = regionValidator;

        public SessionConnectionDetails CreateSessionConnectionDetails(string url, AuthData authData, ElympicsGameConfig gameConfig, RegionData? regionData)
        {
            var gameId = new Guid(gameConfig.GameId);
            var gameVersion = gameConfig.GameVersion;

            if (!regionData.HasValue)
                return new SessionConnectionDetails(url, authData, gameId, gameVersion, string.Empty);

            if (_regionValidator.IsRegionValid(regionData.Value))
                return new SessionConnectionDetails(url, authData, gameId, gameVersion, regionData.Value.Name);

            throw new ElympicsException($"The specified region \"{regionData.Value.Name}\" must be one of the available regions {string.Join(" | ", _regionValidator.GetAvailableRegions)}");
        }
    }
}
#endif
