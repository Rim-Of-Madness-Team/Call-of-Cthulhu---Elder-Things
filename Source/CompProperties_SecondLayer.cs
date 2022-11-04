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
        public AltitudeLayer altitudeLayer;
        public GraphicData graphicData;

        public CompProperties_SecondLayer()
        {
            compClass = typeof(CompSecondLayer);
        }

        public float Altitude => altitudeLayer.AltitudeFor();
    }
}