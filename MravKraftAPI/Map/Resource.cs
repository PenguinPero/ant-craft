using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MravKraftAPI.Map
{
    internal static class Resource
    {
        private static Texture2D _resourceTexture;
        private static Vector2 _origin;
        private static Color _resourceColor;
        private static float _defaultScale;

        internal static void Load(ContentManager content, Color resourceColor, float scale = 0.05f)
        {
            _resourceTexture = content.Load<Texture2D>(@"Images\Mapa\mrvica");
            _origin = new Vector2(_resourceTexture.Width / 2f, _resourceTexture.Height / 2f);
            _resourceColor = resourceColor;
            _defaultScale = scale;
        }

        internal static void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation = 0f)
        {
            spriteBatch.Draw(_resourceTexture, position, null, _resourceColor, rotation, _origin, _defaultScale, SpriteEffects.None, 0f);
        }

    }
}
