﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Baze
{
    using Mravi;
    using Igraci;
    using Map;

    public sealed class Baza
    {
        private static Texture2D _front, _back;
        private static Vector2 _origin;
        private static float _defaultScale;
        private static Color _defaultColor;
        private static int _defaultResources, _defaultHealth;
        private static byte[] _mravCost;
        private static Random _randomizer;
        private static ushort[] _levelUpkeep;
        private static byte _defaultResourceDrop, _defaultWorkers;

        internal static byte PlayerTurn { get; set; }
        internal static Baza[] Baze { get; set; }

        internal static void Load(ContentManager content, Color backColor, ushort[] levelUpkeep, int startingResources = 50,
                                  byte startingWorkers = 6, int defaultHealth = 100000, float scale = 0.18f)
        {
            _front = content.Load<Texture2D>(@"Images\Baza\bazaFront");
            _back = content.Load<Texture2D>(@"Images\Baza\bazaBack");

            _origin = new Vector2(_front.Width / 2, _front.Height / 2);
            _defaultColor = backColor;
            _defaultScale = scale;
            _defaultResources = startingResources;
            _defaultHealth = defaultHealth;
            _levelUpkeep = levelUpkeep;
            _defaultResourceDrop = (byte)_levelUpkeep.Length;
            _defaultWorkers = startingWorkers;

            _mravCost = new byte[4];
            _mravCost[(byte)MravType.Radnik] = Radnik.Cost;
            _mravCost[(byte)MravType.Scout] = Scout.Cost;
            _mravCost[(byte)MravType.Vojnik] = Vojnik.Cost;
            _mravCost[(byte)MravType.Leteci] = Leteci.Cost;

            _randomizer = new Random();
            Baze = new Baza[2];
        }

        internal static void DrawBases(SpriteBatch spriteBatch)
        {
            Baze[0].Draw(spriteBatch);
            Baze[1].Draw(spriteBatch);
        }

        private readonly Vector2 _position;
        private readonly Queue<MravProcess> _productionQueue;
        private readonly Color _color;
        private byte upkeep;
        private int resources;
        private List<Patch> _visiblePatches;
        private bool turnOne;

        public byte Owner { get; private set; }
        public int Resources { get { return (PlayerTurn == Owner) ? resources : 0; } }
        public int Health { get; private set; }
        public byte Upkeep { get { return (PlayerTurn == Owner) ? upkeep : (byte)0; } }

        /// <summary> <see cref="Patch"/> over which base is built </summary>
        public Patch PatchHere { get; private set; }
        /// <summary> List of visible <see cref="Patch"/>es in range </summary>
        public List<Patch> VisiblePatches { get { return (PlayerTurn == Owner) ? _visiblePatches : null; } }

        internal Vector2 Position { get { return _position; } }
        internal bool Alive { get; private set; }

        internal Baza(Vector2 position, Player owner, Patch patchHere)
        {
            Owner = owner.ID;
            resources = _defaultResources;
            Health = _defaultHealth;
            Alive = true;
            PatchHere = patchHere;

            _color = owner.Color;
            _position = position;
            _productionQueue = new Queue<MravProcess>();
            turnOne = true;
        }

        internal void Update()
        {
            if (Health == 0)
            {
                Alive = false;
                return;
            }

            if (turnOne)
            {
                _visiblePatches = Visibility(4).ToList();
                turnOne = false;
            }
            else _visiblePatches.ForEach(p => p.SetVisible(Owner));

            int totalAntUpkeep = Mrav.Mravi[Owner].Where(m => m != null).Sum(m => m.Upkeep);

            for (int i = _levelUpkeep.Length - 1; i >= 0; i--)
                if (totalAntUpkeep > _levelUpkeep[i])
                {
                    upkeep = (byte)i;
                    break;
                }
        }

        internal void GiveResource()
        {
            resources += _defaultResourceDrop - upkeep;
        }

        internal void TakeDamage(byte damage)
        {
            if (Health == 0) return;

            if (Health - damage > 0) Health -= damage;
            else Health = 0;
        }

        private IEnumerable<Patch> Visibility(byte radius)
        {
            PXY center = Patch.GetPXYAt(_position).Value;
            int leftX, rightX, leftY, rightY;

            leftX = (center.X - radius > 0) ? center.X - radius : 0;
            leftY = (center.Y - radius > 0) ? center.Y - radius : 0;

            rightX = (center.X + radius < Patch.Height) ? center.X + radius : Patch.Height - 1;
            rightY = (center.Y + radius < Patch.Width) ? center.Y + radius : Patch.Width - 1;

            for (int i = leftX; i <= rightX; i++)
                for (int j = leftY; j <= rightY; j++)
                {
                    Patch.Map[i, j].SetVisible(Owner);
                    yield return Patch.Map[i, j];
                }
        }

        internal Mrav Production()
        {
            if (_productionQueue.Count > 0)
            {
                MravProcess next = _productionQueue.Peek();

                if (next.DurationLeft == 0)
                {
                    Mrav spawn = null;

                    switch (next.MravTip)
                    {
                        case MravType.Radnik:
                            spawn = Mrav.AddNew(Owner, new Radnik(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Scout:
                            spawn = Mrav.AddNew(Owner, new Scout(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Vojnik:
                            spawn = Mrav.AddNew(Owner, new Vojnik(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                        case MravType.Leteci:
                            spawn = Mrav.AddNew(Owner, new Leteci(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                            break;
                    }

                    if (--next.CountLeft == 0) _productionQueue.Dequeue();
                    else next.ResetDuration();

                    return spawn;
                }
                else next.DurationLeft--;
            }

            return null;
        }

        internal IEnumerable<Mrav> StartingWorkers()
        {
            for (byte i = 0; i < _defaultWorkers; i++)
                yield return Mrav.AddNew(Owner, new Radnik(_position, _color, Owner, (float)(_randomizer.NextDouble() * Math.PI * 2)));
        }

        /// <summary>
        /// If enough resources, enqueues <paramref name="count"/> number of ants
        /// of specified <paramref name="type"/> to production queue.
        /// Returns true if the action was completed.
        /// </summary>
        /// <param name="type"> <see cref="MravType"/> of <see cref="Mrav"/> to produce </param>
        /// <param name="count"> Numbers of ants to produce </param>
        /// <returns> True if action completed. </returns>
        public bool ProduceUnit(MravType type, byte count)
        {
            if (PlayerTurn != Owner) return false;

            if (Resources - _mravCost[(byte)type] * count >= 0)
            {
                resources -= _mravCost[(byte)type] * count;
                _productionQueue.Enqueue(new MravProcess(type, count));
                return true;
            }

            return false;
        }

        /// <summary> Calculates the euclid distance between this <see cref="Baza"/> and a <see cref="Vector2"/> <paramref name="position"/>. </summary>
        /// <param name="position"> <see cref="Vector2"/> position of object </param>
        /// <returns> Euclid distance between this <see cref="Baza"/> and a <see cref="Vector2"/> <paramref name="position"/> </returns>
        public float DistanceTo(Vector2 position)
        {
            return (position - _position).Length();
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_back, _position, null, _defaultColor, 0f, _origin, _defaultScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(_front, _position, null, _color, 0f, _origin, _defaultScale, SpriteEffects.None, 0f); ;
        }

    }
}
