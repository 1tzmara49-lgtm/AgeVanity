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
    [StaticConstructorOnStartup]
    public static class AgingReimagined
    {
        static AgingReimagined()
        {
            var harmony = new Harmony("1tzMara.age_vanity");

            harmony.PatchAll();
        }
    }
}
