using System.Collections.Generic;
using GF;
using UnityEngine;

public class RewardController : MonoBehaviour {
    private TwoStageTask _experimentController;
    public Progressbar Progressbar;
    public List<GameObject> Stars;
    public GameObject Reward;
    public GameObject NoReward;
    public ParticleSystem RewardParticles;
    public float ShowNextStarEveryRewardAmount = 10;

    private void Awake() {
        _experimentController = FindObjectOfType<TwoStageTask>();
        _experimentController.OnShowInterTrialScreen += _ => HideReward();
        _experimentController.OnShowState += (_, __) => HideReward();
        _experimentController.OnShowReward += OnShowReward;
        _experimentController.OnSkippedReward += OnSkippedReward;
    }

    private void Start() => SetDisplayedReward(_experimentController.CumulativeRewards);

    private void HideReward() {
        Reward.SetActive(false);
        NoReward.SetActive(false);
    }
    
    private void OnSkippedReward(Reward reward) => SetDisplayedReward(_experimentController.CumulativeRewards);

    private void OnShowReward(BinaryReward reward, float time) {
        var rewardGO = reward.Win ? Reward : NoReward;
        rewardGO.SetActive(true);
        rewardGO.GetComponent<AudioSource>()?.Play();
        if (!reward.Win) return;

        RewardParticles.Play();
        float stopReward = _experimentController.CumulativeRewards;
        float startReward = stopReward - reward.Value;
        StartCoroutine(AnimationUtil.LinearAction(time, t => SetDisplayedReward(Mathf.Lerp(startReward, stopReward, t))));
    }

    private void SetDisplayedReward(float cumulativeReward) {
        cumulativeReward = Mathf.Round(cumulativeReward * 100) / 100;
        Progressbar.Progress = cumulativeReward / ShowNextStarEveryRewardAmount % 1;
        int stars = (int) (cumulativeReward / ShowNextStarEveryRewardAmount);
        for (int i = 0; i < Stars.Count; i++) Stars[i].SetActive(i < stars);
    }
}
