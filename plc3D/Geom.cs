using HelixToolkit.Wpf;
using System;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows;

namespace plc3D
{
    /// <summary>
    /// These are the 5 transforms added by default for geometries
    /// </summary>
    public enum eTransorm
    {
        ROTATE1,
        ROTATE2,
        ROTATE3,
        SCALE,
        TRANSLATE
    }

    /// <summary>
    /// Wall sides
    /// </summary>
    public enum eWall
    {
        XPOS,
        XNEG,
        YPOS,
        YNEG,
        ZPOS,
        ZNEG,
    }

    /// <summary>
    /// Base class for geometry objects in a scene
    /// </summary>
    public class Geom
    {
        private GeometryModel3D geometryModel;
        private string name = "";
        private int numAnimate = 0;

        /// <summary>
        /// Create a 2D face from a unit cube, the faces point outwards from the cube
        /// </summary>
        /// <param name="dir">Face side, XPOS for example is at X = 1, normal (1,0,0)</param>
        public void Wall(eWall dir)
        {
            MeshBuilder builder = new MeshBuilder(true, true);
            switch (dir)
            {
                case eWall.XPOS:
                    builder.AddFacePX();
                    break;
                case eWall.XNEG:
                    builder.AddFaceNX();
                    break;
                case eWall.YPOS:
                    builder.AddFacePY();
                    break;
                case eWall.YNEG:
                    builder.AddFaceNY();
                    break;
                case eWall.ZPOS:
                    builder.AddFacePZ();
                    break;
                case eWall.ZNEG:
                    builder.AddFaceNZ();
                    break;
            }
            MeshGeometry3D mesh = builder.ToMesh();

            //Subdivide the mesh for improved lighting - only works for a flat surface, since normals and texture not preserved
            //LoopSubdivision subDivision = new LoopSubdivision(mesh);
            //subDivision.Scheme = SubdivisionScheme.Loop;
            //subDivision.Subdivide(3);
            //MeshGeometry3D _mesh = subDivision.ToMeshGeometry3D();
            //_mesh.Normals = new Vector3DCollection();
            //_mesh.TextureCoordinates = new PointCollection();
            //for (int i = 0; i < _mesh.Positions.Count; i++)
            //{
            //    _mesh.Normals.Add(mesh.Normals[0]);
            //    switch (dir)
            //    {
            //        case eWall.XPOS:
            //        case eWall.XNEG:
            //            _mesh.TextureCoordinates.Add(new Point(_mesh.Positions[i].Y, _mesh.Positions[i].Z));
            //            break;
            //        case eWall.YPOS:
            //        case eWall.YNEG:
            //            _mesh.TextureCoordinates.Add(new Point(_mesh.Positions[i].Z, _mesh.Positions[i].X));
            //            break;
            //        case eWall.ZPOS:
            //        case eWall.ZNEG:
            //            _mesh.TextureCoordinates.Add(new Point(_mesh.Positions[i].X, _mesh.Positions[i].Y));
            //            break;
            //    }
            //}

            geometryModel = new GeometryModel3D()
            {
                Geometry = mesh,
            };
            AddTransforms(geometryModel);
        }

        /// <summary>
        /// Create a 3D cone
        /// </summary>
        public void Cone()
        {
            MeshBuilder builder = new MeshBuilder(true, true);
            builder.AddCone(new Point3D(0.5, 0, 0.5), new Vector3D(0, 1, 0), 0.3, 0, 1, false, false, 32);
            MeshGeometry3D mesh = builder.ToMesh();
            geometryModel = new GeometryModel3D()
            {
                Geometry = mesh,
            };
            AddTransforms(geometryModel);
        }

        /// <summary>
        /// Create a 3D Sphere
        /// </summary>
        public void Sphere()
        {
            MeshBuilder builder = new MeshBuilder(true, true);
            builder.AddSphere(new Point3D(0.5, 0.6, 0.5), 0.2, 32, 32);
            MeshGeometry3D mesh = builder.ToMesh();
            geometryModel = new GeometryModel3D()
            {
                Geometry = mesh,
            };
            AddTransforms(geometryModel);
        }

        /// <summary>
        /// Create 3DS model geometry
        /// </summary>
        public void Model3DS(string fileName)
        {
            ModelImporter modelImporter = new ModelImporter();
            Model3DGroup model3DGroup = modelImporter.Load(fileName);
            //TODO consider more than 1 geometry in the group
            foreach (GeometryModel3D geometry in model3DGroup.Children)
            {
                if (geometry.Bounds.IsEmpty) continue;
                geometryModel = geometry;
                //Scale to centered unit cube
                Center(new Point3D(0.5, 0.5, 0.5));
                AddTransforms(geometryModel);
                double scale = 1 / Math.Max(Math.Max(geometryModel.Bounds.SizeX, geometryModel.Bounds.SizeY), geometryModel.Bounds.SizeY);
                Scale(scale, scale, scale);
            }
        }

        /// <summary>
        /// A name label to identify the geometry
        /// </summary>
        public string Name
        {
            get
            { 
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// The number of current animations for the geometry
        /// </summary>
        public int NumAnimate
        {
            get
            {
                return numAnimate;
            }
        }

        /// <summary>
        /// Get the geometry model
        /// </summary>
        /// <returns></returns>
        public GeometryModel3D GetGeometryModel()
        {
            return geometryModel;
        }

        /// <summary>
        /// Get or set the surface material
        /// </summary>
        public Material Material
        {
            get
            {
                return geometryModel.Material;
            }
            set
            {
                geometryModel.Material = value;
            }
        }

        /// <summary>
        /// Get or set the back material - usually hidden interior surface
        /// </summary>
        public Material BackMaterial
        {
            get
            {
                return geometryModel.BackMaterial;
            }
            set
            {
                geometryModel.BackMaterial = value;
            }
        }

        /// <summary>
        /// Invert the normals of the geometry that will show the inside rather than outside
        /// </summary>
        public void ReverseNormals()
        {
            MeshGeometry3D mesh = (MeshGeometry3D)geometryModel.Geometry;
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                int j = mesh.TriangleIndices[i];
                mesh.TriangleIndices[i] = mesh.TriangleIndices[i + 1];
                mesh.TriangleIndices[i + 1] = j;
            }
            for (int i = 0; i < mesh.Normals.Count; i++)
            {
                mesh.Normals[i] *= -1;
            }
        }

        /// <summary>
        /// Move the object relative to its initial position
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="offsetZ"></param>
        public void Translate(double offsetX, double offsetY, double offsetZ)
        {
            TranslateTransform3D transform = (TranslateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.TRANSLATE];
            transform.OffsetX = offsetX;
            transform.OffsetY = offsetY;
            transform.OffsetZ = offsetZ;
        }

        /// <summary>
        /// Rotate the object by angle about axis
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle">angle in degrees</param>
        public void Rotate1(Vector3D axis, double angle)
        {
            RotateTransform3D transform = (RotateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.ROTATE1];
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3D.Axis = axis;
            axisAngleRotation3D.Angle = angle;
            transform.Rotation = axisAngleRotation3D;
        }

        /// <summary>
        /// Second rotation
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate2(Vector3D axis, double angle)
        {
            RotateTransform3D transform = (RotateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.ROTATE2];
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3D.Axis = axis;
            axisAngleRotation3D.Angle = angle;
            transform.Rotation = axisAngleRotation3D;
        }

        /// <summary>
        /// Third rotation
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate3(Vector3D axis, double angle)
        {
            RotateTransform3D transform = (RotateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.ROTATE3];
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3D.Axis = axis;
            axisAngleRotation3D.Angle = angle;
            transform.Rotation = axisAngleRotation3D;
        }

        /// <summary>
        /// Animate a translation of the object, relative to its original position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="duration">duration in ms</param>
        public void AnimateTranslate(double x, double y, double z, long duration)
        {
            TranslateTransform3D transform = (TranslateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.TRANSLATE];

            DoubleAnimation doubleAnimationX = new DoubleAnimation();
            doubleAnimationX.Duration = new Duration(new TimeSpan(duration * 10000));
            doubleAnimationX.From = transform.OffsetX;
            doubleAnimationX.To = x;

            DoubleAnimation doubleAnimationY = new DoubleAnimation();
            doubleAnimationY.Duration = new Duration(new TimeSpan(duration * 10000));
            doubleAnimationY.From = transform.OffsetY;
            doubleAnimationY.To = y;

            DoubleAnimation doubleAnimationZ = new DoubleAnimation();
            doubleAnimationZ.Duration = new Duration(new TimeSpan(duration * 10000));
            doubleAnimationZ.From = transform.OffsetZ;
            doubleAnimationZ.To = z;

            doubleAnimationX.Completed += (s, _) => _TranslationCompletedEvent();

            transform.BeginAnimation(TranslateTransform3D.OffsetXProperty, doubleAnimationX);
            transform.BeginAnimation(TranslateTransform3D.OffsetYProperty, doubleAnimationY);
            transform.BeginAnimation(TranslateTransform3D.OffsetZProperty, doubleAnimationZ);

            numAnimate++;
        }

        /// <summary>
        /// Animate a shape rotation
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="startAngle">degrees</param>
        /// <param name="endAngle">degrees</param>
        /// <param name="duration">ms</param>
        /// <param name="repeats">number of repeats, 0 for infinite</param>
        /// <param name="rotationNumber">1, 2, or 3 for different rotations</param>
        public void AnimateRotation(Vector3D axis, double angle, long duration, int repeats, int rotationNumber)
        {
            RotateTransform3D transform = (RotateTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.ROTATE1 + rotationNumber - 1];
            AxisAngleRotation3D axisAngleRotation3D = (AxisAngleRotation3D)transform.Rotation;

            DoubleAnimation doubleAnimaton = new DoubleAnimation();
            doubleAnimaton.Duration = new Duration(new TimeSpan(duration * 10000));
            doubleAnimaton.RepeatBehavior = (repeats > 0) ? new RepeatBehavior(repeats) : RepeatBehavior.Forever;
            doubleAnimaton.From = axisAngleRotation3D.Angle;
            doubleAnimaton.To = angle;

            doubleAnimaton.Completed += (s, _) => _TranslationCompletedEvent();

            axisAngleRotation3D.Axis = axis;
            axisAngleRotation3D.BeginAnimation(AxisAngleRotation3D.AngleProperty, doubleAnimaton);

            numAnimate++;
        }

        /// <summary>
        /// Animation finished event
        /// </summary>
        private void _TranslationCompletedEvent()
        {
            numAnimate--;
        }

        /// <summary>
        /// Scale the object relative to its original size
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="scaleZ"></param>
        public void Scale(double scaleX, double scaleY, double scaleZ)
        {
            ScaleTransform3D transform = (ScaleTransform3D)((Transform3DGroup)geometryModel.Transform).Children[(int)eTransorm.SCALE];
            transform.ScaleX = scaleX;
            transform.ScaleY = scaleY;
            transform.ScaleZ = scaleZ;
        }

        /// <summary>
        /// Not present in this version of HelixToolkit
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Point3D GetCenter(Rect3D rect)
        {
            return new Point3D(rect.X + rect.SizeX / 2, rect.Y + rect.SizeY / 2, rect.Z + rect.SizeZ / 2);
        }

        /// <summary>
        /// Center the geometry Bounds centre at a point
        /// </summary>
        /// <param name="center"></param>
        private void Center(Point3D center)
        {
            MeshGeometry3D mesh = (MeshGeometry3D)geometryModel.Geometry;
            Point3DCollection _positions = new Point3DCollection();
            foreach (var point in mesh.Positions)
            {
                _positions.Add(center + (point - GetCenter(geometryModel.Geometry.Bounds)));
            }
            mesh.Positions = _positions;
        }

        /// <summary>
        /// Add the default 5 transforms to the geometry
        /// </summary>
        /// <param name="geometry"></param>
        private static void AddTransforms(GeometryModel3D geometry)
        {
            Point3D center = GetCenter(geometry.Bounds);

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3D.Axis = new Vector3D(0, 1, 0);
            axisAngleRotation3D.Angle = 0;
            rotateTransform3D.Rotation = axisAngleRotation3D;
            rotateTransform3D.CenterX = center.X;
            rotateTransform3D.CenterY = center.Y;
            rotateTransform3D.CenterZ = center.Z;

            RotateTransform3D rotateTransform3D2 = new RotateTransform3D();
            AxisAngleRotation3D axisAngleRotation3D2 = new AxisAngleRotation3D();
            axisAngleRotation3D2.Axis = new Vector3D(0, 1, 0);
            axisAngleRotation3D2.Angle = 0;
            rotateTransform3D2.Rotation = axisAngleRotation3D2;
            rotateTransform3D2.CenterX = center.X;
            rotateTransform3D2.CenterY = center.Y;
            rotateTransform3D2.CenterZ = center.Z;

            RotateTransform3D rotateTransform3D3 = new RotateTransform3D();
            AxisAngleRotation3D axisAngleRotation3D3 = new AxisAngleRotation3D();
            axisAngleRotation3D3.Axis = new Vector3D(0, 1, 0);
            axisAngleRotation3D3.Angle = 0;
            rotateTransform3D3.Rotation = axisAngleRotation3D3;
            rotateTransform3D3.CenterX = center.X;
            rotateTransform3D3.CenterY = center.Y;
            rotateTransform3D3.CenterZ = center.Z;

            ScaleTransform3D scaleTransform3D = new ScaleTransform3D();
            scaleTransform3D.CenterX = center.X;
            scaleTransform3D.CenterY = center.Y;
            scaleTransform3D.CenterZ = center.Z;
            scaleTransform3D.ScaleX = 1;
            scaleTransform3D.ScaleY = 1;
            scaleTransform3D.ScaleZ = 1;

            TranslateTransform3D translateTransform3D = new TranslateTransform3D();
            translateTransform3D.OffsetX = 0;
            translateTransform3D.OffsetY = 0;
            translateTransform3D.OffsetZ = 0;

            Transform3DGroup transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(rotateTransform3D);
            transform3DGroup.Children.Add(rotateTransform3D2);
            transform3DGroup.Children.Add(rotateTransform3D3);
            transform3DGroup.Children.Add(scaleTransform3D);
            transform3DGroup.Children.Add(translateTransform3D);
            geometry.Transform = transform3DGroup;
        }
    }
}
