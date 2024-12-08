using BitQuest._Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace BitQuest;

public class GameManager
{
    private GameState _currentGameState; // Tracks the current state of the game
    private readonly Ship _ship;
    private readonly SpriteFont _font;
    private int _score;
    private int _highScore;
    private PauseManager _pauseManager;
    // Song management
    private Song _mainMenuMusic;
    private Song _gameOverMusic;
    private Song _inGameMusic;

    

    public GameManager()
    {
        _currentGameState = GameState.MainMenu; // Start in the MainMenu state
        _font = Globals.Content.Load<SpriteFont>("font");
        _ship = new(Globals.Content.Load<Texture2D>("ship"));

        _pauseManager = new PauseManager();
        _pauseManager.LoadContent();

        _mainMenuMusic = Globals.Content.Load<Song>("MainMenuSound");  // Main Menu Music
        
        _gameOverMusic = Globals.Content.Load<Song>("PausedGameSound");  // Game Over Music
        
        _inGameMusic = Globals.Content.Load<Song>("MainGameSound");  // In-Game Music
        MediaPlayer.Volume = 0.35f;
        LoadHighScore();  // Load the high score from a file

        for (int i = 0; i < 5; i++)
        {
            EnemyManager.AddEnemy();
        }
    }

    // Save the high score to a text file
    private void SaveHighScore()
    {
        string path = "highscore.txt";  // Use a simple text file to store the high score
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(_highScore);  // Write the high score as an integer
        }
    }

    // Load the high score from a text file
    private void LoadHighScore()
    {
        string path = "highscore.txt";
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                if (int.TryParse(reader.ReadLine(), out int savedHighScore))
                {
                    _highScore = savedHighScore;
                }
                else
                {
                    _highScore = 0;  // Default to 0 if the file contents are not valid
                }
            }
        }
        else
        {
            _highScore = 0;  // Default to 0 if no high score file exists
        }
    }

    public void Restart()
    {
        _ship.Restart();
        EnemyManager.Restart();
        ProjectileManager.Restart();
        ExplosionManager.Restart();
        _score = 0;
    }

    public void HandleEnemyCollisions()
    {
        foreach (var enemy in EnemyManager.Enemies.ToArray())
        {
            foreach (var projectile in ProjectileManager.Projectiles.ToArray())
            {
                if (enemy.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                {
                    ExplosionManager.AddExplosion(enemy.Position);
                    EnemyManager.Enemies.Remove(enemy);
                    ProjectileManager.Projectiles.Remove(projectile);
                    _score++;
                    break;
                }
            }
        }
    }

    public void HandlePlayerCollision()
    {
        foreach (var enemy in EnemyManager.Enemies)
        {
            if (enemy.CollisionRectangle.Intersects(_ship.CollisionRectangle))
            {
                _currentGameState = GameState.GameOver; // Switch to Game Over state
                // Update high score if current score is greater
                if (_score > _highScore)
                {
                    _highScore = _score;  // Update high score
                    SaveHighScore();  // Save the new high score
                }
                break;
            }
        }

        foreach (var projectile in ProjectileManager.EnemyProjectiles)
        {
            if (projectile.CollisionRectangle.Intersects(_ship.CollisionRectangle))
            {
                _currentGameState = GameState.GameOver; // Switch to Game Over state
                // Update high score if current score is greater
                if (_score > _highScore)
                {
                    _highScore = _score;  // Update high score
                    SaveHighScore();  // Save the new high score
                }
                break;
            }
        }
    }
    public GameState GetCurrentGameState()
    {
        return _currentGameState;  // Return the current game state
    }
    private void UpdateMusic()
    {
        switch (_currentGameState)
        {
            case GameState.MainMenu:
                if (MediaPlayer.Queue.ActiveSong != _mainMenuMusic)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_mainMenuMusic);
                }
                break;

            case GameState.Paused:
                if (MediaPlayer.Queue.ActiveSong != _mainMenuMusic)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_mainMenuMusic);
                }
                break;

            case GameState.Playing:
                if (MediaPlayer.Queue.ActiveSong != _inGameMusic)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_inGameMusic);
                }
                break;

            case GameState.GameOver:
                if (MediaPlayer.Queue.ActiveSong != _gameOverMusic)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_gameOverMusic);
                }
                break;
        }
    }

    public void Update()
    {
        UpdateMusic();  // Check and update music for the current state

        switch (_currentGameState)
        {
            case GameState.Playing:
                _ship.Update();
                EnemyManager.UpdateEnemies();
                ProjectileManager.UpdateProjectiles();
                HandleEnemyCollisions();
                HandlePlayerCollision();
                ExplosionManager.UpdateExplosions();
                break;

            case GameState.GameOver:
                if (InputManager.IsKeyPressed(Keys.R))
                {
                    Restart();
                    _currentGameState = GameState.Playing;
                }
                else if (InputManager.IsKeyPressed(Keys.M))
                {
                    _currentGameState = GameState.MainMenu;
                }
                break;

            case GameState.MainMenu:
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    Restart();
                    _currentGameState = GameState.Playing;
                }
                break;

            case GameState.Paused:
                if (InputManager.IsKeyPressed(Keys.P))
                {
                    _currentGameState = GameState.Playing;
                }
                break;
        }
    }
    

    public void Draw()
    {
        Globals.SpriteBatch.Begin();

        switch (_currentGameState)
        {
            case GameState.Playing:
                EnemyManager.DrawEnemies();
                ProjectileManager.DrawProjectiles();
                ExplosionManager.DrawExplosions();
                _ship.Draw();
                Globals.SpriteBatch.DrawString(_font, $"Score: {_score}", Vector2.Zero, Color.White);
                break;

            case GameState.GameOver:
                string gameOverText = $"Game Over\nPress R to Restart\nPress M for Main Menu\nHigh Score: {_highScore}";
                Vector2 gameOverSize = _font.MeasureString(gameOverText);
                Vector2 gameOverPosition = (Globals.WindowSize.ToVector2() - gameOverSize) / 2;

                Globals.SpriteBatch.DrawString(_font, gameOverText, gameOverPosition, Color.Red);
                break;

            case GameState.MainMenu:
                string mainMenuText = $"Main Menu\nPress Enter to Start\nHigh Score: {_highScore}";
                Vector2 mainMenuSize = _font.MeasureString(mainMenuText);
                Vector2 mainMenuPosition = (Globals.WindowSize.ToVector2() - mainMenuSize) / 2;

                Globals.SpriteBatch.DrawString(_font, mainMenuText, mainMenuPosition, Color.White);
                break;
        }

        Globals.SpriteBatch.End();
    }
}
