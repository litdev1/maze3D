using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows;

namespace plc3D
{
    /// <summary>
    /// The main setup of the 3D view, camera, lights etc (not geometry objects)
    /// </summary>
    public class Viewport : Viewport3D
    {
        public PerspectiveCamera camera;
        public ModelVisual3D modelVisual3D;
        public Model3DGroup model3DGroup;
        public AmbientLight ambientLight;
        public PointLight cameraLight;

        private RayHitTestResult rayResult;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="performance">May be better performance if true</param>
        public Viewport(bool performance)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            if (performance)
            {
                IsHitTestVisible = false;
                ClipToBounds = false;
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            }

            camera = new PerspectiveCamera()
            {
                Position = new Point3D(0.5, 0.75, 0.5),
                LookDirection = new Vector3D(1, 0, 0),
                UpDirection = new Vector3D(0, 1, 0),
                NearPlaneDistance = 0.001,
                FarPlaneDistance = 1000,
                FieldOfView = 60,
            };
            Camera = camera;

            modelVisual3D = new ModelVisual3D();
            Children.Add(modelVisual3D);

            model3DGroup = new Model3DGroup();
            modelVisual3D.Content = model3DGroup;

            //Follows camera
            cameraLight = AddPointLight(Color.FromArgb(255, 255, 255, 255), camera.Position, 10);
        }

        /// <summary>
        /// Add the ambient light to the scene
        /// </summary>
        /// <param name="color"></param>
        public void AddAmbientLight(Color color)
        {
            ambientLight = new AmbientLight(color);
            model3DGroup.Children.Add(ambientLight);
        }

        /// <summary>
        /// Add a point light to the scene
        /// </summary>
        /// <param name="color"></param>
        /// <param name="position"></param>
        /// <param name="range"></param>
        public PointLight AddPointLight(Color color, Point3D position, double range)
        {
            PointLight pointLight = new PointLight(color, position);
            pointLight.Range = range;
            pointLight.QuadraticAttenuation = 1;
            model3DGroup.Children.Add(pointLight);
            return pointLight;
        }

        /// <summary>
        /// Add a spot light to the scene
        /// </summary>
        /// <param name="color"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="outer"></param>
        /// <param name="inner"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public SpotLight AddSpotLight(Color color, Point3D position, Vector3D direction, double outer, double inner)
        {
            SpotLight spotLight = new SpotLight(color, position, direction, outer, inner);
            spotLight.QuadraticAttenuation = 1;
            model3DGroup.Children.Add(spotLight);
            return spotLight;
        }

        /// <summary>
        /// Add a directional light to the scene
        /// </summary>
        /// <param name="color"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DirectionalLight AddDirectionalLight(Color color, Vector3D direction)
        {
            DirectionalLight directionalLight = new DirectionalLight(color, direction);
            model3DGroup.Children.Add(directionalLight);
            return directionalLight;
        }

        /// <summary>
        /// Add a geometry object to the scene
        /// </summary>
        /// <param name=""></param>
        public void AddGeometryModel(GeometryModel3D geometryModel)
        {
            if (model3DGroup.Children.Contains(geometryModel)) return;
            model3DGroup.Children.Add(geometryModel);
        }

        /// <summary>
        /// Remove a single geometry fromm the 3D view
        /// </summary>
        /// <param name="geometryModel3D"></param>
        public void RemoveGeometryModel(GeometryModel3D geometryModel3D)
        {
            model3DGroup.Children.Remove(geometryModel3D);
        }

        /// <summary>
        /// Move the camera
        /// </summary>
        /// <param name="yaw">Left/Right</param>
        /// <param name="pitch">Up/Down</param>
        /// <param name="roll">Spin</param>
        /// <param name="move">Postive is forwards in current view direction</param>
        public void MoveCamera(double yaw, double pitch, double roll, double move)
        {
            Vector3D lookDirection = camera.LookDirection;
            Vector3D upDirection = camera.UpDirection;
            Point3D position = camera.Position;

            Vector3D rotateAbout1 = upDirection;
            Vector3D rotateAbout2 = Vector3D.CrossProduct(rotateAbout1, lookDirection);

            Matrix3D rotateMatrix = Matrix3D.Identity;
            Quaternion quaterion = new Quaternion(upDirection, -yaw);
            rotateMatrix.Rotate(quaterion);
            quaterion = new Quaternion(rotateAbout2, -pitch);
            rotateMatrix.Rotate(quaterion);
            lookDirection = rotateMatrix.Transform(lookDirection);
            lookDirection.Normalize();
            position += move * lookDirection;

            //Also rotate up direction
            rotateMatrix = Matrix3D.Identity;
            quaterion = new Quaternion(rotateAbout2, -pitch);
            rotateMatrix.Rotate(quaterion);
            quaterion = new Quaternion(lookDirection, roll);
            rotateMatrix.Rotate(quaterion);
            upDirection = rotateMatrix.Transform(upDirection);
            upDirection.Normalize();

            lookDirection.Normalize();
            Vector3D screenDirection = Vector3D.CrossProduct(lookDirection, upDirection);
            screenDirection.Normalize();
            upDirection = Vector3D.CrossProduct(screenDirection, lookDirection);

            camera.LookDirection = lookDirection;
            camera.UpDirection = upDirection;
            camera.Position = position;

            cameraLight.Position = position;
            //cameraLight.Direction = lookDirection;
        }

        /// <summary>
        /// Perform a visual hit test of the camera (player)
        /// </summary>
        /// <returns></returns>
        public RayHitTestResult HitTest(double dx, double dy)
        {
            PointHitTestParameters hitParams = new PointHitTestParameters(new Point(ActualWidth * dx, ActualHeight * dy));
            rayResult = null;
            VisualTreeHelper.HitTest(this, null, ResultCallback, hitParams);
            return rayResult;
        }

        /// <summary>
        /// Get the result of the hit test
        /// </summary>
        public RayHitTestResult RayResult
        {
            get { return rayResult; }
        }

        /// <summary>
        /// The result of the hit test
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            rayResult = result as RayHitTestResult;
            return HitTestResultBehavior.Stop; // Nearest
        }
    }
}
