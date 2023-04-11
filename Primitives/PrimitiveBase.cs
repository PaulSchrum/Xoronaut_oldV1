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
        }

        protected abstract void MakeMesh(Point3D pt3);

    }
}