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
                Mravi[0][i]?.PreUpdate();

            for (int i = 0; i < Mravi[1].Count; i++)
                Mravi[1][i]?.PreUpdate();

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
        protected float rotation;
        protected short health;
        protected bool alive;
        protected bool returnedHit;

        private bool movedOrAttacked;
        private List<Patch> visiblePatches;
        private Patch patchHere;
        private List<Mrav> visibleEnemies;

        public byte Owner { get; private set; }
        public int ID { get; internal set; }
        public byte Damage { get; protected set; }
        public byte Armor { get; protected set; }
        public byte ArmorPen { get; protected set; }
        public byte Vision { get; protected set; }
        public float Speed { get; protected set; }

        protected TProp GetterCheck<TProp>(TProp field)
        {
            if (PlayerTurn != Owner)
            {
                if (visibleToEnemy) return field;
                return default(TProp);
            }

            return field;
        }

        public Vector2 Position { get { return GetterCheck(position); } }
        public float Rotation { get { return GetterCheck(rotation); } }
        public short Health { get { return GetterCheck(health); } }
        public bool MovedOrAttacked { get { return GetterCheck(movedOrAttacked); } }
        public bool Alive { get { return GetterCheck(alive); } }

        public List<Patch> VisiblePatches { get { return GetterCheck(visiblePatches); } }
        public Patch PatchHere { get { return GetterCheck(patchHere); } }
        public List<Mrav> VisibleEnemies { get { return GetterCheck(visibleEnemies); } }

        public MravType Type { get; private set; }
        protected bool visibleToEnemy;

        internal Mrav(Vector2 position, Color color, byte owner, float rotation, MravType type)
        {
            this.position = position;
            this.rotation = rotation;
            Owner = owner;
            Type = type;
            alive = true;

            direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            bodyIndex = 0;

            _color = color;

            patchHere = Patch.GetPatchAt(position);
            visibleEnemies = new List<Mrav>();
        }

        private void Animation()
        {
            bodyIndex = (byte)((bodyIndex + 1) % _bodyCount);
        }

        private void PreUpdate()
        {
            if (!alive)
            {
                patchHere.Mravi[Owner].Remove(ID);
                Die(Owner, ID);

                return;
            }

            visibleToEnemy = false;
        }

        private void Update()
        {
            returnedHit = movedOrAttacked = false;
            visiblePatches = Visibility().ToList();

            visibleEnemies.Clear();
            visiblePatches.ForEach(p => visibleEnemies.AddRange(p.MraviHere((byte)(1 - Owner))
                                                                 .Select(mID => Mravi[1 - Owner][mID])));

            visibleEnemies.ForEach(e => e.visibleToEnemy = true);
        }

        private void SetPatch()
        {
            patchHere.Mravi[Owner].Add(ID);
        }

        public void SetRotation(float rotation)
        {
            if (PlayerTurn != Owner || !alive) return;

            this.rotation = rotation;
            direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        public float DistanceTo(Vector2 position)
        {
            return (position - this.position).Length();
        }

        public void Face(Vector2 position)
        {
            if (PlayerTurn != Owner || !alive) return;

            SetRotation((float)Math.Atan2(position.Y - this.position.Y, position.X - this.position.X));
        }

        public void Face(Baza baza)
        {
            if (baza.Owner != Owner && EnemyBase() != null || baza.Owner == Owner)
                Face(baza.PatchHere);
        }

        public void Face(Patch patch)
        {
            if (patch == null) return;

            Face(patch.Center);
        }

        internal void TakeDamage(byte damage, byte armorPen)
        {
            if (!alive) return;

            health -= (short)(damage - Armor + armorPen);

            if (health <= 0)
            {
                health = 0;
                alive = false;
            }
        }

        internal Patch PatchAhead(float distance)
        {
            return Patch.GetPatchAt(position + direction * distance);
        }

        public void Attack(Mrav mrav)
        {
            if (movedOrAttacked || PlayerTurn != Owner || !alive || mrav == null) return;

            Face(mrav.position);

            if (DistanceTo(mrav.position) <= 12f)
            {
                mrav.TakeDamage(Damage, ArmorPen);

                if (!mrav.returnedHit)
                {
                    TakeDamage(mrav.Damage, mrav.ArmorPen);
                    mrav.returnedHit = true;
                }

                movedOrAttacked = true;
            }
        }

        public void Attack(Baza baza)
        {
            if (movedOrAttacked || PlayerTurn != Owner || !alive || baza == null) return;

            Face(baza.Position);

            if (DistanceTo(baza.Position) <= 12f)
            {
                baza.TakeDamage(Damage);
                movedOrAttacked = true;
            }
        }

        public void MoveForward()
        {
            if (movedOrAttacked || PlayerTurn != Owner || !alive) return;

            Patch patchAhead = PatchAhead(Speed);

            if (patchAhead != null)
            {
                if (!patchAhead.Center.Equals(patchHere.Center))
                {
                    patchHere.Mravi[Owner].Remove(ID);
                    patchAhead.Mravi[Owner].Add(ID);
                    patchHere = patchAhead;
                }

                if (Type != MravType.Leteci && patchHere.GetSlowdown())
                    position += direction * Speed * (1f - Patch.SlowdownValue);
                else
                {
                    position += direction * Speed;
                    Animation();
                }

                movedOrAttacked = true;
            }
        }

        private IEnumerable<Patch> Visibility()
        {
            PXY center = Patch.GetPXYAt(position).Value;
            int leftX, rightX, leftY, rightY;

            leftX = (center.X - Vision > 0) ? center.X - Vision : 0;
            leftY = (center.Y - Vision > 0) ? center.Y - Vision : 0;

            rightX = (center.X + Vision < Patch.Height) ? center.X + Vision : Patch.Height - 1;
            rightY = (center.Y + Vision < Patch.Width) ? center.Y + Vision : Patch.Width - 1;

            for (int i = leftX; i <= rightX; i++)
                for (int j = leftY; j <= rightY; j++)
                {
                    Patch.Map[i, j].SetVisible(Owner);
                    yield return Patch.Map[i, j];
                }
        }

        public Baza EnemyBase()
        {
            if (PlayerTurn != Owner || !Alive) return null;

            Baza enemyBase = Baza.Baze[1 - Owner];

            if (Math.Abs(enemyBase.PatchHere.X - patchHere.X) > Vision ||
                Math.Abs(enemyBase.PatchHere.Y - patchHere.Y) > Vision)
                return null;

            return enemyBase;
        }

        internal abstract void Draw(SpriteBatch spriteBatch);

    }
}
