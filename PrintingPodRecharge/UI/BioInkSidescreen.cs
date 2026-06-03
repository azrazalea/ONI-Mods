using FUtility;
using FUtility.FUI;
using PrintingPodRecharge.Content.Cmps;
using PrintingPodRecharge.Content.Items;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrintingPodRecharge.UI
{
	public class BioInkSidescreen : SideScreenContent
	{
		private BioPrinter printer;
		private static List<Option> options = new List<Option>();

		private LocText descriptionLabel;
		private TextFitter descriptionBoxFitter;

		private TMP_Dropdown dropdown;

		private FButton actionButton;
		private FButton cancelButton;
		private LocText actionButtonLabel;
		private LocText cancelButtonLabel;

		// U59: SideScreenContent.GetTitle() returns Strings.Get(titleKey); this side screen never set
		// titleKey, so U59 rendered the header as "Missing.". Provide the title aki already wrote.
		public override string GetTitle()
		{
			return STRINGS.UI.SETTINGSDIALOG.TITLE.TITLETEXT;
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();

			dropdown = transform.Find("Contents/BeakerSelector/Dropdown").GetComponent<TMP_Dropdown>();

			var descriptionBox = transform.Find("Contents/Description").GetComponent<LayoutElement>();
			descriptionLabel = descriptionBox.transform.Find("Label").GetComponent<LocText>();
			descriptionBoxFitter = descriptionBox.FindOrAddComponent<TextFitter>();
			descriptionBoxFitter.targetText = descriptionLabel;

			var item = dropdown.transform.Find("Template/Viewport/Content/Item");

			actionButton = transform.Find("Contents/Buttons/Deliver").FindOrAddComponent<FButton>();
			actionButtonLabel = actionButton.transform.Find("Text").FindOrAddComponent<LocText>();
			actionButtonLabel.SetText(STRINGS.UI.BIOINKSIDESCREEN.CONTENTS.BUTTONS.DELIVER.TEXT);

			cancelButton = transform.Find("Contents/Buttons/Activate").FindOrAddComponent<FButton>();
			cancelButtonLabel = cancelButton.transform.Find("Text").FindOrAddComponent<LocText>();

			// when converting to LocText these references were lost, so they need to be rebound
			dropdown.itemText = item.Find("Item Label").GetComponent<LocText>();
			dropdown.captionText = dropdown.transform.Find("Label").GetComponent<LocText>();

			AddOptions();
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			cancelButtonLabel.SetText(global::STRINGS.UI.CANCELPLACEINRECEPTACLE);

			dropdown.onValueChanged.AddListener(OnDropdownChanged);
			actionButton.OnClick += OnButtonClicked;
			cancelButton.OnClick += OnCancelClicked;
		}

		private void OnCancelClicked()
		{
			if (printer == null)
			{
				return;
			}

			printer.CancelDelivery();

			RefreshButtons();
		}

		private void OnButtonClicked()
		{
			if (printer == null)
			{
				return;
			}

			if (printer.CanStartPrint())
			{
				printer.StartBonusPrint();
			}
			else
			{
				printer.SetDelivery(options[dropdown.value].prefabID);
			}

			RefreshButtons();
		}

		private void OnDropdownChanged(int index)
		{
			if (printer != null)
			{
				printer.inkTag = options[index].prefabID;

				Log.Debug("dropdown changed to " + printer.inkTag);
				SetDescription(options[index].description);
				actionButton.SetInteractable(true);
			}
		}

		private void RefreshButtons()
		{
			if (printer.inkTag != Tag.Invalid)
			{
				dropdown.interactable = false;

				if (printer.CanStartPrint())
				{
					actionButtonLabel.SetText(STRINGS.UI.BIOINKSIDESCREEN.CONTENTS.BUTTONS.ACTIVATE.TEXT);

					cancelButton.SetInteractable(false);
					actionButton.SetInteractable(!printer.isPrinterBusy);
				}
				else
				{
					actionButtonLabel.SetText(STRINGS.UI.BIOINKSIDESCREEN.CONTENTS.BUTTONS.DELIVER.TEXT);

					if (printer.isDeliveryActive)
					{
						actionButton.SetInteractable(false);
						cancelButton.SetInteractable(true);
					}
					else
					{
						dropdown.interactable = true;
						actionButton.SetInteractable(true);
						cancelButton.SetInteractable(false);
					}
				}
			}
			else
			{
				actionButtonLabel.SetText(STRINGS.UI.BIOINKSIDESCREEN.CONTENTS.BUTTONS.DELIVER.TEXT);

				dropdown.interactable = true;
				actionButton.SetInteractable(true);
				cancelButton.SetInteractable(false);
			}
		}

		public void AddOptions()
		{
			options.Clear();

			foreach (var ink in Assets.GetPrefabsWithTag(ModAssets.Tags.bioInk))
			{
				Log.Debug("ink" + ink.PrefabID());
				Log.Debug(ink.GetComponent<BundleModifier>() != null);

				var bundle = ink.GetComponent<BundleModifier>().bundle;

				if (ImmigrationModifier.Instance.IsBundleAvailable(bundle))
				{
					Log.Debug("added ink " + ink.GetProperName());
					options.Add(new Option(ink));
				}
			}

			dropdown.options.Clear();

			var newOptions = new List<TMP_Dropdown.OptionData>();

			foreach (var option in options)
			{
				// U59 TextMeshPro removed the OptionData(string, Sprite) ctor; use the (string, Sprite, Color) one.
				newOptions.Add(new TMP_Dropdown.OptionData(option.name, option.sprite, Color.white));
			}

			dropdown.AddOptions(newOptions);
			dropdown.RefreshShownValue();
		}

		public override bool IsValidForTarget(GameObject target)
		{
			return target.GetComponent<BioPrinter>() != null;
		}

		public override void SetTarget(GameObject target)
		{
			base.SetTarget(target);

			printer = target.GetComponent<BioPrinter>();

			if (printer == null)
			{
				return;
			}

			// Safety net: if OnPrefabInit failed to bind the UI (e.g. a corrupted prefab left dropdown
			// null), bail out instead of throwing an NRE that propagates up through DetailsScreen.Refresh
			// and crashes the entire object-selection system. The idempotent fix in
			// SideScreen.AddCustomSideScreen should keep this from ever happening.
			if (dropdown == null)
			{
				Log.Warning("BioInkSidescreen.SetTarget: dropdown is null (prefab not initialized); skipping.");
				return;
			}

			SetInk(printer.lastInkTag);
			RefreshButtons();

			SetDescription(options[dropdown.value].description);
		}

		private int GetOptionIndex(Tag tag)
		{
			return options.FindIndex(o => o.prefabID == tag);
		}

		private void SetInk(Tag ink)
		{
			var index = GetOptionIndex(ink);
			index = Math.Max(index, 0);

			dropdown.value = index;
			dropdown.RefreshShownValue();
			dropdown.interactable = false;
		}

		private void SetDescription(string text)
		{
			descriptionLabel.text = text;
			descriptionBoxFitter.SetLayoutVertical();
		}

		public class Option
		{
			public string name;
			public string description;
			public Sprite sprite;
			public Tag prefabID;
			public bool available;

			public Option(GameObject go)
			{
				prefabID = go.PrefabID();
				name = go.GetProperName();
				if (go.TryGetComponent(out InfoDescription infoDescription))
				{
					description = infoDescription.description;
				}
				sprite = Def.GetUISprite(go).first;
			}
		}
	}
}
