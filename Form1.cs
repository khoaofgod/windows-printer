using System.Drawing.Printing;
using System.Text.Json;
using System.Collections.Concurrent;

namespace PhotoPrinter;

public class PrintFile
{
    public string file_name { get; set; } = "";
    public string download_link { get; set; } = "";
}

public class AppSettings
{
    public string? PaperSize { get; set; }
    public string? CustomWidth { get; set; }
    public string? CustomHeight { get; set; }
    public string? PaperSource { get; set; }
    public string? PaperType { get; set; }
    public string? Layout { get; set; }
    public string? PrintQuality { get; set; }
    public string? Orientation { get; set; }
    public bool Borderless { get; set; }
    public string? PrinterPreset { get; set; }
    public string? TopMargin { get; set; }
    public string? BottomMargin { get; set; }
    public string? LeftMargin { get; set; }
    public string? RightMargin { get; set; }
}

public partial class Form1 : Form
{
    private string? selectedImagePath;
    private Image? imageToPrint;
    private int currentPrinterIndex = 0;

    // Auto-print fields
    private CancellationTokenSource? autoPrintCancellation;
    private readonly ConcurrentQueue<string> logQueue = new();
    private readonly HttpClient httpClient = new();
    private const int MaxLogLines = 500;
    private string tmpFolder = "";
    private string readyPrintsFolder = "";

    // Store custom printer settings when using preset
    private PageSettings? savedPresetPageSettings = null;
    private System.Drawing.Printing.PrinterSettings? savedPresetPrinterSettings = null;

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        LoadPrinters();
        LoadPaperSizes();
        LoadPaperSources();
        LoadPaperTypes();
        LoadLayouts();
        LoadPrintQuality();
        LoadOrientation();
        InitializeAutoPrint();
    }

    private void InitializeAutoPrint()
    {
        // Create tmp and ready_prints folders
        string appPath = AppDomain.CurrentDomain.BaseDirectory;
        tmpFolder = Path.Combine(appPath, "tmp");
        readyPrintsFolder = Path.Combine(appPath, "ready_prints");

        Directory.CreateDirectory(tmpFolder);
        Directory.CreateDirectory(readyPrintsFolder);

        AddLog($"Initialized folders: tmp={tmpFolder}, ready_prints={readyPrintsFolder}");
    }

    private void LoadPrinters()
    {
        printerCheckedListBox.Items.Clear();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printerCheckedListBox.Items.Add(printer);
        }
    }

    private void DebugPrinterSizes()
    {
        // Get the first checked printer
        if (printerCheckedListBox.CheckedItems.Count == 0)
        {
            MessageBox.Show("Please select a printer first.", "No Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string printerName = printerCheckedListBox.CheckedItems[0]?.ToString()!;
        System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
        ps.PrinterName = printerName;

        string sizes = $"Available paper sizes for {printerName}:\n\n";
        foreach (PaperSize size in ps.PaperSizes)
        {
            float widthInches = size.Width / 100.0f;
            float heightInches = size.Height / 100.0f;
            sizes += $"{size.PaperName}: {widthInches:F2} x {heightInches:F2} in\n";
        }

        MessageBox.Show(sizes, "Printer Paper Sizes", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void PrinterCheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // Reset round-robin counter when printer selection changes
        currentPrinterIndex = 0;
    }

    private void LoadPaperSizes()
    {
        paperSizeComboBox.Items.Clear();
        paperSizeComboBox.Items.Add("Custom Size");

        // Add common paper sizes
        var commonSizes = new[]
        {
            "Letter (8.5 x 11 in)",
            "2.5 x 7 in",
            "3.5 x 5 in",
            "4 x 6 in",
            "5 x 7 in",
            "8 x 10 in",
            "A4 (210 x 297 mm)",
            "13 x 19 in"
        };

        foreach (var size in commonSizes)
        {
            paperSizeComboBox.Items.Add(size);
        }

        if (paperSizeComboBox.Items.Count > 1)
        {
            paperSizeComboBox.SelectedIndex = 1; // Select first standard size
        }
    }

    private void LoadPaperSources()
    {
        paperSourceComboBox.Items.Clear();

        // Try to get paper sources from the first checked printer or default printer
        try
        {
            string? printerName = null;

            // Try to get first checked printer
            if (printerCheckedListBox.CheckedItems.Count > 0)
            {
                printerName = printerCheckedListBox.CheckedItems[0]?.ToString();
            }

            // Fall back to default printer
            if (string.IsNullOrEmpty(printerName))
            {
                System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                printerName = ps.PrinterName;
            }

            if (!string.IsNullOrEmpty(printerName))
            {
                System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                printerSettings.PrinterName = printerName;

                foreach (PaperSource source in printerSettings.PaperSources)
                {
                    paperSourceComboBox.Items.Add(source.SourceName);
                }
            }
        }
        catch
        {
            // Fall back to generic list if enumeration fails
        }

        // If no sources were loaded, use generic list
        if (paperSourceComboBox.Items.Count == 0)
        {
            var sources = new[] { "Auto Select", "Cassette 1", "Cassette 2", "Rear Paper Feeder" };
            foreach (var source in sources)
            {
                paperSourceComboBox.Items.Add(source);
            }
        }

        if (paperSourceComboBox.Items.Count > 0)
        {
            paperSourceComboBox.SelectedIndex = 0;
        }
    }

    private void LoadPaperTypes()
    {
        paperTypeComboBox.Items.Clear();

        // Common paper types for photo printing
        var paperTypes = new[]
        {
            "Plain Paper",
            "Photo Paper",
            "Premium Photo Paper",
            "Glossy Photo Paper",
            "Premium Glossy",
            "Matte Photo Paper",
            "Premium Matte",
            "Premium Semigloss",
            "Ultra Glossy Photo Paper",
            "Ultra Premium Photo Paper Luster"
        };

        foreach (var type in paperTypes)
        {
            paperTypeComboBox.Items.Add(type);
        }

        if (paperTypeComboBox.Items.Count > 0)
        {
            paperTypeComboBox.SelectedIndex = 1; // Default to "Photo Paper"
        }
    }

    private void LoadLayouts()
    {
        layoutComboBox.Items.Clear();

        var layouts = new[]
        {
            "Full Page Photo (No Cropping)",
            "Fill Page (Crop to Fit)",
            "Actual Size (No Scaling)",
            "Document (With Margins)"
        };

        foreach (var layout in layouts)
        {
            layoutComboBox.Items.Add(layout);
        }

        if (layoutComboBox.Items.Count > 0)
        {
            layoutComboBox.SelectedIndex = 0; // Default to "Full Page Photo (No Cropping)"
        }
    }

    private void LoadPrintQuality()
    {
        printQualityComboBox.Items.Clear();

        // Add print quality options from PrinterResolutionKind enum
        printQualityComboBox.Items.Add("Draft");
        printQualityComboBox.Items.Add("Low");
        printQualityComboBox.Items.Add("Medium (Normal)");
        printQualityComboBox.Items.Add("High");

        // Default to High quality
        printQualityComboBox.SelectedIndex = 3;
    }

    private void LoadOrientation()
    {
        orientationComboBox.Items.Clear();

        orientationComboBox.Items.Add("Auto (Match Image)");
        orientationComboBox.Items.Add("Portrait");
        orientationComboBox.Items.Add("Landscape");

        // Default to Auto
        orientationComboBox.SelectedIndex = 0;
    }

    private void PaperSizeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        bool isCustom = paperSizeComboBox.SelectedIndex == 0;
        customWidthTextBox.Enabled = isCustom;
        customHeightTextBox.Enabled = isCustom;
    }

    private void ConfigurePresetButton_Click(object? sender, EventArgs e)
    {
        // Get the first checked printer
        if (printerCheckedListBox.CheckedItems.Count == 0)
        {
            MessageBox.Show("Please select a printer first.", "No Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string printerName = printerCheckedListBox.CheckedItems[0]?.ToString()!;

        // Create a PrintDialog to let user configure printer settings
        using PrintDialog printDialog = new PrintDialog();
        printDialog.PrinterSettings = new System.Drawing.Printing.PrinterSettings();
        printDialog.PrinterSettings.PrinterName = printerName;
        printDialog.AllowSomePages = false;
        printDialog.AllowSelection = false;
        printDialog.AllowCurrentPage = false;

        // Show the print dialog so user can select preset and configure settings
        if (printDialog.ShowDialog() == DialogResult.OK)
        {
            // Save the configured settings
            savedPresetPrinterSettings = printDialog.PrinterSettings;
            savedPresetPageSettings = printDialog.PrinterSettings.DefaultPageSettings;

            string presetName = printerPresetTextBox.Text;
            if (string.IsNullOrWhiteSpace(presetName))
            {
                printerPresetTextBox.Text = "Custom Settings Saved";
            }

            MessageBox.Show($"Printer settings saved! These settings will be used when printing.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SaveSettingsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Save Settings",
            DefaultExt = "json",
            FileName = "printer_settings.json"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var settings = new AppSettings
                {
                    PaperSize = paperSizeComboBox.SelectedItem?.ToString(),
                    CustomWidth = customWidthTextBox.Text,
                    CustomHeight = customHeightTextBox.Text,
                    PaperSource = paperSourceComboBox.SelectedItem?.ToString(),
                    PaperType = paperTypeComboBox.SelectedItem?.ToString(),
                    Layout = layoutComboBox.SelectedItem?.ToString(),
                    PrintQuality = printQualityComboBox.SelectedItem?.ToString(),
                    Orientation = orientationComboBox.SelectedItem?.ToString(),
                    Borderless = borderlessCheckBox.Checked,
                    PrinterPreset = printerPresetTextBox.Text,
                    TopMargin = topMarginTextBox.Text,
                    BottomMargin = bottomMarginTextBox.Text,
                    LeftMargin = leftMarginTextBox.Text,
                    RightMargin = rightMarginTextBox.Text
                };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFileDialog.FileName, json);

                MessageBox.Show("Settings saved successfully!", "Save Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void LoadSettingsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Load Settings"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    // Apply settings to UI controls (except printer list)
                    if (!string.IsNullOrEmpty(settings.PaperSize))
                    {
                        int index = paperSizeComboBox.Items.IndexOf(settings.PaperSize);
                        if (index >= 0) paperSizeComboBox.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(settings.CustomWidth))
                        customWidthTextBox.Text = settings.CustomWidth;

                    if (!string.IsNullOrEmpty(settings.CustomHeight))
                        customHeightTextBox.Text = settings.CustomHeight;

                    if (!string.IsNullOrEmpty(settings.PaperSource))
                    {
                        int index = paperSourceComboBox.Items.IndexOf(settings.PaperSource);
                        if (index >= 0) paperSourceComboBox.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(settings.PaperType))
                    {
                        int index = paperTypeComboBox.Items.IndexOf(settings.PaperType);
                        if (index >= 0) paperTypeComboBox.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(settings.Layout))
                    {
                        int index = layoutComboBox.Items.IndexOf(settings.Layout);
                        if (index >= 0) layoutComboBox.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(settings.PrintQuality))
                    {
                        int index = printQualityComboBox.Items.IndexOf(settings.PrintQuality);
                        if (index >= 0) printQualityComboBox.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(settings.Orientation))
                    {
                        int index = orientationComboBox.Items.IndexOf(settings.Orientation);
                        if (index >= 0) orientationComboBox.SelectedIndex = index;
                    }

                    borderlessCheckBox.Checked = settings.Borderless;

                    if (!string.IsNullOrEmpty(settings.PrinterPreset))
                        printerPresetTextBox.Text = settings.PrinterPreset;

                    if (!string.IsNullOrEmpty(settings.TopMargin))
                        topMarginTextBox.Text = settings.TopMargin;

                    if (!string.IsNullOrEmpty(settings.BottomMargin))
                        bottomMarginTextBox.Text = settings.BottomMargin;

                    if (!string.IsNullOrEmpty(settings.LeftMargin))
                        leftMarginTextBox.Text = settings.LeftMargin;

                    if (!string.IsNullOrEmpty(settings.RightMargin))
                        rightMarginTextBox.Text = settings.RightMargin;

                    MessageBox.Show("Settings loaded successfully!", "Load Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SelectPhotoButton_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff",
            Title = "Select a Photo"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            selectedImagePath = openFileDialog.FileName;

            // Load and display preview
            if (previewPictureBox.Image != null)
            {
                previewPictureBox.Image.Dispose();
            }

            previewPictureBox.Image = Image.FromFile(selectedImagePath);
            label5.Text = $"Selected: {Path.GetFileName(selectedImagePath)}";
            printButton.Enabled = true;
        }
    }

    private void PrintButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedImagePath))
        {
            MessageBox.Show("Please select a photo first.", "No Photo Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Get checked printers
        var checkedPrinters = new List<string>();
        foreach (var item in printerCheckedListBox.CheckedItems)
        {
            checkedPrinters.Add(item.ToString()!);
        }

        if (checkedPrinters.Count == 0)
        {
            MessageBox.Show("Please select at least one printer.", "No Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Load image for printing
            if (imageToPrint != null)
            {
                imageToPrint.Dispose();
            }
            imageToPrint = Image.FromFile(selectedImagePath);

            // Round-robin print to all checked printers
            string nextPrinter = checkedPrinters[currentPrinterIndex % checkedPrinters.Count];
            currentPrinterIndex++;

            PrintToSpecificPrinter(nextPrinter);

            label4.Text = $"Printed to: {nextPrinter}";
            label4.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            label4.Text = "Print failed";
            label4.ForeColor = Color.Red;
        }
    }

    private void PrintToSpecificPrinter(string printerName)
    {
        using PrintDocument printDoc = new PrintDocument();
        printDoc.PrinterSettings.PrinterName = printerName;
        printDoc.PrintPage += PrintDocument_PrintPage;

        // Check if using saved printer preset/settings
        bool usePreset = !string.IsNullOrWhiteSpace(printerPresetTextBox.Text) && savedPresetPrinterSettings != null;

        // Get orientation setting (used in both preset and manual modes)
        int orientationIndex = orientationComboBox.SelectedIndex;

        if (usePreset)
        {
            // Use the saved preset settings configured via "Configure..." button
            // ALL app settings (orientation, paper size, layout, etc.) are IGNORED
            System.Diagnostics.Debug.WriteLine($"Using saved printer preset/settings: {printerPresetTextBox.Text}");
            System.Diagnostics.Debug.WriteLine("Ignoring all manual settings - using preset configuration only");

            // Copy all settings from the saved preset
            if (savedPresetPrinterSettings != null)
            {
                printDoc.PrinterSettings = savedPresetPrinterSettings;
            }

            if (savedPresetPageSettings != null)
            {
                printDoc.DefaultPageSettings = savedPresetPageSettings;
            }

            // Apply Orientation if specified
            if (orientationIndex == 0) // Auto - calculate based on image dimensions
            {
                if (imageToPrint != null)
                {
                    // If image width > height, use landscape
                    bool imageIsWide = imageToPrint.Width > imageToPrint.Height;
                    printDoc.DefaultPageSettings.Landscape = imageIsWide;
                    System.Diagnostics.Debug.WriteLine($"Auto orientation with preset: {(imageIsWide ? "Landscape" : "Portrait")} (image {imageToPrint.Width}x{imageToPrint.Height})");
                }
            }
            else if (orientationIndex == 1) // Portrait
            {
                printDoc.DefaultPageSettings.Landscape = false;
            }
            else if (orientationIndex == 2) // Landscape
            {
                printDoc.DefaultPageSettings.Landscape = true;
            }

            // Apply Print Quality if specified
            if (printQualityComboBox.SelectedItem != null)
            {
                string quality = printQualityComboBox.SelectedItem.ToString()!;
                PrinterResolutionKind resolutionKind = quality switch
                {
                    "Draft" => PrinterResolutionKind.Draft,
                    "Low" => PrinterResolutionKind.Low,
                    "Medium" => PrinterResolutionKind.Medium,
                    "High" => PrinterResolutionKind.High,
                    _ => PrinterResolutionKind.High
                };

                // Find and set matching printer resolution
                foreach (PrinterResolution resolution in printDoc.PrinterSettings.PrinterResolutions)
                {
                    if (resolution.Kind == resolutionKind)
                    {
                        printDoc.DefaultPageSettings.PrinterResolution = resolution;
                        break;
                    }
                }
            }

            // Print directly with preset settings - skip all custom configuration below
            printDoc.Print();
            return;
        }

        // === Manual Settings Mode (only used when NO preset is configured) ===

        // Set orientation (orientationIndex already declared above)
        if (orientationIndex == 0) // Auto - match image aspect ratio
        {
            if (imageToPrint != null)
            {
                // If image is wider than tall, use landscape
                bool imageIsWide = imageToPrint.Width > imageToPrint.Height;
                printDoc.DefaultPageSettings.Landscape = imageIsWide;
                System.Diagnostics.Debug.WriteLine($"Auto orientation: {(imageIsWide ? "Landscape" : "Portrait")} (image {imageToPrint.Width}x{imageToPrint.Height})");
            }
        }
        else if (orientationIndex == 2) // Landscape
        {
            printDoc.DefaultPageSettings.Landscape = true;
        }
        // orientationIndex == 1 is Portrait (default, no need to set)

        // Handle paper size
        if (paperSizeComboBox.SelectedItem != null)
        {
            string selectedSize = paperSizeComboBox.SelectedItem.ToString()!;

            float width = 0, height = 0;

            if (selectedSize == "Custom Size")
            {
                // Use custom dimensions from text boxes
                if (float.TryParse(customWidthTextBox.Text, out width) &&
                    float.TryParse(customHeightTextBox.Text, out height))
                {
                    // Custom size will be set below
                }
            }
            else
            {
                // Parse predefined sizes
                if (selectedSize.StartsWith("Letter"))
                {
                    width = 8.5f; height = 11f;
                }
                else if (selectedSize.StartsWith("2.5 x 7"))
                {
                    width = 2.5f; height = 7f;
                }
                else if (selectedSize.StartsWith("3.5 x 5"))
                {
                    width = 3.5f; height = 5f;
                }
                else if (selectedSize.StartsWith("4 x 6"))
                {
                    width = 4f; height = 6f;
                }
                else if (selectedSize.StartsWith("5 x 7"))
                {
                    width = 5f; height = 7f;
                }
                else if (selectedSize.StartsWith("8 x 10"))
                {
                    width = 8f; height = 10f;
                }
                else if (selectedSize.StartsWith("13 x 19"))
                {
                    width = 13f; height = 19f;
                }
                else if (selectedSize.StartsWith("A4"))
                {
                    width = 8.27f; height = 11.69f; // A4 in inches
                }
            }

            if (width > 0 && height > 0)
            {
                // Try to find a matching paper size from the printer first
                PaperSize? matchingSize = null;
                foreach (PaperSize ps in printDoc.PrinterSettings.PaperSizes)
                {
                    // Convert to inches for comparison (PaperSize is in hundredths of inch)
                    float psWidth = ps.Width / 100.0f;
                    float psHeight = ps.Height / 100.0f;

                    // Check for exact match (within 0.1 inch tolerance)
                    if (Math.Abs(psWidth - width) < 0.1f && Math.Abs(psHeight - height) < 0.1f)
                    {
                        matchingSize = ps;
                        System.Diagnostics.Debug.WriteLine($"Found matching printer paper size: {ps.PaperName} ({ps.Width}x{ps.Height})");
                        break;
                    }
                }

                if (matchingSize != null)
                {
                    // Use the printer's native paper size
                    printDoc.DefaultPageSettings.PaperSize = matchingSize;
                }
                else
                {
                    // Create custom paper size
                    PaperSize customSize = new PaperSize(selectedSize, (int)(width * 100), (int)(height * 100));
                    printDoc.DefaultPageSettings.PaperSize = customSize;
                    System.Diagnostics.Debug.WriteLine($"Using custom paper size: {width}x{height} inches");
                }
            }
        }

        // Set paper source
        if (paperSourceComboBox.SelectedItem != null)
        {
            string selectedSourceName = paperSourceComboBox.SelectedItem.ToString()!;

            // Find matching paper source from printer
            foreach (PaperSource source in printDoc.PrinterSettings.PaperSources)
            {
                if (source.SourceName == selectedSourceName)
                {
                    printDoc.DefaultPageSettings.PaperSource = source;
                    break;
                }
            }
        }

        // Set print quality
        if (printQualityComboBox.SelectedItem != null)
        {
            string quality = printQualityComboBox.SelectedItem.ToString()!;
            PrinterResolutionKind resolutionKind = quality switch
            {
                "Draft" => PrinterResolutionKind.Draft,
                "Low" => PrinterResolutionKind.Low,
                "Medium (Normal)" => PrinterResolutionKind.Medium,
                "High" => PrinterResolutionKind.High,
                _ => PrinterResolutionKind.High
            };

            // Find and set matching printer resolution
            foreach (PrinterResolution resolution in printDoc.PrinterSettings.PrinterResolutions)
            {
                if (resolution.Kind == resolutionKind)
                {
                    printDoc.DefaultPageSettings.PrinterResolution = resolution;
                    break;
                }
            }
        }

        // Set margins using custom margin values
        int topMargin = 0, bottomMargin = 0, leftMargin = 0, rightMargin = 0;

        // Parse custom margin values
        if (int.TryParse(topMarginTextBox.Text, out int parsedTop))
            topMargin = parsedTop;
        if (int.TryParse(bottomMarginTextBox.Text, out int parsedBottom))
            bottomMargin = parsedBottom;
        if (int.TryParse(leftMarginTextBox.Text, out int parsedLeft))
            leftMargin = parsedLeft;
        if (int.TryParse(rightMarginTextBox.Text, out int parsedRight))
            rightMargin = parsedRight;

        // Apply custom margins
        printDoc.DefaultPageSettings.Margins = new Margins(leftMargin, rightMargin, topMargin, bottomMargin);

        // Always use OriginAtMargins=false for full control
        printDoc.OriginAtMargins = false;

        System.Diagnostics.Debug.WriteLine($"Custom Margins - Top: {topMargin}, Bottom: {bottomMargin}, Left: {leftMargin}, Right: {rightMargin}");

        printDoc.Print();
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        if (imageToPrint == null || e.Graphics == null)
            return;

        // Debug logging
        System.Diagnostics.Debug.WriteLine($"PageBounds: {e.PageBounds.Width}x{e.PageBounds.Height}");
        System.Diagnostics.Debug.WriteLine($"MarginBounds: {e.MarginBounds.Width}x{e.MarginBounds.Height}");
        System.Diagnostics.Debug.WriteLine($"PageSettings.PaperSize: {e.PageSettings.PaperSize.Width}x{e.PageSettings.PaperSize.Height}");
        System.Diagnostics.Debug.WriteLine($"Image Size: {imageToPrint.Width}x{imageToPrint.Height}");
        System.Diagnostics.Debug.WriteLine($"Borderless: {borderlessCheckBox.Checked}");

        // Get custom margins from text boxes
        int topMargin = 0, bottomMargin = 0, leftMargin = 0, rightMargin = 0;
        if (int.TryParse(topMarginTextBox.Text, out int parsedTop))
            topMargin = parsedTop;
        if (int.TryParse(bottomMarginTextBox.Text, out int parsedBottom))
            bottomMargin = parsedBottom;
        if (int.TryParse(leftMarginTextBox.Text, out int parsedLeft))
            leftMargin = parsedLeft;
        if (int.TryParse(rightMarginTextBox.Text, out int parsedRight))
            rightMargin = parsedRight;

        Rectangle bounds;
        int layoutIndex = layoutComboBox.SelectedIndex;

        switch (layoutIndex)
        {
            case 0: // Full Page Photo (No Cropping)
                // Use PageBounds and apply custom margins
                bounds = e.PageBounds;
                bounds = new Rectangle(
                    bounds.Left + leftMargin,
                    bounds.Top + topMargin,
                    bounds.Width - (leftMargin + rightMargin),
                    bounds.Height - (topMargin + bottomMargin)
                );
                DrawFitToPage(e.Graphics, bounds);
                break;

            case 1: // Fill Page (Crop to Fit)
                // Use PageBounds and apply custom margins
                bounds = e.PageBounds;
                bounds = new Rectangle(
                    bounds.Left + leftMargin,
                    bounds.Top + topMargin,
                    bounds.Width - (leftMargin + rightMargin),
                    bounds.Height - (topMargin + bottomMargin)
                );
                DrawFullPage(e.Graphics, bounds);
                break;

            case 2: // Actual Size (No Scaling)
                bounds = e.MarginBounds;
                DrawActualSize(e.Graphics, bounds);
                break;

            case 3: // Document (With Margins)
                bounds = e.MarginBounds;
                DrawDocument(e.Graphics, bounds);
                break;

            default:
                bounds = e.PageBounds;
                DrawFitToPage(e.Graphics, bounds);
                break;
        }

        e.HasMorePages = false;
    }

    private void DrawFullPage(Graphics graphics, Rectangle bounds)
    {
        // Fill entire page, may crop image to fit
        float imageAspect = (float)imageToPrint!.Width / imageToPrint.Height;
        float pageAspect = (float)bounds.Width / bounds.Height;

        int destWidth, destHeight;
        int destX, destY;

        if (imageAspect > pageAspect)
        {
            // Image is wider - fit to height and crop sides
            destHeight = bounds.Height;
            destWidth = (int)(bounds.Height * imageAspect);
            destX = bounds.Left - (destWidth - bounds.Width) / 2;
            destY = bounds.Top;
        }
        else
        {
            // Image is taller - fit to width and crop top/bottom
            destWidth = bounds.Width;
            destHeight = (int)(bounds.Width / imageAspect);
            destX = bounds.Left;
            destY = bounds.Top - (destHeight - bounds.Height) / 2;
        }

        graphics.DrawImage(imageToPrint, destX, destY, destWidth, destHeight);
    }

    private void DrawFitToPage(Graphics graphics, Rectangle bounds)
    {
        // Fit image to page while maintaining aspect ratio
        float imageAspect = (float)imageToPrint!.Width / imageToPrint.Height;
        float pageAspect = (float)bounds.Width / bounds.Height;

        int destWidth, destHeight;
        int destX, destY;

        if (imageAspect > pageAspect)
        {
            // Image is wider than page - fit to width
            destWidth = bounds.Width;
            destHeight = (int)(bounds.Width / imageAspect);
            destX = bounds.Left;
            destY = bounds.Top + (bounds.Height - destHeight) / 2;
        }
        else
        {
            // Image is taller than page - fit to height
            destHeight = bounds.Height;
            destWidth = (int)(bounds.Height * imageAspect);
            destX = bounds.Left + (bounds.Width - destWidth) / 2;
            destY = bounds.Top;
        }

        graphics.DrawImage(imageToPrint, destX, destY, destWidth, destHeight);
    }

    private void DrawActualSize(Graphics graphics, Rectangle bounds)
    {
        // Print at actual size (96 DPI), centered
        int destWidth = imageToPrint!.Width;
        int destHeight = imageToPrint.Height;

        // Center on page
        int destX = bounds.Left + (bounds.Width - destWidth) / 2;
        int destY = bounds.Top + (bounds.Height - destHeight) / 2;

        // Clip if larger than page
        if (destWidth > bounds.Width || destHeight > bounds.Height)
        {
            destX = bounds.Left;
            destY = bounds.Top;
        }

        graphics.DrawImage(imageToPrint, destX, destY, destWidth, destHeight);
    }

    private void DrawDocument(Graphics graphics, Rectangle bounds)
    {
        // Print like a document with reasonable margins
        int margin = 50; // Additional margin for document style
        Rectangle docBounds = new Rectangle(
            bounds.Left + margin,
            bounds.Top + margin,
            bounds.Width - (margin * 2),
            bounds.Height - (margin * 2)
        );

        DrawFitToPage(graphics, docBounds);
    }

    // Auto-print methods
    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";

        if (logTextBox.InvokeRequired)
        {
            logTextBox.Invoke(() => AddLogToTextBox(logMessage));
        }
        else
        {
            AddLogToTextBox(logMessage);
        }
    }

    private void AddLogToTextBox(string message)
    {
        logTextBox.AppendText(message + Environment.NewLine);

        // Keep only last 500 lines
        var lines = logTextBox.Lines;
        if (lines.Length > MaxLogLines)
        {
            var keepLines = lines.Skip(lines.Length - MaxLogLines).ToArray();
            logTextBox.Lines = keepLines;
        }

        // Auto-scroll to bottom
        logTextBox.SelectionStart = logTextBox.Text.Length;
        logTextBox.ScrollToCaret();
    }

    private async void StartPrintsButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(eventCodeTextBox.Text))
        {
            MessageBox.Show("Please enter an Event Code.", "Event Code Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var checkedPrinters = new List<string>();
        foreach (var item in printerCheckedListBox.CheckedItems)
        {
            checkedPrinters.Add(item.ToString()!);
        }

        if (checkedPrinters.Count == 0)
        {
            MessageBox.Show("Please select at least one printer.", "No Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        autoPrintCancellation = new CancellationTokenSource();

        startPrintsButton.Enabled = false;
        cancelButton.Enabled = true;
        eventCodeTextBox.Enabled = false;

        AddLog($"Starting auto-print for event code: {eventCodeTextBox.Text}");

        try
        {
            await RunAutoPrintLoop(eventCodeTextBox.Text, autoPrintCancellation.Token);
        }
        catch (Exception ex)
        {
            AddLog($"Error in auto-print: {ex.Message}");
        }
        finally
        {
            startPrintsButton.Enabled = true;
            cancelButton.Enabled = false;
            eventCodeTextBox.Enabled = true;
            AddLog("Auto-print stopped.");
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        AddLog("Canceling auto-print...");
        autoPrintCancellation?.Cancel();
        cancelButton.Enabled = false;
    }

    private async Task RunAutoPrintLoop(string eventCode, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                string apiUrl = $"https://togetherbooth.com/prints/{eventCode}";
                AddLog($"Fetching URL: {apiUrl}");

                var response = await httpClient.GetAsync(apiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var printFiles = JsonSerializer.Deserialize<List<PrintFile>>(jsonContent);

                if (printFiles == null || printFiles.Count == 0)
                {
                    AddLog("No files to download. Waiting 1 second...");
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                AddLog($"Found {printFiles.Count} files to process");

                // Download all files to tmp
                foreach (var file in printFiles)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    await DownloadFile(file, cancellationToken);
                }

                // Move all files from tmp to ready_prints
                foreach (var file in printFiles)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    MoveFileToReadyPrints(file);
                }

                // Verify tmp is empty
                var tmpFiles = Directory.GetFiles(tmpFolder);
                if (tmpFiles.Length == 0)
                {
                    AddLog("All files moved to ready_prints. Starting print jobs...");

                    // Print all files
                    foreach (var file in printFiles)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        await PrintFileFromReadyPrints(file, cancellationToken);
                    }
                }
                else
                {
                    AddLog($"Warning: {tmpFiles.Length} files still in tmp folder");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                AddLog($"Error in fetch loop: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task DownloadFile(PrintFile file, CancellationToken cancellationToken)
    {
        try
        {
            string tmpPath = Path.Combine(tmpFolder, file.file_name);
            AddLog($"Downloading: {file.file_name} from {file.download_link}");

            var response = await httpClient.GetAsync(file.download_link, cancellationToken);
            response.EnsureSuccessStatusCode();

            var fileBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(tmpPath, fileBytes, cancellationToken);

            AddLog($"Downloaded to tmp: {file.file_name} ({fileBytes.Length} bytes)");
        }
        catch (Exception ex)
        {
            AddLog($"Error downloading {file.file_name}: {ex.Message}");
        }
    }

    private void MoveFileToReadyPrints(PrintFile file)
    {
        try
        {
            string tmpPath = Path.Combine(tmpFolder, file.file_name);
            string readyPath = Path.Combine(readyPrintsFolder, file.file_name);

            if (File.Exists(tmpPath))
            {
                // If file exists in ready_prints, delete it first
                if (File.Exists(readyPath))
                {
                    File.Delete(readyPath);
                }

                File.Move(tmpPath, readyPath);
                AddLog($"Moved to ready_prints: {file.file_name}");
            }
            else
            {
                AddLog($"File not found in tmp: {file.file_name}");
            }
        }
        catch (Exception ex)
        {
            AddLog($"Error moving {file.file_name}: {ex.Message}");
        }
    }

    private async Task PrintFileFromReadyPrints(PrintFile file, CancellationToken cancellationToken)
    {
        try
        {
            string readyPath = Path.Combine(readyPrintsFolder, file.file_name);

            if (!File.Exists(readyPath))
            {
                AddLog($"File not found for printing: {file.file_name}");
                return;
            }

            // Get checked printers
            var checkedPrinters = new List<string>();
            foreach (var item in printerCheckedListBox.CheckedItems)
            {
                checkedPrinters.Add(item.ToString()!);
            }

            if (checkedPrinters.Count == 0)
            {
                AddLog("No printers selected. Skipping print.");
                return;
            }

            // Round-robin printer selection
            string nextPrinter = checkedPrinters[currentPrinterIndex % checkedPrinters.Count];
            currentPrinterIndex++;

            AddLog($"Printing {file.file_name} to {nextPrinter}...");

            // Load and print the image
            using (var printImage = Image.FromFile(readyPath))
            {
                imageToPrint = printImage;

                await Task.Run(() =>
                {
                    PrintToSpecificPrinter(nextPrinter);
                }, cancellationToken);

                imageToPrint = null;
            }

            AddLog($"Printed successfully: {file.file_name} on {nextPrinter}");

            // Delete the file after successful print
            File.Delete(readyPath);
            AddLog($"Deleted from ready_prints: {file.file_name}");
        }
        catch (Exception ex)
        {
            AddLog($"Error printing {file.file_name}: {ex.Message}");
        }
    }

}
