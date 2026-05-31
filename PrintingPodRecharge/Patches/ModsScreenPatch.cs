using FUtility;
using FUtility.FUI;
using HarmonyLib;
using PrintingPodRecharge.UI;

namespace PrintingPodRecharge.Patches
{
    class ModsScreenPatch
    {
        // U59 fix: disabled. This Postfix added a "Settings" button to PrintingPodRecharge's entry in
        // the mods list, but patching ModsScreen collides with the Mod Manager mod (which replaces that
        // screen), shifting the Mod Manager's Tools texture initialization onto a background thread and
        // crashing it on open. Removing the [HarmonyPatch] attribute means Harmony.PatchAll skips this,
        // so our mod no longer touches the mods screen. (The bio-ink config dialog stays in the code,
        // just without this redundant launch button.)
        // [HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
        public static class ModsScreen_BuildDisplay_Patch
        {
            public static void Postfix(object ___displayedMods)
            {
                ModMenuButton.AddModSettingsButton(___displayedMods, "PrintingPodRecharge", OpenModSettingsScreen);
            }

            private static void OpenModSettingsScreen()
            {
                Helper.CreateFDialog<ModSettingsScreen>(ModAssets.Prefabs.settingsDialog, "BioInkSettings");
            }
        }
    }
}
