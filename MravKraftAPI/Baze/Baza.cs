using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Baze
{
    using Mravi;
    using Igraci;
    using Map;

    public sealed class Baza
    {
        private static Texture2D _front, _back;
        private static Vector2 _origin;
        private static float _defaultScale;
        private static Color _defaultColor;
        private static uint _defaultResources;
        private static byte[] _mravCost;
        private static Random _randomizer;

        internal static List<Baza> Baze;

        internal static void Load(ContentManager content, Color backColor, uint startingResources = 50, float scale = 0.18f)
        {
            _front = content.Load<Texture2D>(@"Images\Baza\bazaFront");
            _back = content.Load<Texture2D>(@"Images\Baza\bazaBack");

            _origin = new Vector2(_front.Width / 2, _front.Height / 2);
            _defaultColor = backColor;
            _defaultScale = scale;
            _defaultResources = startingResources;

            _mravCost = new byte[4];
            _mravCost[(byte)MravType.Radnik] = Radnik.Cost;
            _mravCost[(byte)MravType.Scout] = Scout.Cost;
            _mravCost[(byte)MravType.Vojnik] = Vojnik.Cost;
            _mravCost[(byte)MravType.Leteci] = Leteci.Cost;

            _randomizer = new Random();
            Baze = new List<Baza>(2);
        }

        private readonly Vector2 _position;
        private readonly Queue<MravProcess> _productionQueue;
        private readonly Color _color;

        public byte Owner { get; private set; }
        public uint Resources { get; private set; }
        public Vector2 Position { get { return _position; } }

        internal Baza(Vector2 position, Player owner)
        {
            Owner = owner.ID;
            Resources = _defaultResources;

            _color = owner.Color;
            _position = position;
            _productionQueue = new Queue<MravProcess>();
        }

        internal void Update()
        {
            SetVisible(4);
            Production();
        }

        internal void GiveResource()
        {
            Resources++;
        }

        private void SetVisible(byte radius)
        {
            PXY center = Patch.GetPXYAt(_position).Value;
            int leftX, rightX, leftY, rightY;

            leftX = (center.X - radius > 0) ? center.X - radius : 0;
            leftY = (center.Y - radius > 0) ? center.Y - radius : 0;

            rightX = (center.X + radius < Patch.Height) ? center.X + radius : Patch.Height - 1;
            rightY = (center.Y + radius < Patch.Width) ? center.Y + radius : Patch.Width - 1;

            for (int i = leftX; i <= rightX; i++)
                for (int j = leftY; j <= rightY; j++)
                    Patch.Map[i, j].SetVisible(Owner);
        }

        private void Production()
        {
            if (_productionQueue.Count > 0)
            {
                MravProcess next = _productionQueue.Peek();

                if (next.DurationLeft == 0)
                {
                    switch (next.MravTip)
                    {
                        case MravType.Radnik:
                            Mrav.AddNew(Owner, new Radnik(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Scout:
                            Mrav.AddNew(Owner, new Scout(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Vojnik:
                            Mrav.AddNew(Owner, new Vojnik(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Leteci:
                            Mrav.AddNew(Owner, new Leteci(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                    }

                    if (--next.CountLeft == 0) _productionQueue.Dequeue();
                    else next.ResetDuration();
                }
                else next.DurationLeft--;
            }
        }

        public bool ProduceUnit(MravType type, byte count)
        {
            if (Resources - _mravCost[(byte)type] * count >= 0)
            {
                Resources -= (uint)(_mravCost[(byte)type] * count);
                _productionQueue.Enqueue(new MravProcess(type, count));
                return true;
            }

            return false;
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_back, _position, null, _defaultColor, 0f, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_front, _position, null, _color, 0f, _origin, _defaultScale, SpriteEffects.None, 0f); ;
        }

    }
}
