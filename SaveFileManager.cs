namespace GomokuReversiApp
{
    // Writes and reads the save file 
    public static class SaveFileManager
    {
        // Save game
        public static void Save(string filePath, SavedGameData data)
        {
            using StreamWriter writer = new StreamWriter(filePath);

            writer.WriteLine("[CONFIGURATION]");
            writer.WriteLine($"GameType={data.Configuration.GameType}");
            writer.WriteLine($"Variant={data.Configuration.Variant}");
            writer.WriteLine($"GameMode={data.Configuration.GameMode}");
            if (data.Configuration.AIDifficulty != null)
                writer.WriteLine($"AIDifficulty={data.Configuration.AIDifficulty}");

            writer.WriteLine("[CURRENT_STATE]");
            WriteState(writer, data.CurrentState);

            foreach (GameState state in data.UndoHistory)
            {
                writer.WriteLine("[UNDO_STATE]");
                WriteState(writer, state);
            }

            foreach (GameState state in data.RedoHistory)
            {
                writer.WriteLine("[REDO_STATE]");
                WriteState(writer, state);
            }
        }

        private static void WriteState(StreamWriter writer, GameState state)
        {
            writer.WriteLine($"CurrentPlayerIndex={state.CurrentPlayerIndex}");
            writer.WriteLine($"TurnCount={state.TurnCount}");
            writer.WriteLine($"GameResult={state.Result}");
            writer.WriteLine($"Board={string.Join("/", state.BoardRows)}");
            writer.WriteLine($"Player1Inventory={FormatInventory(state.PlayerInventories[0])}");
            writer.WriteLine($"Player2Inventory={FormatInventory(state.PlayerInventories[1])}");
        }

        // e.g. O:null,H:2,E:1
        private static string FormatInventory(Dictionary<char, int?> inventory)
        {
            return string.Join(",", inventory.Select(item => $"{item.Key}:{item.Value?.ToString() ?? "null"}"));
        }

        // Load game
        public static SavedGameData Load(string filePath)
        {
            return ParseSavedGame(File.ReadAllLines(filePath));
        }

        private static SavedGameData ParseSavedGame(string[] lines)
        {
            Dictionary<string, string> configFields = new();
            Dictionary<string, string>? currentStateFields = null;
            List<Dictionary<string, string>> undoStates = new();
            List<Dictionary<string, string>> redoStates = new();
            Dictionary<string, string>? activeSection = null;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line == "" || line.StartsWith("#"))
                    continue;

                switch (line)
                {
                    case "[CONFIGURATION]":
                        activeSection = configFields;
                        continue;

                    case "[CURRENT_STATE]":
                        currentStateFields = new Dictionary<string, string>();
                        activeSection = currentStateFields;
                        continue;

                    case "[UNDO_STATE]":
                        activeSection = new Dictionary<string, string>();
                        undoStates.Add(activeSection);
                        continue;

                    case "[REDO_STATE]":
                        activeSection = new Dictionary<string, string>();
                        redoStates.Add(activeSection);
                        continue;
                }

                if (activeSection == null)
                    throw new FormatException("Data found outside a section.");

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    throw new FormatException($"Invalid line: {line}");

                activeSection[line.Substring(0, separator).Trim()] = line.Substring(separator + 1).Trim();
            }

            if (currentStateFields == null)
                throw new FormatException("Missing current game state.");

            LaunchConfiguration config = new LaunchConfiguration
            {
                GameType = Enum.Parse<GameType>(configFields["GameType"]),
                Variant = Enum.Parse<GameVariant>(configFields["Variant"]),
                GameMode = Enum.Parse<GameMode>(configFields["GameMode"])
            };

            if (configFields.TryGetValue("AIDifficulty", out string? difficulty))
                config.AIDifficulty = Enum.Parse<AIDifficulty>(difficulty);

            return new SavedGameData
            {
                Configuration = config,
                CurrentState = ParseGameState(currentStateFields),
                UndoHistory = undoStates.Select(fields => ParseGameState(fields)).ToList(),
                RedoHistory = redoStates.Select(fields => ParseGameState(fields)).ToList()
            };
        }

        private static GameState ParseGameState(Dictionary<string, string> fields)
        {
            return new GameState
            {
                CurrentPlayerIndex = int.Parse(fields["CurrentPlayerIndex"]),
                TurnCount = int.Parse(fields["TurnCount"]),
                Result = Enum.Parse<GameResult>(fields["GameResult"]),
                BoardRows = fields["Board"].Split('/').ToList(),
                PlayerInventories = new List<Dictionary<char, int?>>
                {
                    ParseInventory(fields["Player1Inventory"]),
                    ParseInventory(fields["Player2Inventory"])
                }
            };
        }

        private static Dictionary<char, int?> ParseInventory(string input)
        {
            Dictionary<char, int?> inventory = new Dictionary<char, int?>();

            foreach (string item in input.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                char symbol = char.Parse(parts[0]);
                int? amount = parts[1] == "null" ? null : int.Parse(parts[1]);
                inventory.Add(symbol, amount);
            }
            return inventory;
        }
    }
}