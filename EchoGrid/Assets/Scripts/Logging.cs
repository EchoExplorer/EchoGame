// Comment out this line to disable logging altogether
#define DEBUG

using UnityEngine;

/// <summary>
/// A small class to allow debug messages of multiple priority levels to be created.
/// </summary>
public class Logging {

    /// <summary>
    /// An enum of logging priority levels that can be used.
    /// </summary>
    public enum LogLevel {
        CRITICAL = 5,
        WARNING = 4,
        ABNORMAL = 3,
        NORMAL = 2,
        LOW_PRIORITY = 1,
        VERBOSE = 0
    }

    /// <summary>
    /// The logging threshold. Any message with equal or higher priority will be displayed,
    ///  and any message with lesser priority will be suppressed.
    /// </summary>
    public const LogLevel THRESHOLD = LogLevel.VERBOSE;

    /// <summary>
    /// Displays a logging message with <code>Debug.Log</code> with some priority level.
    /// </summary>
    /// <param name="message">The message to display in the log</param>
    /// <param name="priority">The priority of the message</param>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Log(string message, LogLevel priority) {
        if (priority >= THRESHOLD) {
            Debug.Log(message);
        }
    }
}
