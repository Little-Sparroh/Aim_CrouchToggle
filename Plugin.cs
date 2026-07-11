using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Pigeon.Movement;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class AimCrouchTogglesPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.aimcrouchtoggles";
    public const string PluginName = "AimCrouchToggles";
    public const string PluginVersion = "1.0.0";

    internal static new ManualLogSource Logger;

    internal static ConfigEntry<bool> toggleAim;
    internal static ConfigEntry<bool> toggleCrouch;

    internal static bool isAimToggled = false;
    internal static InputAction aimAction;

    internal static FieldInfo isAimInputHeldField;
    internal static FieldInfo lastPressedAimTimeField;
    internal static FieldInfo lastPressedFireTimeField;
    internal static FieldInfo playerField;
    internal static MethodInfo isAimingGetter;
    internal static MethodInfo wantsToFireGetter;
    internal static MethodInfo lastFireTimeGetter;

    internal static AimCrouchTogglesPlugin Instance { get; set; }

    private FileSystemWatcher configWatcher;
    private volatile bool configReloadPending;
    private int lastConfigChangeTick;
    private const int ConfigReloadDebounceMs = 250;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;

        toggleAim = Config.Bind("General", "Toggle Aim", true, "If true, aim becomes a toggle (press to enter/exit) instead of hold.");
        toggleCrouch = Config.Bind("General", "Toggle Crouch", true, "If true, enables toggle crouch functionality (hold crouch by pressing slide button).");

        toggleAim.SettingChanged += OnToggleAimChanged;
        toggleCrouch.SettingChanged += OnToggleCrouchChanged;

        SetupConfigWatcher();

        try
        {
            SetupAccessTools();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error setting up access tools: {ex.Message}");
        }

        try
        {
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll(typeof(ToggleAimPatches));
            harmony.PatchAll(typeof(ToggleCrouchPatches));
            harmony.PatchAll(typeof(EndCrouchPatches));
            harmony.PatchAll(typeof(EndSlidePatches));
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error applying patches: {ex.Message}");
        }

        Logger.LogInfo($"{PluginName} loaded successfully.");
    }

    private void Update()
    {
        if (!configReloadPending)
            return;

        // Debounce on the main thread so rapid editor write events settle first.
        int elapsedMs = unchecked(Environment.TickCount - lastConfigChangeTick);
        if (elapsedMs < ConfigReloadDebounceMs)
            return;

        configReloadPending = false;

        try
        {
            Config.Reload();
            Logger.LogInfo("Config reloaded from disk.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to reload config: {ex.Message}");
        }
    }

    private void SetupConfigWatcher()
    {
        try
        {
            string configPath = Config.ConfigFilePath;
            string directory = Path.GetDirectoryName(configPath);
            string fileName = Path.GetFileName(configPath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            {
                Logger.LogWarning("Could not set up config hot-reload: invalid config path.");
                return;
            }

            configWatcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            configWatcher.Changed += OnConfigFileChanged;
            configWatcher.Created += OnConfigFileChanged;
            configWatcher.Renamed += OnConfigFileRenamed;

            Logger.LogInfo($"Watching config for hot-reload: {configPath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to set up config watcher: {ex.Message}");
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // FileSystemWatcher callbacks are not on the Unity main thread.
        lastConfigChangeTick = Environment.TickCount;
        configReloadPending = true;
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        // Some editors save via temp file + rename.
        string configFileName = Path.GetFileName(Config.ConfigFilePath);
        if (string.Equals(e.Name, configFileName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(e.OldName, configFileName, StringComparison.OrdinalIgnoreCase))
        {
            lastConfigChangeTick = Environment.TickCount;
            configReloadPending = true;
        }
    }

    private void SetupAccessTools()
    {
        isAimInputHeldField = AccessTools.Field(typeof(Gun), "isAimInputHeld");
        lastPressedAimTimeField = AccessTools.Field(typeof(Gun), "lastPressedAimTime");
        lastPressedFireTimeField = AccessTools.Field(typeof(Gun), "lastPressedFireTime");
        playerField = AccessTools.Field(typeof(Gun), "player");
        isAimingGetter = AccessTools.PropertyGetter(typeof(Gun), "IsAiming");
        wantsToFireGetter = AccessTools.PropertyGetter(typeof(Gun), "WantsToFire");
        lastFireTimeGetter = AccessTools.PropertyGetter(typeof(Gun), "LastFireTime");
    }

    private void OnToggleAimChanged(object sender, EventArgs e)
    {
        ConfigureAimSubscription();
    }

    private void OnToggleCrouchChanged(object sender, EventArgs e)
    {
        if (!toggleCrouch.Value)
        {
            ToggleCrouchPatches.isToggleOn = false;
        }
    }

    internal static void ConfigureAimSubscription()
    {
        if (aimAction != null)
        {
            aimAction.started -= OnAimStarted;
            if (toggleAim.Value)
            {
                aimAction.started += OnAimStarted;
            }
            else
            {
                isAimToggled = false;
            }
        }
    }

    internal static void OnAimStarted(InputAction.CallbackContext context)
    {
        if (toggleAim.Value)
        {
            isAimToggled = !isAimToggled;
        }
    }

    private void OnDestroy()
    {
        if (configWatcher != null)
        {
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Changed -= OnConfigFileChanged;
            configWatcher.Created -= OnConfigFileChanged;
            configWatcher.Renamed -= OnConfigFileRenamed;
            configWatcher.Dispose();
            configWatcher = null;
        }

        if (aimAction != null)
        {
            aimAction.started -= OnAimStarted;
        }
    }
}
