﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;
using SystemShutdown.GameObjects;

namespace SystemShutdown.BuildPattern
{ 
    //  Lead author: Søren
    public class CPUBuilder : IBuilder
    {
        #region Fields
        private GameObject go;

        public SpriteRenderer sr;

        public Texture2D[] colors;
        public float fps;
        public float timeElapsed;
        public int currentIndex;
        private CPU cpu;
        #endregion

        #region Methods

        public CPU Cpu
        {
            get { return cpu; }
            set { cpu = value; }
        }

        public void BuildGameObject()
        {
            go = new GameObject();

            sr = new SpriteRenderer("cpu");

            go.AddComponent(sr);
            sr.Origin = new Vector2(sr.Sprite.Width / 2, (sr.Sprite.Height) / 2);

            cpu = new CPU();

            go.AddComponent(new Collider(sr, cpu) { CheckCollisionEvents = true });
            go.AddComponent(cpu);
            /// Adds CPU to collider list
            GameWorld.Instance.GameState.AddGameObject(go);

            //Load sprite sheet - Frederik
            colors = new Texture2D[12];

            //Loop animaiton
            for (int g = 0; g < colors.Length; g++)
            {
                colors[g] = GameWorld.Instance.Content.Load<Texture2D>(g + 1 + "cpu");
            }
            //When loop is finished return to first sprite/Sets default sprite
            sr.Sprite = colors[0];
        }

        /// <summary>
        /// Animate cpu color - Frederik
        /// </summary>
        /// <param name="gametime"></param>
        public void Animate(GameTime gametime)
        {
            // Gives time that has passed since last update
            timeElapsed += (float)gametime.ElapsedGameTime.TotalSeconds;

            // Calculates currentIndex
            currentIndex = (int)(timeElapsed * fps);
            sr.Sprite = colors[currentIndex];

            // Checks if animation needs to restart
            if (currentIndex >= colors.Length - 1)
            {
                // Resets animation
                timeElapsed = 0;
                currentIndex = 0;
            }
        }

        public GameObject GetResult()
        {
            return go;
        }
        #endregion
    }
}
