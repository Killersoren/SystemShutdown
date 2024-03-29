﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SystemShutdown.GameObjects
{
    // Lead author: Frederik
    public class CyclebarDay
    {
        #region Fields
        public Texture2D sunSprite;
        public Texture2D dayContainer, dayBar;
        public Vector2 dayBarPosition;
        public int fullBarDay;
        public float currentBarDay;
        public float dayMeter = 0.1f;
        public Color dayBarColor;
        #endregion

        #region Constructor
        public CyclebarDay(ContentManager content)
        {
            LoadContent(content);
            resetDay();
        }
        #endregion

        #region Methods

        public void resetDay()
        {
            fullBarDay = dayBar.Width;
            currentBarDay = fullBarDay;
        }

        private void LoadContent(ContentManager content)
        {
            sunSprite = content.Load<Texture2D>("Textures/sun");
            dayContainer = content.Load<Texture2D>("Textures/HealthbarEmpty");
            dayBar = content.Load<Texture2D>("Textures/Healthbar");
        }

        public void Update()
        {
            if (currentBarDay >= 0)
            {
                currentBarDay -= dayMeter;
            }
            DayColor();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sunSprite, new Vector2(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.X + 525,
                GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.Y - 480), Color.White);

            //Draws bar
            spriteBatch.Draw(dayBar, new Vector2(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.X + 635,
                GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.Y - 455), new Rectangle((int)dayBarPosition.X,
                (int)dayBarPosition.Y, (int)currentBarDay, dayBar.Height), dayBarColor);
            spriteBatch.Draw(dayContainer, new Vector2(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.X + 635,
                GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.Y - 455), Color.White);
        }

        /// <summary>
        /// Changes color by % as bars goes down
        /// </summary>
        public void DayColor()
        {
            if (currentBarDay >= dayBar.Width * 0.70)
                dayBarColor = Color.Yellow;
            else if (currentBarDay >= dayBar.Width * 0.40)
                dayBarColor = Color.DarkOrange;
            else if (currentBarDay >= dayBar.Width * 0.25)
                dayBarColor = Color.MediumBlue;
            else
                dayBarColor = Color.DarkBlue;
        }
        #endregion
    }
}
