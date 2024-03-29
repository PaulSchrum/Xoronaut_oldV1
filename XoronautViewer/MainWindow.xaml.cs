﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using System.Windows.Media.Media3D;
using System.Media;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using DataTicker3D;
using System.Reflection.Metadata;

using Primitives;
using System.Security.Cryptography.Xml;

namespace XoronautViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<PrimitiveBase> primitiveObjects = new List<PrimitiveBase>();

        private GeometryModel3D aTriangle = null;
        private GeometryModel3D mGeometry;
        private bool mDown;
        private Point mLastPos;
        private Point startMousePosition;
        private int mouseDownCount;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        //bool timerIsRegistered;
        USGS_LakeLevelTxtFileReader USGS_LakeLevelTxtFileReader = null;
        TrigFunctionsCsv trigFunctionCsv = null;
        private int theDirectionalLight = -1;
        private Dictionary<System.Windows.Input.Key, bool> keyIsDown;
        private Vector3D viewpointVelocity { get; set; }
        private Double viewpointForwardSpeed { get; set; }
        private Double viewpointRotationSpeedAboutWorldZ { get; set; }
        private Double viewpointUpAngleRotationSpeed { get; set; }
        private Point3D cameraOriginalPosition { get; set; }
        private Vector3D totalCameraMoveVector { get; set; }
        private bool printDiagnostics { get; set; }
        Double upAngle = 0.0;
        Double pivotDistance = 10.5;
        Double pivotRate = 0.5;  // Degrees per second
        private bool mayShowMouseMoves { get; set; }
        private bool showMouseMoves { get; set; }
        private String testDataDir { get; set; }
        public double frameRate_fps { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            var cwd = System.IO.Directory.GetCurrentDirectory();
            var topDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(cwd, @"..\..\.."));
            this.testDataDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(topDir, @"SampleData"));

            mayShowMouseMoves = showMouseMoves = false;
            viewpointVelocity = new Vector3D(0, 0, 0);
            viewpointRotationSpeedAboutWorldZ = 0.0;
            viewpointUpAngleRotationSpeed = 0.005;
            hydrateKeyIsDownDictionary();
            //this.camera.Position = new Point3D(-.5, 2.5, 8);
            this.camera.Position = new Point3D(-18.0,2, 0);
            this.camera.FieldOfView = 55;
            //this.camera.LookDirection = new Vector3D(-10, -15, -35);
            this.camera.LookDirection = new Vector3D(1, -1.05, 0);
            this.upAngle = this.camera.LookDirection.getUpAngle();
            this.camera.UpDirection = new Vector3D(0, 1, 0);
            cameraOriginalPosition = new Point3D(this.camera.Position.X,
               this.camera.Position.Y, this.camera.Position.Z);

            int count = -1;
            foreach (var aThing in this.Scene.Children)
            {
                count++;
                if (aThing is DirectionalLight)
                {
                    theDirectionalLight = count;  // get the index for the DirectionalLight
                    break;
                }
            }

            testRotateVector();
            startMousePosition = new Point(0, 0);
            mouseDownCount = 0;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1.0/60.0);
            dispatcherTimer.Tick += UpdateTimeStep;
            dispatcherTimer.Start();

            //makeAtestObject();
            updateGUItextBoxes();
        }

        private double reportedSpeed = 0.0;
        private void updateGUItextBoxes()
        {
            reportedSpeed = this.viewpointForwardSpeed / this.frameRate_fps;
            this.txt_speed.Text = $"{reportedSpeed:f2} m/s   {(reportedSpeed*2.23694):f1} mph";

            this.txt_xyPlaneAngle.Text = deg(camera.LookDirection.getXZplaneAngle()).ToString();
            this.txt_upAngle.Text = deg(this.camera.LookDirection.getUpAngle()).ToString();
            this.txt_positionX.Text = this.camera.Position.X.ToString();
            this.txt_positionY.Text = this.camera.Position.Y.ToString();
            this.txt_positionZ.Text = this.camera.Position.Z.ToString();
            this.txt_lookX.Text = this.camera.LookDirection.X.ToString();
            this.txt_lookY.Text = this.camera.LookDirection.Y.ToString();
            this.txt_lookZ.Text = this.camera.LookDirection.Z.ToString();
        }

        private void hydrateKeyIsDownDictionary()
        {
            keyIsDown = new Dictionary<System.Windows.Input.Key, bool>();
            keyIsDown.Add(System.Windows.Input.Key.R, false);        // reset view to original position
            keyIsDown.Add(System.Windows.Input.Key.A, false);        // accelerate forward
            keyIsDown.Add(System.Windows.Input.Key.Z, false);        // accelerate backward
            keyIsDown.Add(System.Windows.Input.Key.Space, false);    // reset acceleration to 0
            keyIsDown.Add(System.Windows.Input.Key.Up, false);       // increase up angle
            keyIsDown.Add(System.Windows.Input.Key.Down, false);     // decrease up angle (dive)
            keyIsDown.Add(System.Windows.Input.Key.Left, false);     // rotate left
            keyIsDown.Add(System.Windows.Input.Key.Right, false);    // rotate right
            keyIsDown.Add(System.Windows.Input.Key.NumPad4, false);  // slew -X
            keyIsDown.Add(System.Windows.Input.Key.NumPad6, false);  // slew +X
            keyIsDown.Add(System.Windows.Input.Key.NumPad8, false);  // slew Up (+Y, world Z)
            keyIsDown.Add(System.Windows.Input.Key.NumPad2, false);  // slew Down (-Y, world Z)
            keyIsDown.Add(System.Windows.Input.Key.Divide, false);  // slew Left (+Z, world Y)
            keyIsDown.Add(System.Windows.Input.Key.NumPad0, false);   // slew Right (-Z, world Y)
            keyIsDown.Add(System.Windows.Input.Key.NumPad1, false);  // orbit view clockwise
            keyIsDown.Add(System.Windows.Input.Key.NumPad3, false);  // orbit view counterclockwise
            keyIsDown.Add(System.Windows.Input.Key.LeftShift, false);  // pivot view counterclockwise
            keyIsDown.Add(System.Windows.Input.Key.RightShift, false);  // pivot view counterclockwise

        }


        private void makeAtestObject()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            //mesh.Positions.Add(new Point3D(-3, 0, 0));
            //mesh.Positions.Add(new Point3D(-1.25, 2.5, -1));
            //mesh.Positions.Add(new Point3D(-1.5, 0, -2));

            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(-8, 0, -1));
            mesh.Positions.Add(new Point3D(-8, 0, 1));

            //mesh.Positions.Add(new Point3D(0, 0, -5));
            //mesh.Positions.Add(new Point3D(0, 0, 5));
            //mesh.Positions.Add(new Point3D(1, 0.13, -5));
            //mesh.Positions.Add(new Point3D(1, 0.13, 5));

            //mesh.Positions.Add(new Point3D(-0.5, 0, -5));
            //mesh.Positions.Add(new Point3D(-0.4, 0, 5));
            //mesh.Positions.Add(new Point3D(0.2, -0.13, -5));
            //mesh.Positions.Add(new Point3D(0.2, -0.13, 5));

            //mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, 3, 1, 2 });
            mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2 });

            GeometryModel3D geomod = new GeometryModel3D();
            geomod.Geometry = mesh;
            geomod.Material = new DiffuseMaterial(Brushes.Cyan);
            geomod.BackMaterial = new DiffuseMaterial(Brushes.Red);
            //geomod.Transform = new Transform3DGroup();

            //ModelVisual3D modvis = new ModelVisual3D();
            //modvis.Content = geomod;
            this.Scene.Children.Add(geomod);

            aTriangle = new GeometryModel3D();
            mesh = new MeshGeometry3D();
            aTriangle.Geometry = mesh;

            mesh.Positions.Add(new Point3D(-5, 4, 0));
            mesh.Positions.Add(new Point3D(0, 4, 0));
            mesh.Positions.Add(new Point3D(-5, 0, 0));
            mesh.Positions.Add(new Point3D(0, 0, 0));

            mesh.TriangleIndices = new Int32Collection(new int[] { 0, 2, 3, //}
         1, 0, 3 }
            );

            aTriangle.Geometry = mesh;

            //var img = @"C:\Users\Paul\Documents\temp\CIU Prayer Towers in snow.JPG";
            //var imageBrush = new ImageBrush(new BitmapImage(new Uri(img, UriKind.Relative)));
            //imageBrush.ViewportUnits = BrushMappingMode.Absolute;
            //var xfrm = new TransformGroup();
            ////xfrm.Children.Add(new SkewTransform(0.0, -45.0));
            //imageBrush.Transform = xfrm;

            //DiffuseMaterial dmImage = new DiffuseMaterial(imageBrush);

            DiffuseMaterial dm = new DiffuseMaterial(Brushes.Purple);
            dm.Color = Color.FromArgb(126, 127, 0, 127);
            aTriangle.Material = dm;
            //aTriangle.Material = dmImage;
            aTriangle.BackMaterial = new DiffuseMaterial(Brushes.Orange);
            //aTriangle.BackMaterial = dmImage;
            mesh.TextureCoordinates =
               new PointCollection(new Point[] {
               new Point(0,0),
               new Point(1, 0),
               new Point(0,1),
               new Point(1,1)
                  });
            //geomod.Transform = new Transform3DGroup();

            //ModelVisual3D modvis = new ModelVisual3D();
            //modvis.Content = geomod;
            this.Scene.Children.Add(aTriangle);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //camera.Position = new Point3D(camera.Position.X, camera.Position.Y, 
            //camera.Position.Z - e.Delta / 250D);
            Double fovMultiplier = e.Delta > 0 ? 0.8 : -1 / 0.8;
            camera.FieldOfView *= fovMultiplier * e.Delta / 30D;
            Debug.Print(camera.FieldOfView.ToString());
        }

        private double dx = 0.2; private double dy = 0.05; private double dz = 0.02;
        private double xltX = 0; private double xltY = 0; private double xltZ = 0;
        private double rotZ = 0; private double dRotZ = 0.5;
        private Double lightX = -8;

        private int oie = 0;
        private void UpdateTimeStep(Object sender, EventArgs e)
        {
            this.frameRate_fps = (sender as DispatcherTimer).Interval.TotalSeconds;
            processElements(frameRate_fps);
            if (null != aTriangle && false)
            {
                var xfrm = new Transform3DGroup();

                rotZ += dRotZ;
                RotateTransform3D rotateTransform3D = new RotateTransform3D();
                AxisAngleRotation3D axisAngleRotation3d = new AxisAngleRotation3D();
                axisAngleRotation3d.Axis = new Vector3D(0, 1, 0);
                axisAngleRotation3d.Angle = rotZ;
                rotateTransform3D.Rotation = axisAngleRotation3d;
                xfrm.Children.Add(rotateTransform3D);

                xltX += dx; xltY += dy; xltZ += dz;
                var tranVec = new Vector3D(xltX, xltY, xltZ);
                xfrm.Children.Add(new TranslateTransform3D(tranVec));

                aTriangle.Transform = xfrm;

            }
            processKeyPresses();
            if (this.mDown == false)
            {
                //Debug.Print("- Mouse button is not down.");
                Double addValue = 0.25;
                if (lightX > 8.0) lightX = -8.0;
                lightX += addValue;
                this.Scene.Children[theDirectionalLight] = new DirectionalLight(Colors.Bisque,
                   new Vector3D(lightX, -5, -7)); /* */
                //Debug.Print(lightX.ToString());
                return;
            }
            processMouseNavigation(true);
            //Debug.Print(". Now it is down.");
        }

        private void vortexEquation(PrimitiveBase anObject, out double dx, out double dy, out double dz)
        {
            var centerPt = anObject.anchorPt;
            double HordistToCenter = Math.Sqrt(centerPt.X * centerPt.X + centerPt.Z * centerPt.Z);
            dy = 50 / HordistToCenter;
            if(HordistToCenter > 20) dy = 0;

            double radialSpeed = -10 / HordistToCenter;
            double angluarSpeed = -10 / HordistToCenter;
            double angleToCenter = Math.Atan2(centerPt.Z, centerPt.X);
            double newRadius = HordistToCenter + radialSpeed;
            double newAngle = angleToCenter + angluarSpeed;
            double newX = newRadius * Math.Cos(newAngle);
            double newZ = newRadius * Math.Sin(newAngle);
            dx = newX - centerPt.X;
            dz = newZ - centerPt.Z;
        }

        private int counter = 0;
        private double delx = 0.0; private double dely = 0.0; private double delz = 0.0;
        private void processElements(double timeDelta)
        {
            if (this.primitiveObjects is null ||  this.primitiveObjects.Count == 0) return;

            counter++;
            if(counter == 1)
            {
                Thread.Sleep(3000);
            }
            counter = 2;
            txt_mouseX.Text = $"{(1 / timeDelta):f2} fps";

            foreach(var obj in this.primitiveObjects)
            {
                vortexEquation(obj, out delx, out dely, out delz);
                obj.MoveBy(new Vector3D(delx, dely, delz), timeDelta);
            }

        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.mDown = true;
            }
            else
            {
                this.mDown = false;
            }
            //processMouseNavigation(e.LeftButton == MouseButtonState.Pressed);
        }

        private Double getMagnitude(Double x, Double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        private void processMouseNavigation(bool leftButtonIsPressed)
        //private void processMouseNavigation(object sender, MouseEventArgs e)
        {
            if (true == this.mDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                if (mouseDownCount > 0)
                {
                    //Thread.Sleep(25);


                    double screenDeltaX = pos.X - startMousePosition.X;
                    double screenDeltaY = pos.Y - startMousePosition.Y;
                    Double zBearing = Math.Atan2(camera.LookDirection.Y, camera.LookDirection.X);
                    Double magnitude = getMagnitude(camera.LookDirection.X, camera.LookDirection.Y);
                    //Debug.Print(camera.LookDirection.X.ToString());

                    zBearing += screenDeltaX / 500;
                    Double newXcomponent = magnitude * Math.Cos(zBearing);
                    Double newYcomponent = magnitude * Math.Sin(zBearing);
                    //Debug.Print(screenDeltaX.ToString() + "  " + zBearing.ToString() + "  " + "");
                    Debug.Print("Before: " + camera.LookDirection.ToString());
                    camera.LookDirection = new Vector3D(newXcomponent,
                       newYcomponent, camera.LookDirection.Z);
                    Debug.Print("After: " + camera.LookDirection.ToString());
                    Debug.Print("");

                }
                else
                {
                    startMousePosition = pos;
                    mouseDownCount = 0;
                }
                mouseDownCount++;
            }
            else mouseDownCount = 0;
        }

        #region
        private void Grid_MouseMove_oldVersion(object sender, MouseEventArgs e)
        {
            if (mDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                Transform3DGroup group = mGeometry.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                group.Children.Add(new RotateTransform3D(r));

                mLastPos = actualPos;
            }
        }
        #endregion

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.mDown = e.LeftButton == MouseButtonState.Pressed;
            startMousePosition = Mouse.GetPosition(viewport);
            e.Handled = false;
            //processMouseNavigation(this.mDown);
        }

        private void Grid_MouseDown_old(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            mDown = true;
            Point pos = Mouse.GetPosition(viewport);
            mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
        }


        private void createManyPoints(int numberOfPoint)
        {
            double radius = 100.0;
            double thickness = 5.0;

            double angleRad = 0.0;
            double radiusInstance = 0.0;
            double height = 0.0;

            double x = 0.0; double y = 0.0; double z = 0.0;

            Random random = new Random();
            for (int i = 0; i < numberOfPoint; i++) 
            { 
                angleRad = random.NextDouble() * 2 * Math.PI;
                radiusInstance = Math.Sqrt(random.NextDouble()) * radius;
                height = random.NextDouble() * 10.0;

                x = radiusInstance * Math.Cos(angleRad);
                z = radiusInstance * Math.Sin(angleRad);
                y = height;

                try
                {
                    this.primitiveObjects.AddPrimitive(this.Scene,
                        new PointVisual(new Point3D(x, y, z),
                        new DiffuseMaterial(Brushes.Yellow),
                        new DiffuseMaterial(Brushes.Red)));

                    lbl_speed.Content = $"{i++}";

                }
                catch (Exception e)
                {
                    lbl_speed.Content =$"Overflow: {i} pts.";
                    break;
                }            
            }
        }

        private void generateSampleData()
        {
            // var stopwatch = Stopwatch.StartNew();
            createManyPoints(5_000);
            //stopwatch.Stop();
            //this.lbl_pos.Content =
            //    $"{stopwatch.Elapsed.TotalSeconds:F4} sec.";

            bool lookStraightUp = false;
            if (lookStraightUp == true)
            {
                this.camera.Position =
                    new Point3D(0.0, -20.0, 0.0);
                this.camera.LookDirection = new Vector3D(0.05, 1.0, 0.0);
            }
            else
            {
                this.camera.Position =
                    new Point3D(-180.0, 20.0, 0.0);
                this.camera.LookDirection = new Vector3D(1, -0.07, 0.0);
            }

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat == true) return;
            if (keyIsDown.ContainsKey(e.Key) == true)
            {
                keyIsDown[e.Key] = true;
                e.Handled = true;
            }
            if (e.Key == Key.D)
                printDiagnostics = printDiagnostics == true ? false : true;

            processKeyPresses();
            //Debug.Print("Down " + e.Key.ToString());
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (keyIsDown.ContainsKey(e.Key) == true)
                keyIsDown[e.Key] = false;

            processKeyPresses();
            //Debug.Print("Up " + e.Key.ToString());
        }

        private void processKeyPresses()
        {
            totalCameraMoveVector = new Vector3D(0, 0, 0);
            Double speedIncrement = 0.005;
            Double maxSpeed = 2.0;
            if (keyIsDown[Key.A] == true)
            {
                viewpointForwardSpeed += speedIncrement;
            }
            if (keyIsDown[Key.Z] == true)
            {
                viewpointForwardSpeed -= speedIncrement;
            }
            if (keyIsDown[Key.Space] == true)
            {
                viewpointForwardSpeed = 0;
            }
            if (keyIsDown[Key.R] == true)
            {
                this.camera.Position = new Point3D(
                   cameraOriginalPosition.X, cameraOriginalPosition.Y, cameraOriginalPosition.Z);
                viewpointForwardSpeed = 0.0;
                this.viewpointVelocity = new Vector3D(0, 0, 0);
            }

            viewpointForwardSpeed = Math.Abs(viewpointForwardSpeed) > maxSpeed ?
               Math.Sign(viewpointForwardSpeed) * maxSpeed :
               viewpointForwardSpeed;

            Double zRotationSpeedIncrement = 0.2;
            Double maxZrotationSpeed = zRotationSpeedIncrement * 10;

            if (keyIsDown[Key.Right] == true)
            {
                viewpointRotationSpeedAboutWorldZ += zRotationSpeedIncrement;
            }
            if (keyIsDown[Key.Left] == true)
            {
                viewpointRotationSpeedAboutWorldZ -= zRotationSpeedIncrement;
            }
            zRotationSpeedIncrement = Math.Abs(zRotationSpeedIncrement) > maxZrotationSpeed ?
               Math.Sign(zRotationSpeedIncrement) * maxZrotationSpeed :
               zRotationSpeedIncrement;

            Double upAngleChange;
            upAngleChange = 0;
            if (keyIsDown[Key.Up] == true) upAngleChange = -1 * viewpointUpAngleRotationSpeed;
            if (keyIsDown[Key.Down] == true) upAngleChange = viewpointUpAngleRotationSpeed;
            if (upAngleChange != 0.0)
            {
                this.upAngle = this.camera.LookDirection.getUpAngle();

                this.upAngle += upAngleChange;

                this.upAngle = Math.Abs(deg(upAngle)) > 86.0 ?
                   Math.Sign(upAngle) * rad(86.0) :
                   upAngle;
                myDebugPrint(upAngle.ToString());
                myDebugPrintObj(this.camera.LookDirection);
                this.camera.LookDirection =
                   setVectorUpAngle(upAngle, this.camera.LookDirection);
                myDebugPrintObj(this.camera.LookDirection);
            }

            if (keyIsDown[Key.Left] || keyIsDown[Key.Right])
            {
                this.camera.LookDirection =
                   rotateVector3DaboutWorldY(viewpointRotationSpeedAboutWorldZ, this.camera.LookDirection);
            }
            else
            {
                zRotationSpeedIncrement = 0.0;
                viewpointRotationSpeedAboutWorldZ = 0.0;
            }

            if (keyIsDown[Key.NumPad1] || keyIsDown[Key.NumPad3])
            {
                double localPivotRate;
                Double amplifier;
                amplifier = 1.0;
                if (keyIsDown[Key.LeftShift] == true || keyIsDown[Key.RightShift] == true)
                    amplifier = 3.0;
                else amplifier = 1.0;

                Double multiplier; Vector3D pivotBackVec; Point3D pivotPoint;
                multiplier = keyIsDown[Key.NumPad1] ? -1.0 : 1.0;
                // 
                localPivotRate = Math.Abs(pivotRate) * multiplier * amplifier;
                pivotPoint = camera.Position + pivotDistance * camera.LookDirection / camera.LookDirection.Length;
                camera.LookDirection = rotateVector3DaboutWorldY(localPivotRate, camera.LookDirection);
                pivotBackVec = -1 * pivotDistance * camera.LookDirection / camera.LookDirection.Length;
                String pvBk = pivotBackVec.getAsModifiedPolar();
                camera.Position = pivotPoint + pivotBackVec;
            }

            var vectorToAdd = viewpointForwardSpeed * this.camera.LookDirection /
               this.camera.LookDirection.Length;
            totalCameraMoveVector = totalCameraMoveVector + vectorToAdd;

            processSlewKeys();

            this.camera.Position += totalCameraMoveVector;

            updateGUItextBoxes();

        }

        private Double maxSlewSpeed = 1.0;
        private Double xSlewSpeed = 0;
        private Double ySlewSpeed = 0;
        private Double zSlewSpeed = 0;
        private readonly Double slewDelta = 0.2;
        private void processSlewKeys()
        {
            if (keyIsDown[Key.NumPad4] == true || keyIsDown[Key.NumPad6] == true)
            {
                if (keyIsDown[Key.NumPad4] == true)
                    xSlewSpeed -= slewDelta;
                else
                    xSlewSpeed += slewDelta;
            }
            else
                xSlewSpeed = 0;

            if (keyIsDown[Key.NumPad8] == true || keyIsDown[Key.NumPad2] == true)
            {
                if (keyIsDown[Key.NumPad8] == true)
                    ySlewSpeed -= slewDelta;
                else
                    ySlewSpeed += slewDelta;
            }
            else
                ySlewSpeed = 0;

            if (keyIsDown[Key.Divide] == true || keyIsDown[Key.NumPad0] == true)
            {
                if (keyIsDown[Key.NumPad0] == true)
                    zSlewSpeed -= slewDelta;
                else
                    zSlewSpeed += slewDelta;
            }
            else
                zSlewSpeed = 0;

            totalCameraMoveVector += new Vector3D(xSlewSpeed, zSlewSpeed, ySlewSpeed);
        }

        private void myDebugPrint(String s)
        {
            if (true == printDiagnostics)
                Debug.Print(s);
        }

        private void myDebugPrintObj(Object obj)
        {
            myDebugPrint(obj.ToString());
        }

        private void testRotateVector()
        {
            return;
            var vec = new Vector3D(10, 15, 10);
            Debug.Print(getXZplaneAngleDegrees(vec).ToString());
            var rotatedVec = rotateVector3DaboutWorldY(-45.0, vec);
            Debug.Print(getXZplaneAngleDegrees(rotatedVec).ToString());
        }

        private Vector3D rotateVector3DaboutWorldY(Double rotation, Vector3D vec)
        {
            Double xyLength = getLengthProjToXZplane(vec);
            Double xyDirection = getXZplaneAngleDegrees(vec);
            xyDirection += rotation;
            Double xyDirRad = rad(xyDirection);
            return new Vector3D(
                  xyLength * Math.Cos(xyDirRad),
                  vec.Y,
                  xyLength * Math.Sin(xyDirRad));
        }

        private Vector3D setVectorUpAngle(Double newUpAngle, Vector3D vec)
        {
            Double newYvalue = getLengthProjToXZplane(vec) * Math.Tan(newUpAngle);
            return new Vector3D(
               vec.X,
               newYvalue,
               vec.Z);
        }

        private Double getLengthProjToXZplane(Vector3D vec)
        {
            return Math.Sqrt(vec.X * vec.X + vec.Z * vec.Z);
        }

        private Double getXZplaneAngleDegrees(Vector3D vec)
        {
            return deg(Math.Atan2(vec.Z, vec.X));
        }

        private Double deg(Double rad)
        {
            return 180 * rad / Math.PI;
        }

        private Double rad(Double deg)
        {
            return deg * Math.PI / 180;
        }

        private String vectorAsAngles(Vector3D vec)
        {
            if (vec.X == 0.0 && vec.Y == 0.0)
            {
                if (vec.Z == 0.0)
                    return "point vector";
                else if (vec.Z > 0.0)
                    return "Up";
                else
                    return "Down";
            }
            return "Theta = " + deg(Math.Atan2(vec.Y, vec.X)) +
               "  Up Angle = " + deg(Math.Atan2(vec.Z, getLengthProjToXZplane(vec)));
        }

        //private void mnu_openLakeData_Click(object sender, RoutedEventArgs e)
        //{
        //    openLakeDataFile();
        //    updateGUItextBoxes();
        //}

        private void clearVisualizedData()
        {
            mayShowMouseMoves = false;
            showMouseMoves = false;
            for (int index = this.Scene.Children.Count - 1; index > 3; index--)
            {
                this.Scene.Children.RemoveAt(index);
            }
        }

        private void mnu_clearVisualizedData_Click(object sender, RoutedEventArgs e)
        {
            clearVisualizedData();
        }

        //private void mnu_openTrafficData_Click(object sender, RoutedEventArgs e)
        //{
        //    openTrafficDataFile();
        //}

        private void mnu_startMouseTracking_Click(object sender, RoutedEventArgs e)
        {
            mayShowMouseMoves = true;
            showMouseMoves = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (false == mayShowMouseMoves) return;
            var pos = e.GetPosition(this.viewport);
            this.txt_mouseX.Text = (pos.X - this.viewport.ActualWidth / 2.0).ToString();
            this.txt_mouseY.Text = (this.viewport.ActualHeight / 2.0 - pos.Y).ToString();
        }

        private void mnu_generateSampleData_Click(object sender, RoutedEventArgs e)
        {
            generateSampleData();
        }

    }

    public static class vector3dExtensionMethods
    {
        public static double getXZplaneAngle(this Vector3D vec)
        {
            return Math.Atan2(vec.Z, vec.X);
        }

        public static double xzPlaneLength(this Vector3D vec)
        {
            return Math.Sqrt(vec.X * vec.X + vec.Z * vec.Z);
        }

        public static double getUpAngle(this Vector3D vec)
        {
            return Math.Atan2(vec.Y, vec.xzPlaneLength());
        }

        public static string getAsModifiedPolar(this Vector3D vec)
        {
            return "R: " + vec.Length.ToString() + " Az: " + vec.getXZplaneAngle().ToString()
               + " Up: " + vec.getUpAngle();
        }
    }
}
