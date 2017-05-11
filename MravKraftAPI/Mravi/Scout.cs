using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Mravi
{
    using Map;
    using Baze;

    public sealed class Scout : Mrav
    {
        private static Color _headColor;
        private static float _defaultScale, _defaultSpeed;
        private static byte _defaultHealth;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }
        public static byte Vision { get; private set; }
        public static byte Damage { get; private set; }

        internal static void Load(Color headColor, byte cost = 0, byte duration = 5, byte vision = 4,
                                  byte damage = 0, byte health = 50, float scale = 0.06f, float speed = 0.6f)
        {
            _headColor = headColor;
            _defaultScale = scale;
            _defaultSpeed = speed;
            _defaultHealth = health;

            Cost = cost;
            Duration = duration;
            Vision = vision;
            Damage = damage;
        }

        internal Scout(Vector2 position, Color color, byte owner, float rotation) : base(position, color, owner, rotation, MravType.Scout)
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

        public override Baza EnemyBase()
        {
            return EnemyBase(Vision);
        }

        public override void Attack(Mrav mrav)
        {
            Attack(mrav, Damage);
        }

        public override void Attack(Baza baza)
        {
            Attack(baza, Damage);
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
