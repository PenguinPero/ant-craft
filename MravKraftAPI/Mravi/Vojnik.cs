using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Mravi
{
    using Map;

    public sealed class Vojnik : Mrav
    {
        private static Color _headColor;
        private static float _defaultScale, _defaultSpeed;
        private static byte _defaultHealth;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }
        public static byte Upkeep { get; private set; }
        public static byte Vision { get; private set; }
        public static byte Damage { get; private set; }

        internal static void Load(Color headColor, byte cost = 12, byte duration = 10, byte upkeep = 0,
                                byte vision = 3, byte damage = 5, byte health = 100, float scale = 0.06f, float speed = 1.2f)
        {
            _headColor = headColor;
            _defaultScale = scale;
            _defaultSpeed = speed;
            _defaultHealth = health;

            Cost = cost;
            Duration = duration;
            Upkeep = upkeep;
            Vision = vision;
            Damage = damage;
        }

        internal Vojnik(Vector2 position, Color color, byte owner, float rotation) : base(position, color, owner, rotation, MravType.Vojnik)
        {
            Health = _defaultHealth;
        }

        public override void MoveForward()
        {
            Move(_defaultSpeed);
        }

        internal override IEnumerable<Patch> Visibility()
        {
            return Visibility(Vision);
        }

        public override void Attack(Mrav mrav)
        {
            Attack(mrav, Damage);
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
