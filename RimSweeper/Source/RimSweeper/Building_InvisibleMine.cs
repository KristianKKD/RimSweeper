using RimWorld;
using Verse;
using System.Collections.Generic;

namespace RimSweeper
{
    public class Building_InvisibleMine : Building
    {
        protected override void Tick()
        {
            base.Tick();
            // Check every 10 ticks for performance
            if (this.IsHashIntervalTick(10))
            {
                List<Thing> thingList = this.Position.GetThingList(this.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null)
                    {
                        this.Explode(pawn);
                        break;
                    }
                }
            }
        }

        private void Explode(Pawn triggerer)
        {
            // Standard explosion parameters
            GenExplosion.DoExplosion(

                center: this.Position,
                map: this.Map,
                radius: 5f, 
                damType: DamageDefOf.Bomb,
                instigator: this,
                damAmount: -15,
                armorPenetration: -10f,
                explosionSound: null,
                weapon: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 1,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 1
            );
            
            if (triggerer != null && !triggerer.Dead)
            {
                Messages.Message(triggerer.LabelShort + " stepped on a hidden mine!", triggerer, MessageTypeDefOf.NegativeEvent);
            }

            //this.Destroy(DestroyMode.Vanish);
        }
    }
}
