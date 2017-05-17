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

        public void Update(Vojnik vojnik)
        {
            
        }

        public void Update(Leteci leteci)
        {

        }

        public void Update(Scout scout)
        {

        }

        public void Update(Radnik radnik)
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
                if (!radnik.TurnMovement) radnik.SetRotation(radnik.Rotation + PI);
            }
        }

        public override void Update(Baza glavnaBaza)
        {
            
        }

        public override void Update(List<Mrav> mravi)
        {
            countLeteci = 0;
            countVojnik = 0;
            countRadnik = 0;
            countScout = 0;

            foreach (Mrav mrav in mravi)
            {
                switch (mrav.Type)
                {
                    case MravType.Radnik:
                        Update(mrav as Radnik);
                        countRadnik++;
                        break;
                    case MravType.Scout:

                        break;
                    case MravType.Vojnik:

                        break;
                    case MravType.Leteci:

                        break;
                }

            }
        }
    }

}
