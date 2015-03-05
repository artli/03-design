using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace battleships
{
    public class GameResult {
        public bool Crashed { get; private set; }
        public int BadShots { get; private set; }
        public int TurnsCount { get; private set; }

        public GameResult(bool crashed, int badShots, int turnsCount) {
            Crashed = crashed;
            BadShots = badShots;
            TurnsCount = turnsCount;
        }
    }

	public static class AiTester
	{
		private static readonly Logger resultsLog = LogManager.GetLogger("results");

        public static void TestSingleFile(Settings settings, string exePath) {
            //var monitor = new ProcessMonitor(TimeSpan.FromSeconds(settings.TimeLimitSeconds * settings.GamesCount), settings.MemoryLimit);
            var mapGenerator = new MapGenerator(settings, new Random(settings.RandomSeed));
            var maps = AiTester.GenerateMaps(mapGenerator);
            var games = AiTester.GenerateGames(exePath, maps).Take(settings.GamesCount);
            var gameResults = AiTester.GetGamesResults(games, settings.CrashLimit, settings.Interactive).ToList();

            if (settings.Verbose)
                WriteVerboseResults(gameResults);
            WriteStatistics(settings, gameResults);
        }

        private static IEnumerable<Map> GenerateMaps(MapGenerator generator) {
            while (true)
                yield return generator.GenerateMap();
        }

        private static IEnumerable<Game> GenerateGames(string exePath, IEnumerable<Map> maps) {
            var ai = new Ai(exePath);
            foreach (var map in maps) {
                var game = new Game(map, ai);
                yield return game;
                if (game.AiCrashed)
                    ai = new Ai(exePath);
            }
            ai.Dispose();
        }

        private static IEnumerable<GameResult> GetGamesResults(IEnumerable<Game> games, int crashLimit, bool interactive) {
            int crashes = 0;
            foreach (var game in games) {
                var result = GetGameResult(game, interactive);
                yield return result;
                if (result.Crashed)
                    crashes++;
                if (crashes > crashLimit)
                    break;
            }
        }

        private static GameResult GetGameResult(Game game, bool interactive) {
            var gameSteps = GetGameSteps(game);

            Game lastStep = null;
            if (interactive) {
                var vis = new GameVisualizer();
                foreach (var step in gameSteps) {
                    lastStep = step;
                    vis.Visualize(step);
                    if (step.AiCrashed)
                        Console.WriteLine(step.LastError.Message);
                    Console.ReadKey();
                }
            } else
                lastStep = gameSteps.Last();

            var badShots = lastStep.BadShots;
            bool crashed = lastStep.AiCrashed;
            var turnsCount = lastStep.TurnsCount;
            return new GameResult(crashed, badShots, turnsCount);
        }

		private static IEnumerable<Game> GetGameSteps(Game game)
		{
			while (!game.IsOver())
			{
				game.MakeStep();
                yield return game;
			}
		}

        private static void WriteVerboseResults(List<GameResult> gameResults) {
            int gameNumber = 0;
            foreach (var result in gameResults) {
                Console.WriteLine(
                    "Game #{3,4}: Turns {0,4}, BadShots {1}{2}",
                    result.TurnsCount, result.BadShots, result.Crashed ? ", Crashed" : "", gameNumber);
                gameNumber++;
            }
        }

        private static void WriteStatistics(Settings settings, List<GameResult> gameResults) {
            var statistics = new Statistics(settings.Width, settings.Height, settings.CrashLimit, gameResults);
            //var message = GetStatisticsMessage(ai.Name, statistics);
            var message = GetStatisticsMessage("MyAI", statistics);
            Console.Write(message);
        }

		private static string GetStatisticsMessage(string aiName, Statistics statistics)
		{
			var headers = FormatTableRow(new object[] { "AiName", "Mean", "Sigma", "Median", "Crashes", "Bad%", "Games", "Score" });
            var values = FormatTableRow(new object[] { aiName, statistics.Mean, statistics.Sigma, statistics.Median, statistics.Crashes, statistics.BadFraction, statistics.GamesPlayed, statistics.Score });
			resultsLog.Info(values);
            var message = string.Format(
                "\n" +
                "Score statistics\n" +
                "================\n" +
                "{0}\n" +
                "{1}\n",
                headers, values);
            return message;
		}

		private static string FormatTableRow(object[] values)
		{
			return FormatValue(values[0], 15) 
				+ string.Join(" ", values.Skip(1).Select(v => FormatValue(v, 7)));
		}

		private static string FormatValue(object v, int width)
		{
			return v.ToString().Replace("\t", " ").PadRight(width).Substring(0, width);
		}
	}
}