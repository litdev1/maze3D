// ===========================================================
// A SIMPLE 3D MAZE GAME for C# (SMALL BASIC) 
// using LITDEV extension
// ===========================================================

using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Library;
using LitDev;

using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace maze3d
{

    class Actor
    {
        public int counter = 0;
        public string type = "";   // C -Cone, S - Sphere, Z - Zeb
        public string state = "0"; // 1- active, 0 - inactive

        public Actor()
        {
        }

        public Actor(string objType)
        {
            // counter++;	
            type = objType;
        }

    }

    class Maze3D
    {
        public double size;     // The image per tile scaling proximity = size/10 Closest approach to an FPrimitive
        public int coneCount;

        //const int nCones = 15;
        public const int nCones = 15;
        public int[] coneX = new int[nCones];
        public int[] coneY = new int[nCones];
        public int[] startConeAnimation = new int[nCones] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public int[] coneMoveDir = new int[nCones];
        //public Primitive[] cone = new Primitive[nCones];
        public string[] cone = new string[nCones];

        public Actor[] movingObjects = new Actor[nCones];

        public string pointsCone;
        public string indicesCone;
        public string texturesCone;

        public ArrayList postersWallObj = new ArrayList();
        public int loopCounter = 0, loopCounterDelay = 0;
        //public int rotPhase = 0;

        // Create a world based on layout
        // X is an empty room
        // L is a room with a light
        // C is a room with a rotating illuminated cone
        // S is a room with a rotating illuminated sphere
        // Z is Zeb
        // a-z is a custom wall images in config file

        public string[] layout = new string[16];

        public int ok;

        //public Primitive wall, wallDown, wallUp;
        //public Primitive wallImg, stonesImg, waterImg;
        //public Primitive view3D;    // Will not clip to size if window rescaled
        //public Primitive creation;  // shape

        public string wall, wallDown, wallUp;
        public string wallImg, stonesImg, waterImg;
        public string view3D;
        public string creation;

        // customisation
        public Primitive imgPoster;
        public Primitive objPoster;
        public Primitive imgIndices;
        public Primitive zeb;
        //public string zeb;

        public int yMax; // yMin = 1, 

        // tcpip (1)
        //public Primitive message;
        public string message;
        public Primitive connect;

        public void OnKeyDown()
        {
            if (LDTextWindow.LastKey == "Escape")
            {
                LDServer.Stop();
                Program.End();
            }
        }

        public void OnServerMessage()
        {
            message = LDClient.LastServerMessage;
        }

        public void loadMazeFromFile(string[] args)
        {
            // Load images and set main control variables		
            string path = Program.Directory;
            int iFileRow = 0;

            // Primitive fileName = Program.Directory + "/room.txt";
            string configFile = args[0];
            string fileName = Program.Directory + "/" + configFile;
            string about = File.ReadLine(fileName, ++iFileRow); // About
            imgPoster = File.ReadLine(fileName, ++iFileRow);
            imgIndices = Microsoft.SmallBasic.Library.Array.GetAllIndices(imgPoster);

            TextWindow.WriteLine(imgIndices);

            for (int i = 1; i <= Microsoft.SmallBasic.Library.Array.GetItemCount(imgPoster); i++)
            {
                string imgInd = imgIndices[i];
                imgPoster[imgInd] = Program.Directory + "/" + imgPoster[imgInd];
            }

            Primitive imgWalls = File.ReadLine(fileName, ++iFileRow); // txt file	

            wallImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[1]);
            stonesImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[2]);
            waterImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[3]);

            string line;
            iFileRow++;
            int temp = iFileRow;

            while ((line = File.ReadLine(fileName, ++iFileRow)) != "")
            {
                line = " " + line + " ";
                for (int i = iFileRow; i >= (temp + 2); i--)
                {
                    layout[i - temp] = layout[i - (temp + 1)];
                }

                layout[1] = line;
                TextWindow.WriteLine(line + ".");
            }

            yMax = iFileRow - (temp + 1);
            TextWindow.WriteLine(yMax);

        }

        public void loadMaze()
        {
            imgPoster = "a=coffee.png;b=bug.png;c=head.png;d=Tree.png";
            Primitive imgWalls = "1=wall.jpg;2=stones.jpg;3=water.jpg;";

            layout[6] = " XXaXbXcXdXCXX ";
            layout[5] = " XXXLXdXZXXXZXX ";
            layout[4] = " XXXX  LXXXa XX ";
            layout[3] = " XXXd    XXCXXb ";
            layout[2] = " XXXXXcXaSLXXdX ";
            layout[1] = " XXXcXCXX ";

            //layout[15] = " LXX     LXXXXXXXXXXXXXXXLXXXX ";
            /*
            layout[15] = " LXXXXXXXXXXLXXXXXXXXXXXXXXXLXXXX ";
            layout[14] = " X X XXLXX   X    X        XCX ";
            layout[13] = " XXLXX   X   X    X          L ";
            layout[12] = " L C XXL XL  X    LXXXXXXXLXXXXX ";
            layout[11] = " XXX L X  X  LXXXX     X     XCX ";
            layout[10] = " L XXXXXX X      L     X  XXLXXXLXXXLX ";
            layout[9]  = " X  X  L   XXXXLXXXXXXXXL  X  X     X ";
            layout[8]  = "    X  X   X               L  X   XLXX ";
            layout[7]  = " LXXX    LXXXXXXXXXXXLXXXXXX  L   X  X ";
            layout[6]  = " C X  XLXX     X           X  XXXXX  L ";
            layout[5]  = " XXLXX   X  XXXX     LXXXXXXX     L  X ";
            layout[4]  = " L X XXL XL          X      XXXXXXXXXX ";
            layout[3]  = " CXX L X  XXXXXXXLXXXX        XCX ";
            layout[2]  = " X XXXXXX X        X      XXLXXXXXXXLX ";
            layout[1]  = " XXLXXXXXLXXXXXXXXXXXXXXXXXXXXLXXXXXXX ";
            */

            imgIndices = Microsoft.SmallBasic.Library.Array.GetAllIndices(imgPoster);

            //TextWindow.WriteLine(imgIndices);

            for (int i = 1; i <= Microsoft.SmallBasic.Library.Array.GetItemCount(imgPoster); i++)
            {
                string imgInd = imgIndices[i];
                imgPoster[imgInd] = Program.Directory + "/" + imgPoster[imgInd];
            }

            wallImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[1]);
            stonesImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[2]);
            waterImg = ImageList.LoadImage(Program.Directory + "/" + imgWalls[3]);
        }

        public void createBasicWall()
        {
            string points = "";
            string indices = "";
            string textures = "";
            int index = 0;

            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {

                    int x1 = i - 1;
                    int x2 = i;
                    int y1 = j - 1;
                    int y2 = j;
                    int z = 0;
                    //Triangle1
                    points = points + x1 + ":" + y1 + ":" + z + ":";
                    points = points + x2 + ":" + y1 + ":" + z + ":";
                    points = points + x2 + ":" + y2 + ":" + z + ":";
                    indices = indices + index + ":" + (index + 1) + ":" + (index + 2) + ":";
                    index += 3;
                    textures = textures + "0 1:1 1:1 0:"; //"0 0:0 1:1 1:"

                    // Triangle2
                    points = points + x1 + ":" + y1 + ":" + z + ":";
                    points = points + x2 + ":" + y2 + ":" + z + ":";
                    points = points + x1 + ":" + y2 + ":" + z + ":";
                    indices = indices + index + ":" + (index + 1) + ":" + (index + 2) + ":";
                    index += 3;
                    textures = textures + "0 1:1 0:0 0:"; // "0 0:1 1:1 0:"
                }
            }

            try
            {

                wall = LD3DView.AddGeometry(view3D, points, indices, "", "White", "D");
                LD3DView.AddImage(view3D, wall, textures, wallImg, "D");

                LD3DView.AddBackImage(view3D, wall, "", wallImg, "D");
                LD3DView.ModifyObject(view3D, wall, "H");

                wallDown = LD3DView.AddGeometry(view3D, points, indices, "", "White", "D");
                LD3DView.AddImage(view3D, wallDown, textures, stonesImg, "D");
                LD3DView.ModifyObject(view3D, wallDown, "H");

                wallUp = LD3DView.AddGeometry(view3D, points, indices, "", "White", "D");
                LD3DView.AddImage(view3D, wallUp, textures, waterImg, "D");
                LD3DView.ModifyObject(view3D, wallUp, "H");

                for (int i = 1; i <= Microsoft.SmallBasic.Library.Array.GetItemCount(imgIndices); i++)
                {
                    string imgInd = imgIndices[i];

                    objPoster[imgInd] = LD3DView.AddGeometry(view3D, points, indices, "", "White", "D");

                    //Primitive image = ImageList.LoadImage(imgPoster[imgInd]);
                    string image = ImageList.LoadImage(imgPoster[imgInd]);

                    LD3DView.AddImage(view3D, objPoster[imgInd], textures, image, "D");

                    string imageReverse = LDImage.Copy(image);
                    LDImage.EffectReflect(imageReverse, 0);
                    LD3DView.AddBackImage(view3D, objPoster[imgInd], "", imageReverse, "D");
                    LD3DView.ModifyObject(view3D, objPoster[imgInd], "H");

                }
            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }
        }


        public void createWall(int i, int j, int k, string dir)
        {
            string clonedObject = "";

            try
            {
                if (dir == "F")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wall);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size, -j * size);
                }
                else if (dir == "B")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wall);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, 180);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size, -j * size + size);
                }
                else if (dir == "L")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wall);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, 90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size - size / 2, k * size, -j * size + size / 2);
                }
                else if (dir == "R")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wall);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, -90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size + size / 2, k * size, -j * size + size / 2);
                }
                else if (dir == "U")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wallUp);
                    LD3DView.RotateGeometry(view3D, clonedObject, 1, 0, 0, 90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size + size / 2, -j * size + size / 2);
                }
                else if (dir == "D")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wallDown);
                    LD3DView.RotateGeometry(view3D, clonedObject, 1, 0, 0, -90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size - size / 2, -j * size + size / 2);
                }

            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }

            //TextWindow.WriteLine(clonedObject);

            LD3DView.Freeze(view3D, clonedObject);  // We wont ever modify this so freeze it
        }

        public void createPosterWall(int i, int j, int k, string dir, Primitive objWallPoster)
        {
            string clonedObject = "";

            try
            {
                if (dir == "F")
                {
                    clonedObject = LD3DView.CloneObject(view3D, objWallPoster);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size, -j * size);
                }
                else if (dir == "B")
                {
                    clonedObject = LD3DView.CloneObject(view3D, objWallPoster);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, 180);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size, -j * size + size);
                }
                else if (dir == "L")
                {
                    clonedObject = LD3DView.CloneObject(view3D, objWallPoster);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, 90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size - size / 2, k * size, -j * size + size / 2);
                }
                else if (dir == "R")
                {
                    clonedObject = LD3DView.CloneObject(view3D, objWallPoster);
                    LD3DView.RotateGeometry(view3D, clonedObject, 0, 1, 0, -90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size + size / 2, k * size, -j * size + size / 2);
                }
                else if (dir == "U")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wallUp);
                    LD3DView.RotateGeometry(view3D, clonedObject, 1, 0, 0, 90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size + size / 2, -j * size + size / 2);
                }
                else if (dir == "D")
                {
                    clonedObject = LD3DView.CloneObject(view3D, wallDown);
                    LD3DView.RotateGeometry(view3D, clonedObject, 1, 0, 0, -90);
                    LD3DView.TranslateGeometry(view3D, clonedObject, i * size, k * size - size / 2, -j * size + size / 2);
                }

            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }

            postersWallObj.Add(clonedObject);
            /*
            if(objPoster[imgInd] == "Geometry92" ||                objPoster[imgInd] == "Geometry179")
            */
            {
                // Billboard !!
                //LD3DView.SetBillBoard(view3D, clonedObject);
                //TextWindow.WriteLine(objPoster[imgInd]);
            }


            //TextWindow.WriteLine(clonedObject);		
            LD3DView.Freeze(view3D, clonedObject);  // We wont ever modify this so freeze it
        }


        // Create a cone geometry
        public void createCone()
        {
            double nside = 10;
            double height = size / 2;
            double radius = size / 4;

            pointsCone = "0:" + (2 * height / 3) + ":0 ";
            indicesCone = "";
            texturesCone = "0.5:1 ";

            //For i = 0 To nside
            for (int i = 0; i <= nside; i++)
            {
                double angle = i / nside * 2 * Math.Pi;
                double x = radius * Math.Cos(-angle);
                double y = radius * Math.Sin(-angle);
                pointsCone = pointsCone + x + ":" + (-height / 3) + ":" + y + " ";

                if (i < nside)
                    indicesCone = indicesCone + "0:" + (i + 1) + ":" + (i + 2) + " ";
                else
                    indicesCone = indicesCone + "0:" + (i + 1) + ":" + 1 + " ";

                texturesCone = texturesCone + i / nside + ":0 ";
            }
        }

        // Perhaps faster with global
        public int dirForward, dirBack, dirRight, dirLeft;
        public string[] move = new string[5];
        // The 4 possible new positions
        public int[] newX = new int[5];
        public int[] newY = new int[5];

        // Current position
        public int xMovingObj; // = coneX[i];
        public int yMovingObj; // = coneY[i];

        // Animate a cone (indeed any moving object)
        public void animateCone(int i)
        {
            /*
            int dirForward, dirBack, dirRight, dirLeft;
            string[] move = new string[5];
            // The 4 possible new positions
            int[] newX = new int[5];
            int[] newY = new int[5];

            // Current position
            int xMovingObj = coneX[i];
            int yMovingObj = coneY[i];
            */

            xMovingObj = coneX[i];
            yMovingObj = coneY[i];

            // The 4 possible new positions
            newX[1] = xMovingObj - 1;
            newY[1] = yMovingObj;
            newX[2] = xMovingObj;
            newY[2] = yMovingObj + 1;
            newX[3] = xMovingObj + 1;
            newY[3] = yMovingObj;
            newX[4] = xMovingObj;
            newY[4] = yMovingObj - 1;


            try
            {

                // Check for directions we can move
                for (int iDir = 1; iDir <= 4; iDir++)
                {
                    move[iDir] = getChar(newY[iDir], newX[iDir]);
                }

                // coneMoveDir is the last direction 1 to 4 as listed above - start in +Y direction if unset
                if (coneMoveDir[i] == -1)
                {
                    coneMoveDir[i] = 2;
                }

                // Find the forward, back, left and right wrt current direction
                dirForward = coneMoveDir[i];

                dirLeft = coneMoveDir[i] - 1;
                if (dirLeft < 1)
                    dirLeft += 4;

                dirBack = coneMoveDir[i] - 2;
                if (dirBack < 1)
                    dirBack += 4;

                dirRight = coneMoveDir[i] - 3;
                if (dirRight < 1)
                    dirRight += 4;

                // Move forward with high chance
                if (move[dirForward] != "" && Math.GetRandomNumber(10) > 2)
                {
                    coneX[i] = newX[dirForward];
                    coneY[i] = newY[dirForward];
                    coneMoveDir[i] = dirForward;
                }
                else
                {
                    // If not then move left or right with 50% chance each - otherwise move back
                    if (move[dirLeft] != "" && Math.GetRandomNumber(2) == 1)
                    {
                        coneX[i] = newX[dirLeft];
                        coneY[i] = newY[dirLeft];
                        coneMoveDir[i] = dirLeft;
                    }
                    else if (move[dirRight] != "")
                    {
                        coneX[i] = newX[dirRight];
                        coneY[i] = newY[dirRight];
                        coneMoveDir[i] = dirRight;
                    }
                    else if (move[dirLeft] != "")
                    { // Move left if we cannot go right
                        coneX[i] = newX[dirLeft];
                        coneY[i] = newY[dirLeft];
                        coneMoveDir[i] = dirLeft;
                    }
                    else if (move[dirBack] != "")
                    {
                        coneX[i] = newX[dirBack];
                        coneY[i] = newY[dirBack];
                        coneMoveDir[i] = dirBack;
                    }
                    else if (move[dirForward] != "")
                    { // It is possible we can only go forward
                        coneX[i] = newX[dirForward];
                        coneY[i] = newY[dirForward];
                        coneMoveDir[i] = dirForward;
                    }
                }

                if (movingObjects[i].type == "Z")
                {
                    // Do the move: Zeb
                    LD3DView.AnimateTranslation(view3D, cone[i], coneX[i] * size + size / 2, 0.9, -(coneY[i] * size - size) - 31.5, 1);
                }
                else if (movingObjects[i].type == "C" || movingObjects[i].type == "S")
                {
                    // Do the move: Cone, Sphere
                    LD3DView.AnimateTranslation(view3D, cone[i], coneX[i] * size + size / 2, size / 3, -(coneY[i] * size - size / 2), 1);
                }

            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }

        }


        // Move the cone when its last translation is completed
        public void OnTranslationCompleted()
        {
            Primitive lastCone;

            try
            {
                // Handle rare possibility that a cone can get stuck by ending its animation at the same time as another so check for all queued completed animations
                while (LD3DView.QueuedTranslationCompleted > 0) // Almost always exactly one.
                {
                    lastCone = LD3DView.LastTranslationCompleted; // Get this value only once since it will dequeue any queued items
                    //For jCone = 1 To coneCount  We use jCone because i and kCone are used elsewhere and we dont want them to conflict
                    for (int jCone = 1; jCone <= coneCount; jCone++)
                    {
                        if (cone[jCone] == lastCone || cone[jCone][1] == lastCone)  // ... || zeb : 3ds file
                            startConeAnimation[jCone] = 1;
                    }
                }

            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }

        }

        public void createZeb()
        {
            zeb = LD3DView.LoadModel(view3D, Program.Directory + "/zeb.3ds");
            LD3DView.ScaleGeometry(view3D, zeb[1], 0.01, 0.01, 0.01);
            LD3DView.RotateGeometry(view3D, zeb[1], 1, 0, 0, -90);
            LD3DView.TranslateGeometry(view3D, zeb[1], 0, 0, -31.5);
            LD3DView.ModifyObject(view3D, zeb[1], "S");
        }

        public void createWorld()
        {
            string color;
            string charMap;  // help variables

            bool cameraSet = false;
            coneCount = 0;

            try
            {
                for (int y = 1; y < layout.Length; y++)
                {
                    Shapes.Remove(creation);
                    creation = Shapes.AddText("Creating Scene " + Math.Round(100 * (y) / (layout.Length)) + "%");

                    for (int x = 1; x < Text.GetLength(layout[y]); x++)
                    {
                        charMap = getChar(y, x);

                        if (charMap == "")
                        {
                            //Floor and ceiling
                            createWall(x, y, 0, "U");
                            createWall(x, y, 0, "D");
                        }
                        else if (charMap != "")
                        {
                            if (cameraSet == false)
                            {
                                // Initial camera position and direction and view angle
                                LD3DView.ResetCamera(view3D, x * size + size / 2, 0.4 * size, -size / 2, 0, 0, -1, "", "", "");
                                LD3DView.CameraProperties(view3D, 0, 30 * size, 60);//  limit to 30 blocks (the longest corridor)
                                cameraSet = true; // The first room is the start point
                            }

                            //Floor and ceiling
                            createWall(x, y, 0, "U");
                            createWall(x, y, 0, "D");

                            int asciiCharMap = Text.GetCharacterCode(charMap);

                            if (97 <= asciiCharMap && asciiCharMap <= 122) // a-z
                            {
                                //Primitive poster = null;
                                string poster = "";
                                poster = objPoster[charMap];

                                // Left and right poster walls
                                if (getChar(y, x - 1) == "")
                                {
                                    createPosterWall(x, y, 0, "L", poster);
                                }

                                if (getChar(y, x + 1) == "")
                                {
                                    createPosterWall(x, y, 0, "R", poster);
                                }

                                // Front and back poster walls							
                                if (getChar(y - 1, x) == "")
                                    createPosterWall(x, y, 0, "B", poster);

                                if (getChar(y + 1, x) == "")//(y == yMax)
                                    createPosterWall(x, y, 0, "F", poster);
                            }
                            else
                            {
                                // Left and right walls
                                if (getChar(y, x - 1) == "")
                                    createWall(x, y, 0, "L");

                                if (getChar(y, x + 1) == "")
                                    createWall(x, y, 0, "R");

                                // Front and back walls
                                if (getChar(y - 1, x) == "")
                                    createWall(x, y, 0, "B");

                                if (getChar(y + 1, x) == "")
                                    createWall(x, y, 0, "F");
                            }

                            // Add a lit cone - these are the main performance limiters so keep this kind of thing to a minimum

                            // Add a cone
                            if (charMap == "C")
                            {
                                color = LDColours.HSLtoRGB(Math.GetRandomNumber(360), 1, 0.5); // A random high brightness colour

                                coneCount++;

                                cone[coneCount] = LD3DView.AddGeometry(view3D, pointsCone, indicesCone, "", color, "D"); // A base colour
                                LD3DView.AddImage(view3D, cone[coneCount], texturesCone, waterImg, "S"); //  Some shiny texture that is stronly affected by lights

                                LD3DView.TranslateGeometry(view3D, cone[coneCount], x * size + size / 2, size / 3, -(y * size - size / 2));
                                LD3DView.AnimateRotation(view3D, cone[coneCount], 0, 1, 0, 0, 360, 3, -1);
                                coneX[coneCount] = x;
                                coneY[coneCount] = y;

                                movingObjects[coneCount] = new Actor("C");

                                animateCone(coneCount);
                            }

                            // Add a sphere
                            else if (charMap == "S")
                            {
                                color = LDColours.HSLtoRGB(Math.GetRandomNumber(360), 1, 0.5); // A random high brightness colour

                                coneCount++;

                                cone[coneCount] = LD3DView.AddSphere(view3D, 0.25, 10, color, "D"); // A base colour

                                //LD3DView.AddImage(view3D, cone[coneCount], "", Program.Directory + "/skydome10.jpg", "D");
                                LD3DView.TranslateGeometry(view3D, cone[coneCount], x * size + size / 2, size / 3, -(y * size - size / 2));
                                LD3DView.AnimateRotation(view3D, cone[coneCount], 0, 1, 0, 0, 360, 3, -1);
                                coneX[coneCount] = x;
                                coneY[coneCount] = y;

                                movingObjects[coneCount] = new Actor("S");

                                animateCone(coneCount);
                            }

                            // Add a zeb
                            else if (charMap == "Z")
                            {
                                coneCount++;

                                cone[coneCount] = LD3DView.CloneObject(view3D, zeb[1]);

                                LD3DView.AnimateRotation(view3D, cone[coneCount], 0, 1, 0, 0, 360, 5, -1);

                                //createZeb2(x * size + size / 2, 0.9, -(y * size - size) - 31.5); // zeb;
                                //cone[coneCount] = createZeb(); // zeb;

                                coneX[coneCount] = x;
                                coneY[coneCount] = y;

                                movingObjects[coneCount] = new Actor("Z");

                                animateCone(coneCount); // !!!
                            }

                            // Add a point light
                            else if (charMap == "L")
                            {
                                //Primitive point = LD3DView.AddPointLight(view3D, "White", x * size + size / 2, 0.98 * size, -(y * size - size / 2), 5 * size);

                                string point = LD3DView.AddPointLight(view3D, "White", x * size + size / 2, 0.98 * size, -(y * size - size / 2), 5 * size);
                            }

                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }

        }

        public Primitive truncate(Primitive arr, double sigfig)
        {
            double multiplier = Math.Power(10, sigfig);
            for (int i = 1; i <= arr.GetItemCount(); i++)
            {
                arr[i] = (1 / multiplier) * Math.Round(multiplier * arr[i]);
            }
            return arr;
        }

        // Get layout char1acter
        public string getChar(int i, int j)
        {
            string s = Text.GetSubText(layout[i], j, 1);
            if (s == " ")
                s = "";

            return s;
        }

        public void OnOK()
        {
            ok = 1;
        }
    }

}