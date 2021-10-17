using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ResultsText : MonoBehaviour {
    private void Start() {
        var results = Results.GetLatestResults();
        string text =
            $"This Experiment was conducted for ~{results.TotalExperimentDuration.TotalMinutes:F0}min.\n" +
            $"You achieved a cumulative reward of {results.NumberOfWins} Wins.\n\n" +
            $"Variation: {GetExperimentVariation()}\n" +
            $"Number of Trials: {results.CompleteTrials}{(results.IncompleteTrials > 0 ? $" (+ {results.IncompleteTrials} aborted)" : "")}\n";

        if (results.CompleteTrials > 0) text +=
            $"Start Time: {results.StartTime}\n" +
            $"End Time: {results.EndTime}\n" +
            $"Duration: {results.TotalExperimentDuration}\n" +
            $"Average Inter Trial Time: {results.AverageWaitingTime.TotalSeconds:F2}s\n" +
            $"Average Trial Time: {results.AverageTrialDuration.TotalSeconds:F2}s\n" +
            $"Average Reaction Times: {results.AverageReactionTimeFirstChoice.TotalSeconds:F2}s, {results.AverageReactionTimeSecondChoice.TotalSeconds:F2}s\n" +
            $"Best Reaction Times: {results.BestReactionTimeFirstChoice.TotalSeconds:F2}s, {results.BestReactionTimeSecondChoice.TotalSeconds:F2}s\n" +
            $"Number of Wins: {results.NumberOfWins}\n" +
            $"Number of times best second cue chosen: {results.NumberOfTrialsWithWinChanceGreaterThanHalf}\n" +
            $"Average Win Chance: {results.AverageWinChance * 100:F1}%\n" +
            $"Percentage Common Transition: {results.PercentageCommonTransition * 100:F1}%\n\n";
        
        if (results.CompleteTrials > 1) text +=
            $"Percentage Same Choice after Win with Common Transition: {results.PercentageSameChoiceAfterWinWithCommonTransition * 100:F1}%\n" +
            $"Percentage Same Choice after Win with Rare Transition: {results.PercentageSameChoiceAfterWinWithRareTransition * 100:F1}%";

        GetComponent<Text>().text = text;
    }

    private static string GetExperimentVariation() {
        string fileName = Path.GetFileName(Results.PathToLatestCSVFile);
        return fileName.Substring(17, Math.Min(0, fileName.Length - 17 - 9 - 37));
    }
}