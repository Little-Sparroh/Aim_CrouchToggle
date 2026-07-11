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
        AimCrouchTogglesPlugin.aimAction = PlayerInput.Controls?.Player.Aim;
        AimCrouchTogglesPlugin.ConfigureAimSubscription();
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputPerformed")]
    [HarmonyPrefix]
    public static bool SkipPrefix()
    {
        return !AimCrouchTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputCancelled")]
    [HarmonyPrefix]
    public static bool SkipPrefixCancelled()
    {
        return !AimCrouchTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "HandleAim")]
    [HarmonyPrefix]
    public static void HandleAimPrefix(Gun __instance)
    {
        if (AimCrouchTogglesPlugin.toggleAim.Value)
        {
            AimCrouchTogglesPlugin.isAimInputHeldField.SetValue(__instance, AimCrouchTogglesPlugin.isAimToggled);
            if (AimCrouchTogglesPlugin.isAimToggled)
            {
                AimCrouchTogglesPlugin.lastPressedAimTimeField.SetValue(__instance, Time.time);
            }
        }
    }

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    public static void UpdatePostfix(Gun __instance)
    {
        if (AimCrouchTogglesPlugin.toggleAim.Value)
        {
            bool isAiming = (bool)AimCrouchTogglesPlugin.isAimingGetter.Invoke(__instance, null);
            bool wantsToFire = (bool)AimCrouchTogglesPlugin.wantsToFireGetter.Invoke(__instance, null);
            float lastFireTime = (float)AimCrouchTogglesPlugin.lastFireTimeGetter.Invoke(__instance, null);
            float lastPressedFireTime = (float)AimCrouchTogglesPlugin.lastPressedFireTimeField.GetValue(__instance);
            Player player = (Player)AimCrouchTogglesPlugin.playerField.GetValue(__instance);
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
        AimCrouchTogglesPlugin.isAimToggled = false;
    }
}
