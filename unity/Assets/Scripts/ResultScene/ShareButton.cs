using UnityEngine;
using UnityEngine.UI;
#if !UNITY_EDITOR
using System;
using System.IO;
#endif

[RequireComponent(typeof(Button))]
public class ShareButton : MonoBehaviour {
    private void Start() {
        GetComponent<Button>().onClick.AddListener(ShareResults);
    }

    private static void ShareResults() {
        string filePath = Results.PathToLatestCSVFile;
#if UNITY_EDITOR
        Debug.Log(Results.PathToLatestCSVFile);
#else
        var share = new NativeShare();
        share.SetTitle("Share Experiment Results");
        share.SetSubject($"Experiment Results Two Stage Task Game - {Path.GetFileName(filePath)}");
        share.SetText("Hi,\n\nyou will find my results attached to this message.\n\nGreetings");
        share.AddFile(filePath);
        share.SetCallback((result, target) => {
            if (result == NativeShare.ShareResult.Shared) {
                File.AppendAllText(Path.Combine(Application.persistentDataPath, "log.txt"), 
                    $"{DateTime.Now}: probably shared to {target}: {Results.PathToLatestCSVFile}");
            }
        });
        share.Share();
#endif

    }

}