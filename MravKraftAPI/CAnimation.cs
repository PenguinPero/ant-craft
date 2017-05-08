using Microsoft.Xna.Framework.Graphics;

namespace MravKraftAPI
{
    internal class CAnimation
    {
        private readonly Texture2D[] textureArray;
        private byte textureIndex, length;
        private bool direction;

        internal Texture2D CurrentTexture
        {
            get { return textureArray[textureIndex]; }
        }

        internal CAnimation(Texture2D[] textureArray)
        {
            this.textureArray = textureArray;
            length = (byte)(textureArray.Length);
            textureIndex = 0;
            direction = true;
        }

        internal void Update()
        {
            if (direction)
            {
                if (textureIndex + 1 == length) direction = false;
                else textureIndex++;
            }
            else
            {
                if (textureIndex - 1 == -1) direction = true;
                else textureIndex--;
            }
        }

    }
}
