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
        private int vojnikSpawn;

        public PenguinPero(Color color) : base(color)
        { }

        public override void Update(Vojnik vojnik)
        {
            if (vojnik.VisibleEnemies.Count > 0)
            {
                Mrav closest = vojnik.VisibleEnemies[0];//;.MinBy(m => m.DistanceTo(vojnik.Position));
                vojnik.Attack(closest);

                if (!vojnik.TurnAttack)
                    vojnik.MoveForward();

                vojnik.Attack(closest);

                return;
            }

            vojnik.MoveForward();

            if (!vojnik.TurnMovement)
                vojnik.SetRotation(vojnik.Rotation + 180);
        }

        public override void Update(Leteci leteci)
        {
            leteci.MoveForward();

            if (!leteci.TurnMovement)
                leteci.SetRotation(leteci.Rotation + 180);
        }

        public override void Update(Scout scout)
        {
            scout.MoveForward();

            if (!scout.TurnMovement)
                scout.SetRotation(scout.Rotation + 180);
        }

        public override void Update(Radnik radnik)
        {
            if (radnik.CarryingFood)
            {
                radnik.DropResource();

                if (radnik.CarryingFood)
                {
                    do
                    {
                        radnik.MoveForward();

                        if (!radnik.TurnMovement) radnik.SetRotation(radnik.Rotation + 10);
                        else break;
                    } while (true);
                }
                else radnik.SetRotation(radnik.Rotation - (float)Math.PI);
            }
            else
            {
                Patch closest = radnik.VisiblePatches.Where(p => p.Resources > 0).MinBy(p => radnik.DistanceTo(p.Center));

                if (closest == null)
                {
                    radnik.MoveForward();

                    if (!radnik.TurnMovement) radnik.SetRotation(radnik.Rotation + (float)Math.PI);
                    else radnik.SetRotation(radnik.Rotation - 0.02f + new Random().Next(3) * 0.02f);
                }
                else
                {
                    radnik.GrabResource(closest);

                    do
                    {
                        radnik.MoveForward();

                        if (!radnik.TurnMovement) radnik.SetRotation(radnik.Rotation + 10);
                        else break;
                    } while (true);
                }
            }
        }

        public override void Update(Baza glavnaBaza)
        {
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
