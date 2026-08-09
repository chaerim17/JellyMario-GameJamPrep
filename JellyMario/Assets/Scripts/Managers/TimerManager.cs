using UnityEngine;
using JellyMario.Core;

public class TimerManager : Singleton<TimerManager>
{
    private float currentTime;
    private bool isRunning;

    public float CurrentTime => currentTime;

    private void Update()
    {
        if (!isRunning)
            return;

        currentTime += Time.deltaTime;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = 0f;
    }
}