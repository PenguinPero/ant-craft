using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MravKraftAPI.Mravi
{
    public sealed class Scout : Mrav
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

        internal static void Load(Color headColor, byte cost = 75, byte duration = 38, byte vision = 3,
                                  byte damage = 3, byte health = 25, byte armor = 0, byte armorPen = 2, float scale = 0.06f, float speed = 1.25f)
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

        internal Scout(Vector2 position, Color color, byte owner, float rotation)
            : base(position, color, owner, rotation, MravType.Scout)
        {
            health = _defaultHealth;
            Armor = _defaultArmor;
            ArmorPen = _defaultArmorPen;
            Damage = _defaultDamage;
            Vision = _defaultVision;
            Speed = _defaultSpeed;
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
