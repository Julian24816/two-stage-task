using System.Linq;
using UnityEngine;

public class HideIfNoPreviousResults : MonoBehaviour {
    private void Start() {
        if (!CsvFileCreator.AllPaths.Any()) gameObject.SetActive(false);
    }
}