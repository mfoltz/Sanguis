using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using VampireCommandFramework;

namespace Sanguis;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    private Harmony _harmony;
    internal static Plugin Instance { get; private set; }
    public static Harmony Harmony => Instance._harmony;
    public static ManualLogSource LogInstance => Instance.Log;

    public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string PlayerTokensPath = Path.Combine(ConfigPath, "PlayerSanguis");

    private static ConfigEntry<bool> _Sanguisystem;
    private static ConfigEntry<bool> _dailyLogin;
    private static ConfigEntry<int> _dailyReward;
    private static ConfigEntry<int> _dailyQuantity;
    private static ConfigEntry<int> _SanguisReward;
    private static ConfigEntry<int> _SanguisRewardRatio;
    private static ConfigEntry<int> _SanguisPerMinute;
    private static ConfigEntry<int> _updateInterval;
    public static bool TokenSystem => _Sanguisystem.Value;
    public static bool DailyLogin => _dailyLogin.Value;
    public static int DailyReward => _dailyReward.Value;
    public static int DailyQuantity => _dailyQuantity.Value;
    public static int TokenReward => _SanguisReward.Value;
    public static int TokenRewardRatio => _SanguisRewardRatio.Value;
    public static int TokensPerMinute => _SanguisPerMinute.Value;
    public static int UpdateInterval => _updateInterval.Value;

    public override void Load()
    {
        Instance = this;
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        InitConfig();
        CommandRegistry.RegisterAll();
        LoadAllData();
        Core.Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}[{MyPluginInfo.PLUGIN_VERSION}] loaded!");
    }
    static void InitConfig()
    {
        CreateDirectories(ConfigPath);

        _Sanguisystem = InitConfigEntry("Config", "Sanguis", false, "Enable or disable Sanguis.");
        _dailyLogin = InitConfigEntry("Config", "DailyLogin", false, "Enable or disable daily login rewards.");
        _SanguisReward = InitConfigEntry("Config", "SanguisItemReward", 576389135, "Item prefab for Sanguis redeeming (crystals default).");
        _dailyReward = InitConfigEntry("Config", "DailyItemReward", -257494203, "Item prefab for daily login (crystals default).");
        _dailyQuantity = InitConfigEntry("Config", "DailyItemQuantity", 50, "Amount rewarded for daily login.");
        _SanguisRewardRatio = InitConfigEntry("Config", "SanguisRewardFactor", 6, "Sanguis/reward when redeeming.");
        _SanguisPerMinute = InitConfigEntry("Config", "SanguisPerMinute", 5, "Sanguis/minute spent online.");
        _updateInterval = InitConfigEntry("Config", "SanguisUpdateInterval", 30, "Interval in minutes to update player Sanguis.");
     }

    static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description)
    {
        // Bind the configuration entry and get its value
        var entry = Instance.Config.Bind(section, key, defaultValue, description);

        // Check if the key exists in the configuration file and retrieve its current value
        var configFile = Path.Combine(ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");
        if (File.Exists(configFile))
        {
            var config = new ConfigFile(configFile, true);
            if (config.TryGetEntry(section, key, out ConfigEntry<T> existingEntry))
            {
                // If the entry exists, update the value to the existing value
                entry.Value = existingEntry.Value;
            }
        }
        return entry;
    }
    static void CreateDirectories(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    public override bool Unload()
    {
        Config.Clear();
        _harmony.UnpatchSelf();
        return true;
    }
    static void LoadAllData()
    {
        if (_Sanguisystem.Value) Core.DataStructures.LoadPlayerTokens();
    }  
}
