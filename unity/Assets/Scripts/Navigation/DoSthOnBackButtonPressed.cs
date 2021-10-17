using UnityEngine;

public abstract class DoSthOnBackButtonPressed : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) OnBackButtonPressed();
    }

    protected abstract void OnBackButtonPressed();
}