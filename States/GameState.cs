using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SystemShutdown.AStar;
using SystemShutdown.BuildPattern;
//using SystemShutdown.CommandPattern;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;
using SystemShutdown.FactoryPattern;
//using SystemShutdown.ObjectPool;

namespace SystemShutdown.States
{
    public class GameState : State
    {
        #region Fields
        private Texture2D backgroundSprite;
        private Vector2 backgroundPos;
        private Vector2 backgroundOrigin;

        private Texture2D cursorSprite;
        private Vector2 cursorPosition;

        private static SpriteFont font;
        private string enemyID = "";
        public bool IsThreadsRunning;

        private Song nightMusic;

        private List<GameObject1> gameObjects = new List<GameObject1>();
        private PlayerBuilder playerBuilder;
        private CPUBuilder cpuBuilder;

        private int aliveEnemies = 0;
        private int days = 1;
        private Grid grid;
        private List<Collider> colliders = new List<Collider>();

        private KeyboardState currentKeyState;
        private KeyboardState previousKeyState;

        private Color _healthColor = Color.White;
        private Color _dmgColor = Color.White;
        private Color _killsColor = Color.White;
        public Color _msColor = Color.White;

        public List<ProjectileEffect> NewEffects = new List<ProjectileEffect>();

        private List<ProjectileEffect> effects = new List<ProjectileEffect>();
        //public List<ProjectileEffect> effects = new List<ProjectileEffect>();

        public List<ProjectileEffect> Effects { get { return effects; } set { effects = value; } }

        public Texture2D projektilEffectTexture;
       
    double enemySpawnTimer = 0.0;

        private float dmgTimer = 2f;
        private float healthTimer = 2f;
        private float countDown = 0.05f;
        public Color HealthColor { get { return _healthColor; } set { _healthColor = value; } }
        public Color DmgColor { get { return _dmgColor; } set { _dmgColor = value; } }
        public Color KillsColor { get { return _killsColor; } set { _killsColor = value; } }
        public Color MsColor { get { return _msColor; } set { _msColor = value; } }
        public int AliveEnemies { get { return aliveEnemies; } set { aliveEnemies = value; } }
        public List<Collider> Colliders { get { return colliders; } set { colliders = value; } }
        public CPUBuilder CpuBuilder { get { return cpuBuilder; } set { cpuBuilder = value; } }

        public PlayerBuilder PlayerBuilder { get { return playerBuilder; } set { playerBuilder = value; } }
        public Grid Grid { get { return grid; } set { grid = value; } }
        public List<GameObject1> GameObjects { get { return gameObjects; } set { gameObjects = value; } }
        public Vector2 CursorPosition { get { return cursorPosition; } set { cursorPosition = value; } }
        public int Days { get { return days; } set { days = value; } }

        public bool dmgColorTimer { get; set; }
        public bool healthColorTimer { get; set; }

        #endregion

        #region Methods

        #region Constructor
        /// <summary>
        /// Instantiates a Player and a CPU builder
        /// </summary>
        public GameState()
        {
            PlayerBuilder = new PlayerBuilder();
            CpuBuilder = new CPUBuilder();
        }
        #endregion

        public void ChangeDmgColor()
        {
            DmgColor = Color.YellowGreen;
        }
        public void ChangeHealthColor()
        {
            HealthColor = Color.YellowGreen;

        }

        public override void LoadContent()
        {
            backgroundSprite = content.Load<Texture2D>("Backgrounds/circuitboard");
            cursorSprite = content.Load<Texture2D>("Textures/cursoren");
            projektilEffectTexture = GameWorld.Instance.Content.Load<Texture2D>("Textures/cursoren");

            // Backgrounds music
            //dayMusic = content.Load<Song>("Sounds/song1");

            nightMusic = content.Load<Song>("Sounds/song02");

            MediaPlayer.Play(nightMusic);
            MediaPlayer.IsRepeating = true;

            Director directorCPU = new Director(CpuBuilder);
            GameObjects.Add(directorCPU.Contruct());

            Director director = new Director(PlayerBuilder);
            GameObjects.Add(director.Contruct());

            //Awakes and Starts each gameobject
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Awake();
            }
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Start();
            }
            // Frederik

            font = content.Load<SpriteFont>("Fonts/font");
            Grid = new Grid();

            // Enables threads to be run and spawns the first wave of enemies
            IsThreadsRunning = true;
            //SpawnEnemiesAcordingToDayNumber();
        }




        /// <summary>
        /// Ras
        /// Spawn Enemies acording to daynumber and day/ night cycle.
        /// Spawns a enemy each day for each days passed.
        /// Double spawns in night cycle
        /// If day is > 5, trojan anemies is spawned aswell
        /// maximum numbers of enemies is 50. 
        /// </summary>
        public void SpawnEnemiesAcordingToDayNumber()
        {
            for (int i = 0; i < Days && i < 10; i++)
            {
                if (aliveEnemies < 50)
                {
                    if (GameWorld.Instance.IsDay)
                    {
                        SpawnBugEnemies(SetEnemySpawnInCorner());
                        if (i >= 5)
                        {
                            SpawnTrojanEnemies(SetEnemySpawnInCorner());
                        }
                    }
                    else
                    {
                        SpawnBugEnemies(SetEnemySpawnInCorner());
                        SpawnBugEnemies(SetEnemySpawnInCorner());
                        if (i >= 5)
                        {
                            SpawnTrojanEnemies(SetEnemySpawnInCorner());
                        }
                    }
                    
                }

                //if (aliveEnemies < 50)
                //{
                   
                //    SpawnBugEnemies(SetEnemySpawnInCorner());
                //    SpawnBugEnemies(SetEnemySpawnInCorner());
                //    SpawnBugEnemies(SetEnemySpawnInCorner());
                //    SpawnBugEnemies(SetEnemySpawnInCorner());
                //    SpawnBugEnemies(SetEnemySpawnInCorner());
                //    SpawnTrojanEnemies(SetEnemySpawnInCorner());
                //}
            }
            Debug.WriteLine($"Enemies alive {aliveEnemies}");

        }
        /// <summary>
        /// Ras
        /// Sets enemy start position to one of 4 corners of the grid with a Random (1.5). 
        /// </summary>
        /// <returns></returns>
        public Vector2 SetEnemySpawnInCorner()
        {
            Random rndd = new Random();
            var rndpos = rndd.Next(1, 5);
            int x = 0;
            int y = 0;

            if (rndpos == 1)
            {
                x = 1;
                y = 1;
            }
            else if (rndpos == 2)
            {
                x = GameWorld.Instance.GameState.Grid.Width - 2;
                y = 1;
            }
            else if (rndpos == 3)
            {
                x = 1;
                y = GameWorld.Instance.GameState.Grid.Height - 2;
            }
            else if (rndpos == 4)
            {
                x = GameWorld.Instance.GameState.Grid.Width - 2;
                y = GameWorld.Instance.GameState.Grid.Height - 2;
            }
            return new Vector2(x * 100, y * 100);

        }
        public override void Update(GameTime gameTime)
        {
           
            enemySpawnTimer += GameWorld.Instance.DeltaTime;
            if (enemySpawnTimer >= 1)
            {
                SpawnEnemiesAcordingToDayNumber();
                enemySpawnTimer = 0.0;
            }

            backgroundPos = new Vector2(GameWorld.Instance.RenderTarget.Width / 2, GameWorld.Instance.RenderTarget.Height / 2);
            backgroundOrigin = new Vector2(backgroundSprite.Width / 2, backgroundSprite.Height / 2);
            ///<summary>
            ///Updates cursors position
            /// </summary>
            CursorPosition = new Vector2(PlayerBuilder.player.distance.X - cursorSprite.Width / 2,
                PlayerBuilder.player.distance.Y) + PlayerBuilder.player.GameObject.Transform.Position;
            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            ///<summary>
            /// Goes back to main menu and shuts down all Threads - Frederik
            /// </summary> 
            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
               ShutdownThreads();
                GameWorld.ChangeState(new GameOverState());
            }

#if DEBUG
            // Spawns a bug in debug by pressing P
            if (currentKeyState.IsKeyDown(Keys.P) && !previousKeyState.IsKeyDown(Keys.P))
            {
                SpawnBugEnemies(SetEnemySpawnInCorner());
            }

#endif

            // Updates each gameobject
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Update(gameTime);
            }
            // Checks for collisions between gameobjects colliders
            Collider[] tmpColliders = colliders.ToArray();
            for (int i = 0; i < tmpColliders.Length; i++)
            {
                for (int j = 0; j < tmpColliders.Length; j++)
                {
                    tmpColliders[i].OnCollisionEnter(tmpColliders[j]);
                }
            }

            if (dmgColorTimer == true)
            {
                ChangeDmgColor();
                dmgTimer -= countDown;

                if (dmgTimer <= 0)
                {
                    dmgColorTimer = false;
                    Debug.WriteLine("IT WORKS!!!");
                    DmgColor = Color.White;
                }
            }
            if (healthColorTimer == true)
            {
                ChangeHealthColor();
                healthTimer -= countDown;

                if (healthTimer <= 0)
                {
                    healthColorTimer = false;
                    Debug.WriteLine("IT WORKS for health aswell!!!");
                    HealthColor = Color.White;
                }
            }

            // effects = Effects;
            // var tmpEffects = effects;
            //foreach (ProjectileEffect item in ExpiredEffects)
            //{
            //    if (item.timer > 2)
            //    {
            //        tmpEffects.Remove(item);
            //    }
            //}

            //foreach (ProjectileEffect item in NewEffects)
            //{
            //    tmpEffects.Add(item);
            //}
            //effects = tmpEffects;
            // var tmpeffects = effects;
            // foreach (ProjectileEffect item in tmpeffects.)
            foreach (ProjectileEffect item in new List<ProjectileEffect>(effects))
            {
                item.Update(gameTime);
            }
            // NewEffects.Clear();
            //ExpiredEffects.Clear();
            GameOver();
        }
        
        //public override void PostUpdate(GameTime gameTime)
        //{
        //    //// When sprites collide = attacks colliding with enemy (killing them) (unload game-specific content)

        //    //// If player is dead, show game over screen
        //    //// Frederik
        //    //if (players.All(c => c.IsDead))
        //    //{
        //    //    //highscores can also be added here (to be shown in the game over screen)

        //    //    _game.ChangeState(new GameOverState(_game, content));
        //    //}
        //}

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundSprite, backgroundPos, null, Color.White, 0, backgroundOrigin, 1f, SpriteEffects.None, 0.1f);

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Draw(spriteBatch);
            }

            //Draws cursor
            spriteBatch.Draw(cursorSprite, CursorPosition, Color.White);

            // Draws CPU Health
            spriteBatch.DrawString(font, $"CPU health {CpuBuilder.Cpu.Health}", CpuBuilder.Cpu.GameObject.Transform.Position, Color.White);
            var tmpeffects = effects;

            foreach (ProjectileEffect item in tmpeffects)
            {
                item.Draw(spriteBatch);
            }
            spriteBatch.End();
           
        }

        /// <summary>
        /// Draws Player stats
        /// </summary>
        public void DrawPlayerStats(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, $"| - - Player Stats - - | ", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X - 900, PlayerBuilder.Player.GameObject.Transform.Position.Y + 360), _killsColor);
            spriteBatch.DrawString(font, $"Enemies killed:  {PlayerBuilder.Player.kills}", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X - 900, PlayerBuilder.Player.GameObject.Transform.Position.Y + 400), _killsColor);
            spriteBatch.DrawString(font, $"Health Points: {PlayerBuilder.Player.Health}", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X - 900, PlayerBuilder.Player.GameObject.Transform.Position.Y + 440), _healthColor);
            spriteBatch.DrawString(font, $"Damage:  {PlayerBuilder.Player.dmg}", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X - 900, PlayerBuilder.Player.GameObject.Transform.Position.Y + 480), _dmgColor);
            spriteBatch.DrawString(font, $"Day:  {Days}", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X + 640, PlayerBuilder.Player.GameObject.Transform.Position.Y - 390), Color.White);
            //spriteBatch.DrawString(font, $"{PlayerBuilder.Player.playersMods.Count} Mods", new Vector2(PlayerBuilder.Player.GameObject.Transform.Position.X, PlayerBuilder.Player.GameObject.Transform.Position.Y + 80), Color.White);
        }

        //public void projektilEffects(SpriteBatch spriteBatch)
        //{
            
        //    foreach (ProjectileEffect item in Effects)
        //    {
        //        item.Draw(spriteBatch);
        //    }
        //    //double timer = GameWorld.Instance.DeltaTime;
        //    //timer++;
        //}


        /// <summary>
        /// Ras
        /// Spawns a Trojan enemy at parameter position. 
        /// </summary>
        /// <param name="position"></param>
        private void SpawnTrojanEnemies(Vector2 position)
        {
            GameObject1 go = EnemyFactory.Instance.Create(position, "Trojan");
            AddGameObject(go);
        }
        /// <summary>
        /// Ras
        /// Spawns a Bug enemy at parameter position. 
        /// </summary>
        /// <param name="position"></param>
        public void SpawnBugEnemies(Vector2 position)
        {
            GameObject1 go = EnemyFactory.Instance.Create(position, "Bug");
            AddGameObject(go);
        }

        /// <summary>
        /// Runs Start and Awake on GameObject
        /// Adds the Gameobject to Gameobjects list
        /// Adds collider component to list of colliders
        /// </summary>
        /// <param name="go"></param>
        public void AddGameObject(GameObject1 go)
        {
            go.Awake();
            go.Start();
            GameObjects.Add(go);
            Collider c = (Collider)go.GetComponent("Collider");
            if (c != null)
            {
                colliders.Add(c);
            }
        }

        /// <summary>
        /// Removes a Gameobject from Gameobjects list.
        /// </summary>
        /// <param name="go"></param>
        public void RemoveGameObject(GameObject1 go)
        {
            GameObjects.Remove(go);
        }

        /// <summary>
        /// If Player or CPU Health is 0 or below. 
        /// Shuts down all threads, change state from gamestate to gameOverState 
        /// </summary>
        public void GameOver()
        {
            if (GameWorld.Instance.GameState.CpuBuilder.Cpu.Health <= 0 || GameWorld.Instance.GameState.PlayerBuilder.Player.Health <= 0)
            {
                GameWorld.Instance.deathEffect.Play();
                ShutdownThreads();
                //
                //GameWorld.Instance.repo.Open();
                //GameWorld.Instance.repo.RemoveTables();
                //GameWorld.Instance.repo.Close();
                GameWorld.ChangeState(GameWorld.Instance.GameOverState);
            }
        }

        /// <summary>
        /// Shutdown all enemy threads and clears enemies from draw/update list
        /// Used both as a button for testing and at game exit
        /// </summary>
        public void ShutdownThreads()
        {
            IsThreadsRunning = false;
        }
        #endregion
    }
}