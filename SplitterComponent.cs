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

			if (currentSplit < Model.CurrentState.Run.Count) {
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
					case SplitName.level_mausoleum: shouldSplit = sceneName == "scene_level_mausoleum"; break;
					case SplitName.level_dice_gate: shouldSplit = sceneName == "scene_level_dice_gate"; break;
					case SplitName.level_dice_palace_main: shouldSplit = sceneName == "scene_level_dice_palace_main"; break;

					case SplitName.level_tutorial: shouldSplit = lastSceneName == "scene_level_tutorial" && sceneName != "scene_level_tutorial"; break;

					case SplitName.level_veggies: shouldSplit = sceneName == "scene_level_veggies" && ending; break;
					case SplitName.level_slime: shouldSplit = sceneName == "scene_level_slime" && ending; break;
					case SplitName.level_flower: shouldSplit = sceneName == "scene_level_flower" && ending; break;
					case SplitName.level_frogs: shouldSplit = sceneName == "scene_level_frogs" && ending; break;
					case SplitName.level_flying_blimp: shouldSplit = sceneName == "scene_level_flying_blimp" && ending; break;

					case SplitName.level_baroness: shouldSplit = sceneName == "scene_level_baroness" && ending; break;
					case SplitName.level_clown: shouldSplit = sceneName == "scene_level_clown" && ending; break;
					case SplitName.level_dragon: shouldSplit = sceneName == "scene_level_dragon" && ending; break;
					case SplitName.level_flying_genie: shouldSplit = sceneName == "scene_level_flying_genie" && ending; break;
					case SplitName.level_flying_bird: shouldSplit = sceneName == "scene_level_flying_bird" && ending; break;

					case SplitName.level_bee: shouldSplit = sceneName == "scene_level_bee" && ending; break;
					case SplitName.level_pirate: shouldSplit = sceneName == "scene_level_pirate" && ending; break;
					case SplitName.level_sally_stage_play: shouldSplit = sceneName == "scene_level_sally_stage_play" && ending; break;
					case SplitName.level_mouse: shouldSplit = sceneName == "scene_level_mouse" && ending; break;
					case SplitName.level_robot: shouldSplit = sceneName == "scene_level_robot" && ending; break;
					case SplitName.level_train: shouldSplit = sceneName == "scene_level_train" && ending; break;
					case SplitName.level_flying_mermaid: shouldSplit = sceneName == "scene_level_flying_mermaid" && ending; break;

					case SplitName.level_platforming_1_1F: shouldSplit = sceneName == "scene_level_platforming_1_1F" && ending; break;
					case SplitName.level_platforming_1_2F: shouldSplit = sceneName == "scene_level_platforming_1_2F" && ending; break;
					case SplitName.level_platforming_2_1F: shouldSplit = sceneName == "scene_level_platforming_2_1F" && ending; break;
					case SplitName.level_platforming_2_2F: shouldSplit = sceneName == "scene_level_platforming_2_2F" && ending; break;
					case SplitName.level_platforming_3_1F: shouldSplit = sceneName == "scene_level_platforming_3_1F" && ending; break;
					case SplitName.level_platforming_3_2F: shouldSplit = sceneName == "scene_level_platforming_3_2F" && ending; break;

					case SplitName.level_bat: shouldSplit = sceneName == "scene_level_bat" && ending; break;
					case SplitName.level_devil: shouldSplit = sceneName == "scene_level_devil" && ending; break;

					case SplitName.level_airship_jelly: shouldSplit = sceneName == "scene_level_airship_jelly" && ending; break;
					case SplitName.level_airship_stork: shouldSplit = sceneName == "scene_level_airship_stork" && ending; break;
					case SplitName.level_airship_crab: shouldSplit = sceneName == "scene_level_airship_crab" && ending; break;
					case SplitName.level_airship_clam: shouldSplit = sceneName == "scene_level_airship_clam" && ending; break;

					case SplitName.level_dice_palace_domino: shouldSplit = sceneName == "scene_level_dice_palace_domino" && ending; break;
					case SplitName.level_dice_palace_card: shouldSplit = sceneName == "scene_level_dice_palace_card" && ending; break;
					case SplitName.level_dice_palace_chips: shouldSplit = sceneName == "scene_level_dice_palace_chips" && ending; break;
					case SplitName.level_dice_palace_cigar: shouldSplit = sceneName == "scene_level_dice_palace_cigar" && ending; break;
					case SplitName.level_dice_palace_booze: shouldSplit = sceneName == "scene_level_dice_palace_booze" && ending; break;
					case SplitName.level_dice_palace_roulette: shouldSplit = sceneName == "scene_level_dice_palace_roulette" && ending; break;
					case SplitName.level_dice_palace_pachinko: shouldSplit = sceneName == "scene_level_dice_palace_pachinko" && ending; break;
					case SplitName.level_dice_palace_rabbit: shouldSplit = sceneName == "scene_level_dice_palace_rabbit" && ending; break;
					case SplitName.level_dice_palace_light: shouldSplit = sceneName == "scene_level_dice_palace_light" && ending; break;
					case SplitName.level_dice_palace_eight_ball: shouldSplit = sceneName == "scene_level_dice_palace_eight_ball" && ending; break;
					case SplitName.level_dice_palace_flying_horse: shouldSplit = sceneName == "scene_level_dice_palace_flying_horse" && ending; break;
					case SplitName.level_dice_palace_flying_memory: shouldSplit = sceneName == "scene_level_dice_palace_flying_memory" && ending; break;
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

		[Description("Select Save Slot (Start Game)"), ToolTip("Splits when you select a new save slot")]
		StartGame,
		[Description("Credits (End Game)"), ToolTip("Splits when entering the credits")]
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
		[Description("Mausoleum (Enter Scene)"), ToolTip("Splits when current scene is 'Masoleum'")]
		level_mausoleum,
		[Description("Dice Gate (Enter Scene)"), ToolTip("Splits when current scene is 'Dice Gate'")]
		level_dice_gate,
		[Description("Dice Main (Enter Scene)"), ToolTip("Splits when current scene is 'Dice Main'")]
		level_dice_palace_main,

		[Description("Tutorial (Finished)"), ToolTip("Splits when leaving scene 'Tutorial'")]
		level_tutorial,

		[Description("The Root Pack (Finished)"), ToolTip("Splits when level is finished")]
		level_veggies,
		[Description("Goopy Le Grande (Finished)"), ToolTip("Splits when level is finished")]
		level_slime,
		[Description("Flower (Finished)"), ToolTip("Splits when level is finished")]
		level_flower,
		[Description("Frogs (Finished)"), ToolTip("Splits when level is finished")]
		level_frogs,
		[Description("Blimp - Flying (Finished)"), ToolTip("Splits when level is finished")]
		level_flying_blimp,

		[Description("Baroness (Finished)"), ToolTip("Splits when level is finished")]
		level_baroness,
		[Description("Clown (Finished)"), ToolTip("Splits when level is finished")]
		level_clown,
		[Description("Dragon (Finished)"), ToolTip("Splits when level is finished")]
		level_dragon,
		[Description("Genie - Flying (Finished)"), ToolTip("Splits when level is finished")]
		level_flying_genie,
		[Description("Bird - Flying (Finished)"), ToolTip("Splits when level is finished")]
		level_flying_bird,

		[Description("Bee (Finished)"), ToolTip("Splits when level is finished")]
		level_bee,
		[Description("Pirate (Finished)"), ToolTip("Splits when level is finished")]
		level_pirate,
		[Description("Sally Stage Play (Finished)"), ToolTip("Splits when level is finished")]
		level_sally_stage_play,
		[Description("Mouse (Finished)"), ToolTip("Splits when level is finished")]
		level_mouse,
		[Description("Robot (Finished)"), ToolTip("Splits when level is finished")]
		level_robot,
		[Description("Train (Finished)"), ToolTip("Splits when level is finished")]
		level_train,
		[Description("Mermaid - Flying (Finished)"), ToolTip("Splits when level is finished")]
		level_flying_mermaid,

		[Description("Run 'n Gun 1-1 (Finished)"), ToolTip("Splits when level is finished'")]
		level_platforming_1_1F,
		[Description("Platforming 1-2 (Finished)"), ToolTip("Splits when level is finished")]
		level_platforming_1_2F,
		[Description("Platforming 2-1 (Finished)"), ToolTip("Splits when level is finished")]
		level_platforming_2_1F,
		[Description("Platforming 2-2 (Finished)"), ToolTip("Splits when level is finished")]
		level_platforming_2_2F,
		[Description("Platforming 3-1 (Finished)"), ToolTip("Splits when level is finished")]
		level_platforming_3_1F,
		[Description("Platforming 3-2 (Finished)"), ToolTip("Splits when level is finished")]
		level_platforming_3_2F,

		[Description("Bat (Finished)"), ToolTip("Splits when level is finished")]
		level_bat,
		[Description("Devil (Finished)"), ToolTip("Splits when level is finished")]
		level_devil,

		[Description("Jelly - Airship (Finished)"), ToolTip("Splits when level is finished")]
		level_airship_jelly,
		[Description("Stork - Airship (Finished)"), ToolTip("Splits when level is finished")]
		level_airship_stork,
		[Description("Crab - Airship (Finished)"), ToolTip("Splits when level is finished")]
		level_airship_crab,
		[Description("Clam - Airship (Finished)"), ToolTip("Splits when level is finished")]
		level_airship_clam,

		[Description("Domino - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_domino,
		[Description("Card - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_card,
		[Description("Chips - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_chips,
		[Description("Cigar - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_cigar,
		[Description("Booze - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_booze,
		[Description("Roulette - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_roulette,
		[Description("Pachinko - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_pachinko,
		[Description("Rabbit - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_rabbit,
		[Description("Light - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_light,
		[Description("Eight Ball - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_eight_ball,
		[Description("Flying Horse - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_flying_horse,
		[Description("Flying Memory - Dice (Finished)"), ToolTip("Splits when level is finished")]
		level_dice_palace_flying_memory,
	}
}