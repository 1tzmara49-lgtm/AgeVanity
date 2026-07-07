using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace agingReimagined
{
    public class agingSettings : ModSettings
    {
        public int rhytidesThreshold = 50;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rhytidesThreshold, "rhytidesThreshold", 50);
        }
    }
}
