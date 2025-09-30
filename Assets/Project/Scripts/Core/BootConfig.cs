using UnityEngine;

/// <summary>
/// Boot settings applied on application start.
/// Disables runtime logging expect for errors on in non-development build to improve performance.
/// Also configures other global values (VSync off, 30 fps cap and interval at which physics updates)
/// </summary>
public class BootConfig : MonoBehaviour
{

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Configures the following global timings: Disables VSync, caps frame rate to 30 FPS, 
    /// sets the physics fixed timestep to 1/60 (60 updates per second) and disables runtime logging
    /// except for errors.
    /// </summary>
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        Time.fixedDeltaTime = 1f / 60f;

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = true;
        Debug.unityLogger.filterLogType = LogType.Error;

        Application.SetStackTraceLogType(LogType.Log,      StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning,  StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Error,    StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Assert,   StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Exception,StackTraceLogType.None);
#endif
    }
}
