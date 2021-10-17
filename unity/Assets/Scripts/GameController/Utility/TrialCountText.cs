using GF;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TrialCountText : MonoBehaviour {
    private void Awake() {
        var text = GetComponent<Text>();
        var experimentController = FindObjectOfType<TwoStageTask>();
        experimentController.OnShowInterTrialScreen += _ => 
            text.text = $"{experimentController.SuccessfulTrialCount + 1} / {experimentController.TargetSuccessfulTrialCount}";
    }
}
