using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GotoSceneButton : MonoBehaviour {
    [Scene] public string Scene;
    private void Start() => GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(Scene));
}
