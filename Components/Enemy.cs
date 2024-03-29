﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using SystemShutdown.AStar;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;
using SystemShutdown.FactoryPattern;
using SystemShutdown.ObserverPattern;

namespace SystemShutdown.GameObjects
{
    // Lead author: Ras
    public class Enemy : Component, IGameListener
    {
        #region Fields
        private bool playerTarget = false;
        private bool isGoalFound = false;
        private bool threadRunning = true;
        private bool searching = false;

        private float vision;
        private float speed;
        private double updateTimer = 0.0;

        private Vector2 direction;
        private Vector2 nextpos;
        private Vector2 velocity;

        private Thread internalThread;
        private Stack<Node> path = new Stack<Node>();
        private Node goal;
        private Astar aStar;

        public Texture2D[] walk;
        public float fps;
        public float timeElapsed;
        public int currentIndex;

        public int Dmg { get; set; }
        public bool IsTrojan { get; set; }
        public bool AttackingPlayer { get; set; }
        public bool AttackingCPU { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Removes enemy gameobject from list of gameobjects, -1 to alive enemies and +1 to player kills. Stops threads while loop with bool threadRunning
        /// </summary>
        public override void Destroy()
        {
            GameWorld.Instance.GameState.AliveEnemies--;
            GameWorld.Instance.GameState.PlayerBuilder.Player.Kills++;
            GameWorld.Instance.GameState.killsColorTimer = true;
            threadRunning = false;
        }
        /// <summary>
        /// Sets enemy goal for Astar pathfinder. 
        /// If player is in vision area, else if night, else random pos + or - in each direction from current position.
        /// Also sets enemy movement speed to 100 if random target (iddle behavior)
        /// </summary>
        private void FindGoal()
        {
            if (playerTarget)
            {
                speed = 200;
                goal = GameWorld.Instance.GameState.Grid.Node((int)Math.Round(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.X / 100d, 0) * 100 / 100, (int)Math.Round(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position.Y / 100d, 0) * 100 / 100);
            }
            else if (!GameWorld.Instance.IsDay)
            {
                speed = 200;
                goal = GameWorld.Instance.GameState.Grid.Node((int)Math.Round(GameWorld.Instance.GameState.CpuBuilder.Cpu.GameObject.Transform.Position.X / 100d, 0) * 100 / 100, (int)Math.Round(GameWorld.Instance.GameState.CpuBuilder.Cpu.GameObject.Transform.Position.Y / 100d, 0) * 100 / 100);
            }
            else
            {
                speed = 100;
                var maxvalue = new Vector2(((int)Math.Round(GameObject.Transform.Position.X / 100d, 0) + 5), ((int)Math.Round(GameObject.Transform.Position.Y / 100d, 0) + 5));
                var minvalue = new Vector2(((int)Math.Round(GameObject.Transform.Position.X / 100d, 0) - 5), ((int)Math.Round(GameObject.Transform.Position.Y / 100d, 0) - 5));
                var tmpvector = SetRandomEnemyGoal(minvalue, maxvalue);
                goal = GameWorld.Instance.GameState.Grid.Node((int)tmpvector.X / 100, (int)tmpvector.Y / 100);
            }
            isGoalFound = true;
        }
        /// <summary>
        /// Sets a random goal for enemy. - or + 5 nodes from current position in each direction
        /// </summary>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        /// <returns></returns>
        private Vector2 SetRandomEnemyGoal(Vector2 minLimit, Vector2 maxLimit)
        {
            Random rndd = new Random();
            Node enemypos = null;
            while (enemypos == null || !enemypos.Passable)
            {
                enemypos = GameWorld.Instance.GameState.Grid.Node(rndd.Next((int)minLimit.X, (int)maxLimit.X), rndd.Next((int)minLimit.Y, (int)maxLimit.Y));
            }
            return new Vector2(enemypos.X * 100, enemypos.Y * 100);
        }
        /// <summary>
        /// Destroys a enemys gameobject of health is below 0. 
        /// Also have 33 % to drop a mod on current position on death
        /// </summary>
        /// <param name="health"></param>
        private void IsAlive(int health)
        {
            if (health <= 0)
            {
                if (!IsTrojan)
                {
                    Random rand = new Random();
                    var switchEffect = rand.Next(1, 3);

                    if (switchEffect == 1)
                    {
                        GameWorld.Instance.killEffect.Play();
                    }
                    if (switchEffect == 2)
                    {
                        GameWorld.Instance.killEffect2.Play();
                    }
                    if (switchEffect == 3)
                    {
                        GameWorld.Instance.killEffect3.Play();
                    }
                }
                else
                {
                    GameWorld.Instance.horseEffect2.Play();
                }

                Random rnd = new Random();
                var moddrop = rnd.Next(1, 4);
                if (moddrop == 2)
                {
                    GameObject go = ModFactory.Instance.Create(GameObject.Transform.Position, "");
                    GameWorld.Instance.GameState.AddGameObject(go);

                }
                GameObject.Destroy();
            }
        }
        /// <summary>
        /// Checks each second if player is inside of vision. If true, sets bool playerTarget to true, else to false
        /// </summary>
        /// <param name="gameTime"></param>
        private void ScanForPlayer(GameTime gameTime)
        {
            updateTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (updateTimer >= 1.0)
            {
                if (!IsTrojan)
                {
                    if (IsPlayerInRange(GameWorld.Instance.GameState.PlayerBuilder.Player.GameObject.Transform.Position))
                    {
                        playerTarget = true;
                        isGoalFound = false;

                        GameWorld.Instance.enemyEffect.Play();
                    }
                    else
                    {
                        playerTarget = false;
                    }
                }
                updateTimer = 0.0;
            }
        }
        /// <summary>
        /// First resets earlier values in grid and Astar, and pops all remaining nodes in path stack.
        /// Calls Astar.Search() which populates stack with a path of nodes from current position to goal position
        /// If path stack is containing nodes, pops a node and sets it to enemy`s nextpos (node to move to next)
        /// If path stack is empty, enemy is at goal and goalFound bool is sat to false
        /// </summary>
        private void AstarSeachForPath()
        {
            if (searching)
            {

            }
            while (path.Count > 0)
            {
                path.Pop();
            }
            GameWorld.Instance.GameState.Grid.ResetState();
            aStar.Start();
            Node currentPositionAsNode = GameWorld.Instance.GameState.Grid.Node((int)Math.Round(GameObject.Transform.Position.X / 100d, 0) * 100 / GameWorld.Instance.GameState.Grid.NodeSize, (int)Math.Round(GameObject.Transform.Position.Y / 100d, 0) * 100 / GameWorld.Instance.GameState.Grid.NodeSize);
            aStar.Search(currentPositionAsNode, goal, path);
            if (path.Count > 0)
            {
                Node node = path.Pop();
                int x = node.X * GameWorld.Instance.GameState.Grid.NodeSize;
                int y = node.Y * GameWorld.Instance.GameState.Grid.NodeSize;
                nextpos = new Vector2(x, y);
                Move(nextpos);
            }
            else
            {
                isGoalFound = false;
            }
        }
        public override void Update(GameTime gameTime)
        {
            IsAlive(Health);
            ScanForPlayer(gameTime);
            if (!isGoalFound)
            {
                FindGoal();
            }
            AstarSeachForPath();
            if (!IsTrojan)
            {
                Animate(gameTime);
            }
        }
        /// <summary>
        /// Rotates enemy texture in direction of nextpos 
        /// </summary>
        /// <param name="nextpos"></param>
        public void RotateEnemy(Vector2 nextpos)
        {
            direction = GameObject.Transform.Position - nextpos;
            var tmpSR = (SpriteRenderer)GameObject.GetComponent("SpriteRenderer");
            tmpSR.Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        /// <summary>
        /// Returns a bool depending on if player is inside of vision range 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsPlayerInRange(Vector2 target)
        {
            return vision >= Vector2.Distance(GameObject.Transform.Position, target);
        }
        /// <summary>
        /// Moves enemy in direction of Vector2 nextpos 
        /// Normalizes velocity and runs RotateEnemy after move direction is decided. 
        /// </summary>
        /// <param name="nextpos"></param>
        public void Move(Vector2 nextpos)
        {
            velocity = nextpos - GameObject.Transform.Position;
            if (velocity != Vector2.Zero)
            {
                velocity.Normalize();
            }
            velocity *= speed;
            GameObject.Transform.Translate(velocity * GameWorld.Instance.DeltaTime);
            RotateEnemy(nextpos);
        }
        /// <summary>
        /// Sets Thread id
        /// If any of the 2 bools are true, enemy enters either player or CPU 
        /// Else sets thread to sleep(1000)
        /// </summary>
        private void ThreadMethod(object callback)
        {
            while (threadRunning == true && GameWorld.Instance.GameState.IsThreadsRunning == true)
            {
                if (AttackingPlayer)
                {
                    Thread.Sleep(100);
                    GameWorld.Instance.GameState.PlayerBuilder.Player.Enter(internalThread, this);
                    AttackingPlayer = false;
                    AttackingCPU = false;
                }
                else if (AttackingCPU)
                {
                    Thread.Sleep(1000);
                    AttackingPlayer = false;
                    AttackingCPU = false;

                    CPU.Enter(internalThread, this);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        /// <summary>
        /// Starts enemy thread if thread is not already alive. 
        /// Sets thread to Isbackground (to close down thread when game is closing)
        /// </summary>
        public void StartThread()
        {
            if (!internalThread.IsAlive)
            {
                internalThread.Start();
                internalThread.IsBackground = true;
            }
            threadRunning = true;
        }
        /// <summary>
        /// Sets / resets enemy values on creation
        /// </summary>
        public override void Awake()
        {
            fps = 8f;
            this.vision = 500;
            aStar = new Astar();
            Dmg = 1;
            GameObject.Tag = "Enemy";
            isGoalFound = false;
            playerTarget = false;
            threadRunning = true;
            if (IsTrojan)
            {
                Health = 300;
            }
            else
            {
                Health = 100;
            }
            internalThread = new Thread(ThreadMethod);
            StartThread();
            //Load sprite sheet - Frederik
            walk = new Texture2D[3];
            //Loop animaiton textures
            for (int g = 0; g < walk.Length; g++)
            {
                walk[g] = GameWorld.Instance.Content.Load<Texture2D>(g + 1 + "enemy");
            }
        }
        public override string ToString()
        {
            return "Enemy";
        }
        /// <summary>
        /// Not used, player and CPU is responsible for handling of collision to disable collision checks between enemies
        /// </summary>
        /// <param name="gameEvent"></param>
        /// <param name="component"></param>
        public void Notify(GameEvent gameEvent, Component component)
        {
        }

        /// <summary>
        /// Animate enemy bug - Frederik
        /// </summary>
        /// <param name="gametime"></param>
        public void Animate(GameTime gametime)
        {
            // Gives time that has passed since last update
            timeElapsed += (float)gametime.ElapsedGameTime.TotalSeconds;
            // Calculates currentIndex
            currentIndex = (int)(timeElapsed * fps);
            var tmpSpriteRenderer = (SpriteRenderer)GameObject.GetComponent("SpriteRenderer");
            tmpSpriteRenderer.Sprite = walk[currentIndex];

            // Checks if animation needs to restart
            if (currentIndex >= walk.Length - 1)
            {
                // Resets animation
                timeElapsed = 0;
                currentIndex = 0;
            }
        }
        #endregion
    }
}
