using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;

using MravKraftAPI;
using MravKraftAPI.Igraci;
using MravKraftAPI.Mravi;
using MravKraftAPI.Baze;
using MravKraftAPI.Map;

namespace MravKraft.Botovi
{
    public class PenguinPero : Player
    {
        private readonly Random _radomizer;
        private const float PI = (float)Math.PI;

        private int countVojnik, countLeteci, countScout, countRadnik;
        private Baza enemyBase;
        private int unitTotal = 0;
        private bool leteciStart;

        private Baza myBase;
        private SortedDictionary<float, Patch> resourcePatches = new SortedDictionary<float, Patch>();
        private Dictionary<Patch, byte> workersForPatch = new Dictionary<Patch, byte>();
        private HashSet<Radnik> readyToWork = new HashSet<Radnik>();

        private float[] startingRotations = new float[] { PI / 3, PI * 2 / 3, PI, PI * 4 / 3, PI * 5 / 3, PI * 2 };
        private int startingSpawns = 6;
        private const float PATCH_DIST_FIX = 0.00001f;

        private bool scoutProducing = false;
        private int numScouts = 0;
        private int scoutDir;

        private bool rushEnemy = false;

        public PenguinPero(Color color) : base(color)
        {
            _radomizer = new Random();
        }

        private void AttackClosest(Mrav mrav)
        {
            if (mrav.VisibleEnemies.Count > 0)
            {
                Mrav closest = mrav.VisibleEnemies.MinBy(m => mrav.DistanceTo(m.Position));

                mrav.Attack(closest);

                if (!mrav.MovedOrAttacked)
                    mrav.MoveForward();
            }
        }

        private void RandomMovement(Mrav mrav)
        {
            mrav.SetRotation(mrav.Rotation - 0.02f + (float)_radomizer.NextDouble() * 0.04f);
            mrav.MoveForward();

            if (!mrav.MovedOrAttacked) mrav.SetRotation(mrav.Rotation + PI);
        }

        private void Update(Vojnik vojnik)
        {
            
        }

        private void Update(Leteci leteci)
        {

        }

        private void Update(Scout scout)
        {
            if (scout.JustSpawned)
            {
                scoutProducing = false;

                if (numScouts <= 1)
                {
                    scout["explorer"] = numScouts;
                    scout["direction"] = (numScouts == 0) ? 1 : -1;
                    scout.SetRotation((numScouts) * PI + PI / 2);
                    numScouts++;
                }
            }

            if (scout.HasProp("explorer"))
            {
                int direction = (int)scout["direction"];
                scout.MoveForward();

                if (direction == 0)
                {
                    scout["distance"] = (float)scout["distance"] - scout.Speed;

                    if ((float)scout["distance"] <= 0)
                    {
                        scout["direction"] = -1 * (int)scout["lastDir"];

                        if ((int)scout["direction"] == 1) scout.SetRotation(PI / 2f);
                        else scout.SetRotation(3 * PI / 2f);
                    }
                }

                if (!scout.MovedOrAttacked)
                {
                    if (direction == 0)
                    {
                        scout["explorer"] = 1 - (int)scout["explorer"];
                        scout.SetRotation(scout.Rotation + PI);
                        scoutDir = 180 - scoutDir;
                    }
                    else
                    {
                        if ((int)scout["explorer"] == 0) scout.SetRotation(scoutDir);
                        else scout.SetRotation(180 - scoutDir);


                        scout["lastDir"] = direction;
                        scout["direction"] = 0;
                        scout["distance"] = Patch.Size * 6f;
                    }
                }
            }
            else
            {
                RandomMovement(scout);

                foreach (Patch patch in scout.VisiblePatches.Where(p => p.Resources > 0)) // pronalazenje patcheva (slanje bazi)
                    AddResourcePatch(patch);

                if (enemyBase == null)
                    enemyBase = scout.EnemyBase();
            }
        }

        private void Update(Radnik radnik)
        {
            if (radnik.JustSpawned)
            {
                // cover all 6 directions
                if (startingSpawns > 0) radnik.SetRotation(startingRotations[--startingSpawns]);
                else // postavi da je spreman ako se tek spawna
                {
                    readyToWork.Add(radnik);
                    return;
                }
            }

            if (radnik.CarryingFood) // vrati spizu doma
            {
                if (radnik.DropResource())
                    radnik.SetRotation(radnik.Rotation + PI);

                return;
            }

            if (radnik.HasProp("targetPatch")) // ima li patch target (triba bi uvik?)
            {
                Patch target = (Patch)radnik["targetPatch"];

                radnik.Face(target);

                if (radnik.DistanceTo(target.Center) <= 12f)
                {
                    if (radnik.GrabResource(target)) radnik.SetRotation(radnik.Rotation + PI); // grab resource and turn around
                    else if (target.Resources == 0) readyToWork.Add(radnik); // if wasted say you're ready
                }
                else radnik.MoveForward();
            }
            else // random kretanje (istrazuje)
            {
                radnik.SetRotation(radnik.Rotation - 0.02f + (float)_radomizer.NextDouble() * 0.04f);

                radnik.MoveForward();
                if (!radnik.MovedOrAttacked) radnik.SetRotation(radnik.Rotation + PI);

                // pronalazenje patcheva (slanje bazi)
                foreach (Patch patch in radnik.VisiblePatches.Where(p => p.Resources > 0))
                    AddResourcePatch(patch);

                readyToWork.Add(radnik); // oznaci ga ka spremnog da kupi
            }
        }

        private void UpdateAll(List<Mrav> mravi)
        {
            foreach (Mrav mrav in mravi)
            {
                switch (mrav.Type)
                {
                    case MravType.Radnik:
                        countRadnik++;
                        break;
                    case MravType.Scout:
                        countScout++;
                        break;
                    case MravType.Vojnik:
                        countVojnik++;
                        break;
                    case MravType.Leteci:
                        countLeteci++;
                        break;
                }
            }

            foreach (Mrav mrav in mravi)
            {
                switch (mrav.Type)
                {
                    case MravType.Radnik:
                        Update(mrav as Radnik);
                        break;
                    case MravType.Scout:
                        Update(mrav as Scout);
                        break;
                    case MravType.Vojnik:
                        Update(mrav as Vojnik);
                        break;
                    case MravType.Leteci:
                        Update(mrav as Leteci);
                        break;
                }
            }
        }

        public override void Update(List<Mrav> mravi, Baza glavnaBaza)
        {
            if (myBase == null) // turn one
            {
                myBase = glavnaBaza;
                scoutDir = (ID == 0) ? 0 : 180;
            }

            countLeteci = countVojnik = countRadnik = countScout = 0;

            UpdateAll(mravi);
            HandleResourcePatches();

            if (numScouts <= 1 || unitTotal % 8 == 0 && !scoutProducing)
            {
                if (glavnaBaza.ProduceUnit(MravType.Scout, 1))
                {
                    unitTotal++;
                    scoutProducing = true;
                }

                return;
            }

            if (countRadnik < 35 || unitTotal % 5 == 0) { if (glavnaBaza.ProduceUnit(MravType.Radnik, 1)) unitTotal++; }
            else if (glavnaBaza.ProduceUnit(MravType.Leteci, 1)) unitTotal++;
        }

        private void AddResourcePatch(Patch patch)
        {
            float dist = myBase.DistanceTo(patch.Center);

            while (resourcePatches.ContainsKey(dist) && resourcePatches[dist] != patch)
                dist += 0.0001f;

            resourcePatches[dist] = patch;

            if (!workersForPatch.ContainsKey(patch)) workersForPatch[patch] = 0;
        }

        private void HandleResourcePatches()
        {
            if (resourcePatches.Count > 0)
            {
                var patch = resourcePatches.Where(p => p.Value.Resources == 0).FirstOrDefault();

                try
                {
                    resourcePatches.Remove(patch.Key);
                    workersForPatch.Remove(patch.Value);
                }
                catch { }
            }

            // postavi radnicima koji su "Ready to work" metu
            if (readyToWork.Count > 0)
            {
                foreach (Patch patch in resourcePatches.Values)
                {
                    while (workersForPatch[patch] < 16)
                    {
                        Radnik current = readyToWork.First();
                        readyToWork.Remove(current);

                        current["targetPatch"] = patch;
                        workersForPatch[patch]++;

                        if (readyToWork.Count == 0) break;
                    }

                    if (readyToWork.Count == 0) break;
                }
            }
        }

    }

}
