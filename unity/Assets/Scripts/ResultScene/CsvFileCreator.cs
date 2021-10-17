using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DataCollector))]
public class CsvFileCreator : MonoBehaviour {
    private static Guid DeviceId { get {
        if (!PlayerPrefs.HasKey("DeviceId")) PlayerPrefs.SetString("DeviceId", Guid.NewGuid().ToString());
        return new Guid(PlayerPrefs.GetString("DeviceId"));
    }}
    
    public string GetCSVFileContent() {
        string result = "TrialNumber,Completed,Timestamp Before Black Screen,Black Screen Duration,Timestamp First Choice Shown," +
                        "Timestamp First Decision,First Choice Reaction Time,First Choice,Common Transition,Second Stage," +
                        "Timestamp Second Choice Shown,Timestamp Second Decision,Second Decision Reaction Time,Second Choice," +
                        "Reward Probability,Reward,Timestamp End\n";

        foreach (var trial in GetComponent<DataCollector>().AllTrials) {
            result += $"{trial.TrialNumber},{Format(trial.Completed)},{Format(trial.TimeBeforeBlackScreen)},";
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.FirstShown, 
                Format(trial.BlackScreenDuration), Format(trial.TimeFirstChoiceShown));
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.FirstDecision,
                Format(trial.TimeFirstDecision), Format(trial.FirstReactionTime), trial.FirstChoice);
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.FirstTransition,
                Format(trial.CommonTransition), trial.SecondStage);
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.SecondShown,
                Format(trial.TimeSecondChoiceShown));
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.SecondDecision,
                Format(trial.TimeSecondDecision), Format(trial.SecondReactionTime), trial.SecondChoice);
            AddIfFilled(ref result, trial, DataCollector.Trial.FillState.SecondTransition,
                Format(trial.RewardProbability), Format(trial.Win));
            result += $"{Format(trial.TimeEnd)}\n";
        }

        return result;
    }

    public string SaveCSVFile(string fileContent = null, string fileName = null) {
        fileContent ??= GetCSVFileContent();
        fileName ??=$"{DateTime.Now:yyyy'-'MM'-'dd'-'HH'-'mm}-{SceneManager.GetActiveScene().name}-{DeviceId}-data";
        if (!Directory.Exists(Application.persistentDataPath)) Directory.CreateDirectory(Application.persistentDataPath);
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.csv");
        using var writer = new StreamWriter(filePath);
        writer.Write(fileContent);
        return filePath;
    }

    private void OnDisable() => Results.PathToLatestCSVFile = SaveCSVFile();

    public static IEnumerable<string> AllPaths => Directory.EnumerateFiles(Application.persistentDataPath)
        .Where(path => Path.GetExtension(path).Equals(".csv"));

    private static void AddIfFilled(ref string result, DataCollector.Trial trial, DataCollector.Trial.FillState target,
        params string [] values) {
        if ((int) trial.State >= (int) target) foreach (string value in values) result += value + ",";
        else result += new string(',', values.Length);
    }

    private static string Format(bool value) => value ? "yes" : "no";
    private static string Format(DateTime time) => FormattableString.Invariant($"{time}");
    private static string Format(TimeSpan time) => FormattableString.Invariant($"{time}");
    private static string Format(float probability) => FormattableString.Invariant($"{probability * 100:F1}%");
}