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
        public override string EnglishName => "EasyPav";

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
            var pavingWindow = new PavingForm();
            pavingWindow.Show();

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
                    dynamicGem.UpdateDiameter(pavingWindow.get_diameter());
                    dynamicGem.DynamicDrawGem(sender, e);
                };

                // Wait for the user to click a point in the viewport
                var getResult = getPoint.Get(true);
                if (getResult != GetResult.Point)
                {
                    pavingWindow.Hide();
                    return Result.Success;
                }

                // Step 4: Create the final circle in the document at the clicked point
                Point3d center = getPoint.Point();
                dynamicGem.SaveGem(doc, center);
            }
        }
    }

    public class MoveObjectsCommand : Command
    {
        public override string EnglishName => "EasyMove";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Step 1: Prompt the user to select objects
            var result = Rhino.Input.RhinoGet.GetMultipleObjects(
                "Select objects to move",
                false,
                ObjectType.Brep,
                out ObjRef[] objRefs
            );

            if (result != Result.Success || objRefs == null || objRefs.Length == 0)
            {
                RhinoApp.WriteLine("No objects selected.");
                return Result.Cancel;
            }

            // Step 2: Store the selected objects
            var selectedObjects = new List<RhinoObject>();
            foreach (var objRef in objRefs)
            {
                var rhinoObject = objRef.Object();
                if (rhinoObject != null)
                {
                    selectedObjects.Add(rhinoObject);
                }
            }

            // Step 3: Show the control window
            MoveObjectsForm.ShowForm(selectedObjects, doc);

            return Result.Success;
        }
    }

    public class ComputeNomenclatureCommand: Command
    {
        public override string EnglishName => "easyNomenc";

        private static DiameterCountForm _form;


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            // Step 1: Prompt the user to select objects
            var result = Rhino.Input.RhinoGet.GetMultipleObjects(
                "Select cones to compute diameters",
                false,
                ObjectType.Brep,
                out ObjRef[] objRefs
            );

            if (result != Result.Success || objRefs == null || objRefs.Length == 0)
            {
                RhinoApp.WriteLine("No objects selected.");
                return Result.Cancel;
            }

            // Step 2: Process selected objects to compute diameters
            var diameterCounts = new Dictionary<double, List<Guid>>();
            foreach (var objRef in objRefs)
            {
                var brep = objRef.Brep();
                if (brep == null)
                    continue;

                Cone cone;
                if (brep.Faces[0].TryGetCone(out cone))
                {
                    double diameter = Math.Round(cone.Radius * 2, 3); // Compute and round the diameter
                    if (!diameterCounts.ContainsKey(diameter))
                    {
                        diameterCounts[diameter] = new List<Guid>();
                    }
                    diameterCounts[diameter].Add(objRef.ObjectId);
                }
            }

            // Step 3: Show or update the form
            if (_form == null || _form.IsDisposed)
            {
                _form = new DiameterCountForm(diameterCounts, doc);
                _form.Show();
            }
            else
            {
                _form.UpdateData(diameterCounts);
                _form.BringToFront();
            }

            return Result.Success;
        }
    }
}
