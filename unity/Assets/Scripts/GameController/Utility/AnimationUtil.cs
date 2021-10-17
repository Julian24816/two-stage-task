using System;
using System.Collections;
using UnityEngine;

public static class AnimationUtil {
    public static IEnumerator LinearAction(float time, Action<float> callback) {
        float startTime = Time.time;
        float endTime = Time.time + time;
        while (Time.time < endTime) {
            callback(Mathf.InverseLerp(startTime, endTime, Time.time));
            yield return null;
        }

        callback(1);
    }

    public static IEnumerator AfterDelay(float time, Action callback) {
        float endTime = Time.time + time;
        while (Time.time < endTime) yield return null;

        callback();
    }

    public static IEnumerator Infinite(Action<float> callback) {
        float previousTime = Time.time;
        while (true) {
            yield return null;

            callback(Time.time - previousTime);
            previousTime = Time.time;
        }
    }
}