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
            AttackClosest(vojnik);
        }

        private void Update(Leteci leteci)
        {
            if (countLeteci >= 20 || leteciStart)
            {
                leteciStart = true;
                AttackClosest(leteci);

                if (!leteci.MovedOrAttacked)
                {
                    if (enemyBase != null)
                    {
                        leteci.Attack(enemyBase);

                        if (!leteci.MovedOrAttacked)
                            leteci.MoveForward();
                    }
                    else RandomMovement(leteci);
                }

                if (enemyBase == null)
                    enemyBase = leteci.EnemyBase();
            }
        }

        private void Update(Scout scout)
        {
            RandomMovement(scout);

            foreach (Patch patch in scout.VisiblePatches) // pronalazenje patcheva (slanje bazi)
                if (patch.Resources > 0) AddResourcePatch(patch);

            if (enemyBase == null)
                enemyBase = scout.EnemyBase();
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
                foreach (Patch patch in radnik.VisiblePatches)
                    if (patch.Resources > 0) AddResourcePatch(patch);

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
            if (myBase == null) myBase = glavnaBaza;

            countLeteci = countVojnik = countRadnik = countScout = 0;

            UpdateAll(mravi);
            HandleResourcePatches();

            if (unitTotal % 6 == 0)
            {
                if (glavnaBaza.ProduceUnit(MravType.Scout, 1)) unitTotal++;
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
            while (true)
            {
                var topPatch = resourcePatches.FirstOrDefault();

                if (resourcePatches.Count > 0 && topPatch.Value.Resources == 0)
                {
                    resourcePatches.Remove(topPatch.Key); // brisi sve one koji su potroseni u zadnjen updateu
                    workersForPatch.Remove(topPatch.Value);
                }
                else break;
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
