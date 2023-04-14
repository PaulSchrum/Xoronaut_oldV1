using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Media3D;

[assembly: InternalsVisibleTo("XoronautViewer")]

namespace Primitives
{
    internal abstract class PrimitiveBase
    {
        private Point3D anchorPt_ = default(Point3D);
        public Point3D anchorPt 
        { 
            get
            {
                if (gm3D is null || gm3D.Geometry is null)
                {
                    return anchorPt_;
                }
                var pos0 = (this.gm3D.Geometry as MeshGeometry3D).Positions[0];
                return this.gm3D.Transform.Transform(pos0);
            }
            private set
            {
                if (gm3D is null || gm3D.Geometry is null)
                    anchorPt_ = value;
                else
                {
                    MoveTo(value);
                }
            }
        }
        internal MeshGeometry3D mesh { get; private set; } = null;
        internal Material material { get; private set; } = default;
        internal Material backMaterial { get; private set; } = default;
        public  GeometryModel3D gm3D { get; private set; } = null;

        private Transform3DGroup objectTransform_ = null;
        public Transform3DGroup objectTransform
        {
            get { return objectTransform_; }
            private set
            {
                objectTransform_ = value;
                this.gm3D.Transform = value;
            }
        }

        // Todo: Implement these next three.
        //public Vector3D velocity { get; private set; } = new Vector3D(0, 0, 0);
        //private Quaternion rotation { get; /* private */ set; } = new Quaternion(0, 0, 0, 0);
        //public Vector3D acceleration { get; private set; } = new Vector3D(0, 0, 0);

        protected PrimitiveBase(Point3D point3D, Material material = default, 
            Material backMaterial = default)
        {
            this.anchorPt_ = point3D;
            this.mesh = new MeshGeometry3D();
            MakeMesh(point3D);
            this.material = material;
            this.backMaterial = backMaterial;
            gm3D = new GeometryModel3D(mesh, material);
            gm3D.BackMaterial = backMaterial;
            gm3D.Geometry = mesh;
            gm3D.Transform = new Transform3DGroup();
            this.objectTransform_ = gm3D.Transform as Transform3DGroup;
        }

        protected abstract void MakeMesh(Point3D pt3);

        public void MoveTo(Point3D newPosition)
        {
            // Calculate the translation vector required to move to the new position
            Vector3D translation = newPosition - anchorPt_;

            // Create a new transform to apply the translation
            TranslateTransform3D translateTransform = new TranslateTransform3D(translation);

            // If the existing transform already has a TranslateTransform3D, replace it with the new one
            // Otherwise, add the new transform to the existing transform
            var existingTranslateTransform = objectTransform.Children.OfType<TranslateTransform3D>().FirstOrDefault();
            if (existingTranslateTransform != null)
            {
                int index = objectTransform.Children.IndexOf(existingTranslateTransform);
                objectTransform.Children[index] = translateTransform;
            }
            else
            {
                objectTransform.Children.Add(translateTransform);
            }
        }

        public void MoveBy(Vector3D deltaPositions)
        {
            MoveTo(anchorPt + deltaPositions);
        }

        public void MoveBy(Vector3D velocityVector_ups, double timeDeltaSeconds)
        {
            MoveBy(velocityVector_ups * timeDeltaSeconds);
        }
    }
}