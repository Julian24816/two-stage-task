using GF;
using UnityEngine;

public class RewardSounds : MonoBehaviour {
    public AudioSource Reward, NoReward;

    private void Awake() {
        var experimentController = FindObjectOfType<TwoStageTask>();
        experimentController.OnShowReward += (reward, _) => { (reward.Value > 0 ? Reward : NoReward).Play(); };
    }
}