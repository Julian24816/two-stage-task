using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

public class Results {
    public static string PathToLatestCSVFile = null;

    public readonly int IncompleteTrials, CompleteTrials; // = 0 -> rest not filled
    public readonly DateTime StartTime, EndTime;
    public readonly TimeSpan TotalExperimentDuration, AverageTrialDuration, AverageWaitingTime;
    public readonly TimeSpan AverageReactionTimeFirstChoice, AverageReactionTimeSecondChoice;
    public readonly TimeSpan BestReactionTimeFirstChoice, BestReactionTimeSecondChoice;
    public readonly int NumberOfWins;
    public readonly int NumberOfTrialsWithWinChanceGreaterThanHalf;
    public readonly float AverageWinChance;
    public readonly float PercentageCommonTransition;
    public readonly float PercentageSameChoiceAfterWinWithCommonTransition;
    public readonly float PercentageSameChoiceAfterWinWithRareTransition;

    public static Results GetLatestResults() {
        Contract.Assert(PathToLatestCSVFile != null);
        string[] allLines = File.ReadAllLines(PathToLatestCSVFile);
        var allRows = allLines.Skip(1).Select(Trial.FromCSVLine).ToList();
        int incompleteTrials = allRows.Count(value => value == null);
        var trials = allRows.Where(value => value != null).ToList();
        return new Results(trials, incompleteTrials);
    }

    private Results(IReadOnlyCollection<Trial> trials, int incompleteTrials) {
        IncompleteTrials = incompleteTrials;
        CompleteTrials = trials.Count;
        if (CompleteTrials == 0) return;

        StartTime = trials.First().BeforeBlackScreen;
        EndTime = trials.Last().EndTime;
        TotalExperimentDuration = EndTime - StartTime;
        AverageTrialDuration = trials.Average(trial => trial.EndTime - trial.FirstChoiceShown);
        AverageWaitingTime = trials.Average(trial => trial.BlackScreenDuration);
        AverageReactionTimeFirstChoice = trials.Average(trial => trial.FirstReactionTime);
        AverageReactionTimeSecondChoice = trials.Average(trial => trial.SecondReactionTime);
        BestReactionTimeFirstChoice = trials.Min(trial => trial.FirstReactionTime);
        BestReactionTimeSecondChoice = trials.Min(trial => trial.SecondReactionTime);
        NumberOfWins = trials.Count(trial => trial.Win);
        NumberOfTrialsWithWinChanceGreaterThanHalf = trials.Count(trial => trial.WinChance > .5);
        AverageWinChance = trials.Average(trial => trial.WinChance);
        PercentageCommonTransition = trials.Percentage(trial => trial.CommonTransition);
        var data = GetComplexData(trials).ToList();

        if (data.Count == 0) return;
        PercentageSameChoiceAfterWinWithCommonTransition = data
            .Where(cd => cd.Win && cd.CommonTransition)
            .Percentage(cd => cd.FirstChoice == cd.NextFirstChoice);
        
        PercentageSameChoiceAfterWinWithRareTransition = data
            .Where(cd => cd.Win && !cd.CommonTransition)
            .Percentage(cd => cd.FirstChoice == cd.NextFirstChoice);
    }

    private struct ComplexData {
        public string FirstChoice, NextFirstChoice;
        public bool Win, CommonTransition;
    }

    private static IEnumerable<ComplexData> GetComplexData(IEnumerable<Trial> trials) {
        using var enumerator = trials.GetEnumerator();
        if (!enumerator.MoveNext()) yield break;

        string firstChoice = enumerator.Current.FirstChoice;
        bool win = enumerator.Current.Win;
        bool commonTransition = enumerator.Current.CommonTransition;
        while (enumerator.MoveNext()) {
            string nextChoice = enumerator.Current.FirstChoice;
            yield return new ComplexData {
                FirstChoice = firstChoice,
                NextFirstChoice = nextChoice,
                Win = win,
                CommonTransition = commonTransition
            };

            firstChoice = nextChoice;
            win = enumerator.Current.Win;
            commonTransition = enumerator.Current.CommonTransition;
        }
    }

    private class Trial {
        public DateTime BeforeBlackScreen, FirstChoiceShown, EndTime;
        public TimeSpan BlackScreenDuration, FirstReactionTime, SecondReactionTime;
        public bool Win, CommonTransition;
        public float WinChance;
        public string FirstChoice;
        
        public static Trial FromCSVLine(string line) {
            string[] columns = line.Split(',');
            if (!ParseBool(columns[1])) return null;

            return new Trial {
                BeforeBlackScreen = ParseDateTime(columns[2]),
                BlackScreenDuration = ParseTimeSpan(columns[3]),
                FirstChoiceShown = ParseDateTime(columns[4]),
                FirstReactionTime = ParseTimeSpan(columns[6]),
                FirstChoice = columns[7],
                CommonTransition = ParseBool(columns[8]),
                SecondReactionTime = ParseTimeSpan(columns[12]),
                WinChance = ParseProbability(columns[14]),
                Win = ParseBool(columns[15]),
                EndTime = ParseDateTime(columns[16])
            };
        }

        private static bool ParseBool(string value) => value == "yes";
        private static DateTime ParseDateTime(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture);
        private static TimeSpan ParseTimeSpan(string value) => TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        private static float ParseProbability(string value) => 
            float.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) / 100;
    }
}

public static class Extension {
    public static TimeSpan Average<T>(this IEnumerable<T> values, Func<T, TimeSpan> selector)
        => TimeSpan.FromSeconds(values.Select(selector).Average(ts => ts.TotalSeconds));

    public static TimeSpan Min<T>(this IEnumerable<T> values, Func<T, TimeSpan> selector)
        => TimeSpan.FromSeconds(values.Select(selector).Min(ts => ts.TotalSeconds));

    public static float Percentage<T>(this IEnumerable<T> values, Func<T, bool> selector) {
        (int tru, int count) = values.Select(selector)
            .Aggregate((0, 0), (data, yes) => (data.Item1 + (yes ? 1 : 0), data.Item2 + 1));
        return tru / (float) count;
    }
}