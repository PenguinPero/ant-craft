using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Map
{
    using Baze;
    using Igraci;
    using Mravi;

    public struct PXY
    {
        public readonly byte X, Y;

        public PXY(byte x, byte y)
        {
            X = x;
            Y = y;
        }

    }

    public class Patch
    {
        internal static Patch[,] Map;
        internal static byte Width, Height, Size;
        private static Texture2D _backTexture, _frontTexture, _wallTexture;
        private static Color _backColor, _frontColor, _wallColor;
        private static Vector2 _wallOrigin;
        private static Point _size;
        private static float _defaultScale;
        private static Random _randomizer;

        internal static Vector2 StartPoint;
        internal static byte PlayerVision;

        internal static void Load(ContentManager content, Vector2 startPoint, Color backColor, Color frontColor,
                                  Color wallColor, byte width = 96, byte height = 54, byte size = 20)
        {
            Map = new Patch[Height = height, Width = width];
            Size = size;
            StartPoint = startPoint;

            _size = new Point(size, size);
            _backTexture = content.Load<Texture2D>(@"Images\Mapa\patch");
            _frontTexture = content.Load<Texture2D>(@"Images\Mapa\patchOver");
            _wallTexture = content.Load<Texture2D>(@"Images\Mapa\wall");
            _backColor = backColor;
            _frontColor = frontColor;
            _wallColor = wallColor;
            _wallOrigin = new Vector2(_wallTexture.Width / 2f, _wallTexture.Height / 2f);
            _defaultScale = (float)size / _backTexture.Width;
            _randomizer = new Random();

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    Map[i, j] = new Patch(i, j);
        }

        internal static Patch GetPatchAt(Vector2 position)
        {
            float dX = position.Y - StartPoint.Y;
            float dY = position.X - StartPoint.X;

            if (dX < 0 || dY < 0) return null;

            int pX = (int)(dX / Size);
            int pY = (int)(dY / Size);

            if (pX >= Height || pY >= Width) return null;

            return Map[pX, pY];
        }

        internal static PXY? GetPXYAt(Vector2 position)
        {
            float dX = position.Y - StartPoint.Y;
            float dY = position.X - StartPoint.X;

            if (dX < 0 || dY < 0) return null;

            byte pX = (byte)(dX / Size);
            byte pY = (byte)(dY / Size);

            if (pX >= Height || pY >= Width) return null;

            return new PXY(pX, pY);
        }

        internal static void ResetVisibility()
        {
            foreach (Patch patch in Map)
                patch.visible[0] = patch.visible[1] = false;
        }

        internal static void UpdateMap()
        {
            foreach (Patch patch in Map)
                patch.Update();
        }

        internal static void DrawMapBack(SpriteBatch spriteBatch)
        {
            foreach (Patch patch in Map)
                patch.DrawBack(spriteBatch);
        }

        internal static void DrawMapFront(SpriteBatch spriteBatch)
        {
            foreach (Patch patch in Map)
                patch.DrawFront(spriteBatch);
        }

        private readonly Rectangle _position;
        private readonly Vector2 _resPosition;
        private float resRotation;
        private bool[] visible;
        private bool fogOfWar;

        public byte Resources { get; private set; }
        public Vector2 Center { get { return _resPosition; } }
        public bool Wall { get; internal set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        internal readonly HashSet<int>[] Mravi;

        internal Patch(int x, int y)
        {
            _position = new Rectangle(new Point((int)(StartPoint.X) + y * Size, (int)(StartPoint.Y) + x * Size), _size);
            _resPosition = new Vector2(_position.X + Size / 2f, _position.Y + Size / 2f);
            Mravi = new HashSet<int>[] { new HashSet<int>(), new HashSet<int>() };

            visible = new bool[2];
            Wall = false;
        }

        internal void Update()
        {
            fogOfWar = false;

            if (PlayerVision == 3 && !visible[0] && !visible[1]) fogOfWar = true;
            else if (PlayerVision == 1 && !visible[0]) fogOfWar = true;
            else if (PlayerVision == 2 && !visible[1]) fogOfWar = true;
            //else if (PlayerVision == 0) fogOfWar = true;
        }

        internal IEnumerable<int> MraviHere(byte owner)
        {
            if (!visible[1 - owner]) return null;

            return Mravi[owner].Select(m => m);
        }

        internal Baza BuildBase(Player owner)
        {
            return new Baza(_resPosition, owner);
        }

        internal void GrowResource()
        {
            resRotation = (float)(_randomizer.NextDouble() * Math.PI * 2);
            Resources++;
        }

        internal void SetVisible(byte player)
        {
            visible[player] = true;
        }

        internal bool TakeResource(Vector2 myPosition)
        {
            if (Resources == 0 || DistanceTo(myPosition) > 12f) return false;

            Resources--;
            return true;
        }

        private float DistanceTo(Vector2 position)
        {
            return (position - _resPosition).Length();
        }

        internal void DrawBack(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backTexture, _position, (Mravi[0].Count > 0 || Mravi[1].Count > 0) ? Color.Red : _backColor);
            //spriteBatch.Draw(_backTexture, _position, _backColor);
        }

        internal void DrawFront(SpriteBatch spriteBatch)
        {
            if (fogOfWar) spriteBatch.Draw(_frontTexture, _position, _frontColor);
            else
            {
                if (Wall) spriteBatch.Draw(_wallTexture, _resPosition, null, _wallColor, 0f, _wallOrigin, _defaultScale, SpriteEffects.None, 0f);
                else if (Resources > 0) Resource.Draw(spriteBatch, _resPosition, resRotation);
            }
        }

        public override string ToString()
        {
            return $"{Center.X} {Center.Y}";
        }

    }
}
