using FUtility;
using PrintingPodRecharge.Content.Cmps;
using System.Collections.Generic;
using UnityEngine;

namespace PrintingPodRecharge.Content.Items
{
    public class BioInkConfig : IMultiEntityConfig
    {
        public const string DEFAULT = "PrintingPodRecharge_BioInk";
        public const string METALLIC = "PrintingPodRecharge_MetallicBioInk";
        public const string VACILLATING = "PrintingPodRecharge_VacillatingBioInk";
        public const string SEEDED = "PrintingPodRecharge_SeededBioInk";
        public const string GERMINATED = "PrintingPodRecharge_GerminatedBioInk";
        public const string FOOD = "PrintingPodRecharge_FoodBioInk";
        public const string SHAKER = "PrintingPodRecharge_ChaosBioInk";
        public const string TWITCH = "PrintingPodRecharge_TwitchBioInk";
        public const string MEDICINAL = "PrintingPodRecharge_Medicinal";
        public const string BIONIC = "PrintingPodRecharge_BionicBioInk";
        public const string GOURMET = "PrintingPodRecharge_GourmetBioInk";

        // Marker "tags" used only in the recipe table (Settings/Recipes.cs) to mean "any low-quality
        // food" / "any high-quality food". EntityConfigManagerPatch expands them into the actual food
        // members at load time (quality-split + raw-vs-prepared weighting). They are not real items.
        public const string FOOD_LOW_TAG = "PrintingPodRecharge_FoodLow";
        public const string FOOD_HIGH_TAG = "PrintingPodRecharge_FoodHigh";

        public static Dictionary<Bundle, string> itemsToBundle = new Dictionary<Bundle, string>();

        public List<GameObject> CreatePrefabs()
        {
            return new List<GameObject>()
            {
                CreateBioInk(DEFAULT, STRINGS.ITEMS.BIO_INK.NAME, STRINGS.ITEMS.BIO_INK.DESC, "rrp_oozing_bioink_kanim", Bundle.None),
                CreateBioInk(METALLIC, STRINGS.ITEMS.METALLIC_BIO_INK.NAME, STRINGS.ITEMS.METALLIC_BIO_INK.DESC, "rrp_metallic_bioink_kanim", Bundle.Metal),
                CreateBioInk(VACILLATING, STRINGS.ITEMS.VACILLATING_BIO_INK.NAME, STRINGS.ITEMS.VACILLATING_BIO_INK.DESC, "rrp_vacillating_bioink_kanim", Bundle.SuperDuplicant),
                CreateBioInk(GERMINATED, STRINGS.ITEMS.GERMINATED_BIO_INK.NAME, STRINGS.ITEMS.GERMINATED_BIO_INK.DESC, "rrp_germinated_bioink_kanim", Bundle.Egg),
                CreateBioInk(SEEDED, STRINGS.ITEMS.SEEDED_BIO_INK.NAME, STRINGS.ITEMS.SEEDED_BIO_INK.DESC, "rrp_seedy_bioink_kanim", Bundle.Seed),
                CreateBioInk(FOOD, STRINGS.ITEMS.FOOD_BIO_INK.NAME, STRINGS.ITEMS.FOOD_BIO_INK.DESC, "rrp_food_bioink_kanim", Bundle.Food),
                // Reuses the food-ink kanim with a warm gold tint to read as "fine cuisine".
                CreateBioInk(GOURMET, STRINGS.ITEMS.GOURMET_BIO_INK.NAME, STRINGS.ITEMS.GOURMET_BIO_INK.DESC, "rrp_food_bioink_kanim", Bundle.Gourmet, new Color(1f, 0.78f, 0.28f)),
                CreateBioInk(SHAKER, STRINGS.ITEMS.SHAKER_BIO_INK.NAME, STRINGS.ITEMS.SHAKER_BIO_INK.DESC, "rrp_rando_bioink_kanim", Bundle.Shaker),
                CreateBioInk(TWITCH, STRINGS.ITEMS.TWITCH_BIO_INK.NAME, STRINGS.ITEMS.TWITCH_BIO_INK.DESC, "rrp_twitch_bioink_kanim", Bundle.Twitch),
                CreateBioInk(MEDICINAL, STRINGS.ITEMS.MEDICINAL_BIO_INK.NAME, STRINGS.ITEMS.MEDICINAL_BIO_INK.DESC, "rrp_medicinal_bioink_kanim", Bundle.Medicinal),
                CreateBioInk(BIONIC, STRINGS.ITEMS.BIONIC_BIO_INK.NAME, STRINGS.ITEMS.BIONIC_BIO_INK.DESC, "rrp_bionic_bioink_kanim", Bundle.Bionic),
            };
        }

        public static GameObject CreateBioInk(string ID, string name, string description, string anim, Bundle bundle, Color? tint = null)
        {
            // Guard: a missing/misnamed anim would pass null to CreateLooseEntity and throw while the
            // whole ink list is being built, taking down every Bio-Ink. Fall back to the base ink's
            // anim so one bad asset can't nuke them all.
            var kanim = Assets.GetAnim(anim);
            if (kanim == null)
            {
                Log.Warning($"Bio-Ink '{ID}' anim '{anim}' not found; using the base Bio-Ink anim instead.");
                kanim = Assets.GetAnim("rrp_oozing_bioink_kanim");
            }

            var prefab = EntityTemplates.CreateLooseEntity(
                ID,
                name,
                description,
                1f,
                false,
                kanim,
                "object",
                Grid.SceneLayer.BuildingBack,
                EntityTemplates.CollisionShape.RECTANGLE,
                0.66f,
                0.8f,
                true,
                0,
                SimHashes.Creature,
                additionalTags: new List<Tag>
                {
                    GameTags.Organics,
                    ModAssets.Tags.bioInk,
                    GameTags.PedestalDisplayable
                });

            prefab.AddOrGet<EntitySplitter>();
            prefab.AddOrGet<SimpleMassStatusItem>();
            prefab.AddOrGet<BundleModifier>().bundle = bundle;

            if (tint.HasValue)
            {
                prefab.AddOrGet<AnimTinter>().tint = tint.Value;
            }

            itemsToBundle[bundle] = ID;

            return prefab;
        }

        // "Available in all versions" = vanilla + Spaced Out. The modern IHasDlcRestrictions API is
        // FORBIDDEN on IMultiEntityConfig (the game asserts "wrap the individual config instead"), so
        // we keep GetDlcIds. We return the ids directly rather than via the obsolete
        // DlcManager.AVAILABLE_ALL_VERSIONS, which keeps this both assert-free and warning-free.
        public string[] GetDlcIds()
        {
            return new[] { DlcManager.VANILLA_ID, DlcManager.EXPANSION1_ID };
        }

        public void OnPrefabInit(GameObject inst)
        {
        }

        public void OnSpawn(GameObject inst)
        {
        }
    }
}