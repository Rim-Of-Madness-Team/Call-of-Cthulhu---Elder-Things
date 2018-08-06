using System;
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
        public bool ShowNow = false;

        private Graphic graphicInt;

        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)this.props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                Graphic badGraphic;
                if (this.graphicInt == null)
                {
                    if (this.Props.graphicData == null)
                    {
                        Log.ErrorOnce(this.parent.def + " has no SecondLayer graphicData but we are trying to access it.", 764532);
                        badGraphic = BaseContent.BadGraphic;
                        return badGraphic;
                    }
                    this.graphicInt = this.Props.graphicData.GraphicColoredFor(this.parent);
                }
                badGraphic = this.graphicInt;
                return badGraphic;
            }
        }


        

        public override void PostDraw()
        {
            base.PostDraw();
            if (ShowNow)
                this.Graphic.Draw(Gen.TrueCenter(this.parent.Position, this.parent.Rotation, this.parent.def.size, this.Props.Altitude), this.parent.Rotation, this.parent);
        }
    }
}
