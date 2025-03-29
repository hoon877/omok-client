using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    // [Serializable]
    // public class FillSettings
    // {
    //     public Color color;
    // }
    // public FillSettings fillSettings;
    //
    // [Serializable]
    // public class BackgroundSettings
    // {
    //     public Color color;
    // }
    // public BackgroundSettings backgroundSettings;

    [SerializeField] private Image fillImage;
    [SerializeField] private float totalTime;
    [SerializeField] private Image headCapImage;
    [SerializeField] private Image tailCapImage;
    [SerializeField] private TMP_Text timeText;
    
    public float CurrentTime { get; private set; }
    private bool _isPaused;

    public delegate void TimerDelegate();

    public TimerDelegate OnTimeout;
    private void Awake()
    {
        _isPaused = true;
    }
    private void Update()
    {
        if(!_isPaused)
        {
            CurrentTime += Time.deltaTime;
            if(CurrentTime >= totalTime)
            {
                headCapImage.gameObject.SetActive(false);
                tailCapImage.gameObject.SetActive(false);
                _isPaused = true;
                OnTimeout?.Invoke();
            }
            else
            {
                fillImage.fillAmount = 1 - CurrentTime / totalTime;
                headCapImage.transform.localRotation = Quaternion.Euler(new Vector3(0,0,-fillImage.fillAmount * 360));
                var timeTextTime = totalTime - CurrentTime;
                timeText.text = timeTextTime.ToString("F0");
            }
        }
    }

    public void StartTImer()
    {
        _isPaused = false;
        headCapImage.gameObject.SetActive(true);
        tailCapImage.gameObject.SetActive(true);
    }
    public void PauseTimer()
    {
        _isPaused = true;
    }
    public void InitTimer()
    {
        CurrentTime = 0;
        fillImage.fillAmount = 1;
        timeText.text = totalTime.ToString("F0");
        headCapImage.gameObject.SetActive(false);
        tailCapImage.gameObject.SetActive(false);
        _isPaused = true;
    }
}
