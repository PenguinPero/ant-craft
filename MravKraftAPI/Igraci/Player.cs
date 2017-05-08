using Microsoft.Xna.Framework;

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

        /// <summary> Poziva update metodu za mrava </summary>
        /// <param name="mrav"> Mrav koji se updatea </param>
        internal void Update(Mrav mrav)
        {
            switch (mrav.Type)
            {
                case MravType.Radnik:
                    Update((Radnik)mrav);
                    break;
                case MravType.Scout:
                    Update((Scout)mrav);
                    break;
                case MravType.Vojnik:
                    Update((Vojnik)mrav);
                    break;
                case MravType.Leteci:
                    Update((Leteci)mrav);
                    break;
            }
        }

        /// <summary> Update metoda za bazu </summary>
        /// <param name="glavnaBaza"> Glavna baza od igraca</param>
        public abstract void Update(Baza glavnaBaza);

        /// <summary> Update metoda za radnike </summary>
        /// <param name="radnik"> Trenutni radnik </param>
        public abstract void Update(Radnik radnik);

        /// <summary> Update metoda za scoute </summary>
        /// <param name="scout"> Trenutni scout </param>
        public abstract void Update(Scout scout);

        /// <summary> Update metoda za vojnike </summary>
        /// <param name="vojnik"> Trenutni vojnik </param>
        public abstract void Update(Vojnik vojnik);

        /// <summary> Update metoda za letece mrave </summary>
        /// <param name="leteci"> Trenutni leteci mrav </param>
        public abstract void Update(Leteci leteci);

    }
}
