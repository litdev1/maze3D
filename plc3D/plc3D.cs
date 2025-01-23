using plc3D.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using plcLib;

namespace plc3D
{
    /// <summary>
    /// Top level class for the ViewPort3D control
    /// </summary>
    public class Plc3D
    {
        //Set this to suitable location to control by plc
        private string database = "C:\\Users\\steve\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\3381f5rv.dev-edition-default\\storage\\default\\https+++developtube.com\\ls\\data.sqlite";

        private Grid grid;
        private Viewport viewport;
        private Scene scene;
        private double wallProximity = 0.1;
        private double enemyProximity = 0.3;
        private double speed = 1;
        private string[] layout;
        private Random rand = new Random();
        private Vector hotspot = new Vector(0.5, 0.5);

        private Canvas canvas;
        private TextBlock tbInfo;
        private TextBlock tbInitial;
        private Image elTarget;

        private Stopwatch stopWatch = Stopwatch.StartNew();
        private double lastTime = 0;
        private double startTime;
        private double time;
        private int step;
        private double dt;

        private Com com = null;
        private plcAPI plcCom = null;

        private Dictionary<string, double> keysDown = new Dictionary<string, double>();

        public Plc3D(Grid mainGrid, bool performance = true)
        {
            grid = mainGrid;

            //The main viewport
            grid.Background = new SolidColorBrush(Colors.DeepSkyBlue);
            viewport = new Viewport(performance);
            viewport.AddAmbientLight(Color.FromArgb(255, 40, 40, 40));
            viewport.SizeChanged += Viewport_SizeChanged;
            grid.Children.Add(viewport);

            //To overlay things
            canvas = new Canvas()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            grid.Children.Add(canvas);

            //Info overlays
            tbInfo = new TextBlock() { Foreground = new SolidColorBrush(Colors.Red) };
            canvas.Children.Add(tbInfo);
            BitmapSource sights = Utilities.GetBitmapImage(Resources.sights);
            elTarget = new Image() { Width = 60, Source = sights, Opacity = 0.5 };
            canvas.Children.Add(elTarget);

            //Initial info
            tbInitial = new TextBlock() { 
                Foreground = new SolidColorBrush(Colors.Red),
                TextAlignment = TextAlignment.Center,
                FontSize = 30,
                Text = "Arrow keys to move, Space to boost, Escape to exit\n\nWSAD to simulate PLC",
            };
            canvas.Children.Add(tbInitial);

            //X - Open
            //  - Wall
            //a to d - Wall art
            //L - Open with Light
            //C - Open with Cone
            //S - Open with Sphere
            //Z - Open with Zeb
            layout = new string[6];
            layout[5] = "XXaXbXcXdXCXXXXXXXXXXXXXXXXXXXX";
            layout[4] = "XXXLXdXXXXXZXX   S   X        X";
            layout[3] = "XXXX  LXXXa XX   LXXXXSXXX    Z";
            layout[2] = "bXXd    XXCXXb        X  X    X";
            layout[1] = "XXXXXcXaSLXXdXXXXXXXXXXXXXXXXXX";
            layout[0] = "XXXcXZXX";

            //I/O for layout
            File.WriteAllLines("layout.txt", layout);
            layout = File.ReadAllLines("layout.txt");

            //Create the objects in the scene
            scene = new Scene(viewport);
            scene.MakeMaze(layout);

            //PLC communication with sqlite
            //com = new Com(database);
            //PLC communication with plcLib
            //plcCom = new plcAPI();
            //plcCom.COM_setupPLC();
            //plcCom.COM_setup();
            string source = "namespace plc3D\r\n{\r\n" +
    "    public static class plcCode\r\n" +
    "    {\r\n" +
    "        private static int TIMER0 = 0;\r\n" +
    "        private static int TIMER1 = 0;\r\n\r\n" +
    "        public static void setup()\r\n" +
    "        {\r\n\r\n        }\r\n\r\n" +
    "        public static void loop()\r\n" +
    "        {\r\n" +
    "            //Forwards W key or toggle forward in bursts S key\r\n" +
    "            plcLibrary._in(plcLibrary.X3);\r\n" +
    "            plcLibrary.timerCycle(ref TIMER0, 200, ref TIMER1, 500);   // cycle activated by X3 (S)\r\n" +
    "            plcLibrary.orBit(plcLibrary.X0);\r\n" +
    "            plcLibrary._out(plcLibrary.Y0);\r\n\r\n" +
    "            //Left A key\r\n" +
    "            plcLibrary._in(plcLibrary.X1);\r\n" +
    "            plcLibrary._out(plcLibrary.Y1);\r\n\r\n" +
    "            //Right D key\r\n" +
    "            plcLibrary._in(plcLibrary.X2);\r\n" +
    "            plcLibrary._out(plcLibrary.Y2);\r\n" +
    "        }\r\n" +
    "    }\r\n}";

            plcLibrary.COM_setup(source);

            keysDown["Up"] = 0;
            keysDown["Left"] = 0;
            keysDown["Right"] = 0;

            //The game loop - controlled by OS to update as required
            step = 0;
            startTime = stopWatch.Elapsed.TotalMilliseconds;
            CompositionTarget.Rendering += Loop;
        }

        /// <summary>
        /// Update visuals when the Viewport3D control is resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Viewport_SizeChanged(object sender, EventArgs e)
        {
            Canvas.SetLeft(elTarget, viewport.ActualWidth * hotspot.X - elTarget.ActualWidth / 2);
            Canvas.SetTop(elTarget, viewport.ActualHeight * hotspot.Y - elTarget.ActualHeight / 2);
            Canvas.SetLeft(tbInitial, viewport.ActualWidth * 0.5 - tbInitial.ActualWidth / 2);
            Canvas.SetTop(tbInitial, viewport.ActualHeight * 0.1 - tbInitial.ActualHeight / 2);
        }

        /// <summary>
        /// Get the Viewport3D control
        /// </summary>
        public Viewport GetViewport
        {
            get { return viewport; }
        }

        /// <summary>
        /// Game loop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loop(object sender, EventArgs e)
        {
            step++;
            double now = stopWatch.Elapsed.TotalMilliseconds;
            time = (now - startTime) / 1000;
            dt = (now - lastTime) / 1000;
            lastTime = now;
            double fps = 1 / dt;
            tbInfo.Text = fps.ToString("0") + " fps";
            tbInitial.Opacity = Math.Min(1, Math.Max(0, (300 - step) / 100.0));

            if (Utilities.KeyDown("Escape"))
            {
                Environment.Exit(0);
            }
            UpdateSprites();
            UpdateCollisions();
            UpdateCamera(dt);
        }

        /// <summary>
        /// Animate sprites
        /// </summary>
        private void UpdateSprites()
        {
            foreach (Enemy enemy in scene.GetEnemies)
            {
                if (enemy.NumAnimate == 0)
                {
                    Point3D center = Geom.GetCenter(enemy.GetGeometryModel().Bounds);
                    int x = (int)center.X;
                    int y = (int)center.Y;
                    int z = (int)center.Z;

                    //Possible directions
                    List<Vector3D> dirs = new List<Vector3D>();
                    if (x > 0 && layout[z][x - 1] != ' ') dirs.Add(new Vector3D(-1, 0, 0));
                    if (x < layout[z].Length - 1 && layout[z][x + 1] != ' ') dirs.Add(new Vector3D(1, 0, 0));
                    if (z > 0 && x < layout[z - 1].Length && layout[z - 1][x] != ' ') dirs.Add(new Vector3D(0, 0, -1));
                    if (z < layout.Length - 1 && x < layout[z + 1].Length && layout[z + 1][x] != ' ') dirs.Add(new Vector3D(0, 0, 1));

                    if (dirs.Contains(enemy.direction) && rand.NextDouble() > 0.2)
                    {
                        //Moving forward is an option
                    }
                    else if (dirs.Count > 0) //Pick one
                    {
                        int dir = rand.Next(0, dirs.Count);
                        enemy.direction = dirs[dir];

                        double angle = 0;
                        if (enemy.direction == new Vector3D(-1, 0, 0))
                            angle = -90;
                        else if (enemy.direction == new Vector3D(1, 0, 0))
                            angle = 90;
                        else if (enemy.direction == new Vector3D(0, 0, -1))
                            angle = 180;
                        else if (enemy.direction == new Vector3D(0, 0, 1))
                            angle = 0;

                        enemy.AnimateRotation(new Vector3D(0, 1, 0), angle, (long)(speed * 300), 1, 2);
                    }
                    //Speed is 1 unit per second
                    enemy.AnimateTranslate(enemy.direction.X + x, enemy.direction.Y, enemy.direction.Z + z, (long)(speed * 1000));
                }
            }
        }

        /// <summary>
        /// Perform a visual hit test of the camera (player)
        /// </summary>
        private void UpdateCollisions()
        {
            viewport.HitTest(hotspot.X, hotspot.Y);

            if (null != viewport.RayResult)
            {
                RayMeshGeometry3DHitTestResult rayResultMesh = viewport.RayResult as RayMeshGeometry3DHitTestResult;
                Enemy enemy = scene.GetEnemies.FirstOrDefault(x => x.GetGeometryModel() == rayResultMesh.ModelHit);
                tbInfo.Text += "\nDistance " + rayResultMesh.DistanceToRayOrigin.ToString("0.00") + " m";
                if (null != enemy)
                {
                    tbInfo.Text += " Enemy";
                    if (rayResultMesh.DistanceToRayOrigin < enemyProximity)
                    {
                        viewport.RemoveGeometryModel(enemy.GetGeometryModel());
                    }
                }
                tbInfo.Text += "\nDirection X=" + 
                    viewport.camera.LookDirection.X.ToString("0.00") + " Y=" +
                    viewport.camera.LookDirection.Z.ToString("0.00");
            }
        }

        /// <summary>
        /// Move the camera (player)
        /// </summary>
        private void UpdateCamera(double dt)
        {
            double yaw = 0;
            double pitch = 0;
            double roll = 0;
            double move = 0;

            //PLC communication
            int[] data = new int[10];
            //Browser sqlite - doesn't really work
            if (null != com)
            {
                data = com.ReadData();
                if (null == data)
                {
                    data = new int[10];
                }
            }
            else if (null != plcCom)
            {
                //PLC Input
                plcCom.COM_SetPin(0, Utilities.KeyDown("W") ? 1 : 0);
                plcCom.COM_SetPin(1, Utilities.KeyDown("A") ? 1 : 0);
                plcCom.COM_SetPin(2, Utilities.KeyDown("D") ? 1 : 0);
                plcCom.COM_SetPin(3, Utilities.KeyDown("S") ? 1 : 0);

                //This the PLC programming - output = input
                plcCom.COM_loop();

                //PLC Output
                for (int i = 4; i < 8; i++)
                {
                    data[i] = plcCom.COM_GetPin(i);
                }
            }
            else
            {
                //PLC Input
                plcLibrary.COM_setPin(0, Utilities.KeyDown("W") ? 1 : 0);
                plcLibrary.COM_setPin(1, Utilities.KeyDown("A") ? 1 : 0);
                plcLibrary.COM_setPin(2, Utilities.KeyDown("D") ? 1 : 0);
                plcLibrary.COM_setPin(3, Utilities.KeyDown("S") ? 1 : 0);

                //This the PLC programming - output = input
                plcLibrary.COM_loop();

                //PLC Output
                for (int i = 4; i < 8; i++)
                {
                    data[i] = plcLibrary.COM_getPin(i);
                }
            }

            //Time keys have been down for smoother action
            if (Utilities.KeyDown("Up") || data[4] == 1)
                keysDown["Up"] += dt;
            else
                keysDown["Up"] = 0;
            if (Utilities.KeyDown("Left") || data[5] == 1)
                keysDown["Left"] += dt;
            else
                keysDown["Left"] = 0;
            if (Utilities.KeyDown("Right") || data[6] == 1)
                keysDown["Right"] += dt;
            else
                keysDown["Right"] = 0;

            //Accelerate more if space down
            double acc = Utilities.KeyDown("Space") ? 2 : 1;

            //Accelerate movement for first period of key down
            double accTime = 0.5;
            move += 1.5 * speed * Math.Pow(Math.Min(keysDown["Up"], accTime) / accTime, 2) * acc * dt;
            yaw -= 100 * speed * Math.Pow(Math.Min(keysDown["Left"], accTime) / accTime, 2) * acc * dt;
            yaw += 100 * speed * Math.Pow(Math.Min(keysDown["Right"], accTime) / accTime, 2) * acc * dt;

            if (null != viewport.RayResult)
            {
                RayMeshGeometry3DHitTestResult rayResultMesh = viewport.RayResult as RayMeshGeometry3DHitTestResult;
                if (rayResultMesh.DistanceToRayOrigin < wallProximity)
                {
                    move = Math.Min(0, move);
                }
            }

            viewport.MoveCamera(yaw, pitch, roll, move);
        }
    }
}
