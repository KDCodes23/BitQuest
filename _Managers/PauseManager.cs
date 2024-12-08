using BitQuest._Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace BitQuest
{
    public class PauseManager
    {
        private bool _isPaused; // Track pause state
        private SpriteFont _font; // Font for pause text
        private Song _pauseMusic; // Pause music
        private GameState _previousGameState; // To restore the game state after pausing

        public PauseManager()
        {
            _isPaused = false;
        }

        public void LoadContent()
        {
            _font = Globals.Content.Load<SpriteFont>("font"); // Load font
            _pauseMusic = Globals.Content.Load<Song>("PausedGameSound"); // Pause music
        }

        public bool IsPaused => _isPaused;

        public void Update(GameState currentGameState)
        {
            // Check for the "P" key press to toggle pause
            if (InputManager.IsKeyPressed(Keys.P))
            {
                _isPaused = !_isPaused;

                if (_isPaused)
                {
                    _previousGameState = currentGameState; // Save the current game state
                    MediaPlayer.Pause(); // Pause the current music
                    MediaPlayer.Play(_pauseMusic); // Play the pause music
                }
                else
                {
                    MediaPlayer.Stop(); // Stop pause music
                    MediaPlayer.Resume(); // Resume the original music
                }
            }
        }

        public void Draw()
        {
            if (_isPaused)
            {
                Globals.SpriteBatch.Begin();

                // Draw a semi-transparent overlay for the pause screen
                Globals.SpriteBatch.Draw(Globals.Content.Load<Texture2D>("pauseBackground"), new Rectangle(0, 0, Globals.WindowSize.X, Globals.WindowSize.Y), Color.Black * 0.5f);

                // Draw "Game Paused" text
                string pauseText = "Game Paused\nPress P to Resume";
                Vector2 textSize = _font.MeasureString(pauseText);
                Vector2 position = (Globals.WindowSize.ToVector2() - textSize) / 2;

                Globals.SpriteBatch.DrawString(_font, pauseText, position, Color.White);

                Globals.SpriteBatch.End();
            }
        }
    }
}
