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

                mrav.Attack(closest);
            }
        }

        private void Update(Vojnik vojnik)
        {
            AttackClosest(vojnik);
        }

        private void Update(Leteci leteci)
        {
            AttackClosest(leteci);
        }

        private void Update(Scout scout)
        {
            AttackClosest(scout);
        }

        private void Update(Radnik radnik)
        {
            if (radnik.CarryingFood)
            {
                radnik.MoveForward();
                radnik.DropResource();

                if (!radnik.CarryingFood)
                    radnik.SetRotation(radnik.Rotation + PI);
            }
            else
            {
                Patch closestWithResources = radnik.VisiblePatches
                                                   .Where(p => p.Resources > 0)
                                                   .MinBy(p => radnik.DistanceTo(p.Center));

                if (closestWithResources == null) radnik.SetRotation(radnik.Rotation - 0.02f + (float)_radomizer.NextDouble() * 0.04f);
                else radnik.GrabResource(closestWithResources);

                radnik.MoveForward();
                if (!radnik.MovedOrAttacked) radnik.SetRotation(radnik.Rotation + PI);
            }
        }

        public override void Update(List<Mrav> ants, Baza mainBase)
        {
            countLeteci = 0;
            countVojnik = 0;
            countRadnik = 0;
            countScout = 0;

            foreach (Mrav mrav in ants)
            {
                switch (mrav.Type)
                {
                    case MravType.Radnik:
                        Update(mrav as Radnik);
                        countRadnik++;
                        break;
                    case MravType.Scout:
                        Update(mrav as Scout);
                        countScout++;
                        break;
                    case MravType.Vojnik:
                        Update(mrav as Vojnik);
                        countVojnik++;
                        break;
                    case MravType.Leteci:
                        Update(mrav as Leteci);
                        countLeteci++;
                        break;
                }
            }

            if (countRadnik < 50)
                mainBase.ProduceUnit(MravType.Radnik, 1);
            else mainBase.ProduceUnit(MravType.Leteci, 1);
        }

    }

}
