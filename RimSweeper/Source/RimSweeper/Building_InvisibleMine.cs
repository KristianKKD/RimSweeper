using RimWorld;
using Verse;
using System.Collections.Generic;

namespace RimSweeper
{
    public class Building_InvisibleMine : Building
    {
        private int disabledUntilTick = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disabledUntilTick, "disabledUntilTick", -1);
        }

        protected override void Tick()
        {
            base.Tick();
            // Check every 10 ticks for performance
            if (this.IsHashIntervalTick(10))
            {
                List<Thing> thingList = this.Position.GetThingList(this.Map);
                Pawn triggerer = null;
                bool corpseFound = false;

                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing t = thingList[i];
                    if (t is Corpse)
                    {
                        corpseFound = true;
                    }
                    if (t is Pawn p)
                    {
                        triggerer = p;
                    }
                }

                if (corpseFound)
                {
                    // Disable for 1 hour (2500 ticks) after corpse is removed
                    // While corpse is present, keep pushing the timer forward
                    disabledUntilTick = Find.TickManager.TicksGame + 2500;
                }

                bool isDisabled = Find.TickManager.TicksGame < disabledUntilTick;

                if (!isDisabled && triggerer != null)
                {
                    this.Explode(triggerer);
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
