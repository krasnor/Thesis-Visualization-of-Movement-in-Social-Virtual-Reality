using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class ModeSelector : MonoBehaviour
{
    [SerializeField]
    private Mode m_mode = Mode.None;

    [SerializeField]
    private GameObject m_analysisManager;

    [SerializeField]
    private GameObject m_loggingManager;

    public Mode Mode { get => m_mode; private set => m_mode = value; }

    void Start()
    {
        CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
        culture.DateTimeFormat.LongTimePattern = "HH:mm:ss.fff";
        Thread.CurrentThread.CurrentCulture = culture;
        Debug.Log(DateTime.UtcNow);

        switch (m_mode)
        {
            case Mode.None:
                break;
            case Mode.Logging:
                m_loggingManager.SetActive(true);
                break;
            case Mode.Analysis:
                m_analysisManager.SetActive(true);
                break;
            default:
                break;
        }
    }
}

public enum Mode
{
    None, Logging, Analysis
}
