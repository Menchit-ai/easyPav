using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace easyPav
{
    public class MoveObjectsForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        private static MoveObjectsForm _instance;
        private List<RhinoObject> _selectedObjects;
        private RhinoDoc _doc;
        private NumericUpDownFix _movementDistance;
        private Button _moveUpButton;
        private Button _moveDownButton;

        private MoveObjectsForm(List<RhinoObject> selectedObjects, RhinoDoc doc)
        {
            _selectedObjects = selectedObjects;
            _doc = doc;

            InitializeComponent();
            // Set Rhino as the owner of this window
            var rhinoMainWindowHandle = Rhino.RhinoApp.MainWindowHandle();
            SetParent(this.Handle, rhinoMainWindowHandle);
        }

        public static void ShowForm(List<RhinoObject> selectedObjects, RhinoDoc doc)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new MoveObjectsForm(selectedObjects, doc);
                _instance.Show();
            }
            else
            {
                _instance.BringToFront();
            }
        }

        private void MoveObjectsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dispose of the form on close
            _instance = null;
        }

        private void InitializeComponent()
        {
            _movementDistance = new NumericUpDownFix();
            _moveUpButton = new Button();
            _moveDownButton = new Button();
            ((System.ComponentModel.ISupportInitialize)_movementDistance).BeginInit();
            SuspendLayout();
            // 
            // _movementDistance
            // 
            _movementDistance.DecimalPlaces = 2;
            _movementDistance.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            _movementDistance.Location = new System.Drawing.Point(74, 12);
            _movementDistance.Name = "_movementDistance";
            _movementDistance.Size = new System.Drawing.Size(120, 23);
            _movementDistance.TabIndex = 0;
            _movementDistance.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _moveUpButton
            // 
            _moveUpButton.Location = new System.Drawing.Point(155, 50);
            _moveUpButton.Name = "_moveUpButton";
            _moveUpButton.Size = new System.Drawing.Size(75, 23);
            _moveUpButton.TabIndex = 1;
            _moveUpButton.Text = "up";
            _moveUpButton.Click += MoveUpButton_Click;
            // 
            // _moveDownButton
            // 
            _moveDownButton.Location = new System.Drawing.Point(45, 50);
            _moveDownButton.Name = "_moveDownButton";
            _moveDownButton.Size = new System.Drawing.Size(75, 23);
            _moveDownButton.TabIndex = 2;
            _moveDownButton.Text = "down";
            _moveDownButton.Click += MoveDownButton_Click;
            // 
            // MoveObjectsForm
            // 
            ClientSize = new System.Drawing.Size(284, 161);
            Controls.Add(_movementDistance);
            Controls.Add(_moveUpButton);
            Controls.Add(_moveDownButton);
            Name = "MoveObjectsForm";
            Text = "Move Objects";
            TopMost = true;
            Load += MoveObjectsForm_Load;
            ((System.ComponentModel.ISupportInitialize)_movementDistance).EndInit();
            ResumeLayout(false);
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            MoveObjects((double)_movementDistance.Value);
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            MoveObjects(-(double)_movementDistance.Value);
        }

        private void MoveObjects(double distance)
        {
            if (_selectedObjects.Count == 0)
            {
                MessageBox.Show("No objects to move.");
                return;
            }

            var updatedObjects = new List<RhinoObject>();

            foreach (var obj in _selectedObjects)
            {
                // Get the object's Brep geometry
                var brep = obj.Geometry as Brep;
                if (brep == null)
                    continue;

                // Get the base plane of the cone
                var normal = brep.Faces[1].NormalAt(0, 0);

                // Create a translation transform along the base plane's normal
                var translation = Transform.Translation(normal * distance);

                // Duplicate and transform the Brep geometry
                var newBrep = brep.DuplicateBrep();
                newBrep.Transform(translation);

                // Replace the existing object with the transformed geometry
                _doc.Objects.Replace(obj.Id, newBrep); // Use obj.Id to refer to the object's GUID
                updatedObjects.Add(_doc.Objects.Find(obj.Id));
            }
            _selectedObjects = updatedObjects;
            _doc.Views.Redraw();
        }

        private void MoveObjectsForm_Load(object sender, EventArgs e)
        {

        }
    }
}
