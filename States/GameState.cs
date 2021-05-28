using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SystemShutdown.AStar;
using SystemShutdown.BuildPattern;
using SystemShutdown.Buttons;
using SystemShutdown.CommandPattern;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;
using SystemShutdown.GameObjects;
using SystemShutdown.ObjectPool;

namespace SystemShutdown.States
{
    public class GameState : State
    {
        #region Fields

        public Texture2D cursorSprite;
        public Vector2 cursorPosition;
        public static SpriteFont font;
        private List<Enemy> delEnemies;
        private List<Button2> buttons;
        public static bool running = true;
        private Button2 spawnEnemyBtn;
        private Button2 cpuBtn;
        private Button2 activeThreadsBtn;
        private Button2 shutdownThreadsBtn;
        private CPU cpu;
        private string enemyID = "";
        private Texture2D cpuTexture;
        private Texture2D standardBtn;

        private List<Player> players;

        //private List<GameObject> gameObjects;
        private CyclebarDay cyclebarDay;
        private CyclebarNight cyclebarNight;
        private bool isDay;
        private bool isNight;
        private List<MenuObject> menuObjects/* = new List<GameObject>()*/;
        private List<GameObject1> gameObjects = new List<GameObject1>();

        private List<Component> playerObjects;


        public int playerCount = 1;

       
        private InputHandler inputHandler;

        public PlayerBuilder playerBuilder;

        public CPUBuilder cpuBuilder;

     
        Texture2D rectTexture;



        public Grid grid;

        public int NodeSize = Grid.NodeSize;

        public List<Collider> Colliders { get; set; } = new List<Collider>();


        Astar aStar;
        
        //public Texture2D sprite;
        protected Texture2D[] sprites, upWalk;
        private SpriteRenderer spriteRenderer;
        protected float fps;
        private float timeElapsed;
        private int currentIndex;

        public Vector2 position;
        public Rectangle rectangle;
        public Vector2 previousPosition;
        public Vector2 currentDir;
        protected float rotation;
        protected Vector2 velocity;


        private KeyboardState currentKeyState;
        private KeyboardState previousKeyState;

        //private static GameState instance;
        //public static GameState Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new GameState();
        //        }
        //        return instance;
        //    }
        //}

        #endregion

        #region Methods

        #region Constructor
        public GameState()
        {
            delEnemies = new List<Enemy>();
            buttons = new List<Button2>();
            isDay = true;
            isNight = false;
            // cpu = new CPU();

            //Director director = new Director(new PlayerBuilder());
            //gameObjects.Add(director.Contruct());

            //for (int i = 0; i < gameObjects.Count; i++)
            //{
            //    gameObjects[i].Awake();
            //}

            //gameObjects.Add(GameWorld.Instance.Director.Contruct());

            //for (int i = 0; i < gameObjects.Count; i++)
            //{
            //    gameObjects[i].Awake();
            //}

            playerBuilder = new PlayerBuilder();
            cpuBuilder = new CPUBuilder();
        }
        #endregion

        public override void LoadContent()
        {
            cursorSprite = content.Load<Texture2D>("Textures/cursoren");

            Director director = new Director(playerBuilder);
            gameObjects.Add(director.Contruct());

            Director directorCPU = new Director(cpuBuilder);
            gameObjects.Add(directorCPU.Contruct());
            //DirectorCPU directorCpu = new DirectorCPU(cpuBuilder);
            //gameObjects.Add(directorCpu.Contruct());

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Awake();
            }
            //Frederik
            font = content.Load<SpriteFont>("Fonts/font");
            standardBtn = content.Load<Texture2D>("Controls/button");
            cpuTexture = content.Load<Texture2D>("Textures/box");
            shutdownThreadsBtn = new Button2(800, 840, "Shutdown Threads", standardBtn);
            activeThreadsBtn = new Button2(1000, 840, "Thread info", standardBtn);
            spawnEnemyBtn = new Button2(150, 10, "Spawn Enemy", standardBtn);
            cpuBtn = new Button2(700, 700, "CPU", cpuTexture);

            shutdownThreadsBtn.Click += ShutdownBtn_Clicked;
            activeThreadsBtn.Click += ActiveThreadsBtn_Clicked;
            buttons.Add(spawnEnemyBtn);
            buttons.Add(shutdownThreadsBtn);
            buttons.Add(activeThreadsBtn);
            buttons.Add(cpuBtn);

            cyclebarDay = new CyclebarDay(content);
            cyclebarNight = new CyclebarNight(content);
            //camera = new Camera();
            //camera.Follow(playerBuilder);

            //for (int i = 0; i < gameObjects.Count; i++)
            //{
            //    gameObjects[i].Start();
            //}
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Start();
            }
            
            // Frederik
            inputHandler = new InputHandler();

            font = content.Load<SpriteFont>("Fonts/font");

            menuObjects = new List<MenuObject>()
            {
                new MenuObject()
                {
              
                    position = new Vector2(GameWorld.ScreenWidth / 2, GameWorld.ScreenHeight / 2),
                }
            };

            grid = new Grid();
        }

        public override void Update(GameTime gameTime)
        {
            ///<summary>
            ///Updates cursors position
            /// </summary>
            cursorPosition = new Vector2(playerBuilder.player.distance.X - cursorSprite.Width / 2, playerBuilder.player.distance.Y) + playerBuilder.player.GameObject.Transform.Position;

            previousKeyState = currentKeyState;

            currentKeyState = Keyboard.GetState();
            ///<summary>
            /// Goes back to main menu and shuts down all Threads - Frederik
            /// </summary> 
            //if (Keyboard.GetState().IsKeyDown(Keys.Back))
            //{
            //    ShutdownThreads();
            //    GameWorld.ChangeState(new MenuState());
            //}
            if (currentKeyState.IsKeyDown(Keys.P) && !previousKeyState.IsKeyDown(Keys.P))
            {
                SpawnEnemies();


            }

            if (currentKeyState.IsKeyDown(Keys.RightShift) && !previousKeyState.IsKeyDown(Keys.RightShift))
            {
                Mods mods = new Mods();
                mods.Create();

            }

            // Rotates player
            playerBuilder.player.RotatePlayer();

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Update(gameTime);
            }
            InputHandler.Instance.Execute();


            Collider[] tmpColliders = Colliders.ToArray();
            for (int i = 0; i < tmpColliders.Length; i++)
            {
                for (int j = 0; j < tmpColliders.Length; j++)
                {
                    tmpColliders[i].OnCollisionEnter(tmpColliders[j]);
                }
            }


            foreach (var item in buttons)
            {
                item.Update();
            }


        }

        public override void PostUpdate(GameTime gameTime)
        {
            //// When sprites collide = attacks colliding with enemy (killing them) (unload game-specific content)

            //// If player is dead, show game over screen
            //// Frederik
            //if (players.All(c => c.IsDead))
            //{
            //    //highscores can also be added here (to be shown in the game over screen)

            //    _game.ChangeState(new GameOverState(_game, content));
            //}
        }


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin(SpriteSortMode.FrontToBack);
            spriteBatch.Begin();

            //Draws cursor
            spriteBatch.Draw(cursorSprite, cursorPosition, Color.White);

            // Frederik
            //float x = 10f;
            //foreach (var player in players)
            //{
            //    spriteBatch.DrawString(font, "Player: ", /*+ player name,*/ new Vector2(x, 10f), Color.White);
            //    spriteBatch.DrawString(font, "Health: ", /*+ health,*/ new Vector2(x, 30f), Color.White);
            //    spriteBatch.DrawString(font, "Score: ", /*+ score,*/ new Vector2(x, 50f), Color.White);

            //    x += 150;
            //}

            foreach (var item in buttons)
            {
                item.Draw(spriteBatch);
            }

            //Draw selected Enemy ID
            spriteBatch.DrawString(font, $"Enemy: {enemyID} selected", new Vector2(300, 100), Color.Black);

          
               // Frederik
            foreach (var sprite in menuObjects)
            {
                sprite.Draw(gameTime, spriteBatch);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(spriteBatch);
            }

            //Vector2 gridPosition = new Vector2(0, 0);
            //Vector2 pos = gridPosition;
            //int margin = 0;

            //for (int j = 0; j < grid.Height; j++)
            //{
            //    pos.Y = j * (NodeSize + margin) + gridPosition.Y;
            //    for (int i = 0; i < grid.Width; i++)
            //    {
            //       // grid.Node(i, j).rectangle(new Point(i * 100, j * 100));

            //        pos.X = i * (NodeSize + margin) + gridPosition.X;
            //        //grid.Node(i, j).rectangle((int)pos.X, (int)pos.Y, rectTexture.Width, rectTexture.Height);

            //        if (grid.Node(i, j).Passable)
            //        {
            //            //if (goal.x == i && goal.y == j)
            //            //{
            //            //    //spriteBatch.Draw(rectTexture, pos, Color.Blue);
            //            //    spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.Blue);

            //            //}
            //            //else if (grid.Node(i, j).Path)
            //            //{
            //            //    //spriteBatch.Draw(rectTexture, pos, Color.LightBlue);
            //            //    spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.LightBlue);

            //            //}
            //            //else if (grid.Node(i, j).Open)
            //            //{
            //            //    //spriteBatch.Draw(rectTexture, pos, Color.LightCoral);
            //            //    spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.LightCoral);

            //            //}
            //            //else if (grid.Node(i, j).Closed)
            //            //{
            //            //    //spriteBatch.Draw(rectTexture, pos, Color.RosyBrown);
            //            //    spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.RosyBrown);

            //            //}
            //            //else
            //            //{
            //            //    //spriteBatch.Draw(rectTexture, pos, Color.White);
            //            //spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.White);

            //            //}
            //        }
            //        else
            //        {

            //           // spriteBatch.Draw(rectTexture, pos, Color.Gray);
            //           //spriteBatch.Draw(rectTexture, grid.Node(i, j).collisionRectangle, Color.Gray);


            //        }
            //        //if (grid.Node(i, j).nodeOccupiedByEnemy)
            //        //{
            //        //    grid.Node(i, j).Passable = false;
            //        //}
            //        //else if (true)
            //        //{
            //        //    grid.Node(i, j).Passable = true;

            //        //}
                    
            //    }
            //}

            //

            spriteBatch.DrawString(font, $"{GameWorld.gameState.playerBuilder.Player.hp} health points", new Vector2(playerBuilder.Player.GameObject.Transform.Position.X, playerBuilder.Player.GameObject.Transform.Position.Y +20), Color.White);
            spriteBatch.DrawString(font, $"{GameWorld.gameState.playerBuilder.Player.dmg} dmg points", new Vector2(playerBuilder.Player.GameObject.Transform.Position.X , playerBuilder.Player.GameObject.Transform.Position.Y +40), Color.White);

            spriteBatch.DrawString(font, $"CPU health {cpuBuilder.Cpu.Health}", cpuBuilder.Cpu.GameObject.Transform.Position, Color.White);


            spriteBatch.End();

        }

        //private void SpawnMod()
        //{
        //    List<Mods> pickupAble;

        //    GameWorld.repo.Open();
        //    pickupAble = GameWorld.repo.FindMods(Mods.Id);
        //}


        public void ApplyMod()
        {
            
            Random rnd = new Random();
            int randomnumber = rnd.Next(1, 5);

            List<Effects> pickupable = new List<Effects>();
            
            GameWorld.repo.Open();
            pickupable = GameWorld.repo.FindEffects(randomnumber);

            //playerBuilder.player.dmg += pickupAble.Effect;
            GameWorld.repo.Close();

            Random rndeffect = new Random();
            int randomeffect = rndeffect.Next(0, 3);

            Effects choseneffect = pickupable[randomeffect];

            Debug.WriteLine($"{choseneffect.Effectname}");

            if (choseneffect.ModFK == 1)
            {
                playerBuilder.player.dmg += choseneffect.Effect;
            }
            else if (choseneffect.ModFK == 2)
            {
                //movespeed
            }
            else if (choseneffect.ModFK == 3)
            {
                //attackspeed
            }
            else if (choseneffect.ModFK == 4)
            {
                playerBuilder.player.Health += choseneffect.Effect;
            }


        }
        private Node GetRandomPassableNode()
        {
            Random rndd = new Random();
            var tmppos = grid.nodes[rndd.Next(1, grid.Width), rndd.Next(1, grid.Height)];
            //var tmppos = grid.nodes[1,1];

            return tmppos;
        }
        
       
        private void SpawnEnemies()
        {
            //spawnTime += delta;
            //if (spawnTime >= cooldown)
            //{
            GameObject1 go = EnemyPool.Instance.GetObject();

            //Random rnd = new Random();
            //go.Transform.Position = new Vector2(rnd.Next(100, GameWorld.renderTarget.Width - 200), 500);

            Node enemypos = GetRandomPassableNode();

            while(!enemypos.Passable || enemypos== null)
            {
                enemypos = GetRandomPassableNode();
            }
            go.Transform.Position = new Vector2(enemypos.x*100, enemypos.y*100);


            GameState.running = true;


            AddGameObject(go);
               // spawnTime = 0;
            //}
        }

        public void AddGameObject(GameObject1 go)
        {
            go.Awake();
            go.Start();
            gameObjects.Add(go);
            Collider c = (Collider)go.GetComponent("Collider");
            if (c != null)
            {
                Colliders.Add(c);
            }
        }

        public void RemoveGameObject(GameObject1 go)
        {
            gameObjects.Remove(go);
        }

        /// <summary>
        /// Shows all threads aktive and Total number
        /// Used for debugging purpose only and is not part of the game. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActiveThreadsBtn_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process procces = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.ProcessThreadCollection threadCollection = procces.Threads;
            string threads = string.Empty;
            foreach (System.Diagnostics.ProcessThread proccessThread in threadCollection)
            {
                threads += string.Format("Thread Id: {0}, ThreadState: {1}\r\n", proccessThread.Id, proccessThread.ThreadState);
            }
            Debug.WriteLine($"{threads}");
            int number = Process.GetCurrentProcess().Threads.Count;
            Debug.WriteLine($"Total number of aktive threads: {number}");
        }

        /// <summary>
        /// Shutdown all enemy threads and clears enemies from draw/update list
        /// Used both as a button for testing and at game exit
        /// </summary>
        public void ShutdownThreads()
        {
            running = false;
        }
        /// <summary>
        /// Calls ShutdownThreads method on click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShutdownBtn_Clicked(object sender, EventArgs e)
        {
            ShutdownThreads();
        }
        //protected void Animate(GameTime gametime)
        //{
        //    if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.A))
        //    {
        //        //Giver tiden, der er g�et, siden sidste update
        //        timeElapsed += (float)gametime.ElapsedGameTime.TotalSeconds;

        ////        //Beregner currentIndex
        ////        currentIndex = (int)(timeElapsed * fps);
        ////        spriteRenderer.Sprite = upWalk[currentIndex];

        //        //Checks if animation needs to restart
        //        if (currentIndex >= upWalk.Length - 1)
        //        {
        //            //Resets animation
        //            timeElapsed = 0;
        //            currentIndex = 0;
        //        }
        //    }
        //}
        #endregion
    }
}