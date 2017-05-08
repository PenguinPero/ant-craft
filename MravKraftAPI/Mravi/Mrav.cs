using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI.Mravi
{
    using Map;
    using Baze;

    public abstract class Mrav
    {
        protected static Texture2D[] _bodyTextures;
        protected static Texture2D _headTexture;
        protected static Vector2 _origin;
        protected static byte _bodyCount;

        internal static byte PlayerTurn { get; set; }
        internal static List<Mrav>[] Mravi { get; private set; }
        private static Stack<int>[] _recycle;

        internal static void LoadDefault(ContentManager content)
        {
            string root = @"Images\Mrav\Obicni\mravBody";
            _bodyTextures = new Texture2D[22];
            _bodyCount = 22;

            for (int i = 0; i < _bodyCount; i++)
                _bodyTextures[i] = content.Load<Texture2D>(root + i);

            _headTexture = content.Load<Texture2D>(@"Images\Mrav\mravKapa");
            _origin = new Vector2(_headTexture.Width / 2, _headTexture.Height / 2);
            Mravi = new List<Mrav>[] { new List<Mrav>(), new List<Mrav>() };
            _recycle = new Stack<int>[] { new Stack<int>(), new Stack<int>() };
        }

        internal static void ResetAnts()
        {
            for (int i = 0; i < Mravi[0].Count; i++)
                Mravi[0][i]?.Update();

            for (int i = 0; i < Mravi[1].Count; i++)
                Mravi[1][i]?.Update();
        }

        internal static void Die(byte owner, int id)
        {
            Mravi[owner][id] = null;
            _recycle[owner].Push(id);
        }

        internal static void AddNew(byte owner, Mrav newMrav)
        {
            if (_recycle[owner].Count > 0)
            {
                int id = _recycle[owner].Pop();

                Mravi[owner][id] = newMrav;
                newMrav.ID = id;
            }
            else
            {
                newMrav.ID = Mravi[owner].Count;
                Mravi[owner].Add(newMrav);
            }

            newMrav.SetPatch();
        }

        internal static void DrawAll(SpriteBatch spriteBatch)
        {
            Mravi[0].ForEach(m => m?.Draw(spriteBatch));
            Mravi[1].ForEach(m => m?.Draw(spriteBatch));
        }

        protected Vector2 position, direction;
        protected byte bodyIndex;
        protected readonly Color _color;

        public Vector2 Position { get { return position; } }
        public byte Owner { get; private set; }
        public float Rotation { get; protected set; }
        public byte Health { get; protected set; }
        internal MravType Type { get; private set; }
        public int ID { get; internal set; }

        public List<Patch> VisiblePatches { get; private set; }
        public Patch PatchHere { get; private set; }
        public List<Mrav> VisibleEnemies { get; private set; }

        public bool TurnMovement { get; protected set; }
        public bool TurnAttack { get; protected set; }
        public bool Alive { get; protected set; }

        internal Mrav(Vector2 position, Color color, byte owner, float rotation, MravType type)
        {
            this.position = position;
            Rotation = rotation;
            Owner = owner;
            Type = type;
            Alive = true;

            direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            bodyIndex = 0;

            _color = color;

            PatchHere = Patch.GetPatchAt(position);
            VisibleEnemies = new List<Mrav>();
        }

        private void Animation()
        {
            bodyIndex = (byte)((bodyIndex + 1) % _bodyCount);
        }

        private void Update()
        {
            if (Health == 0)
            {
                Alive = false;
                PatchHere.Mravi[Owner].Remove(ID);
                Die(Owner, ID);

                return;
            }

            TurnMovement = TurnAttack = false;
            VisiblePatches = Visibility().ToList();

            VisibleEnemies.Clear();

            VisiblePatches.ForEach(p =>
            {
                if (p.Mravi[1 - Owner].Any())
                    VisibleEnemies.AddRange(p.Mravi[1 - Owner].Select(mID => Mravi[1 - Owner][mID]));
            });
        }

        private void SetPatch()
        {
            PatchHere.Mravi[Owner].Add(ID);
        }

        public abstract void MoveForward();
        public abstract void Attack(Mrav mrav);
        internal abstract IEnumerable<Patch> Visibility();
        internal abstract void Draw(SpriteBatch spriteBatch);

        // PUBLIC
        public void SetRotation(float rotation)
        {
            if (PlayerTurn != Owner || !Alive) return;

            Rotation = rotation;
            direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        public float DistanceTo(Vector2 position)
        {
            return (position - this.position).Length();
        }

        public void Face(Vector2 position)
        {
            if (PlayerTurn != Owner || !Alive) return;

            SetRotation((float)Math.Atan2(position.Y - this.position.Y, position.X - this.position.X));
        }

        public void FaceBase()
        {
            Face(Baza.Baze[Owner].Position);
        }

        // INTERNAL
        internal void TakeDamage(byte damage)
        {
            if (Health - damage > 0) Health -= damage;
            else Health = 0;
        }

        internal Patch PatchAhead(float distance, bool leteci)
        {
            Patch pAhead = Patch.GetPatchAt(position + direction * distance);

            return (pAhead == null) ? null : ((pAhead.Wall && !leteci) ? null : pAhead);
        }

        // PROTECTED
        protected void Attack(Mrav mrav, byte damage)
        {
            if (TurnAttack || PlayerTurn != Owner || !Alive) return;

            Face(mrav.Position);

            if (DistanceTo(mrav.Position) <= 12f)
            {
                mrav.TakeDamage(damage);
                TurnAttack = true;
            }
        }

        protected void Move(float speed, bool leteci = false)
        {
            if (TurnMovement || PlayerTurn != Owner || !Alive) return;

            Patch patchAhead = PatchAhead(speed, leteci);

            if (patchAhead != null)
            {
                if (!patchAhead.Center.Equals(PatchHere.Center))
                {
                    PatchHere.Mravi[Owner].Remove(ID);
                    patchAhead.Mravi[Owner].Add(ID);
                    PatchHere = patchAhead;
                }

                position += direction * speed;
                Animation();

                TurnMovement = true;
            }
        }

        protected IEnumerable<Patch> Visibility(byte radius)
        {
            PXY center = Patch.GetPXYAt(position).Value;
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

    }
}
