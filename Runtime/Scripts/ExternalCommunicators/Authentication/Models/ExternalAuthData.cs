#nullable enable
using Elympics.Models.Authentication;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    public struct ExternalAuthData
    {
        public readonly AuthData AuthData;
        public readonly bool IsMobile;
        public readonly Capabilities Capabilities;
        public readonly string Environment;
        public readonly string ClosestRegion;
        public TournamentInfo? TournamentInfo;

        public ExternalAuthData(
            AuthData authData,
            bool isMobile,
            Capabilities capabilities,
            string environment,
            string closestRegion,
            TournamentInfo? tournamentInfo)
        {
            TournamentInfo = tournamentInfo;
            AuthData = authData;
            IsMobile = isMobile;
            Capabilities = capabilities;
            Environment = environment;
            ClosestRegion = closestRegion;
        }

        public override string ToString() => $"{nameof(ExternalAuthData)} IsAuthData: {AuthData is not null}, IsTournament {TournamentInfo is not null}";
    }
}
