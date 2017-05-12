using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MravKraftAPI.Mravi
{
    using Map;
    using Baze;

    public sealed class Radnik : Mrav
    {
        private static Color _headColor;
        private static float _defaultScale, _defaultSpeed;
        private static byte _defaultHealth;
        private static byte _defaultVision;
        private static byte _defaultDamage;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }

        internal static void Load(Color headColor, byte cost = 5, byte duration = 10, byte vision = 2,
                                  byte damage = 0, byte health = 100, float scale = 0.06f, float speed = 1.4f)
        {
            _headColor = headColor;
            _defaultScale = scale;
            _defaultSpeed = speed;
            _defaultHealth = health;

            Cost = cost;
            Duration = duration;
            _defaultVision = vision;
            _defaultDamage = damage;
        }

        public bool CarryingFood { get; private set; }

        internal Radnik(Vector2 position, Color color, byte owner, float rotation)
            : base(position, color, owner, rotation, MravType.Radnik)
        {
            Health = _defaultHealth;
            Damage = _defaultDamage;
            Vision = _defaultVision;
            Speed = _defaultSpeed;
        }

        public void GrabResource(Patch patch)
        {
            if (patch.Resources == 0 || PlayerTurn != Owner || !Alive) return;

            Face(patch.Center);

            if (patch.TakeResource(position))
                CarryingFood = true;
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
