//using Harmony;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DetailsScreen;

namespace FUtility.FUI
{
	public class SideScreen
	{
		public static void AddClonedSideScreen<T>(string name, string originalName, Type originalType, SidescreenTabTypes targetTab = SidescreenTabTypes.Config)
		{
			bool elementsReady = GetElements(out List<SideScreenRef> screens, out var tabs);
			if (elementsReady)
			{
				var contentBody = GetContentBodyForTab(targetTab, tabs);
				var oldPrefab = FindOriginal(originalName, screens);
				var newPrefab = Copy<T>(oldPrefab, contentBody, name, originalType);

				screens.Add(NewSideScreen(name, newPrefab));
			}
		}

		public static void AddClonedSideScreen<T>(string name, Type originalType, SidescreenTabTypes targetTab = SidescreenTabTypes.Config)
		{
			bool elementsReady = GetElements(out List<SideScreenRef> screens, out var tabs);
			if (elementsReady)
			{
				var contentBody = GetContentBodyForTab(targetTab, tabs);
				var oldPrefab = FindOriginal(originalType, screens);
				var newPrefab = Copy<T>(oldPrefab, contentBody, name, originalType);

				screens.Add(NewSideScreen(name, newPrefab));
			}
		}

		public static void AddCustomSideScreen<T>(string name, GameObject prefab)
		{
			bool elementsReady = GetElements(out List<SideScreenRef> screens, out var tabs);
			if (elementsReady)
			{
				// DetailsScreen.OnPrefabInit fires on every world load / new game, and this method
				// registers the shared asset-bundle prefab itself (it is never cloned). AddComponent-ing
				// T again on each call stacks duplicate SideScreenContents onto the same prefab; the extra
				// ones re-run OnPrefabInit against an already-mutated hierarchy, fail their transform.Find
				// calls, and end up registered with null UI fields (e.g. a null dropdown) that crash the
				// whole object-selection system on select. Add the component once and reuse it thereafter.
				var newScreen = prefab.GetComponent(typeof(T)) as SideScreenContent
					?? prefab.AddComponent(typeof(T)) as SideScreenContent;
				screens.Add(NewSideScreen(name, newScreen));
			}
		}

		private static bool GetElements(out List<SideScreenRef> screens, out List<SidescreenTab> tabs)
		{
			var detailsScreen = Traverse.Create(DetailsScreen.Instance);
			screens = detailsScreen.Field("sideScreens").GetValue<List<SideScreenRef>>();
			tabs = detailsScreen.Field("sidescreenTabs").GetValue<SidescreenTab[]>().ToList();

			return screens != null && tabs != null;
		}

		private static GameObject GetContentBodyForTab(SidescreenTabTypes targetTab, List<SidescreenTab> tabs)
		{
			foreach (var tab in tabs)
			{
				if (tab.type == targetTab)
					return tab.bodyInstance;
			}

			Log.Warning($"no targetTab {targetTab.GetType().Name}");
			return null;
		}

		private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
		{
			foreach (var screen in screens)
			{
				Log.Debug(screen.name, screen?.screenPrefab.GetType());
			}

			var result = screens.Find(s => s.name == name).screenPrefab;

			if (result == null)
				Debug.LogWarning("Could not find a sidescreen with the name " + name);

			return result;
		}

		private static SideScreenContent FindOriginal(Type type, List<SideScreenRef> screens)
		{
			foreach (var screen in screens)
			{
				Log.Debug(screen.name, screen.GetType());
			}

			var result = screens.Find(s => s?.screenPrefab.GetType() == type)?.screenPrefab;

			if (result == null)
				Debug.LogWarning("Could not find a sidescreen with the type " + type);

			return result;
		}

		private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
		{
			var screen = Util.KInstantiateUI<SideScreenContent>(original.gameObject, contentBody).gameObject;
			UnityEngine.Object.Destroy(screen.GetComponent(originalType));

			var prefab = screen.AddComponent(typeof(T)) as SideScreenContent;
			prefab.name = name.Trim();

			screen.SetActive(false);
			return prefab;
		}

		private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab)
		{
			return new SideScreenRef
			{
				name = name,
				offset = Vector2.zero,
				screenPrefab = prefab
			};
		}
	}
}
