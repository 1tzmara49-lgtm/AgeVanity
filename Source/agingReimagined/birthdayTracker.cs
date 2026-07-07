using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace agingReimagined
{
    // Settings setup
    public class AgeVanity : Mod
    {
        public static agingSettings settings;
        public AgeVanity(ModContentPack content) : base(content)
        {
            settings = GetSettings<agingSettings>();

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label($"Wrinkle age threshold: {settings.rhytidesThreshold}");
            settings.rhytidesThreshold = (int)listingStandard.Slider(settings.rhytidesThreshold, 18f, 100f);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Age vanity";
        }
    }

    // Check if rhytides is blocked by any gene
    public static class HediffGeneChecker
    {
        public static bool IsHediffBlockedByGenes(Pawn pawn, HediffDef hediffDef)
        {
            if (pawn == null || hediffDef == null) return false;

            if (!ModsConfig.BiotechActive) return false;

            return CheckGenesInternal(pawn, hediffDef);
        }

        private static bool CheckGenesInternal(Pawn pawn, HediffDef hediffDef)
        {
        
            if (pawn.genes == null) return false;
            List<Gene> pawnGenes = pawn.genes.GenesListForReading;

            for (int i = 0; i < pawnGenes.Count; i++)
            {
                GeneDef geneDef = pawnGenes[i].def;

                if (geneDef.makeImmuneTo != null && geneDef.makeImmuneTo.Contains(hediffDef))
                {
                    return true; // Found a matching hediff
                }
            }

            return false; // No genes found
        }
    }

    //Harmony patch for birthdays.

    [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
    public static class Patch_Pawn_BirthdayEvent
    {
        public static void Postfix(Pawn_AgeTracker __instance, Pawn ___pawn)
        {
            if (___pawn == null) return;
            int newAge = ___pawn.ageTracker.AgeBiologicalYears;
            HediffDef hediff_rhytids = DefDatabase<HediffDef>.GetNamed("ar_rhytides", false);
            // Age check
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.Humanlike)
            {
                if (newAge >= AgeVanity.settings.rhytidesThreshold)
                {
                    // If hediff is not present / pawn alerady has hediff, do nothing
                    if (hediff_rhytids == null)
                    {
                        Log.Error("[Age Vanity] Could not find HediffDef to add. Check your XML definition name.");
                        return;
                    }
                    if (!___pawn.health.hediffSet.HasHediff(hediff_rhytids) || !HediffGeneChecker.IsHediffBlockedByGenes(pawn, hediff_rhytids))
                    {
                        ___pawn.health.AddHediff(hediff_rhytids);
                    }
                }
                else
                {
                    // If hediff is present, remove
                    Hediff activeWrinkles = ___pawn.health.hediffSet.GetFirstHediffOfDef(hediff_rhytids);

                    if (activeWrinkles != null)
                    {
                        ___pawn.health.RemoveHediff(activeWrinkles);
                    }
                }
            }
        }
    }

    // Harmony patch for any age-tampering that is not birthday. Should work on everything, not sure about performance though.

    [HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeBiologicalTicks), MethodType.Setter)]
    public static class Patch_PawnAgeJumps
    {
        [HarmonyPrefix]
        public static void Prefix (Pawn_AgeTracker __instance, out int __state)
        {
            __state = __instance.AgeBiologicalYears;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn_AgeTracker __instance, int __state)
        {
            int newAgeYears = __instance.AgeBiologicalYears;

            if (__state != newAgeYears)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn != null)
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        RefreshAgeHediffs(pawn, newAgeYears);
                    }
                }

            }
        }

        public static void RefreshAgeHediffs(Pawn pawn, int newAgeYears)
        {
            if (pawn == null)
            {
                return;
            }
            HediffDef hediff_rhytides = DefDatabase<HediffDef>.GetNamed("ar_rhytides");
            if (newAgeYears >= AgeVanity.settings.rhytidesThreshold)
            {
                if(hediff_rhytides == null)
                {
                    Log.Error("[Age vanity] Could not find HediffDef to add. Check your XML definition name.");
                    return;
                }
                if (!pawn.health.hediffSet.HasHediff(hediff_rhytides) || !HediffGeneChecker.IsHediffBlockedByGenes(pawn, hediff_rhytides))
                {
                    pawn.health.AddHediff(hediff_rhytides);
                }

            }
            else
            {
                Hediff active_rhytides = pawn.health.hediffSet.GetFirstHediffOfDef(hediff_rhytides);

                if (active_rhytides != null)
                {
                    pawn.health.RemoveHediff(active_rhytides);
                }
            }
        }
    }



    
}
