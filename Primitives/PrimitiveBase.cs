using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Media3D;

[assembly: InternalsVisibleTo("XoronautViewer")]

namespace Primitives
{
    internal abstract class PrimitiveBase
    {
        public Point3D anchorPt { get; private set; }
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

        protected PrimitiveBase(Point3D point3D, Material material = default, 
            Material backMaterial = default)
        {
            this.anchorPt = point3D;
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

        public void Transform(Transform3D xfrm)
        {
            // code provided by (inspired by) Chat-GPT on 12 April 2023
            objectTransform.Children.Add(xfrm);
            gm3D.Transform = objectTransform;
        }

        public void setVelocity(double velocityX, double velocityY, double velocityZ, double timeDeltaSeconds)
        {  // Function corrected for me by Chat-GPT.

            // Get the existing TranslateTransform3D from the Transform3DGroup, or create a new one if it doesn't exist
            TranslateTransform3D translation = objectTransform.Children.OfType<TranslateTransform3D>().FirstOrDefault();
            if (translation == null)
            {
                translation = new TranslateTransform3D();
                objectTransform.Children.Add(translation);
            }

            // Set the x, y, and z translations based on the velocity and time delta
            translation.OffsetX += velocityX * timeDeltaSeconds;
            translation.OffsetY += velocityY * timeDeltaSeconds;
            translation.OffsetZ += velocityZ * timeDeltaSeconds;
        }
    }
}