namespace PhotoPrinter;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            imageToPrint?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        printerCheckedListBox = new CheckedListBox();
        paperSizeComboBox = new ComboBox();
        paperSourceComboBox = new ComboBox();
        paperTypeComboBox = new ComboBox();
        layoutComboBox = new ComboBox();
        customWidthTextBox = new TextBox();
        customHeightTextBox = new TextBox();
        borderlessCheckBox = new CheckBox();
        orientationComboBox = new ComboBox();
        selectPhotoButton = new Button();
        printButton = new Button();
        previewPictureBox = new PictureBox();
        label1 = new Label();
        label2 = new Label();
        label3 = new Label();
        label4 = new Label();
        label5 = new Label();
        label6 = new Label();
        label7 = new Label();
        label8 = new Label();
        logTextBox = new TextBox();
        eventCodeTextBox = new TextBox();
        startPrintsButton = new Button();
        cancelButton = new Button();
        label9 = new Label();
        label10 = new Label();
        printQualityComboBox = new ComboBox();
        label11 = new Label();
        ((System.ComponentModel.ISupportInitialize)previewPictureBox).BeginInit();
        SuspendLayout();
        //
        // printerCheckedListBox
        //
        printerCheckedListBox.CheckOnClick = true;
        printerCheckedListBox.FormattingEnabled = true;
        printerCheckedListBox.Location = new Point(120, 20);
        printerCheckedListBox.Name = "printerCheckedListBox";
        printerCheckedListBox.Size = new Size(300, 150);
        printerCheckedListBox.TabIndex = 0;
        printerCheckedListBox.ItemCheck += PrinterCheckedListBox_ItemCheck;
        //
        // paperSizeComboBox
        //
        paperSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        paperSizeComboBox.FormattingEnabled = true;
        paperSizeComboBox.Location = new Point(120, 190);
        paperSizeComboBox.Name = "paperSizeComboBox";
        paperSizeComboBox.Size = new Size(300, 28);
        paperSizeComboBox.TabIndex = 1;
        paperSizeComboBox.SelectedIndexChanged += PaperSizeComboBox_SelectedIndexChanged;
        //
        // paperSourceComboBox
        //
        paperSourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        paperSourceComboBox.FormattingEnabled = true;
        paperSourceComboBox.Location = new Point(120, 230);
        paperSourceComboBox.Name = "paperSourceComboBox";
        paperSourceComboBox.Size = new Size(300, 28);
        paperSourceComboBox.TabIndex = 2;
        //
        // paperTypeComboBox
        //
        paperTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        paperTypeComboBox.FormattingEnabled = true;
        paperTypeComboBox.Location = new Point(120, 270);
        paperTypeComboBox.Name = "paperTypeComboBox";
        paperTypeComboBox.Size = new Size(300, 28);
        paperTypeComboBox.TabIndex = 3;
        //
        // layoutComboBox
        //
        layoutComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        layoutComboBox.FormattingEnabled = true;
        layoutComboBox.Location = new Point(120, 310);
        layoutComboBox.Name = "layoutComboBox";
        layoutComboBox.Size = new Size(300, 28);
        layoutComboBox.TabIndex = 4;
        //
        // customWidthTextBox
        //
        customWidthTextBox.Enabled = false;
        customWidthTextBox.Location = new Point(120, 390);
        customWidthTextBox.Name = "customWidthTextBox";
        customWidthTextBox.PlaceholderText = "Width (inches)";
        customWidthTextBox.Size = new Size(145, 27);
        customWidthTextBox.TabIndex = 5;
        //
        // customHeightTextBox
        //
        customHeightTextBox.Enabled = false;
        customHeightTextBox.Location = new Point(275, 390);
        customHeightTextBox.Name = "customHeightTextBox";
        customHeightTextBox.PlaceholderText = "Height (inches)";
        customHeightTextBox.Size = new Size(145, 27);
        customHeightTextBox.TabIndex = 6;
        //
        // borderlessCheckBox
        //
        borderlessCheckBox.AutoSize = true;
        borderlessCheckBox.Location = new Point(120, 430);
        borderlessCheckBox.Name = "borderlessCheckBox";
        borderlessCheckBox.Size = new Size(145, 24);
        borderlessCheckBox.TabIndex = 7;
        borderlessCheckBox.Text = "Borderless Printing";
        borderlessCheckBox.UseVisualStyleBackColor = true;
        //
        // orientationComboBox
        //
        orientationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        orientationComboBox.FormattingEnabled = true;
        orientationComboBox.Location = new Point(275, 425);
        orientationComboBox.Name = "orientationComboBox";
        orientationComboBox.Size = new Size(145, 28);
        orientationComboBox.TabIndex = 27;
        //
        // selectPhotoButton
        //
        selectPhotoButton.Location = new Point(120, 470);
        selectPhotoButton.Name = "selectPhotoButton";
        selectPhotoButton.Size = new Size(150, 35);
        selectPhotoButton.TabIndex = 8;
        selectPhotoButton.Text = "Select Photo";
        selectPhotoButton.UseVisualStyleBackColor = true;
        selectPhotoButton.Click += SelectPhotoButton_Click;
        //
        // printButton
        //
        printButton.Enabled = false;
        printButton.Location = new Point(290, 470);
        printButton.Name = "printButton";
        printButton.Size = new Size(130, 35);
        printButton.TabIndex = 9;
        printButton.Text = "Print";
        printButton.UseVisualStyleBackColor = true;
        printButton.Click += PrintButton_Click;
        //
        // previewPictureBox
        //
        previewPictureBox.BorderStyle = BorderStyle.FixedSingle;
        previewPictureBox.Location = new Point(450, 20);
        previewPictureBox.Name = "previewPictureBox";
        previewPictureBox.Size = new Size(400, 400);
        previewPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        previewPictureBox.TabIndex = 10;
        previewPictureBox.TabStop = false;
        //
        // label1
        //
        label1.AutoSize = true;
        label1.Location = new Point(20, 23);
        label1.Name = "label1";
        label1.Size = new Size(65, 20);
        label1.TabIndex = 11;
        label1.Text = "Printers:";
        //
        // label2
        //
        label2.AutoSize = true;
        label2.Location = new Point(20, 193);
        label2.Name = "label2";
        label2.Size = new Size(79, 20);
        label2.TabIndex = 12;
        label2.Text = "Paper Size:";
        //
        // label3
        //
        label3.AutoSize = true;
        label3.Location = new Point(20, 393);
        label3.Name = "label3";
        label3.Size = new Size(90, 20);
        label3.TabIndex = 13;
        label3.Text = "Custom Size:";
        //
        // label4
        //
        label4.AutoSize = true;
        label4.Location = new Point(120, 480);
        label4.Name = "label4";
        label4.Size = new Size(0, 20);
        label4.TabIndex = 14;
        //
        // label5
        //
        label5.AutoSize = true;
        label5.Location = new Point(450, 430);
        label5.Name = "label5";
        label5.Size = new Size(0, 20);
        label5.TabIndex = 15;
        //
        // label6
        //
        label6.AutoSize = true;
        label6.Location = new Point(20, 233);
        label6.Name = "label6";
        label6.Size = new Size(100, 20);
        label6.TabIndex = 16;
        label6.Text = "Paper Source:";
        //
        // label7
        //
        label7.AutoSize = true;
        label7.Location = new Point(20, 273);
        label7.Name = "label7";
        label7.Size = new Size(85, 20);
        label7.TabIndex = 17;
        label7.Text = "Paper Type:";
        //
        // label8
        //
        label8.AutoSize = true;
        label8.Location = new Point(20, 313);
        label8.Name = "label8";
        label8.Size = new Size(55, 20);
        label8.TabIndex = 18;
        label8.Text = "Layout:";
        //
        // printQualityComboBox
        //
        printQualityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        printQualityComboBox.FormattingEnabled = true;
        printQualityComboBox.Location = new Point(120, 350);
        printQualityComboBox.Name = "printQualityComboBox";
        printQualityComboBox.Size = new Size(300, 28);
        printQualityComboBox.TabIndex = 25;
        //
        // label11
        //
        label11.AutoSize = true;
        label11.Location = new Point(20, 353);
        label11.Name = "label11";
        label11.Size = new Size(93, 20);
        label11.TabIndex = 26;
        label11.Text = "Print Quality:";
        //
        // logTextBox
        //
        logTextBox.BackColor = SystemColors.Window;
        logTextBox.Font = new Font("Consolas", 9F);
        logTextBox.Location = new Point(20, 560);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Size = new Size(830, 120);
        logTextBox.TabIndex = 19;
        //
        // eventCodeTextBox
        //
        eventCodeTextBox.CharacterCasing = CharacterCasing.Upper;
        eventCodeTextBox.Location = new Point(120, 695);
        eventCodeTextBox.Name = "eventCodeTextBox";
        eventCodeTextBox.PlaceholderText = "e.g., EM191";
        eventCodeTextBox.Size = new Size(300, 27);
        eventCodeTextBox.TabIndex = 20;
        //
        // startPrintsButton
        //
        startPrintsButton.Location = new Point(450, 690);
        startPrintsButton.Name = "startPrintsButton";
        startPrintsButton.Size = new Size(150, 35);
        startPrintsButton.TabIndex = 21;
        startPrintsButton.Text = "Start Prints";
        startPrintsButton.UseVisualStyleBackColor = true;
        startPrintsButton.Click += StartPrintsButton_Click;
        //
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(620, 690);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(150, 35);
        cancelButton.TabIndex = 22;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        //
        // label9
        //
        label9.AutoSize = true;
        label9.Location = new Point(20, 530);
        label9.Name = "label9";
        label9.Size = new Size(77, 20);
        label9.TabIndex = 23;
        label9.Text = "Auto Print:";
        //
        // label10
        //
        label10.AutoSize = true;
        label10.Location = new Point(20, 698);
        label10.Name = "label10";
        label10.Size = new Size(85, 20);
        label10.TabIndex = 24;
        label10.Text = "Event Code:";
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(880, 740);
        Controls.Add(label11);
        Controls.Add(printQualityComboBox);
        Controls.Add(label10);
        Controls.Add(label9);
        Controls.Add(cancelButton);
        Controls.Add(startPrintsButton);
        Controls.Add(eventCodeTextBox);
        Controls.Add(logTextBox);
        Controls.Add(label8);
        Controls.Add(label7);
        Controls.Add(label6);
        Controls.Add(label5);
        Controls.Add(label4);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(previewPictureBox);
        Controls.Add(printButton);
        Controls.Add(selectPhotoButton);
        Controls.Add(orientationComboBox);
        Controls.Add(borderlessCheckBox);
        Controls.Add(customHeightTextBox);
        Controls.Add(customWidthTextBox);
        Controls.Add(layoutComboBox);
        Controls.Add(paperTypeComboBox);
        Controls.Add(paperSourceComboBox);
        Controls.Add(paperSizeComboBox);
        Controls.Add(printerCheckedListBox);
        Name = "Form1";
        Text = "Photo Printer";
        Load += Form1_Load;
        ((System.ComponentModel.ISupportInitialize)previewPictureBox).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private CheckedListBox printerCheckedListBox;
    private ComboBox paperSizeComboBox;
    private ComboBox paperSourceComboBox;
    private ComboBox paperTypeComboBox;
    private ComboBox layoutComboBox;
    private TextBox customWidthTextBox;
    private TextBox customHeightTextBox;
    private CheckBox borderlessCheckBox;
    private ComboBox orientationComboBox;
    private Button selectPhotoButton;
    private Button printButton;
    private PictureBox previewPictureBox;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label label5;
    private Label label6;
    private Label label7;
    private Label label8;
    private TextBox logTextBox;
    private TextBox eventCodeTextBox;
    private Button startPrintsButton;
    private Button cancelButton;
    private Label label9;
    private Label label10;
    private ComboBox printQualityComboBox;
    private Label label11;
}
