using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        /// <summary> Učitava formu sa opcijama igre </summary>
        /// <param name="position"> Pozicija gdje će se forma prikazat </param>
        public static void SetupControl(Point position)
        {
            _controlForm = new ControlForm();
            _controlForm.Show();
            _controlForm.Location = new System.Drawing.Point(position.X, position.Y);
        }

        /// <summary> Postavlja kameru za igru </summary>
        /// <param name="viewport"> Vidno polje, GraphicsDevice.Viewport </param>
        /// <param name="center"> Pozicija na kojoj se kamera centrirana </param>
        public static void SetupCamera(Viewport viewport, Vector2 center)
        {
            _mainCamera = new Camera2D(viewport, center);
        }

        /// <summary> Load-a simulaciju sa predefiniranim postavkama </summary>
        /// <param name="content"> ContentManager objekt za učitavanje tekstura </param>
        /// <param name="player1"> Bot za prvog igrača </param>
        /// <param name="player2"> Bot za drugog igrača </param>
        public static void LoadDefault(ContentManager content, Player player1, Player player2, ushort timeout = 100)
        {
            Mrav.LoadDefault(content);
            Leteci.Load(content, Color.White);
            Radnik.Load(Color.Yellow);
            Scout.Load(Color.DeepSkyBlue);
            Vojnik.Load(Color.OrangeRed);

            Baza.Load(content, Color.White);
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
                    //Patch.Map[rX, rY].Wall = true;
                    //Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].Wall = true;
                }
                else
                {
                    do
                    {
                        rX = rand.Next(Patch.Height);
                        rY = rand.Next(Patch.Width);
                    } while (Patch.Map[rX, rY].Wall);

                    Patch.Map[rX, rY].GrowResource();
                    Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].GrowResource();
                }
            }

            do
            {
                rX = rand.Next(3, Patch.Height - 3);
                rY = rand.Next(3, Patch.Width / 4);
            } while (Patch.Map[rX, rY].Wall || Patch.Map[rX, rY].Resources > 0);

            Baza.Baze.Add(Patch.Map[rX, rY].BuildBase(_igraci[0]));
            Baza.Baze.Add(Patch.Map[Patch.Height - rX - 1, Patch.Width - rY - 1].BuildBase(_igraci[1]));
        }

        /// <summary> Obavlja jedan korak simulacije, pokrece logiku botova </summary>
        public static void Update()
        {
            if (gameOver) return;

            _mainCamera.Update();

            Patch.ResetVisibility();
            Leteci.UpdateAnimation();

            Baza.Baze.ForEach(b => b.Update());
            Mrav.ResetAnts();

            Patch.UpdateMap();

            // BOTOVI
            for (byte i = 0; i < 2; i++)
                UpdatePlayer(i);

#if true

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                Random rand = new Random();

                Baza.Baze[0].ProduceUnit((MravType)rand.Next(4), 1);
                Baza.Baze[1].ProduceUnit((MravType)rand.Next(4), 1);
            }
#endif
        }

        private static void UpdatePlayer(byte playerID)
        {
            Mrav.PlayerTurn = playerID;
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

        /// <summary> Crta simulaciju, poziva spriteBatch.Begin() i .End() </summary>
        /// <param name="spriteBatch"> SpriteBatch objekt korišten za crtanje </param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            _mainCamera.Begin(spriteBatch);

            Patch.DrawMapBack(spriteBatch);
            Baza.Baze.ForEach(b => b.Draw(spriteBatch));

            Patch.DrawMapFront(spriteBatch);
            Mrav.DrawAll(spriteBatch);

            spriteBatch.End();
        }

    }
}
