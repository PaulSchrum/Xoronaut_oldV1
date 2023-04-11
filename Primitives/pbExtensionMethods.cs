using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Primitives
{
    public static class pbExtensionMethods
    {
        internal static PrimitiveBase AddPrimitive(this List<PrimitiveBase> lst,
            Model3DGroup theScene, 
            PrimitiveBase prim)
        {
            theScene.Children.Add(prim.gm3D);
            lst.Add(prim);
            return prim;
        }
    }
}
