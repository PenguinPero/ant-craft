using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

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

        public static byte Cost { get; private set; }
        public static byte Duration { get; private set; }
        public static byte Upkeep { get; private set; }
        public static byte Vision { get; private set; }
        public static byte Damage { get; private set; }

        internal static void Load(ContentManager content, Color wingColor, byte cost = 0, byte duration = 50, byte upkeep = 0,
                                byte vision = 4, byte damage = 0, byte health = 0, float scale = 0.06f, float speed = 0.55f)
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

            Cost = cost;
            Duration = duration;
            Upkeep = upkeep;
            Vision = vision;
            Damage = damage;
        }

        internal static void UpdateAnimation()
        {
            _wingsAnimation.Update();
        }

        internal Leteci(Vector2 position, Color color, byte owner, float rotation) : base(position, color, owner, rotation, MravType.Leteci)
        {
            Health = _defaultHealth;
        }

        public override void MoveForward()
        {
            Move(_defaultSpeed, true);
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
            spriteBatch.Draw(_flyBodyTexture, position, null, _color, Rotation, _flyOrigin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_wingsAnimation.CurrentTexture, position, null, _defaultColor, Rotation, _flyOrigin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
