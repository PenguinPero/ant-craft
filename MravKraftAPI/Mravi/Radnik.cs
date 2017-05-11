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

    public sealed class Radnik : Mrav
    {
        private static Color _headColor;
        private static float _defaultScale, _defaultSpeed;
        private static byte _defaultHealth;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }
        public static byte Vision { get; private set; }
        public static byte Damage { get; private set; }

        internal static void Load(Color headColor, byte cost = 5, byte duration = 10, byte vision = 2,
                                  byte damage = 0, byte health = 100, float scale = 0.06f, float speed = 1.4f)
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

        public bool CarryingFood { get; private set; }

        internal Radnik(Vector2 position, Color color, byte owner, float rotation) : base(position, color, owner, rotation, MravType.Radnik)
        {
            Health = _defaultHealth;
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

        public override void MoveForward()
        {
            Move(_defaultSpeed);
        }

        public void GrabResource(Patch patch)
        {
            if (patch.Resources == 0 || PlayerTurn != Owner || !Alive) return;

            Face(patch.Center);

            if (patch.TakeResource(position)) CarryingFood = true;
            else Move(_defaultSpeed);
        }

        public void DropResource()
        {
            if (!CarryingFood || PlayerTurn != Owner || !Alive) return;

            FaceBase();

            if (DistanceTo(Baza.Baze[Owner].Position) <= 12f)
            {
                Baza.Baze[Owner].GiveResource();
                CarryingFood = false;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            if (CarryingFood)
                Resource.Draw(spriteBatch, position + direction * 12f);

            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, Rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
