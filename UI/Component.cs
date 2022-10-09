using LiveSplit.Model;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Cuphead {
    public class Component : UI.Components.IComponent {
        public TimerModel Model { get; set; }
        public string ComponentName { get { return "Cuphead Autosplitter"; } }
        public IDictionary<string, Action> ContextMenuControls { get { return null; } }
        private LogicManager logic;
        private LogManager log;
        private SplitterSettings settings;
        private bool isRunning = false;
        private bool shouldLog = false;
        private bool isAutosplitting = false;
#if Info
        public static void Main(string[] args) {
            Component component = new Component(null);
            component.log.EnableLogging = true;
            Application.Run();
        }
#endif

        public Component(LiveSplitState state) {
            log = new LogManager();
            settings = new SplitterSettings();
            logic = new LogicManager(settings);

            if (state != null) {
                Model = new TimerModel() { CurrentState = state };
                Model.InitializeGameTime();
                Model.CurrentState.IsGameTimePaused = true;
                state.OnReset += OnReset;
                state.OnPause += OnPause;
                state.OnResume += OnResume;
                state.OnStart += OnStart;
                state.OnSplit += OnSplit;
                state.OnUndoSplit += OnUndoSplit;
                state.OnSkipSplit += OnSkipSplit;
            }

            StartAutosplitter();
        }

        public void WaitForLogic() {
            lock (logic) {
                while (!shouldLog && isRunning) { Monitor.Wait(logic); }
                shouldLog = false;
            }
        }
        public void PulseLog() {
            lock (logic) {
                shouldLog = true;
                Monitor.PulseAll(logic);
            }
        }
        public void StartAutosplitter() {
            if (isRunning) { return; }
            isRunning = true;

            Task.Factory.StartNew(delegate () {
                try {
                    while (isRunning) {
                        try {
                            if (logic.IsHooked()) {
                                int currentSplit = Model == null ? 0 : Model.CurrentState.CurrentPhase == TimerPhase.NotRunning ? 0 : Model.CurrentState.CurrentSplitIndex + 1;
                                logic.Update(currentSplit);
                                PulseLog();
                            }
                            HandleLogic();
                        } catch (Exception ex) {
                            log.AddEntry(new EventLogEntry(ex.ToString()));
                        }
                        Thread.Sleep(7);
                    }
                } catch { }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(delegate () {
                try {
                    while (isRunning) {
                        try {
                            WaitForLogic();
                            log.Update(logic, settings);
                        } catch (Exception ex) {
                            log.AddEntry(new EventLogEntry(ex.ToString()));
                        }
                    }
                } catch { }
            }, TaskCreationOptions.LongRunning);
        }
        private void HandleLogic() {
            if (Model == null) { return; }

            Model.CurrentState.IsGameTimePaused = logic.Paused;
            if (logic.GameTime >= 0) {
                Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(logic.GameTime));
            }

            if (logic.ShouldReset) {
                isAutosplitting = true;
                Model.Reset();
            } else if (logic.ShouldSplit) {
                isAutosplitting = true;
                if (Model.CurrentState.CurrentPhase == TimerPhase.NotRunning) {
                    Model.Start();
                } else {
                    Model.Split();
                }
            }
            isAutosplitting = false;
        }
        private void HandleGameTimes() {
            if (logic.CurrentSplit > 0 && logic.CurrentSplit <= Model.CurrentState.Run.Count && Model.CurrentState.Run.Count == 1) {
                TimeSpan gameTime = TimeSpan.FromSeconds(logic.Memory.LevelTime());
                if (logic.CurrentSplit == 1) {
                    Time t = Model.CurrentState.Run[0].SplitTime;
                    Model.CurrentState.Run[0].SplitTime = new Time(t.RealTime, gameTime);
                } else {
                    Model.CurrentState.SetGameTime(gameTime);
                }
            }
        }
        public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) { }
        public void OnReset(object sender, TimerPhase e) {
            logic.Reset();
            if (!isAutosplitting) {
                log.AddEntry(new EventLogEntry("Reset Splits Manual"));
            } else {
                log.AddEntry(new EventLogEntry("Reset Splits Auto"));
            }
        }
        public void OnResume(object sender, EventArgs e) {
            log.AddEntry(new EventLogEntry("Resumed Splits"));
        }
        public void OnPause(object sender, EventArgs e) {
            log.AddEntry(new EventLogEntry("Paused Splits"));
        }
        public void OnStart(object sender, EventArgs e) {
            Model.CurrentState.SetGameTime(TimeSpan.Zero);
            Model.CurrentState.IsGameTimePaused = true;
            if (!isAutosplitting) {
                logic.Increment();
                log.AddEntry(new EventLogEntry("Started Splits Manual " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            } else {
                log.AddEntry(new EventLogEntry("Started Splits Auto " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            }
        }
        public void OnUndoSplit(object sender, EventArgs e) {
            logic.Decrement();
            log.AddEntry(new EventLogEntry($"Undo Current Split {Model.CurrentState.CurrentTime.RealTime.Value}"));
        }
        public void OnSkipSplit(object sender, EventArgs e) {
            logic.Increment();
            log.AddEntry(new EventLogEntry($"Skip Current Split {Model.CurrentState.CurrentTime.RealTime.Value}"));
        }
        public void OnSplit(object sender, EventArgs e) {
            if (!isAutosplitting) {
                logic.Increment();
                log.AddEntry(new EventLogEntry($"Split Manual {Model.CurrentState.CurrentTime.RealTime.Value}"));
            } else {
                log.AddEntry(new EventLogEntry($"Split Auto {Model.CurrentState.CurrentTime.RealTime.Value}"));
            }
            HandleGameTimes();
        }
        public Control GetSettingsControl(LayoutMode mode) { return settings; }
        public void SetSettings(XmlNode document) { settings.SetSettings(document); }
        public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
        public float HorizontalWidth { get { return 0; } }
        public float MinimumHeight { get { return 0; } }
        public float MinimumWidth { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }
        public float PaddingTop { get { return 0; } }
        public float VerticalHeight { get { return 0; } }
        public void Dispose() {
            if (Model != null) {
                Model.CurrentState.OnReset -= OnReset;
                Model.CurrentState.OnPause -= OnPause;
                Model.CurrentState.OnResume -= OnResume;
                Model.CurrentState.OnStart -= OnStart;
                Model.CurrentState.OnSplit -= OnSplit;
                Model.CurrentState.OnUndoSplit -= OnUndoSplit;
                Model.CurrentState.OnSkipSplit -= OnSkipSplit;
                Model = null;
            }
            settings.Dispose();
        }
    }
}