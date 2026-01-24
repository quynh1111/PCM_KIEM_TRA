namespace PCM.Application.Interfaces
{
    /// <summary>
    /// ELO Rating calculation service
    /// </summary>
    public interface IEloRatingService
    {
        /// <summary>
        /// Calculate ELO change for a match
        /// K-factor determines rating volatility (default 32)
        /// </summary>
        (double player1NewRating, double player2NewRating) CalculateNewRatings(
            double player1Rating, 
            double player2Rating, 
            bool player1Won, 
            int kFactor = 32);
        
        /// <summary>
        /// Calculate team average ELO for doubles matches
        /// </summary>
        double CalculateTeamRating(double player1Rating, double? player2Rating = null);
    }
}
