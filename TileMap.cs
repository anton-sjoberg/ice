using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Ice
{
    // Klassen representerar en enskild tile i tilekartan.
    internal class Tile
    {
        public Texture2D Texture;    // Textur för denna tile
        public bool IsSolid;         // Om spelaren kan stå på denna tile
        public bool IsDeadly;        // Om denna tile är dödlig (t.ex. spik)
        public bool IsBouncer;       // Om denna tile är en studstile
        public float Rotation;       // Rotation för att kunna återanvända texturer

        // Skapar en tile med angivna egenskaper.
        public Tile(Texture2D texture, bool isSolid, bool isDeadly, bool isBouncer, float rotation)
        {
            Texture = texture;
            IsSolid = isSolid;
            IsDeadly = isDeadly;
            IsBouncer = isBouncer;
            Rotation = rotation;
        }
    }

    // Klassen hanterar tilekartan, dess rendering och tiletyper.
    internal class TileMap
    {
        // Konstruktor som tar emot grafikobjekt och spritebatch.
        public TileMap(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
        }

        // Grafik och rendering
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        // Tile-skalning och texturer
        private Vector2 tileScale;
        private Texture2D ground, platform, surface, surfaceTop, surfaceBottom, spike, bouncerLeft, bouncerRight;

        // Tiletyper och karta
        public Dictionary<int, Tile> tileTypes; // Alla tiletyper, indexerade med ID
        public int mapWidth, mapHeight, tileSize; // Kartans dimensioner och tile-storlek
        public int[,] tileMap; // Själva tilekartan (2D-array av tile-ID:n)

        // Initierar tilekartans grundläggande värden.
        public void Initialize()
        {
            tileSize = 60;
        }

        // Laddar in alla tiletexturer och definierar tiletyper.
        public void LoadContent(ContentManager content)
        {
            ground = content.Load<Texture2D>("Tile Textures/Ground");
            platform = content.Load<Texture2D>("Tile Textures/Platform");
            surface = content.Load<Texture2D>("Tile Textures/Surface");
            spike = content.Load<Texture2D>("Tile Textures/Spike");
            bouncerLeft = content.Load<Texture2D>("Tile Textures/Bouncer Left");
            bouncerRight = content.Load<Texture2D>("Tile Textures/Bouncer Right");

            // Definierar alla tiletyper med respektive egenskaper.
            tileTypes = new Dictionary<int, Tile>()
                {
                    { 0, new Tile(null, false, false, false, 0) }, // Tom tile
                    { 1, new Tile(ground, true, false, false, 0) }, // Mark
                    { 2, new Tile(platform, true, false, false, 0) }, // Plattform
                    { 3, new Tile(surface, true, false, false, 0) }, // Yta
                    { 4, new Tile(surface, true, false, false, (float)Math.PI) }, // Yta (upp och ner)
                    { 5, new Tile(spike, false, true, false, 0) }, // Spik (uppåt)
                    { 6, new Tile(spike, false, true, false, (float)Math.PI * 0.5f) }, // Spik (höger)
                    { 7, new Tile(spike, false, true, false, (float)Math.PI) }, // Spik (nedåt)
                    { 8, new Tile(spike, false, true, false, (float)Math.PI * 1.5f) }, // Spik (vänster)
                    { 9, new Tile(bouncerLeft, false, false, true, 0) }, // Studs vänster
                    { 10, new Tile(bouncerRight, false, false, true, 0) } // Studs höger
                };
        }

        // Ritar hela tilekartan på skärmen.
        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int tileId = tileMap[y, x];
                    Tile tile = tileTypes[tileId];

                    // Ritar endast tiles som har en textur (dvs. inte tomma tiles)
                    if (tile.Texture != null)
                    {
                        tileScale = new Vector2((float)tileSize / tile.Texture.Width, (float)tileSize / tile.Texture.Height);

                        Vector2 tilePos = new Vector2(x * tileSize, y * tileSize);
                        _spriteBatch.Draw(tile.Texture, tilePos + new Vector2(tileSize * 0.5f, tileSize * 0.5f), null, Color.White, tile.Rotation, new Vector2(tile.Texture.Width * 0.5f, tile.Texture.Height * 0.5f), tileScale, SpriteEffects.None, 0);
                    }
                }
            }

            _spriteBatch.End();
        }
    }
}
