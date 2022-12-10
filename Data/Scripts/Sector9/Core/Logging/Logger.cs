using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Utils;

namespace Sector9.Core.Logging
{
    internal class Logger
    {
        public enum Severity : int
        {
            Info = 0,
            Warning = 1,
            Error = 2,
            Fatal = 3
        }

        public enum LogType
        {
            Player,
            Server,
            System
        }

        private struct LogMessage
        {
            public LogMessage(string message, Severity severity)
            {
                Message = message;
                Severity = severity;
            }

            public string Message { get; }
            public Severity Severity { get; }
        }

        private static List<LogMessage> MessageQueue = new List<LogMessage>();
        private static object QueueLock = new object();
        private static bool ServerLog = true;
        private static bool PlayerLog = true;

        /// <summary>
        /// Write the logs async to IO
        /// </summary>
        public static void CycleLogs()
        {
            MyAPIGateway.Parallel.Start(() =>
            {
                ProcessQueue();
            });
        }

        /// <summary>
        /// write the logs to IO
        /// </summary>
        public static void CycleLogsBlocking()
        {
            ProcessQueue();
        }

        /// <summary>
        /// Toggle loggin on/off for server and player
        /// </summary>
        /// <param name="server">Enalbe server logging</param>
        /// <param name="player">enalbe player logging</param>
        public static void SetLogTypes(bool server, bool player)
        {
            ServerLog = server;
            PlayerLog = player;
        }

        public static void Log(string message, Severity severity, LogType type)
        {
            if ((type == LogType.Player && !PlayerLog && severity < Severity.Error) || (type == LogType.Server && !ServerLog && severity < Severity.Error))
            {
                return; //don't log
            }

            lock (QueueLock)
            {
                MessageQueue.Add(new LogMessage(message, severity));
            }

            ProcessQueue(); //todo remove once done with debugging
        }

        public static void ProcessQueue()
        {
            lock (MessageQueue)
            {
                for (int i = 0; i < MessageQueue.Count; i++)
                {
                    LogMessage message = MessageQueue[i];
                    WriteToIo(message.Severity, $"S9. {message.Message}");
                }
                MessageQueue.Clear();
            }
        }

        private static void WriteToIo(Severity severity, string message)
        {
            switch (severity)
            {
                case Severity.Info:
                    MyLog.Default.Info(message);
                    break;

                case Severity.Warning:
                    MyLog.Default.Warning(message);
                    break;

                case Severity.Error:
                    MyLog.Default.Error(message);
                    break;

                case Severity.Fatal:
                    MyLog.Default.Critical(message);
                    break;

                default:
                    MyLog.Default.Error($"Got unkown log request for {severity} with message: '{message}'");
                    break;
            }
        }
    }
}