using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;

    private float elapsedTime;
    private bool isRunning;

    public float CurrentTime => elapsedTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetClearTime()
    {
        return elapsedTime;
    }
}