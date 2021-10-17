using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ListPreviousResults : MonoBehaviour {
    public Text Template;

    private void Start() {
        Template.gameObject.SetActive(false);

        int row = 0;
        foreach (string resultsPath in CsvFileCreator.AllPaths) {
            var newText = Instantiate(Template.gameObject, Template.transform.parent).GetComponent<Text>();
            newText.gameObject.SetActive(true);
            newText.name = Path.GetFileName(resultsPath);
            newText.text = newText.name.Substring(0, Math.Max(0, newText.name.Length - 9 - 37));
            var pos = newText.rectTransform.anchoredPosition;
            pos.y = row++ * -Template.rectTransform.sizeDelta.y;
            newText.rectTransform.anchoredPosition = pos;
            newText.GetComponent<GotoResultsSceneOnClick>().Path = resultsPath;
        }

        var parentRectTransform = Template.rectTransform.parent.gameObject.GetComponent<RectTransform>();
        var sizeDelta = parentRectTransform.sizeDelta;
        sizeDelta.y = row * Template.rectTransform.sizeDelta.y;
        parentRectTransform.sizeDelta = sizeDelta;
    }
}