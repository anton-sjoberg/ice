using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Ice
{
    public class Game1 : Game 
    {
        // Hanterar grafikinställningar och rendering
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Spelkomponenter
        private Player _player;
        private TileMap _tileMap;
        private LevelManager _levelManager;
        private ParticleManager _particleManager;
        private TimeToCompleteManager _timeToCompleteManager;
        private UIManager _uiManager;

        private float frameTime; // Variabel för att lagra tid mellan frames
        private Song backgroundMusic; // Bakgrundsmusik för spelet

        // Konstruktor, sätter upp grundläggande grafikinställningar
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920; // Skärmupplösning bredd
            _graphics.PreferredBackBufferHeight = 1080; // Skärmupplösning höjd
            _graphics.IsFullScreen = true; // Fullskärmsläge
            _graphics.SynchronizeWithVerticalRetrace = false; // VSync av
            IsFixedTimeStep = false; // Ej fast uppdateringsintervall
            IsMouseVisible = true; // Visa muspekare
            Content.RootDirectory = "Content"; // Sökväg till innehåll
        }

        // Initierar spelet (körs en gång vid start)
        protected override void Initialize()
        {
            base.Initialize();
        }

        // Laddar allt innehåll och initierar alla komponenter
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Ladda och försöker spela bakgrundsmusik, annars fångar undantag om ingen ljudhårdvara finns
            try
            {
                backgroundMusic = Content.Load<Song>("Sounds/Arcadia");

                MediaPlayer.Play(backgroundMusic);
                MediaPlayer.IsRepeating = true;
            }
            catch (Microsoft.Xna.Framework.Audio.NoAudioHardwareException)
            {
                Console.WriteLine("No audio hardware found.");
            }

            // Skapa och initiera alla spelkomponenter
            _tileMap = new TileMap(GraphicsDevice, _spriteBatch);
            _player = new Player(GraphicsDevice, _spriteBatch, _tileMap, null, null);
            _particleManager = new ParticleManager(GraphicsDevice, _spriteBatch, _player, _tileMap, null);
            _levelManager = new LevelManager(GraphicsDevice, _player, _tileMap, _particleManager);
            _particleManager.SetNullManagers(_levelManager);
            _player.SetNullManagers(_particleManager, _levelManager);
            _timeToCompleteManager = new TimeToCompleteManager(_player, _levelManager);
            _uiManager = new UIManager(GraphicsDevice, _spriteBatch, _player, _levelManager, _particleManager, _timeToCompleteManager);

            // Initiera och ladda innehåll för varje komponent
            _player.Initialize();
            _particleManager.Initialize();
            _levelManager.Initialize();
            _tileMap.Initialize();
            _timeToCompleteManager.Initialize();
            _uiManager.Initialize();

            _player.LoadContent(Content);
            _tileMap.LoadContent(Content);
            _particleManager.LoadContent(Content);
            _uiManager.LoadContent(Content);
        }

        // Uppdaterar spelets logik varje frame
        protected override void Update(GameTime gameTime)
        {
            frameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Uppdatera spelaren endast om spelet är aktivt och inte pausat
            if (_levelManager.gameActive && !_levelManager.gamePaused)
            {
                _player.Update(gameTime, frameTime);
            }

            // Uppdatera partiklar även i vissa UI-lägen
            if (!_levelManager.gamePaused || _uiManager.settingsActive || _uiManager.howToPlayActive) _particleManager.Update(gameTime, frameTime);
            _levelManager.Update(gameTime);
            _timeToCompleteManager.Update(gameTime);
            _uiManager.Update(gameTime);

            base.Update(gameTime);
        }

        // Renderar allt grafiskt innehåll varje frame
        protected override void Draw(GameTime gameTime)
        {
            // Lägger bakgrunden till en ljusblå färg
            GraphicsDevice.Clear(new Color(0xE5, 0xEF, 0xFF));

            // Partiklar och UI ritas alltid, spelare och tilemap ritas endast om spelet är aktivt och inte i inställnings- eller instruktionstillstånd
            _particleManager.Draw(gameTime);
            if (_levelManager.gameActive && !_uiManager.settingsActive && !_uiManager.howToPlayActive) _player.Draw(gameTime);
            if (_levelManager.gameActive && !_uiManager.settingsActive && !_uiManager.howToPlayActive) _tileMap.Draw(gameTime);
            _uiManager.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
