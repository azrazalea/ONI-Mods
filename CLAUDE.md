# Build & deploy notes (azrazalea fork of aki-art/ONI-Mods)

Local working notes for building the mods we actually touch here — primarily
**PrintingPodRecharge** (the "Bio-Inks: Rechargeable Printing Pod" mod). Game target:
ONI U59 beta (Spaced Out). Read this before fighting the build; the ILRepack step has
two non-obvious gotchas that cost a lot of time to rediscover.

## TL;DR — build PrintingPodRecharge (the bundled, deployable DLL)

Use **full-framework MSBuild from Visual Studio 2022**, and run **restore and build as
SEPARATE invocations**:

```bash
MSB="/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe"
cd /c/Users/azraz/Documents/aki-ONI-Mods

# 1) restore FIRST, on its own (this is what makes the ILRepack UsingTask resolve)
"$MSB" PrintingPodRecharge/PrintingPodRecharge.csproj -t:Restore -v:quiet -nologo

# 2) then build, config = Release2 (the project ONLY defines Debug;Release2 — there is no "Release")
"$MSB" PrintingPodRecharge/PrintingPodRecharge.csproj -t:Build -p:Configuration=Release2 -v:minimal -nologo
```

Output: `PrintingPodRecharge/bin/PrintingPodRecharge.dll`, **~230 KB** (FUtility is ILRepacked
**in**; ONITwitchLib stays **external/not bundled**). A ~146 KB DLL means ILRepack did NOT run
(FUtility not merged — not deployable).

Verify the merge + your changes landed:
```bash
cd PrintingPodRecharge/bin
ls -l PrintingPodRecharge.dll                 # expect ~230 KB, and NO standalone FUtility.dll
grep -c FUtility PrintingPodRecharge.dll      # >0 => FUtility merged in
```

## The two gotchas (why naive builds fail)

1. **`dotnet build` cannot run ILRepack.** `ILRepack.Lib.MSBuild.Task` (2.0.18.2) is a
   full-framework task needing `Microsoft.Build.Utilities.v4.0`, which the .NET SDK MSBuild
   doesn't provide → `error MSB4062: The "ILRepack" task could not be loaded`. You MUST use
   VS2022's `MSBuild.exe` (full framework).
   - `dotnet build PrintingPodRecharge/PrintingPodRecharge.csproj -p:IsPacked=false` is still
     useful as a **quick compile check** (skips ILRepack; produces a non-bundled, non-deployable
     DLL). Build succeeds with 0 errors = your C# is fine.

2. **Restore must be a separate MSBuild invocation.** `-t:Restore,Build` (or `-restore`) in one
   call fails with `error MSB4036: The "ILRepack" task was not found`, because the ILRepack
   package's `UsingTask` (imported from its restored `build/*.props`) isn't picked up within the
   same evaluation that just restored it. Run `-t:Restore` once, then `-t:Build` as a second
   process (as in the TL;DR).

## Config / environment

- **Target framework: `netstandard2.1`** (net48 fails on current ONI: ReadOnlySpan + netstandard
  2.1 facade + default interface methods). Don't "fix" it back to net48.
- **`Configuration=Release2`** is the real release config (optimized). The only other config is
  `Debug` (compiles `DEBUG` → verbose logging). There is intentionally no `Release`.
- **`SteamFolder`** is set in `Directory.Build.props` to `C:\Program Files (x86)\Steam` — correct
  for the current install (game lives at
  `C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded`). If the game ever moves
  (it was previously on `F:\SteamLibrary`), override per-build with
  `-p:SteamFolder="<drive>:\SteamLibrary"` (or whatever the library root is). Game DLLs resolve
  from `$(SteamFolder)\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`.
- **FUtility** builds as a `ProjectReference` (`FUtility/FUtility.csproj`) and is ILRepacked into
  the mod. **ONITwitchLib** comes from `Lib/ONITwitchLib.dll` (v1.2.9) with `<Private>False</Private>`
  — external, twitch code is runtime-guarded, so the Twitch mod isn't a hard dependency.
- ILRepack config lives in `Directory.Build.targets` (`ILRepack` target, `AfterTargets="Build"`,
  gated by `IsPacked=true`, which is the project default). It merges every loose `*.dll` in the
  output dir except `0Harmony` / `System.*` / `Microsoft.*` / `UnityEngine.*`.

## Deploy (so the game picks it up)

We deploy by overwriting the installed Steam Workshop copy (a `.orig` backup is preserved):

```
copy  PrintingPodRecharge/bin/PrintingPodRecharge.dll
  ->  C:\Users\azraz\Documents\Klei\OxygenNotIncluded\mods\Steam\2869608898\PrintingPodRecharge.dll
```

- **The game must be fully closed** (quit to desktop) or the copy fails with a file lock.
- Workshop ID **2869608898**. The original Steam DLL is kept as `PrintingPodRecharge.dll.orig`
  in that folder — restore it to revert to stock.
- (`Directory.Build.targets` has a `CopyModFiles` target, but it's gated behind `DeployDevMod`
  which is off, so the build does NOT auto-deploy. We copy the DLL ourselves.)

## GOTCHA: recipes are loaded from a saved config that SHADOWS code changes

`Settings/Recipes.cs` is an `IUserSetting` loaded via `SaveDataManager<Recipes>` from
**`<KleiSave>/mods/config/PrintingPodRecharge/data/recipes.json`** (the "Klei save" root is
`C:\Users\azraz\Documents\Klei\OxygenNotIncluded`). On read it uses
`ObjectCreationHandling.Replace`, so the JSON **entirely replaces the code defaults** in
`Recipes.BioInks`. The file is only written when it does **not** exist (`WriteIfDoesntExist`) —
it is NOT force-regenerated (unlike the bundle JSONs, which `Mod.cs` regenerates with `force:true`).

**Consequence:** editing a recipe in `Recipes.cs` and rebuilding does NOTHING if that json already
exists — the stale file keeps winning. This is exactly what made a Food-ink change (MushBar→Edible)
appear to "not work" — the crafting table kept showing only Mush Bar.

**Fix / dev workflow:** after changing recipes in code, delete (or rename) the json so it
regenerates from the new code defaults on next launch:
```
del  "C:\Users\azraz\Documents\Klei\OxygenNotIncluded\mods\config\PrintingPodRecharge\data\recipes.json"
```
Then **fully restart ONI** (recipe/entity registration happens once at app/DB init, not per save load).
A back-up copy was left as `recipes.json.bak` in that folder.

## Misc

- Decompile the game for API spelunking with `ilspycmd` (v8.2.0.7535): one `-t <FullTypeName>`
  per call; `-o` is ignored with `-t` so redirect stdout; nested types use `+` (e.g.
  `ComplexRecipe+RecipeElement`) or just decompile the parent type.
- Bio-Ink crafting recipes live in `PrintingPodRecharge/Settings/Recipes.cs`; they're turned into
  `ComplexRecipe`s in `PrintingPodRecharge/Patches/EntityConfigManagerPatch.cs`. To accept "any X"
  (any refined metal / crop seed / edible / egg) the ingredient must be the **explicit member-tag
  array** (`RecipeElement(Tag[], amount)`), NOT a bare category tag — `ComplexRecipeManager`
  filters `possibleMaterials` to prefab-backed tags and a category tag has no prefab, so it
  silently vanishes from the crafting table. `EntityConfigManagerPatch` expands category markers
  to their members at load time (leaning on the game's own categorization).
