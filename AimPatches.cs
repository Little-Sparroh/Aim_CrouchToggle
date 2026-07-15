using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

[HarmonyPatch]
public static class ToggleAimPatches
{
    [HarmonyPatch(typeof(PlayerInput), "Initialize")]
    [HarmonyPostfix]
    public static void PlayerInputInitializePostfix()
    {
        AimAndCrouchTogglesPlugin.aimAction = PlayerInput.Controls?.Player.Aim;
        AimAndCrouchTogglesPlugin.ConfigureAimSubscription();
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputPerformed")]
    [HarmonyPrefix]
    public static bool SkipPrefix()
    {
        return !AimAndCrouchTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputCancelled")]
    [HarmonyPrefix]
    public static bool SkipPrefixCancelled()
    {
        return !AimAndCrouchTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "HandleAim")]
    [HarmonyPrefix]
    public static void HandleAimPrefix(Gun __instance)
    {
        if (AimAndCrouchTogglesPlugin.toggleAim.Value)
        {
            AimAndCrouchTogglesPlugin.isAimInputHeldField.SetValue(__instance, AimAndCrouchTogglesPlugin.isAimToggled);
            if (AimAndCrouchTogglesPlugin.isAimToggled)
            {
                AimAndCrouchTogglesPlugin.lastPressedAimTimeField.SetValue(__instance, Time.time);
            }
        }
    }

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    public static void UpdatePostfix(Gun __instance)
    {
        if (AimAndCrouchTogglesPlugin.toggleAim.Value)
        {
            bool isAiming = (bool)AimAndCrouchTogglesPlugin.isAimingGetter.Invoke(__instance, null);
            bool wantsToFire = (bool)AimAndCrouchTogglesPlugin.wantsToFireGetter.Invoke(__instance, null);
            float lastFireTime = (float)AimAndCrouchTogglesPlugin.lastFireTimeGetter.Invoke(__instance, null);
            float lastPressedFireTime = (float)AimAndCrouchTogglesPlugin.lastPressedFireTimeField.GetValue(__instance);
            Player player = (Player)AimAndCrouchTogglesPlugin.playerField.GetValue(__instance);
            if (player != null && !isAiming && !wantsToFire && Time.time - Mathf.Max(lastFireTime, lastPressedFireTime) > 0.5f)
            {
                player.ResumeSprint();
            }
        }
    }

    [HarmonyPatch(typeof(Player), "Resurrect_ClientRpc")]
    [HarmonyPostfix]
    public static void ResetTogglePostfix()
    {
        AimAndCrouchTogglesPlugin.isAimToggled = false;
    }
}
