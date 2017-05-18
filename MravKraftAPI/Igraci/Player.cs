using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MravKraftAPI.Igraci
{
    using Mravi;
    using Baze;

    /// <summary> Base class for player bots </summary>
    public abstract class Player
    {
        /// <summary> Player ID </summary>
        public byte ID { get; internal set; }

        /// <summary> Team color </summary>
        public Color Color { get; private set; }

        /// <summary> Base constructor for player </summary>
        /// <param name="color"> Team color </param>
        public Player(Color color)
        {
            Color = color;
        }

        /// <summary> Update method for bots </summary>
        /// <param name="ants"> List of alive owned ants </param>
        /// <param name="mainBase"> Main base </param>
        public abstract void Update(List<Mrav> ants, Baza mainBase);

    }
}
