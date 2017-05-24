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
        private HashSet<Patch> resPatches = new HashSet<Patch>();
        private Patch closestResPatch;
        private Baza enemyBase;
        private int unitTotal = 0;
        private bool leteciStart;

        public PenguinPero(Color color) : base(color)
        {
            _radomizer = new Random();
        }

        private void AttackClosest(Mrav mrav)
        {
            if (mrav.VisibleEnemies.Count > 0)
            {
                Mrav closest = mrav.VisibleEnemies.Where(m => m.Health > 0)
                                                  .MinBy(m => mrav.DistanceTo(m.Position));

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
            if (leteciStart)
            {
                leteciStart = true;
                AttackClosest(leteci);

                if (!leteci.MovedOrAttacked && enemyBase != null)
                {
                    //leteci.Attack(enemyBase);

                    if (!leteci.MovedOrAttacked)
                        leteci.MoveForward();
                }
                //else RandomMovement(leteci);

                //if (enemyBase == null)
                //    enemyBase = leteci.EnemyBase();
            }
        }

        private void Update(Scout scout)
        {
            RandomMovement(scout);

            foreach (Patch patch in scout.VisiblePatches) // pronalazenje patcheva (slanje bazi)
            {
                if (resPatches.Contains(patch))
                {
                    if (patch.Resources == 0)
                        resPatches.Remove(patch);
                }
                else if (patch.Resources > 0)
                    resPatches.Add(patch);
            }

            if (enemyBase == null)
                enemyBase = scout.EnemyBase();
        }

        private void Update(Radnik radnik)
        {
            // test custom propertija
            if (radnik.JustSpawned) radnik["intentions"] = new Queue<int>();
            else
            {
                Queue<int> radnikIntentions = (Queue<int>)radnik["intentions"];
                radnikIntentions.Enqueue(countRadnik);
                radnikIntentions.Dequeue();
            }

            foreach (Patch patch in radnik.VisiblePatches) // pronalazenje patcheva (slanje bazi)
            {
                if (resPatches.Contains(patch)) // update istrosenih
                {
                    if (patch.Resources == 0)
                        resPatches.Remove(patch);
                }
                else if (patch.Resources > 0)
                    resPatches.Add(patch);
            }

            if (radnik.CarryingFood) // vrati spizu doma
            {
                radnik.MoveForward();
                radnik.DropResource();

                if (!radnik.CarryingFood)
                    radnik.SetRotation(radnik.Rotation + PI);
            }
            else
            {
                if (closestResPatch == null) // ako baza nezna nijedan patch
                    radnik.SetRotation(radnik.Rotation - 0.02f + (float)_radomizer.NextDouble() * 0.04f);
                else // kupi najblizi poznati resurs
                {
                    if (radnik.DistanceTo(closestResPatch.Center) > 12f)
                        radnik.Face(closestResPatch);
                    else
                    {
                        radnik.GrabResource(closestResPatch);
                        return;
                    }
                }

                radnik.MoveForward();
                if (!radnik.MovedOrAttacked) radnik.SetRotation(radnik.Rotation + PI);
            }
        }

        private List<string> spawns = new List<string>();

        private void UpdateAll(List<Mrav> mravi)
        {
            foreach (Mrav mrav in mravi)
            {
                if (mrav.JustSpawned) spawns.Add($"[{mrav.ID}] Rotation: {mrav.Rotation}");

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
            if (enemyBase != null) ;

            countLeteci = countVojnik = countRadnik = countScout = 0;

            UpdateAll(mravi);

            if (this.ID == 0) ;

            closestResPatch = resPatches.MinBy(p => glavnaBaza.DistanceTo(p.Center));

            if (unitTotal % 6 == 0)
            {
                if (glavnaBaza.ProduceUnit(MravType.Scout, 1)) unitTotal++;
                return;
            }

            if (countRadnik < 35 || unitTotal % 5 == 0) { if (glavnaBaza.ProduceUnit(MravType.Radnik, 1)) unitTotal++; }
            else if (glavnaBaza.ProduceUnit(MravType.Leteci, 1)) unitTotal++;
        }

    }

}
