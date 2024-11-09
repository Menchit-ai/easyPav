using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

namespace easyPav
{
    public class easyPavCommand : Command
    {
        public easyPavCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static easyPavCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "easyPav";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            ObjRef objRef;
            var rc = RhinoGet.GetOneObject("Select a surface", false, ObjectType.Surface, out objRef);
            if (rc != Result.Success)
            {
                return Result.Cancel;
            }

            // Get the selected surface
            var surface = objRef.Surface();
            if (surface == null)
            {
                RhinoApp.WriteLine("No surface selected.");
                return Result.Failure;
            }

            // Step 1: Initialize with a default diameter
            double diameter = 1; // Default value

            // Step 2: Open the WPF window to let the user adjust the diameter
            var diameterWindow = new Form();
            diameterWindow.Show();

            while (true)
            {
                // Step 3: Set up dynamic drawing for the preview circle
                var dynamicGem = new DynamicGem(diameter, surface);

                var getPoint = new GetPoint();
                getPoint.Constrain(surface, false);
                getPoint.SetCommandPrompt("Click to place the circle");

                // Hook into the dynamic draw event
                getPoint.DynamicDraw += (sender, e) =>
                {
                    // Update the dynamic circle based on the current diameter from the window
                    dynamicGem.UpdateDiameter(diameterWindow.get_diameter());
                    dynamicGem.DynamicDrawGem(sender, e);
                };

                // Wait for the user to click a point in the viewport
                var getResult = getPoint.Get(true);
                if (getResult != GetResult.Point)
                {
                    diameterWindow.Hide();
                    return Result.Success;
                }

                // Step 4: Create the final circle in the document at the clicked point
                Point3d center = getPoint.Point();
                dynamicGem.SaveGem(doc, center);
            }
        }
    }
}
