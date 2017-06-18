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

        internal static Mrav AddNew(byte owner, Mrav newMrav)
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
            return newMrav;
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
        protected bool visibleToEnemy;

        private bool movedOrAttacked;
        private List<Patch> visiblePatches;
        private Patch patchHere;
        private List<Mrav> visibleEnemies;
        private Dictionary<string, object> _customProp;

        public byte Owner { get; private set; }
        public int ID { get; internal set; }
        public byte Damage { get; protected set; }
        public byte Armor { get; protected set; }
        public byte ArmorPen { get; protected set; }
        public byte Vision { get; protected set; }
        public float Speed { get; protected set; }
        public byte Upkeep { get; protected set; }
        public bool JustSpawned { get; internal set; }

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

        /// <summary> <see cref="Patch"/> on which ant is currently standing </summary>
        public Patch PatchHere { get { return GetterCheck(patchHere); } }
        /// <summary> <see cref="Patch"/>es visible in ant's range </summary>
        public List<Patch> VisiblePatches { get { return GetterCheck(visiblePatches); } }
        /// <summary> List of enemy <see cref="Mrav"/>s visible in ant's range </summary>
        public List<Mrav> VisibleEnemies { get { return GetterCheck(visibleEnemies); } }

        /// <summary> Ant's type (Worker/Scout/Ground/Flying) </summary>
        public MravType Type { get; private set; }

        /// <summary> Custom ant properties </summary>
        /// <param name="key"> Key to bind to custom property </param>
        /// <returns> Object assigned to key </returns>
        public object this[string key]
        {
            get { return _customProp[key]; }
            set { _customProp[key] = value; }
        }

        /// <summary> Checks if this ant has custom property with a specified <paramref name="key"/> </summary>
        /// <param name="key"> Key to check </param>
        /// <returns> True/false contains key or not </returns>
        public bool HasProp(string key)
        {
            return _customProp.ContainsKey(key);
        }

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
            _customProp = new Dictionary<string, object>();

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

            visibleToEnemy = JustSpawned = false;
        }

        private void Update()
        {
            movedOrAttacked = false;
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

        /// <summary> Sets the ant's rotation to specified value in radians. </summary>
        /// <param name="rotation"> Rotation in radians </param>
        public void SetRotation(float rotation)
        {
            if (PlayerTurn != Owner || !alive) return;

            this.rotation = rotation;
            direction = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        /// <summary> Sets the ant's rotation to specified value in degrees. </summary>
        /// <param name="rotation"> Rotation in degrees </param>
        public void SetRotation(int rotation)
        {
            if (PlayerTurn != Owner || !alive) return;

            if (rotation == 180)
            {
                this.rotation = (float)Math.PI;
                direction = new Vector2(-1, 0);
            }
            else SetRotation(rotation / 180f * (float)Math.PI);
        }

        /// <summary> Calculates the euclid distance between this <see cref="Mrav"/> and a <see cref="Vector2"/> <paramref name="position"/>. </summary>
        /// <param name="position"> <see cref="Vector2"/> position of object </param>
        /// <returns> Euclid distance between this <see cref="Mrav"/> and a <see cref="Vector2"/> <paramref name="position"/> </returns>
        public float DistanceTo(Vector2 position)
        {
            return (position - this.position).Length();
        }

        /// <summary> Turns <see cref="Mrav"/> around to face targeted <see cref="Vector2"/> position. </summary>
        /// <param name="position"> <see cref="Vector2"/> position to face </param>
        public void Face(Vector2 position)
        {
            if (PlayerTurn != Owner || !alive) return;

            SetRotation((float)Math.Atan2(position.Y - this.position.Y, position.X - this.position.X));
        }

        /// <summary> Turns <see cref="Mrav"/> around to face targeted <see cref="Baza"/>. </summary>
        /// <param name="baseToface"> <see cref="Baza"/> to face </param>
        public void Face(Baza baseToface)
        {
            Face(baseToface.PatchHere);
        }

        /// <summary> Turns <see cref="Mrav"/> around to face targeted <see cref="Patch"/>. </summary>
        /// <param name="patch"> <see cref="Patch"/> to face </param>
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

        /// <summary>
        /// Faces targeted <see cref="Mrav"/> and if in range (magical 12f) deals
        /// <see cref="Damage"/> to it. Also takes damage from that <see cref="Mrav"/> back.
        /// Does nothing if it already moved or attacked this turn or if the targeted <see cref="Mrav"/> is null.
        /// </summary>
        /// <param name="antToAttack"> <see cref="Mrav"/> to attack </param>
        public void Attack(Mrav antToAttack)
        {
            if (movedOrAttacked || PlayerTurn != Owner || !alive || antToAttack == null) return;

            Face(antToAttack.position);

            if (DistanceTo(antToAttack.position) <= 12f)
            {
                antToAttack.TakeDamage(Damage, ArmorPen);
                TakeDamage(antToAttack.Damage, antToAttack.ArmorPen);

                movedOrAttacked = true;
            }
        }

        /// <summary>
        /// Faces targeted <see cref="Baza"/> and if in range (magical 12f) deals <see cref="Damage"/> to it. 
        /// Does nothing if it already moved or attacked this turn or if the targeted <see cref="Baza"/> is null.
        /// </summary>
        /// <param name="baseToAttack"> <see cref="Baza"/> to attack </param>
        public void Attack(Baza baseToAttack)
        {
            if (movedOrAttacked || PlayerTurn != Owner || !alive || baseToAttack == null) return;

            Face(baseToAttack.Position);

            if (DistanceTo(baseToAttack.Position) <= 12f)
            {
                baseToAttack.TakeDamage(Damage);
                movedOrAttacked = true;
            }
        }

        /// <summary>
        /// Moves <see cref="Speed"/> steps forward.
        /// Does nothing if it already moved or attacked this turn
        /// or if the position in front is out of bounds.
        /// </summary>
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

        /// <summary>
        /// Checks if the enemy <see cref="Baza"/> is visible, if it is returns it.
        /// Othwerwise returns null.
        /// </summary>
        /// <returns> Enemy <see cref="Baza"/> or null </returns>
        public Baza EnemyBase()
        {
            if (PlayerTurn != Owner || !Alive) return null;

            Baza enemyBase = Baza.Baze[1 - Owner];

            if (!enemyBase.PatchHere.Visible)
                return null;

            return enemyBase;
        }

        internal abstract void Draw(SpriteBatch spriteBatch);

    }
}
