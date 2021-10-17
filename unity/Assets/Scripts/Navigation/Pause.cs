using System;
using System.Linq;
using GF;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour {
    public Button PauseButton;
    public GameObject PauseMenu;

    public Text StatsText;
    public Button ResumeButton, EndButton;
    [Scene] public string ResultsScene;
    private TwoStageTask _experimentController;
    private DataCollector _dataCollector;

    private void Awake() {
        PauseMenu.SetActive(false);
        
        _experimentController = FindObjectOfType<TwoStageTask>();
        _dataCollector = FindObjectOfType<DataCollector>();

        PauseButton.onClick.AddListener(ShowMenu);
        ResumeButton.onClick.AddListener(HideMenu);
        EndButton.onClick.AddListener(() => SceneManager.LoadScene(ResultsScene));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !PauseMenu.activeSelf) ShowMenu();
    }

    private void ShowMenu() {
        _experimentController.PauseExperiment();
        PauseMenu.SetActive(true);
        var playtime = TimeSpan.Zero;
        if (_dataCollector.AllTrials.Any(trial => trial.Completed)) playtime = 
            _dataCollector.AllTrials.Last(trial => trial.Completed).TimeEnd
            - _dataCollector.AllTrials.First(trial => trial.Completed).TimeBeforeBlackScreen;
        StatsText.text = $"{_experimentController.SuccessfulTrialCount} / {_experimentController.TargetSuccessfulTrialCount} Trials, " +
                         $"~{playtime.TotalMinutes:F0} min Playtime";
    }
    
    private void HideMenu() {
        PauseMenu.SetActive(false);
        _experimentController.StartExperiment();
    }
}