using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MravKraftAPI.Map
{
    internal class Camera2D
    {
        private readonly Vector3 _viewPoint;

        internal Vector2 Center;
        internal float Zoom;
        internal float Rotation;

        internal Matrix Transform { get; private set; }
        internal Matrix InverseTransform { get; private set; }

        private Vector2 upLeftBound, downRightBound;

        internal Vector2 UpLeftBound { set { upLeftBound = value + new Vector2(_viewPoint.X, _viewPoint.Y); } }
        internal Vector2 DownRightBound { set { downRightBound = value - new Vector2(_viewPoint.X, _viewPoint.Y); } }

        private MouseState oldMouseState;

        private const float ZOOM_RATIO = 0.0005f;
        private const float MAX_ZOOM = 0.45f;
        private const float MIN_ZOOM = 1f;

        internal Camera2D(Viewport viewport, Vector2 center)
        {
            _viewPoint = new Vector3(viewport.Width / 2f, viewport.Height / 2f, 0);

            Center = center;
            Zoom = MAX_ZOOM;
            Rotation = 0f;
        }

        internal void Update()
        {
            MouseState newMouseState = Mouse.GetState();

            if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
            {
                Center.X += oldMouseState.X - newMouseState.X;
                Center.Y += oldMouseState.Y - newMouseState.Y;
            }

            int deltaZ = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

            Zoom += deltaZ * ZOOM_RATIO;

            if (Zoom < MAX_ZOOM) Zoom = MAX_ZOOM;
            else if (Zoom > MIN_ZOOM) Zoom = MIN_ZOOM;

            oldMouseState = newMouseState;

            if (Center.X < upLeftBound.X) Center.X = upLeftBound.X;
            else if (Center.X > downRightBound.X) Center.X = downRightBound.X;

            if (Center.Y < upLeftBound.Y) Center.Y = upLeftBound.Y;
            else if (Center.Y > downRightBound.Y) Center.Y = downRightBound.Y;

            Transform = Matrix.CreateTranslation(-Center.X, -Center.Y, 0) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateScale(Zoom) *
                        Matrix.CreateTranslation(_viewPoint);

            InverseTransform = Matrix.Invert(Transform);
        }

        internal void Begin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Transform);
        }

    }
}