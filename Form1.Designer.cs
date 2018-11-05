namespace INFOIBV
{
    partial class INFOIBV
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(INFOIBV));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.LoadImageButton1 = new System.Windows.Forms.Button();
            this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageFileName1 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveButton = new System.Windows.Forms.Button();
            this.outputBox1 = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.kernelInput = new System.Windows.Forms.TextBox();
            this.ErosionRadio = new System.Windows.Forms.RadioButton();
            this.DilationRadio = new System.Windows.Forms.RadioButton();
            this.OpeningRadio = new System.Windows.Forms.RadioButton();
            this.ClosingRadio = new System.Windows.Forms.RadioButton();
            this.ValueRadio = new System.Windows.Forms.RadioButton();
            this.BoundaryRadio = new System.Windows.Forms.RadioButton();
            this.FourierRadio = new System.Windows.Forms.RadioButton();
            this.MessageBox2 = new System.Windows.Forms.TextBox();
            this.complementRadio = new System.Windows.Forms.RadioButton();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.FourierSamples = new System.Windows.Forms.Label();
            this.checkBinary = new System.Windows.Forms.CheckBox();
            this.checkBlackBackground = new System.Windows.Forms.CheckBox();
            this.thresholdRadio = new System.Windows.Forms.RadioButton();
            this.thresholdTrackbar = new System.Windows.Forms.TrackBar();
            this.edgeDetection = new System.Windows.Forms.RadioButton();
            this.RightAsInput = new System.Windows.Forms.CheckBox();
            this.thresholdValue = new System.Windows.Forms.Label();
            this.greyscaleRadio = new System.Windows.Forms.RadioButton();
            this.preprocessingRadio = new System.Windows.Forms.RadioButton();
            this.regionLabelRadio = new System.Windows.Forms.RadioButton();
            this.cornerDetRadio = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thresholdTrackbar)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadImageButton1
            // 
            this.LoadImageButton1.Location = new System.Drawing.Point(12, 12);
            this.LoadImageButton1.Name = "LoadImageButton1";
            this.LoadImageButton1.Size = new System.Drawing.Size(98, 23);
            this.LoadImageButton1.TabIndex = 0;
            this.LoadImageButton1.Text = "Load image 1...";
            this.LoadImageButton1.UseVisualStyleBackColor = true;
            this.LoadImageButton1.Click += new System.EventHandler(this.LoadImageButton1_Click);
            // 
            // openImageDialog
            // 
            this.openImageDialog.Filter = "Bitmap files (*.bmp;*.gif;*.jpg;*.png;*.tiff;*.jpeg)|*.bmp;*.gif;*.jpg;*.png;*.ti" +
    "ff;*.jpeg";
            this.openImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // imageFileName1
            // 
            this.imageFileName1.Location = new System.Drawing.Point(116, 14);
            this.imageFileName1.Name = "imageFileName1";
            this.imageFileName1.ReadOnly = true;
            this.imageFileName1.Size = new System.Drawing.Size(316, 20);
            this.imageFileName1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(15, 40);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 512);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(457, 687);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(103, 23);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // saveImageDialog
            // 
            this.saveImageDialog.Filter = "Bitmap file (*.bmp)|*.bmp";
            this.saveImageDialog.InitialDirectory = "..\\..\\images";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(902, 687);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(103, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save as BMP...";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // outputBox1
            // 
            this.outputBox1.Location = new System.Drawing.Point(533, 41);
            this.outputBox1.Name = "outputBox1";
            this.outputBox1.Size = new System.Drawing.Size(512, 512);
            this.outputBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.outputBox1.TabIndex = 5;
            this.outputBox1.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(457, 664);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(278, 20);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // kernelInput
            // 
            this.kernelInput.Location = new System.Drawing.Point(15, 582);
            this.kernelInput.Multiline = true;
            this.kernelInput.Name = "kernelInput";
            this.kernelInput.Size = new System.Drawing.Size(216, 128);
            this.kernelInput.TabIndex = 7;
            this.kernelInput.Text = resources.GetString("kernelInput.Text");
            // 
            // ErosionRadio
            // 
            this.ErosionRadio.AutoSize = true;
            this.ErosionRadio.Location = new System.Drawing.Point(258, 606);
            this.ErosionRadio.Name = "ErosionRadio";
            this.ErosionRadio.Size = new System.Drawing.Size(60, 17);
            this.ErosionRadio.TabIndex = 9;
            this.ErosionRadio.TabStop = true;
            this.ErosionRadio.Text = "Erosion";
            this.ErosionRadio.UseVisualStyleBackColor = true;
            // 
            // DilationRadio
            // 
            this.DilationRadio.AutoSize = true;
            this.DilationRadio.Location = new System.Drawing.Point(258, 630);
            this.DilationRadio.Name = "DilationRadio";
            this.DilationRadio.Size = new System.Drawing.Size(60, 17);
            this.DilationRadio.TabIndex = 10;
            this.DilationRadio.TabStop = true;
            this.DilationRadio.Text = "Dilation";
            this.DilationRadio.UseVisualStyleBackColor = true;
            // 
            // OpeningRadio
            // 
            this.OpeningRadio.AutoSize = true;
            this.OpeningRadio.Location = new System.Drawing.Point(258, 653);
            this.OpeningRadio.Name = "OpeningRadio";
            this.OpeningRadio.Size = new System.Drawing.Size(65, 17);
            this.OpeningRadio.TabIndex = 11;
            this.OpeningRadio.TabStop = true;
            this.OpeningRadio.Text = "Opening";
            this.OpeningRadio.UseVisualStyleBackColor = true;
            // 
            // ClosingRadio
            // 
            this.ClosingRadio.AutoSize = true;
            this.ClosingRadio.Location = new System.Drawing.Point(258, 676);
            this.ClosingRadio.Name = "ClosingRadio";
            this.ClosingRadio.Size = new System.Drawing.Size(59, 17);
            this.ClosingRadio.TabIndex = 12;
            this.ClosingRadio.TabStop = true;
            this.ClosingRadio.Text = "Closing";
            this.ClosingRadio.UseVisualStyleBackColor = true;
            // 
            // ValueRadio
            // 
            this.ValueRadio.AutoSize = true;
            this.ValueRadio.Location = new System.Drawing.Point(457, 579);
            this.ValueRadio.Name = "ValueRadio";
            this.ValueRadio.Size = new System.Drawing.Size(97, 17);
            this.ValueRadio.TabIndex = 15;
            this.ValueRadio.TabStop = true;
            this.ValueRadio.Text = "Value Counting";
            this.ValueRadio.UseVisualStyleBackColor = true;
            this.ValueRadio.CheckedChanged += new System.EventHandler(this.ValueRadio_CheckedChanged);
            // 
            // BoundaryRadio
            // 
            this.BoundaryRadio.AutoSize = true;
            this.BoundaryRadio.Location = new System.Drawing.Point(457, 602);
            this.BoundaryRadio.Name = "BoundaryRadio";
            this.BoundaryRadio.Size = new System.Drawing.Size(101, 17);
            this.BoundaryRadio.TabIndex = 16;
            this.BoundaryRadio.TabStop = true;
            this.BoundaryRadio.Text = "Boundary Trace";
            this.BoundaryRadio.UseVisualStyleBackColor = true;
            this.BoundaryRadio.CheckedChanged += new System.EventHandler(this.BoundaryRadio_CheckedChanged);
            // 
            // FourierRadio
            // 
            this.FourierRadio.AutoSize = true;
            this.FourierRadio.Location = new System.Drawing.Point(457, 625);
            this.FourierRadio.Name = "FourierRadio";
            this.FourierRadio.Size = new System.Drawing.Size(138, 17);
            this.FourierRadio.TabIndex = 17;
            this.FourierRadio.TabStop = true;
            this.FourierRadio.Text = "Fourier shape descriptor";
            this.FourierRadio.UseVisualStyleBackColor = true;
            this.FourierRadio.CheckedChanged += new System.EventHandler(this.FourierRadio_CheckedChanged);
            // 
            // MessageBox2
            // 
            this.MessageBox2.Location = new System.Drawing.Point(566, 690);
            this.MessageBox2.Name = "MessageBox2";
            this.MessageBox2.ReadOnly = true;
            this.MessageBox2.Size = new System.Drawing.Size(319, 20);
            this.MessageBox2.TabIndex = 23;
            // 
            // complementRadio
            // 
            this.complementRadio.AutoSize = true;
            this.complementRadio.Location = new System.Drawing.Point(258, 579);
            this.complementRadio.Name = "complementRadio";
            this.complementRadio.Size = new System.Drawing.Size(171, 17);
            this.complementRadio.TabIndex = 24;
            this.complementRadio.TabStop = true;
            this.complementRadio.Text = "Complementary (inverse) image";
            this.complementRadio.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            this.chart1.BorderlineWidth = 10;
            chartArea1.AxisX.LabelAutoFitMinFontSize = 20;
            chartArea1.AxisX.Title = "n";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            chartArea1.AxisY.LabelAutoFitMinFontSize = 20;
            chartArea1.AxisY.Title = "Cn Value";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Alignment = System.Drawing.StringAlignment.Far;
            legend1.AutoFitMinFontSize = 10;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(1050, 18);
            this.chart1.Margin = new System.Windows.Forms.Padding(2);
            this.chart1.Name = "chart1";
            series1.BorderWidth = 8;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "Cn Real";
            series2.BorderWidth = 8;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.Legend = "Legend1";
            series2.Name = "Cn Imaginary";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(518, 544);
            this.chart1.TabIndex = 26;
            this.chart1.Text = "Cn plot";
            title1.Name = "Fourier Transform";
            title1.Text = "Fourier Descriptors";
            this.chart1.Titles.Add(title1);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(601, 622);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 27;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // FourierSamples
            // 
            this.FourierSamples.AutoSize = true;
            this.FourierSamples.Location = new System.Drawing.Point(598, 606);
            this.FourierSamples.Name = "FourierSamples";
            this.FourierSamples.Size = new System.Drawing.Size(106, 13);
            this.FourierSamples.TabIndex = 28;
            this.FourierSamples.Text = "sample every n steps";
            this.FourierSamples.Click += new System.EventHandler(this.FourierSamples_Click);
            // 
            // checkBinary
            // 
            this.checkBinary.AutoSize = true;
            this.checkBinary.Location = new System.Drawing.Point(438, 18);
            this.checkBinary.Name = "checkBinary";
            this.checkBinary.Size = new System.Drawing.Size(91, 17);
            this.checkBinary.TabIndex = 29;
            this.checkBinary.Text = "input is Binary";
            this.checkBinary.UseVisualStyleBackColor = true;
            // 
            // checkBlackBackground
            // 
            this.checkBlackBackground.AutoSize = true;
            this.checkBlackBackground.Location = new System.Drawing.Point(535, 17);
            this.checkBlackBackground.Name = "checkBlackBackground";
            this.checkBlackBackground.Size = new System.Drawing.Size(194, 17);
            this.checkBlackBackground.TabIndex = 30;
            this.checkBlackBackground.Text = "black background white foreground";
            this.checkBlackBackground.UseVisualStyleBackColor = true;
            // 
            // thresholdRadio
            // 
            this.thresholdRadio.AutoSize = true;
            this.thresholdRadio.Location = new System.Drawing.Point(727, 582);
            this.thresholdRadio.Margin = new System.Windows.Forms.Padding(2);
            this.thresholdRadio.Name = "thresholdRadio";
            this.thresholdRadio.Size = new System.Drawing.Size(97, 17);
            this.thresholdRadio.TabIndex = 32;
            this.thresholdRadio.TabStop = true;
            this.thresholdRadio.Text = "Threshold Filter";
            this.thresholdRadio.UseVisualStyleBackColor = true;
            this.thresholdRadio.CheckedChanged += new System.EventHandler(this.thresholdRadio_CheckedChanged);
            // 
            // thresholdTrackbar
            // 
            this.thresholdTrackbar.Enabled = false;
            this.thresholdTrackbar.Location = new System.Drawing.Point(828, 582);
            this.thresholdTrackbar.Margin = new System.Windows.Forms.Padding(2);
            this.thresholdTrackbar.Maximum = 255;
            this.thresholdTrackbar.Name = "thresholdTrackbar";
            this.thresholdTrackbar.Size = new System.Drawing.Size(94, 45);
            this.thresholdTrackbar.TabIndex = 31;
            this.thresholdTrackbar.Value = 127;
            this.thresholdTrackbar.Scroll += new System.EventHandler(this.thresholdTrackbar_Scroll);
            // 
            // edgeDetection
            // 
            this.edgeDetection.Location = new System.Drawing.Point(727, 623);
            this.edgeDetection.Name = "edgeDetection";
            this.edgeDetection.Size = new System.Drawing.Size(104, 24);
            this.edgeDetection.TabIndex = 33;
            this.edgeDetection.Text = "Edge Detection";
            // 
            // RightAsInput
            // 
            this.RightAsInput.AutoSize = true;
            this.RightAsInput.Location = new System.Drawing.Point(727, 18);
            this.RightAsInput.Name = "RightAsInput";
            this.RightAsInput.Size = new System.Drawing.Size(142, 17);
            this.RightAsInput.TabIndex = 34;
            this.RightAsInput.Text = "use Right image as input";
            this.RightAsInput.UseVisualStyleBackColor = true;
            // 
            // thresholdValue
            // 
            this.thresholdValue.AutoSize = true;
            this.thresholdValue.Location = new System.Drawing.Point(926, 581);
            this.thresholdValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.thresholdValue.Name = "thresholdValue";
            this.thresholdValue.Size = new System.Drawing.Size(25, 13);
            this.thresholdValue.TabIndex = 35;
            this.thresholdValue.Text = "127";
            this.thresholdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // greyscaleRadio
            // 
            this.greyscaleRadio.AutoSize = true;
            this.greyscaleRadio.Location = new System.Drawing.Point(727, 606);
            this.greyscaleRadio.Margin = new System.Windows.Forms.Padding(2);
            this.greyscaleRadio.Name = "greyscaleRadio";
            this.greyscaleRadio.Size = new System.Drawing.Size(72, 17);
            this.greyscaleRadio.TabIndex = 36;
            this.greyscaleRadio.TabStop = true;
            this.greyscaleRadio.Text = "Greyscale";
            this.greyscaleRadio.UseVisualStyleBackColor = true;
            // 
            // preprocessingRadio
            // 
            this.preprocessingRadio.AutoSize = true;
            this.preprocessingRadio.Location = new System.Drawing.Point(836, 627);
            this.preprocessingRadio.Margin = new System.Windows.Forms.Padding(2);
            this.preprocessingRadio.Name = "preprocessingRadio";
            this.preprocessingRadio.Size = new System.Drawing.Size(132, 17);
            this.preprocessingRadio.TabIndex = 37;
            this.preprocessingRadio.TabStop = true;
            this.preprocessingRadio.Text = "Preprocessing Pipeline";
            this.preprocessingRadio.UseVisualStyleBackColor = true;
            // 
            // regionLabelRadio
            // 
            this.regionLabelRadio.AutoSize = true;
            this.regionLabelRadio.Location = new System.Drawing.Point(836, 649);
            this.regionLabelRadio.Margin = new System.Windows.Forms.Padding(2);
            this.regionLabelRadio.Name = "regionLabelRadio";
            this.regionLabelRadio.Size = new System.Drawing.Size(102, 17);
            this.regionLabelRadio.TabIndex = 38;
            this.regionLabelRadio.TabStop = true;
            this.regionLabelRadio.Text = "Region Labeling";
            this.regionLabelRadio.UseVisualStyleBackColor = true;
            // 
            // cornerDetRadio
            // 
            this.cornerDetRadio.AutoSize = true;
            this.cornerDetRadio.Location = new System.Drawing.Point(458, 647);
            this.cornerDetRadio.Margin = new System.Windows.Forms.Padding(2);
            this.cornerDetRadio.Name = "cornerDetRadio";
            this.cornerDetRadio.Size = new System.Drawing.Size(102, 17);
            this.cornerDetRadio.TabIndex = 36;
            this.cornerDetRadio.TabStop = true;
            this.cornerDetRadio.Text = "corner detection";
            this.cornerDetRadio.UseVisualStyleBackColor = true;
            // 
            // INFOIBV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1588, 739);
            this.Controls.Add(this.regionLabelRadio);
            this.Controls.Add(this.preprocessingRadio);
            this.Controls.Add(this.greyscaleRadio);
            this.Controls.Add(this.cornerDetRadio);
            this.Controls.Add(this.thresholdValue);
            this.Controls.Add(this.RightAsInput);
            this.Controls.Add(this.edgeDetection);
            this.Controls.Add(this.thresholdRadio);
            this.Controls.Add(this.thresholdTrackbar);
            this.Controls.Add(this.checkBlackBackground);
            this.Controls.Add(this.checkBinary);
            this.Controls.Add(this.FourierSamples);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.complementRadio);
            this.Controls.Add(this.MessageBox2);
            this.Controls.Add(this.FourierRadio);
            this.Controls.Add(this.BoundaryRadio);
            this.Controls.Add(this.ValueRadio);
            this.Controls.Add(this.ClosingRadio);
            this.Controls.Add(this.OpeningRadio);
            this.Controls.Add(this.DilationRadio);
            this.Controls.Add(this.ErosionRadio);
            this.Controls.Add(this.kernelInput);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.outputBox1);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.imageFileName1);
            this.Controls.Add(this.LoadImageButton1);
            this.Location = new System.Drawing.Point(10, 10);
            this.Name = "INFOIBV";
            this.ShowIcon = false;
            this.Text = "INFOIBV";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thresholdTrackbar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadImageButton1;
        private System.Windows.Forms.OpenFileDialog openImageDialog;
        private System.Windows.Forms.TextBox imageFileName1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.SaveFileDialog saveImageDialog;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.PictureBox outputBox1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox kernelInput;
        private System.Windows.Forms.RadioButton ErosionRadio;
        private System.Windows.Forms.RadioButton DilationRadio;
        private System.Windows.Forms.RadioButton OpeningRadio;
        private System.Windows.Forms.RadioButton ClosingRadio;
        private System.Windows.Forms.RadioButton ValueRadio;
        private System.Windows.Forms.RadioButton BoundaryRadio;
        private System.Windows.Forms.RadioButton FourierRadio;
        private System.Windows.Forms.TextBox MessageBox2;
        private System.Windows.Forms.RadioButton complementRadio;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label FourierSamples;
        private System.Windows.Forms.CheckBox checkBinary;
        private System.Windows.Forms.CheckBox checkBlackBackground;
        private System.Windows.Forms.RadioButton thresholdRadio;
        private System.Windows.Forms.TrackBar thresholdTrackbar;
        private System.Windows.Forms.RadioButton edgeDetection;
        private System.Windows.Forms.CheckBox RightAsInput;
        private System.Windows.Forms.Label thresholdValue;
        private System.Windows.Forms.RadioButton greyscaleRadio;
        private System.Windows.Forms.RadioButton preprocessingRadio;
        private System.Windows.Forms.RadioButton regionLabelRadio;
        private System.Windows.Forms.RadioButton cornerDetRadio;
    }
}

