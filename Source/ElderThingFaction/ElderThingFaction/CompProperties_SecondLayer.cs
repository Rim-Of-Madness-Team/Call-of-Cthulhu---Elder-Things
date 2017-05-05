﻿using System;
using Verse;

namespace ElderThingFaction
{
    /*
     *
     *  Vendan is the original creator of this class.
     *  Bless
     *  -Jecrell 
     * 
     */
    internal class CompProperties_SecondLayer : CompProperties
    {
        public GraphicData graphicData;

        public AltitudeLayer altitudeLayer;

        public float Altitude
        {
            get
            {
                return Altitudes.AltitudeFor(this.altitudeLayer);
            }
        }

        public CompProperties_SecondLayer()
        {
            this.compClass = typeof(CompSecondLayer);
        }
    }
}
