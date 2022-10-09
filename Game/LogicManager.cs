using System;
namespace LiveSplit.Cuphead {
    public class LogicManager {
        public bool ShouldSplit { get; private set; }
        public bool ShouldReset { get; private set; }
        public int CurrentSplit { get; private set; }
        public bool Running { get; private set; }
        public bool Paused { get; private set; }
        public float GameTime { get; private set; }
        public MemoryManager Memory { get; private set; }
        public SplitterSettings Settings { get; private set; }

        private string lastSceneName, lastSceneSeen;
        private DateTime splitLate;

        public LogicManager(SplitterSettings settings) {
            Memory = new MemoryManager();
            Settings = settings;
            splitLate = DateTime.MaxValue;
        }

        public void Reset() {
            splitLate = DateTime.MaxValue;
            Paused = false;
            Running = false;
            CurrentSplit = 0;
            InitializeSplit();
            ShouldSplit = false;
            ShouldReset = false;
        }
        public void Decrement() {
            CurrentSplit--;
            splitLate = DateTime.MaxValue;
            InitializeSplit();
        }
        public void Increment() {
            Running = true;
            splitLate = DateTime.MaxValue;
            CurrentSplit++;
            InitializeSplit();
        }
        private void InitializeSplit() {
            if (CurrentSplit < Settings.Splits.Count) {
                bool temp = ShouldSplit;
                CheckSplit(Settings.Splits[CurrentSplit], true);
                ShouldSplit = temp;
            }
        }
        public bool IsHooked() {
            bool hooked = Memory.HookProcess();
            Paused = !hooked;
            ShouldSplit = false;
            ShouldReset = false;
            GameTime = -1;
            return hooked;
        }
        public void Update(int currentSplit) {
            if (currentSplit != CurrentSplit) {
                CurrentSplit = currentSplit;
                Running = CurrentSplit > 0;
                InitializeSplit();
            }

            if (CurrentSplit < Settings.Splits.Count) {
                CheckSplit(Settings.Splits[CurrentSplit], !Running);
                if (!Running) {
                    Paused = true;
                    if (ShouldSplit) {
                        Running = true;
                    }
                }

                if (ShouldSplit) {
                    Increment();
                }
            }
        }
        private void CheckSplit(SplitInfo split, bool updateValues) {
            ShouldSplit = false;
            Paused = Memory.Loading();

            if (!updateValues && Paused) {
                return;
            }

            bool inGame = Memory.InGame();
            float levelTime = Memory.LevelTime();
            string sceneName = Memory.SceneName();
            bool ending = Memory.LevelEnding() && Memory.LevelWon();

            switch (split.Split) {
                case SplitName.StartGame: ShouldSplit = inGame && Paused && sceneName == "scene_cutscene_intro"; break;

                case SplitName.Shop: ShouldSplit = sceneName == "scene_shop"; break;
                case SplitName.map_world_1: ShouldSplit = sceneName == "scene_map_world_1"; break;
                case SplitName.map_world_2: ShouldSplit = sceneName == "scene_map_world_2"; break;
                case SplitName.map_world_3: ShouldSplit = sceneName == "scene_map_world_3"; break;
                case SplitName.map_world_4: ShouldSplit = sceneName == "scene_map_world_4"; break;
                case SplitName.map_world_DLC: ShouldSplit = sceneName == "scene_map_world_DLC"; break;


                case SplitName.level_tutorial: ShouldSplit = lastSceneName == "scene_level_tutorial" && sceneName != "scene_level_tutorial"; break;
                case SplitName.level_chalice_tutorial: ShouldSplit = lastSceneName == "scene_level_chalice_tutorial" && sceneName != "scene_level_chalice_tutorial"; break;

                case SplitName.level_veggies: ShouldSplit = InScene("scene_level_veggies") && Memory.LevelComplete(Levels.Veggies, split.Difficulty, split.Grade); break;
                case SplitName.level_slime: ShouldSplit = InScene("scene_level_slime") && Memory.LevelComplete(Levels.Slime, split.Difficulty, split.Grade); break;
                case SplitName.level_flower: ShouldSplit = InScene("scene_level_flower") && Memory.LevelComplete(Levels.Flower, split.Difficulty, split.Grade); break;
                case SplitName.level_frogs: ShouldSplit = InScene("scene_level_frogs") && Memory.LevelComplete(Levels.Frogs, split.Difficulty, split.Grade); break;
                case SplitName.level_flying_blimp: ShouldSplit = InScene("scene_level_flying_blimp") && Memory.LevelComplete(Levels.FlyingBlimp, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_1_1F: ShouldSplit = InScene("scene_level_platforming_1_1F") && Memory.LevelComplete(Levels.Platforming_Level_1_1, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_1_2F: ShouldSplit = InScene("scene_level_platforming_1_2F") && Memory.LevelComplete(Levels.Platforming_Level_1_2, split.Difficulty, split.Grade); break;
                case SplitName.level_mausoleum_1: ShouldSplit = sceneName == "scene_level_mausoleum" && Memory.LevelMode() == Mode.Easy && ending; break;

                case SplitName.level_baroness: ShouldSplit = InScene("scene_level_baroness") && Memory.LevelComplete(Levels.Baroness, split.Difficulty, split.Grade); break;
                case SplitName.level_clown: ShouldSplit = InScene("scene_level_clown") && Memory.LevelComplete(Levels.Clown, split.Difficulty, split.Grade); break;
                case SplitName.level_dragon: ShouldSplit = InScene("scene_level_dragon") && Memory.LevelComplete(Levels.Dragon, split.Difficulty, split.Grade); break;
                case SplitName.level_flying_genie: ShouldSplit = InScene("scene_level_flying_genie") && Memory.LevelComplete(Levels.FlyingGenie, split.Difficulty, split.Grade); break;
                case SplitName.level_flying_bird: ShouldSplit = InScene("scene_level_flying_bird") && Memory.LevelComplete(Levels.FlyingBird, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_2_1F: ShouldSplit = InScene("scene_level_platforming_2_1F") && Memory.LevelComplete(Levels.Platforming_Level_2_1, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_2_2F: ShouldSplit = InScene("scene_level_platforming_2_2F") && Memory.LevelComplete(Levels.Platforming_Level_2_2, split.Difficulty, split.Grade); break;
                case SplitName.level_mausoleum_2: ShouldSplit = sceneName == "scene_level_mausoleum" && Memory.LevelMode() == Mode.Normal && ending; break;

                case SplitName.level_bee: ShouldSplit = InScene("scene_level_bee") && Memory.LevelComplete(Levels.Bee, split.Difficulty, split.Grade); break;
                case SplitName.level_pirate: ShouldSplit = InScene("scene_level_pirate") && Memory.LevelComplete(Levels.Pirate, split.Difficulty, split.Grade); break;
                case SplitName.level_sally_stage_play: ShouldSplit = InScene("scene_level_sally_stage_play") && Memory.LevelComplete(Levels.SallyStagePlay, split.Difficulty, split.Grade); break;
                case SplitName.level_mouse: ShouldSplit = InScene("scene_level_mouse") && Memory.LevelComplete(Levels.Mouse, split.Difficulty, split.Grade); break;
                case SplitName.level_robot: ShouldSplit = InScene("scene_level_robot") && Memory.LevelComplete(Levels.Robot, split.Difficulty, split.Grade); break;
                case SplitName.level_train: ShouldSplit = InScene("scene_level_train") && Memory.LevelComplete(Levels.Train, split.Difficulty, split.Grade); break;
                case SplitName.level_flying_mermaid: ShouldSplit = InScene("scene_level_flying_mermaid") && Memory.LevelComplete(Levels.FlyingMermaid, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_3_1F: ShouldSplit = InScene("scene_level_platforming_3_1F") && Memory.LevelComplete(Levels.Platforming_Level_3_1, split.Difficulty, split.Grade); break;
                case SplitName.level_platforming_3_2F: ShouldSplit = InScene("scene_level_platforming_3_2F") && Memory.LevelComplete(Levels.Platforming_Level_3_2, split.Difficulty, split.Grade); break;
                case SplitName.level_mausoleum_3: ShouldSplit = sceneName == "scene_level_mausoleum" && Memory.LevelMode() == Mode.Hard && ending; break;

                case SplitName.level_old_man: ShouldSplit = InScene("scene_level_old_man") && Memory.LevelComplete(Levels.OldMan, split.Difficulty, split.Grade); break;
                case SplitName.level_snow_cult: ShouldSplit = InScene("scene_level_snow_cult") && Memory.LevelComplete(Levels.SnowCult, split.Difficulty, split.Grade); break;
                case SplitName.level_airplane: ShouldSplit = InScene("scene_level_airplane") && Memory.LevelComplete(Levels.Airplane, split.Difficulty, split.Grade); break;
                case SplitName.level_flying_cowboy: ShouldSplit = InScene("scene_level_flying_cowboy") && Memory.LevelComplete(Levels.FlyingCowboy, split.Difficulty, split.Grade); break;
                case SplitName.level_rum_runners: ShouldSplit = InScene("scene_level_rum_runners") && Memory.LevelComplete(Levels.RumRunners, split.Difficulty, split.Grade); break;
                case SplitName.level_saltbaker: ShouldSplit = InScene("scene_level_saltbaker") && Memory.LevelComplete(Levels.Saltbaker, split.Difficulty, split.Grade); break;
                case SplitName.level_graveyard: ShouldSplit = InScene("scene_level_graveyard") && Memory.LevelComplete(Levels.Graveyard, split.Difficulty, split.Grade); break;
                case SplitName.level_chess_pawn: ShouldSplit = InScene("scene_level_chess_pawn") && Memory.LevelComplete(Levels.ChessPawn, split.Difficulty, split.Grade); break;
                case SplitName.level_chess_knight: ShouldSplit = InScene("scene_level_chess_knight") && Memory.LevelComplete(Levels.ChessKnight, split.Difficulty, split.Grade); break;
                case SplitName.level_chess_bishop: ShouldSplit = InScene("scene_level_chessbishop") && Memory.LevelComplete(Levels.ChessBishop, split.Difficulty, split.Grade); break;
                case SplitName.level_chess_rook: ShouldSplit = InScene("scene_level_chess_rook") && Memory.LevelComplete(Levels.ChessRook, split.Difficulty, split.Grade); break;
                case SplitName.level_chess_queen: ShouldSplit = InScene("scene_level_chess_queen") && Memory.LevelComplete(Levels.ChessQueen, split.Difficulty, split.Grade); break;

                case SplitName.level_dice_palace_enter: ShouldSplit = sceneName == "scene_cutscene_kingdice"; break;
                case SplitName.level_dice_palace_main: ShouldSplit = InScene("scene_level_dice_palace_main") && Memory.LevelComplete(Levels.DicePalaceMain, split.Difficulty, split.Grade); break;
                case SplitName.level_devil: ShouldSplit = InScene("scene_level_devil") && Memory.LevelComplete(Levels.Devil, split.Difficulty, split.Grade); break;

                case SplitName.EndGame: ShouldSplit = sceneName == "scene_cutscene_credits"; break;

                case SplitName.EnterLevel: ShouldSplit = levelTime > 0 && levelTime < 0.5; break;
                case SplitName.EndLevel: ShouldSplit = levelTime > 0 && ending; break;
            }

            if (lastSceneName != sceneName) {
                lastSceneSeen = lastSceneName;
            }
            lastSceneName = sceneName;

            ShouldReset = Settings.Splits.Count == 1 && Paused && levelTime == 0;

            if (Running && Paused) {
                ShouldSplit = false;
            } else if (DateTime.Now > splitLate) {
                ShouldSplit = true;
                splitLate = DateTime.MaxValue;
            }
        }
        private bool InScene(string scene) {
            return lastSceneName == scene || lastSceneSeen == scene;
        }
    }
}