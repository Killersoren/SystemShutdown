﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemShutdown.GameObjects
{
    public class Player : GameObject
    {
        #region Fields
        private KeyboardState currentKey;

        private KeyboardState previousKey;

        private float speed;
        private float laserSpeed;
        public Vector2 previousPosition;

        private float currentDirY;
        private float currentDirX;

        #endregion

        #region Properties
        public bool IsDead
        {
            get
            {
                return Health <= 0;
            }
        }

        public Input Input { get; set; }
        #endregion

        #region Methods

        #region Constructor
        public Player()
        {
            this.speed = 300;
            fps = 10f;

        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (IsDead)
            {
                return;
            }



            origin = new Vector2(rectangle.Width / 2, rectangle.Height / 2);

            Animate(gametime: gameTime);

            ////Right
            //if (currentDir.X == 1)
            //{
            //    rotation += 90f;
            //}
            ////Left
            //if (currentDir.X == -1)
            //{
            //    rotation += 90f;
            //}
            ////Down
            //if (currentDir.Y == 1)
            //{

            //}
            ////Up
            //if (currentDir.Y == -1)
            //{

            //}
            //Move(gameTime);
        }

        public void LoadContent(ContentManager content)
        {
            sprite = content.Load<Texture2D>("1GuyUp");
            rectangle = new Rectangle(new Point((int)position.X, (int)position.Y), new Point(sprite.Width - 10, sprite.Height - 10));

            //Load sprite sheet
            upWalk = new Texture2D[3];

            //Loop animaiton
            for (int g = 0; g < upWalk.Length; g++)
            {
                upWalk[g] = content.Load<Texture2D>(g + 1 + "GuyUp");
            }
            //When loop is finished return to first sprite/Sets default sprite
            sprite = upWalk[0];

            ////Load sprite sheet
            //downWalk = new Texture2D[3];

            ////Loop animaiton
            //for (int h = 0; h < downWalk.Length; h++)
            //{
            //    downWalk[h] = content.Load<Texture2D>(h + 1 + "GuyDown");
            //}
            ////When loop is finished return to first sprite/Sets default sprite
            //sprite = downWalk[0];

            //Load sprite sheet
            //rightWalk = new Texture2D[3];

            ////Loop animaiton
            //for (int i = 0; i < rightWalk.Length; i++)
            //{
            //    rightWalk[i] = content.Load<Texture2D>(i + 1 + "GuyRight");
            //}
            ////When loop is finished return to first sprite/Sets default sprite
            //sprite = rightWalk[0];

            ////Load sprite sheet
            //leftWalk = new Texture2D[3];

            ////Loop animaiton
            //for (int j = 0; j < leftWalk.Length; j++)
            //{
            //    leftWalk[j] = content.Load<Texture2D>(j + 1 + "GuyLeft");
            //}
            ////When loop is finished return to first sprite/Sets default sprite
            //sprite = leftWalk[0];

        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Frederik
            if (IsDead)
            {
                return;
            }
            //spriteBatch.Draw(sprite, rectangle, Colour);

            base.Draw(gameTime, spriteBatch);
        }

        public void Move(Vector2 velocity)
        {
            //currentDirX = 0;
            //currentDirY = 0;
            if (velocity.X == 0 && velocity.Y != 0)
            {
                currentDirY = velocity.Y;
                //currentDirX = 0;
            }
            if (velocity.Y == 0 && velocity.X != 0)
            {
                currentDirX = velocity.X;
                //currentDirY = 0;
            }
            //currentDir = velocity;

            if (velocity != Vector2.Zero)
            {
                velocity.Normalize();
            }
            velocity *= speed;
            position += (velocity * GameWorld.DeltaTime);
            rectangle.X = (int)position.X;
            rectangle.Y = (int)position.Y;
            if (/*Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.D)*/currentDirY == -1 && currentDirX == 1)
            {
                rotation = (float)Math.PI / 4;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.A)*/currentDirY == -1 && currentDirX == -1)
            {
                rotation = (float)Math.PI / 4 * 7;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.D)*/currentDirY == 1 && currentDirX == 1)
            {
                rotation = (float)Math.PI * 3 / 4;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.A)*/currentDirY == 1 && currentDirX == -1)
            {
                rotation = (float)Math.PI / 4 * 5;
            }

            else if (/*Keyboard.GetState().IsKeyDown(Keys.D)*/currentDirX == 1)
            {
                rotation = (float)Math.PI / 2;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.A)*/currentDirX == -1)
            {
                rotation = (float)Math.PI * 3 / 2;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.S)*/currentDirY == 1)
            {
                rotation = (float)Math.PI;
            }
            else if (/*Keyboard.GetState().IsKeyDown(Keys.W)*/currentDirY == -1)
            {
                rotation = (float)Math.PI * 2;
            }
            
        }

        public void Shoot(Vector2 velocity, Vector2 position)
        {
            
            velocity *= laserSpeed;
            position += (velocity * GameWorld.DeltaTime);
            rectangle.X = (int)position.X;
            rectangle.Y = (int)position.Y;
        }


        //public void Move(GameTime gameTime)
        //{
        //    ///<summary>
        //    /// Movement speed will be consistent no matter the framerate
        //    /// Frederik
        //    ///</summary>
        //    timePassed = gameTime.ElapsedGameTime.Milliseconds;
        //    float tangentialVelocity = timePassed / 4;

        //    previousKey = currentKey;
        //    currentKey = Keyboard.GetState();

        //    /// <summary>
        //    /// Rotation rectangle, Player rotation, movement & speed
        //    /// Frederik
        //    /// </summary>
        //    //Player is able to move
        //    position = velocity + position;

        //    if (currentKey.IsKeyDown(Input.Right))
        //    {
        //        rotation += 0.1f;
        //    }
        //    if (currentKey.IsKeyDown(Input.Left))
        //    {
        //        rotation -= 0.1f;
        //    }

        //    if (currentKey.IsKeyDown(Input.Up))
        //    {
        //        velocity.X = (float)Math.Cos(rotation) * tangentialVelocity;
        //        velocity.Y = (float)Math.Sin(rotation) * tangentialVelocity;
        //    }
        //    //Stops movement when key released & adds friction
        //    else if (velocity != Vector2.Zero)
        //    {
        //        float k = velocity.X;
        //        float l = velocity.Y;

        //        velocity.X = k -= friction * k;
        //        velocity.Y = l -= friction * l;
        //    }

        //    //var velocity = Vector2.Zero;
        //    //rotation = 0;

        //    //if (currentKey.IsKeyDown(Input.Up))
        //    //{
        //    //    velocity.Y = -movementSpeed;
        //    //    velocity.Normalize();
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Down))
        //    //{
        //    //    velocity.Y += movementSpeed;
        //    //    rotation = MathHelper.ToRadians(180);
        //    //    velocity.Normalize();
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Left))
        //    //{
        //    //    velocity.X -= movementSpeed;
        //    //    rotation = MathHelper.ToRadians(-90);
        //    //    velocity.Normalize();
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Right))
        //    //{
        //    //    velocity.X += movementSpeed;
        //    //    rotation = MathHelper.ToRadians(90);
        //    //    velocity.Normalize();
        //    //}

        //    //if (currentKey.IsKeyDown(Input.Up) && currentKey.IsKeyDown(Input.Right))
        //    //{
        //    //    rotation = MathHelper.ToRadians(45);
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Up) && currentKey.IsKeyDown(Input.Left))
        //    //{
        //    //    rotation = MathHelper.ToRadians(-45);
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Down) && currentKey.IsKeyDown(Input.Right))
        //    //{
        //    //    rotation = MathHelper.ToRadians(-225);
        //    //}
        //    //if (currentKey.IsKeyDown(Input.Down) && currentKey.IsKeyDown(Input.Left))
        //    //{
        //    //    rotation = MathHelper.ToRadians(225);
        //    //}

        //    //// Movement
        //    //position += velocity;
        //}

        // Frederik
        public override void OnCollision(GameObject sprite)
        {
            if (IsDead)
            {
                return;
            }
        }
        #endregion
    }
}
