using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ice
{
    // Hanterar tidtagning och sparande av snabbaste tid
    internal class TimeToCompleteManager
    {
        // Konstruktor, tar emot referenser till spelare och nivåhanterare
        public TimeToCompleteManager(Player player, LevelManager levelManager)
        {
            _player = player;
            _levelManager = levelManager;
        }

        private Player _player;
        private LevelManager _levelManager;

        private string filePath;    // Sökväg till fil för snabbaste tid
        private bool timeSaved;     // Om tiden redan sparats

        public float timeToComplete, oldTimeToComplete, fastestTime;
        public bool beatFastestTime;

        // Initierar sökväg och laddar snabbaste tid
        public void Initialize()
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ice", "Fastest Time.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            fastestTime = LoadFastestTime();

            timeSaved = false;
        }

        // Läser in snabbaste tid från fil
        private float LoadFastestTime()
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);
                return float.MaxValue;
            }

            try
            {
                return float.Parse(File.ReadAllText(filePath));
            }
            catch
            {
                return float.MaxValue;
            }
        }

        // Sparar snabbaste tid till fil
        private void SaveFastestTime()
        {
            File.WriteAllText(filePath, $"{fastestTime}");
        }

        // Uppdaterar tidtagning och hanterar sparande av ny snabbaste tid
        public void Update(GameTime gameTime)
        {
            // Öka tiden om spelaren har rört sig och spelet inte är pausat
            if (_player.hasMoved && !_levelManager.gamePaused) timeToComplete += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Om spelet är klart och tiden inte redan sparats
            if (_levelManager.gameFinished && !timeSaved)
            {
                if (timeToComplete < fastestTime)
                {
                    fastestTime = timeToComplete;
                    beatFastestTime = true;
                }
                else beatFastestTime = false;
                
                SaveFastestTime();
                oldTimeToComplete = timeToComplete;
                timeToComplete = 0;
                timeSaved = true;
            }
            else if (!_levelManager.gameFinished) timeSaved = false;
        }
    }
}
