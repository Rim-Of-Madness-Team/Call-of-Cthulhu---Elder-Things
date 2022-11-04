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

        public virtual Graphic Graphic
        {
            get
            {
                Graphic badGraphic;
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce(parent.def + " has no SecondLayer graphicData but we are trying to access it.",
                            764532);
                        badGraphic = BaseContent.BadGraphic;
                        return badGraphic;
                    }

                    graphicInt = Props.graphicData.GraphicColoredFor(parent);
                }

                badGraphic = graphicInt;
                return badGraphic;
            }
        }


        public override void PostDraw()
        {
            base.PostDraw();
            if (ShowNow)
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude),
                    parent.Rotation, parent);
            }
        }
    }
}