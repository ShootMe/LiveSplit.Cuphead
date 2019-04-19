#if !Info
using LiveSplit.Model;
using LiveSplit.UI;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Cuphead {
#if !Info
	public class SplitterComponent : UI.Components.IComponent {
		public TimerModel Model { get; set; }
#else
	public class SplitterComponent {
#endif
		public string ComponentName { get { return "Cuphead Autosplitter"; } }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private static string LOGFILE = "_Cuphead.txt";
		internal static string[] keys = { "CurrentSplit", "State", "InGame", "Scene", "LevelEnding", "LevelWon" };
		private SplitterMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0;
		private bool hasLog = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplitterSettings settings;
#if !Info
		private bool lastInGame, lastLoading;
		private string lastSceneName, lastSceneSeen;
		private float lastLevelTime;

		public SplitterComponent(LiveSplitState state) {
#else
		public SplitterComponent() {
#endif
			mem = new SplitterMemory();
			settings = new SplitterSettings();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

#if !Info
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
#endif
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

#if !Info
			if (Model != null) {
				HandleSplits();
			}
#endif

			LogValues();
		}
#if !Info
		private void HandleSplits() {
			bool shouldSplit = false;
			bool inGame = mem.InGame();
			float levelTime = mem.LevelTime();
			string sceneName = mem.SceneName();
			bool loading = mem.Loading();
			bool ending = mem.LevelEnding() && mem.LevelWon();

			if (currentSplit < Model.CurrentState.Run.Count && settings.Splits.Count > 0) {
				SplitInfo split = currentSplit + 1 < settings.Splits.Count ? settings.Splits[currentSplit + 1] : SplitInfo.EndGame;
				switch (split.Split) {
					case SplitName.StartGame: shouldSplit = inGame && loading && sceneName == "scene_cutscene_intro"; break;

					case SplitName.Shop: shouldSplit = sceneName == "scene_shop"; break;
					case SplitName.map_world_1: shouldSplit = sceneName == "scene_map_world_1"; break;
					case SplitName.map_world_2: shouldSplit = sceneName == "scene_map_world_2"; break;
					case SplitName.map_world_3: shouldSplit = sceneName == "scene_map_world_3"; break;
					case SplitName.map_world_4: shouldSplit = sceneName == "scene_map_world_4"; break;

					case SplitName.level_tutorial: shouldSplit = lastSceneName == "scene_level_tutorial" && sceneName != "scene_level_tutorial"; break;

					case SplitName.level_veggies: shouldSplit = InScene("scene_level_veggies") && mem.LevelComplete(Levels.Veggies, split.Difficulty, split.Grade); break;
					case SplitName.level_slime: shouldSplit = InScene("scene_level_slime") && mem.LevelComplete(Levels.Slime, split.Difficulty, split.Grade); break;
					case SplitName.level_flower: shouldSplit = InScene("scene_level_flower") && mem.LevelComplete(Levels.Flower, split.Difficulty, split.Grade); break;
					case SplitName.level_frogs: shouldSplit = InScene("scene_level_frogs") && mem.LevelComplete(Levels.Frogs, split.Difficulty, split.Grade); break;
					case SplitName.level_flying_blimp: shouldSplit = InScene("scene_level_flying_blimp") && mem.LevelComplete(Levels.FlyingBlimp, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_1_1F: shouldSplit = InScene("scene_level_platforming_1_1F") && mem.LevelComplete(Levels.Platforming_Level_1_1, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_1_2F: shouldSplit = InScene("scene_level_platforming_1_2F") && mem.LevelComplete(Levels.Platforming_Level_1_2, split.Difficulty, split.Grade); break;
					case SplitName.level_mausoleum_1: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Easy && ending; break;

					case SplitName.level_baroness: shouldSplit = InScene("scene_level_baroness") && mem.LevelComplete(Levels.Baroness, split.Difficulty, split.Grade); break;
					case SplitName.level_clown: shouldSplit = InScene("scene_level_clown") && mem.LevelComplete(Levels.Clown, split.Difficulty, split.Grade); break;
					case SplitName.level_dragon: shouldSplit = InScene("scene_level_dragon") && mem.LevelComplete(Levels.Dragon, split.Difficulty, split.Grade); break;
					case SplitName.level_flying_genie: shouldSplit = InScene("scene_level_flying_genie") && mem.LevelComplete(Levels.FlyingGenie, split.Difficulty, split.Grade); break;
					case SplitName.level_flying_bird: shouldSplit = InScene("scene_level_flying_bird") && mem.LevelComplete(Levels.FlyingBird, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_2_1F: shouldSplit = InScene("scene_level_platforming_2_1F") && mem.LevelComplete(Levels.Platforming_Level_2_1, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_2_2F: shouldSplit = InScene("scene_level_platforming_2_2F") && mem.LevelComplete(Levels.Platforming_Level_2_2, split.Difficulty, split.Grade); break;
					case SplitName.level_mausoleum_2: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Normal && ending; break;

					case SplitName.level_bee: shouldSplit = InScene("scene_level_bee") && mem.LevelComplete(Levels.Bee, split.Difficulty, split.Grade); break;
					case SplitName.level_pirate: shouldSplit = InScene("scene_level_pirate") && mem.LevelComplete(Levels.Pirate, split.Difficulty, split.Grade); break;
					case SplitName.level_sally_stage_play: shouldSplit = InScene("scene_level_sally_stage_play") && mem.LevelComplete(Levels.SallyStagePlay, split.Difficulty, split.Grade); break;
					case SplitName.level_mouse: shouldSplit = InScene("scene_level_mouse") && mem.LevelComplete(Levels.Mouse, split.Difficulty, split.Grade); break;
					case SplitName.level_robot: shouldSplit = InScene("scene_level_robot") && mem.LevelComplete(Levels.Robot, split.Difficulty, split.Grade); break;
					case SplitName.level_train: shouldSplit = InScene("scene_level_train") && mem.LevelComplete(Levels.Train, split.Difficulty, split.Grade); break;
					case SplitName.level_flying_mermaid: shouldSplit = InScene("scene_level_flying_mermaid") && mem.LevelComplete(Levels.FlyingMermaid, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_3_1F: shouldSplit = InScene("scene_level_platforming_3_1F") && mem.LevelComplete(Levels.Platforming_Level_3_1, split.Difficulty, split.Grade); break;
					case SplitName.level_platforming_3_2F: shouldSplit = InScene("scene_level_platforming_3_2F") && mem.LevelComplete(Levels.Platforming_Level_3_2, split.Difficulty, split.Grade); break;
					case SplitName.level_mausoleum_3: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Hard && ending; break;

					case SplitName.level_dice_palace_enter: shouldSplit = sceneName == "scene_cutscene_kingdice"; break;
					case SplitName.level_dice_palace_main: shouldSplit = InScene("scene_level_dice_palace_main") && mem.LevelComplete(Levels.DicePalaceMain, split.Difficulty, split.Grade); break;
					case SplitName.level_devil: shouldSplit = InScene("scene_level_devil") && mem.LevelComplete(Levels.Devil, split.Difficulty, split.Grade); break;

					case SplitName.EndGame: shouldSplit = sceneName == "scene_cutscene_credits"; break;

					case SplitName.EnterLevel: shouldSplit = levelTime > 0 && levelTime < 0.5; break;
					case SplitName.EndLevel: shouldSplit = levelTime > 0 && ending; break;
				}
			}

			Model.CurrentState.IsGameTimePaused = loading;

			lastInGame = inGame;
			lastLevelTime = levelTime;
			if (lastSceneName != sceneName) {
				lastSceneSeen = lastSceneName;
			}
			lastSceneName = sceneName;
			lastLoading = loading;

			HandleSplit(shouldSplit, Model.CurrentState.Run.Count == 1 && loading && levelTime == 0);
		}
		public bool InScene(string scene) {
			return lastSceneName == scene || lastSceneSeen == scene;
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit < 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
		private void HandleGameTimes() {
			if (currentSplit > 0 && currentSplit <= Model.CurrentState.Run.Count && Model.CurrentState.Run.Count == 1) {
				TimeSpan gameTime = TimeSpan.FromSeconds(mem.LevelTime());
				if (currentSplit == Model.CurrentState.Run.Count) {
					Time t = Model.CurrentState.Run[currentSplit - 1].SplitTime;
					Model.CurrentState.Run[currentSplit - 1].SplitTime = new Time(t.RealTime, gameTime);
				} else {
					Model.CurrentState.SetGameTime(gameTime);
				}
			}
		}
#endif
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = "", curr = "";
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						case "InGame": curr = mem.InGame().ToString(); break;
						case "Scene": curr = mem.SceneName(); break;
						case "LevelEnding": curr = mem.LevelEnding().ToString(); break;
						case "LevelWon": curr = mem.LevelWon().ToString(); break;
						default: curr = ""; break;
					}

					if (!prev.Equals(curr)) {
						WriteLogWithTime(key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (!Console.IsOutputRedirected) {
					Console.WriteLine(data);
				}
				if (hasLog) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				}
			}
		}
		private void WriteLogWithTime(string data) {
#if !Info
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null && Model.CurrentState.CurrentTime.RealTime.HasValue ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + data);
#else
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + ": " + data);
#endif
		}

#if !Info
		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(0));
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			state = 0;
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			HandleGameTimes();
			state = 0;
		}
		public Control GetSettingsControl(LayoutMode mode) { return settings; }
		public void SetSettings(XmlNode document) { settings.SetSettings(document); }
		public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
#endif
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() {
#if !Info
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
#endif
		}
	}
}