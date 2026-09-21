using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Ice
{
    // Klassen representerar spelaren i spelet.
    internal class Player
    {
        // Konstruktor som tar emot nödvändiga managers och grafikobjekt.
        public Player(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, TileMap tileMap, ParticleManager particleManager, LevelManager levelManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _tileMap = tileMap;
            _particleManager = particleManager;
            _levelManager = levelManager;
        }

        // Grafik och managers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private TileMap _tileMap;
        private ParticleManager _particleManager;
        private LevelManager _levelManager;

        // Fysik- och rörelsevariabler
        private Vector2 playerScale, walljumpVelocity, preVelocity;
        private float acceleration, friction, gravity, jumpVelocity, playerRotation, rotationSpeed, bouncerVelocity, frameTime;
        private Texture2D player;
        private KeyboardState keyboardState;
        private bool intersectedWall;

        // Publika fält för position, hastighet och storlek
        public int playerSize;
        public Vector2 position, velocity, averageVelocity;
        public float maxSpeed;
        public bool hasMoved;

        // Initierar spelarens startvärden och fysikparametrar.
        public void Initialize()
        {
            velocity = Vector2.Zero;
            position.Y = 720;

            playerSize = 75;

            acceleration = 1.5f;
            friction = 0.9f;
            gravity = 2;
            maxSpeed = 15;
            jumpVelocity = 25;
            bouncerVelocity = 40;
            walljumpVelocity.X = 12; walljumpVelocity.Y = 25;
            hasMoved = false;
        }

        // Laddar spelarens textur från Content-pipelinen.
        public void LoadContent(ContentManager content)
        {
            player = content.Load<Texture2D>("Sprites/Player");
        }

        // Sätter referenser till managers efter att de skapats.
        public void SetNullManagers(ParticleManager particleManager, LevelManager levelManager)
        {
            _particleManager = particleManager;
            _levelManager = levelManager;
        }

        // Hjälpmetod: Kollar om en cirkel (spelaren) kolliderar med en rektangel (tile).
        private bool CircleIntersectsRectangle(Vector2 center, float radius, Rectangle rect)
        {
            float closestX = Math.Clamp(center.X, rect.Left, rect.Right);
            float closestY = Math.Clamp(center.Y, rect.Top, rect.Bottom);
            float dx = center.X - closestX;
            float dy = center.Y - closestY;
            return (dx * dx + dy * dy) < (radius * radius);
        }

        // Hjälpmetod: Kollar om en cirkel (spelaren) kolliderar med en triangel (tile).
        private bool CircleIntersectsTriangle(Vector2 center, float radius, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            if (PointInTriangle(center, p1, p2, p3))
                return true;

            if (CircleIntersectsLine(center, radius, p1, p2)) return true;
            if (CircleIntersectsLine(center, radius, p2, p3)) return true;
            if (CircleIntersectsLine(center, radius, p3, p1)) return true;

            return false;
        }

        // Hjälpmetod till CircleIntersectsTriangle: Kollar om en cirkel (spelaren) kolliderar med en linje.
        private bool CircleIntersectsLine(Vector2 center, float radius, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ac = center - a;
            float t = Vector2.Dot(ac, ab) / ab.LengthSquared();
            t = Math.Clamp(t, 0, 1);
            Vector2 closest = a + t * ab;
            return Vector2.DistanceSquared(center, closest) < radius * radius;
        }

        // Hjälpmetod till CircleIntersectsTriangle: Kollar om en punkt ligger inom en triangel.
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

        // Kollar om spelaren kolliderar med en solid tile
        public bool PlayerIntersectsSolid(Vector2 pos)
        {
            float radius = playerSize * 0.5f;
            Vector2 center = pos + new Vector2(radius, radius);

            int leftTile = Math.Max(0, (int)((center.X - radius) / _tileMap.tileSize));
            int rightTile = Math.Min(_tileMap.mapWidth - 1, (int)((center.X + radius) / _tileMap.tileSize));
            int topTile = Math.Max(0, (int)((center.Y - radius) / _tileMap.tileSize));
            int bottomTile = Math.Min(_tileMap.mapHeight - 1, (int)((center.Y + radius) / _tileMap.tileSize));

            for (int x = leftTile; x <= rightTile; x++)
            {
                for (int y = topTile; y <= bottomTile; y++)
                {
                    int tileId = _tileMap.tileMap[y, x];
                    if (_tileMap.tileTypes[tileId].IsSolid)
                    {
                        Rectangle tileRect = new Rectangle(x * _tileMap.tileSize, y * _tileMap.tileSize, _tileMap.tileSize, _tileMap.tileSize);
                        if (CircleIntersectsRectangle(center, radius, tileRect))
                            return true;
                    }
                }
            }
            return false;
        }

        // Kollar om spelaren är i kontakt med en vägg på vänster sida.
        private bool PlayerIntersectsLeftWall(Vector2 pos)
        {
            if (pos.X < 0) return true;
            return false;

        }

        // Hanterar spelarens död, återställer position och hastighet.
        public void Die()
        {            
            float startX = (_levelManager != null && _levelManager.CurrentLevel == 0) ? 100 : 0;
            position = new Vector2(startX, Game1.VirtualHeight - 1);
            velocity = Vector2.Zero;
            averageVelocity = Vector2.Zero;

            while (PlayerIntersectsSolid(position) && position.Y > 0) position.Y -= 0.1f;
        }

        // Kollar om spelaren är i kontakt med en bouncer-tile.
        private void CheckForBouncer(Vector2 pos)
        {
            float radius = playerSize * 0.5f;
            Vector2 center = pos + new Vector2(radius, radius);

            int leftTile = Math.Max(0, (int)((center.X - radius) / _tileMap.tileSize));
            int rightTile = Math.Min(_tileMap.mapWidth - 1, (int)((center.X + radius) / _tileMap.tileSize));
            int topTile = Math.Max(0, (int)((center.Y - radius) / _tileMap.tileSize));
            int bottomTile = Math.Min(_tileMap.mapHeight - 1, (int)((center.Y + radius) / _tileMap.tileSize));

            for (int x = leftTile; x <= rightTile; x++)
            {
                for (int y = topTile; y <= bottomTile; y++)
                {
                    int tileId = _tileMap.tileMap[y, x];
                    if (_tileMap.tileTypes[tileId].IsBouncer)
                    {
                        Rectangle tileRect = new Rectangle(x * _tileMap.tileSize, y * _tileMap.tileSize, _tileMap.tileSize, _tileMap.tileSize);
                        if (CircleIntersectsRectangle(center, radius, tileRect)) velocity.Y = -bouncerVelocity;
                    }
                }
            }
        }

        // Kollar om spelaren är i kontakt med en deadly-tile.
        private void CheckForDeadly(Vector2 pos)
        {
            float radius = playerSize * 0.5f;
            Vector2 center = pos + new Vector2(radius, radius);

            int leftTile = Math.Max(0, (int)((center.X - radius) / _tileMap.tileSize));
            int rightTile = Math.Min(_tileMap.mapWidth - 1, (int)((center.X + radius) / _tileMap.tileSize));
            int topTile = Math.Max(0, (int)((center.Y - radius) / _tileMap.tileSize));
            int bottomTile = Math.Min(_tileMap.mapHeight - 1, (int)((center.Y + radius) / _tileMap.tileSize));

            for (int x = leftTile; x <= rightTile; x++)
            {
                for (int y = topTile; y <= bottomTile; y++)
                {
                    int tileId = _tileMap.tileMap[y, x];
                    Tile tile = _tileMap.tileTypes[tileId];
                    if (tile.IsDeadly)
                    {
                        Vector2 tileCenter = new Vector2(x * _tileMap.tileSize + _tileMap.tileSize * 0.5f, y * _tileMap.tileSize + _tileMap.tileSize * 0.5f);
                        float half = _tileMap.tileSize * 0.5f;
                        Vector2[] tri =
                        {
                            new Vector2(0, -half),
                            new Vector2(-half, half),
                            new Vector2(half, half)
                        };

                        for (int i = 0; i < 3; i++)
                        {
                            float cos = (float)Math.Cos(tile.Rotation);
                            float sin = (float)Math.Sin(tile.Rotation);
                            float x0 = tri[i].X, y0 = tri[i].Y;
                            tri[i] = new Vector2(tileCenter.X + (x0 * cos - y0 * sin), tileCenter.Y + (x0 * sin + y0 * cos));
                        }

                        if (CircleIntersectsTriangle(center, radius, tri[0], tri[1], tri[2]))
                        {
                            _particleManager.CreateDeathParticles();

                            Die();
                        }
                    }
                }
            }
        }

        // Kollar om spelaren är utanför skärmens nedre gräns, dör om så är fallet.
        private void CheckForOutOfBounds(Vector2 pos) { if (pos.Y > Game1.VirtualHeight) Die(); }

        // Hanterar spelarens rörelse i X-led.
        private void MovementX()
        {
            position.X += averageVelocity.X * frameTime * 60;

            // Logik för när spelaren nuddar en brant
            if (PlayerIntersectsSolid(position))
            {
                for (int i = 0; i < (int)maxSpeed; i++)
                {
                    if (PlayerIntersectsSolid(position)) position.Y -= 1;
                }

                if (PlayerIntersectsSolid(position)) position.Y += (int)maxSpeed;
            }

            // Logik för när spelaren nuddar en vägg
            if (PlayerIntersectsSolid(position) || PlayerIntersectsLeftWall(position))
            {
                if (PlayerIntersectsLeftWall(position)) intersectedWall = true;
                else intersectedWall = false;

                while (PlayerIntersectsSolid(position) || PlayerIntersectsLeftWall(position)) position.X -= Math.Sign(averageVelocity.X) * 0.1f;

                if (!intersectedWall && (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W)) && (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.A)))
                {
                    velocity.X = -Math.Sign(averageVelocity.X) * walljumpVelocity.X;
                    velocity.Y = -walljumpVelocity.Y;
                }
                else
                {
                    velocity.X = 0;
                }
            }
            else if ((keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.A)) && !(keyboardState.IsKeyDown(Keys.D) && keyboardState.IsKeyDown(Keys.A)))
            {
                if (!_levelManager.gameFinished) hasMoved = true;

                preVelocity.X = velocity.X;

                if (keyboardState.IsKeyDown(Keys.D)) velocity.X += acceleration * frameTime * 60;
                else velocity.X -= acceleration * frameTime * 60;
            }
            else { preVelocity.X = velocity.X; velocity.X *= (float)Math.Pow(friction, frameTime * 60); }

            if (Math.Abs(velocity.X) > maxSpeed) { preVelocity.X = velocity.X; velocity.X = Math.Sign(velocity.X) * maxSpeed; }
            else preVelocity.X = velocity.X;

            averageVelocity.X = (preVelocity.X + velocity.X) * 0.5f;
        }

        // Hanterar spelarens rörelse i Y-led.
        private void MovementY(GameTime gameTime)
        {
            position.Y += averageVelocity.Y * frameTime * 60;
            
            // Logik för när spelaren nuddar marken eller ett tak
            if (PlayerIntersectsSolid(position))
            {
                while (PlayerIntersectsSolid(position)) position.Y -= Math.Sign(averageVelocity.Y) * 0.1f;

                if (Math.Abs(velocity.X) == maxSpeed) _particleManager.CreateChip();
                
                if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W)) && velocity.Y >= 0) velocity.Y = -jumpVelocity;
                else velocity.Y = 0;
            }

            CheckForBouncer(position);

            preVelocity.Y = velocity.Y;

            velocity.Y += gravity * frameTime * 60;

            averageVelocity.Y = (preVelocity.Y + velocity.Y) * 0.5f;
        }

        // Hanterar spelarens rotation baserat på rörelsehastighet.
        private void ManageRotation()
        {
            if (Math.Abs(velocity.X) <= acceleration * 2 && ((keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.A)) && !(keyboardState.IsKeyDown(Keys.D) && keyboardState.IsKeyDown(Keys.A)))) rotationSpeed = 0;
            else rotationSpeed = (averageVelocity.X * 120 * frameTime) / playerSize;

            playerRotation += rotationSpeed;
        }

        // Uppdaterar spelarens tillstånd varje frame.
        public void Update(GameTime gameTime, float FrameTime)
        {
            keyboardState = Keyboard.GetState(); // Definierar tangentbordsstatus som keyboardState

            frameTime = FrameTime; // Hämtar frametiden från Game1.cs

            CheckForDeadly(position);
            CheckForOutOfBounds(position);

            MovementX();
            MovementY(gameTime);
            ManageRotation();
        }

        // Ritar spelaren på skärmen med korrekt position, rotation och skala.
        public void Draw(GameTime gameTime)
        {
            playerScale = new Vector2((float)playerSize / player.Width, (float)playerSize / player.Height);

            _spriteBatch.Begin();

            _spriteBatch.Draw(player, position + new Vector2(playerSize * 0.5f, playerSize * 0.5f), null, Color.White, playerRotation, new Vector2(player.Width * 0.5f, player.Height * 0.5f), playerScale, SpriteEffects.None, 0);

            _spriteBatch.End();
        }
    }
}
