using FUtility.SaveData;
using PrintingPodRecharge.Content.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrintingPodRecharge.Settings
{
    public class Recipes : IUserSetting
    {
        public class FRecipe
        {
            public string Description { get; set; }

            public float Time { get; set; } = 40f;

            public FRecipeElement[] Inputs { get; set; }

            public FRecipeElement[] Outputs { get; set; }
        }


        public class FRecipeElement
        {
            public string ID { get; set; }

            public float Amount { get; set; }

            public FRecipeElement(Tag iD, float amount)
            {
                ID = iD.ToString();
                Amount = amount;
            }
        }

        public List<FRecipe> BioInks { get; set; } = new List<FRecipe>()
        {
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.GERMINATED_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(RawEggConfig.ID, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.GERMINATED, 2)
                }
            },

            // Accept ANY crop seed via the category tag instead of one recipe per plant.
            // The player can easily verify which seed they're spending in the crafting UI.
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.SEEDED_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(GameTags.CropSeed, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.SEEDED, 2)
                }
            },

            // Basic (Nutritious) ink: any LOW-quality food (quality <= Good). EntityConfigManagerPatch
            // expands the marker into the actual foods and weights raw food heavier than prepared
            // (cooked) food. Output bundle stays the broad food mix, so high-quality food can still
            // turn up rarely just by being in the pool. Amount here is the base (prepared) cost.
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.FOOD_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(BioInkConfig.FOOD_LOW_TAG, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.FOOD, 2)
                }
            },

            // Gourmet ink: any HIGH-quality food (quality >= Great). Outputs the gourmet bundle
            // (high-quality food only) - trade one delicacy for another. High-quality food is
            // essentially all prepared, so the raw weighting rarely applies here.
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.GOURMET_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(BioInkConfig.FOOD_HIGH_TAG, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.GOURMET, 2)
                }
            },

            // Accept ANY refined metal via the category tag instead of one recipe per metal.
            // The crafting table lets the player choose which refined metal to spend.
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.METALLIC_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(GameTags.RefinedMetal, 25)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.METALLIC, 2)
                }
            },

            // Emergency bionic survival ink. 4 Microchips ("PowerStationTools") = two Basic Boosters
            // / half an Advanced Booster — the bionic "currency". Auto-gates to the Bionic pack: with
            // no pack there are no Microchips, so CreateRecipe's prefab check skips this recipe.
            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.BIONIC_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement("PowerStationTools", 4)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.BIONIC, 2)
                }
            },

            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.VACILLATING_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(GeneShufflerRechargeConfig.ID, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.VACILLATING, 2)
                }
            },

            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.SHAKER_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(SimHashes.SlimeMold.CreateTag(), 20),
                    new FRecipeElement(SimHashes.Water.CreateTag(), 5),
                    new FRecipeElement(MeatConfig.ID, 1),
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.SHAKER, 2)
                }
            },

            new FRecipe()
            {
                Description = "PrintingPodRecharge.STRINGS.ITEMS.MEDICINAL_BIO_INK.DESC",
                Time = 40f,
                Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(BasicBoosterConfig.ID, 1)
                },
                Outputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.MEDICINAL, 2)
                }
            },
        };

        internal bool Process()
        {
            if(!BioInks.Any(i => i.Outputs[0].ID == BioInkConfig.MEDICINAL))
            {
                BioInks.Add(new FRecipe()
                {
                    Description = "PrintingPodRecharge.STRINGS.ITEMS.MEDICINAL_BIO_INK.DESC",
                    Time = 40f,
                    Inputs = new FRecipeElement[] {
                    new FRecipeElement(BioInkConfig.DEFAULT, 2),
                    new FRecipeElement(BasicBoosterConfig.ID, 1)
                    },
                        Outputs = new FRecipeElement[] {
                        new FRecipeElement(BioInkConfig.MEDICINAL, 2)
                    }
                });

                return true;
            }

            return false;
        }
    }
}