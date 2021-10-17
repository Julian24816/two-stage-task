using UnityEngine.SceneManagement;

public class GotoSceneOnBackButton : DoSthOnBackButtonPressed {
    [Scene] public string Scene;
    protected override void OnBackButtonPressed() => SceneManager.LoadScene(Scene);
}