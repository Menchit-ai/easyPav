using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Eto.Forms;
using Rhino;
using Rhino.DocObjects;
using System.Globalization;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace easyPav
{
    public class DiameterCountForm : System.Windows.Forms.Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private RhinoDoc _doc;
        private Dictionary<double, Color> _colorMap;
        private System.Windows.Forms.ListView _listView;
        private Dictionary<double, List<Guid>> _diameterCounts;
        private int _nb_diameters;
        private List<List<Guid>> _rhinoObjects;

        public DiameterCountForm(Dictionary<double, List<Guid>> diameterCounts, RhinoDoc doc)
        {
            Text = "Cone Diameters";
            Size = new System.Drawing.Size(500, 400);
            _doc = doc;
            _nb_diameters = diameterCounts.Keys.Count;
            double threshold = 0.1; // Adjust as needed
            var groupedDiameters = GroupDiameters(diameterCounts, threshold);
            _rhinoObjects = diameterCounts.Values.ToList();
            var _diameterCounts = SortByDiameter(groupedDiameters);


            InitializeColorMap(diameterCounts.Keys.ToList());
            InitializeListView(diameterCounts);

            // Handle form closing to reset colors
            FormClosing += DiameterCountForm_FormClosing;

            // Set Rhino as the owner of this window
            var rhinoMainWindowHandle = Rhino.RhinoApp.MainWindowHandle();
            SetParent(this.Handle, rhinoMainWindowHandle);
        }

        private void InitializeColorMap(List<double> diameters)
        {
            _colorMap = new Dictionary<double, Color>();
            var random = new Random();
            List<Color> colors = GenerateHighlyContrastingColors(_nb_diameters);

            //foreach (var diameter in diameters)
            //{
            //_colorMap[diameter] = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            //}
            for (int i = 0; i < diameters.Count; i++)
            {
                _colorMap[diameters[i]] = colors[i];
            }
        }

        private void InitializeListView(Dictionary<double, List<Guid>> diameterCounts)
        {
            _listView = new System.Windows.Forms.ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details, // Allows column headers
                FullRowSelect = false, // No selection
                MultiSelect = false, // Disables multi-row selection
                GridLines = true, // Optional: to display grid lines for better readability
                OwnerDraw = true // Enable custom drawing for the color column
            };

            // Define columns
            _listView.Columns.Add("Color", 100);
            _listView.Columns.Add("Diameter", 100);
            _listView.Columns.Add("Count", 100);

            // Sort the diameterCounts by diameter (ascending order)
            var sortedDiameterCounts = diameterCounts
                .OrderBy(kvp => kvp.Key) // Sort by diameter
                .ToList(); // Convert to a sorted list

            // Populate rows
            foreach (var kvp in sortedDiameterCounts)
            {
                var diameter = kvp.Key;
                var guids = kvp.Value;
                var color = _colorMap[diameter];


                // Add row to ListView
                var row = new ListViewItem(new[] { "", diameter.ToString("F2"), guids.Count.ToString() });
                row.UseItemStyleForSubItems = false;
                row.Tag = color;
                var current_row = _listView.Items.Add(row);
                // current_row.BackColor = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

                // Apply the color to the objects
                ApplyColorToObjects(guids, color);
            }

            // Add custom drawing events
            _listView.DrawColumnHeader += ListView_DrawColumnHeader;
            _listView.DrawSubItem += ListView_DrawSubItem;


            // Add the ListView to the form
            Controls.Add(_listView);
        }

        public void UpdateData(Dictionary<double, List<Guid>> diameterCounts)
        {
            _listView.Items.Clear(); // Clear existing rows
            InitializeColorMap(diameterCounts.Keys.ToList()); // Recompute colors
            InitializeListView(diameterCounts); // Populate the list view with updated data
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true; // Default header drawing
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 0) // Color column
            {
                // Retrieve the color from the row's tag
                var color = (Color)e.Item.Tag;

                // Draw the background color
                using (var brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                // Optionally draw a border around the cell
                e.Graphics.DrawRectangle(Pens.Black, e.Bounds);
            }
            else
            {
                // Default drawing for other columns
                e.DrawDefault = true;
            }
        }

        private void ApplyColorToObjects(List<Guid> guids, Color color)
        {
            foreach (var guid in guids)
            {
                var rhinoObject = _doc.Objects.Find(guid);
                if (rhinoObject != null)
                {
                    var attributes = rhinoObject.Attributes;
                    attributes.ObjectColor = color;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    _doc.Objects.ModifyAttributes(guid, attributes, quiet: true);
                }
            }

            _doc.Views.Redraw();
        }

        private void DiameterCountForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ResetConeColors();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // DiameterCountForm
            // 
            ClientSize = new Size(284, 261);
            Name = "DiameterCountForm";
            TopMost = true;
            Load += DiameterCountForm_Load;
            ResumeLayout(false);
        }

        private void ResetConeColors()
        {
            foreach (var guids in _rhinoObjects)
            {
                foreach (var guid in guids)
                {
                    var rhinoObject = _doc.Objects.Find(guid);
                    if (rhinoObject != null)
                    {
                        var attributes = rhinoObject.Attributes;
                        attributes.ColorSource = ObjectColorSource.ColorFromLayer; // Reset to default color source
                        _doc.Objects.ModifyAttributes(guid, attributes, quiet: true);
                    }
                }
            }

            _doc.Views.Redraw();
        }

        private void DiameterCountForm_Load(object sender, EventArgs e)
        {

        }

        public static List<Color> GenerateHighlyContrastingColors(int count)
        {
            var colors = new List<Color>();
            //var goldenRatioConjugate = 0.618033988749895; // Golden ratio for even spacing
            var goldenRatioConjugate = 0.818033988749895;
            double hue = 0;

            for (int i = 0; i < count; i++)
            {
                hue = (hue + goldenRatioConjugate) % 1.0; // Evenly distribute hues
                colors.Add(HslToRgb(hue * 360, 0.7, 0.5)); // Use fixed saturation and lightness
            }

            return colors;
        }

        private Dictionary<double, List<Guid>> SortByDiameter(Dictionary<double, List<Guid>> diameterGroups)
        {
            return diameterGroups
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }



        public static Color HslToRgb(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r = 0, g = 0, b = 0;

            if (h >= 0 && h < 60) { r = c; g = x; b = 0; }
            else if (h >= 60 && h < 120) { r = x; g = c; b = 0; }
            else if (h >= 120 && h < 180) { r = 0; g = c; b = x; }
            else if (h >= 180 && h < 240) { r = 0; g = x; b = c; }
            else if (h >= 240 && h < 300) { r = x; g = 0; b = c; }
            else if (h >= 300 && h < 360) { r = c; g = 0; b = x; }

            int rRgb = (int)((r + m) * 255);
            int gRgb = (int)((g + m) * 255);
            int bRgb = (int)((b + m) * 255);

            return Color.FromArgb(rRgb, gRgb, bRgb);
        }

        private Dictionary<double, List<Guid>> GroupDiameters(Dictionary<double, List<Guid>> diameterGroups, double threshold)
        {
            var groupedDiameters = new Dictionary<double, List<Guid>>();

            foreach (var kvp in diameterGroups.OrderBy(kvp => kvp.Key))
            {
                double currentDiameter = kvp.Key;
                bool foundGroup = false;

                foreach (var group in groupedDiameters.Keys.ToList())
                {
                    if (Math.Abs(group - currentDiameter) <= threshold)
                    {
                        // Merge into the existing group
                        groupedDiameters[group].AddRange(kvp.Value);
                        foundGroup = true;
                        break;
                    }
                }

                if (!foundGroup)
                {
                    // Create a new group
                    groupedDiameters[currentDiameter] = new List<Guid>(kvp.Value);
                }
            }

            return groupedDiameters;
        }


    }
}

