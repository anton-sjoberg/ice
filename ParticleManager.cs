using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Ice
{
    // Klass som representerar en snöflinga (partikel)
    internal class Snowflake
    {
        public Vector2 Position;
        public float Velocity;
        public float Opacity;

        public Snowflake(Vector2 position, float velocity, float opacity)
        {
            Position = position;
            Velocity = velocity;
            Opacity = opacity;
        }
    }

    // Klass som representerar en chip-partikel (t.ex. vid hopp)
    internal class Chip
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Rotation;
        public float Opacity;
        public float Scale;

        public Chip(Texture2D texture, Vector2 position, float rotation, float opacity, float scale)
        {
            Texture = texture;
            Position = position;
            Rotation = rotation;
            Opacity = opacity;
            Scale = scale;
        }
    }

    // Partikelhalva (mörk) vid spelarens död
    internal class DarkHalf
    {
        public Vector2 Position, Velocity;
        public float Rotation, RotationSpeed;
        public float Opacity;

        public DarkHalf(Vector2 position, Vector2 velocity, float rotation, float rotationSpeed, float opacity)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            RotationSpeed = rotationSpeed;
            Opacity = opacity;
        }
    }

    // Partikelhalva (ljus) vid spelarens död
    internal class LightHalf
    {
        public Vector2 Position, Velocity;
        public float Rotation, RotationSpeed;
        public float Opacity;

        public LightHalf(Vector2 position, Vector2 velocity, float rotation, float rotationSpeed, float opacity)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            RotationSpeed = rotationSpeed;
            Opacity = opacity;
        }
    }

    // Hanterar alla partiklar i spelet (snö, chips, dödseffekter)
    internal class ParticleManager
    {
        // Konstruktor, tar emot referenser till managers och grafik
        public ParticleManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Player player, TileMap tileMap, LevelManager levelManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _player = player;
            _tileMap = tileMap;
            _levelManager = levelManager;
        }

        // Grafik och managers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private Player _player;
        private TileMap _tileMap;
        private LevelManager _levelManager;

        // Texturer och skalor för partiklar
        private Texture2D snowflake, squareChip, triangularChip, darkHalf, lightHalf;
        private Vector2 snowflakeScale, chipSize, chipScale, darkHalfScale, lightHalfScale;
        private float timeSinceLastSnowflake, timeSinceLastChip;
        private Random random = new Random();

        public int snowflakeSize, deathParticleSize;
        public List<Snowflake> snowflakes = new List<Snowflake>();
        public List<Chip> chips = new List<Chip>();
        public DarkHalf _darkHalf;
        public LightHalf _lightHalf;

        // Initierar partikelstorlekar
        public void Initialize()
        {
            snowflakeSize = 12;
            chipSize.Y = 18;
            deathParticleSize = _player.playerSize;
        }

        // Laddar partikeltexturer
        public void LoadContent(ContentManager content)
        {
            snowflake = content.Load<Texture2D>("Particles/Snowflake");
            squareChip = content.Load<Texture2D>("Particles/Square Chip");
            triangularChip = content.Load<Texture2D>("Particles/Triangular Chip");
            darkHalf = content.Load<Texture2D>("Particles/Dark Half");
            lightHalf = content.Load<Texture2D>("Particles/Light Half");
        }

        // Sätter referens till LevelManager efter skapande
        public void SetNullManagers(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        // Hjälpmetod: Kollar om en cirkel (partikel) kolliderar med en rektangel (tile)
        private bool CircleIntersectsRectangle(Vector2 center, float radius, Rectangle rect)
        {
            float closestX = Math.Clamp(center.X, rect.Left, rect.Right);
            float closestY = Math.Clamp(center.Y, rect.Top, rect.Bottom);
            float dx = center.X - closestX;
            float dy = center.Y - closestY;
            return (dx * dx + dy * dy) < (radius * radius);
        }

        // Hjälpmetod: Kollar om en cirkel (partikel) kolliderar med en triangel (tile)
        private bool CircleIntersectsTriangle(Vector2 center, float radius, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            if (PointInTriangle(center, p1, p2, p3))
                return true;

            if (CircleIntersectsLine(center, radius, p1, p2)) return true;
            if (CircleIntersectsLine(center, radius, p2, p3)) return true;
            if (CircleIntersectsLine(center, radius, p3, p1)) return true;

            return false;
        }

        // Hjälpmetod till CircleIntersectsTriangle: Kollar om en cirkel (partikel) kolliderar med en linje
        private bool CircleIntersectsLine(Vector2 center, float radius, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ac = center - a;
            float t = Vector2.Dot(ac, ab) / ab.LengthSquared();
            t = Math.Clamp(t, 0, 1);
            Vector2 closest = a + t * ab;
            return Vector2.DistanceSquared(center, closest) < radius * radius;
        }

        // Hjälpmetod till CircleIntersectsTriangle: Kollar om en punkt ligger inom en triangel
        private bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float dX = pt.X - v3.X;
            float dY = pt.Y - v3.Y;
            float dX21 = v3.X - v2.X;
            float dY12 = v2.Y - v3.Y;
            float D = dY12 * (v1.X - v3.X) + dX21 * (v1.Y - v3.Y);
            float s = dY12 * dX + dX21 * dY;
            float t = (v3.Y - v1.Y) * dX + (v1.X - v3.X) * dY;
            if (D < 0) return (s <= 0) && (t <= 0) && (s + t >= D);
            return (s >= 0) && (t >= 0) && (s + t <= D);
        }

        // Kollar om en snöflinga kolliderar med spelaren eller en tile
        public bool SnowIntersectsObject(Vector2 pos)
        {
            if (!_levelManager.gameActive) return false;

            float snowflakeRadius = snowflakeSize * 0.5f;
            Vector2 snowflakeCenter = pos + new Vector2(snowflakeRadius, snowflakeRadius);

            float playerRadius = _player.playerSize * 0.5f;
            Vector2 playerCenter = _player.position + new Vector2(playerRadius, playerRadius);
            if (Vector2.DistanceSquared(snowflakeCenter, playerCenter) < (snowflakeRadius + playerRadius) * (snowflakeRadius + playerRadius))
                return true;

            int leftTile = Math.Max(0, (int)((snowflakeCenter.X - snowflakeRadius) / _tileMap.tileSize));
            int rightTile = Math.Min(_tileMap.mapWidth - 1, (int)((snowflakeCenter.X + snowflakeRadius) / _tileMap.tileSize));
            int topTile = Math.Max(0, (int)((snowflakeCenter.Y - snowflakeRadius) / _tileMap.tileSize));
            int bottomTile = Math.Min(_tileMap.mapHeight - 1, (int)((snowflakeCenter.Y + snowflakeRadius) / _tileMap.tileSize));

            for (int x = leftTile; x <= rightTile; x++)
            {
                for (int y = topTile; y <= bottomTile; y++)
                {
                    int tileId = _tileMap.tileMap[y, x];
                    Tile tile = _tileMap.tileTypes[tileId];

                    if (tileId == 0) continue;

                    if (tile.IsDeadly)
                    {
                        Vector2 tileCenter = new Vector2(x * _tileMap.tileSize + _tileMap.tileSize * 0.5f, y * _tileMap.tileSize + _tileMap.tileSize * 0.5f);
                        float half = _tileMap.tileSize * 0.5f;
                        Vector2[] tri =
                        [
                            new Vector2(0, -half),
                            new Vector2(-half, half),
                            new Vector2(half, half)
                        ];

                        for (int i = 0; i < 3; i++)
                        {
                            float cos = (float)Math.Cos(tile.Rotation);
                            float sin = (float)Math.Sin(tile.Rotation);
                            float x0 = tri[i].X, y0 = tri[i].Y;
                            tri[i] = new Vector2(tileCenter.X + (x0 * cos - y0 * sin), tileCenter.Y + (x0 * sin + y0 * cos));
                        }

                        if (CircleIntersectsTriangle(snowflakeCenter, snowflakeRadius, tri[0], tri[1], tri[2]))
                            return true;
                    }
                    else if (tile.IsSolid || tile.IsBouncer)
                    {
                        Rectangle tileRect = new Rectangle(x * _tileMap.tileSize, y * _tileMap.tileSize, _tileMap.tileSize, _tileMap.tileSize);
                        if (CircleIntersectsRectangle(snowflakeCenter, snowflakeRadius, tileRect))
                            return true;
                    }
                }
            }

            return false;
        }

        // Skapar en ny snöflinga om tillräckligt med tid har gått
        private void CreateSnowflake(GameTime gameTime)
        {
            timeSinceLastSnowflake += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastSnowflake >= 0.1f)
            {
                snowflakes.Add(new Snowflake(new Vector2(random.Next(0, _graphicsDevice.Viewport.Width - snowflakeSize), -snowflakeSize), 0, 1));
                timeSinceLastSnowflake = 0;
            }
        }

        // Uppdaterar alla snöflingor (rörelse, kollision, opacitet)
        private void UpdateSnowflakes(float frameTime)
        {
            for (int i = snowflakes.Count - 1; i >= 0; i--)
            {
                if (snowflakes[i].Position.Y > _graphicsDevice.Viewport.Height || SnowIntersectsObject(snowflakes[i].Position))
                {
                    if (snowflakes[i].Opacity <= 0) snowflakes.RemoveAt(i);
                    else snowflakes[i].Opacity -= (float)frameTime * 2;
                }
                else
                {
                    snowflakes[i].Velocity += (float)(random.NextDouble() * 4 - 2) * MathF.Sqrt(frameTime);
                    snowflakes[i].Position += new Vector2(snowflakes[i].Velocity * frameTime * 30, 60 * frameTime);
                }
            }
        }

        // Skapar en chip-partikel vid hopp eller landning
        public void CreateChip()
        {
            if (timeSinceLastChip >= 0.045f)
            {
                chips.Add(new Chip(random.Next(2) == 0 ? squareChip : triangularChip, new Vector2(_player.position.X + _player.playerSize * 0.5f, _player.position.Y + _player.playerSize), (float)(random.NextDouble() * Math.PI * 2), 1, random.Next(8, 12) * 0.1f));
                timeSinceLastChip = 0;
            }
        }

        // Uppdaterar alla chip-partiklar (rörelse, rotation, opacitet)
        private void UpdateChips(float frameTime)
        {
            for (int i = chips.Count - 1; i >= 0; i--)
            {
                if (chips[i].Opacity <= 0) chips.RemoveAt(i);
                else
                {
                    chips[i].Rotation += (float)(random.Next(-5, 5) * Math.PI * frameTime) / 6;
                    chips[i].Opacity -= (float)frameTime * 5;
                    chips[i].Position.Y -= random.Next(100, 200) * frameTime;
                }
            }
        }

        // Skapar dödspartiklar (två halvor) vid spelarens död
        public void CreateDeathParticles()
        {
            Vector2 center = new Vector2(_player.position.X + _player.playerSize * 0.5f, _player.position.Y + _player.playerSize * 0.5f);

            _darkHalf = new DarkHalf(center, new Vector2(random.Next(60, 180), random.Next(120, 360)), 0, (float)(random.Next(5, 15) * Math.PI) / 6, 1);
            _lightHalf = new LightHalf(center, new Vector2(random.Next(-180, -60), random.Next(120, 360)), 0, (float)(random.Next(-15, -5) * Math.PI) / 6, 1);
        }

        // Uppdaterar dödspartiklar (rörelse, rotation, opacitet)
        private void UpdateDeathParticles(float frameTime)
        {
            if (_darkHalf != null)
            {
                _darkHalf.Position -= _darkHalf.Velocity * frameTime;
                _darkHalf.Rotation += _darkHalf.RotationSpeed * frameTime;
                _darkHalf.Opacity -= (float)frameTime * 3;

                if (_darkHalf.Opacity <= 0) _darkHalf = null;
            }
            if (_lightHalf != null)
            {
                _lightHalf.Position -= _lightHalf.Velocity * frameTime;
                _lightHalf.Rotation += _lightHalf.RotationSpeed * frameTime;
                _lightHalf.Opacity -= (float)frameTime * 3;

                if (_lightHalf.Opacity <= 0) _lightHalf = null;

            }
        }

        // Uppdaterar alla partiklar varje frame
        public void Update(GameTime gameTime, float frameTime)
        {
            timeSinceLastChip += (float)gameTime.ElapsedGameTime.TotalSeconds;

            CreateSnowflake(gameTime);
            UpdateSnowflakes(frameTime);

            UpdateChips(frameTime);

            UpdateDeathParticles(frameTime);
        }

        // Ritar alla partiklar på skärmen
        public void Draw(GameTime gameTime)
        {
            snowflakeScale = new Vector2((float)snowflakeSize / snowflake.Width, (float)snowflakeSize / snowflake.Height);

            darkHalfScale = new Vector2((float)deathParticleSize / darkHalf.Height);
            lightHalfScale = new Vector2((float)deathParticleSize / lightHalf.Height);

            _spriteBatch.Begin();

            foreach (Snowflake snowflake in snowflakes)
            {
                _spriteBatch.Draw(this.snowflake, snowflake.Position + new Vector2(snowflakeSize * 0.5f), null, Color.White * snowflake.Opacity, 0, new Vector2(this.snowflake.Width * 0.5f, this.snowflake.Height * 0.5f), snowflakeScale, SpriteEffects.None, 0);
            }

            foreach (Chip chip in chips)
            {
                chipSize.X = (float)chip.Texture.Height / chip.Texture.Width * chipSize.Y;
                chipScale = new Vector2(chipSize.Y / chip.Texture.Height, chipSize.X / chip.Texture.Width);
                _spriteBatch.Draw(chip.Texture, chip.Position - new Vector2(0, chipSize.Y * 0.5f), null, Color.White * chip.Opacity, chip.Rotation, new Vector2(chip.Texture.Width * 0.5f, chip.Texture.Height * 0.5f), chipScale, SpriteEffects.None, 0);
            }

            if (_darkHalf != null)
            {
                _spriteBatch.Draw(darkHalf, _darkHalf.Position, null, Color.White * _darkHalf.Opacity, _darkHalf.Rotation, new Vector2(darkHalf.Width * 0.5f, darkHalf.Height * 0.5f), darkHalfScale, SpriteEffects.None, 0);
            }

            if (_lightHalf != null)
            {
                _spriteBatch.Draw(lightHalf, _lightHalf.Position, null, Color.White * _lightHalf.Opacity, _lightHalf.Rotation, new Vector2(lightHalf.Width * 0.5f, lightHalf.Height * 0.5f), lightHalfScale, SpriteEffects.None, 0);
            }

            _spriteBatch.End();
        }
    }
}
