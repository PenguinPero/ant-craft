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
        private static Player[] _igraci;
        private static Camera2D _mainCamera;
        private static ControlForm _controlForm;
        private static ushort _timeout;
        private static bool gameOver;
        private static string gameOverMessage;
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
        public static void LoadDefault(ContentManager content, Player player1, Player player2, ushort timeout = 100)
        {
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

            _igraci = new Player[] { player1, player2 };
            player1.ID = 0;
            player2.ID = 1;

            _timeout = timeout;

            _controlForm.SetNames(player1.GetType().Name, player2.GetType().Name);
            Patch.PlayerVision = 3;

            RandomizeMap();
        }

        private static void RandomizeMap()
        {
            Random rand = new Random();
            int rX, rY;

            for (int i = 0; i < 600; i++)
            {
                rX = rand.Next(Patch.Height);
                rY = rand.Next(Patch.Width);

                if (i < 100)
                {
                    Patch.Map[rX, rY].Slowdown = true;
                    Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].Slowdown = true;
                }
                else
                {
                    do
                    {
                        rX = rand.Next(Patch.Height);
                        rY = rand.Next(Patch.Width);
                    } while (Patch.Map[rX, rY].GetSlowdown());

                    Patch.Map[rX, rY].GrowResource();
                    Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].GrowResource();
                }
            }

            do
            {
                rX = rand.Next(3, Patch.Height - 3);
                rY = rand.Next(3, Patch.Width / 4);
            } while (Patch.Map[rX, rY].GetSlowdown() || Patch.Map[rX, rY].Resources > 0);

            Baza.Baze[0] = Patch.Map[rX, rY].BuildBase(_igraci[0]);
            Baza.Baze[1] = Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].BuildBase(_igraci[1]);
        }

        /// <summary> One step of simulation, runs players' bots </summary>
        public static void Update()
        {
            _mainCamera.Update();

            if (gameOver) return;

            Patch.ResetVisibility();
            Leteci.UpdateAnimation();

            Baza.Baze[0].Update();
            Baza.Baze[1].Update();

            // game end
            if (!Baza.Baze[0].Alive) { gameOver = true; gameOverMessage = $"{_igraci[0].GetType().Name} lost the game."; return; }
            if (!Baza.Baze[1].Alive) { gameOver = true; gameOverMessage = $"{_igraci[1].GetType().Name} lost the game."; return; }

            Mrav.ResetAnts();

            Patch.UpdateMap();

#if true

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                Random rand = new Random();

                Baza.PlayerTurn = 0;
                bool temp = Baza.Baze[0].ProduceUnit(MravType.Leteci, 1);
            }
#endif

            // BOTOVI
            for (byte i = 0; i < 2; i++)
                UpdatePlayer(i);
        }

        private static void UpdatePlayer(byte playerID)
        {
            Patch.PlayerTurn = Mrav.PlayerTurn = Baza.PlayerTurn = playerID;

            Task pTask = Task.Factory.StartNew(() =>
            {
                foreach (Mrav mrav in Mrav.Mravi[playerID].Where(m => m != null))
                    _igraci[playerID].Update(mrav);

                _igraci[playerID].Update(Baza.Baze[playerID]);
            });

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
