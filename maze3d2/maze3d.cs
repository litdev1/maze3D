// ===========================================================
// A SIMPLE 3D MAZE GAME for C# (SMALL BASIC) 
// using LITDEV extension
// ===========================================================

// buildcs.bat (1. or 2.): 
// 1. csc /r:SmallBasicLibrary.dll /r:LitDev.dll maze3d.cs mazeCls.cs
// 2. C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /r:SmallBasicLibrary.dll /r:LitDev.dll %1 %2

using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Library;
using LitDev;

namespace maze3d
{
    class ProgramMaze3D
    {
        static void Main(string[] args)
        {
            //const int nCones = 15;

            Maze3D m3d = new Maze3D();

            int gw = 800;
            int gh = 600;

            GraphicsWindow.Width = gw;
            GraphicsWindow.Height = gh;

            GraphicsWindow.BackgroundColor = "SteelBlue";
            GraphicsWindow.Title = "My Graphics Window";

            LDGraphicsWindow.State = 2;
            GraphicsWindow.BackgroundColor = "Black";
            GraphicsWindow.Title = "C#/Small Basic 3D Cone Maze";

            GraphicsWindow.BrushColor = "Red";
            GraphicsWindow.FontSize = 20;


            // tcpip (2)
            m3d.connect = File.ReadContents(Program.Directory + "/ChatConnect.txt");
            LDClient.ServerMessage += m3d.OnServerMessage;
            //LDClient.Connect(connect, "True");

            // Load maze from file or not
            if (args.Length > 0)
                m3d.loadMazeFromFile(args);
            else
                m3d.loadMaze();

            double speed = 1; // Control speed
            m3d.size = 1;  //  The image per tile scaling 
            double proximity = m3d.size / 10; // Closest approach to an object

            //for (int i = 0; i < m3d.nCones; i++)
            for (int i = 0; i < Maze3D.nCones; i++)
            {
                m3d.coneMoveDir[i] = -1;
            }

            Primitive button;

            try
            {

            START:
                GraphicsWindow.Clear();

                // Create a view 
                m3d.view3D = LD3DView.AddView(gw, gh, "True"); // Will not clip to size if window rescaled

                // Create a cone geometry
                m3d.createCone();
                m3d.createZeb();
                // Create a basic wall tile object
                m3d.createBasicWall();
                m3d.createWorld();

                // Animation end event
                LD3DView.TranslationCompleted += m3d.OnTranslationCompleted;

                LD3DView.AddAmbientLight(m3d.view3D, "LightBlue");

                Shapes.Remove(m3d.creation);

                //===========================================================
                // GAME LOOP
                //===========================================================

                Primitive startTime = Clock.ElapsedMilliseconds;
                int progress = 0;

                double yaw, pitch, roll, move;
                // HUD
                Primitive hud1 = Shapes.AddText("Counter ");

                while (progress < m3d.coneCount)
                //while (true)
                {
                    Primitive start = Clock.ElapsedMilliseconds;

                    // Use the keys to move the camera - comment S to prevent backwards movement out of the maze, or Up, Down, A and D to simplify movement
                    yaw = 0;
                    pitch = 0;
                    roll = 0;
                    move = 0;

                    // tcpip (3)
                    /*
                    if (message != "")
                    {
                        // Client1:s
                        string wsad = message.Substring(message.IndexOf(":") + 1);
                        if (wsad == "w") move += size / 25 * speed;
                        if (wsad == "s") move -= size / 25 * speed;
                        if (wsad == "a") yaw -= 1.5 * speed;
                        if (wsad == "d") yaw += 1.5 * speed;
                        if (wsad == "x") message = "";
                    }
                    */

                    string inputsX = "";

                    if (LDUtilities.KeyDown("W"))
                    {
                        inputsX += "1=49;";
                    }
                    else
                    {
                        inputsX += "1=48;";
                    }
                    if (LDUtilities.KeyDown("S"))
                    {
                        inputsX += "2=49;";
                    }
                    else
                    {
                        inputsX += "2=48;";
                    }
                    if (LDUtilities.KeyDown("Left"))
                    {
                        inputsX += "3=49;";
                    }
                    else
                    {
                        inputsX += "3=48;";
                    }
                    if (LDUtilities.KeyDown("Right"))
                    {
                        inputsX += "4=49;";
                    }
                    else
                    {
                        inputsX += "4=48;";
                    }

                    //LDFile.WriteByteArray(Program.Directory + "/in.txt", "1=49;2=48;3=48;4=49",1);
                    LDFile.WriteByteArray(Program.Directory + "/inputsX.txt", inputsX, 1);

                    Primitive outputsY = LDFile.ReadByteArray(Program.Directory + "/outputsY.txt", "False");

                    TextWindow.WriteLine(outputsY);

                    if (outputsY[1] == 49)
                    {
                        move += m3d.size / 25 * speed;
                    }
                    if (outputsY[2] == 49)
                    {
                        move -= m3d.size / 25 * speed;
                    }
                    if (outputsY[3] == 49)
                    {
                        yaw -= 1.5 * speed;
                    }
                    if (outputsY[4] == 49)
                    {
                        yaw += 1.5 * speed;
                    }

                    /*
                    if (LDUtilities.KeyDown("Up"))   pitch -= 1.5 * speed;
                    if (LDUtilities.KeyDown("Down")) pitch += 1.5 * speed;
                    if (LDUtilities.KeyDown("A"))    roll += 1.5 * speed;
                    if (LDUtilities.KeyDown("D"))    roll -= 1.5 * speed;
                    */

                    // Prevent forward movement into an object
                    Primitive hit = LD3DView.HitTest(m3d.view3D, -1, -1);

                    if (hit != "" && hit[2] < proximity)
                    {
                        move = Math.Min(0, move); // We can still back up

                        for (int i = 1; i <= m3d.coneCount; i++)
                        {
                            // if (hit[1] == cone[i] || hit[1] == cone[i][1])
                            if (hit[1] == m3d.cone[i])
                            {
                                // Remove cones as we find them
                                LD3DView.ModifyObject(m3d.view3D, m3d.cone[i], "H");
                                progress++;
                            }
                        }
                    }

                    //managePosters();

                    // Perhaps better without the pitch and roll
                    LD3DView.MoveCamera(m3d.view3D, yaw, pitch, roll, move); // These are relative changes wrt current view

                    // Exit
                    if (LDUtilities.KeyDown("Escape"))
                        Program.End();

                    for (int i = 1; i <= m3d.coneCount; i++)
                    {
                        if (m3d.startConeAnimation[i] == 1)
                        {
                            m3d.startConeAnimation[i] = 0;
                            m3d.animateCone(i);
                        }
                    }

                    Primitive pos = m3d.truncate(LD3DView.GetCameraPosition(m3d.view3D), 2);
                    Primitive dir = m3d.truncate(LD3DView.GetCameraDirection(m3d.view3D), 2);

                    string info = "Time = " + Math.Round((Clock.ElapsedMilliseconds - startTime) / 1000) + " Cones remaining = " + (m3d.coneCount - progress) + " Position = (" + pos[1] + " , " + pos[2] + " , " + pos[3] + ") Direction = (" + dir[1] + " , " + dir[2] + " , " + dir[3] + "); " + m3d.loopCounter.ToString() + " | " + m3d.loopCounterDelay.ToString();

                    GraphicsWindow.Title = info;
                    Shapes.SetText(hud1, info);

                    m3d.loopCounter++;

                    Primitive delay = 20 - (Clock.ElapsedMilliseconds - start);
                    if (delay > 0)
                    {
                        m3d.loopCounterDelay++;
                        Program.Delay(delay);
                    }

                }

                // Game ends - check the scores
                Shapes.Remove(m3d.view3D);
                Primitive timesec = Math.Round((Clock.ElapsedMilliseconds - startTime) / 1000);
                GraphicsWindow.DrawText(50, 50, "You did it in " + timesec + " seconds");
                Primitive score = Math.Max(0, 1000 - timesec);

                GraphicsWindow.DrawText(50, 100, "Your score is " + score);

                // Wait for OK
                button = Controls.AddButton("OK", 50, 200);
                m3d.ok = 0;
                Controls.ButtonClicked += m3d.OnOK;

                while (m3d.ok == 0)
                    Program.Delay(100);

                goto START;
            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }
        }
    }
}