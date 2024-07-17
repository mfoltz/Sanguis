using BepInEx.Logging;
using ProjectM;
using ProjectM.Scripting;
using Sanguis.Services;
using System.Text.Json;
using Unity.Entities;

namespace Sanguis;

internal static class Core
{
    public static World Server { get; } = GetWorld("Server") ?? throw new Exception("There is no Server world (yet)...");
    public static EntityManager EntityManager { get; } = Server.EntityManager;
    public static ServerScriptMapper ServerScriptMapper { get; internal set; }
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static PrefabCollectionSystem PrefabCollectionSystem { get; internal set; }
    public static LocalizationService Localization => new();
    public static SanguisService SanguisService => new();
    public static ManualLogSource Log => Plugin.LogInstance;

    public static bool hasInitialized;
    public static void Initialize()
    {
        if (hasInitialized) return;

        ServerScriptMapper = Server.GetExistingSystemManaged<ServerScriptMapper>();
        PrefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
        // Initialize utility services
        hasInitialized = true;
    }
    static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            if (world.Name == name)
            {
                return world;
            }
        }
        return null;
    }
    public class DataStructures
    {
        // Encapsulated fields with properties

        static readonly JsonSerializerOptions prettyJsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        // structures to write to json for permanence

        static Dictionary<ulong, (int Tokens, (DateTime Start, DateTime DailyLogin) TimeData)> playerTokens = [];

        public static Dictionary<ulong, (int Tokens, (DateTime Start, DateTime DailyLogin) TimeData)> PlayerTokens
        {
            get => playerTokens;
            set => playerTokens = value;
        }

        // file paths dictionary
        static readonly Dictionary<string, string> filePaths = new()
        {
            {"Tokens", JsonFiles.PlayerTokenJsons},
        };

        // Generic method to save any type of dictionary.
        public static void LoadData<T>(ref Dictionary<ulong, T> dataStructure, string key)
        {
            string path = filePaths[key];
            if (!File.Exists(path))
            {
                // If the file does not exist, create a new empty file to avoid errors on initial load.
                File.Create(path).Dispose();
                dataStructure = []; // Initialize as empty if file does not exist.
                return;
            }
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    // Handle the empty file case
                    dataStructure = []; // Provide default empty dictionary
                }
                else
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, T>>(json, prettyJsonOptions);
                    dataStructure = data ?? []; // Ensure non-null assignment
                }
            }
            catch (IOException)
            {
                dataStructure = []; // Provide default empty dictionary on error.
            }
            catch (JsonException)
            {
                dataStructure = []; // Provide default empty dictionary on error.
            }
        }
        public static void LoadPlayerTokens() => LoadData(ref playerTokens, "Tokens");
        public static void SaveData<T>(Dictionary<ulong, T> data, string key)
        {
            string path = filePaths[key];
            try
            {
                string json = JsonSerializer.Serialize(data, prettyJsonOptions);
                File.WriteAllText(path, json);
            }
            catch (IOException ex)
            {
                Log.LogError($"Failed to write {key} data to file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Log.LogError($"JSON serialization error when saving {key} data: {ex.Message}");
            }
        }
        public static void SavePlayerTokens() => SaveData(PlayerTokens, "Tokens");
    }
    static class JsonFiles
    {
        public static readonly string PlayerTokenJsons = Plugin.PlayerTokensPath;
    }
}