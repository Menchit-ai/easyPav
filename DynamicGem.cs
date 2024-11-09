using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easyPav;
using Rhino.Runtime;
using Rhino;
using Rhino.Commands;

namespace easyPav
{
    public class DynamicGem
    {
        private double _diameter;
        private Surface _surface;
        private Gem _gemLoader = new Gem();
        private double _height;

    // Constructor that takes the diameter as a parameter
    public DynamicGem(double diameter, Surface surface)
        {
            _diameter = diameter;
            _height = _diameter * 0.431;
            _surface = surface;
        }

        // Method to update the diameter dynamically
        public void UpdateDiameter(double newDiameter)
        {
            _diameter = newDiameter;
            _height = _diameter * 0.431;
        }

        // This method will be called during dynamic drawing
        public void DynamicDrawGem(object sender, GetPointDrawEventArgs e)
        {
            // Project the current mouse location onto the surface
            Point3d cursorLoc = e.CurrentPoint;

            _surface.ClosestPoint(cursorLoc, out double u, out double v);
            _surface.FrameAt(u, v, out Plane localPlane);

            // Define the base center of the cone at the cursor location
            Point3d baseCenter = cursorLoc + (localPlane.Normal * _height);

            // Calculate the apex by moving from the base center along the opposite of the normal direction
            Point3d apex = baseCenter - (localPlane.Normal * _height);

            // Create the cone geometry with the base at the cursor location
            // Set the cone direction to face along the negative normal direction
            Cone cone = new Cone(new Plane(baseCenter, -localPlane.Normal), _height, _diameter/2);

            e.Display.DrawCone(cone, System.Drawing.Color.Blue);
        }

        // Method to add the Gem to the document at a specified center point
        public void SaveGem(Rhino.RhinoDoc doc, Point3d center)
        {
            _surface.ClosestPoint(center, out double u, out double v);
            _surface.FrameAt(u, v, out Plane localPlane);

            // Define the base center of the cone at the cursor location
            Point3d baseCenter = center + (localPlane.Normal * _height);

            // Calculate the apex by moving from the base center along the opposite of the normal direction
            Point3d apex = baseCenter - (localPlane.Normal * _height);

            // Create the cone geometry with the base at the cursor location
            // Set the cone direction to face along the negative normal direction
            Cone cone = new Cone(new Plane(baseCenter, -localPlane.Normal), _height, _diameter/2);

            doc.Objects.AddBrep(cone.ToBrep(true));
            doc.Views.Redraw();

        }
    }

}
