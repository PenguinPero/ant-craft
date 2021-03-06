﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MravKraftAPI.Mravi
{
    using Map;

    public sealed class Leteci : Mrav
    {
        private static Texture2D _flyBodyTexture;
        private static CAnimation _wingsAnimation;
        private static Vector2 _flyOrigin;
        private static float _defaultScale, _defaultSpeed;
        private static Color _defaultColor;
        private static byte _defaultHealth;
        private static byte _defaultVision;
        private static byte _defaultDamage;
        private static byte _defaultArmor;
        private static byte _defaultArmorPen;
        private static byte _defaultUpkeep;

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }

        internal static void Load(ContentManager content, Color wingColor, byte cost = 150, byte duration = 75,
                                  byte vision = 4, byte damage = 10, byte health = 50, byte armor = 0, byte armorPen = 0,
                                  byte upkeep = 2, float scale = 0.06f, float speed = 1.5f)
        {
            _flyBodyTexture = content.Load<Texture2D>(@"Images\Mrav\mravLeteci");

            string root = @"Images\Mrav\Leteci\krila";
            Texture2D[] animation = new Texture2D[16];

            for (int i = 0; i < 16; i++)
                animation[i] = content.Load<Texture2D>(root + i);

            _wingsAnimation = new CAnimation(animation);

            _flyOrigin = new Vector2(_flyBodyTexture.Width / 2, _flyBodyTexture.Height / 2);
            _defaultColor = wingColor;
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

        internal static void UpdateAnimation()
        {
            _wingsAnimation.Update();
        }

        internal Leteci(Vector2 position, Color color, byte owner, float rotation)
            : base(position, color, owner, rotation, MravType.Leteci)
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
            spriteBatch.Draw(_flyBodyTexture, position, null, _color, rotation, _flyOrigin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_wingsAnimation.CurrentTexture, position, null, _defaultColor, rotation, _flyOrigin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
