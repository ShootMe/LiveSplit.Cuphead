using System;
using System.Collections.Generic;
using System.IO;
namespace LiveSplit.Cuphead {
    public enum LogObject {
        CurrentSplit,
        Pointers,
        Version,
        Loading,
        InGame,
        Scene,
        LevelEnding,
        LevelWon,
        LevelMode
    }
    public class LogManager {
        public const string LOG_FILE = "Cuphead.txt";
        private Dictionary<LogObject, string> currentValues = new Dictionary<LogObject, string>();
        private bool enableLogging;
        public bool EnableLogging {
            get { return enableLogging; }
            set {
                if (value != enableLogging) {
                    enableLogging = value;
                    if (value) {
                        AddEntryUnlocked(new EventLogEntry("Initialized"));
                    }
                }
            }
        }

        public LogManager() {
            EnableLogging = false;
            Clear();
        }
        public void Clear(bool deleteFile = false) {
            lock (currentValues) {
                if (deleteFile) {
                    try {
                        File.Delete(LOG_FILE);
                    } catch { }
                }
                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    currentValues[key] = null;
                }
            }
        }
        public void AddEntry(ILogEntry entry) {
            lock (currentValues) {
                AddEntryUnlocked(entry);
            }
        }
        private void AddEntryUnlocked(ILogEntry entry) {
            string logEntry = entry.ToString();
            if (EnableLogging) {
                try {
                    using (StreamWriter sw = new StreamWriter(LOG_FILE, true)) {
                        sw.WriteLine(logEntry);
                    }
                } catch { }
                Console.WriteLine(logEntry);
            }
        }
        public void Update(LogicManager logic, SplitterSettings settings) {
            if (!EnableLogging) { return; }

            lock (currentValues) {
                DateTime date = DateTime.Now;
                bool updateLog = true;

                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    string previous = currentValues[key];
                    string current = null;

                    switch (key) {
                        case LogObject.CurrentSplit: current = $"{logic.CurrentSplit} ({GetCurrentSplit(logic, settings)})"; break;
                        case LogObject.Pointers: current = logic.Memory.GamePointers(); break;
                        case LogObject.Version: current = MemoryManager.Version.ToString(); break;
                        case LogObject.Loading: current = logic.Memory.Loading().ToString(); break;
                        case LogObject.InGame: current = updateLog ? logic.Memory.InGame().ToString() : previous; break;
                        case LogObject.Scene: current = updateLog ? logic.Memory.SceneName() : previous; break;
                        case LogObject.LevelEnding: current = updateLog ? logic.Memory.LevelEnding().ToString() : previous; break;
                        case LogObject.LevelWon: current = updateLog ? logic.Memory.LevelWon().ToString() : previous; break;
                        case LogObject.LevelMode: current = updateLog ? logic.Memory.LevelMode().ToString() : previous; break;
                    }

                    if (previous != current) {
                        AddEntryUnlocked(new ValueLogEntry(date, key, previous, current));
                        currentValues[key] = current;
                    }
                }
            }
        }
        private string GetCurrentSplit(LogicManager logic, SplitterSettings settings) {
            if (logic.CurrentSplit >= settings.Splits.Count) { return "N/A"; }
            return settings.Splits[logic.CurrentSplit].ToString();
        }
    }
    public interface ILogEntry { }
    public class ValueLogEntry : ILogEntry {
        public DateTime Date;
        public LogObject Type;
        public object PreviousValue;
        public object CurrentValue;

        public ValueLogEntry(DateTime date, LogObject type, object previous, object current) {
            Date = date;
            Type = type;
            PreviousValue = previous;
            CurrentValue = current;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": (",
                Type.ToString(),
                ") ",
                PreviousValue,
                " -> ",
                CurrentValue
            );
        }
    }
    public class EventLogEntry : ILogEntry {
        public DateTime Date;
        public string Event;

        public EventLogEntry(string description) {
            Date = DateTime.Now;
            Event = description;
        }
        public EventLogEntry(DateTime date, string description) {
            Date = date;
            Event = description;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": ",
                Event
            );
        }
    }
}
