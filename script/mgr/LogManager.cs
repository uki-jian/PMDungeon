using UnityEngine;
using System.Collections.Generic;


public class CLogManager : MonoBehaviour
{
    public enum ELogLevel
    {
        Debug = 0,
        GameInfo,   //呈现给用户
        Warning,
        Error,
        Fatal
    }
    struct LogInfo
    {
        public string m_log;
        public ELogLevel m_level;

        public LogInfo(string log, ELogLevel level) { m_log = log; m_level = level; }
    }
    static Queue<LogInfo> m_queue;

    static bool m_bShowInEditor;

    static CLogManager()
    {
        m_queue = new Queue<LogInfo>();
        m_bShowInEditor = true;
    }
    public static void AddLog(string log, ELogLevel level = ELogLevel.GameInfo)
    {
        m_queue.Enqueue(new LogInfo(log, level));
    }
    public static void ShowLog()
    {
        LogInfo info;
        bool suc = m_queue.TryDequeue(out info);
        if (!suc) return;

        string log_level = string.Empty;
        switch(info.m_level)
        {
            case ELogLevel.Debug:
                log_level = "DEBUG";break;
            case ELogLevel.GameInfo:
                log_level = "INFO"; break;
            case ELogLevel.Warning:
                log_level = "WARNING"; break;
            case ELogLevel.Error:
                log_level = "ERROR"; break;
            case ELogLevel.Fatal:
                log_level = "FATAL"; break;
        }
        if (m_bShowInEditor)
        {
            Debug.Log($"[{log_level}] {info.m_log}");
        }
    }

    void Update()
    {
        while(m_queue.Count > 0)
        {
            ShowLog();
        }
    }
}
