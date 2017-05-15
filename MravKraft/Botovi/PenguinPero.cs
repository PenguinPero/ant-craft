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
        private int vojnikSpawn = 3;
        private readonly Random _radomizer;
        private const float PI = (float)Math.PI;
        private Baza enemyBase, myBase;
        private Mrav enemyAntTest;

        public PenguinPero(Color color) : base(color)
        {
            _radomizer = new Random();
        }

        public override void Update(Vojnik vojnik)
        {
            if (enemyBase == null)
            {
                enemyBase = vojnik.EnemyBase();

                if (myBase != null)
                {
                    vojnik.Face(Patch.Map[Patch.Height - myBase.PatchHere.X,
                                          Patch.Width - myBase.PatchHere.Y]
                                          .Center);
                }
            }

            if (vojnik.VisibleEnemies.Count > 0)
            {
                Mrav closest = vojnik.VisibleEnemies.MinBy(m => m.DistanceTo(vojnik.Position));
                vojnik.Attack(closest);

                if (!vojnik.TurnAttack)
                    vojnik.MoveForward();

                vojnik.Attack(closest);

                return;
            }

            if (enemyBase != null)
            {
                vojnik.Attack(enemyBase);

                if (!vojnik.TurnAttack)
                    vojnik.MoveForward();

                vojnik.Attack(enemyBase);

                return;
            }

            vojnik.MoveForward();

            if (!vojnik.TurnMovement)
                vojnik.SetRotation(vojnik.Rotation + PI);
        }

        public override void Update(Leteci leteci)
        {
            if (enemyBase == null)
            {
                enemyBase = leteci.EnemyBase();

                if (myBase != null)
                {
                    leteci.Face(Patch.Map[Patch.Height - myBase.PatchHere.X,
                                          Patch.Width - myBase.PatchHere.Y]
                                          .Center);
                }
            }

            if (leteci.VisibleEnemies.Count > 0)
            {
                Mrav closest = leteci.VisibleEnemies.MinBy(m => m.DistanceTo(leteci.Position));
                leteci.Attack(closest);

                if (enemyAntTest == null)
                    enemyAntTest = closest;

                if (!leteci.TurnAttack)
                    leteci.MoveForward();

                leteci.Attack(closest);

                return;
            }

            if (enemyBase != null)
            {
                leteci.Attack(enemyBase);

                if (!leteci.TurnAttack)
                    leteci.MoveForward();

                leteci.Attack(enemyBase);
                return;
            }

            leteci.MoveForward();

            if (!leteci.TurnMovement)
                leteci.SetRotation(leteci.Rotation + PI);
        }

        public override void Update(Scout scout)
        {
            scout.MoveForward();

            if (!scout.TurnMovement)
                scout.SetRotation(scout.Rotation + PI);
        }

        public override void Update(Radnik radnik)
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

                radnik.MoveForward();
                if (!radnik.TurnMovement) radnik.SetRotation(radnik.Rotation + PI);

                if (closestWithResources == null) radnik.SetRotation(radnik.Rotation - 0.02f + (float)_radomizer.NextDouble() * 0.04f);
                else radnik.GrabResource(closestWithResources);
            }
        }

        public override void Update(Baza glavnaBaza)
        {
            if (myBase == null) myBase = glavnaBaza;

            if (vojnikSpawn == 0)
            {
                if (glavnaBaza.ProduceUnit(MravType.Vojnik, 1))
                    vojnikSpawn = 3;
            }
            else if (glavnaBaza.ProduceUnit(MravType.Radnik, 1))
            {
                vojnikSpawn--;
            }
        }

    }

}
