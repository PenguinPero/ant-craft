using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MravKraftAPI.Simulacija
{
    using Baze;
    using Mravi;
    using Map;
    using Igraci;

    public static class Simulation
    {
        private static Player[] _players;
        private static Baza _player1Base, _player2Base;
        private static Camera2D _mainCamera;
        private static ControlForm _controlForm;
        private static ushort _timeout;
        private static bool gameOver;
        private static string gameOverMessage;
        private static Random _randomizer;
        //private static GameOutput ...

        /// <summary> Opens up game options windows form </summary>
        /// <param name="position"> Position of a form, upper right corner of the game window by default </param>
        public static void SetupControl(Point position)
        {
            _controlForm = new ControlForm();
            _controlForm.Show();
            _controlForm.Location = new System.Drawing.Point(position.X, position.Y);
        }

        /// <summary> Sets up game camera </summary>
        /// <param name="viewport"> Viewport of the window, GraphicsDevice.Viewport </param>
        /// <param name="center"> Camera's initial center point </param>
        public static void SetupCamera(Viewport viewport, Vector2 center)
        {
            _mainCamera = new Camera2D(viewport, center);
        }

        /// <summary> Loads simulation with predefined settings </summary>
        /// <param name="content"> ContentManager object for loading textures </param>
        /// <param name="player1"> Player one bot </param>
        /// <param name="player2"> Player two bot </param>
        /// <param name="timeout"> Maximum duration for each player's turn </param>
        /// <param name="seed"> Seed for randomizing, not used if it is equal -1 </param>
        public static void LoadDefault(ContentManager content, Player player1, Player player2, ushort timeout = 100, int seed = 0)
        {
            if (seed == -1) _randomizer = new Random();
            else _randomizer = new Random(seed);

            Mrav.LoadDefault(content);
            Leteci.Load(content, Color.White);
            Radnik.Load(Color.Yellow);
            Scout.Load(Color.DeepSkyBlue);
            Vojnik.Load(Color.OrangeRed);

            Baza.Load(content, Color.White, new ushort[] { 0, 100, 150, 200, 250, 300 });
            Patch.Load(content, Vector2.Zero, Color.Silver, Color.Black, Color.White);
            Resource.Load(content, Color.DodgerBlue, 0.06f);

            _mainCamera.Center = Patch.StartPoint + new Vector2(Patch.Width * Patch.Size / 2, Patch.Height * Patch.Size / 2);
            _mainCamera.UpLeftBound = Patch.StartPoint;
            _mainCamera.DownRightBound = new Vector2(Patch.StartPoint.X + Patch.Width * Patch.Size,
                                                     Patch.StartPoint.Y + Patch.Height * Patch.Size);

            _players = new Player[] { player1, player2 };
            player1.ID = 0;
            player2.ID = 1;

            _timeout = timeout;

            _controlForm.SetNames(player1.GetType().Name, player2.GetType().Name);
            Patch.PlayerVision = 3;

            RandomizeMap();
        }

        private static void RandomizeMap()
        {
            int rX = _randomizer.Next(3, Patch.Height);
            int rY = _randomizer.Next(3, Patch.Width / 4);

            _player1Base = Baza.Baze[0] = Patch.Map[rX, rY].BuildBase(_players[0]);
            _player2Base = Baza.Baze[1] = Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].BuildBase(_players[1]);

            // Slowdowns
            for (int i = 0; i < 80; i++)
            {
                do
                {
                    rX = _randomizer.Next(Patch.Height);
                    rY = _randomizer.Next(Patch.Width);
                } while (Patch.Map[rX, rY].GetSlowdown() ||
                         _player1Base.DistanceTo(Patch.Map[rX, rY].Center) < 100f ||
                         _player2Base.DistanceTo(Patch.Map[rX, rY].Center) < 100f);

                Patch.Map[rX, rY].Slowdown = true;
                Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].Slowdown = true;
            }

            // Resources
            for (int i = 0; i < 50; i++)
            {
                do
                {
                    rX = _randomizer.Next(Patch.Height);
                    rY = _randomizer.Next(Patch.Width);
                } while (Patch.Map[rX, rY].GetSlowdown() ||
                         Patch.Map[rX, rY].GetResources() > 0 ||
                         _player1Base.DistanceTo(Patch.Map[rX, rY].Center) < 100f ||
                         _player2Base.DistanceTo(Patch.Map[rX, rY].Center) < 100f);

                float minDist = Math.Min(_player1Base.DistanceTo(Patch.Map[rX, rY].Center),
                                         _player2Base.DistanceTo(Patch.Map[rX, rY].Center));

                short resources = (short)(400 + (minDist / 100f) * 12);

                Patch.Map[rX, rY].GrowResource(resources);
                Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].GrowResource(resources);
            }

            // Starting ants
            for (int i = 0; i < 6; i++)
            {
                Mrav.AddNew(0, new Radnik(_player1Base.Position, _players[0].Color, 0, (float)(_randomizer.NextDouble() * Math.PI * 2)));
                Mrav.AddNew(1, new Radnik(_player2Base.Position, _players[1].Color, 1, (float)(_randomizer.NextDouble() * Math.PI * 2)));
            }

#if false
            Vector2 pos1 = Patch.Map[Patch.Height / 2 - 1, Patch.Width / 2].Center;
            Vector2 pos2 = Patch.Map[Patch.Height / 2 + 1, Patch.Width / 2].Center;

            for (int i = 0; i < 130; i++)
            {
                Mrav.AddNew(0, new Leteci(pos1, _igraci[0].Color, 0, 0f));
            }

            for (int i = 0; i < 50; i++)
            {
                Mrav.AddNew(1, new Vojnik(pos2, _igraci[1].Color, 1, 0f));
            }
#endif
        }

        /// <summary> One step of simulation, runs players' bots </summary>
        public static void Update()
        {
            _mainCamera.Update();

            if (gameOver) return;

            Patch.ResetVisibility();
            Leteci.UpdateAnimation();

            _player1Base.Update();
            Mrav player1Spawn = _player1Base.Production();

            _player2Base.Update();
            Mrav player2Spawn = _player2Base.Production();

            // game end
            if (!_player1Base.Alive) { gameOver = true; gameOverMessage = $"{_players[0].GetType().Name} lost the game."; return; }
            if (!_player2Base.Alive) { gameOver = true; gameOverMessage = $"{_players[1].GetType().Name} lost the game."; return; }

            Mrav.ResetAnts();
            Patch.UpdateMap();

            if (player1Spawn != null) player1Spawn.JustSpawned = true;
            if (player2Spawn != null) player2Spawn.JustSpawned = true;

            // BOTOVI
            for (byte i = 0; i < 2; i++)
                UpdatePlayer(i);
        }

        private static void UpdatePlayer(byte playerID)
        {
            Patch.PlayerTurn = Mrav.PlayerTurn = Baza.PlayerTurn = playerID;

            Task pTask = Task.Factory.StartNew(() =>
                _players[playerID].Update(Mrav.Mravi[playerID].Where(m => m != null && m.Alive).ToList(), Baza.Baze[playerID])
            );

#if true
            pTask.Wait();
#endif

#if false
            try { pTask.Wait(_timeout); }
            catch
            {
                gameOver = true;
                gameOverMessage = $"Player {_igraci[playerID].GetType().Name} has thrown an error.";
                return;
            }

            if (!pTask.IsCompleted)
            {
                gameOver = true;
                gameOverMessage = $"Player {_igraci[playerID].GetType().Name} timeouted.";
            }
#endif
        }

        /// <summary> Used for drawing simulation, calls spriteBatch.Begin() and .End() </summary>
        /// <param name="spriteBatch"> SpriteBatch object used for drawing </param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            _mainCamera.Begin(spriteBatch);

            Patch.DrawMapBack(spriteBatch);
            Baza.DrawBases(spriteBatch);

            Patch.DrawMapFront(spriteBatch);
            Mrav.DrawAll(spriteBatch);

            spriteBatch.End();
        }

    }
}
