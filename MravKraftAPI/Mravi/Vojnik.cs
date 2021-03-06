﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MravKraftAPI.Mravi
{
    public sealed class Vojnik : Mrav
    {
        private static Color _headColor;
        private static float _defaultScale, _defaultSpeed;
        private static byte _defaultHealth;
        private static byte _defaultVision;
        private static byte _defaultDamage;
        private static byte _defaultArmor;
        private static byte _defaultArmorPen;
        private static byte _defaultUpkeep;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }

        internal static void Load(Color headColor, byte cost = 100, byte duration = 60, byte vision = 2,
                                  byte damage = 12, byte health = 100, byte armor = 2, byte armorPen = 0,
                                  byte upkeep = 2, float scale = 0.06f, float speed = 0.75f)
        {
            _headColor = headColor;
            _defaultScale = scale;
            _defaultSpeed = speed;
            _defaultHealth = health;
            _defaultArmor = armor;
            _defaultArmorPen = armorPen;
            _defaultUpkeep = upkeep;

            Cost = cost;
            Duration = duration;
            _defaultVision = vision;
            _defaultDamage = damage;
        }

        internal Vojnik(Vector2 position, Color color, byte owner, float rotation)
            : base(position, color, owner, rotation, MravType.Vojnik)
        {
            health = _defaultHealth;
            Armor = _defaultArmor;
            ArmorPen = _defaultArmorPen;
            Damage = _defaultDamage;
            Vision = _defaultVision;
            Speed = _defaultSpeed;
            Upkeep = _defaultUpkeep;
        }

        internal override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_bodyTextures[bodyIndex], position, null, _color, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_headTexture, position, null, _headColor, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
