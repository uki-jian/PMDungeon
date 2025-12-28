using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;

public class CLogManager : MonoBehaviour
{
    /// <summary>
    /// 日志等级枚举，从高到低：Error > Warning > Info > Debug
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,  // 最低等级，输出所有日志
        Info = 1,
        Warning = 2,
        Error = 3   // 最高等级，只输出错误日志
    }

    /// <summary>
    /// 当前日志等级，默认输出所有日志（Debug级别）
    /// </summary>
    private static LogLevel m_currentLogLevel = LogLevel.Debug;

    /// <summary>
    /// 设置日志等级，只输出大于等于该等级的日志
    /// </summary>
    public static void SetLogLevel(LogLevel level)
    {
        m_currentLogLevel = level;
    }

    /// <summary>
    /// 获取当前日志等级
    /// </summary>
    public static LogLevel GetLogLevel()
    {
        return m_currentLogLevel;
    }

    /// <summary>
    /// 检查指定等级是否应该输出
    /// </summary>
    private static bool ShouldLog(LogLevel level)
    {
        return (int)level >= (int)m_currentLogLevel;
    }

    /// <summary>
    /// 从文件路径中提取文件名
    /// </summary>
    private static string GetFileNameFromPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return "Unknown";
        
        string fileName = Path.GetFileName(filePath);
        return fileName;
    }

    /// <summary>
    /// 格式化日志消息，格式为：[等级] 消息内容 [文件名:行号]
    /// </summary>
    private static string FormatLogMessage(string level, string message, string fileName, int lineNumber)
    {
        return $"[{level}] {message} [{fileName}:{lineNumber}]";
    }

    /// <summary>
    /// 输出Info级别日志
    /// </summary>
    public static void LogInfo(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (!ShouldLog(LogLevel.Info))
            return;
        
        string fileName = GetFileNameFromPath(filePath);
        Debug.Log(FormatLogMessage("INFO", message, fileName, lineNumber));
    }

    /// <summary>
    /// 输出Warning级别日志
    /// </summary>
    public static void LogWarning(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (!ShouldLog(LogLevel.Warning))
            return;
        
        string fileName = GetFileNameFromPath(filePath);
        Debug.LogWarning(FormatLogMessage("WARNING", message, fileName, lineNumber));
    }

    /// <summary>
    /// 输出Error级别日志
    /// </summary>
    public static void LogError(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (!ShouldLog(LogLevel.Error))
            return;
        
        string fileName = GetFileNameFromPath(filePath);
        Debug.LogError(FormatLogMessage("ERROR", message, fileName, lineNumber));
    }

    /// <summary>
    /// 输出Debug级别日志（仅在Editor和Development Build中输出）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogDebug(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (!ShouldLog(LogLevel.Debug))
            return;
        
        string fileName = GetFileNameFromPath(filePath);
        Debug.Log(FormatLogMessage("DEBUG", message, fileName, lineNumber));
    }

    // 保留旧的AddLog方法以保持兼容性（已废弃，建议使用新的Log方法）
    [System.Obsolete("请使用LogInfo、LogWarning、LogError或LogDebug方法")]
    public static void AddLog(string log, ELogLevel level = ELogLevel.GameInfo)
    {
        switch(level)
        {
            case ELogLevel.Debug:
                LogDebug(log);
                break;
            case ELogLevel.GameInfo:
                LogInfo(log);
                break;
            case ELogLevel.Warning:
                LogWarning(log);
                break;
            case ELogLevel.Error:
                LogError(log);
                break;
            case ELogLevel.Fatal:
                LogError($"[FATAL] {log}");
                break;
        }
    }

    public enum ELogLevel
    {
        Debug = 0,
        GameInfo,
        Warning,
        Error,
        Fatal
    }
}
