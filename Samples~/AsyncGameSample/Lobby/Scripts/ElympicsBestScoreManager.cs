using ElympicsPlayPad.ExternalCommunicators;

/// <summary>
/// This class helps keep track of the All Time best score of the player and their Tournament Best score to be displayed in the end game screen
/// </summary>
public class ElympicsBestScoreManager
{
    public static int AllTimeHighScore { get; private set; }
    public static int TournamentHighScore { get; private set; }

    /// <summary>
    /// Method to initialize the class
    /// </summary>
    public static void OnStart()
    {
        if (PlayPadCommunicator.Instance is not null && PlayPadCommunicator.Instance.ExternalAuthenticator is not null)
            PlayPadCommunicator.Instance.ExternalAuthenticator.AuthenticationUpdated += AuthenticationUpdated;
    }

    /// <summary>
    /// Check if the current score is the new "Tournament high score". This is needed since the current score is not saved in the backend until the game server closes.
    /// </summary>
    public static bool CheckIsNewTournamentHighScore(int currentScore)
    {
        if (currentScore <= TournamentHighScore)
            return false;
        TournamentHighScore = currentScore;
        return true;
    }

    /// <summary>
    /// Check if the current score is the new high score. This is needed since the current score is not saved in the backend until the game server closes.
    /// </summary>
    public static bool CheckIsNewAllTimeHighScore(int currentScore)
    {
        if (currentScore <= AllTimeHighScore)
            return false;
        AllTimeHighScore = currentScore;
        return true;
    }

    /// <summary>
    /// Callback for when authentication is updated (We should update the scores)
    /// </summary>
    private static void AuthenticationUpdated(Elympics.Models.Authentication.AuthData obj)
    { }
}
