using Verse;

namespace RimSweeper
{
    public class Building_AvoidMarker : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            RS_AvoidGridComponent.GetComponent(map)?.SetAvoid(this.Position, true);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            RS_AvoidGridComponent.GetComponent(this.Map)?.SetAvoid(this.Position, false);
            base.DeSpawn(mode);
        }
    }
}
