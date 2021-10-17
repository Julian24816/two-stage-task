using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GotoResultsSceneOnClick : MonoBehaviour, IPointerClickHandler {
    [Scene] public string ResultsScene;
    public string Path;
    
    public void OnPointerClick(PointerEventData eventData) {
        Results.PathToLatestCSVFile = Path;
        SceneManager.LoadScene(ResultsScene);
    }
}