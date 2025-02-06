using UnityEngine;
using System.Collections.Generic;


public class CLogManager : MonoBehaviour
{
    public enum ELogSplit
    {
        Wrap = 0
    }
    struct LogInfo
    {
        public string m_log;
        public ELogSplit m_split;

        public LogInfo(string log, ELogSplit split) { m_log = log; m_split = split; }
    }
    static Queue<LogInfo> m_queue;

    static bool m_bShowInEditor;

    static CLogManager()
    {
        m_queue = new Queue<LogInfo>();
        m_bShowInEditor = true;
    }
    public static void AddLog(string log, ELogSplit split = ELogSplit.Wrap)
    {
        m_queue.Enqueue(new LogInfo(log, split));
    }
    public static void ShowLog()
    {
        LogInfo info;
        bool suc = m_queue.TryDequeue(out info);
        if (!suc) return;

        if (m_bShowInEditor)
        {
            Debug.Log(info.m_log);
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
