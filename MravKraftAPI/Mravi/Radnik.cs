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
        private static byte _defaultArmor;
        private static byte _defaultArmorPen;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }

        internal static void Load(Color headColor, byte cost = 50, byte duration = 30, byte vision = 2,
                                  byte damage = 5, byte health = 50, byte armor = 0, byte armorPen = 0, float scale = 0.06f, float speed = 1f)
        {
            _headColor = headColor;
            _defaultScale = scale;
            _defaultSpeed = speed;
            _defaultHealth = health;
            _defaultArmor = armor;
            _defaultArmorPen = armorPen;

            Cost = cost;
            Duration = duration;
            _defaultVision = vision;
            _defaultDamage = damage;
        }

        private bool carryingFood;

        public bool CarryingFood { get { return GetterCheck(carryingFood); } }

        internal Radnik(Vector2 position, Color color, byte owner, float rotation)
            : base(position, color, owner, rotation, MravType.Radnik)
        {
            health = _defaultHealth;
            Armor = _defaultArmor;
            ArmorPen = _defaultArmorPen;
            Damage = _defaultDamage;
            Vision = _defaultVision;
            Speed = _defaultSpeed;
        }

        public void GrabResource(Patch patch)
        {
            if (patch.Resources <= 0 || PlayerTurn != Owner || !alive) return;

            Face(patch);

            if (patch.TakeResource(position))
                carryingFood = true;
        }

        public void DropResource()
        {
            if (!carryingFood || PlayerTurn != Owner || !alive) return;

            Face(Baza.Baze[Owner]);

            if (DistanceTo(Baza.Baze[Owner].Position) <= 12f)
            {
                Baza.Baze[Owner].GiveResource();
                carryingFood = false;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            if (carryingFood)
                Resource.Draw(spriteBatch, position + direction * 12f);

            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
