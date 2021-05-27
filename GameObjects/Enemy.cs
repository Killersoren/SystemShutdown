﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SystemShutdown.AStar;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;
using SystemShutdown.ObjectPool;
using SystemShutdown.ObserverPattern;
using SystemShutdown.States;

namespace SystemShutdown.GameObjects
{
   public class Enemy : Component, IGameListener
    {
        Thread internalThread;
        string data;
        private bool attackingPlayer = false;
        private bool attackingCPU = false;
        private bool enableAstar = true;

        double updateTimer = 0.0;
        private SpriteRenderer spriteRenderer;

        public bool AttackingCPU
        {
            get { return attackingCPU; }
            set { attackingCPU = value; }
        }
        public bool AttackingPlayer
        {
            get { return attackingPlayer; }
            set { attackingPlayer = value; }
        }

        public int dmg { get; set; }
        public int id { get; set; }
        private string name = "Enemy";
        public event EventHandler ClickSelect;
        private Random randomNumber;
        private float vision;

        private Texture2D sprite;
        private Rectangle rectangle;

        bool playerTarget = false;


        // Astar 

        private double updateTimerA = 0.0;
        private double updateTimerB= 0.0;


        private bool Searching = false;

        Stack<Node> path = new Stack<Node>();
        Node goal;

        Astar aStar;
        GameObject1 go;
        //

        Node[,] enemyNodes;


        private bool threadRunning = true;

        public bool ThreadRunning
        {
            get { return threadRunning; }
            set { threadRunning = value; }
        }

        public Enemy()
        {
            this.vision = 500;
            dmg = 5;
           // this.rectangle = Rectangle;
            internalThread = new Thread(ThreadMethod);
            LoadContent(GameWorld.content);

            //    randomNumber = new Random();
            //    positionX = 300 + randomNumber.Next(0, 150);
            //    positionY = 700 + randomNumber.Next(0, 150);
            //    x = new Point(positionX, positionY);
            //    y = new Point(24, 48);
            //    this.rectangle = new Rectangle(x, y);
            //go = new GameObject1();
            //go.AddComponent(goal);
            enemyNodes = GameWorld.gameState.grid.nodes;
        }
  
  
        public override void Destroy()
        {
            EnemyPool.Instance.RealeaseObject(GameObject);
        }
        public override void Update(GameTime gameTime)
        {
            updateTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (updateTimer >= 1.0)
            {
                if (IsPlayerInRange(GameWorld.gameState.playerBuilder.Player.GameObject.Transform.Position))
                {

                    //        //  better handling of walls is needed

                    //        //foreach (var item in GameWorld.gameState.grid.nodes)
                    //        //{
                    //        //    if (!item.Passable)
                    //        //    {
                    //        //        if (item.position.X < GameWorld.gameState.Player1Test.position.X && item.position.X > rectangle.X)
                    //        //        {

                   // Debug.WriteLine("Enemy can see player!");
                    playerTarget = true;
                //        //        }
                //        //    }
                //        //}

                }
                    else
                    {
                    // Debug.WriteLine("Enemy can not see player!");
                    playerTarget = false;

                }
                updateTimer = 0.0;
            }


            // Astar

            //updateTimerB += gameTime.ElapsedGameTime.TotalSeconds;
            //if (updateTimerB >= 2.0)
            //{
            //if (enableAstar)
            //{
                //if (playerTarget && !Searching )//*&& GameWorld.gameState.playerBuilder.player.GameObject.Transform.Position.X != goal.x * 100 && GameWorld.gameState.playerBuilder.player.GameObject.Transform.Position.Y != goal.y * 100)
                //{
                   // enableAstar = false;



            //Player target

            if (playerTarget)
            {
                goal = aStar.Node((int)GameWorld.gameState.playerBuilder.Player.GameObject.Transform.Position.X / 100, (int)GameWorld.gameState.playerBuilder.Player.GameObject.Transform.Position.Y / 100);

            }

            //CPU target
            else
            {
                goal = aStar.Node((int)GameWorld.gameState.cpuBuilder.Cpu.GameObject.Transform.Position.X / 100, (int)GameWorld.gameState.cpuBuilder.Cpu.GameObject.Transform.Position.Y / 100);
            }

            // go.Transform.Position = new Vector2((int)GameWorld.gameState.playerBuilder.player.GameObject.Transform.Position.X, (int)GameWorld.gameState.playerBuilder.player.GameObject.Transform.Position.Y);

            //GameWorld.gameState.AddGameObject(go);


            Node start = null;
                    start = aStar.Node((int)GameObject.Transform.Position.X / GameWorld.gameState.NodeSize, (int)GameObject.Transform.Position.Y / GameWorld.gameState.NodeSize);

                    // if clicked on non passable node, then march in direction of player till passable found
                    //while (!goal.Passable)
                    //{
                    //    int di = start.x - goal.x;
                    //    int dj = start.y - goal.y;

                    //    int di2 = di * di;
                    //    int dj2 = dj * dj;

                    //    int ni = (int)Math.Round(di / Math.Sqrt(di2 + dj2));
                    //    int nj = (int)Math.Round(dj / Math.Sqrt(di2 + dj2));

                    //    goal = aStar.Node(goal.x + ni, goal.y + nj);
                    //}


                    aStar.Start(start);


                while (path.Count > 0) path.Pop();
                    aStar.ResetState();
                Searching = true;

                //  }
                // updateTimerB = 0.0;
                //}

           // }
            // use update timer to slow down animation
            updateTimerA += gameTime.ElapsedGameTime.TotalSeconds;
            if (updateTimerA >= 0.8)
            {

                // begin the search to goal from enemy's position
                // search function pushs path onto the stack
                if (Searching)
                {
                    Node current = null;

                    current = aStar.Node((int)GameObject.Transform.Position.X / GameWorld.gameState.NodeSize, (int)GameObject.Transform.Position.Y / GameWorld.gameState.NodeSize);
                    //current.alreadyOccupied = true;
                    //if (current.cameFrom != null)
                    //{
                    //    current.cameFrom.alreadyOccupied = false;

                    //}
                    aStar.Search(GameWorld.gameState.grid, current, goal, path);

                    Searching = false;
                    if (path.Count > 0)
                    {
                        Node node = path.Pop();
                        int x = node.x * GameWorld.gameState.NodeSize;
                        int y = node.y * GameWorld.gameState.NodeSize;
                        //  node.alreadyOccupied = true;
                        // node.cameFrom.alreadyOccupied = false;

                        Move(x, y);
                    }

                }
                //if (path.Count > 0)
                //{
                //    Node node = path.Pop();
                //    int x = node.x * GameWorld.gameState.NodeSize;
                //    int y = node.y * GameWorld.gameState.NodeSize;
                //    //  node.alreadyOccupied = true;
                //    // node.cameFrom.alreadyOccupied = false;

                //    Move(x, y);
                //}

                updateTimerA = 0.0;
            }

            //if (GameObject.Transform.Position.X == goal.x * 100 && GameObject.Transform.Position.Y == goal.y * 100)
            //{
            //    enableAstar = true;

            //}
        }


        public bool IsPlayerInRange(Vector2 target)
        {
            return vision >= Vector2.Distance(GameObject.Transform.Position, target);
        }



        //public Rectangle Rectangle
        //{
        //    get { return rectangle; }

        //}
        public void LoadContent(ContentManager content)
        {
            //sprite = content.Load<Texture2D>("Textures/pl1");

            // astar
           // MouseState PrevMS = Mouse.GetState();


            aStar = new Astar();

            goal = aStar.Node(1, 1);


        }

        public void Move(int x, int y)
        {
            //rectangle.X = x;
            //rectangle.Y = y;
            //GameObject.Transform.Position.X = x;
            //GameObject.Transform.Position.Y = y;
            //Vector2 position = new Vector2(rectangle.X,rectangle.Y);
            GameObject.Transform.Position = new Vector2(x,y);
            //if (GameObject.Transform.Position == goal.GameObject.Transform.Position)
            //{
            //    //Debug.Write("!");
            //    attackingPlayer = true;

            //}
        }
      
     


        /// <summary>
        /// Sets Thread id
        /// If any of the 3 bools are true, worker enters corresponding building (Volcano/PalmTree/MainBuilding)
        /// </summary>
        private void ThreadMethod()
        {
            this.id = Thread.CurrentThread.ManagedThreadId;

            while (GameState.running == true)
            {
                if (attackingPlayer == true)
                {
                    Debug.WriteLine($"{data}{id} is Running;");
                    Thread.Sleep(2000);

                    Debug.WriteLine($"{data}{id} Trying to enter CPU");

                    GameWorld.gameState.playerBuilder.Player.Enter(internalThread);

                    attackingPlayer = false;
                    //delivering = true;

                    GameWorld.gameState.playerBuilder.Player.hp -= dmg;

                    Debug.WriteLine(string.Format($"{data}{id} shutdown"));

                }
                else if (attackingCPU == true)
                {
                    Debug.WriteLine($"{data}{id} is Running;");
                    Thread.Sleep(2000);

                    Debug.WriteLine($"{data}{id} Trying to enter CPU");

                    CPU.Enter(internalThread);

                    attackingPlayer = false;
                    attackingCPU = false;
                    //delivering = true;

                    Debug.WriteLine(string.Format($"{data}{id} shutdown"));

                //    CPU.CPUTakingDamage(internalThread);

                    GameWorld.gameState.cpuBuilder.Cpu.Health -= dmg;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }


        public void StartThread()
        {
            internalThread.IsBackground = true;
            internalThread.Start();
        }
        public override void Awake()
        {
            GameObject.Tag = "Enemy";

            GameObject.Transform.Position = new Vector2(GameWorld.graphics.GraphicsDevice.Viewport.Width / 2, GameWorld.graphics.GraphicsDevice.Viewport.Height);
            // this.position = GameObject.Transform.Position;
            //spriteRenderer = (SpriteRenderer)GameObject.GetComponent("SpriteRenderer");
            StartThread();
        }
        public override string ToString()
        {
            return "Enemy";
        }
        public void Notify(GameEvent gameEvent, Component component)
        {
            if (gameEvent.Title == "Collision" && component.GameObject.Tag == "Player")
            {

                // throw new NotImplementedException();
                attackingPlayer = true;

            }

            if (gameEvent.Title == "Collision" && component.GameObject.Tag == "CPU")
            {
                attackingCPU = true;
            }
        }
    }
}
