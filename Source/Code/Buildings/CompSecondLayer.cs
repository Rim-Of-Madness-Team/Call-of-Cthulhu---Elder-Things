using RimWorld;
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
    internal class CompSecondLayer : ThingComp
    {
        private Graphic graphicInt;
        public bool ShowNow = false;

        public CompProperties_SecondLayer Props => (CompProperties_SecondLayer) props;


        public void DrawSecondLayer()
        {
            if (ShowNow)
            {

            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                Graphic badGraphic;
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce(text: parent.def + " has no SecondLayer graphicData but we are trying to access it.",
                            key: 764532);
                        badGraphic = BaseContent.BadGraphic;
                        return badGraphic;
                    }

                    graphicInt = Props.graphicData.GraphicColoredFor(t: parent);
                }

                badGraphic = graphicInt;
                return badGraphic;
            }
        }

    }
}