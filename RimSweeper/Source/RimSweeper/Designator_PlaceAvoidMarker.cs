using RimWorld;
using Verse;
using UnityEngine;

namespace RimSweeper
{
    public class Designator_PlaceAvoidMarker : Designator
    {
        public Designator_PlaceAvoidMarker()
        {
            this.defaultLabel = "Place Avoid Marker";
            this.defaultDesc = "Places a marker that colonists will try to avoid, but other pawns will ignore.";
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true); // Placeholder icon
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            ThingDef def = ThingDef.Named("RS_AvoidMarker");
            // Check if already exists
            if (c.GetFirstThing(base.Map, def) == null)
            {
                GenSpawn.Spawn(def, c, base.Map);
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
