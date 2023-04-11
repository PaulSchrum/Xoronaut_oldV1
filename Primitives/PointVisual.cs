using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

[assembly: InternalsVisibleTo("XoronautViewer")]


namespace Primitives
{
    /// <summary>
    /// PointVisual is just a point. The word "visual" is added to distinguish
    /// it from a Point3D, which is a point in 3D space. PointVisual is used
    /// visualize Point3D objects.
    /// 
    /// PointVisual is implemented as a 4-sided pyramid so as to get the benefits
    /// of being a MeshGeometry3D.
    /// </summary>
    internal class PointVisual : PrimitiveBase
    {
        internal PointVisual(Point3D point3D, Material material = default,
            Material backMaterial = default) : base(point3D, material, backMaterial)
        {
        }

        /// <summary>
        /// For the given primitive type, create the mesh geometry.
        /// </summary>
        /// <param name="pt3"></param>
        /// <returns></returns>
        protected override void MakeMesh(Point3D pt3)
        {
            var aPt = new Point3D(pt3.X, pt3.Y, pt3.Z+0.2);
            mesh.Positions.Add(aPt);
            aPt = new Point3D(pt3.X + 0.1, pt3.Y, pt3.Z - 0.15);
            mesh.Positions.Add(aPt);
            aPt = new Point3D(pt3.X - 0.15, pt3.Y + 0.15, pt3.Z - 0.15);
            mesh.Positions.Add(aPt);
            aPt = new Point3D(pt3.X - 0.15, pt3.Y - 0.15, pt3.Z - 0.15);
            mesh.Positions.Add(aPt);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            return;
        }
    }
}
