using GHelper.UI;
using GHelper.USB;
using System.Drawing.Drawing2D;

namespace GHelper
{
    // Per-key RGB editor for full-size Strix/Scar layouts (G733 family).
    // Slot indices below are positions in Aura.packetMap (the physical LED map).
    public class KeyCustom : RForm
    {
        class DrawnKey
        {
            public int[] Slots;
            public int Row;
            public float X, Y, W, H;
            public string Label;

            public DrawnKey(int[] slots, int row, float x, float y, float w, float h, string label = "")
            {
                Slots = slots;
                Row = row;
                X = x;
                Y = y;
                W = w;
                H = h;
                Label = label;
            }
        }

        class KeyCanvas : Panel
        {
            readonly KeyCustom form;
            readonly List<DrawnKey> keys = new();
            readonly List<Rectangle> rects = new();
            readonly int u = 28;
            readonly int gap = 3;
            readonly int margin = 14;
            float scale = 1;
            int hover = -1;
            bool painting;
            int lastRow = -1;

            public KeyCanvas(KeyCustom form)
            {
                this.form = form;
                DoubleBuffered = true;
                BackColor = RForm.formBack;

                BuildLayout();

                // Slots not drawn in the layout: lightbar (120-129), keyboard status key (130),
                // logo (131) and lid (132, 133). These follow the main keyboard color.
                Aura.CustomKeys.StaticSlots = new int[] { 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133 };
            }

            // G733 (Scar 17) layout in u units. 21.5u wide, numpad columns at 17.5/18.5/19.5/20.5.
            // Lightbar and logo/lid LEDs are not drawn here; they follow the main keyboard
            // color (see Aura.CustomKeys.StaticSlots).
            void BuildLayout()
            {
                // Mini keys above the F row
                string[] mini = { "VOL-", "VOL+", "MIC", "HP", "ARM" };
                for (int i = 0; i < 5; i++)
                    keys.Add(new DrawnKey(new[] { i }, 1, 3 + i, 0.7f, 1, 0.55f, mini[i]));

                // F row
                keys.Add(new DrawnKey(new[] { 5 }, 2, 0, 1.5f, 1, 1, "ESC"));
                for (int i = 1; i <= 12; i++)
                    keys.Add(new DrawnKey(new[] { 5 + i }, 2, i, 1.5f, 1, 1, "F" + i));
                keys.Add(new DrawnKey(new[] { 18 }, 2, 14.5f, 1.5f, 1, 1, "DEL"));
                keys.Add(new DrawnKey(new[] { 19 }, 2, 15.5f, 1.5f, 1, 1, "INS"));
                keys.Add(new DrawnKey(new[] { 20 }, 2, 16.5f, 1.5f, 1, 1, "PAUS"));
                keys.Add(new DrawnKey(new[] { 21 }, 2, 17.5f, 1.5f, 1, 1, "PRT"));
                keys.Add(new DrawnKey(new[] { 22 }, 2, 18.5f, 1.5f, 1, 1, "END"));

                // Number row
                keys.Add(new DrawnKey(new[] { 23 }, 3, 0, 2.6f, 1, 1, "`"));
                string[] digits = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };
                for (int i = 0; i < digits.Length; i++)
                    keys.Add(new DrawnKey(new[] { 23 + 1 + i }, 3, 1 + i, 2.6f, 1, 1, digits[i]));
                keys.Add(new DrawnKey(new[] { 36, 37, 38 }, 3, 13, 2.6f, 2.5f, 1, "BSPC"));
                keys.Add(new DrawnKey(new[] { 39 }, 3, 16.5f, 2.6f, 1, 1, "PLAY"));
                keys.Add(new DrawnKey(new[] { 40 }, 3, 17.5f, 2.6f, 1, 1, "|\\|"));
                keys.Add(new DrawnKey(new[] { 41 }, 3, 18.5f, 2.6f, 1, 1, "|-|"));
                keys.Add(new DrawnKey(new[] { 42 }, 3, 19.5f, 2.6f, 1, 1, "PI"));
                keys.Add(new DrawnKey(new[] { 43 }, 3, 20.5f, 2.6f, 1, 1, "X"));

                // QWERTY row
                keys.Add(new DrawnKey(new[] { 44 }, 4, 0, 3.7f, 1.5f, 1, "TAB"));
                string[] qw = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" };
                for (int i = 0; i < qw.Length; i++)
                    keys.Add(new DrawnKey(new[] { 44 + 1 + i }, 4, 1.5f + i, 3.7f, 1, 1, qw[i]));
                keys.Add(new DrawnKey(new[] { 58 }, 4, 16.5f, 3.7f, 1, 1, "STP"));
                keys.Add(new DrawnKey(new[] { 59 }, 4, 17.5f, 3.7f, 1, 1, "7"));
                keys.Add(new DrawnKey(new[] { 60 }, 4, 18.5f, 3.7f, 1, 1, "8"));
                keys.Add(new DrawnKey(new[] { 61 }, 4, 19.5f, 3.7f, 1, 1, "9"));
                keys.Add(new DrawnKey(new[] { 62, 83 }, 4, 20.5f, 3.7f, 1, 2.1f, "+"));

                // Home row
                keys.Add(new DrawnKey(new[] { 63 }, 5, 0, 4.8f, 1.75f, 1, "CAPS"));
                string[] home = { "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "#" };
                for (int i = 0; i < home.Length; i++)
                    keys.Add(new DrawnKey(new[] { 63 + 1 + i }, 5, 1.75f + i, 4.8f, 1, 1, home[i]));
                keys.Add(new DrawnKey(new[] { 76, 77, 78 }, 5, 13.75f, 4.8f, 2.25f, 1, "ENTER"));
                keys.Add(new DrawnKey(new[] { 79 }, 5, 16.5f, 4.8f, 1, 1, "PRV"));
                keys.Add(new DrawnKey(new[] { 80 }, 5, 17.5f, 4.8f, 1, 1, "4"));
                keys.Add(new DrawnKey(new[] { 81 }, 5, 18.5f, 4.8f, 1, 1, "5"));
                keys.Add(new DrawnKey(new[] { 82 }, 5, 19.5f, 4.8f, 1, 1, "6"));

                // Bottom letter row
                keys.Add(new DrawnKey(new[] { 84 }, 6, 0, 5.9f, 2.25f, 1, "LSHIFT"));
                keys.Add(new DrawnKey(new[] { 85 }, 6, 2.25f, 5.9f, 1, 1, "\\"));
                string[] low = { "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/" };
                for (int i = 0; i < low.Length; i++)
                    keys.Add(new DrawnKey(new[] { 84 + 2 + i }, 6, 3.25f + i, 5.9f, 1, 1, low[i]));
                keys.Add(new DrawnKey(new[] { 96, 97, 98 }, 6, 13.25f, 5.9f, 1.5f, 1, "RSHIFT"));
                keys.Add(new DrawnKey(new[] { 99 }, 6, 15.5f, 5.9f, 1, 1, "^"));
                keys.Add(new DrawnKey(new[] { 100 }, 6, 16.5f, 5.9f, 1, 1, "NEXT"));
                keys.Add(new DrawnKey(new[] { 101 }, 6, 17.5f, 5.9f, 1, 1, "1"));
                keys.Add(new DrawnKey(new[] { 102 }, 6, 18.5f, 5.9f, 1, 1, "2"));
                keys.Add(new DrawnKey(new[] { 103 }, 6, 19.5f, 5.9f, 1, 1, "3"));
                keys.Add(new DrawnKey(new[] { 104, 119 }, 6, 20.5f, 5.9f, 1, 2.1f, "="));

                // Control row (arrows then numpad bottom, aligned with numpad columns)
                keys.Add(new DrawnKey(new[] { 105 }, 7, 0, 7f, 1.25f, 1, "CTRL"));
                keys.Add(new DrawnKey(new[] { 106 }, 7, 1.25f, 7f, 1.25f, 1, "FN"));
                keys.Add(new DrawnKey(new[] { 107 }, 7, 2.5f, 7f, 1.25f, 1, "WIN"));
                keys.Add(new DrawnKey(new[] { 108 }, 7, 3.75f, 7f, 1.25f, 1, "ALT"));
                keys.Add(new DrawnKey(new[] { 109 }, 7, 5, 7f, 5.75f, 1, "SPACE"));
                keys.Add(new DrawnKey(new[] { 110 }, 7, 10.75f, 7f, 1.25f, 1, "ALT"));
                keys.Add(new DrawnKey(new[] { 111 }, 7, 12f, 7f, 1.25f, 1, "FN"));
                keys.Add(new DrawnKey(new[] { 112 }, 7, 13.25f, 7f, 1.25f, 1, "CTRL"));
                keys.Add(new DrawnKey(new[] { 113 }, 7, 14.5f, 7f, 1, 1, "<"));
                keys.Add(new DrawnKey(new[] { 114 }, 7, 15.5f, 7f, 1, 1, "v"));
                keys.Add(new DrawnKey(new[] { 115 }, 7, 16.5f, 7f, 1, 1, ">"));
                keys.Add(new DrawnKey(new[] { 116 }, 7, 17.5f, 7f, 1, 1, "PRT"));
                keys.Add(new DrawnKey(new[] { 117 }, 7, 18.5f, 7f, 1, 1, "0"));
                keys.Add(new DrawnKey(new[] { 118 }, 7, 19.5f, 7f, 1, 1, "."));
            }

            public int U { get { return (int)(u * scale); } }

            public void FillRow(int row, Color c)
            {
                foreach (var k in keys)
                    if (k.Row == row)
                        foreach (int slot in k.Slots)
                            Aura.CustomKeys.Keys[slot] = c;
            }

            Rectangle RectOf(DrawnKey k)
            {
                int x = margin + (int)(k.X * u * scale);
                int y = margin + (int)(k.Y * u * scale);
                int w = (int)(k.W * u * scale) - gap;
                int h = (int)(k.H * u * scale) - gap;
                return new Rectangle(x, y, w, h);
            }

            public void Relayout()
            {
                using (var g = CreateGraphics()) scale = g.DpiX / 96f;
                var size = new Size(
                    margin * 2 + (int)(21.5f * u * scale),
                    margin * 2 + (int)(8.3f * u * scale));
                if (Size != size) Size = size;

                rects.Clear();
                foreach (var k in keys) rects.Add(RectOf(k));

                Invalidate();
            }

            int HitTest(Point p)
            {
                for (int i = 0; i < rects.Count; i++)
                    if (rects[i].Contains(p)) return i;
                return -1;
            }

            void SetKeyColor(int index, Color c)
            {
                if (index < 0) return;
                foreach (int slot in keys[index].Slots)
                    Aura.CustomKeys.Keys[slot] = c;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                int hit = HitTest(e.Location);

                if (hit != hover)
                {
                    hover = hit;
                    Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
                    Invalidate();
                }

                if (painting && hit >= 0)
                {
                    SetKeyColor(hit, form.CurrentColor);
                    Invalidate();
                }

                base.OnMouseMove(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                int hit = HitTest(e.Location);

                if (hit >= 0)
                {
                    painting = true;
                    lastRow = keys[hit].Row;
                    form.LastRow = lastRow;
                    SetKeyColor(hit, form.CurrentColor);
                    Invalidate();
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (painting)
                {
                    painting = false;
                    form.Preview();
                }

                base.OnMouseUp(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (hover >= 0)
                {
                    hover = -1;
                    Invalidate();
                }

                base.OnMouseLeave(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var font = new Font("Segoe UI", 6.5f);

                for (int i = 0; i < keys.Count; i++)
                {
                    var r = rects[i];
                    if (r.Width <= 0 || r.Height <= 0) continue;

                    bool isBar = keys[i].H <= 0.6f;
                    int radius = Math.Min(isBar ? r.Height / 2 : 5, r.Height / 2);

                    var path = RoundedPath(r, radius);
                    using (var fill = new SolidBrush(Color.FromArgb(255,
                        Aura.CustomKeys.Keys[keys[i].Slots[0]].R,
                        Aura.CustomKeys.Keys[keys[i].Slots[0]].G,
                        Aura.CustomKeys.Keys[keys[i].Slots[0]].B)))
                    {
                        g.FillPath(fill, path);
                    }

                    if (i == hover)
                    {
                        using (var pen = new Pen(RForm.foreMain, 1.5f))
                            g.DrawPath(pen, path);
                    }
                    else if (!isBar)
                    {
                        using (var pen = new Pen(RForm.borderMain))
                            g.DrawPath(pen, path);
                    }

                    if (!string.IsNullOrEmpty(keys[i].Label))
                    {
                        var bounds = new RectangleF(r.X + 2, r.Y, Math.Max(r.Width - 4, 1), r.Height);
                        g.DrawString(keys[i].Label, font, Brushes.Gray, bounds,
                            StringFormat.GenericTypographic);
                    }

                    path.Dispose();
                }
            }

            static GraphicsPath RoundedPath(Rectangle r, int radius)
            {
                var path = new GraphicsPath();
                int d = radius * 2;

                if (d < 2)
                {
                    path.AddRectangle(r);
                    return path;
                }

                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                return path;
            }
        }

        public Color CurrentColor = Color.FromArgb(0x00, 0xC8, 0xFF);
        public int LastRow = -1;

        readonly KeyCanvas canvas;

        public KeyCustom()
        {
            Font = new Font("Segoe UI", 9F);
            Text = Properties.Strings.KeyCustomTitle;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;

            Aura.CustomKeys.Load();

            canvas = new KeyCanvas(this);
            canvas.Location = new Point(12, 12);
            Controls.Add(canvas);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                Padding = new Padding(8, 4, 8, 4),
            };
            Controls.Add(flow);

            var buttonColor = new RColorButton
            {
                Text = Properties.Strings.Color,
                AutoSize = false,
                Size = new Size(110, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonColor.SwatchColor = CurrentColor;
            buttonColor.Click += (s, e) => PickColor(buttonColor);
            flow.Controls.Add(buttonColor);

            var buttonFillAll = new RButton
            {
                Text = Properties.Strings.KeyCustomFillAll,
                Secondary = true,
                Size = new Size(90, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonFillAll.Click += (s, e) =>
            {
                Aura.CustomKeys.Fill(CurrentColor);
                canvas.Invalidate();
                Preview();
            };
            flow.Controls.Add(buttonFillAll);

            var buttonFillRow = new RButton
            {
                Text = Properties.Strings.KeyCustomFillRow,
                Secondary = true,
                Size = new Size(90, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonFillRow.Click += (s, e) =>
            {
                if (LastRow < 0) return;

                canvas.FillRow(LastRow, CurrentColor);
                Aura.CustomKeys.Save();
                canvas.Invalidate();
                Preview();
            };
            flow.Controls.Add(buttonFillRow);

            var buttonGradient = new RButton
            {
                Text = Properties.Strings.KeyCustomGradient,
                Secondary = true,
                Size = new Size(90, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonGradient.Click += (s, e) =>
            {
                Aura.CustomKeys.FillGradient(CurrentColor, Color.White);
                canvas.Invalidate();
                Preview();
            };
            flow.Controls.Add(buttonGradient);

            var buttonTest = new RButton
            {
                Text = Properties.Strings.KeyCustomTest,
                Secondary = true,
                Size = new Size(130, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonTest.Click += (s, e) =>
            {
                // One hue per slot, left to right. Verify the physical mapping on the
                // real board: hue i must land on the drawn key at position i. Not saved.
                Task.Run(() => Aura.CustomKeys.ApplyTestPattern());
            };
            flow.Controls.Add(buttonTest);

            var buttonReset = new RButton
            {
                Text = Properties.Strings.KeyCustomReset,
                Secondary = true,
                Size = new Size(80, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonReset.Click += (s, e) =>
            {
                Aura.CustomKeys.Reset();
                canvas.Invalidate();
                Preview();
            };
            flow.Controls.Add(buttonReset);

            var buttonApply = new RButton
            {
                Text = Properties.Strings.KeyCustomApply,
                Size = new Size(90, 40),
                Margin = new Padding(4, 4, 4, 4),
            };
            buttonApply.Click += (s, e) => Apply();
            flow.Controls.Add(buttonApply);

            InitTheme();

            Load += (s, e) =>
            {
                canvas.Relayout();
                ClientSize = new Size(canvas.Width + 24, canvas.Height + 12 + 56);
            };
            Shown += (s, e) =>
            {
                canvas.Relayout();
                ClientSize = new Size(canvas.Width + 24, canvas.Height + 12 + 56);
            };
        }

        void PickColor(RColorButton button)
        {
            using var picker = new RColorPicker(CurrentColor);
            picker.ColorChanged += c =>
            {
                CurrentColor = c;
                button.SwatchColor = c;
            };
            picker.ShowDialog(this);
        }

        // Live preview without persisting (called on stroke end)
        public void Preview()
        {
            Task.Run(() => Aura.CustomKeys.Preview());
        }

        // Save, switch the main form to Custom Keys mode (persists across restarts)
        public void Apply()
        {
            Aura.CustomKeys.Save();
            Program.settingsForm?.SelectAuraMode(AuraMode.CustomKeys);
        }
    }
}
