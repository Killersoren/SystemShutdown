﻿//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using SystemShutdown.AStar;
//using SystemShutdown.ComponentPattern;
//using SystemShutdown.FactoryPattern;

//namespace SystemShutdown.ObjectPool
//{
//    class EnemyPool : ObjectPool
//    {
//        private static EnemyPool instance;
//        public static EnemyPool Instance
//        {
//            get
//            {
//                if (instance == null)
//                {
//                    instance = new EnemyPool();
//                }
//                return instance;
//            }
//        }

//        protected override void Cleanup(GameObject1 gameObject)
//        {
//            throw new NotImplementedException();
//        }

//        protected override GameObject1 Create(Vector2 position, string type)
//        {
//            return EnemyFactory.Instance.Create(position, type);

//        }
//    }
//}
