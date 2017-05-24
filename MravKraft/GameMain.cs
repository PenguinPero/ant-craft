using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MravKraftAPI.Simulacija;

namespace MravKraft
{
    using Botovi;

    public class GameMain : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private const int WIDTH = 1920, HEIGHT = 1080, LEFT = 100, TOP = 100;

        public GameMain()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
            Content.RootDirectory = "Content";

            Window.Position = new Point((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - WIDTH - LEFT) / 2,
                                        (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - HEIGHT - TOP) / 2);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Simulation.SetupControl(new Point(Window.Position.X + WIDTH, Window.Position.Y));
            Simulation.SetupCamera(GraphicsDevice.Viewport, new Vector2(WIDTH / 2f, HEIGHT / 2f));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            PenguinPero player1 = new PenguinPero(Color.Black);
            PenguinPero player2 = new PenguinPero(Color.DarkRed);
            //Sm4Ck player2 = new Sm4Ck(Color.DarkRed);

            Simulation.LoadDefault(Content, player1, player2);
        }

        protected override void Update(GameTime gameTime)
        {
            Simulation.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Simulation.Draw(spriteBatch);

            base.Draw(gameTime);
        }

    }
}
