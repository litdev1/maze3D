using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using plc3D.Properties;

namespace plc3D
{
    /// <summary>
    /// Methods to add objects to the scene
    /// </summary>
    public class Scene
    {
        private Viewport viewport;
        private List<Geom> geoms = new List<Geom>();
        private List<Enemy> enemies = new List<Enemy>();

        /// <summary>
        /// Constructor takes the ViewPort3D control
        /// </summary>
        /// <param name="_viewport"></param>
        public Scene(Viewport _viewport)
        {
            viewport = _viewport;
        }

        /// <summary>
        /// Get the list of created Geoms that are general (not enemies)
        /// </summary>
        public List<Geom> GetGeoms
        {
            get { return geoms; }
        }

        /// <summary>
        /// Get the list of created Enemies
        /// </summary>
        public List<Enemy> GetEnemies
        {
            get { return enemies; }
        }

        /// <summary>
        /// Create a maze enviroment consisting of walls, floor and ceiling
        /// </summary>
        /// <param name="layout">A string list of characters describing cells in the maze</param>
        public void MakeMaze(string[] layout)
        {
            BitmapSource zeb = Utilities.GetBitmapImage(Resources.zeb);
            BitmapSource stones = Utilities.GetBitmapImage(Resources.stones);
            BitmapSource water = Utilities.GetBitmapImage(Resources.water);
            BitmapSource wall = Utilities.GetBitmapImage(Resources.wall);
            BitmapSource coffee = Utilities.GetBitmapImage(Resources.coffee);
            BitmapSource bug = Utilities.GetBitmapImage(Resources.bug);
            BitmapSource head = Utilities.GetBitmapImage(Resources.head);
            BitmapSource tree = Utilities.GetBitmapImage(Resources.Tree);
            BitmapSource stonesBack = Utilities.ReflectImage(stones, 0);
            BitmapSource waterBack = Utilities.ReflectImage(water, 0);
            BitmapSource wallBack = Utilities.ReflectImage(wall, 0);
            BitmapSource coffeeBack = Utilities.ReflectImage(coffee, 0);
            BitmapSource bugBack = Utilities.ReflectImage(bug, 0);
            BitmapSource headBack = Utilities.ReflectImage(head, 0);
            BitmapSource treeBack = Utilities.ReflectImage(tree, 0);

            for (int z = 0; z < layout.Length; z++)
            {
                for (int x = 0; x < layout[z].Length; x++)
                {
                    //Floor and ceiling
                    AddWall(x, 0, z, eWall.YNEG, Utilities.GetBitmapImage(Resources.stones), stonesBack);
                    AddWall(x, 0, z, eWall.YPOS, Utilities.GetBitmapImage(Resources.water), waterBack);
                    BitmapSource image = wall;
                    BitmapSource imageBack = wallBack;
                    switch (layout[z][x])
                    {
                        case 'a':
                            image = coffee;
                            imageBack = coffeeBack;
                            break;
                        case 'b':
                            image = bug;
                            imageBack = bugBack;
                            break;
                        case 'c':
                            image = head;
                            imageBack = headBack;
                            break;
                        case 'd':
                            image = tree;
                            imageBack = treeBack;
                            break;
                    }
                    //XNEG
                    if (x == 0 || layout[z][x] == ' ')
                    {
                        AddWall(x, 0, z, eWall.XNEG, image, imageBack);
                    }
                    //XPOS
                    if (x == layout[z].Length - 1 || layout[z][x] == ' ')
                    {
                        AddWall(x, 0, z, eWall.XPOS, image, imageBack);
                    }
                    //ZNEG
                    if (z == 0 || x >= layout[z - 1].Length || layout[z][x] == ' ')
                    {
                        AddWall(x, 0, z, eWall.ZNEG, image, imageBack).Rotate1(new Vector3D(0, 0, 1), 90);
                    }
                    //ZPOS
                    if (z == layout.Length - 1 || x >= layout[z + 1].Length || layout[z][x] == ' ')
                    {
                        AddWall(x, 0, z, eWall.ZPOS, image, imageBack).Rotate1(new Vector3D(0, 0, 1), 90);
                    }
                    if (layout[z][x] == 'L')
                    {
                        viewport.AddPointLight(Colors.White, new Point3D(x + 0.5, 0.98, z + 0.5), 5);
                    }
                    else if (layout[z][x] == 'C')
                    {
                        AddCone(x, 0, z, Colors.Red);
                    }
                    else if (layout[z][x] == 'S')
                    {
                        AddSphere(x, 0, z, Colors.Yellow);
                    }
                    else if (layout[z][x] == 'Z')
                    {
                        Geom geom = AddModel3DS(x, 0, z, "models\\zeb.3ds");
                        geom.Rotate1(new Vector3D(1, 0, 0), -90);
                        geom.Material = new DiffuseMaterial(new ImageBrush(zeb));
                    }
                }
            }
        }

        /// <summary>
        /// Add a wall to the scene
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="wall"></param>
        /// <param name="image"></param>
        /// <param name="imageBack"></param>
        private Geom AddWall(double x, double y, double z, eWall wall, BitmapSource image, BitmapSource imageBack)
        {
            Geom geom = new Geom();
            geom.Name = "Wall";
            geoms.Add(geom);
            geom.Wall(wall);
            geom.Translate(x, y, z);
            geom.Material = new DiffuseMaterial(new ImageBrush(image));
            geom.BackMaterial = new DiffuseMaterial(new ImageBrush(imageBack));
            viewport.AddGeometryModel(geom.GetGeometryModel());
            return geom;
        }

        /// <summary>
        /// Add a cone (enemy) to the scene
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color"></param>
        private Geom AddCone(double x, double y, double z, System.Windows.Media.Color color)
        {
            Enemy enemy = new Enemy();
            enemy.Name = "Cone";
            enemies.Add(enemy);
            enemy.Cone();
            enemy.Translate(x, y, z);
            enemy.Material = new DiffuseMaterial(new SolidColorBrush(color));
            viewport.AddGeometryModel(enemy.GetGeometryModel());
            return enemy;
        }

        /// <summary>
        /// Add a sphere (enemy) to the scene
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private Geom AddSphere(double x, double y, double z, System.Windows.Media.Color color)
        {
            Enemy enemy = new Enemy();
            enemy.Name = "Sphere";
            enemies.Add(enemy);
            enemy.Sphere();
            enemy.Translate(x, y, z);
            enemy.Material = new DiffuseMaterial(new SolidColorBrush(color));
            viewport.AddGeometryModel(enemy.GetGeometryModel());
            return enemy;
        }

        /// <summary>
        /// Add a 3ds geometry (enemy) to the scene
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private Geom AddModel3DS(double x, double y, double z, string fileName)
        {
            Enemy enemy = new Enemy();
            enemy.Name = "Model3DS";
            enemies.Add(enemy);
            enemy.Model3DS(fileName);
            enemy.Translate(x, y, z);
            viewport.AddGeometryModel(enemy.GetGeometryModel());
            return enemy;
        }
    }
}
