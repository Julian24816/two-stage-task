#if !UNITY_EDITOR
using UnityEngine;
#endif

public class QuitApplicationOnBackButton : DoSthOnBackButtonPressed {
    protected override void OnBackButtonPressed() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
