using Rhino.FileIO;
using Rhino.Geometry;
using System;
using System.IO;
using System.Reflection;
using System.Resources;

namespace easyPav
{
    internal class Gem
    {
        public Gem() { }

        public Brep GetGem()
        {
            return LoadGem();

        }

        private static Brep LoadGem()
        {
            Plane plane = new Plane(Point3d.Origin, Point3d.Origin, Point3d.Origin);
            Cone cone = new Cone(plane, 1, 1);
            
            return cone.ToBrep(true);
        }
    }
}
