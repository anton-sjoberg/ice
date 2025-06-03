using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.IO;

namespace Ice
{
    // Klass för knapphantering i UI:t
    internal class Button
    {
        public Texture2D TextureUp, TextureDown, Texture;
        public Color ButtonColor;
        public Rectangle ButtonRect;
        public Vector2 Position;
        public float Width, Height, Scale;
        public bool MouseDown, MouseDownBeforeContact;

        // Skapar en knapp med texturer, position och färg
        public Button(Texture2D textureUp, Texture2D textureDown, Vector2 position, float height, Color color)
        {
            TextureUp = textureUp;
            TextureDown = textureDown;
            Texture = textureUp;
            Position = position;
            Height = height;
            Scale = height / textureUp.Height;
            Width = textureUp.Width * Scale;
            ButtonColor = color;
            MouseDown = false;
            MouseDownBeforeContact = false;
            UpdateRect();
        }

        // Uppdaterar knappens rektangel (för kollision)
        public void UpdateRect()
        {
            ButtonRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }

        // Sätter ny position för knappen
        public void SetPosition(Vector2 position)
        {
            Position = position;
            UpdateRect();
        }

        // Sätter ny skala/höjd för knappen
        public void SetScale(float height)
        {
            Height = height;
            Scale = height / TextureUp.Height;
            Width = TextureUp.Width * Scale;
            UpdateRect();
        }
    }

    // Klassen hanterar allt UI i spelet (knappar, texter, menyer)
    internal class UIManager
    {
        // Konstruktor som tar emot nödvändiga managers och grafikobjekt
        public UIManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Player player, LevelManager levelManager, ParticleManager particleManager, TimeToCompleteManager timeToCompleteManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _player = player;
            _levelManager = levelManager;
            _particleManager = particleManager;
            _timeToCompleteManager = timeToCompleteManager;
        }

        // Grafik och managers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private Player _player;
        private LevelManager _levelManager;
        private ParticleManager _particleManager;
        private TimeToCompleteManager _timeToCompleteManager;

        // UI-variabler och resurser
        private float timeSinceLastFPSUpdate;
        private Texture2D crown, gameTitle, howToPlay;
        private Texture2D playButtonUp, playButtonDown;
        private Texture2D playAgainButtonUp, playAgainButtonDown;
        private Texture2D settingsButtonUp, settingsButtonDown;
        private Texture2D backButtonUp, backButtonDown;
        private Texture2D exitButtonUp, exitButtonDown;
        private Texture2D showFPSButtonUncheckedUp, showFPSButtonUncheckedDown, showFPSButtonCheckedUp, showFPSButtonCheckedDown;
        private Texture2D showDebugTextButtonUncheckedUp, showDebugTextButtonUncheckedDown, showDebugTextButtonCheckedUp, showDebugTextButtonCheckedDown;
        private Texture2D pauseButtonUp, pauseButtonDown;
        private Texture2D mainMenuButtonUp, mainMenuButtonDown;
        private Texture2D howToPlayButtonUp, howToPlayButtonDown;
        private SpriteFont smallText, mediumText, largeText;
        private KeyboardState keyboardState;
        private MouseState mouseState;
        private int frameCounter, fps, buttonHeight, titleHeight, howToPlayHeight;
        private string configFilePath, fpsText, positionXText, positionYText, velocityXText, velocityYText, particleCountText, timeToCompleteText, fastestTimeText, gameFinishedText;
        private bool canContinue, canEscape, showDebugText, showFPS;
        private List<Button> buttons;
        private Button playButton, playAgainButton, settingsButton, backButton, exitButton, showFPSButton, showDebugTextButton, pauseButton, mainMenuButton, howToPlayButton;

        public bool settingsActive, howToPlayActive;

        // Initierar UI:ts startvärden och laddar konfiguration
        public void Initialize()
        {
            buttonHeight = 100;
            titleHeight = 300;
            howToPlayHeight = 800;

            showDebugText = false;
            showFPS = true;

            InitializeConfigPath();
            LoadConfig();

            buttons = new List<Button>();
        }

        // Laddar UI-resurser (texturer, fonter, knappar)
        public void LoadContent(ContentManager content)
        {
            smallText = content.Load<SpriteFont>("SpriteFonts/Small Text");
            mediumText = content.Load<SpriteFont>("SpriteFonts/Medium Text");
            largeText = content.Load<SpriteFont>("SpriteFonts/Large Text");
            crown = content.Load<Texture2D>("Sprites/Crown");
            gameTitle = content.Load<Texture2D>("Sprites/Game Title");
            howToPlay = content.Load<Texture2D>("Sprites/How To Play");
            playButtonUp = content.Load<Texture2D>("Sprites/Play Button Up");
            playButtonDown = content.Load<Texture2D>("Sprites/Play Button Down");
            playAgainButtonUp = content.Load<Texture2D>("Sprites/Play Again Button Up");
            playAgainButtonDown = content.Load<Texture2D>("Sprites/Play Again Button Down");
            settingsButtonUp = content.Load<Texture2D>("Sprites/Settings Button Up");
            settingsButtonDown = content.Load<Texture2D>("Sprites/Settings Button Down");
            backButtonUp = content.Load<Texture2D>("Sprites/Back Button Up");
            backButtonDown = content.Load<Texture2D>("Sprites/Back Button Down");
            exitButtonUp = content.Load<Texture2D>("Sprites/Exit Button Up");
            exitButtonDown = content.Load<Texture2D>("Sprites/Exit Button Down");
            showFPSButtonUncheckedUp = content.Load<Texture2D>("Sprites/Show FPS Button Unchecked Up");
            showFPSButtonUncheckedDown = content.Load<Texture2D>("Sprites/Show FPS Button Unchecked Down");
            showFPSButtonCheckedUp = content.Load<Texture2D>("Sprites/Show FPS Button Checked Up");
            showFPSButtonCheckedDown = content.Load<Texture2D>("Sprites/Show FPS Button Checked Down");
            showDebugTextButtonUncheckedUp = content.Load<Texture2D>("Sprites/Show Debug Text Button Unchecked Up");
            showDebugTextButtonUncheckedDown = content.Load<Texture2D>("Sprites/Show Debug Text Button Unchecked Down");
            showDebugTextButtonCheckedUp = content.Load<Texture2D>("Sprites/Show Debug Text Button Checked Up");
            showDebugTextButtonCheckedDown = content.Load<Texture2D>("Sprites/Show Debug Text Button Checked Down");
            pauseButtonUp = content.Load<Texture2D>("Sprites/Pause Button Up");
            pauseButtonDown = content.Load<Texture2D>("Sprites/Pause Button Down");
            mainMenuButtonUp = content.Load<Texture2D>("Sprites/Main Menu Button Up");
            mainMenuButtonDown = content.Load<Texture2D>("Sprites/Main Menu Button Down");
            howToPlayButtonUp = content.Load<Texture2D>("Sprites/How To Play Button Up");
            howToPlayButtonDown = content.Load<Texture2D>("Sprites/How To Play Button Down");

            // Skapar alla knappar och lägger till i listan
            playButton = new Button(playButtonUp, playButtonDown, Vector2.Zero, buttonHeight, Color.White);
            playAgainButton = new Button(playAgainButtonUp, playAgainButtonDown, Vector2.Zero, buttonHeight, Color.White);
            settingsButton = new Button(settingsButtonUp, settingsButtonDown, Vector2.Zero, buttonHeight, Color.White);
            backButton = new Button(backButtonUp, backButtonDown, Vector2.Zero, buttonHeight, Color.White);
            exitButton = new Button(exitButtonUp, exitButtonDown, Vector2.Zero, buttonHeight, Color.White);
            showFPSButton = new Button(showFPS ? showFPSButtonCheckedUp : showFPSButtonUncheckedUp, showFPS ? showFPSButtonCheckedDown : showFPSButtonUncheckedDown, Vector2.Zero, buttonHeight, Color.White);
            showDebugTextButton = new Button(showDebugText ? showDebugTextButtonCheckedUp : showDebugTextButtonUncheckedUp, showDebugText ? showDebugTextButtonCheckedDown : showDebugTextButtonUncheckedDown, Vector2.Zero, buttonHeight, Color.White);
            pauseButton = new Button(pauseButtonUp, pauseButtonDown, Vector2.Zero, buttonHeight, Color.White);
            mainMenuButton = new Button(mainMenuButtonUp, mainMenuButtonDown, Vector2.Zero, buttonHeight, Color.White);
            howToPlayButton = new Button(howToPlayButtonUp, howToPlayButtonDown, Vector2.Zero, buttonHeight, Color.White);

            buttons.Add(playButton);
            buttons.Add(playAgainButton);
            buttons.Add(settingsButton);
            buttons.Add(backButton);
            buttons.Add(exitButton);
            buttons.Add(showFPSButton);
            buttons.Add(showDebugTextButton);
            buttons.Add(pauseButton);
            buttons.Add(mainMenuButton);
            buttons.Add(howToPlayButton);

            UpdateButtonPositions();
        }

        // Uppdaterar positionerna för alla knappar beroende på skärmstorlek
        private void UpdateButtonPositions()
        {
            Vector2 playPosition = new Vector2((_graphicsDevice.Viewport.Width - playButton.Width) * 0.5f, 600);
            playButton.SetPosition(playPosition);

            Vector2 playAgainPosition = new Vector2((_graphicsDevice.Viewport.Width - playAgainButton.Width) * 0.5f, (_graphicsDevice.Viewport.Height - playAgainButton.Height) * 0.5f);
            playAgainButton.SetPosition(playAgainPosition);

            Vector2 settingsPosition = new Vector2((_graphicsDevice.Viewport.Width - settingsButton.Width) * 0.5f, 740);
            settingsButton.SetPosition(settingsPosition);

            Vector2 backPosition = new Vector2(20, 40);
            backButton.SetPosition(backPosition);

            Vector2 exitPosition = new Vector2(20, 40);
            exitButton.SetPosition(exitPosition);

            Vector2 showFPSPosition = new Vector2(20, 190);
            showFPSButton.SetPosition(showFPSPosition);

            Vector2 showDebugTextPosition = new Vector2(20, 300);
            showDebugTextButton.SetPosition(showDebugTextPosition);

            Vector2 pausePosition = new Vector2(20, 40);
            pauseButton.SetPosition(pausePosition);

            Vector2 mainMenuPosition = new Vector2((_graphicsDevice.Viewport.Width - mainMenuButton.Width) * 0.5f, 720);
            mainMenuButton.SetPosition(mainMenuPosition);

            Vector2 howToPlayPosition = new Vector2((_graphicsDevice.Viewport.Width - howToPlayButton.Width) * 0.5f, 880);
            howToPlayButton.SetPosition(howToPlayPosition);
        }

        // Initierar sökvägen till konfigurationsfilen och skapar mapp om den inte finns
        private void InitializeConfigPath()
        {
            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ice", "Config.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
        }

        // Läser in konfigurationsfilen och sätter inställningar
        private void LoadConfig()
        {
            if (!File.Exists(configFilePath))
            {
                SaveConfig();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(configFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (key == "ShowFPS") showFPS = bool.Parse(value);
                    else if (key == "ShowDebugText") showDebugText = bool.Parse(value);
                }
            }
            catch
            {
                // Ignorerar eventuella fel vid inläsning
            }
        }

        // Sparar nuvarande inställningar till konfigurationsfilen
        private void SaveConfig()
        {
            var lines = new[]
            {
                    $"ShowFPS={showFPS}",
                    $"ShowDebugText={showDebugText}"
                };
            File.WriteAllLines(configFilePath, lines);
        }

        // Räknar ut FPS och uppdaterar texten
        private void FPSCounter(GameTime gameTime)
        {
            timeSinceLastFPSUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter++;

            if (timeSinceLastFPSUpdate >= 0.5f)
            {
                fps = (int)(frameCounter / timeSinceLastFPSUpdate);
                fpsText = $"{fps} FPS";
                frameCounter = 0;
                timeSinceLastFPSUpdate = 0;
            }
        }

        // Sätter alla texter som visas i UI:t (debug, tider, med mera)
        private void SetStrings()
        {
            if (showDebugText)
            {
                positionXText = $"X Position: {(int)_player.position.X}";
                positionYText = $"Y Position: {(int)_player.position.Y}";

                velocityXText = $"X Velocity: {(int)_player.averageVelocity.X}";
                velocityYText = $"Y Velocity: {(int)_player.averageVelocity.Y}";

                int particleCount = _particleManager.snowflakes.Count + _particleManager.chips.Count;
                if (_particleManager._darkHalf != null) particleCount++;
                if (_particleManager._lightHalf != null) particleCount++;
                particleCountText = $"Particle count: {particleCount}";
            }

            timeToCompleteText = $"{_timeToCompleteManager.timeToComplete.ToString("F2", CultureInfo.InvariantCulture)} s";
            fastestTimeText = _timeToCompleteManager.fastestTime == float.MaxValue ? "None" : $"{_timeToCompleteManager.fastestTime.ToString("F2", CultureInfo.InvariantCulture)} s";

            gameFinishedText = _timeToCompleteManager.beatFastestTime ? $"You recorded a new fastest time of {_timeToCompleteManager.oldTimeToComplete.ToString("F2", CultureInfo.InvariantCulture)} seconds!" : $"Game finished in {_timeToCompleteManager.oldTimeToComplete.ToString("F2", CultureInfo.InvariantCulture)} seconds!";
        }

        // Kollar om Enter har tryckts för att fortsätta eller starta om
        private void CheckForEnter()
        {
            if (keyboardState.IsKeyDown(Keys.Enter) && canContinue)
            {
                canContinue = false;

                if (!_levelManager.gameFinished) _levelManager.gameStarted = true;
                else
                {
                    _levelManager.gameFinished = false;
                    _levelManager.gameStarted = false;
                }
            }
            if (keyboardState.IsKeyUp(Keys.Enter)) canContinue = true;
        }

        // Kollar om Escape har tryckts för att öppna/stänga menyer
        private void CheckForEscape()
        {
            if (keyboardState.IsKeyDown(Keys.Escape) && canEscape)
            {
                canEscape = false;

                if (settingsActive) settingsActive = false;
                else if (howToPlayActive) howToPlayActive = false;
                else if (_levelManager.gameActive) _levelManager.gamePaused = !_levelManager.gamePaused;
            }
            if (keyboardState.IsKeyUp(Keys.Escape)) canEscape = true;
        }

        // Hanterar enskild knapp (tryck, släpp, klick)
        private void ManageButton(Button button, System.Action onClick)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (button.ButtonRect.Contains(mouseState.Position))
                {
                    if (!button.MouseDownBeforeContact)
                    {
                        button.MouseDown = true;
                        button.Texture = button.TextureDown;
                    }
                }
                else if (!button.MouseDown)
                {
                    button.MouseDownBeforeContact = true;
                    button.Texture = button.TextureUp;
                }
                else button.Texture = button.TextureUp;
            }
            else
            {
                button.MouseDownBeforeContact = false;
                button.Texture = button.TextureUp;
            }

            if (button.MouseDown)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    if (button.ButtonRect.Contains(mouseState.Position))
                    {
                        if (onClick != null) onClick();
                    }
                    button.MouseDown = false;
                }
            }
        }

        // Hanterar alla knappar beroende på speltillstånd och menyer
        private void ManageButtons()
        {
            if (_levelManager.gameActive && !settingsActive && !howToPlayActive)
            {
                ManageButton(pauseButton, delegate { _levelManager.gamePaused = !_levelManager.gamePaused; });

                if (_levelManager.gamePaused)
                {
                    Vector2 playPosition = new Vector2((_graphicsDevice.Viewport.Width - playButton.Width) * 0.5f, 300);
                    playButton.SetPosition(playPosition);
                    ManageButton(playButton, delegate { _levelManager.gamePaused = false; });

                    Vector2 settingsPosition = new Vector2((_graphicsDevice.Viewport.Width - settingsButton.Width) * 0.5f, 440);
                    settingsButton.SetPosition(settingsPosition);
                    ManageButton(settingsButton, delegate { settingsActive = true; });

                    Vector2 howToPlayPosition = new Vector2((_graphicsDevice.Viewport.Width - howToPlayButton.Width) * 0.5f, 580);
                    howToPlayButton.SetPosition(howToPlayPosition);
                    ManageButton(howToPlayButton, delegate { howToPlayActive = true; });

                    ManageButton(mainMenuButton, delegate
                    {
                        _levelManager.ReturnToMainMenu();
                        _timeToCompleteManager.timeToComplete = 0;
                    });
                }
            }
            else
            {
                if (!_levelManager.gameStarted && !settingsActive && !howToPlayActive)
                {
                    Vector2 playPosition = new Vector2((_graphicsDevice.Viewport.Width - playButton.Width) * 0.5f, 600);
                    playButton.SetPosition(playPosition);
                    ManageButton(playButton, delegate { _levelManager.gameStarted = true; });

                    Vector2 settingsPosition = new Vector2((_graphicsDevice.Viewport.Width - settingsButton.Width) * 0.5f, 740);
                    settingsButton.SetPosition(settingsPosition);
                    ManageButton(settingsButton, delegate { settingsActive = true; });

                    Vector2 howToPlayPosition = new Vector2((_graphicsDevice.Viewport.Width - howToPlayButton.Width) * 0.5f, 880);
                    howToPlayButton.SetPosition(howToPlayPosition);
                    ManageButton(howToPlayButton, delegate { howToPlayActive = true; });

                    ManageButton(exitButton, delegate { Environment.Exit(0); });
                }

                if (_levelManager.gameFinished)
                {
                    ManageButton(playAgainButton, delegate
                    {
                        _levelManager.gameFinished = false;
                        _levelManager.gameStarted = false;
                    });
                }
            }

            if (settingsActive)
            {
                ManageButton(backButton, delegate { settingsActive = false; });

                ManageButton(showFPSButton, delegate
                {
                    showFPS = !showFPS;

                    showFPSButton.TextureUp = showFPS ? showFPSButtonCheckedUp : showFPSButtonUncheckedUp;
                    showFPSButton.TextureDown = showFPS ? showFPSButtonCheckedDown : showFPSButtonUncheckedDown;

                    SaveConfig();
                });
                ManageButton(showDebugTextButton, delegate
                {
                    showDebugText = !showDebugText;

                    showDebugTextButton.TextureUp = showDebugText ? showDebugTextButtonCheckedUp : showDebugTextButtonUncheckedUp;
                    showDebugTextButton.TextureDown = showDebugText ? showDebugTextButtonCheckedDown : showDebugTextButtonUncheckedDown;

                    SaveConfig();
                });
            }

            if (howToPlayActive)
            {
                ManageButton(backButton, delegate { howToPlayActive = false; });
            }
        }

        // Uppdaterar UI varje frame (knappar, texter, menyer)
        public void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            UpdateButtonPositions();

            if (!settingsActive && !howToPlayActive) CheckForEnter();
            CheckForEscape();
            ManageButtons();
            if (showFPS) FPSCounter(gameTime);
            SetStrings();
        }

        // Ritar UI:t på skärmen beroende på speltillstånd och menyer
        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            // Ritar FPS-räknare om aktiv
            if (!string.IsNullOrEmpty(fpsText) && showFPS)
            {
                Vector2 fpsPos = new Vector2(10, 10);
                _spriteBatch.DrawString(smallText, fpsText, fpsPos, Color.Black);
            }

            if (_levelManager.gameActive)
            {
                // Ritar debugtext om aktiv
                if (showDebugText)
                {
                    Vector2 posX = new Vector2(10, _graphicsDevice.Viewport.Height - smallText.MeasureString(positionXText).Y - 94);
                    Vector2 posY = new Vector2(10, _graphicsDevice.Viewport.Height - smallText.MeasureString(positionYText).Y - 76);
                    Vector2 velX = new Vector2(10, _graphicsDevice.Viewport.Height - smallText.MeasureString(velocityXText).Y - 50);
                    Vector2 velY = new Vector2(10, _graphicsDevice.Viewport.Height - smallText.MeasureString(velocityYText).Y - 32);
                    Vector2 particleCount = new Vector2(10, _graphicsDevice.Viewport.Height - smallText.MeasureString(particleCountText).Y - 6);

                    _spriteBatch.DrawString(smallText, positionXText, posX, Color.Black);
                    _spriteBatch.DrawString(smallText, positionYText, posY, Color.Black);
                    _spriteBatch.DrawString(smallText, velocityXText, velX, Color.Black);
                    _spriteBatch.DrawString(smallText, velocityYText, velY, Color.Black);
                    _spriteBatch.DrawString(smallText, particleCountText, particleCount, Color.Black);
                }

                // Ritar tidtagning och snabbaste tid
                float timeToCompleteX = (_graphicsDevice.Viewport.Width - mediumText.MeasureString(timeToCompleteText).X) * 0.5f;
                Vector2 timeToCompletePos = new Vector2(timeToCompleteX, 10);
                _spriteBatch.DrawString(mediumText, timeToCompleteText, timeToCompletePos, Color.Black);

                float crownScale = 22f / crown.Height;
                float fastestTimeWidth = mediumText.MeasureString(fastestTimeText).X;
                float crownX = _graphicsDevice.Viewport.Width - crown.Width * crownScale - fastestTimeWidth - 20;
                float crownY = 12;
                Vector2 crownPos = new Vector2(crownX, crownY);

                float fastestTimeX = _graphicsDevice.Viewport.Width - fastestTimeWidth - 10;
                Vector2 fastestTimePos = new Vector2(fastestTimeX, 10);

                _spriteBatch.Draw(crown, crownPos, null, Color.White, 0f, Vector2.Zero, new Vector2(crownScale), SpriteEffects.None, 0f);
                _spriteBatch.DrawString(mediumText, fastestTimeText, fastestTimePos, Color.Black);

                // Ritar knappar och menyer beroende på pausmeny
                if (!settingsActive && !howToPlayActive)
                {
                    _spriteBatch.Draw(pauseButton.Texture, pauseButton.Position, null, pauseButton.ButtonColor, 0f, Vector2.Zero, new Vector2(pauseButton.Scale), SpriteEffects.None, 0f);

                    if (_levelManager.gamePaused)
                    {
                        _spriteBatch.Draw(playButton.Texture, playButton.Position, null, playButton.ButtonColor, 0f, Vector2.Zero, new Vector2(playButton.Scale), SpriteEffects.None, 0f);
                        _spriteBatch.Draw(settingsButton.Texture, settingsButton.Position, null, settingsButton.ButtonColor, 0f, Vector2.Zero, new Vector2(settingsButton.Scale), SpriteEffects.None, 0f);
                        _spriteBatch.Draw(howToPlayButton.Texture, howToPlayButton.Position, null, howToPlayButton.ButtonColor, 0f, Vector2.Zero, new Vector2(howToPlayButton.Scale), SpriteEffects.None, 0f);
                        _spriteBatch.Draw(mainMenuButton.Texture, mainMenuButton.Position, null, mainMenuButton.ButtonColor, 0f, Vector2.Zero, new Vector2(mainMenuButton.Scale), SpriteEffects.None, 0f);
                    }
                }
            }
            else
            {
                // Ritar huvudmeny om spelet inte är startat
                if (!_levelManager.gameStarted && !settingsActive && !howToPlayActive)
                {
                    float titleScale = (float)titleHeight / gameTitle.Height;
                    Vector2 titlePos = new Vector2((_graphicsDevice.Viewport.Width - gameTitle.Width * titleScale) * 0.5f, 200);

                    _spriteBatch.Draw(gameTitle, titlePos, null, Color.White, 0f, Vector2.Zero, new Vector2(titleScale), SpriteEffects.None, 0f);

                    _spriteBatch.Draw(playButton.Texture, playButton.Position, null, playButton.ButtonColor, 0f, Vector2.Zero, new Vector2(playButton.Scale), SpriteEffects.None, 0f);
                    _spriteBatch.Draw(settingsButton.Texture, settingsButton.Position, null, settingsButton.ButtonColor, 0f, Vector2.Zero, new Vector2(settingsButton.Scale), SpriteEffects.None, 0f);
                    _spriteBatch.Draw(howToPlayButton.Texture, howToPlayButton.Position, null, howToPlayButton.ButtonColor, 0f, Vector2.Zero, new Vector2(howToPlayButton.Scale), SpriteEffects.None, 0f);
                    _spriteBatch.Draw(exitButton.Texture, exitButton.Position, null, exitButton.ButtonColor, 0f, Vector2.Zero, new Vector2(exitButton.Scale), SpriteEffects.None, 0f);
                }

                // Ritar "spela igen"-knapp och resultattext om spelet är klart
                if (_levelManager.gameFinished)
                {
                    _spriteBatch.Draw(playAgainButton.Texture, playAgainButton.Position, null, playAgainButton.ButtonColor, 0f, Vector2.Zero, new Vector2(playAgainButton.Scale), SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(largeText, gameFinishedText, new Vector2((_graphicsDevice.Viewport.Width - largeText.MeasureString(gameFinishedText).X) * 0.5f, 400), Color.Black);
                }
            }

            // Ritar inställningsmeny om aktiv
            if (settingsActive)
            {
                _spriteBatch.Draw(backButton.Texture, backButton.Position, null, backButton.ButtonColor, 0f, Vector2.Zero, new Vector2(backButton.Scale), SpriteEffects.None, 0f);
                _spriteBatch.Draw(showFPSButton.Texture, showFPSButton.Position, null, showFPSButton.ButtonColor, 0f, Vector2.Zero, new Vector2(showFPSButton.Scale), SpriteEffects.None, 0f);
                _spriteBatch.Draw(showDebugTextButton.Texture, showDebugTextButton.Position, null, showDebugTextButton.ButtonColor, 0f, Vector2.Zero, new Vector2(showDebugTextButton.Scale), SpriteEffects.None, 0f);
            }

            // Ritar "hur man spelar"-meny om aktiv
            if (howToPlayActive)
            {
                float howToPlayScale = (float)howToPlayHeight / howToPlay.Height;
                Vector2 howToPlayPos = new Vector2((_graphicsDevice.Viewport.Width - howToPlay.Width * howToPlayScale) * 0.5f, (_graphicsDevice.Viewport.Height - howToPlay.Height * howToPlayScale) * 0.5f);

                _spriteBatch.Draw(howToPlay, howToPlayPos, null, Color.White, 0f, Vector2.Zero, new Vector2(howToPlayScale), SpriteEffects.None, 0f);

                _spriteBatch.Draw(backButton.Texture, backButton.Position, null, backButton.ButtonColor, 0f, Vector2.Zero, new Vector2(backButton.Scale), SpriteEffects.None, 0f);
            }

            _spriteBatch.End();
        }
    }
}
