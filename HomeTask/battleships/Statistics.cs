using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleships {
    public class Statistics {
        public int GamesPlayed { get; private set; }
        public int Crashes { get; private set; }
        public int Median { get; private set; }
        public double Mean { get; private set; }
        public double Sigma { get; private set; }
        public double BadFraction { get; private set; }
        public double Score { get; private set; }

        public Statistics(int fieldWidth, int fieldHeight, int crashLimit, List<GameResult> gameResults) {
            GamesPlayed = gameResults.Count;
            Crashes = gameResults.Sum(result => result.Crashed ? 1 : 0);
            List<int> turnsCountList = gameResults.Select(result => result.TurnsCount).ToList();
            if (turnsCountList.Count == 0) turnsCountList.Add(1000 * 1000);
            turnsCountList.Sort();
            Median = turnsCountList.Count % 2 == 1 ? turnsCountList[turnsCountList.Count / 2] : (turnsCountList[turnsCountList.Count / 2] + turnsCountList[(turnsCountList.Count + 1) / 2]) / 2;
            Mean = turnsCountList.Average();
            Sigma = Math.Sqrt(turnsCountList.Average(s => (s - Mean) * (s - Mean)));
            int badShots = gameResults.Sum(result => result.BadShots);
            BadFraction = (100.0 * badShots) / turnsCountList.Sum();
            var crashPenalty = 100.0 * Crashes / crashLimit;
            var efficiencyScore = 100.0 * (fieldWidth * fieldHeight - Mean) / (fieldWidth * fieldHeight);
            Score = efficiencyScore - crashPenalty - BadFraction;
        }
    }
}
