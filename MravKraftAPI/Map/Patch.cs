﻿using Microsoft.Xna.Framework;
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

    internal struct PXY
    {
        internal readonly byte X, Y;

        internal PXY(byte x, byte y)
        {
            X = x;
            Y = y;
        }

    }

    public class Patch
    {
        /// <summary> Patch map matrix [<see cref="Height"/>, <see cref="Width"/>] </summary>
        public static Patch[,] Map { get; private set; }
        /// <summary> Horizontal number of patches </summary>
        public static byte Width { get; private set; }
        /// <summary> Vertical number of patches </summary>
        public static byte Height { get; private set; }
        /// <summary> Size of each patch (both width and height) </summary>
        public static byte Size { get; private set; }

        private static Texture2D _backTexture, _frontTexture, _wallTexture;
        private static Color _backColor, _frontColor, _wallColor;
        private static Vector2 _wallOrigin;
        private static Point _size;
        private static float _defaultScale;
        private static Random _randomizer;
        private static byte _maxWorkers;

        /// <summary>
        /// Amount of which each marked patch slows down ants crossing it.
        /// Should be in range of 0f to 1f, where 0f does nothing and 1f stops them completely.
        /// </summary>
        public static float SlowdownValue { get; private set; }

        internal static Vector2 StartPoint { get; private set; }
        internal static byte PlayerVision { get; set; }
        internal static byte PlayerTurn { get; set; }

        internal static void Load(ContentManager content, Vector2 startPoint, Color backColor, Color frontColor,
                                  Color wallColor, byte width = 96, byte height = 54, byte size = 20, byte workers = 8, float slowdown = 0.5f)
        {
            Map = new Patch[Height = height, Width = width];
            Size = size;
            StartPoint = startPoint;
            SlowdownValue = slowdown;

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
            _maxWorkers = workers;

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
        private short resources;
        private bool slowdown;

        /// <summary>
        /// Amount of resources on this patch,
        /// returns -1 if patch is not visible to a friendly ant.
        /// </summary>
        public short Resources { get { return (visible[PlayerTurn]) ? resources : (short)-1; } }

        /// <summary> <see cref="Vector2"/> position of patch's center. </summary>
        public Vector2 Center { get { return _resPosition; } }

        /// <summary> Is patch visible to any friendly ant. </summary>
        public bool Visible { get { return visible[PlayerTurn]; } }

        /// <summary> If true, ants crossing this patch will be slowed down by <see cref="SlowdownValue"/>. </summary>
        public bool? Slowdown
        {
            get { return (visible[PlayerTurn]) ? (bool?)slowdown : null; }
            internal set { slowdown = (bool)value; }
        }

        /// <summary> X coordinate of this patch in <see cref="Map"/> </summary>
        public int X { get; private set; }
        /// <summary> Y coordinate of this patch in <see cref="Map"/> </summary>
        public int Y { get; private set; }

        internal readonly HashSet<int>[] Mravi;
        internal readonly HashSet<int>[] Workers;

        internal Patch(int x, int y)
        {
            _position = new Rectangle(new Point((int)(StartPoint.X) + y * Size, (int)(StartPoint.Y) + x * Size), _size);
            _resPosition = new Vector2(_position.X + Size / 2f, _position.Y + Size / 2f);
            Mravi = new HashSet<int>[] { new HashSet<int>(), new HashSet<int>() };
            Workers = new HashSet<int>[] { new HashSet<int>(), new HashSet<int>() };
            X = x;
            Y = y;

            visible = new bool[2];
            slowdown = false;
        }

        internal void Update()
        {
            fogOfWar = false;

            if (PlayerVision == 3 && !visible[0] && !visible[1]) fogOfWar = true;
            else if (PlayerVision == 1 && !visible[0]) fogOfWar = true;
            else if (PlayerVision == 2 && !visible[1]) fogOfWar = true;
            //else if (PlayerVision == 0) fogOfWar = true;

            if (resources > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Mrav.PlayerTurn = (byte)i;

                    Workers[i].RemoveWhere(mID =>
                    {
                        Mrav current = Mrav.Mravi[i][mID];

                        return current == null || !current.Alive || !((Radnik)current).CarryingFood;
                    });
                }
            }
        }

        internal IEnumerable<int> MraviHere(byte owner)
        {
            if (!visible[1 - owner]) return null;

            return Mravi[owner].Select(m => m);
        }

        internal Baza BuildBase(Player owner)
        {
            return new Baza(_resPosition, owner, this);
        }

        internal void GrowResource(short count)
        {
            resRotation = (float)(_randomizer.NextDouble() * Math.PI * 2);
            resources += count;
        }

        internal short GetResources()
        {
            return resources;
        }

        internal bool GetSlowdown()
        {
            return slowdown;
        }

        private bool AssignWorker(int id)
        {
            if (Workers[PlayerTurn].Count == _maxWorkers) return false;

            Workers[PlayerTurn].Add(id);
            return true;
        }

        internal void SetVisible(byte player)
        {
            visible[player] = true;
        }

        internal bool TakeResource(Vector2 myPosition, int id)
        {
            if (!AssignWorker(id) || resources == 0 || DistanceTo(myPosition) > 12f) return false;

            resources--;
            return true;
        }

        private float DistanceTo(Vector2 position)
        {
            return (position - _resPosition).Length();
        }

        internal void DrawBack(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(_backTexture, _position, (Mravi[0].Count > 0 || Mravi[1].Count > 0) ? Color.Red : _backColor);
            spriteBatch.Draw(_backTexture, _position, _backColor);
        }

        internal void DrawFront(SpriteBatch spriteBatch)
        {
            if (fogOfWar) spriteBatch.Draw(_frontTexture, _position, _frontColor);
            else
            {
                if (slowdown) spriteBatch.Draw(_wallTexture, _resPosition, null, _wallColor, 0f, _wallOrigin, _defaultScale, SpriteEffects.None, 0f);
                else if (resources > 0) Resource.Draw(spriteBatch, _resPosition, resRotation);
            }
        }

        public override string ToString()
        {
            return $"PATCH [{X}, {Y}]";
        }

    }
}
