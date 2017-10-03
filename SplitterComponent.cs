#if !Info
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private static string LOGFILE = "_Cuphead.log";
		internal static string[] keys = { "CurrentSplit", "State", "InGame", "Scene", "LevelEnding", "LevelWon" };
		private SplitterMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0;
		private bool hasLog = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplitterSettings settings;
#if !Info
		private bool lastInGame, lastLoading;
		private string lastSceneName;
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
				SplitName split = currentSplit + 1 < settings.Splits.Count ? settings.Splits[currentSplit + 1] : SplitName.EndGame;
				switch (split) {
					case SplitName.StartGame: shouldSplit = inGame && loading && sceneName == "scene_cutscene_intro"; break;
					case SplitName.EndGame: shouldSplit = sceneName == "scene_cutscene_credits"; break;
					case SplitName.EnterLevel:
						if (state == 0 && lastLevelTime == 0 && levelTime == 0) {
							state++;
						} else if (state == 1) {
							shouldSplit = !loading && levelTime > 0;
						}
						break;
					case SplitName.EndLevel: shouldSplit = levelTime > 0 && ending; break;
					case SplitName.Shop: shouldSplit = sceneName == "scene_shop"; break;
					case SplitName.map_world_1: shouldSplit = sceneName == "scene_map_world_1"; break;
					case SplitName.map_world_2: shouldSplit = sceneName == "scene_map_world_2"; break;
					case SplitName.map_world_3: shouldSplit = sceneName == "scene_map_world_3"; break;
					case SplitName.map_world_4: shouldSplit = sceneName == "scene_map_world_4"; break;
					
					case SplitName.level_tutorial: shouldSplit = lastSceneName == "scene_level_tutorial" && sceneName != "scene_level_tutorial"; break;

					case SplitName.level_veggies: shouldSplit = mem.LevelComplete(Levels.Veggies); break;
					case SplitName.level_slime: shouldSplit = mem.LevelComplete(Levels.Slime); break;
					case SplitName.level_flower: shouldSplit = mem.LevelComplete(Levels.Flower); break;
					case SplitName.level_frogs: shouldSplit = mem.LevelComplete(Levels.Frogs); break;
					case SplitName.level_flying_blimp: shouldSplit = mem.LevelComplete(Levels.FlyingBlimp); break;
					case SplitName.level_platforming_1_1F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_1_1); break;
					case SplitName.level_platforming_1_2F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_1_2); break;
					case SplitName.level_mausoleum_1: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Easy && ending; break;

					case SplitName.level_baroness: shouldSplit = mem.LevelComplete(Levels.Baroness); break;
					case SplitName.level_clown: shouldSplit = mem.LevelComplete(Levels.Clown); break;
					case SplitName.level_dragon: shouldSplit = mem.LevelComplete(Levels.Dragon); break;
					case SplitName.level_flying_genie: shouldSplit = mem.LevelComplete(Levels.FlyingGenie); break;
					case SplitName.level_flying_bird: shouldSplit = mem.LevelComplete(Levels.FlyingBird); break;
					case SplitName.level_platforming_2_1F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_2_1); break;
					case SplitName.level_platforming_2_2F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_2_2); break;
					case SplitName.level_mausoleum_2: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Normal && ending; break;

					case SplitName.level_bee: shouldSplit = mem.LevelComplete(Levels.Bee); break;
					case SplitName.level_pirate: shouldSplit = mem.LevelComplete(Levels.Pirate); break;
					case SplitName.level_sally_stage_play: shouldSplit = mem.LevelComplete(Levels.SallyStagePlay); break;
					case SplitName.level_mouse: shouldSplit = mem.LevelComplete(Levels.Mouse); break;
					case SplitName.level_robot: shouldSplit = mem.LevelComplete(Levels.Robot); break;
					case SplitName.level_train: shouldSplit = mem.LevelComplete(Levels.Train); break;
					case SplitName.level_flying_mermaid: shouldSplit = mem.LevelComplete(Levels.FlyingMermaid); break;
					case SplitName.level_platforming_3_1F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_3_1); break;
					case SplitName.level_platforming_3_2F: shouldSplit = mem.LevelComplete(Levels.Platforming_Level_3_2); break;
					case SplitName.level_mausoleum_3: shouldSplit = sceneName == "scene_level_mausoleum" && mem.LevelMode() == Mode.Hard && ending; break;

					case SplitName.level_dice_palace_main: shouldSplit = mem.LevelComplete(Levels.DicePalaceMain); break;
					case SplitName.level_devil: shouldSplit = mem.LevelComplete(Levels.Devil); break;
				}
			}

			Model.CurrentState.IsGameTimePaused = loading;

			lastInGame = inGame;
			lastLevelTime = levelTime;
			lastSceneName = sceneName;
			lastLoading = loading;

			HandleSplit(shouldSplit, Model.CurrentState.Run.Count == 1 && loading && levelTime == 0);
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
			//Remove duplicate autosplitter componenets
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			bool hasAutosplitter = false;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplitterComponent) {
					if ((invalidator == null && width == 0 && height == 0) || hasAutosplitter) {
						components.Remove(component);
					}
					hasAutosplitter = true;
				}
			}

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
		public void Dispose() { }
	}
	public enum SplitName {
		[Description("Manual Split (Not Automatic)"), ToolTip("Specify to split manually when an automatic split does not exist yet")]
		ManualSplit,

		[Description("Start Game (Select Save)"), ToolTip("Splits when you select a new save slot")]
		StartGame,
		[Description("End Game (Credits)"), ToolTip("Splits when entering the credits")]
		EndGame,
		[Description("Enter Level (IL)"), ToolTip("Splits when entering any level")]
		EnterLevel,
		[Description("End Level (IL)"), ToolTip("Splits when ending any level")]
		EndLevel,

		[Description("Shop (Enter Scene)"), ToolTip("Splits when current scene is 'Shop'")]
		Shop,
		[Description("World 1 (Enter Scene)"), ToolTip("Splits when current scene is 'Map World 1'")]
		map_world_1,
		[Description("World 2 (Enter Scene)"), ToolTip("Splits when current scene is 'Map World 2'")]
		map_world_2,
		[Description("World 3 (Enter Scene)"), ToolTip("Splits when current scene is 'Map World 3'")]
		map_world_3,
		[Description("World 4 (Enter Scene)"), ToolTip("Splits when current scene is 'Map World 4'")]
		map_world_4,
		
		[Description("Tutorial (Level)"), ToolTip("Splits when leaving scene 'Tutorial'")]
		level_tutorial,

		[Description("The Root Pack (Boss)"), ToolTip("Splits when level is finished")]
		level_veggies,
		[Description("Goopy Le Grande (Boss)"), ToolTip("Splits when level is finished")]
		level_slime,
		[Description("Cagney Carnation (Boss)"), ToolTip("Splits when level is finished")]
		level_flower,
		[Description("Ribby And Croaks (Boss)"), ToolTip("Splits when level is finished")]
		level_frogs,
		[Description("Hilda Berg (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_blimp,

		[Description("Baroness Von Bon Bon (Boss)"), ToolTip("Splits when level is finished")]
		level_baroness,
		[Description("Djimmi The Great (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_genie,
		[Description("Beppi The Clown (Boss)"), ToolTip("Splits when level is finished")]
		level_clown,
		[Description("Wally Warbles (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_bird,
		[Description("Grim Matchstick (Boss)"), ToolTip("Splits when level is finished")]
		level_dragon,

		[Description("Rumor Honeybottoms (Boss)"), ToolTip("Splits when level is finished")]
		level_bee,
		[Description("Captin Brineybeard (Boss)"), ToolTip("Splits when level is finished")]
		level_pirate,
		[Description("Werner Werman (Boss)"), ToolTip("Splits when level is finished")]
		level_mouse,
		[Description("Dr. Kahl's Robot (Boss)"), ToolTip("Splits when level is finished")]
		level_robot,
		[Description("Sally Stageplay (Boss)"), ToolTip("Splits when level is finished")]
		level_sally_stage_play,
		[Description("Cala Maria (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_mermaid,
		[Description("Phantom Express (Boss)"), ToolTip("Splits when level is finished")]
		level_train,

		[Description("King Dice (Boss)"), ToolTip("Splits when all mini boss dice levels are complete")]
		level_dice_palace_main,
		[Description("Devil (Boss)"), ToolTip("Splits when level is finished")]
		level_devil,

		[Description("Forest Follies (Run 'n Gun)"), ToolTip("Splits when level is finished'")]
		level_platforming_1_1F,
		[Description("Treetop Trouble (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_1_2F,
		[Description("Funfair Fever (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_2_1F,
		[Description("Funhouse Frazzle (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_2_2F,
		[Description("Perilous Piers (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_3_1F,
		[Description("Rugged Ridge (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_3_2F,

		[Description("Mausoleum I (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_1,
		[Description("Mausoleum II (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_2,
		[Description("Mausoleum III (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_3,
	}
}