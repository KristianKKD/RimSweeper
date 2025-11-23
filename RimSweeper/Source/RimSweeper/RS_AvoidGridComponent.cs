using Verse;
using Verse.AI;
using System.Collections.Generic;
using HarmonyLib;

namespace RimSweeper
{
    public class RS_AvoidGridComponent : MapComponent
    {
        private bool[] avoidGrid;
        private int mapSizeX;

        public RS_AvoidGridComponent(Map map) : base(map)
        {
            this.avoidGrid = new bool[map.cellIndices.NumGridCells];
            this.mapSizeX = map.Size.x;
        }

        public static RS_AvoidGridComponent GetComponent(Map map)
        {
            return map.GetComponent<RS_AvoidGridComponent>();
        }

        public void SetAvoid(IntVec3 cell, bool value)
        {
            if (cell.InBounds(map))
            {
                int index = map.cellIndices.CellToIndex(cell);
                avoidGrid[index] = value;
                
                // Use Harmony Traverse to access pathGrid if direct access fails (it shouldn't, but compiler is complaining)
                // map.pathGrid.RecalculatePerceivedPathCostAt(cell);
                var pathGrid = Traverse.Create(map).Field("pathGrid").GetValue<PathGrid>();
                if (pathGrid != null)
                {
                    bool dummy = false;
                    pathGrid.RecalculatePerceivedPathCostAt(cell, ref dummy);
                }
            }
        }

        public bool ShouldAvoid(int index)
        {
            if (index >= 0 && index < avoidGrid.Length)
            {
                return avoidGrid[index];
            }
            return false;
        }
        
        // Persistence is not strictly needed if we rebuild from spawned things on load,
        // but since we hook into SpawnSetup, it should rebuild automatically on load.
        // So we don't need to save the bool array directly.
    }
}
