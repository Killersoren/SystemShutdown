﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using SystemShutdown.ComponentPattern;
using SystemShutdown.Components;

namespace SystemShutdown.AStar
{
    //Ras
    public class Grid
    {
        public static int NodeSize = 100;

        public int Width = 35;
            public int Height = 35;
            public Node[,] nodes;
            
            public Grid()
            {
                Random rand = new Random();
                nodes = new Node[Width, Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        nodes[x, y] = new Node();
                        nodes[x, y].x = x;
                        nodes[x, y].y = y;
                    //


                    //


                    if ((x != 0 && y != 0) && (x != Width - 1 && y != Height - 1) &&
                        rand.Next(1, 25) < 5)
                    {
                        nodes[x, y].Passable = false;
                    }


                    if (x > 12 && x < 22 && y > 12 && y < 22)
                    {
                        nodes[x, y].Passable = true;
                    }

                   


                    if (x > Width / 2 -5 && x < Width / 2 +5 && y == Height /2 + 5)
                    {
                        nodes[x, y].Passable = false;
                    }

                    if (x > Width / 2 - 5 && x < Width / 2 + 5 && y == Height / 2 -5)
                    {
                        nodes[x, y].Passable = false;
                    }

                    if (y > Height / 2 - 5 && y < Height / 2 + 5 && x == Width / 2 - 5)
                    {
                        nodes[x, y].Passable = false;
                    }

                    if (y > Height / 2 - 5 && y < Height / 2 + 5 && x == Width / 2 + 5)
                    {
                        nodes[x, y].Passable = false;
                    }


                    if (x == Width / 2 && y == Height / 2 + 5 || x == Width / 2 && y == Height / 2 - 5)
                    {
                        nodes[x, y].Passable = true;
                    }
                    if (y == Height / 2 && x == Width / 2 + 5 || y == Height / 2 && x == Width / 2 - 5)
                    {
                        nodes[x, y].Passable = true;
                    }


                    if (nodes[x, y].Passable == false)
                    {
                        GameObject1 nodeGO = new GameObject1();
                        SpriteRenderer nodeSR = new SpriteRenderer("Textures/pl4");
                        nodeGO.AddComponent(nodeSR);
                        nodeGO.Transform.Position = new Vector2(x * 100, y * 100);
                        nodeSR.Origin = new Vector2(nodeSR.Sprite.Width / 2, (nodeSR.Sprite.Height) / 2);

                        nodeGO.AddComponent(new Collider(nodeSR, nodes[x, y]) { CheckCollisionEvents = false });
                        nodeGO.AddComponent(nodes[x, y]);
                        GameWorld.gameState.AddGameObject(nodeGO);
                    }
                    if (y == 0 || y == Height - 1 || x == 0 || x == Width - 1)
                    {
                        nodes[x, y].Passable = false;

                        GameObject1 nodeGO = new GameObject1();
                        SpriteRenderer nodeSR = new SpriteRenderer("Textures/nogo");
                        nodeGO.AddComponent(nodeSR);
                        nodeGO.Transform.Position = new Vector2(x * 100, y * 100);
                        nodeSR.Origin = new Vector2(nodeSR.Sprite.Width / 2, (nodeSR.Sprite.Height) / 2);

                        nodeGO.AddComponent(new Collider(nodeSR, nodes[x, y]) { CheckCollisionEvents = false });
                        nodeGO.AddComponent(nodes[x, y]);
                        GameWorld.gameState.AddGameObject(nodeGO);
                    }


                    //if (nodes[x, y].Passable == false)
                    //{
                    //    nodes[x, y].rectangle(new Point(x, y));
                    //}



                    
                    //if (x == 7 * Width / 8 && y < 7 * Height / 8 && y > Height / 4)
                    //{
                    //    nodes[x, y].Passable = false;
                    //}
                }
        }

        public Node Node(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return nodes[x, y];
            else
                return null;
        }

        public void ResetState()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    nodes[x, y].f = int.MaxValue;
                    nodes[x, y].g = 0;
                    nodes[x, y].h = 0;
                    nodes[x, y].cameFrom = null;
                    nodes[x, y].Path = false;
                    nodes[x, y].Open = false;
                    nodes[x, y].Closed = false;
                }
        }
    }



}
