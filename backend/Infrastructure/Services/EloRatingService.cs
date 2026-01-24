using System;
using PCM.Application.Interfaces;

namespace PCM.Infrastructure.Services
{
    /// <summary>
    /// ELO Rating calculation service
    /// Based on standard ELO algorithm used in chess and competitive games
    /// </summary>
    public class EloRatingService : IEloRatingService
    {
        public (double player1NewRating, double player2NewRating) CalculateNewRatings(
            double player1Rating, 
            double player2Rating, 
            bool player1Won, 
            int kFactor = 32)
        {
            // Expected scores
            double expectedScorePlayer1 = 1.0 / (1.0 + Math.Pow(10, (player2Rating - player1Rating) / 400.0));
            double expectedScorePlayer2 = 1.0 / (1.0 + Math.Pow(10, (player1Rating - player2Rating) / 400.0));

            // Actual scores
            double actualScorePlayer1 = player1Won ? 1.0 : 0.0;
            double actualScorePlayer2 = player1Won ? 0.0 : 1.0;

            // New ratings
            double newRatingPlayer1 = player1Rating + kFactor * (actualScorePlayer1 - expectedScorePlayer1);
            double newRatingPlayer2 = player2Rating + kFactor * (actualScorePlayer2 - expectedScorePlayer2);

            return (Math.Round(newRatingPlayer1, 2), Math.Round(newRatingPlayer2, 2));
        }

        public double CalculateTeamRating(double player1Rating, double? player2Rating = null)
        {
            if (player2Rating.HasValue)
            {
                // Average for doubles
                return (player1Rating + player2Rating.Value) / 2.0;
            }

            // Singles
            return player1Rating;
        }
    }
}
