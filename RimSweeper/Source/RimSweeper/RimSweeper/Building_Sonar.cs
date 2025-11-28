using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace RimSweeper
{
    public class Building_Sonar : Building
    {
        private int cachedDistance = -999;
        private Graphic cachedGraphic;

        public override Graphic Graphic
        {
            get
            {
                if (cachedGraphic != null)
                {
                    return cachedGraphic;
                }
                return base.Graphic;
            }
        }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            RecalculateDistance();
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(60))
            {
                RecalculateDistance();
            }
        }

        private void RecalculateDistance()
        {
            if (!this.Spawned) return;

            ThingDef mineDef = ThingDef.Named("RS_InvisibleMine");
            List<Thing> mines = this.Map.listerThings.ThingsOfDef(mineDef);
            
            int newDistance = -1;

            if (mines.NullOrEmpty())
            {
                newDistance = -1;
            }
            else
            {
                float closestDist = float.MaxValue;
                
                foreach (var mine in mines)
                {
                    float dist = this.Position.DistanceTo(mine.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                    }
                }
                newDistance = Mathf.RoundToInt(closestDist);
            }
            
            if (newDistance != cachedDistance)
            {
                cachedDistance = newDistance;
                UpdateGraphic();
            }
        }

        private void UpdateGraphic()
        {
            string texPath = "rimgor_9"; // Default to 9 (safe/far)
            
            if (cachedDistance > 0)
            {
                switch (cachedDistance)
                {
                    case 1: texPath = "rimgor_1"; break;
                    case 2: texPath = "rimgor_2"; break;
                    case 3: texPath = "rimgor_3"; break;
                    case 4: texPath = "rimgor_4"; break;
                    case 5: texPath = "rimgor_5"; break;
                    case 6: texPath = "rimgor_6"; break;
                    case 7: texPath = "rimgor_7"; break;
                    case 8: texPath = "rimgor_8"; break;
                    default: texPath = "rimgor_9"; break; // 9 or more
                }
            }
            
            cachedGraphic = GraphicDatabase.Get<Graphic_Single>(texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
            
            // Refresh the map drawer to update the visual
            if (this.Spawned)
            {
                this.Map.mapDrawer.MapMeshDirty(this.Position, MapMeshFlagDefOf.Things);
            }
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            
            if (cachedDistance > 0 && cachedDistance <= 5)
            {
                Vector3 worldPos = this.DrawPos;
                worldPos.z += 0.5f; // Offset slightly up
                Vector2 screenPos = Find.Camera.WorldToScreenPoint(worldPos);
                screenPos.y = UI.screenHeight - screenPos.y; // Invert Y for GUI
                
                Rect rect = new Rect(screenPos.x - 15f, screenPos.y - 15f, 30f, 30f);
                
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                
                // Draw shadow for visibility
                GUI.color = Color.black;
                Rect shadowRect = new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height);
                Widgets.Label(shadowRect, cachedDistance.ToString());
                
                GUI.color = Color.white; // Or maybe a specific color based on distance?
                // Let's use Green for far, Red for close? Or just White.
                // Minesweeper: 1 is Blue, 2 is Green, 3 is Red...
                // Let's stick to White for now.
                Widgets.Label(rect, cachedDistance.ToString());
                
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }
    }
}
