using RimWorld;
using Verse;

namespace RimSweeper
{
    public class GenStep_ScatterInvisibleMines : GenStep_Scatterer
    {
        public override int SeedPart => 349210432;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            
            // Don't spawn on existing buildings
            if (c.GetEdifice(map) != null)
            {
                return false;
            }

            // Check terrain
            TerrainDef terrain = c.GetTerrain(map);
            if (terrain.IsWater || terrain.IsRoad)
            {
                return false;
            }
            
            // Don't spawn under roofs (optional, but mines usually outside?)
            // if (c.Roofed(map)) return false;

            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            ThingDef mineDef = ThingDef.Named("RS_InvisibleMine");
            if (mineDef != null)
            {
                GenSpawn.Spawn(mineDef, loc, map);
            }
            else
            {
                Log.Error("RimSweeper: Could not find ThingDef named RS_InvisibleMine");
            }
        }
    }
}
