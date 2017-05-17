using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MravKraftAPI.Igraci
{
    using Mravi;
    using Baze;

    public abstract class Player
    {
        /// <summary> ID igrača </summary>
        public byte ID { get; internal set; }

        /// <summary> Boja tima </summary>
        public Color Color { get; private set; }

        /// <summary> Bazni konstruktor igrača </summary>
        /// <param name="color"> Boja tima </param>
        public Player(Color color)
        {
            Color = color;
        }

        /// <summary> Update metoda za mrave </summary>
        /// <param name="mravi"> Lista svih zivih mravi </param>
        public abstract void Update(List<Mrav> mravi);

        /// <summary> Update metoda za bazu </summary>
        /// <param name="glavnaBaza"> Glavna baza od igraca </param>
        public abstract void Update(Baza glavnaBaza);

    }
}
