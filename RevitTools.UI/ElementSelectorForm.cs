using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitTools.UI
{
    public class ElementSelectorForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAll;
        private System.Windows.Forms.Button buttonNone;

        public Dictionary<ElementId, string> SelectedElements { get; private set; }

        public ElementSelectorForm(Dictionary<ElementId, string> elements)
        {
            InitializeComponent();
            SelectedElements = new Dictionary<ElementId, string>();

            foreach (var elem in elements)
            {
                checkedListBox1.Items.Add(
                    new ElementItem {
                        Id = elem.Key,
                        Display = elem.Value
                    },
                    false
                );
            }
        }

        private void InitializeComponent()
        {
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAll = new System.Windows.Forms.Button();
            this.buttonNone = new System.Windows.Forms.Button();

            // CheckedListBox
            this.checkedListBox1.Location = new System.Drawing.Point(12, 12);
            this.checkedListBox1.Size = new System.Drawing.Size(360, 260);
            this.checkedListBox1.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            // OK button
            this.buttonOk.Text = "OK";
            this.buttonOk.Location = new System.Drawing.Point(12, 280);
            this.buttonOk.Anchor =
                System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left;
            this.buttonOk.Click += buttonOk_Click;

            // Cancel button
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Location = new System.Drawing.Point(100, 280);
            this.buttonCancel.Anchor =
                System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left;
            this.buttonCancel.Click += buttonCancel_Click;

            // All button
            this.buttonAll.Text = "All";
            this.buttonAll.Location = new System.Drawing.Point(188, 280);
            this.buttonAll.Anchor =
                System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left;
            this.buttonAll.Click += buttonAll_Click;

            // None button
            this.buttonNone.Text = "None";
            this.buttonNone.Location = new System.Drawing.Point(276, 280);
            this.buttonNone.Anchor =
                System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left;
            this.buttonNone.Click += buttonNone_Click;

            // Form settings
            this.Text = "Select elements";
            this.ClientSize = new System.Drawing.Size(400, 320);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;

            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAll);
            this.Controls.Add(this.buttonNone);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SelectedElements.Clear();

            foreach (ElementItem item in checkedListBox1.CheckedItems)
            {
                SelectedElements.Add(item.Id, item.Display);
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, true);
        }

        private void buttonNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, false);
        }
    }
}
