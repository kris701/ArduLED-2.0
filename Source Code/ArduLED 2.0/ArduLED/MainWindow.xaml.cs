using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using ArduLED_Serial_Protocol;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace ArduLED
{
    #region StructsAndEnums

    public struct SetupStrip
    {
        public int ID;
        public Point Location;
        public int LEDsPrStrip;
        public int FromID;
        public int PinID;
        public FlipDir FlipDir;
        public object ConnectLineObject;
        public object ConnectLineArrowObject;
        public int DragingFrom;
        public int ConnectedToID;
        public int ConnectedFrom;
        public int ConnectedFromID;

        public SetupStrip(int _ID, int _ConnectedToID, int _ConnectedFromID, int _ConnectedFrom, int _DragingFrom, Point _Location, int _LEDsPrStrip, int _FromID, int _PinID, FlipDir _FlipDir, object _ConnectLineObject, object _ConnectLineArrowObject)
        {
            ID = _ID;
            ConnectedToID = _ConnectedToID;
            ConnectedFromID = _ConnectedFromID;
            ConnectedFrom = _ConnectedFrom;
            DragingFrom = _DragingFrom;
            Location = _Location;
            LEDsPrStrip = _LEDsPrStrip;
            FromID = _FromID;
            PinID = _PinID;
            FlipDir = _FlipDir;
            ConnectLineObject = _ConnectLineObject;
            ConnectLineArrowObject = _ConnectLineArrowObject;
        }
    }

    public struct TransferDevice
    {
        public string DeviceName;
        public bool IsWireless;
        public IPAddress IPAddress;
        public int Port;
        public string COMPortName;
        public int BaudRate;
        public object SourceGrid;
        public ArduLEDSerialProtocol Device;
        public string SetupSaveFileName;
        public int TotalLEDCount;

        public TransferDevice(string _DeviceName, object _SourceGrid, bool _IsWireless, IPAddress _IPAddress, int _Port, string _COMPortName, int _BaudRate, ArduLEDSerialProtocol _Device, string _SetupSaveFileName, int _TotalLEDCount)
        {
            DeviceName = _DeviceName;
            SourceGrid = _SourceGrid;
            IsWireless = _IsWireless;
            IPAddress = _IPAddress;
            Port = _Port;
            COMPortName = _COMPortName;
            BaudRate = _BaudRate;
            Device = _Device;
            SetupSaveFileName = _SetupSaveFileName;
            TotalLEDCount = _TotalLEDCount;
        }
    }

    public struct AmbilightSide
    {
        public bool Enabled;
        public int Width;
        public int Height;
        public int BlockSpacing;
        public int XOffSet;
        public int YOffSet;
        public int FromID;
        public int ToID;
        public int LEDsPrBlock;

        public AmbilightSide(bool _Enabled, int _Width, int _Height, int _BlockSpacing, int _XOffSet, int _YOffSet, int _FromID, int _ToID, int _LEDsPrBlock)
        {
            Enabled = _Enabled;
            Width = _Width;
            Height = _Height;
            BlockSpacing = _BlockSpacing;
            XOffSet = _XOffSet;
            YOffSet = _YOffSet;
            FromID = _FromID;
            ToID = _ToID;
            LEDsPrBlock = _LEDsPrBlock;
        }
    }

    public struct FromToIDS
    {
        public int FromID;
        public int ToID;

        public FromToIDS(int _FromID, int _ToID)
        {
            FromID = _FromID;
            ToID = _ToID;
        }
    }

    public enum FlipDir { Up, Right, Down, Left };

    enum SideID { Left, Top, Right, Bottom };

    #endregion

    public partial class MainWindow : Window
    {

        #region Vars

        Point DragPoint = new Point();
        bool AutoSendSetupAtStartup = false;
        System.Windows.Forms.SaveFileDialog SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
        System.Windows.Forms.OpenFileDialog LoadFileDialog = new System.Windows.Forms.OpenFileDialog();

        bool SetupDraging = false;
        Point SetupDragPoint = new Point();
        object SetupDragingObject;
        static int ButtonHeight = 25;
        static int ButtonWidth = 25;
        static int IOShapeSize = 15;
        bool SetupLineDraging = false;
        object SetupLineDragingObject;
        object SetupIODragingObject;
        int SetupLineDragingArrowWidth = 10;
        int IDCount = 1;
        List<int> StripIDCount = new List<int>(new int[50]);
        List<TransferDevice> DeviceList = new List<TransferDevice>();
        bool RunVisualizer = false;
        bool VisualizerStopped = true;
        private WASAPIPROC BassProcess;
        int VisualizerHighestRPS = 0;
        int VisualizerLowestRPS =9999;
        bool SpectrumCrash = false;
        List<double> VisualizerSpectrumRed = new List<double>();
        List<double> VisualizerSpectrumGreen = new List<double>();
        List<double> VisualizerSpectrumBlue = new List<double>();
        List<double> VisualizerBeatWaveRed = new List<double>();
        List<double> VisualizerBeatWaveGreen = new List<double>();
        List<double> VisualizerBeatWaveBlue = new List<double>();
        string VisualizerDeviceName = "";
        int VisualizerVisualRefreshTick = 0;
        int VisualizerVisualRefreshTickMax = 2;
        int CurrentFPSValue = 0;

        bool Draging = false;
        bool Maximized = false;

        bool ColorWheelCursorMoving = false;

        bool Loading = true;

        string CurrentMode = "";
        string CurrentSaveFileName = "";

        static string[] DefaultAdvancedValues = {
            "FADECOLORWHEEL;19,5;82,5",
            "SLIDER;FromToIDSliderFrom;0",
            "SLIDER;FromToIDSliderTo;4",
            "SLIDER;FadingGammaCorrectionSlider;2",
            "SLIDER;FadingFadeSpeedSlider;30",
            "SLIDER;FadeingFadeFactorSlider;0,1",
            "SLIDER;VisualizerVisualSamplesSlider;128",
            "SLIDER;VisualizerSensitivitySlider;3",
            "SLIDER;VisualizerSmoothnessSlider;1",
            "SLIDER;VisualizerSampleDelaySlider;10",
            "CHECKBOX;VisualizerBeatZoneAutoTriggerCheckBox;True",
            "SLIDER;VisualizerBeatZoneAutoTriggerSlider;52",
            "TEXTBOX;VisualizerBeatZoneAutoTriggerMaxTextBox;255",
            "TEXTBOX;VisualizerBeatZoneAutoTriggerMinTextBox;10",
            "SLIDER;VisualizerBeatZoneFromValueSlider;0",
            "SLIDER;VisualizerBeatZoneToValueSlider;128",
            "TEXTBOX;VisualizerBeatZoneAutoTriggerIncresseTextBox;500",
            "TEXTBOX;VisualizerBeatZoneAutoTriggerDecreeseTextBox;50",
            "TEXTBOX;AmbilightMaximumFPSTextbox;30",
            "TEXTBOX;AmbilightBlockSampleSplitTextbox;25",
            "TEXTBOX;AmbilightGammaFactorTextbox;2",
            "TEXTBOX;AmbilightFadeFactorTextbox;0,25",
            "TEXTBOX;AmbilightAssumePrCentTextBox;50",
            "TEXTBOX;AmbilightMaxAssumeVariationTextBox;100",
            "TEXTBOX;AmbilightTopSideLEDsPrBlockTextbox;1",
            "TEXTBOX;AmbilightTopSideBlockSpacingTextbox;10",
            "TEXTBOX;AmbilightTopSideBlockWidthTextbox;100",
            "TEXTBOX;AmbilightTopSideBlockHeightTextbox;100",
            "TEXTBOX;AmbilightTopSideBlockOffsetXTextbox;0",
            "TEXTBOX;AmbilightTopSideBlockOffsetYTextbox;0",
            "TEXTBOX;AmbilightRightSideLEDsPrBlockTextbox;1",
            "TEXTBOX;AmbilightRightSideBlockSpacingTextbox;10",
            "TEXTBOX;AmbilightRightSideBlockWidthTextbox;100",
            "TEXTBOX;AmbilightRightSideBlockHeightTextbox;100",
            "TEXTBOX;AmbilightRightSideBlockOffsetXTextbox;0",
            "TEXTBOX;AmbilightRightSideBlockOffsetYTextbox;0",
            "TEXTBOX;AmbilightBottomSideLEDsPrBlockTextbox;1",
            "TEXTBOX;AmbilightBottomSideBlockSpacingTextbox;10",
            "TEXTBOX;AmbilightBottomSideBlockWidthTextbox;100",
            "TEXTBOX;AmbilightBottomSideBlockHeightTextbox;100",
            "TEXTBOX;AmbilightBottomSideBlockOffsetXTextbox;0",
            "TEXTBOX;AmbilightBottomSideBlockOffsetYTextbox;0",
            "TEXTBOX;AmbilightLeftSideLEDsPrBlockTextbox;1",
            "TEXTBOX;AmbilightLeftSideBlockSpacingTextbox;10",
            "TEXTBOX;AmbilightLeftSideBlockWidthTextbox;100",
            "TEXTBOX;AmbilightLeftSideBlockHeightTextbox;100",
            "TEXTBOX;AmbilightLeftSideBlockOffsetXTextbox;0",
            "TEXTBOX;AmbilightLeftSideBlockOffsetYTextbox;0",
            "COMBOBOX;AmbilightScreenIDCombobox;1"
        };

        List<Block> BlockList = new List<Block>();
        List<System.Drawing.Rectangle> ScreenList = new List<System.Drawing.Rectangle>();
        AmbilightSide LeftSide;
        AmbilightSide TopSide;
        AmbilightSide RightSide;
        AmbilightSide BottomSide;
        bool SelectingSide = false;
        bool CancelSelectingSide = false;
        Task AmbilightTask;
        private List<List<List<int>>> AmbilightColorStore = new List<List<List<int>>>();
        double AssumeLevel = 1;
        int MaxVariation = 765;
        bool RunAmbilight = false;
        DateTime AmbilightFPSCounter;
        int AmbilightFPSCounterFramesRendered;

        #endregion

        #region Other

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow1.Visibility = Visibility.Hidden;
            LoadingSplashScreen NewLoadingScreen = new LoadingSplashScreen();
            NewLoadingScreen.Show();

            for (double i = 0; i <= 1; i += 0.1)
            {
                NewLoadingScreen.Opacity = i;
                await Task.Delay(50);
            }

            NewLoadingScreen.ProgressLabel.Content = "Loading: Internals";

            HideAllInnerGrids();
            HideAllSideBars();
            HideAllSubMenus();

            SaveFileDialog.DefaultExt = ".txt";
            LoadFileDialog.DefaultExt = ".txt";

            NewLoadingScreen.ProgressLabel.Content = "Loading: Default Combobox Indexes";

            DeviceSelectionCombobox.Items.Add(" - All - ");
            ModesDeviceSelectionCombobox.Items.Add(" - All - ");

            VisualizerSelectVisualizationTypeCombobox.Items.Add("Beat");
            VisualizerSelectVisualizationTypeCombobox.Items.Add("Spectrum Beat");
            VisualizerSelectVisualizationTypeCombobox.Items.Add("Spectrum Wave");
            VisualizerSelectVisualizationTypeCombobox.Items.Add("Beat Wave");
            VisualizerSelectVisualizationTypeCombobox.Items.Add("\"Beat Wave\" Beat");
            VisualizerSelectVisualizationTypeCombobox.Items.Add("Full Spectrum");

            DeviceSelectionCombobox.SelectedIndex = 0;
            ModesDeviceSelectionCombobox.SelectedIndex = 0;

            NewLoadingScreen.ProgressLabel.Content = "Loading: BASS";

            VisualizerSelectDeviceCombobox.Items.Clear();
            int DeviceCount = BassWasapi.BASS_WASAPI_GetDeviceCount();
            for (int i = 0; i < DeviceCount; i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    VisualizerSelectDeviceCombobox.Items.Add(string.Format("{0} - {1}", i, device.name));
                }
                NewLoadingScreen.ProgressLabel.Content = "Loading: BASS " + i + " / " + DeviceCount;
            }

            NewLoadingScreen.ProgressLabel.Content = "Loading: Folder Tree";

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Setups"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Setups");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\DeviceConfigs"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\DeviceConfigs");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Ranges"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Ranges");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\ManualSaves"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ManualSaves");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\ManualSaves\\Visualizer"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ManualSaves\\Visualizer");

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\ManualSaves\\Ambilight"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\ManualSaves\\Ambilight");

            NewLoadingScreen.ProgressLabel.Content = "Loading: Combobox values";

            Update_DeviceSelectionCombobox_Items();

            Update_AmbilightScreenIDCombobox_Values();

            NewLoadingScreen.ProgressLabel.Content = "Loading: Previous Configuration";

            SettingsAdvancedSettingsCheckBox.IsChecked = false;

            LoadDeviceConfig();

            LoadApplicationConfigsForDevice(" - All - ");

            if ((bool)SettingsAdvancedSettingsCheckBox.IsChecked)
                LoadAdvancedApplicationConfigsForDevice(" - All - ");
            else
                ResetAllAdvancedSettingsToDefault();

            NewLoadingScreen.ProgressLabel.Content = "Loading: Ambilight Sides";

            SetSides();

            NewLoadingScreen.ProgressLabel.Content = "Loading: Visualizer Spectrums";

            UpdateSpectrums(
                VisualizerSpectrumRedTextbox.Text,
                VisualizerSpectrumGreenTextbox.Text,
                VisualizerSpectrumBlueTextbox.Text,
                (bool)VisualizerSpectrumAutoscaleValuesCheckbox.IsChecked,
                VisualizerSpectrumCanvas,
                VisualizerSpectrumCanvas.Width,
                VisualizerSpectrumCanvas.Height,
                (int)VisualizerVisualSamplesSlider.Value,
                VisualizerSpectrumRed,
                VisualizerSpectrumGreen,
                VisualizerSpectrumBlue
                );
            UpdateSpectrums(
                VisualizerBeatWaveRedTextbox.Text,
                VisualizerBeatWaveGreenTextbox.Text,
                VisualizerBeatWaveBlueTextbox.Text,
                (bool)VisualizerBeatWaveAutoscaleValuesCheckbox.IsChecked,
                VisualizerBeatWaveCanvas,
                VisualizerBeatWaveCanvas.Width,
                VisualizerBeatWaveCanvas.Height,
                255 * 3,
                VisualizerBeatWaveRed,
                VisualizerBeatWaveGreen,
                VisualizerBeatWaveBlue
                );

            NewLoadingScreen.ProgressLabel.Content = "Loading: Resetting Value Labels";

            FadingGammaCorrectionSliderValueLabel.Content = Math.Round(FadingGammaCorrectionSlider.Value, 2);
            FadingFadeSpeedSliderValueLabel.Content = Math.Round(FadingFadeSpeedSlider.Value, 0);
            FadeingFadeFactorSliderValueLabel.Content = Math.Round(FadeingFadeFactorSlider.Value, 2);
            FromToIDSliderFromValue.Content = Math.Round(FromToIDSliderFrom.Value, 0);
            FromToIDSliderToValue.Content = Math.Round(FromToIDSliderTo.Value, 0);
            VisualizerSensitivityValueLabel.Content = Math.Round(VisualizerSensitivitySlider.Value, 0);
            VisualizerSmoothnessValueLabel.Content = Math.Round(VisualizerSmoothnessSlider.Value, 0);
            VisualizerSampleDelayValueLabel.Content = Math.Round(VisualizerSampleDelaySlider.Value, 0);
            VisualizerBeatZoneFromValueSliderValueLabel.Content = Math.Round(VisualizerBeatZoneFromValueSlider.Value, 0);
            VisualizerBeatZoneToValueSliderValueLabel.Content = Math.Round(VisualizerBeatZoneToValueSlider.Value, 0);
            VisualizerBeatZoneAutoTriggerValueLabel.Content = Math.Round(VisualizerBeatZoneAutoTriggerSlider.Value, 0);
            VisualizerVisualSamplesValueLabel.Content = Math.Round(VisualizerVisualSamplesSlider.Value, 0);

            if ((bool)SettingsStartupAutoConnectAtStartupCheckbox.IsChecked)
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    NewLoadingScreen.ProgressLabel.Content = "Connecting To Device: " + (i + 1) + " / " + DeviceList.Count;
                    await ConnectToDeviceOrDevices(false, i);
                }
                if ((bool)SettingsStartupAutoSendSetupsAtStartupCheckbox.IsChecked)
                {
                    for (int i = 0; i < DeviceList.Count; i++)
                    {
                        NewLoadingScreen.ProgressLabel.Content = "Sending Setups To Device: " + (i + 1) + " / " + DeviceList.Count;
                        LoadSetup(DeviceList[i].SetupSaveFileName);
                        await SendSetupOrSetups(false, i);
                    }
                }
            }

            NewLoadingScreen.ProgressLabel.Content = "Complete!";

            for (double i = 1; i >= 0; i -= 0.1)
            {
                NewLoadingScreen.Opacity = i;
                await Task.Delay(50);
            }
            MainWindow1.Opacity = 0;
            MainWindow1.Visibility = Visibility.Visible;
            for (double i = 0; i <= 1; i += 0.1)
            {
                MainWindow1.Opacity = i;
                await Task.Delay(50);
            }

            NewLoadingScreen.Close();

            Loading = false;
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveApplicationConfig(ModesDeviceSelectionCombobox.SelectedItem.ToString());
            SaveGeneralConfigs();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragPoint = e.GetPosition(this);
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
                Draging = true;

            Mouse.Capture(TopBarDragGrid);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Draging)
            {
                Left = (System.Windows.Forms.Cursor.Position.X - DragPoint.X);
                Top = (System.Windows.Forms.Cursor.Position.Y - DragPoint.Y);
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Draging)
            {
                Left = (System.Windows.Forms.Cursor.Position.X - DragPoint.X);
                Top = (System.Windows.Forms.Cursor.Position.Y - DragPoint.Y);
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Draging = false;
            Mouse.Capture(null);
        }

        private void MainWindow1_LayoutUpdated(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                Maximized = true;
            else
                Maximized = false;
        }

        void HideAllInnerGrids()
        {
            SetupGrid.Visibility = Visibility.Hidden;
            ConnectionGrid.Visibility = Visibility.Hidden;
            FadingGrid.Visibility = Visibility.Hidden;
            VisualizerGrid.Visibility = Visibility.Hidden;
            SettingsStartupGrid.Visibility = Visibility.Hidden;
            AmbilightGrid.Visibility = Visibility.Hidden;
        }

        void HideAllSideBars()
        {
            ModesSideBar.Visibility = Visibility.Hidden;
            SettingsSideBar.Visibility = Visibility.Hidden;
            SetupSideBar.Visibility = Visibility.Hidden;
            HelpSideBar.Visibility = Visibility.Hidden;
        }

        void HideAllSubMenus()
        {
            SetupSubMenu.Visibility = Visibility.Hidden;
        }

        private void DeviceSelectionCombobox_DropDownOpened(object sender, EventArgs e)
        {
            Update_DeviceSelectionCombobox_Items();
        }

        void Update_DeviceSelectionCombobox_Items()
        {
            if (DeviceSelectionCombobox.SelectedIndex != 0)
            {
                if (CurrentSaveFileName == "")
                    CurrentSaveFileName = DeviceSelectionCombobox.SelectedItem.ToString();
                string NewSaveFileLoc = Directory.GetCurrentDirectory() + "\\Setups\\" + CurrentSaveFileName + ".txt";
                SaveSetup(NewSaveFileLoc);
                SetDeviceSetupSaveFileName(FindDeviceIndexByName(DeviceSelectionCombobox.SelectedItem.ToString()), NewSaveFileLoc);
            }
            else
            {
                SaveSetup(Directory.GetCurrentDirectory() + "\\Setups\\ALL.txt");
            }
            DeviceSelectionCombobox.Items.Clear();
            DeviceSelectionCombobox.Items.Add(" - All - ");
            DeviceSelectionCombobox.SelectedIndex = 0;
            foreach (TransferDevice Device in DeviceList)
                DeviceSelectionCombobox.Items.Add(Device.DeviceName);
        }

        private void ModeDeviceSelectionCombobox_DropDownOpened(object sender, EventArgs e)
        {
            SaveApplicationConfig(ModesDeviceSelectionCombobox.SelectedItem.ToString());

            if (ModesDeviceSelectionCombobox.Items.Count != DeviceList.Count + 1)
            {
                ModesDeviceSelectionCombobox.Items.Clear();
                ModesDeviceSelectionCombobox.Items.Add(" - All - ");
                ModesDeviceSelectionCombobox.SelectedIndex = 0;
                foreach (TransferDevice Device in DeviceList)
                    ModesDeviceSelectionCombobox.Items.Add(Device.DeviceName);
            }
        }

        private async void FromToIDSliderFrom_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await SetRanges((int)Math.Round(FromToIDSliderFrom.Value, 0), (int)Math.Round(FromToIDSliderTo.Value, 0));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Maximized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            else
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ModesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralConfigs();

            HideAllInnerGrids();
            HideAllSideBars();
            HideAllSubMenus();
            ModesSideBar.Visibility = Visibility.Visible;
        }

        private void FromToIDSliderFrom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
            {
                if (Math.Round(FromToIDSliderFrom.Value, 0) >= Math.Round(FromToIDSliderTo.Value, 0))
                {
                    if (Math.Round(FromToIDSliderFrom.Value, 0) + 1 <= FromToIDSliderTo.Maximum)
                        FromToIDSliderTo.Value = Math.Round(FromToIDSliderFrom.Value, 0) + 1;
                }
                FromToIDSliderFromValue.Content = Math.Round(FromToIDSliderFrom.Value, 0);
            }
        }
        private void ModesDeviceSelectionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Loading)
            {
                if (ModesDeviceSelectionCombobox.SelectedItem != null)
                {
                    LoadApplicationConfigsForDevice(ModesDeviceSelectionCombobox.SelectedItem.ToString());
                    if ((bool)SettingsAdvancedSettingsCheckBox.IsChecked)
                        LoadAdvancedApplicationConfigsForDevice(ModesDeviceSelectionCombobox.SelectedItem.ToString());
                    LoadRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), CurrentMode);

                    if (ModesDeviceSelectionCombobox.SelectedIndex == 0)
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() + "\\Setups\\AllSave.txt"))
                            LoadSetup(Directory.GetCurrentDirectory() + "\\Setups\\AllSave.txt");
                        else
                            InnerSetupPanel.Children.Clear();
                    }
                    else
                    {
                        int Index = FindDeviceIndexByName(ModesDeviceSelectionCombobox.SelectedItem.ToString());
                        if (File.Exists(DeviceList[Index].SetupSaveFileName))
                            LoadSetup(DeviceList[Index].SetupSaveFileName);
                        else
                            InnerSetupPanel.Children.Clear();
                    }
                }
            }
        }

        private void FromToIDSliderTo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
            {
                if (Math.Round(FromToIDSliderFrom.Value, 0) >= Math.Round(FromToIDSliderTo.Value, 0))
                {
                    if (Math.Round(FromToIDSliderTo.Value, 0) - 1 >= FromToIDSliderFrom.Minimum)
                        FromToIDSliderFrom.Value = Math.Round(FromToIDSliderTo.Value, 0) - 1;
                }
                FromToIDSliderToValue.Content = Math.Round(FromToIDSliderTo.Value, 0);
            }
        }

        #endregion

        #region ConnectionRegion

        void AddDeviceButtons(int Row, int Column)
        {
            Grid NewInnerGrid = new Grid();
            NewInnerGrid.Name = "SelectDeviceGrid";
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewInnerGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            NewInnerGrid.VerticalAlignment = VerticalAlignment.Stretch;
            NewInnerGrid.Margin = new Thickness(2, 2, 2, 2);
            NewInnerGrid.Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 70));
            Grid.SetColumn(NewInnerGrid, Column);
            Grid.SetRow(NewInnerGrid, Row);

            Button AddSerialButton = new Button();
            AddSerialButton.Name = "AddSerialDeviceButton";
            AddSerialButton.Content = "Add USB Device";
            AddSerialButton.Width = 175;
            AddSerialButton.Height = 50;
            AddSerialButton.VerticalAlignment = VerticalAlignment.Center;
            AddSerialButton.HorizontalAlignment = HorizontalAlignment.Center;
            AddSerialButton.Click += AddSerialDeviceButton_Click;
            AddSerialButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(AddSerialButton, 0);
            Grid.SetRow(AddSerialButton, 0);
            NewInnerGrid.Children.Add(AddSerialButton);

            Button AddWirelessButton = new Button();
            AddWirelessButton.Name = "AddWirelessDeviceButton";
            AddWirelessButton.Content = "Add Wireless Device";
            AddWirelessButton.Width = 175;
            AddWirelessButton.Height = 50;
            AddWirelessButton.VerticalAlignment = VerticalAlignment.Center;
            AddWirelessButton.HorizontalAlignment = HorizontalAlignment.Center;
            AddWirelessButton.Click += AddWirelessDeviceButton_Click;
            AddWirelessButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(AddWirelessButton, 0);
            Grid.SetRow(AddWirelessButton, 1);
            NewInnerGrid.Children.Add(AddWirelessButton);

            ConnectionGrid.Children.Add(NewInnerGrid);
        }

        void AddSerialDeviceButtons(object sender, string DeviceName, string COMPortName, int BaudRate, string SaveFileName, int Row, int Column, int TotalLEDCount)
        {
            Button SenderButton = sender as Button;
            Grid SenderParentGrid = SenderButton.Parent as Grid;

            Grid NewInnerGrid = new Grid();
            NewInnerGrid.Name = "DeviceGrid";
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            ColumnDefinition NewColumn = new ColumnDefinition();
            NewColumn.Width = new GridLength(150);
            NewInnerGrid.ColumnDefinitions.Add(NewColumn);
            NewInnerGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewInnerGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            NewInnerGrid.VerticalAlignment = VerticalAlignment.Stretch;
            NewInnerGrid.Margin = new Thickness(2, 2, 2, 2);
            NewInnerGrid.Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 70));
            if (Row == -1 && Column == -1)
            {
                Grid.SetColumn(NewInnerGrid, Grid.GetColumn(SenderParentGrid));
                Grid.SetRow(NewInnerGrid, Grid.GetRow(SenderParentGrid));
            }
            else
            {
                Grid.SetColumn(NewInnerGrid, Column);
                Grid.SetRow(NewInnerGrid, Row);
            }

            TextBox DeviceNameTextBox = new TextBox();
            DeviceNameTextBox.Name = "NewDeviceName";
            DeviceNameTextBox.Text = DeviceName;
            DeviceNameTextBox.TextChanged += NewDeviceName_TextChanged;
            DeviceNameTextBox.Width = 175;
            DeviceNameTextBox.Height = 35;
            DeviceNameTextBox.VerticalAlignment = VerticalAlignment.Center;
            DeviceNameTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            DeviceNameTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            DeviceNameTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            DeviceNameTextBox.Margin = new Thickness(10, 1, 1, 1);
            DeviceNameTextBox.Style = Resources["TextboxStyle"] as Style;
            Grid.SetColumn(DeviceNameTextBox, 0);
            Grid.SetRow(DeviceNameTextBox, 0);
            NewInnerGrid.Children.Add(DeviceNameTextBox);

            Button RemoveDeviceButton = new Button();
            RemoveDeviceButton.Name = "RemoveDeviceButton";
            RemoveDeviceButton.Content = "X";
            RemoveDeviceButton.Width = 25;
            RemoveDeviceButton.Height = 25;
            RemoveDeviceButton.VerticalAlignment = VerticalAlignment.Center;
            RemoveDeviceButton.HorizontalAlignment = HorizontalAlignment.Center;
            RemoveDeviceButton.Click += RemoveDeviceButton_Click;
            RemoveDeviceButton.Margin = new Thickness(1, 1, 1, 1);
            RemoveDeviceButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(RemoveDeviceButton, 1);
            Grid.SetRow(RemoveDeviceButton, 0);
            NewInnerGrid.Children.Add(RemoveDeviceButton);

            Label SelectComDeviceLabel = new Label();
            SelectComDeviceLabel.Name = "SelectComDeviceLabel";
            SelectComDeviceLabel.Content = "Select COM Device";
            SelectComDeviceLabel.Width = 175;
            SelectComDeviceLabel.Height = 35;
            SelectComDeviceLabel.VerticalAlignment = VerticalAlignment.Center;
            SelectComDeviceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            SelectComDeviceLabel.VerticalContentAlignment = VerticalAlignment.Center;
            SelectComDeviceLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectComDeviceLabel.Foreground = Brushes.White;
            SelectComDeviceLabel.Margin = new Thickness(1, 1, 1, 1);
            Grid.SetColumn(SelectComDeviceLabel, 0);
            Grid.SetRow(SelectComDeviceLabel, 1);
            Grid.SetColumnSpan(SelectComDeviceLabel, 2);
            NewInnerGrid.Children.Add(SelectComDeviceLabel);

            ComboBox SelectComDeviceCombobox = new ComboBox();
            SelectComDeviceCombobox.Name = "SelectComDeviceComboBox";
            foreach (string Port in SerialPort.GetPortNames())
                SelectComDeviceCombobox.Items.Add(Port);
            for (int i = 0; i < SelectComDeviceCombobox.Items.Count; i++)
            {
                if (SelectComDeviceCombobox.Items[i].ToString() == COMPortName)
                {
                    SelectComDeviceCombobox.SelectedIndex = i;
                    break;
                }
            }
            SelectComDeviceCombobox.SelectionChanged += SelectComDeviceComboBox_SelectionChanged;
            SelectComDeviceCombobox.Width = 175;
            SelectComDeviceCombobox.Height = 35;
            SelectComDeviceCombobox.VerticalAlignment = VerticalAlignment.Center;
            SelectComDeviceCombobox.HorizontalAlignment = HorizontalAlignment.Center;
            SelectComDeviceCombobox.VerticalContentAlignment = VerticalAlignment.Center;
            SelectComDeviceCombobox.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectComDeviceCombobox.Margin = new Thickness(1, 1, 1, 1);
            SelectComDeviceCombobox.DropDownOpened += GetAllCOMPorts;
            SelectComDeviceCombobox.Style = Resources["ComboboxStyle"] as Style;
            Grid.SetColumn(SelectComDeviceCombobox, 0);
            Grid.SetRow(SelectComDeviceCombobox, 2);
            Grid.SetColumnSpan(SelectComDeviceCombobox, 2);
            NewInnerGrid.Children.Add(SelectComDeviceCombobox);

            Label SelectBaudRateLabel = new Label();
            SelectBaudRateLabel.Name = "SelectBaudRateLabel";
            SelectBaudRateLabel.Content = "Select Baud Rate";
            SelectBaudRateLabel.Width = 175;
            SelectBaudRateLabel.Height = 35;
            SelectBaudRateLabel.VerticalAlignment = VerticalAlignment.Center;
            SelectBaudRateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            SelectBaudRateLabel.VerticalContentAlignment = VerticalAlignment.Center;
            SelectBaudRateLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectBaudRateLabel.Margin = new Thickness(1, 1, 1, 1);
            SelectBaudRateLabel.Foreground = Brushes.White;
            Grid.SetColumn(SelectBaudRateLabel, 0);
            Grid.SetRow(SelectBaudRateLabel, 3);
            NewInnerGrid.Children.Add(SelectBaudRateLabel);

            TextBox SelectBaudRateTextBox = new TextBox();
            SelectBaudRateTextBox.Name = "SelectBaudRateTextBox";
            SelectBaudRateTextBox.Text = BaudRate.ToString();
            SelectBaudRateTextBox.TextChanged += SetTextBoxToOnlyNumbers;
            SelectBaudRateTextBox.TextChanged += SelectBaudRateTextBox_TextChanged;
            SelectBaudRateTextBox.Width = 175;
            SelectBaudRateTextBox.Height = 35;
            SelectBaudRateTextBox.VerticalAlignment = VerticalAlignment.Center;
            SelectBaudRateTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            SelectBaudRateTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            SelectBaudRateTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectBaudRateTextBox.Margin = new Thickness(1, 1, 1, 1);
            SelectBaudRateTextBox.Style = Resources["TextboxStyle"] as Style;
            Grid.SetColumn(SelectBaudRateTextBox, 0);
            Grid.SetRow(SelectBaudRateTextBox, 4);
            Grid.SetColumnSpan(SelectBaudRateTextBox, 2);
            NewInnerGrid.Children.Add(SelectBaudRateTextBox);

            Button ConnectButton = new Button();
            ConnectButton.Name = "ConnectToDeviceButton";
            ConnectButton.Content = "Connect";
            ConnectButton.Width = 175;
            ConnectButton.Height = 35;
            ConnectButton.VerticalAlignment = VerticalAlignment.Center;
            ConnectButton.HorizontalAlignment = HorizontalAlignment.Center;
            ConnectButton.Click += ConnectToDeviceButton_Click;
            ConnectButton.Margin = new Thickness(1, 1, 1, 1);
            ConnectButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(ConnectButton, 0);
            Grid.SetRow(ConnectButton, 5);
            Grid.SetColumnSpan(ConnectButton, 2);
            NewInnerGrid.Children.Add(ConnectButton);

            ConnectionGrid.Children.Remove(SenderParentGrid);
            ConnectionGrid.Children.Add(NewInnerGrid);

            ArduLEDSerialProtocol NewDevice = new ArduLEDSerialProtocol(false);
            NewDevice.SerialPort1.WriteTimeout = -1;
            NewDevice.SerialPort1.Encoding = Encoding.ASCII;
            NewDevice.SerialPort1.NewLine = "/n";
            DeviceList.Add(new TransferDevice(DeviceName, NewInnerGrid, false, IPAddress.None, 0, COMPortName, BaudRate, NewDevice, SaveFileName, TotalLEDCount));

        }

        void AddWirelessDeviceButtons(object sender, string DeviceName, IPAddress IP, int Port, string SaveFileName, int Row, int Column, int TotalLEDCount)
        {
            Button SenderButton = sender as Button;
            Grid SenderParentGrid = SenderButton.Parent as Grid;

            Grid NewInnerGrid = new Grid();
            NewInnerGrid.Name = "DeviceGrid";
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            NewInnerGrid.RowDefinitions.Add(new RowDefinition());
            ColumnDefinition NewColumn = new ColumnDefinition();
            NewColumn.Width = new GridLength(150);
            NewInnerGrid.ColumnDefinitions.Add(NewColumn);
            NewInnerGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewInnerGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            NewInnerGrid.VerticalAlignment = VerticalAlignment.Stretch;
            NewInnerGrid.Margin = new Thickness(2, 2, 2, 2);
            NewInnerGrid.Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 70));
            if (Row == -1 && Column == -1)
            {
                Grid.SetColumn(NewInnerGrid, Grid.GetColumn(SenderParentGrid));
                Grid.SetRow(NewInnerGrid, Grid.GetRow(SenderParentGrid));
            }
            else
            {
                Grid.SetColumn(NewInnerGrid, Column);
                Grid.SetRow(NewInnerGrid, Row);
            }

            TextBox DeviceNameTextBox = new TextBox();
            DeviceNameTextBox.Name = "NewDeviceName";
            DeviceNameTextBox.Text = DeviceName;
            DeviceNameTextBox.TextChanged += NewDeviceName_TextChanged;
            DeviceNameTextBox.Width = 175;
            DeviceNameTextBox.Height = 35;
            DeviceNameTextBox.VerticalAlignment = VerticalAlignment.Center;
            DeviceNameTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            DeviceNameTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            DeviceNameTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            DeviceNameTextBox.Margin = new Thickness(10, 1, 1, 1);
            DeviceNameTextBox.Style = Resources["TextboxStyle"] as Style;
            Grid.SetColumn(DeviceNameTextBox, 0);
            Grid.SetRow(DeviceNameTextBox, 0);
            NewInnerGrid.Children.Add(DeviceNameTextBox);

            Button RemoveDeviceButton = new Button();
            RemoveDeviceButton.Name = "RemoveDeviceButton";
            RemoveDeviceButton.Content = "X";
            RemoveDeviceButton.Width = 25;
            RemoveDeviceButton.Height = 25;
            RemoveDeviceButton.VerticalAlignment = VerticalAlignment.Center;
            RemoveDeviceButton.HorizontalAlignment = HorizontalAlignment.Center;
            RemoveDeviceButton.Click += RemoveDeviceButton_Click;
            RemoveDeviceButton.Margin = new Thickness(1, 1, 1, 1);
            RemoveDeviceButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(RemoveDeviceButton, 1);
            Grid.SetRow(RemoveDeviceButton, 0);
            NewInnerGrid.Children.Add(RemoveDeviceButton);

            Label SelectIPLabel = new Label();
            SelectIPLabel.Name = "SelectIPLabel";
            SelectIPLabel.Content = "Select IP";
            SelectIPLabel.Width = 175;
            SelectIPLabel.Height = 35;
            SelectIPLabel.VerticalAlignment = VerticalAlignment.Center;
            SelectIPLabel.HorizontalAlignment = HorizontalAlignment.Center;
            SelectIPLabel.VerticalContentAlignment = VerticalAlignment.Center;
            SelectIPLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectIPLabel.Foreground = Brushes.White;
            SelectIPLabel.Margin = new Thickness(1, 1, 1, 1);
            Grid.SetColumn(SelectIPLabel, 0);
            Grid.SetRow(SelectIPLabel, 1);
            Grid.SetColumnSpan(SelectIPLabel, 2);
            NewInnerGrid.Children.Add(SelectIPLabel);

            TextBox SelectIPTextBox = new TextBox();
            SelectIPTextBox.Name = "SelectIPTextBox";
            SelectIPTextBox.Text = IP.ToString();
            SelectIPTextBox.TextChanged += SelectIPAddressTextBox_TextChanged;
            SelectIPTextBox.Width = 175;
            SelectIPTextBox.Height = 35;
            SelectIPTextBox.VerticalAlignment = VerticalAlignment.Center;
            SelectIPTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            SelectIPTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            SelectIPTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectIPTextBox.Margin = new Thickness(1, 1, 1, 1);
            SelectIPTextBox.Style = Resources["TextboxStyle"] as Style;
            Grid.SetColumn(SelectIPTextBox, 0);
            Grid.SetRow(SelectIPTextBox, 2);
            Grid.SetColumnSpan(SelectIPTextBox, 2);
            NewInnerGrid.Children.Add(SelectIPTextBox);

            Label SelectPortLabel = new Label();
            SelectPortLabel.Name = "SelectPortLabel";
            SelectPortLabel.Content = "Select Port";
            SelectPortLabel.Width = 175;
            SelectPortLabel.Height = 35;
            SelectPortLabel.VerticalAlignment = VerticalAlignment.Center;
            SelectPortLabel.HorizontalAlignment = HorizontalAlignment.Center;
            SelectPortLabel.VerticalContentAlignment = VerticalAlignment.Center;
            SelectPortLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectPortLabel.Margin = new Thickness(1, 1, 1, 1);
            SelectPortLabel.Foreground = Brushes.White;
            Grid.SetColumn(SelectPortLabel, 0);
            Grid.SetRow(SelectPortLabel, 3);
            NewInnerGrid.Children.Add(SelectPortLabel);

            TextBox SelectPortTextBox = new TextBox();
            SelectPortTextBox.Name = "SelectPortTextBox";
            SelectPortTextBox.Text = Port.ToString();
            SelectPortTextBox.TextChanged += SetTextBoxToOnlyNumbers;
            SelectPortTextBox.TextChanged += SelectPortTextBox_TextChanged;
            SelectPortTextBox.Width = 175;
            SelectPortTextBox.Height = 35;
            SelectPortTextBox.VerticalAlignment = VerticalAlignment.Center;
            SelectPortTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            SelectPortTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            SelectPortTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            SelectPortTextBox.Margin = new Thickness(1, 1, 1, 1);
            SelectPortTextBox.Style = Resources["TextboxStyle"] as Style;
            Grid.SetColumn(SelectPortTextBox, 0);
            Grid.SetRow(SelectPortTextBox, 4);
            Grid.SetColumnSpan(SelectPortTextBox, 2);
            NewInnerGrid.Children.Add(SelectPortTextBox);

            Button ConnectButton = new Button();
            ConnectButton.Name = "ConnectToDeviceButton";
            ConnectButton.Content = "Connect";
            ConnectButton.Width = 175;
            ConnectButton.Height = 35;
            ConnectButton.VerticalAlignment = VerticalAlignment.Center;
            ConnectButton.HorizontalAlignment = HorizontalAlignment.Center;
            ConnectButton.Click += ConnectToDeviceButton_Click;
            ConnectButton.Margin = new Thickness(1, 1, 1, 1);
            ConnectButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetColumn(ConnectButton, 0);
            Grid.SetRow(ConnectButton, 5);
            Grid.SetColumnSpan(ConnectButton, 2);
            NewInnerGrid.Children.Add(ConnectButton);

            ConnectionGrid.Children.Remove(SenderParentGrid);
            ConnectionGrid.Children.Add(NewInnerGrid);

            ArduLEDSerialProtocol NewDevice = new ArduLEDSerialProtocol(true);
            DeviceList.Add(new TransferDevice(DeviceName, NewInnerGrid, true, IP, Port, "", 0, NewDevice, SaveFileName, TotalLEDCount));
        }

        public async Task ConnectToDeviceOrDevices(bool All, int Index)
        {
            if (All)
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    await ConnectToDevice(DeviceList[i]);
                }
            }
            else
            {
                await ConnectToDevice(DeviceList[Index]);
            }
        }

        public async Task ConnectToDevice(TransferDevice Device)
        {
            Grid SenderGrid = Device.SourceGrid as Grid;
            TextBox ChangeTextColorTextBox = null;
            foreach (UIElement Element in SenderGrid.Children)
            {
                if (Element is TextBox)
                {
                    if ((Element as TextBox).Name == "NewDeviceName")
                    {
                        ChangeTextColorTextBox = (Element as TextBox);
                        ChangeTextColorTextBox.Foreground = Brushes.Yellow;
                    }
                }
            }
            if (Device.IsWireless)
            {
                bool Result = await Device.Device.ConnectToWirelessDeviceAsync(Device.IPAddress, Device.Port);
                if (Result)
                {
                    while (!Device.Device.UnitReady)
                        await Task.Delay(100);
                    ChangeTextColorTextBox.Foreground = Brushes.Green;
                    if (AutoSendSetupAtStartup)
                    {
                        // do stuff
                    }
                }
                else
                {
                    MessageBox.Show("Could not connect to Wireless device!");
                    ChangeTextColorTextBox.Foreground = Brushes.Red;
                    await Task.Delay(1000);
                    ChangeTextColorTextBox.Foreground = Brushes.White;
                }
            }
            else
            {
                bool Result = Device.Device.ConnectToCOMDevice(Device.COMPortName, Device.BaudRate);
                if (Result)
                {
                    while (!Device.Device.UnitReady)
                        await Task.Delay(100);
                    ChangeTextColorTextBox.Foreground = Brushes.Green;
                    if (AutoSendSetupAtStartup)
                    {
                        // do stuff
                    }
                }
                else
                {
                    MessageBox.Show("Could not connect to COM device!");
                    ChangeTextColorTextBox.Foreground = Brushes.Red;
                    await Task.Delay(1000);
                    ChangeTextColorTextBox.Foreground = Brushes.White;
                }
            }
        }

        private void ConnectionAddDeviceButton1_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceButtons(0, 0);
        }

        private void ConnectionAddDeviceButton2_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceButtons(0, 1);
        }

        private void ConnectionAddDeviceButton3_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceButtons(1, 0);
        }

        private void ConnectionAddDeviceButton4_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceButtons(1, 1);
        }

        private void AddSerialDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            AddSerialDeviceButtons(sender, "New Device", "", 1000000, "", -1, -1, 0);
        }

        private void AddWirelessDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            AddWirelessDeviceButtons(sender, "New Device", IPAddress.None, 8888, "", -1, -1, 0);
        }

        private void RemoveDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            Button SenderButton = sender as Button;
            DeviceList.RemoveAt(FindDeviceIndexByParentGrid(SenderButton.Parent as Grid));
            ConnectionGrid.Children.Remove(SenderButton.Parent as Grid);
        }

        private void NewDeviceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Loading)
            {
                TextBox SenderTextBox = sender as TextBox;
                SetDeviceDeviceName(FindDeviceIndexByParentGrid(SenderTextBox.Parent as Grid), SenderTextBox.Text);
            }
        }

        private void SelectComDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Loading)
            {
                ComboBox SenderCombobox = sender as ComboBox;
                if (SenderCombobox.SelectedItem != null)
                    SetDeviceCOMPortName(FindDeviceIndexByParentGrid(SenderCombobox.Parent as Grid), SenderCombobox.SelectedItem.ToString());
            }
        }

        private void SelectBaudRateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Loading)
            {
                TextBox SenderTextBox = sender as TextBox;
                SetDeviceBaudRate(FindDeviceIndexByParentGrid(SenderTextBox.Parent as Grid), Int32.Parse(SenderTextBox.Text));
            }
        }

        private void SelectIPAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Loading)
            {
                TextBox SenderTextBox = sender as TextBox;
                IPAddress NewAddress = null;
                if (IPAddress.TryParse(SenderTextBox.Text, out NewAddress))
                    SetDeviceIPAddress(FindDeviceIndexByParentGrid(SenderTextBox.Parent as Grid), NewAddress);
            }
        }

        private void SelectPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Loading)
            {
                TextBox SenderTextBox = sender as TextBox;
                SetDeviceBaudRate(FindDeviceIndexByParentGrid(SenderTextBox.Parent as Grid), Int32.Parse(SenderTextBox.Text));
            }
        }

        private async void ConnectToDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralConfigs();
            Button SenderButton = sender as Button;
            await ConnectToDevice(DeviceList[FindDeviceIndexByParentGrid(SenderButton.Parent as Grid)]);
        }

        private void DeviceSelectionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Loading)
            {
                StripIDCount = new List<int>(new int[50]);
                if (DeviceSelectionCombobox.SelectedItem != null)
                {
                    if (DeviceSelectionCombobox.SelectedIndex == 0)
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() + "\\Setups\\AllSave.txt"))
                            LoadSetup(Directory.GetCurrentDirectory() + "\\Setups\\AllSave.txt");
                        else
                            InnerSetupPanel.Children.Clear();
                    }
                    else
                    {
                        int Index = FindDeviceIndexByName(DeviceSelectionCombobox.SelectedItem.ToString());
                        if (File.Exists(DeviceList[Index].SetupSaveFileName))
                            LoadSetup(DeviceList[Index].SetupSaveFileName);
                        else
                            InnerSetupPanel.Children.Clear();
                    }
                }
            }
        }

        #endregion

        #region FadeColorsRegion
        private void FadingGridColorWheelPointer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorWheelCursorMoving = true;
            Point MousePos = FadingGridColorWheelImage.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
            if (Math.Sqrt(Math.Pow(MousePos.X - FadingGridColorWheelImage.Width / 2, 2) + Math.Pow(MousePos.Y - FadingGridColorWheelImage.Height / 2, 2)) < FadingGridColorWheelImage.Width / 2)
                FadingGridColorWheelPointer.Margin = new Thickness(MousePos.X + FadingGridColorWheelImage.Margin.Left - FadingGridColorWheelPointer.Width / 2, MousePos.Y + FadingGridColorWheelImage.Margin.Top - FadingGridColorWheelPointer.Width / 2, 0, 0);
            Mouse.Capture(FadingGridColorWheelImage);
        }

        private void FadingGridColorWheelPointer_MouseMove(object sender, MouseEventArgs e)
        {
            if (ColorWheelCursorMoving)
            {
                Point MousePos = FadingGridColorWheelImage.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                if (Math.Sqrt(Math.Pow(MousePos.X - FadingGridColorWheelImage.Width / 2, 2) + Math.Pow(MousePos.Y - FadingGridColorWheelImage.Height / 2, 2)) < FadingGridColorWheelImage.Width / 2)
                    FadingGridColorWheelPointer.Margin = new Thickness(MousePos.X + FadingGridColorWheelImage.Margin.Left - FadingGridColorWheelPointer.Width / 2, MousePos.Y + FadingGridColorWheelImage.Margin.Top - FadingGridColorWheelPointer.Width / 2, 0, 0);
                else
                {
                    double CenterX = (FadingGridColorWheelImage.Width / 2 + FadingGridColorWheelImage.Margin.Left) - FadingGridColorWheelPointer.Width / 2;
                    double CenterY = (FadingGridColorWheelImage.Height / 2 + FadingGridColorWheelImage.Margin.Top) - FadingGridColorWheelPointer.Height / 2;
                    double CircleRadius = (FadingGridColorWheelImage.Width / 2);
                    double XPointX = MousePos.X + FadingGridColorWheelImage.Margin.Left;
                    double XPointY = MousePos.Y + FadingGridColorWheelImage.Margin.Top;

                    FadingGridColorWheelPointer.Margin = new Thickness(
                        CenterX + CircleRadius * ((XPointX - CenterX) / Math.Sqrt(Math.Pow(XPointX - CenterX, 2) + Math.Pow(XPointY - CenterY, 2))),
                        CenterY + CircleRadius * ((XPointY - CenterY) / Math.Sqrt(Math.Pow(XPointX - CenterX, 2) + Math.Pow(XPointY - CenterY, 2))),
                        0,
                        0
                    );
                }
            }
        }

        private async void FadingGridColorWheelPointer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ColorWheelCursorMoving = false;
            Mouse.Capture(null);

            Color ColorInColorCirle = GammaCorrection(
                GetColorOfPixelInImage(
                    (BitmapSource)FadingGridColorWheelImage.Source,
                    (int)FadingGridColorWheelImage.Width,
                    (int)FadingGridColorWheelImage.Height,
                    (int)FadingGridColorWheelPointer.Margin.Left,
                    (int)FadingGridColorWheelPointer.Margin.Top),
                FadingGammaCorrectionSlider.Value);

            FadingRGBColorCodeLabel.Content = "RGB Color: " + ColorInColorCirle.R + ", " + ColorInColorCirle.G + ", " + ColorInColorCirle.B;

            await TransferToDeviceOrDevicesAsync((ModesDeviceSelectionCombobox.SelectedIndex == 0), ModesDeviceSelectionCombobox.SelectedItem.ToString(), new TransferMode.FadeColorsMode(ColorInColorCirle.R, ColorInColorCirle.G, ColorInColorCirle.B, (int)FadingFadeSpeedSlider.Value, FadeingFadeFactorSlider.Value));

            await FadeRectangleToColor(ColorInColorCirle, (int)FadingFadeSpeedSlider.Value, FadeingFadeFactorSlider.Value);
        }

        async Task FadeRectangleToColor(Color _InputColor, int _FadeSpeed, double _FadeFactor)
        {
            Color OriginalColors = ((SolidColorBrush)FadingColorRectangle.Fill).Color;

            int[] OriginalColor = { OriginalColors.R, OriginalColors.G, OriginalColors.B };
            int[] TargerColor = { _InputColor.R, _InputColor.G, _InputColor.B };
            float[] CurrentColor = { 0, 0, 0 };
            float[] CurrentColorJump = { 0, 0, 0 };

            for (short i = 0; i < 3; i++)
            {
                CurrentColorJump[i] = (((float)OriginalColor[i] - (float)TargerColor[i]) * (float)_FadeFactor);
                CurrentColor[i] = OriginalColor[i];
            }

            while (Convert.ToInt32(CurrentColor[0] == TargerColor[0]) + Convert.ToInt32(CurrentColor[1] == TargerColor[1]) + Convert.ToInt32(CurrentColor[2] == TargerColor[2]) < 3)
            {
                for (short i = 0; i < 3; i++)
                {
                    CurrentColor[i] -= CurrentColorJump[i];
                    CurrentColorJump[i] = ((CurrentColor[i] - (float)TargerColor[i]) * (float)_FadeFactor);
                    if (CurrentColor[i] < 0)
                        CurrentColor[i] = 0;
                    if (CurrentColor[i] > 255)
                        CurrentColor[i] = 255;
                    if (CurrentColorJump[i] < 0)
                    {
                        if (CurrentColorJump[i] >= -1)
                        {
                            CurrentColor[i] = TargerColor[i];
                            CurrentColorJump[i] = 0;
                        }
                    }
                    else
                    {
                        if (CurrentColorJump[i] <= 1)
                        {
                            CurrentColor[i] = TargerColor[i];
                            CurrentColorJump[i] = 0;
                        }
                    }
                }

                FadingColorRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, (byte)CurrentColor[0], (byte)CurrentColor[1], (byte)CurrentColor[2]));

                await Task.Delay(_FadeSpeed);
            }
        }

        Color GetColorOfPixelInImage(BitmapSource _SourceImage, int ControlWidth, int ControlHeight, int X, int Y)
        {
            int stride = _SourceImage.PixelWidth * 4;
            int size = _SourceImage.PixelHeight * stride;
            byte[] pixels = new byte[size];
            _SourceImage.CopyPixels(pixels, stride, 0);
            int index = (int)(Y * (_SourceImage.PixelHeight / ControlHeight)) * stride + 4 * (int)(X * (_SourceImage.PixelWidth / ControlWidth));
            byte red = pixels[index + 2];
            byte green = pixels[index + 1];
            byte blue = pixels[index];

            return Color.FromArgb(255, red, green, blue);
        }

        private async void ModesFadeingButton_Click(object sender, RoutedEventArgs e)
        {
            SaveRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), CurrentMode);

            HideAllInnerGrids();
            FadingGrid.Visibility = Visibility.Visible;

            LoadRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), "FADING");
            CurrentMode = "FADING";

            Color ColorInColorCirle = GammaCorrection(
                GetColorOfPixelInImage(
                    (BitmapSource)FadingGridColorWheelImage.Source,
                    (int)FadingGridColorWheelImage.Width,
                    (int)FadingGridColorWheelImage.Height,
                    (int)FadingGridColorWheelPointer.Margin.Left,
                    (int)FadingGridColorWheelPointer.Margin.Top),
                FadingGammaCorrectionSlider.Value);

            await TransferToDeviceOrDevicesAsync((ModesDeviceSelectionCombobox.SelectedIndex == 0), ModesDeviceSelectionCombobox.SelectedItem.ToString(), new TransferMode.FadeColorsMode((short)(ColorInColorCirle.R + 1), ColorInColorCirle.G, ColorInColorCirle.B, (int)FadingFadeSpeedSlider.Value, FadeingFadeFactorSlider.Value));
            await TransferToDeviceOrDevicesAsync((ModesDeviceSelectionCombobox.SelectedIndex == 0), ModesDeviceSelectionCombobox.SelectedItem.ToString(), new TransferMode.FadeColorsMode(ColorInColorCirle.R, ColorInColorCirle.G, ColorInColorCirle.B, (int)FadingFadeSpeedSlider.Value, FadeingFadeFactorSlider.Value));
        }

        private void FadingGammaCorrectionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                FadingGammaCorrectionSliderValueLabel.Content = Math.Round(FadingGammaCorrectionSlider.Value, 2);
        }

        private void FadingFadeSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                FadingFadeSpeedSliderValueLabel.Content = Math.Round(FadingFadeSpeedSlider.Value, 0);
        }

        private void FadeingFadeFactorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                FadeingFadeFactorSliderValueLabel.Content = Math.Round(FadeingFadeFactorSlider.Value, 2);
        }

        #endregion

        #region VisualizerRegion
        async Task StartVisualizer(bool Start, string DeviceName)
        {
            if (Start)
            {
                if (RunVisualizer)
                {
                    RunVisualizer = false;
                    while (!VisualizerStopped)
                        await Task.Delay(10);
                }
                if (BassWasapi.BASS_WASAPI_IsStarted())
                    BassWasapi.BASS_WASAPI_Stop(true);

                BassWasapi.BASS_WASAPI_Free();
                Bass.BASS_Free();

                BassProcess = new WASAPIPROC(Process);

                var array = (VisualizerSelectDeviceCombobox.Items[VisualizerSelectDeviceCombobox.SelectedIndex] as string).Split(' ');
                int devindex = Convert.ToInt32(array[0]);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                bool result = BassWasapi.BASS_WASAPI_Init(devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, BassProcess, IntPtr.Zero);
                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    MessageBox.Show(error.ToString());
                }

                BassWasapi.BASS_WASAPI_Start();

                RunVisualizer = true;
                VisualizerStopped = false;
                VisualizerDeviceName = DeviceName;

                Task VisualizerThreadStart = new Task(delegate {
                    VisualizerThread();
                });
                VisualizerThreadStart.Start();
            }
            else
            {
                if (RunVisualizer)
                {
                    RunVisualizer = false;
                    while (!VisualizerStopped)
                        await Task.Delay(10);
                }
                if (BassWasapi.BASS_WASAPI_IsStarted())
                    BassWasapi.BASS_WASAPI_Stop(true);

                BassWasapi.BASS_WASAPI_Free();
                Bass.BASS_Free();
            }
        }

        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        private void VisualizerThread()
        {
            int VisualSamples = 0;
            int Smoothness = 0;
            int Sensitivity = 0;
            int BeatZoneFrom = 0;
            int BeatZoneTo = 0;
            int VisualizationIndex = 0;
            int TriggerHeight = 0;
            int SpectrumSplit = 0;
            int RefreshRate = 0;
            string DeviceName = "";

            Dispatcher.Invoke(() =>
            {
                VisualSamples = (int)VisualizerVisualSamplesSlider.Value;
                Smoothness = (int)VisualizerSmoothnessSlider.Value;
                Sensitivity = (int)VisualizerSensitivitySlider.Value;
                BeatZoneFrom = (int)VisualizerBeatZoneFromValueSlider.Value;
                BeatZoneTo = (int)VisualizerBeatZoneToValueSlider.Value;
                VisualizationIndex = (int)VisualizerSelectVisualizationTypeCombobox.SelectedIndex;
                TriggerHeight = (int)VisualizerBeatZoneAutoTriggerSlider.Value;
                SpectrumSplit = 1;
                RefreshRate = (int)VisualizerSampleDelaySlider.Value;
                DeviceName = VisualizerDeviceName;
            });

            DateTime VisualizerRPSCounter = new DateTime();
            DateTime CalibrateRefreshRate = new DateTime();
            int VisualizerUpdatesCounter = 0;
            List<List<int>> AudioDataPointStore = new List<List<int>>();
            List<int> AudioValues = new List<int>(new int[VisualSamples]);
            float[] AudioData = new float[16384];
            int X, Y;
            int B0 = 0;
            int AverageValue = 0;

            for (int i = 0; i < VisualSamples; i++)
                AudioDataPointStore.Add(new List<int>(new int[Smoothness]));

            while (RunVisualizer)
            {
                CalibrateRefreshRate = DateTime.Now;

                if (VisualizerVisualRefreshTick == 0)
                    Dispatcher.Invoke(() => { TriggerHeight = (int)VisualizerBeatZoneAutoTriggerSlider.Value; });

                int ReturnValue = BassWasapi.BASS_WASAPI_GetData(AudioData, (int)(BASSData)Enum.Parse(typeof(BASSData), "BASS_DATA_FFT16384"));
                if (ReturnValue < -1) return;

                B0 = 0;
                for (X = BeatZoneFrom; X < BeatZoneTo - 1; X++)
                {
                    float Peak = 0;
                    int B1 = (int)Math.Pow(2, X * 10.0 / ((int)VisualSamples - 1));
                    if (B1 > 1023) B1 = 1023;
                    if (B1 <= B0) B1 = B0 + 1;
                    for (; B0 < B1; B0++)
                    {
                        if (Peak < AudioData[1 + B0]) Peak = AudioData[1 + B0];
                    }
                    Y = (int)(Math.Sqrt(Peak) * Sensitivity * 255 - 4);
                    if (Y > 255) Y = 255;
                    if (Y < 1) Y = 1;

                    if (X >= BeatZoneFrom)
                    {
                        if (X <= BeatZoneTo)
                        {
                            AverageValue = 0;
                            if (Smoothness > 1)
                            {
                                AudioDataPointStore[X].Add(Y);
                                while (AudioDataPointStore[X].Count > Smoothness)
                                    AudioDataPointStore[X].RemoveAt(0);

                                for (int s = 0; s < Smoothness; s++)
                                {
                                    AverageValue += AudioDataPointStore[X][s];
                                }
                                AverageValue = AverageValue / Smoothness;
                            }
                            else
                            {
                                AverageValue = Y;
                            }
                            if (AverageValue > 255)
                                AverageValue = 255;
                            if (AverageValue < 0)
                                AverageValue = 0;

                            AudioValues[X] = AverageValue;
                        }
                    }
                }

                if (VisualizationIndex == 0)
                {
                    double Hit = 0;
                    for (int i = 0; i < AudioValues.Count; i++)
                    {
                        if (AudioValues[i] >= TriggerHeight)
                            Hit++;
                    }
                    double OutValue = Math.Round(Math.Round((Hit / ((double)BeatZoneTo - (double)BeatZoneFrom)), 2) * 99, 0);
                    AutoTrigger((OutValue / 99) * (255 * 3));
                    if (OutValue > 99)
                        OutValue = 99;

                    TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, new TransferMode.VisualizerBeat((int)OutValue));
                }
                if (VisualizationIndex == 1 | VisualizationIndex == 2)
                {
                    double EndR = 0;
                    double EndG = 0;
                    double EndB = 0;
                    int CountR = 0;
                    int CountG = 0;
                    int CountB = 0;
                    int Hit = 0;
                    for (int i = 0; i < VisualizerSpectrumRed.Count; i++)
                    {
                        if (AudioValues[i] >= TriggerHeight)
                        {
                            try
                            {
                                if (VisualizerSpectrumRed[i] <= 255)
                                {
                                    if (VisualizerSpectrumRed[i] >= 0)
                                    {
                                        EndR += VisualizerSpectrumRed[i];
                                        CountR++;
                                    }
                                }
                            }
                            catch
                            {
                                EndR += 0;
                                CountR++;
                            }
                            try
                            {
                                if (VisualizerSpectrumGreen[i] <= 255)
                                {
                                    if (VisualizerSpectrumGreen[i] >= 0)
                                    {
                                        EndG += VisualizerSpectrumGreen[i];
                                        CountG++;
                                    }
                                }
                            }
                            catch
                            {
                                EndG += 0;
                                CountG++;
                            }
                            try
                            {
                                if (VisualizerSpectrumBlue[i] <= 255)
                                {
                                    if (VisualizerSpectrumBlue[i] >= 0)
                                    {
                                        EndB += VisualizerSpectrumBlue[i];
                                        CountB++;
                                    }
                                }
                            }
                            catch
                            {
                                EndB += 0;
                                CountB++;
                            }
                            Hit++;
                        }
                    }

                    AutoTrigger(((float)Hit / ((float)BeatZoneTo - (float)BeatZoneFrom)) * (255 * 3));

                    if (CountR > 0)
                    {
                        EndR = EndR / CountR;
                    }
                    if (CountG > 0)
                    {
                        EndG = EndG / CountG;
                    }
                    if (CountB > 0)
                    {
                        EndB = EndB / CountB;
                    }

                    if (VisualizationIndex == 1)
                        TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, new TransferMode.FadeColorsMode((short)EndR, (short)EndG, (short)EndB, 0, 0));
                    if (VisualizationIndex == 2)
                        TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, new TransferMode.VisualizerWave((short)EndR, (short)EndG, (short)EndB));
                }
                if (VisualizationIndex == 3 | VisualizationIndex == 4)
                {
                    int EndR = 0;
                    int EndG = 0;
                    int EndB = 0;
                    int Hit = 0;

                    for (int i = 0; i < AudioValues.Count; i++)
                    {
                        if (AudioValues[i] >= TriggerHeight)
                        {
                            Hit++;
                        }
                    }

                    int EndValue = (int)(((float)255 * (float)3) * ((float)Hit / ((float)BeatZoneTo - (float)BeatZoneFrom)));
                    if (EndValue >= 765)
                        EndValue = 764;
                    if (EndValue < 0)
                        EndValue = 0;

                    try
                    {
                        EndR = (int)VisualizerBeatWaveRed[EndValue];
                        EndG = (int)VisualizerBeatWaveGreen[EndValue];
                        EndB = (int)VisualizerBeatWaveBlue[EndValue];
                    }
                    catch
                    {
                        EndR = 0;
                        EndG = 0;
                        EndB = 0;
                    }

                    AutoTrigger(((float)Hit / ((float)BeatZoneTo - (float)BeatZoneFrom)) * (255 * 3));

                    if (EndR > 255)
                        EndR = 0;

                    if (EndG > 255)
                        EndG = 0;

                    if (EndB > 255)
                        EndB = 0;

                    if (EndR < 0)
                        EndR = 0;

                    if (EndG < 0)
                        EndG = 0;

                    if (EndB < 0)
                        EndB = 0;

                    if (VisualizationIndex == 4)
                        TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, new TransferMode.FadeColorsMode((short)EndR, (short)EndG, (short)EndB, 0, 0));
                    if (VisualizationIndex == 3)
                        TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, new TransferMode.VisualizerWave((short)EndR, (short)EndG, (short)EndB));
                }
                if (VisualizationIndex == 5)
                {
                    int Hit = 0;
                    TransferMode.VisualizerFullSpectrum newSpec = new TransferMode.VisualizerFullSpectrum("", SpectrumSplit);
                    for (int i = 0; i < AudioValues.Count; i++)
                    {
                        if (AudioValues[i] >= TriggerHeight)
                        {
                            newSpec.SpectrumValues += Math.Round((AudioValues[i] / 255) * (double)SpectrumSplit, 0) + ";";
                            Hit++;
                        }
                        else
                            newSpec.SpectrumValues += "0;";
                    }

                    AutoTrigger(((float)Hit / ((float)BeatZoneTo - (float)BeatZoneFrom)) * (255 * 3));

                    TransferToDeviceOrDevices((DeviceName == "ALL"), DeviceName, newSpec);
                }

                if (VisualizerVisualRefreshTick == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (VisualizerGrid.Visibility == Visibility.Visible)
                        {
                            VisualizerBeatZoneCanvas.Children.Clear();

                            double TransformX = VisualizerBeatZoneCanvas.Width / (AudioValues.Count / 2);
                            double TransformY = VisualizerBeatZoneCanvas.Height / 255;
                            Point PrePoint = new Point(0, VisualizerBeatZoneCanvas.Height);
                            for (int i = 0; i < AudioValues.Count / 2; i++)
                            {
                                Line NewLine = new Line();
                                NewLine.Stroke = Brushes.Blue;
                                NewLine.X1 = PrePoint.X;
                                NewLine.Y1 = PrePoint.Y;
                                NewLine.X2 = TransformX * i;
                                NewLine.Y2 = VisualizerBeatZoneCanvas.Height - (TransformY * AudioValues[i * 2]);
                                PrePoint = new Point(TransformX * i, VisualizerBeatZoneCanvas.Height - (TransformY * AudioValues[i * 2]));
                                VisualizerBeatZoneCanvas.Children.Add(NewLine);
                            }
                        }
                    });
                }

                VisualizerUpdatesCounter++;
                if ((DateTime.Now - VisualizerRPSCounter).TotalSeconds >= 1)
                {
                    CurrentFPSValue = VisualizerUpdatesCounter;
                    VisualizerUpdatesCounter = 0;
                    VisualizerRPSCounter = DateTime.Now;
                }

                if (VisualizerVisualRefreshTick == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        VisualizerBeatZoneRPSLabel.Content = "RPS:" + Environment.NewLine + CurrentFPSValue;
                        if (CurrentFPSValue > VisualizerHighestRPS)
                        {
                            VisualizerBeatZoneRPSHighestLabel.Content = "Top:" + Environment.NewLine + CurrentFPSValue;
                            VisualizerHighestRPS = CurrentFPSValue;
                        }
                        if (CurrentFPSValue < VisualizerLowestRPS)
                        {
                            VisualizerBeatZoneRPSLowestLabel.Content = "Low:" + Environment.NewLine + CurrentFPSValue;
                            VisualizerLowestRPS = CurrentFPSValue;
                        }
                    });
                }

                int ExectuionTime = (int)(DateTime.Now - CalibrateRefreshRate).TotalMilliseconds;
                int ActuralRefreshTime = RefreshRate - ExectuionTime;

                if (ActuralRefreshTime < 0)
                    ActuralRefreshTime = 0;

                Thread.Sleep(ActuralRefreshTime);

                VisualizerVisualRefreshTick++;
                if (VisualizerVisualRefreshTick > VisualizerVisualRefreshTickMax)
                    VisualizerVisualRefreshTick = 0;
            }
            VisualizerStopped = true;
        }

        private void AutoTrigger(double _TriggerValue)
        {
            if (VisualizerVisualRefreshTick == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    if ((bool)VisualizerBeatZoneAutoTriggerCheckBox.IsChecked)
                    {
                        VisualizerBeatZoneCurrentValueLabel.Content = ((int)_TriggerValue).ToString();
                        if (_TriggerValue >= Int32.Parse(VisualizerBeatZoneAutoTriggerIncresseTextBox.Text))
                        {
                            if (VisualizerBeatZoneAutoTriggerSlider.Value < Int32.Parse(VisualizerBeatZoneAutoTriggerMaxTextBox.Text))
                                VisualizerBeatZoneAutoTriggerSlider.Value++;
                        }
                        if (_TriggerValue <= Int32.Parse(VisualizerBeatZoneAutoTriggerDecreeseTextBox.Text))
                        {
                            if (VisualizerBeatZoneAutoTriggerSlider.Value > Int32.Parse(VisualizerBeatZoneAutoTriggerMinTextBox.Text))
                                VisualizerBeatZoneAutoTriggerSlider.Value--;
                        }
                    }
                    else
                        VisualizerBeatZoneAutoTriggerValueLabel.Content = "0";
                });
            }
        }

        void UpdateSpectrums(string _Red, string _Green, string _Blue, bool AutoScale, Canvas ParentCanvas, double CanvasWidth, double CanvasHeight, int _XValues, List<double> ListOfRedPoints, List<double> ListOfGreenPoints, List<double> ListOfBluePoints)
        {
            ParentCanvas.Children.Clear();
            ListOfRedPoints.Clear();
            ListOfGreenPoints.Clear();
            ListOfBluePoints.Clear();

            SpectrumCrash = false;
            double TransformX = CanvasWidth / _XValues;
            double TransformY = CanvasHeight / 255;
            try
            {
                Point PreRedPoint = new Point(0, 0);
                Point PreGreenPoint = new Point(0, 0);
                Point PreBluePoint = new Point(0, 0);
                double YValue = 0;
                for (int i = 0; i < _XValues; i++)
                {
                    Line NewRedLine = new Line();
                    NewRedLine.Stroke = Brushes.Red;
                    NewRedLine.StrokeThickness = 2;
                    NewRedLine.X1 = PreRedPoint.X;
                    NewRedLine.Y1 = PreRedPoint.Y;
                    NewRedLine.X2 = i * TransformX;
                    YValue = TransformToPoint(_Red, i);

                    if (AutoScale)
                    {
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                        ListOfRedPoints.Add(YValue);
                    }
                    else
                    {
                        ListOfRedPoints.Add(YValue);
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                    }

                    NewRedLine.Y2 = CanvasHeight - (YValue * TransformY);

                    PreRedPoint.X = NewRedLine.X2;
                    PreRedPoint.Y = NewRedLine.Y2;

                    ParentCanvas.Children.Add(NewRedLine);

                    Line NewGreenLine = new Line();
                    NewGreenLine.Stroke = Brushes.Green;
                    NewGreenLine.StrokeThickness = 2;
                    NewGreenLine.X1 = PreGreenPoint.X;
                    NewGreenLine.Y1 = PreGreenPoint.Y;
                    NewGreenLine.X2 = i * TransformX;
                    YValue = TransformToPoint(_Green, i);

                    if (AutoScale)
                    {
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                        ListOfGreenPoints.Add(YValue);
                    }
                    else
                    {
                        ListOfGreenPoints.Add(YValue);
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                    }

                    NewGreenLine.Y2 = CanvasHeight - (YValue * TransformY);

                    PreGreenPoint.X = NewGreenLine.X2;
                    PreGreenPoint.Y = NewGreenLine.Y2;

                    ParentCanvas.Children.Add(NewGreenLine);

                    Line NewBlueLine = new Line();
                    NewBlueLine.Stroke = Brushes.Blue;
                    NewBlueLine.StrokeThickness = 2;
                    NewBlueLine.X1 = PreBluePoint.X;
                    NewBlueLine.Y1 = PreBluePoint.Y;
                    NewBlueLine.X2 = i * TransformX;
                    YValue = TransformToPoint(_Blue, i);

                    if (AutoScale)
                    {
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                        ListOfBluePoints.Add(YValue);
                    }
                    else
                    {
                        ListOfBluePoints.Add(YValue);
                        if (YValue > 255) YValue = 255;
                        if (YValue < 0) YValue = 0;
                    }

                    NewBlueLine.Y2 = CanvasHeight - (YValue * TransformY);

                    PreBluePoint.X = NewBlueLine.X2;
                    PreBluePoint.Y = NewBlueLine.Y2;

                    ParentCanvas.Children.Add(NewBlueLine);

                    if (SpectrumCrash)
                    {
                        MessageBox.Show("Error in input string");
                        break;
                    }
                }
            }
            catch { MessageBox.Show("Error in input string"); }
        }

        private double TransformToPoint(string _InputEquation, int _XValue)
        {
            try
            {
                string TransformedInputString = _InputEquation.ToLower().Replace("x", _XValue.ToString()).Replace(".", ",").Replace(" ", "");
                string[] Split = System.Text.RegularExpressions.Regex.Split(TransformedInputString, @"(?<=[()^*/+-])");

                List<string> EquationParts = new List<string>();
                foreach (string s in Split)
                {
                    EquationParts.Add(s);
                }

                if (EquationParts[0] == "-")
                {
                    EquationParts[0] = "-" + EquationParts[1];
                    EquationParts.RemoveAt(1);
                }
                if (EquationParts[0] == "+")
                {
                    EquationParts.RemoveAt(0);
                }

                for (int i = 0; i < EquationParts.Count; i++)
                {
                    if (EquationParts[i].Contains("(") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("(", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("(", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains(")") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace(")", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace(")", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains("^") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("^", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("^", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains("*") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("*", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("*", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains("/") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("/", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("/", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains("+") && EquationParts[i].Length > 1)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("+", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("+", "");
                        i = 0;
                    }
                    if (EquationParts[i].Contains("-") && EquationParts[i].Length > 1 && EquationParts[i].IndexOf('-') != 0)
                    {
                        EquationParts.Insert(i + 1, EquationParts[i].Replace(EquationParts[i].Replace("-", ""), ""));
                        EquationParts[i] = EquationParts[i].Replace("-", "");
                        i = 0;
                    }
                }

                while (EquationParts.Contains("("))
                {
                    int StartIndex = EquationParts.FindIndex(s => s.Equals("("));
                    int EndIndex = EquationParts.FindIndex(s => s.Equals(")"));
                    string ComputeString = "";
                    for (int i = StartIndex + 1; i < EndIndex; i++)
                        ComputeString += EquationParts[i];
                    EquationParts[StartIndex] = TransformToPoint(ComputeString, _XValue).ToString();
                    EquationParts.RemoveRange(StartIndex + 1, EndIndex - StartIndex);
                }
                while (EquationParts.Contains("^"))
                {
                    int Index = EquationParts.FindIndex(s => s.Equals("^"));
                    EquationParts[Index] = (Math.Pow(Convert.ToDouble(EquationParts[Index - 1]), Convert.ToDouble(EquationParts[Index + 1]))).ToString();
                    EquationParts.RemoveAt(Index + 1);
                    EquationParts.RemoveAt(Index - 1);
                }
                while (EquationParts.Contains("*"))
                {
                    int Index = EquationParts.FindIndex(s => s.Equals("*"));
                    EquationParts[Index] = (Convert.ToDecimal(EquationParts[Index - 1]) * Convert.ToDecimal(EquationParts[Index + 1])).ToString();
                    EquationParts.RemoveAt(Index + 1);
                    EquationParts.RemoveAt(Index - 1);
                }
                while (EquationParts.Contains("/"))
                {
                    int Index = EquationParts.FindIndex(s => s.Equals("/"));
                    EquationParts[Index] = (Convert.ToDecimal(EquationParts[Index - 1]) / Convert.ToDecimal(EquationParts[Index + 1])).ToString();
                    EquationParts.RemoveAt(Index + 1);
                    EquationParts.RemoveAt(Index - 1);
                }
                while (EquationParts.Contains("+"))
                {
                    int Index = EquationParts.FindIndex(s => s.Equals("+"));
                    EquationParts[Index] = (Convert.ToDecimal(EquationParts[Index - 1]) + Convert.ToDecimal(EquationParts[Index + 1])).ToString();
                    EquationParts.RemoveAt(Index + 1);
                    EquationParts.RemoveAt(Index - 1);
                }
                while (EquationParts.Contains("-"))
                {
                    int Index = EquationParts.FindIndex(s => s.Equals("-"));
                    EquationParts[Index] = (Convert.ToDecimal(EquationParts[Index - 1]) - Convert.ToDecimal(EquationParts[Index + 1])).ToString();
                    EquationParts.RemoveAt(Index + 1);
                    EquationParts.RemoveAt(Index - 1);
                }

                return Convert.ToDouble(EquationParts[0]);

            }
            catch
            {
                MessageBox.Show("Error in input string");
                SpectrumCrash = true;
                return 0;
            }
        }

        private void ModesVisualizerButton_Click(object sender, RoutedEventArgs e)
        {
            SaveRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), CurrentMode);

            HideAllInnerGrids();
            VisualizerGrid.Visibility = Visibility.Visible;

            LoadRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), "VISUALIZER");
            CurrentMode = "VISUALIZER";
        }

        private void VisualizerSelectVisualizationTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualizerBeatWaveSettingsGrid.Visibility = Visibility.Hidden;
            VisualizerSpectrumSettingsGrid.Visibility = Visibility.Hidden;
            VisualizerFullSpectrumSettingsGrid.Visibility = Visibility.Hidden;

            if ((bool)SettingsAdvancedSettingsCheckBox.IsChecked)
            {
                if (VisualizerSelectVisualizationTypeCombobox.SelectedIndex == 1 | VisualizerSelectVisualizationTypeCombobox.SelectedIndex == 2)
                    VisualizerSpectrumSettingsGrid.Visibility = Visibility.Visible;

                if (VisualizerSelectVisualizationTypeCombobox.SelectedIndex == 3 | VisualizerSelectVisualizationTypeCombobox.SelectedIndex == 4)
                    VisualizerBeatWaveSettingsGrid.Visibility = Visibility.Visible;

                if (VisualizerSelectVisualizationTypeCombobox.SelectedIndex == 5)
                    VisualizerFullSpectrumSettingsGrid.Visibility = Visibility.Visible;
            }
        }

        private void VisualizerSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerSensitivityValueLabel.Content = Math.Round(VisualizerSensitivitySlider.Value, 0);
        }

        private void VisualizerVisualSamplesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
            {
                VisualizerVisualSamplesValueLabel.Content = Math.Round(VisualizerVisualSamplesSlider.Value, 0);
                VisualizerBeatZoneFromValueSlider.Maximum = Math.Round(VisualizerVisualSamplesSlider.Value, 0);
                VisualizerBeatZoneFromValueSliderValueLabel.Content = "0";
                VisualizerBeatZoneToValueSlider.Maximum = Math.Round(VisualizerVisualSamplesSlider.Value, 0);
                VisualizerBeatZoneToValueSliderValueLabel.Content = Math.Round(VisualizerVisualSamplesSlider.Value, 0);
                VisualizerBeatZoneFromValueSlider.Value = 0;
                VisualizerBeatZoneToValueSlider.Value = Math.Round(VisualizerVisualSamplesSlider.Value, 0);
            }
        }

        private void VisualizerSampleDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerSampleDelayValueLabel.Content = Math.Round(VisualizerSampleDelaySlider.Value, 0);
        }

        private void VisualizerSmoothnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerSmoothnessValueLabel.Content = Math.Round(VisualizerSmoothnessSlider.Value, 0);
        }

        private void VisualizerBeatZoneAutoTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)VisualizerBeatZoneAutoTriggerCheckBox.IsChecked)
            {
                VisualizerBeatZoneAutoTriggerIncresseTextBox.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerDecreeseTextBox.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerIncresseLabel.IsEnabled = true;
                VisualizerBeatZoneCurrentValueLabel.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerDecreeseLabel.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerMaxLabel.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerMaxTextBox.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerMinLabel.IsEnabled = true;
                VisualizerBeatZoneAutoTriggerMinTextBox.IsEnabled = true;
            }
            else
            {
                VisualizerBeatZoneAutoTriggerIncresseTextBox.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerDecreeseTextBox.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerIncresseLabel.IsEnabled = false;
                VisualizerBeatZoneCurrentValueLabel.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerDecreeseLabel.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerMaxLabel.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerMaxTextBox.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerMinLabel.IsEnabled = false;
                VisualizerBeatZoneAutoTriggerMinTextBox.IsEnabled = false;
            }
        }

        private void VisualizerBeatZoneAutoTriggerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerBeatZoneAutoTriggerValueLabel.Content = Math.Round(VisualizerBeatZoneAutoTriggerSlider.Value, 0);
        }

        private void VisualizerBeatZoneFromValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerBeatZoneFromValueSliderValueLabel.Content = Math.Round(VisualizerBeatZoneFromValueSlider.Value, 0);
        }

        private void VisualizerBeatZoneToValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Loading)
                VisualizerBeatZoneToValueSliderValueLabel.Content = Math.Round(VisualizerBeatZoneToValueSlider.Value, 0);
        }

        private void VisualizerBeatZoneRPSResetButton_Click(object sender, RoutedEventArgs e)
        {
            VisualizerLowestRPS = 9999;
            VisualizerHighestRPS = 0;
        }

        private async void VisualizerStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModesDeviceSelectionCombobox.SelectedIndex == 0)
            {
                await StartVisualizer(true, "ALL");
            }
            else
            {
                await StartVisualizer(true, ModesDeviceSelectionCombobox.Items[ModesDeviceSelectionCombobox.SelectedIndex].ToString());
            }
        }

        private async void VisualizerStopButton_Click(object sender, RoutedEventArgs e)
        {
            await StartVisualizer(false, "");
        }

        private void VisualizerSpectrumUpdateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VisualizerBeatWaveUpdateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region AmbilightRegion

        private void ModesAmbilightButton_Click(object sender, RoutedEventArgs e)
        {
            SaveRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), CurrentMode);

            HideAllInnerGrids();
            AmbilightGrid.Visibility = Visibility.Visible;

            LoadRangesForMode(ModesDeviceSelectionCombobox.SelectedItem.ToString(), "AMBILIGHT");
            CurrentMode = "AMBILIGHT";
        }

        private void AmbilightShowHideBlocks_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideBlocks((BlockList.Count == 0), AmbilightScreenIDCombobox.SelectedIndex, Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text));
        }

        private async void AmbilightAutosetOffsetsButton_Click(object sender, RoutedEventArgs e)
        {
            await AutoSetOffsets(AmbilightScreenIDCombobox.SelectedIndex, Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text));
        }

        private void AmbilightSaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AmbilightAutosetBlockSizes_Click(object sender, RoutedEventArgs e)
        {
            await AutoSetBlockSize(AmbilightScreenIDCombobox.SelectedIndex, Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text));
        }

        private void AmbilightLoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AmbilightStartAmbilightButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)SettingsAdvancedSettingsCheckBox.IsChecked)
            {
                await AutoSetBlockSize(AmbilightScreenIDCombobox.SelectedIndex, Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text));
                await AutoSetBlockSize(AmbilightScreenIDCombobox.SelectedIndex, Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text));
            }
            StartAmbilight(
                AmbilightScreenIDCombobox.SelectedIndex, 
                Convert.ToInt32(AmbilightBlockSampleSplitTextbox.Text), 
                Convert.ToDouble(AmbilightGammaFactorTextbox.Text), 
                Convert.ToDouble(AmbilightFadeFactorTextbox.Text), 
                (1000 / Convert.ToInt32(AmbilightMaximumFPSTextbox.Text)), 
                (ModesDeviceSelectionCombobox.SelectedIndex == 0), 
                ModesDeviceSelectionCombobox.SelectedItem.ToString()
                );
        }

        private void AmbilightStopAmbilightButton_Click(object sender, RoutedEventArgs e)
        {
            StopAmbilight();
        }

        private async void AmbilightSelectTopSideCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                FromToIDS ReturnedValues = await GetAmbilightToFromID();

                if (ReturnedValues.FromID == -1 && ReturnedValues.ToID == -1)
                {
                    TopSide.Enabled = false;
                    AmbilightSelectTopSideCheckbox.IsChecked = false;
                }
                else
                {
                    TopSide.FromID = ReturnedValues.FromID;
                    TopSide.ToID = ReturnedValues.ToID;
                }
            }
        }

        private void AmbilightSelectTopSideCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            TopSide.Enabled = false;
        }

        private async void AmbilightSelectRightSideCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                FromToIDS ReturnedValues = await GetAmbilightToFromID();

                if (ReturnedValues.FromID == -1 && ReturnedValues.ToID == -1)
                {
                    RightSide.Enabled = false;
                    AmbilightSelectRightSideCheckbox.IsChecked = false;
                }
                else
                {
                    RightSide.FromID = ReturnedValues.FromID;
                    RightSide.ToID = ReturnedValues.ToID;
                }
            }
        }

        private void AmbilightSelectRightSideCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RightSide.Enabled = false;
        }

        private async void AmbilightSelectLeftSideCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                FromToIDS ReturnedValues = await GetAmbilightToFromID();

                if (ReturnedValues.FromID == -1 && ReturnedValues.ToID == -1)
                {
                    LeftSide.Enabled = false;
                    AmbilightSelectLeftSideCheckbox.IsChecked = false;
                }
                else
                {
                    LeftSide.FromID = ReturnedValues.FromID;
                    LeftSide.ToID = ReturnedValues.ToID;
                }
            }
        }

        private void AmbilightSelectLeftSideCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            BottomSide.Enabled = false;
        }

        private async void AmbilightSelectBottomSideCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                FromToIDS ReturnedValues = await GetAmbilightToFromID();

                if (ReturnedValues.FromID == -1 && ReturnedValues.ToID == -1)
                {
                    BottomSide.Enabled = false;
                    AmbilightSelectBottomSideCheckbox.IsChecked = false;
                }
                else
                {
                    BottomSide.FromID = ReturnedValues.FromID;
                    BottomSide.ToID = ReturnedValues.ToID;
                }
            }
        }

        private void AmbilightSelectBottomSideCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            LeftSide.Enabled = false;
        }

        async Task<FromToIDS> GetAmbilightToFromID()
        {
            int Highest = 0;
            int Lowest = 9999;

            SelectingSide = true;

            AmbilightSelectSideGrid.Visibility = Visibility.Visible;

            foreach (UIElement Element in InnerSetupPanel.Children)
            {
                if (Element is Border)
                {
                    Grid NewGrid = new Grid();
                    NewGrid.Margin = (Element as Border).Margin;
                    NewGrid.Width = ((Element as Border).Child as Grid).Width;
                    NewGrid.Height = ((Element as Border).Child as Grid).Height;
                    NewGrid.Tag = (SetupStrip)((Element as Border).Child as Grid).Tag;
                    NewGrid.Background = Brushes.DarkSlateGray;
                    NewGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    NewGrid.VerticalAlignment = VerticalAlignment.Top;

                    CheckBox SelectGridCheckbox = new CheckBox();
                    SelectGridCheckbox.VerticalAlignment = VerticalAlignment.Center;
                    SelectGridCheckbox.HorizontalAlignment = HorizontalAlignment.Center;
                    NewGrid.Children.Add(SelectGridCheckbox);

                    AmbilightSelectSideGridInnerGrid.Children.Add(NewGrid);
                }
            }

            while (SelectingSide)
                await Task.Delay(50);

            if (!CancelSelectingSide)
            {
                foreach (UIElement Element in AmbilightSelectSideGridInnerGrid.Children)
                {
                    if (Element is Grid)
                    {
                        if ((bool)((Element as Grid).Children[0] as CheckBox).IsChecked)
                        {
                            SetupStrip GridTag = (SetupStrip)(Element as Grid).Tag;

                            int FromID = 0;
                            int ToID = 0;
                            if (GridTag.ConnectedFrom <= 0)
                            {
                                if (GridTag.DragingFrom <= 0)
                                {
                                    ToID = (GridTag.FromID + (GridTag.LEDsPrStrip - 1));
                                    FromID = GridTag.FromID;
                                }
                                else
                                {
                                    ToID = GridTag.FromID;
                                    FromID = (GridTag.FromID + (GridTag.LEDsPrStrip - 1));
                                }
                            }
                            else
                            {
                                if (GridTag.DragingFrom <= 0)
                                {
                                    ToID = (GridTag.FromID + (GridTag.LEDsPrStrip - 1));
                                    FromID = GridTag.FromID;
                                }
                                else
                                {
                                    ToID = GridTag.FromID;
                                    FromID = (GridTag.FromID + (GridTag.LEDsPrStrip - 1));
                                }
                            }

                            if (ToID > FromID)
                            {
                                if (FromID < Lowest)
                                    Lowest = FromID;
                                if (ToID > Highest)
                                    Highest = ToID;
                            }
                            else
                            {
                                if (ToID < Lowest)
                                    Lowest = ToID;
                                if (FromID > Highest)
                                    Highest = FromID;
                            }
                        }
                    }
                }
            }
            else
            {
                Lowest = -1;
                Highest = -1;
            }

            CancelSelectingSide = false;

            AmbilightSelectSideGrid.Visibility = Visibility.Hidden;

            AmbilightSelectSideGridInnerGrid.Children.Clear();

            FromToIDS ReturnValues = new FromToIDS(Lowest, Highest);

            return ReturnValues;
        }

        private void AmbilightScreenIDCombobox_DropDownOpened(object sender, EventArgs e)
        {
            Update_AmbilightScreenIDCombobox_Values();
        }

        void Update_AmbilightScreenIDCombobox_Values()
        {
            ScreenList.Clear();
            AmbilightScreenIDCombobox.Items.Clear();
            AmbilightScreenIDCombobox.Items.Add("0 (Virtual Screen)");
            ScreenList.Add(System.Windows.Forms.SystemInformation.VirtualScreen);
            int i = 1;
            foreach (System.Windows.Forms.Screen Rec in System.Windows.Forms.Screen.AllScreens)
            {
                AmbilightScreenIDCombobox.Items.Add(i);
                ScreenList.Add(Rec.Bounds);
                i++;
            }
        }

        public void ShowOrHideBlocks(bool _ShowOrHide, int _ScreenID, int _SampleSplit)
        {
            SetSides();

            if (_ShowOrHide)
            {
                if (BlockList.Count == 0)
                {
                    if (LeftSide.Enabled)
                    {
                        MakeNewBlock(
                            (ScreenList[_ScreenID].Height - LeftSide.Height + LeftSide.YOffSet),
                            LeftSide.YOffSet,
                            LeftSide.Height + LeftSide.BlockSpacing,
                            true,
                            LeftSide.Width,
                            LeftSide.Height,
                            false,
                            ScreenList[_ScreenID].X + LeftSide.XOffSet,
                            ScreenList[_ScreenID].Y,
                            _SampleSplit
                            );
                    }
                    if (TopSide.Enabled)
                    {
                        MakeNewBlock(
                            TopSide.XOffSet,
                            (ScreenList[_ScreenID].Width - TopSide.Width),
                            TopSide.Width + TopSide.BlockSpacing,
                            false,
                            TopSide.Width,
                            TopSide.Height,
                            true,
                            ScreenList[_ScreenID].X,
                            ScreenList[_ScreenID].Y + TopSide.YOffSet,
                            _SampleSplit
                            );
                    }
                    if (RightSide.Enabled)
                    {
                        MakeNewBlock(
                            RightSide.YOffSet,
                            ScreenList[_ScreenID].Height - RightSide.Height,
                            RightSide.Height + RightSide.BlockSpacing,
                            false,
                            RightSide.Width,
                            RightSide.Height,
                            false,
                            (ScreenList[_ScreenID].X + ScreenList[_ScreenID].Width - RightSide.Width) + RightSide.XOffSet,
                            ScreenList[_ScreenID].Y + RightSide.YOffSet,
                            _SampleSplit
                            );
                    }
                    if (BottomSide.Enabled)
                    {
                        MakeNewBlock(
                            (ScreenList[_ScreenID].Width - BottomSide.Width) + BottomSide.XOffSet,
                            0,
                            BottomSide.Width + BottomSide.BlockSpacing,
                            true,
                            BottomSide.Width,
                            BottomSide.Height,
                            true,
                            ScreenList[_ScreenID].X,
                            (ScreenList[_ScreenID].Y + ScreenList[_ScreenID].Height - BottomSide.Height) + BottomSide.YOffSet,
                            _SampleSplit
                            );
                    }
                }
            }
            else
            {
                foreach (Block b in BlockList)
                    b.Close();
                BlockList.Clear();
            }
        }
        private void MakeNewBlock(int _FromI, int _UntilI, int _AddWith, bool _UntilILarger, int _BoxWidth, int _BoxHeight, bool _LocOfI, int _XOffset, int _YOffset, int _PixelSpread)
        {
            if (_UntilILarger)
            {
                for (int i = _FromI; i > _UntilI; i -= _AddWith)
                {
                    MakeNewBoxInner(
                        _BoxWidth,
                        _BoxHeight,
                        _LocOfI,
                        i,
                        _XOffset,
                        _YOffset,
                        _PixelSpread
                        );
                }
            }
            else
            {
                for (int i = _FromI; i < _UntilI; i += _AddWith)
                {
                    MakeNewBoxInner(
                        _BoxWidth,
                        _BoxHeight,
                        _LocOfI,
                        i,
                        _XOffset,
                        _YOffset,
                        _PixelSpread
                        );
                }
            }
        }

        private void MakeNewBoxInner(int _BoxWidth, int _BoxHeight, bool _LocOfI, int _i, int _XOffset, int _YOffset, int _PixelSpread)
        {
            Block NewBlock = new Block();
            NewBlock.Width = _BoxWidth;
            NewBlock.Height = _BoxHeight;
            if (_LocOfI)
            {
                NewBlock.Left = _XOffset + _i;
                NewBlock.Top = _YOffset;
            }
            else
            {
                NewBlock.Left = _XOffset;
                NewBlock.Top = _YOffset + _i;
            }
            if (_PixelSpread > 20)
            {
                for (int j = 0; j < _BoxWidth; j += _PixelSpread)
                {
                    for (int l = 0; l < _BoxHeight; l += _PixelSpread)
                    {
                        Rectangle Pixel = new Rectangle();
                        Pixel.Fill = Brushes.Black;
                        Pixel.Width = 1;
                        Pixel.Height = 1;
                        Pixel.VerticalAlignment = VerticalAlignment.Top;
                        Pixel.HorizontalAlignment = HorizontalAlignment.Left;
                        Pixel.Margin = new Thickness(j, l, 0, 0);
                        NewBlock.MainGrid.Children.Add(Pixel);
                    }
                }
            }
            BlockList.Add(NewBlock);
            NewBlock.Show();
        }

        public void SetSides()
        {
            TopSide.Enabled = (bool)AmbilightSelectTopSideCheckbox.IsChecked;
            TopSide.Width = Convert.ToInt32(AmbilightTopSideBlockWidthTextbox.Text);
            TopSide.Height = Convert.ToInt32(AmbilightTopSideBlockHeightTextbox.Text);
            TopSide.BlockSpacing = Convert.ToInt32(AmbilightTopSideBlockSpacingTextbox.Text);
            TopSide.XOffSet = Convert.ToInt32(AmbilightTopSideBlockOffsetXTextbox.Text);
            TopSide.YOffSet = Convert.ToInt32(AmbilightTopSideBlockOffsetYTextbox.Text);
            TopSide.LEDsPrBlock = Convert.ToInt32(AmbilightTopSideLEDsPrBlockTextbox.Text);

            LeftSide.Enabled = (bool)AmbilightSelectLeftSideCheckbox.IsChecked;
            LeftSide.Width = Convert.ToInt32(AmbilightLeftSideBlockWidthTextbox.Text);
            LeftSide.Height = Convert.ToInt32(AmbilightLeftSideBlockHeightTextbox.Text);
            LeftSide.BlockSpacing = Convert.ToInt32(AmbilightLeftSideBlockSpacingTextbox.Text);
            LeftSide.XOffSet = Convert.ToInt32(AmbilightLeftSideBlockOffsetXTextbox.Text);
            LeftSide.YOffSet = Convert.ToInt32(AmbilightLeftSideBlockOffsetYTextbox.Text);
            LeftSide.LEDsPrBlock = Convert.ToInt32(AmbilightLeftSideLEDsPrBlockTextbox.Text);

            BottomSide.Enabled = (bool)AmbilightSelectBottomSideCheckbox.IsChecked;
            BottomSide.Width = Convert.ToInt32(AmbilightBottomSideBlockWidthTextbox.Text);
            BottomSide.Height = Convert.ToInt32(AmbilightBottomSideBlockHeightTextbox.Text);
            BottomSide.BlockSpacing = Convert.ToInt32(AmbilightBottomSideBlockSpacingTextbox.Text);
            BottomSide.XOffSet = Convert.ToInt32(AmbilightBottomSideBlockOffsetXTextbox.Text);
            BottomSide.YOffSet = Convert.ToInt32(AmbilightBottomSideBlockOffsetYTextbox.Text);
            BottomSide.LEDsPrBlock = Convert.ToInt32(AmbilightBottomSideLEDsPrBlockTextbox.Text);

            RightSide.Enabled = (bool)AmbilightSelectRightSideCheckbox.IsChecked;
            RightSide.Width = Convert.ToInt32(AmbilightRightSideBlockWidthTextbox.Text);
            RightSide.Height = Convert.ToInt32(AmbilightRightSideBlockHeightTextbox.Text);
            RightSide.BlockSpacing = Convert.ToInt32(AmbilightRightSideBlockSpacingTextbox.Text);
            RightSide.XOffSet = Convert.ToInt32(AmbilightRightSideBlockOffsetXTextbox.Text);
            RightSide.YOffSet = Convert.ToInt32(AmbilightRightSideBlockOffsetYTextbox.Text);
            RightSide.LEDsPrBlock = Convert.ToInt32(AmbilightRightSideLEDsPrBlockTextbox.Text);
        }
        private void AmbilightSelectSideGridCancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSelectingSide = true;
            SelectingSide = false;
        }

        private void AmbilightSelectSideGridOKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectingSide = false;
        }

        public async Task AutoSetOffsets(int _ScreenID, int _SampleSplit)
        {
            SetSides();

            this.Opacity = 0;
            System.Drawing.Bitmap Screenshot = new System.Drawing.Bitmap(ScreenList[_ScreenID].Width, ScreenList[_ScreenID].Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (System.Drawing.Graphics GFXScreenshot = System.Drawing.Graphics.FromImage(Screenshot))
            {
                GFXScreenshot.CopyFromScreen(ScreenList[_ScreenID].X, ScreenList[_ScreenID].Y, 0, 0, new System.Drawing.Size(Screenshot.Width, Screenshot.Height), System.Drawing.CopyPixelOperation.SourceCopy);
            }
            this.Opacity = 1;

            if (LeftSide.Enabled)
            {
                AmbilightLeftSideBlockOffsetXTextbox.Text = FindFirstLightPixel(
                    0,
                    ScreenList[_ScreenID].Width / 2,
                    false,
                    true,
                    0,
                    ScreenList[_ScreenID].Height / 2,
                    Screenshot
                    ).ToString();
            }

            if (TopSide.Enabled)
            {
                AmbilightTopSideBlockOffsetYTextbox.Text = FindFirstLightPixel(
                    0,
                    ScreenList[_ScreenID].Height / 2,
                    false,
                    false,
                    ScreenList[_ScreenID].Width / 2,
                    0,
                    Screenshot
                    ).ToString();
            }

            if (RightSide.Enabled)
            {
                AmbilightRightSideBlockOffsetXTextbox.Text = (-(ScreenList[_ScreenID].Width - FindFirstLightPixel(
                    ScreenList[_ScreenID].Width - 1,
                    ScreenList[_ScreenID].Width / 2,
                    true,
                    true,
                    0,
                    ScreenList[_ScreenID].Height / 2,
                    Screenshot
                    ))).ToString();
            }

            if (BottomSide.Enabled)
            {
                AmbilightBottomSideBlockOffsetYTextbox.Text = (-(ScreenList[_ScreenID].Height - FindFirstLightPixel(
                    ScreenList[_ScreenID].Height - 1,
                    ScreenList[_ScreenID].Height / 2,
                    true,
                    false,
                    ScreenList[_ScreenID].Width / 2,
                    -2,
                    Screenshot
                    ))).ToString();
            }
            Screenshot.Dispose();

            SetSides();

            ShowOrHideBlocks(true, _ScreenID, _SampleSplit);

            await Task.Delay(1000);

            ShowOrHideBlocks(false, 0, 0);
        }

        private int FindFirstLightPixel(int _FromI, int _UntilI, bool _UntilILarger, bool _ILoc, int _XOffset, int _YOffset, System.Drawing.Bitmap _ScreenShot)
        {
            if (_UntilILarger)
            {
                for (int i = _FromI; i > _UntilI; i--)
                {
                    System.Drawing.Color Pixel;
                    if (_ILoc)
                        Pixel = _ScreenShot.GetPixel(_XOffset + i, _YOffset);
                    else
                        Pixel = _ScreenShot.GetPixel(_XOffset, _YOffset + i);
                    if (Convert.ToInt32(Pixel.R > 5) + Convert.ToInt32(Pixel.G > 5) + Convert.ToInt32(Pixel.B > 5) > 0)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = _FromI; i < _UntilI; i++)
                {
                    System.Drawing.Color Pixel;
                    if (_ILoc)
                        Pixel = _ScreenShot.GetPixel(_XOffset + i, _YOffset);
                    else
                        Pixel = _ScreenShot.GetPixel(_XOffset, _YOffset + i);
                    if (Convert.ToInt32(Pixel.R > 5) + Convert.ToInt32(Pixel.G > 5) + Convert.ToInt32(Pixel.B > 5) > 0)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        public async Task AutoSetBlockSize(int _ScreenID, int _SampleSplit)
        {
            SetSides();

            if (LeftSide.Enabled)
            {
                try
                {
                    AmbilightLeftSideBlockHeightTextbox.Text = ((int)((ScreenList[_ScreenID].Height - LeftSide.YOffSet) / Math.Round((double)(Math.Abs(LeftSide.ToID - LeftSide.FromID) / LeftSide.LEDsPrBlock), 0) - LeftSide.BlockSpacing)).ToString();
                }
                catch
                {
                    AmbilightLeftSideBlockHeightTextbox.Text = "100";
                }
            }

            if (TopSide.Enabled)
            {
                try
                {
                    AmbilightTopSideBlockWidthTextbox.Text = ((int)((ScreenList[_ScreenID].Width - TopSide.XOffSet) / Math.Round((double)(Math.Abs(TopSide.ToID - TopSide.FromID) / TopSide.LEDsPrBlock), 0) - TopSide.BlockSpacing)).ToString();
                }
                catch
                {
                    AmbilightTopSideBlockWidthTextbox.Text = "100";
                }
            }

            if (RightSide.Enabled)
            {
                try
                {
                    AmbilightRightSideBlockHeightTextbox.Text = ((int)((ScreenList[_ScreenID].Height - RightSide.YOffSet) / Math.Round((double)(Math.Abs(RightSide.ToID - RightSide.FromID) / RightSide.LEDsPrBlock), 0) - RightSide.BlockSpacing)).ToString();
                }
                catch
                {
                    AmbilightRightSideBlockHeightTextbox.Text = "100";
                }
            }

            if (BottomSide.Enabled)
            {
                try
                {
                    AmbilightBottomSideBlockWidthTextbox.Text = ((int)((ScreenList[_ScreenID].Width - BottomSide.XOffSet) / Math.Round((double)(Math.Abs(BottomSide.ToID - BottomSide.FromID) / BottomSide.LEDsPrBlock), 0) - BottomSide.BlockSpacing)).ToString();
                }
                catch
                {
                    AmbilightBottomSideBlockWidthTextbox.Text = "100";
                }
            }

            SetSides();

            ShowOrHideBlocks(true, _ScreenID, _SampleSplit);

            await Task.Delay(1000);

            ShowOrHideBlocks(false, 0 ,0);
        }

        async void StartAmbilight(int _ScreenID, int _SampleSplit, double _GammaValue, double _FadeFactor, int _RefreshRate, bool _SendToAllDevices, string _DeviceName)
        {
            SetSides();

            if (AmbilightTask != null)
                if (AmbilightTask.Status == TaskStatus.Running)
                    StopAmbilight();

            int Highest = 0;
            int Lowest = 0;

            if (LeftSide.Enabled)
                if (LeftSide.FromID < Lowest)
                    Lowest = LeftSide.FromID;

            if (TopSide.Enabled)
                if (TopSide.FromID < Lowest)
                    Lowest = TopSide.FromID;

            if (RightSide.Enabled)
                if (RightSide.FromID < Lowest)
                    Lowest = RightSide.FromID;

            if (BottomSide.Enabled)
                if (BottomSide.FromID < Lowest)
                    Lowest = BottomSide.FromID;

            if (LeftSide.Enabled)
                if (LeftSide.ToID < Lowest)
                    Lowest = LeftSide.ToID;

            if (TopSide.Enabled)
                if (TopSide.ToID < Lowest)
                    Lowest = TopSide.ToID;

            if (RightSide.Enabled)
                if (RightSide.ToID < Lowest)
                    Lowest = RightSide.ToID;

            if (BottomSide.Enabled)
                if (BottomSide.ToID < Lowest)
                    Lowest = BottomSide.ToID;

            if (LeftSide.Enabled)
                if (LeftSide.ToID > Highest)
                    Highest = LeftSide.ToID;

            if (TopSide.Enabled)
                if (TopSide.ToID > Highest)
                    Highest = TopSide.ToID;

            if (RightSide.Enabled)
                if (RightSide.ToID > Highest)
                    Highest = RightSide.ToID;

            if (BottomSide.Enabled)
                if (BottomSide.ToID > Highest)
                    Highest = BottomSide.ToID;

            if (LeftSide.Enabled)
                if (LeftSide.FromID > Highest)
                    Highest = LeftSide.FromID;

            if (TopSide.Enabled)
                if (TopSide.FromID > Highest)
                    Highest = TopSide.FromID;

            if (RightSide.Enabled)
                if (RightSide.FromID > Highest)
                    Highest = RightSide.FromID;

            if (BottomSide.Enabled)
                if (BottomSide.FromID > Highest)
                    Highest = BottomSide.FromID;

            await TransferToDeviceOrDevicesAsync((ModesDeviceSelectionCombobox.SelectedIndex == 0), ModesDeviceSelectionCombobox.SelectedItem.ToString(), new TransferMode.Ranges(Lowest, Highest));

            if (AmbilightColorStore.Count != 4)
            {
                AmbilightColorStore.Clear();
                AmbilightColorStore.Add(new List<List<int>>());
                AmbilightColorStore.Add(new List<List<int>>());
                AmbilightColorStore.Add(new List<List<int>>());
                AmbilightColorStore.Add(new List<List<int>>());
            }

            AssumeLevel = Convert.ToDouble(AmbilightAssumePrCentTextBox.Text);
            MaxVariation = Convert.ToInt32(AmbilightMaxAssumeVariationTextBox.Text);

            AmbilightAdvancedSettingsGrid.IsEnabled = false;
            AmbilightTopSideGrid.IsEnabled = false;
            AmbilightRightSideGrid.IsEnabled = false;
            AmbilightBottomSideGrid.IsEnabled = false;
            AmbilightLeftSideGrid.IsEnabled = false;
            AmbilightStartAmbilightButton.IsEnabled = false;
            AmbilightSelectTopSideCheckbox.IsEnabled = false;
            AmbilightSelectRightSideCheckbox.IsEnabled = false;
            AmbilightSelectBottomSideCheckbox.IsEnabled = false;
            AmbilightSelectLeftSideCheckbox.IsEnabled = false;

            AmbilightTask = new Task(delegate { AmbilightThread(LeftSide, TopSide, RightSide, BottomSide, _ScreenID, _SampleSplit, _GammaValue, _FadeFactor, _RefreshRate, _SendToAllDevices, _DeviceName); });
            AmbilightTask.Start();

            RunAmbilight = true;
        }

        async void StopAmbilight()
        {
            if (AmbilightTask != null)
            {
                RunAmbilight = false;
                while (AmbilightTask.Status == TaskStatus.Running)
                {
                    await Task.Delay(10);
                }
            }
            AmbilightAdvancedSettingsGrid.IsEnabled = true;
            AmbilightTopSideGrid.IsEnabled = true;
            AmbilightRightSideGrid.IsEnabled = true;
            AmbilightBottomSideGrid.IsEnabled = true;
            AmbilightLeftSideGrid.IsEnabled = true;
            AmbilightStartAmbilightButton.IsEnabled = true;
            AmbilightSelectTopSideCheckbox.IsEnabled = true;
            AmbilightSelectRightSideCheckbox.IsEnabled = true;
            AmbilightSelectBottomSideCheckbox.IsEnabled = true;
            AmbilightSelectLeftSideCheckbox.IsEnabled = true;
        }

        private void AmbilightThread(AmbilightSide _LeftSide, AmbilightSide _TopSide, AmbilightSide _RightSide, AmbilightSide _BottomSide, int _ScreenID, int _SampleSplit, double _GammaValue, double _FadeFactor, int _RefreshRate, bool _SendToAllDevices, string _DeviceName)
        {
            DateTime CalibrateRefreshRate = new DateTime();
            int SerialOutLeftSection = 0;
            int SerialOutTopSection = 0;
            int SerialOutRightSection = 0;
            int SerialOutBottomSection = 0;

            int SerialOutLeftSection2 = 0;
            int SerialOutTopSection2 = 0;
            int SerialOutRightSection2 = 0;
            int SerialOutBottomSection2 = 0;

            TransferMode.Ambilight[] SerialOutLeft = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] SerialOutTop = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] SerialOutRight = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] SerialOutBottom = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutLeft = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutTop = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutRight = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutBottom = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };

            TransferMode.Ambilight[] InnerSerialOutLeft2 = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutTop2 = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutRight2 = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };
            TransferMode.Ambilight[] InnerSerialOutBottom2 = new TransferMode.Ambilight[] { new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, ""), new TransferMode.Ambilight(0, 0, 0, "") };

            bool SerialOutLeftReady = false;
            bool SerialOutTopReady = false;
            bool SerialOutRightReady = false;
            bool SerialOutBottomReady = false;

            bool SerialOutLeftReady2 = false;
            bool SerialOutTopReady2 = false;
            bool SerialOutRightReady2 = false;
            bool SerialOutBottomReady2 = false;

            bool AllSendt = true;
            bool ProcessingDoneInnerFlip = true;

            bool ProcessingDoneInnerFlip2 = true;

            System.Drawing.Bitmap ImageWindowLeft = new System.Drawing.Bitmap(_LeftSide.Width, ScreenList[_ScreenID].Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowTop = new System.Drawing.Bitmap(ScreenList[_ScreenID].Width, _TopSide.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowRight = new System.Drawing.Bitmap(_RightSide.Width, ScreenList[_ScreenID].Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowBottom = new System.Drawing.Bitmap(ScreenList[_ScreenID].Width, _BottomSide.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Graphics GFXScreenshotLeft = System.Drawing.Graphics.FromImage(ImageWindowLeft);
            System.Drawing.Graphics GFXScreenshotTop = System.Drawing.Graphics.FromImage(ImageWindowTop);
            System.Drawing.Graphics GFXScreenshotRight = System.Drawing.Graphics.FromImage(ImageWindowRight);
            System.Drawing.Graphics GFXScreenshotBottom = System.Drawing.Graphics.FromImage(ImageWindowBottom);

            System.Drawing.Bitmap ImageWindowLeft2 = new System.Drawing.Bitmap(_LeftSide.Width, ScreenList[_ScreenID].Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowTop2 = new System.Drawing.Bitmap(ScreenList[_ScreenID].Width, _TopSide.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowRight2 = new System.Drawing.Bitmap(_RightSide.Width, ScreenList[_ScreenID].Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Bitmap ImageWindowBottom2 = new System.Drawing.Bitmap(ScreenList[_ScreenID].Width, _BottomSide.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Graphics GFXScreenshotLeft2 = System.Drawing.Graphics.FromImage(ImageWindowLeft2);
            System.Drawing.Graphics GFXScreenshotTop2 = System.Drawing.Graphics.FromImage(ImageWindowTop2);
            System.Drawing.Graphics GFXScreenshotRight2 = System.Drawing.Graphics.FromImage(ImageWindowRight2);
            System.Drawing.Graphics GFXScreenshotBottom2 = System.Drawing.Graphics.FromImage(ImageWindowBottom2);

            int LeftCaptureX = ScreenList[_ScreenID].X + _LeftSide.XOffSet;
            int LeftCaptureY = ScreenList[_ScreenID].Y + _LeftSide.YOffSet;
            int LeftCaptureWidth = _LeftSide.Width;
            int LeftCaptureHeight = ScreenList[_ScreenID].Height;
            int LeftFromI = ScreenList[_ScreenID].Height - _LeftSide.Height;
            int LeftAddIWith = _LeftSide.Height + _LeftSide.BlockSpacing;

            int TopCaptureX = ScreenList[_ScreenID].X + _TopSide.XOffSet;
            int TopCaptureY = ScreenList[_ScreenID].Y + _TopSide.YOffSet;
            int TopCaptureWidth = ScreenList[_ScreenID].Width;
            int TopCaptureHeight = _TopSide.Height;
            int TopUntilI = (ScreenList[_ScreenID].Width - _TopSide.Width);
            int TopAddIWith = _TopSide.Width + _TopSide.BlockSpacing;

            int RightCaptureX = (ScreenList[_ScreenID].X + ScreenList[_ScreenID].Width - _RightSide.Width) + _RightSide.XOffSet;
            int RightCaptureY = ScreenList[_ScreenID].Y + _RightSide.YOffSet;
            int RightCaptureWidth = _RightSide.Width;
            int RightCaptureHeight = ScreenList[_ScreenID].Height;
            int RightUntilI = ScreenList[_ScreenID].Height - _RightSide.Height;
            int RightAddIWith = _RightSide.Height + _RightSide.BlockSpacing;

            int BottomCaptureX = ScreenList[_ScreenID].X + _BottomSide.XOffSet;
            int BottomCaptureY = (ScreenList[_ScreenID].Y + ScreenList[_ScreenID].Height - _BottomSide.Height) + _BottomSide.YOffSet;
            int BottomCaptureWidth = ScreenList[_ScreenID].Width;
            int BottomCaptureHeight = _BottomSide.Height;
            int BottomFromI = ScreenList[_ScreenID].Width - _BottomSide.Width;
            int BottomAddIWith = _BottomSide.Width + _BottomSide.BlockSpacing;

            int WaitTime = -1;

            while (RunAmbilight)
            {
                if (ProcessingDoneInnerFlip)
                {
                    ProcessingDoneInnerFlip = false;

                    SerialOutLeftReady = false;
                    SerialOutTopReady = false;
                    SerialOutRightReady = false;
                    SerialOutBottomReady = false;

                    SerialOutLeftSection = 0;
                    SerialOutTopSection = 0;
                    SerialOutRightSection = 0;
                    SerialOutBottomSection = 0;

                    Array.Clear(InnerSerialOutLeft, 0, InnerSerialOutLeft.Length);
                    Array.Clear(InnerSerialOutTop, 0, InnerSerialOutTop.Length);
                    Array.Clear(InnerSerialOutRight, 0, InnerSerialOutRight.Length);
                    Array.Clear(InnerSerialOutBottom, 0, InnerSerialOutBottom.Length);

                    if (_LeftSide.Enabled)
                    {
                        Task.Run(() =>
                        {
                            SerialOutLeftReady = ProcessSection(
                                InnerSerialOutLeft,
                                GFXScreenshotLeft,
                                ImageWindowLeft,
                                SerialOutLeftSection,
                                LeftCaptureX,
                                LeftCaptureY,
                                LeftCaptureWidth,
                                LeftCaptureHeight,
                                _LeftSide.FromID,
                                _LeftSide.ToID,
                                _LeftSide.LEDsPrBlock,
                                LeftFromI,
                                0,
                                LeftAddIWith,
                                true,
                                _LeftSide.Width,
                                _LeftSide.Height,
                                false,
                                SideID.Left,
                                _FadeFactor,
                                _SampleSplit,
                                _GammaValue
                                );
                        });
                    }
                    else
                        SerialOutLeftReady = true;

                    if (_TopSide.Enabled)
                    {
                        Task.Run(() =>
                        {
                            SerialOutTopReady = ProcessSection(
                                InnerSerialOutTop,
                                GFXScreenshotTop,
                                ImageWindowTop,
                                SerialOutTopSection,
                                TopCaptureX,
                                TopCaptureY,
                                TopCaptureWidth,
                                TopCaptureHeight,
                                _TopSide.FromID,
                                _TopSide.ToID,
                                _TopSide.LEDsPrBlock,
                                0,
                                TopUntilI,
                                TopAddIWith,
                                false,
                                _TopSide.Width,
                                _TopSide.Height,
                                true,
                                SideID.Top,
                                _FadeFactor,
                                _SampleSplit,
                                _GammaValue
                                );
                        });
                    }
                    else
                        SerialOutTopReady = true;

                    if (_RightSide.Enabled)
                    {
                        Task.Run(() =>
                        {
                            SerialOutRightReady = ProcessSection(
                                InnerSerialOutRight,
                                GFXScreenshotRight,
                                ImageWindowRight,
                                SerialOutRightSection,
                                RightCaptureX,
                                RightCaptureY,
                                RightCaptureWidth,
                                RightCaptureHeight,
                                _RightSide.FromID,
                                _RightSide.ToID,
                                _RightSide.LEDsPrBlock,
                                0,
                                RightUntilI,
                                RightAddIWith,
                                false,
                                _RightSide.Width,
                                _RightSide.Height,
                                false,
                                SideID.Right,
                                _FadeFactor,
                                _SampleSplit,
                                _GammaValue
                                );
                        });
                    }
                    else
                        SerialOutRightReady = true;

                    if (_BottomSide.Enabled)
                    {
                        Task.Run(() =>
                        {
                            SerialOutBottomReady = ProcessSection(
                                InnerSerialOutBottom,
                                GFXScreenshotBottom,
                                ImageWindowBottom,
                                SerialOutBottomSection,
                                BottomCaptureX,
                                BottomCaptureY,
                                BottomCaptureWidth,
                                BottomCaptureHeight,
                                _BottomSide.FromID,
                                _BottomSide.ToID,
                                _BottomSide.LEDsPrBlock,
                                BottomFromI,
                                0,
                                BottomAddIWith,
                                true,
                                _BottomSide.Width,
                                _BottomSide.Height,
                                true,
                                SideID.Bottom,
                                _FadeFactor,
                                _SampleSplit,
                                _GammaValue
                                );
                        });
                    }
                    else
                        SerialOutBottomReady = true;

                    if (ProcessingDoneInnerFlip2 && WaitTime != -1)
                    {
                        ProcessingDoneInnerFlip2 = false;

                        SerialOutLeftReady2 = false;
                        SerialOutTopReady2 = false;
                        SerialOutRightReady2 = false;
                        SerialOutBottomReady2 = false;

                        SerialOutLeftSection2 = 0;
                        SerialOutTopSection2 = 0;
                        SerialOutRightSection2 = 0;
                        SerialOutBottomSection2 = 0;

                        Array.Clear(InnerSerialOutLeft2, 0, InnerSerialOutLeft2.Length);
                        Array.Clear(InnerSerialOutTop2, 0, InnerSerialOutTop2.Length);
                        Array.Clear(InnerSerialOutRight2, 0, InnerSerialOutRight2.Length);
                        Array.Clear(InnerSerialOutBottom2, 0, InnerSerialOutBottom2.Length);

                        int WaitTimeInner = WaitTime;
                        WaitTime = -1;

                        if (_LeftSide.Enabled)
                        {
                            Task.Run(() =>
                            {
                                Thread.Sleep(WaitTimeInner);
                                SerialOutLeftReady2 = ProcessSection(
                                    InnerSerialOutLeft2,
                                    GFXScreenshotLeft2,
                                    ImageWindowLeft2,
                                    SerialOutLeftSection2,
                                    LeftCaptureX,
                                    LeftCaptureY,
                                    LeftCaptureWidth,
                                    LeftCaptureHeight,
                                    _LeftSide.FromID,
                                    _LeftSide.ToID,
                                    _LeftSide.LEDsPrBlock,
                                    LeftFromI,
                                    0,
                                    LeftAddIWith,
                                    true,
                                    _LeftSide.Width,
                                    _LeftSide.Height,
                                    false,
                                    SideID.Left,
                                    _FadeFactor,
                                    _SampleSplit,
                                    _GammaValue
                                    );
                            });
                        }
                        else
                            SerialOutLeftReady2 = true;

                        if (_TopSide.Enabled)
                        {
                            Task.Run(() =>
                            {
                                Thread.Sleep(WaitTimeInner);
                                SerialOutTopReady2 = ProcessSection(
                                    InnerSerialOutTop2,
                                    GFXScreenshotTop2,
                                    ImageWindowTop2,
                                    SerialOutTopSection2,
                                    TopCaptureX,
                                    TopCaptureY,
                                    TopCaptureWidth,
                                    TopCaptureHeight,
                                    _TopSide.FromID,
                                    _TopSide.ToID,
                                    _TopSide.LEDsPrBlock,
                                    0,
                                    TopUntilI,
                                    TopAddIWith,
                                    false,
                                    _TopSide.Width,
                                    _TopSide.Height,
                                    true,
                                    SideID.Top,
                                    _FadeFactor,
                                    _SampleSplit,
                                    _GammaValue
                                    );
                            });
                        }
                        else
                            SerialOutTopReady2 = true;

                        if (_RightSide.Enabled)
                        {
                            Task.Run(() =>
                            {
                                Thread.Sleep(WaitTimeInner);
                                SerialOutRightReady2 = ProcessSection(
                                    InnerSerialOutRight2,
                                    GFXScreenshotRight2,
                                    ImageWindowRight2,
                                    SerialOutRightSection2,
                                    RightCaptureX,
                                    RightCaptureY,
                                    RightCaptureWidth,
                                    RightCaptureHeight,
                                    _RightSide.FromID,
                                    _RightSide.ToID,
                                    _RightSide.LEDsPrBlock,
                                    0,
                                    RightUntilI,
                                    RightAddIWith,
                                    false,
                                    _RightSide.Width,
                                    _RightSide.Height,
                                    false,
                                    SideID.Right,
                                    _FadeFactor,
                                    _SampleSplit,
                                    _GammaValue
                                    );
                            });
                        }
                        else
                            SerialOutRightReady2 = true;

                        if (_BottomSide.Enabled)
                        {
                            Task.Run(() =>
                            {
                                Thread.Sleep(WaitTimeInner);
                                SerialOutBottomReady2 = ProcessSection(
                                    InnerSerialOutBottom2,
                                    GFXScreenshotBottom2,
                                    ImageWindowBottom2,
                                    SerialOutBottomSection2,
                                    BottomCaptureX,
                                    BottomCaptureY,
                                    BottomCaptureWidth,
                                    BottomCaptureHeight,
                                    _BottomSide.FromID,
                                    _BottomSide.ToID,
                                    _BottomSide.LEDsPrBlock,
                                    BottomFromI,
                                    0,
                                    BottomAddIWith,
                                    true,
                                    _BottomSide.Width,
                                    _BottomSide.Height,
                                    true,
                                    SideID.Bottom,
                                    _FadeFactor,
                                    _SampleSplit,
                                    _GammaValue
                                    );
                            });
                        }
                        else
                            SerialOutBottomReady2 = true;
                    }
                }

                if (AllSendt && (SerialOutLeftReady && SerialOutTopReady && SerialOutRightReady && SerialOutBottomReady) | (SerialOutLeftReady2 && SerialOutTopReady2 && SerialOutRightReady2 && SerialOutBottomReady2))
                {
                    if ((DateTime.Now - AmbilightFPSCounter).TotalSeconds >= 1)
                    {
                        Dispatcher.Invoke(() => { AmbilightFPSLabel.Content = "FPS: " + AmbilightFPSCounterFramesRendered; });
                        AmbilightFPSCounterFramesRendered = 0;
                        AmbilightFPSCounter = DateTime.Now;
                    }

                    AllSendt = false;
                    if (SerialOutLeftReady && SerialOutTopReady && SerialOutRightReady && SerialOutBottomReady)
                    {
                        WaitTime = (DateTime.Now.Millisecond - CalibrateRefreshRate.Millisecond) / 2;
                        if (WaitTime < 0)
                            WaitTime = 0;
                        ProcessingDoneInnerFlip = true;
                        for (int i = 0; i < 5; i++)
                        {
                            SerialOutLeft[i] = InnerSerialOutLeft[i];
                            SerialOutTop[i] = InnerSerialOutTop[i];
                            SerialOutRight[i] = InnerSerialOutRight[i];
                            SerialOutBottom[i] = InnerSerialOutBottom[i];
                        }
                    }
                    if (SerialOutLeftReady2 && SerialOutTopReady2 && SerialOutRightReady2 && SerialOutBottomReady2)
                    {
                        ProcessingDoneInnerFlip2 = true;
                        for (int i = 0; i < 5; i++)
                        {
                            SerialOutLeft[i] = InnerSerialOutLeft2[i];
                            SerialOutTop[i] = InnerSerialOutTop2[i];
                            SerialOutRight[i] = InnerSerialOutRight2[i];
                            SerialOutBottom[i] = InnerSerialOutBottom2[i];
                        }
                    }

                    Task.Run(() =>
                    {
                        if (_LeftSide.Enabled)
                        {
                            for (int i = 0; i < 5; i++)
                                if (SerialOutLeft[i] != null)
                                    if (SerialOutLeft[i].Values != null)
                                        TransferToDeviceOrDevices(_SendToAllDevices, _DeviceName, SerialOutLeft[i]);
                        }
                        if (_TopSide.Enabled)
                        {
                            for (int i = 0; i < 5; i++)
                                if (SerialOutTop[i] != null)
                                    if (SerialOutTop[i].Values != null)
                                        TransferToDeviceOrDevices(_SendToAllDevices, _DeviceName, SerialOutTop[i]);
                        }
                        if (_RightSide.Enabled)
                        {
                            for (int i = 0; i < 5; i++)
                                if (SerialOutRight[i] != null)
                                    if (SerialOutRight[i].Values != null)
                                        TransferToDeviceOrDevices(_SendToAllDevices, _DeviceName, SerialOutRight[i]);
                        }
                        if (_BottomSide.Enabled)
                            for (int i = 0; i < 5; i++)
                                if (SerialOutBottom[i] != null)
                                    if (SerialOutBottom[i].Values != null)
                                        TransferToDeviceOrDevices(_SendToAllDevices, _DeviceName, SerialOutBottom[i]);

                        AllSendt = true;

                        AmbilightFPSCounterFramesRendered++;
                    });

                    int ExectuionTime = (int)(DateTime.Now - CalibrateRefreshRate).TotalMilliseconds;
                    int ActuralRefreshTime = _RefreshRate - ExectuionTime;

                    if (ActuralRefreshTime < 0)
                        ActuralRefreshTime = 0;

                    Thread.Sleep(ActuralRefreshTime);

                    CalibrateRefreshRate = DateTime.Now;
                }
            }

            while (!(SerialOutLeftReady && SerialOutTopReady && SerialOutRightReady && SerialOutBottomReady) && !(SerialOutLeftReady2 && SerialOutTopReady2 && SerialOutRightReady2 && SerialOutBottomReady2))
                Thread.Sleep(100);

            Thread.Sleep(1000);

            GFXScreenshotLeft.Dispose();
            GFXScreenshotTop.Dispose();
            GFXScreenshotRight.Dispose();
            GFXScreenshotBottom.Dispose();
            GFXScreenshotLeft2.Dispose();
            GFXScreenshotTop2.Dispose();
            GFXScreenshotRight2.Dispose();
            GFXScreenshotBottom2.Dispose();

            ImageWindowLeft.Dispose();
            ImageWindowTop.Dispose();
            ImageWindowRight.Dispose();
            ImageWindowBottom.Dispose();
            ImageWindowLeft2.Dispose();
            ImageWindowTop2.Dispose();
            ImageWindowRight2.Dispose();
            ImageWindowBottom2.Dispose();
        }

        private bool ProcessSection(
            TransferMode.Ambilight[] InnerSerialOut,
            System.Drawing.Graphics _GFXScreenShot,
            System.Drawing.Bitmap _ImageWindow,
            int _SectionIndex,
            int _CaptureX,
            int _CaptureY,
            int _CaptureWidth,
            int _CaptureHeight,
            int _FromID,
            int _ToID,
            int _PixelsPrBlock,
            int _FromI,
            int _UntilI,
            int _AddToI,
            bool _WhileILarger,
            int _ProcessColorWidth,
            int _ProcessColorHeight,
            bool _ILoc,
            SideID _SideID,
            double _FadeFactor,
            int _AddBy,
            double _GammaValue
            )
        {
            _GFXScreenShot.CopyFromScreen(_CaptureX, _CaptureY, 0, 0, new System.Drawing.Size(_CaptureWidth, _CaptureHeight), System.Drawing.CopyPixelOperation.SourceCopy);

            int Count = 0;
            InnerSerialOut[_SectionIndex] = new TransferMode.Ambilight(_FromID, _ToID, _PixelsPrBlock, "");
            if (_WhileILarger)
            {
                for (int i = _FromI; i > _UntilI; i -= _AddToI)
                {
                    _SectionIndex = ProcessSectionInner(
                        _ILoc,
                        _ImageWindow,
                        _ProcessColorWidth,
                        _ProcessColorHeight,
                        i,
                        _SideID,
                        Count,
                        InnerSerialOut,
                        _SectionIndex,
                        _FromID,
                        _ToID,
                        _PixelsPrBlock,
                        _FadeFactor,
                        _AddBy,
                        _GammaValue
                        );
                    Count++;
                }
            }
            else
            {
                for (int i = _FromI; i < _UntilI; i += _AddToI)
                {
                    _SectionIndex = ProcessSectionInner(
                        _ILoc,
                        _ImageWindow,
                        _ProcessColorWidth,
                        _ProcessColorHeight,
                        i,
                        _SideID,
                        Count,
                        InnerSerialOut,
                        _SectionIndex,
                        _FromID,
                        _ToID,
                        _PixelsPrBlock,
                        _FadeFactor,
                        _AddBy,
                        _GammaValue
                        );
                    Count++;
                }
            }

            return true;
        }

        private int ProcessSectionInner(
            bool _ILoc,
            System.Drawing.Bitmap _ImageWindow,
            int _ProcessColorWidth,
            int _ProcessColorHeight,
            int _i,
            SideID _SideID,
            int _Count,
            TransferMode.Ambilight[] _InnerSerialOut,
            int _SectionIndex,
            int _FromID,
            int _ToID,
            int _PixelsPrBlock,
            double _FadeFactor,
            int _AddBy,
            double _GammaValue
            )
        {
            Color OutPutColor;
            if (_ILoc)
                OutPutColor = GetColorOfSection(_ImageWindow, _ProcessColorWidth, _ProcessColorHeight, _i, 0, _AddBy);
            else
                OutPutColor = GetColorOfSection(_ImageWindow, _ProcessColorWidth, _ProcessColorHeight, 0, _i, _AddBy);
            if (_FadeFactor != 0)
            {
                int RedValue;
                int GreenValue;
                int BlueValue;
                if (AmbilightColorStore[(int)_SideID].Count == _Count)
                {
                    AmbilightColorStore[(int)_SideID].Add(new List<int> { OutPutColor.R, OutPutColor.G, OutPutColor.B });
                    RedValue = OutPutColor.R;
                    GreenValue = OutPutColor.G;
                    BlueValue = OutPutColor.B;
                }
                else
                {
                    RedValue = AmbilightColorStore[(int)_SideID][_Count][0] + (int)(((double)OutPutColor.R - (double)AmbilightColorStore[(int)_SideID][_Count][0]) * _FadeFactor);
                    if (RedValue > 255)
                        RedValue = 255;
                    if (RedValue < 0)
                        RedValue = 0;
                    AmbilightColorStore[(int)_SideID][_Count][0] = RedValue;
                    GreenValue = AmbilightColorStore[(int)_SideID][_Count][1] + (int)(((double)OutPutColor.G - (double)AmbilightColorStore[(int)_SideID][_Count][1]) * _FadeFactor);
                    if (GreenValue > 255)
                        GreenValue = 255;
                    if (GreenValue < 0)
                        GreenValue = 0;
                    AmbilightColorStore[(int)_SideID][_Count][1] = GreenValue;
                    BlueValue = AmbilightColorStore[(int)_SideID][_Count][2] + (int)(((double)OutPutColor.B - (double)AmbilightColorStore[(int)_SideID][_Count][2]) * _FadeFactor);
                    if (BlueValue > 255)
                        BlueValue = 255;
                    if (BlueValue < 0)
                        BlueValue = 0;
                    AmbilightColorStore[(int)_SideID][_Count][2] = BlueValue;
                }
                OutPutColor = GammaCorrection(Color.FromArgb(255, (byte)RedValue, (byte)GreenValue, (byte)BlueValue), _GammaValue);
            }
            else
            {
                OutPutColor = GammaCorrection(OutPutColor, _GammaValue);
            }

            int RVal = (int)Math.Round((decimal)8 / ((decimal)255 / (OutPutColor.R + 1)), 0) + 1;
            int GVal = (int)Math.Round((decimal)8 / ((decimal)255 / (OutPutColor.G + 1)), 0) + 1;
            int BVal = (int)Math.Round((decimal)8 / ((decimal)255 / (OutPutColor.B + 1)), 0) + 1;

            string AddString = "";

            if (RVal == GVal && GVal == BVal)
            {
                AddString = RVal.ToString() + ";";
            }
            else
            {
                if (RVal == GVal && RVal != 0)
                {
                    AddString = RVal.ToString() + BVal.ToString() + ";";
                }
                else
                {
                    AddString = RVal.ToString() + GVal.ToString() + BVal.ToString() + ";";
                }
            }

            if (AddString.Length + _InnerSerialOut[_SectionIndex].Values.Length > 75)
            {
                string[] ChangeToIDString = _InnerSerialOut[_SectionIndex].Values.Split(';');
                if (_FromID < _ToID)
                    _InnerSerialOut[_SectionIndex].ToID = (_FromID + (_Count * _PixelsPrBlock));
                else
                    _InnerSerialOut[_SectionIndex].ToID = (_FromID - (_Count * _PixelsPrBlock));

                _SectionIndex++;

                if (_FromID < _ToID)
                    _InnerSerialOut[_SectionIndex] = new TransferMode.Ambilight((_FromID + (_Count * _PixelsPrBlock)), _ToID, _PixelsPrBlock, "");
                else
                    _InnerSerialOut[_SectionIndex] = new TransferMode.Ambilight((_FromID - (_Count * _PixelsPrBlock)), _ToID, _PixelsPrBlock, "");
            }
            _InnerSerialOut[_SectionIndex].Values += AddString;
            return _SectionIndex;
        }

        private Color GetColorOfSection(System.Drawing.Bitmap _InputImage, int _Width, int _Height, int _Xpos, int _Ypos, int _AddBy)
        {
            int Count = 0;
            int AvgR = 0;
            int AvgG = 0;
            int AvgB = 0;

            int High = 0;
            int Low = 765;

            int CountTarget = (int)(((_Height / _AddBy) * (_Width / _AddBy)) * AssumeLevel);

            for (int y = _Ypos; y < _Ypos + _Height; y += _AddBy)
            {
                for (int x = _Xpos; x < _Xpos + _Width; x += _AddBy)
                {
                    System.Drawing.Color Pixel = _InputImage.GetPixel(x, y);
                    AvgR += Pixel.R;
                    AvgG += Pixel.G;
                    AvgB += Pixel.B;
                    if ((AvgR + AvgG + AvgB) > High)
                        High = (AvgR + AvgG + AvgB);
                    if ((AvgR + AvgG + AvgB) < Low)
                        Low = (AvgR + AvgG + AvgB);
                    if (CountTarget == Count)
                        if (High - Low <= MaxVariation)
                            break;
                    Count++;
                }
            }

            AvgR = AvgR / Count;
            AvgG = AvgG / Count;
            AvgB = AvgB / Count;

            return Color.FromArgb(255, (byte)AvgR, (byte)AvgG, (byte)AvgB);
        }

        #endregion

        #region IndividualColorsRegion

        private void ModesIndividualColorsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region InstructionsRegion

        private void ModesInstructionsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region ServerRegion
        private void ModesServerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region AnimationRegion

        private void ModesAnimationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region SettingsRegion

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralConfigs();

            HideAllInnerGrids();
            HideAllSideBars();
            HideAllSubMenus();
            SettingsSideBar.Visibility = Visibility.Visible;
        }

        private void SettingsAdvancedSettingsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
                SaveApplicationConfig(ModesDeviceSelectionCombobox.SelectedItem.ToString());

            ChangeVisibilityByUID(MainGrid, Visibility.Visible, "AdvancedSetting");
            ChangeVisibilityByUID(MainGrid, Visibility.Visible, "VisualAdvancedSetting");

            if (!Loading)
                LoadAdvancedApplicationConfigsForDevice(ModesDeviceSelectionCombobox.SelectedItem.ToString());
        }

        private void SettingsAdvancedSettingsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!Loading)
                SaveApplicationConfig(ModesDeviceSelectionCombobox.SelectedItem.ToString());

            ChangeVisibilityByUID(MainGrid, Visibility.Hidden, "AdvancedSetting");
            ChangeVisibilityByUID(MainGrid, Visibility.Hidden, "VisualAdvancedSetting");

            if (!Loading)
                ResetAllAdvancedSettingsToDefault();
        }

        private void HelpSideBarWikiButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/kris701/ArduLED/wiki");
        }

        private void HelpSideBarForumButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/kris701/ArduLED/issues");
        }

        private void HelpSideBarContactButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/kris701/ArduLED/issues");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralConfigs();

            HideAllInnerGrids();
            HideAllSideBars();
            HideAllSubMenus();
            HelpSideBar.Visibility = Visibility.Visible;
        }

        private void SettingsSideBarConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllInnerGrids();
            ConnectionGrid.Visibility = Visibility.Visible;
        }

        private void SettingsSideBarStartupButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllInnerGrids();
            SettingsStartupGrid.Visibility = Visibility.Visible;
        }

        private void SettingsSideBarOtherButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region SetupRegion

        void PlaceLineByFromToID(int ID, int From, int ToID)
        {
            Grid FoundGrid = FindGridByID(ID);
            if (FoundGrid != null)
            {
                Border FoundGridParentBorder = FoundGrid.Parent as Border;
                SetupStrip GridTag = (SetupStrip)FoundGrid.Tag;
                Line GridLine = GridTag.ConnectLineObject as Line;
                Polygon SenderArrow = GridLine.Tag as Polygon;

                if (GridTag.FlipDir == FlipDir.Up)
                {
                    if (From > 0)
                    {
                        GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                        GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                    }
                    else
                    {
                        GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                        GridLine.Y1 = FoundGridParentBorder.Margin.Top + FoundGrid.Height - ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                    }
                }
                else
                {
                    if (GridTag.FlipDir == FlipDir.Right)
                    {
                        if (From > 0)
                        {
                            GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                            GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                        }
                        else
                        {
                            GridLine.X1 = FoundGridParentBorder.Margin.Left + FoundGrid.Width - ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                            GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                        }
                    }
                    else
                    {
                        if (GridTag.FlipDir == FlipDir.Down)
                        {
                            if (From > 0)
                            {
                                GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                                GridLine.Y1 = FoundGridParentBorder.Margin.Top + FoundGrid.Height - ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                            }
                            else
                            {
                                GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                                GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                            }
                        }
                        else
                        {
                            if (GridTag.FlipDir == FlipDir.Left)
                            {
                                if (From > 0)
                                {
                                    GridLine.X1 = FoundGridParentBorder.Margin.Left + FoundGrid.Width - ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                                    GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                                }
                                else
                                {
                                    GridLine.X1 = FoundGridParentBorder.Margin.Left + ButtonWidth / 2 + FoundGridParentBorder.BorderThickness.Left;
                                    GridLine.Y1 = FoundGridParentBorder.Margin.Top + ButtonHeight / 2 + FoundGridParentBorder.BorderThickness.Top;
                                }
                            }
                        }
                    }
                }

                Grid ConnectToGrid = FindGridByID(Math.Abs(ToID));
                if (ConnectToGrid != null)
                {
                    Border ConnectToGridParentBorder = ConnectToGrid.Parent as Border;
                    SetupStrip ConnectToGridTag = (SetupStrip)ConnectToGrid.Tag;

                    if (ConnectToGridTag.FlipDir == FlipDir.Up)
                    {
                        if (ToID > 0)
                        {
                            GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                            GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                        }
                        else
                        {
                            GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                            GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ConnectToGrid.Height - ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                        }
                    }
                    else
                    {
                        if (ConnectToGridTag.FlipDir == FlipDir.Right)
                        {
                            if (ToID > 0)
                            {
                                GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                            }
                            else
                            {
                                GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ConnectToGrid.Width - ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                            }
                        }
                        else
                        {
                            if (ConnectToGridTag.FlipDir == FlipDir.Down)
                            {
                                if (ToID > 0)
                                {
                                    GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                    GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ConnectToGrid.Height - ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                                }
                                else
                                {
                                    GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                    GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                                }
                            }
                            else
                            {
                                if (ConnectToGridTag.FlipDir == FlipDir.Left)
                                {
                                    if (ToID > 0)
                                    {
                                        GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ConnectToGrid.Width - ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                        GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                                    }
                                    else
                                    {
                                        GridLine.X2 = ConnectToGridParentBorder.Margin.Left + ButtonWidth / 2 + ConnectToGridParentBorder.BorderThickness.Left;
                                        GridLine.Y2 = ConnectToGridParentBorder.Margin.Top + ButtonHeight / 2 + ConnectToGridParentBorder.BorderThickness.Top;
                                    }
                                }
                            }
                        }
                    }

                    SetArrowByLine(SenderArrow, GridLine.X1, GridLine.Y1, GridLine.X2, GridLine.Y2);

                    GridLine.Stroke = Brushes.Green;
                    SenderArrow.Stroke = Brushes.Green;
                    SenderArrow.Fill = Brushes.Green;
                    GridLine.StrokeThickness = 3;
                    GridLine.Visibility = Visibility.Visible;
                    SenderArrow.Visibility = Visibility.Visible;
                }
            }
        }

        void SetArrowByLine(Polygon SenderPolygon, double RefPointX, double RefPointY, double ToPointX, double ToPointY)
        {
            double DiffAngle = Math.Atan2(RefPointX - ToPointX, RefPointY - ToPointY);
            double DistanceForTwoInnerPoints = Math.Sqrt(Math.Pow(RefPointX - ToPointX, 2) + Math.Pow(RefPointY - ToPointY, 2)) - SetupLineDragingArrowWidth;
            double SpreadAngle = SetupLineDragingArrowWidth / DistanceForTwoInnerPoints;
            double OffSetX1 = Math.Sin(DiffAngle - SpreadAngle) * DistanceForTwoInnerPoints;
            double OffSetY1 = Math.Cos(DiffAngle - SpreadAngle) * DistanceForTwoInnerPoints;
            double OffSetX2 = Math.Sin(DiffAngle + SpreadAngle) * DistanceForTwoInnerPoints;
            double OffSetY2 = Math.Cos(DiffAngle + SpreadAngle) * DistanceForTwoInnerPoints;

            SenderPolygon.Points[0] = new Point(ToPointX, ToPointY);
            SenderPolygon.Points[1] = new Point(RefPointX - OffSetX1, RefPointY - OffSetY1);
            SenderPolygon.Points[2] = new Point(RefPointX - OffSetX2, RefPointY - OffSetY2);
        }

        private void SetupFlipButton_Click(object sender, RoutedEventArgs e)
        {
            Button SenderButton = sender as Button;
            Grid SenderGrid = SenderButton.Parent as Grid;
            Border SenderGridBorder = SenderGrid.Parent as Border;

            int SenderWidth = (int)SenderGrid.Width;
            int SenderHeight = (int)SenderGrid.Height;
            SenderGrid.Width = SenderHeight;
            SenderGrid.Height = SenderWidth;

            SetupStrip NewGridTag = (SetupStrip)SenderGrid.Tag;

            if (NewGridTag.FlipDir == FlipDir.Up)
            {
                FlipGridDir(SenderGrid);
                NewGridTag.FlipDir = FlipDir.Right;
            }
            else
            {
                if (NewGridTag.FlipDir == FlipDir.Right)
                {
                    FlipGridDir(SenderGrid);
                    NewGridTag.FlipDir = FlipDir.Down;
                }
                else
                {
                    if (NewGridTag.FlipDir == FlipDir.Down)
                    {
                        FlipGridDir(SenderGrid);
                        NewGridTag.FlipDir = FlipDir.Left;
                    }
                    else
                    {
                        if (NewGridTag.FlipDir == FlipDir.Left)
                        {
                            FlipGridDir(SenderGrid);
                            NewGridTag.FlipDir = FlipDir.Up;
                        }
                    }
                }
            }

            SenderGrid.Tag = NewGridTag;
        }

        void FlipGridDir(Grid SenderGrid)
        {
            if (SenderGrid.RowDefinitions.Count > 3)
            {
                int Amount = SenderGrid.RowDefinitions.Count;
                SenderGrid.RowDefinitions.Clear();
                SenderGrid.RowDefinitions.Add(new RowDefinition());
                SenderGrid.RowDefinitions.Add(new RowDefinition());
                SenderGrid.ColumnDefinitions.Clear();
                for (int i = 0; i < Amount; i++)
                    SenderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                foreach (UIElement InnerControl in SenderGrid.Children)
                {
                    int PreRow = Grid.GetRow(InnerControl);
                    int PreCol = Grid.GetColumn(InnerControl);
                    Grid.SetColumn(InnerControl, PreRow);
                    Grid.SetRow(InnerControl, PreCol);
                    if (InnerControl is Label)
                    {
                        Label InnerPinLabel = InnerControl as Label;
                        if (InnerPinLabel.Name == "PinLabel")
                        {
                            InnerPinLabel.Content = "PIN " + InnerPinLabel.Content.ToString().Split(' ')[1];
                        }
                    }
                }
            }
            else
            {
                if (SenderGrid.ColumnDefinitions.Count > 3)
                {
                    int Amount = SenderGrid.ColumnDefinitions.Count;
                    SenderGrid.ColumnDefinitions.Clear();
                    SenderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    SenderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    SenderGrid.RowDefinitions.Clear();
                    for (int i = 0; i < Amount; i++)
                        SenderGrid.RowDefinitions.Add(new RowDefinition());
                    foreach (UIElement InnerControl in SenderGrid.Children)
                    {
                        int PreRow = Grid.GetRow(InnerControl);
                        int PreCol = Grid.GetColumn(InnerControl) + (Grid.GetRowSpan(InnerControl) - 1);
                        Grid.SetColumn(InnerControl, PreRow);
                        Grid.SetRow(InnerControl, (Amount - 1) - PreCol);
                        if (InnerControl is Label)
                        {
                            Label InnerPinLabel = InnerControl as Label;
                            if (InnerPinLabel.Name == "PinLabel")
                            {
                                InnerPinLabel.Content = "PIN" + Environment.NewLine + " " + InnerPinLabel.Content.ToString().Split(' ')[1];
                            }
                        }
                    }
                }
            }
        }

        private void SetupRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button SenderButton = sender as Button;
            Grid SenderGrid = SenderButton.Parent as Grid;

            SetupStrip NewGridTag = (SetupStrip)SenderGrid.Tag;

            Line SenderLine = NewGridTag.ConnectLineObject as Line;

            InnerSetupPanel.Children.Remove(SenderLine);

            Border SenderGridBorder = SenderGrid.Parent as Border;
            InnerSetupPanel.Children.Remove(SenderGridBorder);
        }

        private void SetupSideBarAddStripButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetupSubMenu.Visibility == Visibility.Visible)
                SetupSubMenu.Visibility = Visibility.Hidden;
            else
                SetupSubMenu.Visibility = Visibility.Visible;
        }
        private void SetupAddStripAddStripButton_Click(object sender, RoutedEventArgs e)
        {
            StripIDCount[Int32.Parse(SetupAddStripPinIDTextBox.Text)] = Int32.Parse(SetupAddStripFromIDTextBox.Text) + Int32.Parse(SetupAddStripLEDsOnStripTextBox.Text);
            MakeNewStrip(IDCount, 0, 0, 0, 0, new Point(100, 100), Int32.Parse(SetupAddStripLEDsOnStripTextBox.Text), Int32.Parse(SetupAddStripFromIDTextBox.Text), Int32.Parse(SetupAddStripPinIDTextBox.Text), FlipDir.Up);
            IDCount++;
        }

        private void SetupSideBarSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Setups";
            if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveSetup(SaveFileDialog.FileName);
            }
        }

        private void SetupSideBarLoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Setups";
            if (LoadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadSetup(LoadFileDialog.FileName);
            }
        }

        private async void SetupSideBarSendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendSetupOrSetups((DeviceSelectionCombobox.SelectedIndex == 0), FindDeviceIndexByName(DeviceSelectionCombobox.SelectedItem.ToString()));
        }
        
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralConfigs();

            HideAllInnerGrids();
            HideAllSideBars();
            HideAllSubMenus();
            SetupGrid.Visibility = Visibility.Visible;
            SetupSideBar.Visibility = Visibility.Visible;
        }
        async Task SendSetupOrSetups(bool All, int Index)
        {
            if (All)
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    LoadSetup(DeviceList[i].SetupSaveFileName);
                    await SendSetup(DeviceList[i]);
                }
            }
            else
            {
                await SendSetup(DeviceList[Index]);
            }
        }

        async Task SendSetup(TransferDevice Device)
        {
            await ConnectToDeviceOrDevices(false, FindDeviceIndexByName(Device.DeviceName));

            SetupSideBarTransferSetupProgressBar.MaxHeight = StripIDCount.Count + InnerSetupPanel.Children.Count + DeviceList.Count;
            SetupSideBarTransferSetupProgressBar.Value = 0;
            int TotalLEDCount = 0;
            int SoftwareIDCount = 0;

            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode(" "));

            for (int i = 0; i < StripIDCount.Count; i++)
            {
                if (StripIDCount[i] != 0)
                {
                    TotalLEDCount += StripIDCount[i];
                    await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode(StripIDCount[i] + ";" + i));
                }
                SetupSideBarTransferSetupProgressBar.Value++;
            }

            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode("9999"));

            List<int> IDSeriesList = new List<int>();

            Grid StartGrid = FindGridByID(GetFirstInSeriesID());
            if (StartGrid != null)
            {
                SetupStrip FindTags = (SetupStrip)StartGrid.Tag;
                Label FromIDLabel = FindChildByName(StartGrid, "SoftwareIDLabelA") as Label;
                Label DirLabelA = FindChildByName(StartGrid, "DirLabelA") as Label;
                Label ToIDLabel = FindChildByName(StartGrid, "SoftwareIDLabelB") as Label;
                Label DirLabelB = FindChildByName(StartGrid, "DirLabelB") as Label;
                while (true)
                {
                    DirLabelA.Content = SetArrowFromPointDir(FindTags.FlipDir, FindTags.ConnectedFrom, FindTags.DragingFrom);
                    DirLabelB.Content = DirLabelA.Content;
                    if (FindTags.ConnectedFrom <= 0)
                    {
                        ToIDLabel.Content = SoftwareIDCount;
                        FromIDLabel.Content = SoftwareIDCount + FindTags.LEDsPrStrip - 1;
                        if (FindTags.DragingFrom <= 0)
                        {
                            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode(FindTags.FromID + ";" + (FindTags.FromID + (FindTags.LEDsPrStrip - 1)) + ";" + FindTags.PinID));
                        }
                        else
                        {
                            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode((FindTags.FromID + (FindTags.LEDsPrStrip - 1)) + ";" + FindTags.FromID + ";" + FindTags.PinID));
                        }
                    }
                    else
                    {
                        FromIDLabel.Content = SoftwareIDCount;
                        ToIDLabel.Content = SoftwareIDCount + FindTags.LEDsPrStrip - 1;
                        if (FindTags.DragingFrom <= 0)
                        {
                            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode(FindTags.FromID + ";" + (FindTags.FromID + (FindTags.LEDsPrStrip - 1)) + ";" + FindTags.PinID));
                        }
                        else
                        {
                            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode((FindTags.FromID + (FindTags.LEDsPrStrip - 1)) + ";" + FindTags.FromID + ";" + FindTags.PinID));
                        }
                    }
                    SoftwareIDCount += FindTags.LEDsPrStrip;
                    if (FindTags.ConnectedToID == 0)
                        break;
                    StartGrid = FindGridByID(Math.Abs(FindTags.ConnectedToID)) as Grid;
                    FromIDLabel = FindChildByName(StartGrid, "SoftwareIDLabelA") as Label;
                    DirLabelA = FindChildByName(StartGrid, "DirLabelA") as Label;
                    ToIDLabel = FindChildByName(StartGrid, "SoftwareIDLabelB") as Label;
                    DirLabelB = FindChildByName(StartGrid, "DirLabelB") as Label;
                    FindTags = (SetupStrip)StartGrid.Tag;
                    SetupSideBarTransferSetupProgressBar.Value++;
                }

                await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.NoneMode("9999"));
            }

            SetDeviceTotalLEDCount(FindDeviceIndexByName(Device.DeviceName), TotalLEDCount);

            SetupSideBarTransferSetupProgressBar.Value = SetupSideBarTransferSetupProgressBar.Maximum;
            await Task.Delay(1000);
            SetupSideBarTransferSetupProgressBar.Value = 0;

            await TransferToDeviceOrDevicesAsync(false, Device.DeviceName, new TransferMode.Ranges(0, -1));

            SaveGeneralConfigs();
        }

        string SetArrowFromPointDir(FlipDir _SenderFlipDir, int _ConnectedFrom, int _DraggingFrom)
        {
            if (_SenderFlipDir == FlipDir.Up)
            {
                if (_DraggingFrom != 0)
                {
                    if (_DraggingFrom >= 0)
                        return "\u2191";
                    else
                        return "\u2193";
                }
                else
                {
                    if (_DraggingFrom >= 0)
                        return "\u2193";
                    else
                        return "\u2191";
                }
            }
            else
            {
                if (_SenderFlipDir == FlipDir.Right)
                {
                    if (_DraggingFrom != 0)
                    {
                        if (_DraggingFrom >= 0)
                            return "\u2190";
                        else
                            return "\u2192";
                    }
                    else
                    {
                        if (_DraggingFrom >= 0)
                            return "\u2192";
                        else
                            return "\u2190";
                    }
                }
                else
                {
                    if (_SenderFlipDir == FlipDir.Down)
                    {
                        if (_DraggingFrom != 0)
                        {
                            if (_DraggingFrom >= 0)
                                return "\u2193";
                            else
                                return "\u2191";
                        }
                        else
                        {
                            if (_DraggingFrom >= 0)
                                return "\u2191";
                            else
                                return "\u2193";
                        }
                    }
                    else
                    {
                        if (_SenderFlipDir == FlipDir.Left)
                        {
                            if (_DraggingFrom != 0)
                            {
                                if (_DraggingFrom >= 0)
                                    return "\u2192";
                                else
                                    return "\u2190";
                            }
                            else
                            {
                                if (_DraggingFrom >= 0)
                                    return "\u2190";
                                else
                                    return "\u2192";
                            }
                        }
                    }
                }
            }
            return "O";
        }

        int GetFirstInSeriesID()
        {
            for (int i = 0; i < IDCount; i++)
            {
                Grid SenderGrid = FindGridByID(i);
                if (SenderGrid != null)
                {
                    SetupStrip GridTag = (SetupStrip)SenderGrid.Tag;
                    if (GridTag.ConnectedFromID == 0)
                    {
                        return GridTag.ID;
                    }
                }
            }
            return 0;
        }

        void MakeNewStrip(int ID, int ConnectedToID, int ConnectedFromID, int ConnectedFrom, int DraginFrom, Point Location, int LEDsPrStrip, int FromID, int PinID, FlipDir FlipDir)
        {
            Border NewGridBorder = new Border();
            NewGridBorder.BorderBrush = Brushes.Black;
            NewGridBorder.BorderThickness = new Thickness(2, 2, 2, 2);
            NewGridBorder.HorizontalAlignment = HorizontalAlignment.Left;
            NewGridBorder.VerticalAlignment = VerticalAlignment.Top;
            NewGridBorder.Margin = new Thickness(Location.X, Location.Y, 0, 0);

            Grid NewGrid = new Grid();
            NewGrid.Width = ButtonWidth * 2;
            NewGrid.Height = (ButtonHeight * 4) + ButtonHeight * LEDsPrStrip;
            NewGrid.Background = Brushes.DarkSlateGray;
            NewGrid.ShowGridLines = false;
            for (int i = 0; i < LEDsPrStrip + 4; i++)
                NewGrid.RowDefinitions.Add(new RowDefinition());
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NewGrid.MouseDown += SetupGrid_MouseDown;
            NewGrid.MouseMove += SetupGrid_MouseMove;
            NewGrid.MouseUp += SetupGrid_MouseUp;

            SetupStrip NewGridTag = new SetupStrip(ID, ConnectedToID, ConnectedFromID, ConnectedFrom, DraginFrom, Location, LEDsPrStrip, FromID, PinID, FlipDir.Up, null, null);

            NewGridBorder.Child = NewGrid;

            Ellipse IOShapeA = new Ellipse();
            IOShapeA.Stroke = Brushes.Red;
            IOShapeA.Fill = Brushes.Blue;
            IOShapeA.StrokeThickness = 3;
            IOShapeA.Width = IOShapeSize;
            IOShapeA.Height = IOShapeSize;
            IOShapeA.HorizontalAlignment = HorizontalAlignment.Center;
            IOShapeA.VerticalAlignment = VerticalAlignment.Center;
            IOShapeA.MouseDown += SetupLineGrid_MouseDown;
            IOShapeA.MouseMove += SetupGrid_MouseMove;
            IOShapeA.MouseUp += SetupGrid_MouseUp;
            IOShapeA.Tag = 1;
            Grid.SetRow(IOShapeA, 0);
            Grid.SetColumn(IOShapeA, 0);
            NewGrid.Children.Add(IOShapeA);

            Button FlipButton = new Button();
            FlipButton.Name = "SetupFlipButton";
            FlipButton.Content = "F";
            FlipButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            FlipButton.VerticalAlignment = VerticalAlignment.Stretch;
            FlipButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            FlipButton.VerticalContentAlignment = VerticalAlignment.Center;
            FlipButton.Click += SetupFlipButton_Click;
            FlipButton.Tag = "Up";
            FlipButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetRow(FlipButton, 1);
            Grid.SetColumn(FlipButton, 0);
            NewGrid.Children.Add(FlipButton);

            Button RemoveButton = new Button();
            RemoveButton.Content = "X";
            RemoveButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            RemoveButton.VerticalAlignment = VerticalAlignment.Stretch;
            RemoveButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            RemoveButton.VerticalContentAlignment = VerticalAlignment.Center;
            RemoveButton.Click += SetupRemoveButton_Click;
            RemoveButton.Style = Resources["HoverStyleSideBarCenterText"] as Style;
            Grid.SetRow(RemoveButton, 2);
            Grid.SetColumn(RemoveButton, 0);
            NewGrid.Children.Add(RemoveButton);

            for (int i = 0; i < LEDsPrStrip; i++)
            {
                Label NewLab = new Label();
                NewLab.Content = FromID + i;
                NewLab.HorizontalAlignment = HorizontalAlignment.Stretch;
                NewLab.VerticalAlignment = VerticalAlignment.Stretch;
                NewLab.HorizontalContentAlignment = HorizontalAlignment.Center;
                NewLab.VerticalContentAlignment = VerticalAlignment.Center;
                NewLab.Foreground = Brushes.White;
                Grid.SetRow(NewLab, i + 3);
                Grid.SetColumn(NewLab, 0);
                NewGrid.Children.Add(NewLab);
            }

            Ellipse IOShapeB = new Ellipse();
            IOShapeB.Stroke = Brushes.Green;
            IOShapeB.Fill = Brushes.Blue;
            IOShapeB.StrokeThickness = 3;
            IOShapeB.Width = IOShapeSize;
            IOShapeB.Height = IOShapeSize;
            IOShapeB.HorizontalAlignment = HorizontalAlignment.Center;
            IOShapeB.VerticalAlignment = VerticalAlignment.Center;
            IOShapeB.MouseDown += SetupLineGrid_MouseDown;
            IOShapeB.MouseMove += SetupGrid_MouseMove;
            IOShapeB.MouseUp += SetupGrid_MouseUp;
            IOShapeB.Tag = -1;
            Grid.SetRow(IOShapeB, LEDsPrStrip + 3);
            Grid.SetColumn(IOShapeB, 0);
            NewGrid.Children.Add(IOShapeB);

            Label PinLabel = new Label();
            PinLabel.Name = "PinLabel";
            PinLabel.FontSize = 10;
            PinLabel.Content = "PIN" + Environment.NewLine + " " + PinID;
            PinLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
            PinLabel.VerticalAlignment = VerticalAlignment.Stretch;
            PinLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            PinLabel.VerticalContentAlignment = VerticalAlignment.Center;
            PinLabel.Foreground = Brushes.White;
            Grid.SetRow(PinLabel, 1);
            Grid.SetColumn(PinLabel, 1);
            Grid.SetRowSpan(PinLabel, 2);
            Grid.SetColumnSpan(PinLabel, 2);
            NewGrid.Children.Add(PinLabel);

            Label DirLabelA = new Label();
            DirLabelA.Name = "DirLabelA";
            DirLabelA.FontSize = 11;
            DirLabelA.Content = "O";
            DirLabelA.HorizontalAlignment = HorizontalAlignment.Stretch;
            DirLabelA.VerticalAlignment = VerticalAlignment.Stretch;
            DirLabelA.HorizontalContentAlignment = HorizontalAlignment.Center;
            DirLabelA.VerticalContentAlignment = VerticalAlignment.Center;
            DirLabelA.Foreground = Brushes.White;
            Grid.SetRow(DirLabelA, 0);
            Grid.SetColumn(DirLabelA, 1);
            NewGrid.Children.Add(DirLabelA);

            Label DirLabelB = new Label();
            DirLabelB.Name = "DirLabelB";
            DirLabelB.FontSize = 11;
            DirLabelB.Content = "O";
            DirLabelB.HorizontalAlignment = HorizontalAlignment.Stretch;
            DirLabelB.VerticalAlignment = VerticalAlignment.Stretch;
            DirLabelB.HorizontalContentAlignment = HorizontalAlignment.Center;
            DirLabelB.VerticalContentAlignment = VerticalAlignment.Center;
            DirLabelB.Foreground = Brushes.White;
            Grid.SetRow(DirLabelB, LEDsPrStrip + 3);
            Grid.SetColumn(DirLabelB, 1);
            NewGrid.Children.Add(DirLabelB);

            Label SoftwareIDLabelA = new Label();
            SoftwareIDLabelA.Name = "SoftwareIDLabelA";
            SoftwareIDLabelA.FontSize = 10;
            SoftwareIDLabelA.Content = "-";
            SoftwareIDLabelA.HorizontalAlignment = HorizontalAlignment.Stretch;
            SoftwareIDLabelA.VerticalAlignment = VerticalAlignment.Stretch;
            SoftwareIDLabelA.HorizontalContentAlignment = HorizontalAlignment.Center;
            SoftwareIDLabelA.VerticalContentAlignment = VerticalAlignment.Center;
            SoftwareIDLabelA.Foreground = Brushes.White;
            Grid.SetRow(SoftwareIDLabelA, 3);
            Grid.SetColumn(SoftwareIDLabelA, 1);
            Grid.SetRowSpan(SoftwareIDLabelA, 2);
            Grid.SetColumnSpan(SoftwareIDLabelA, 2);
            NewGrid.Children.Add(SoftwareIDLabelA);

            Label SoftwareIDLabelB = new Label();
            SoftwareIDLabelB.Name = "SoftwareIDLabelB";
            SoftwareIDLabelB.FontSize = 10;
            SoftwareIDLabelB.Content = "-";
            SoftwareIDLabelB.HorizontalAlignment = HorizontalAlignment.Stretch;
            SoftwareIDLabelB.VerticalAlignment = VerticalAlignment.Stretch;
            SoftwareIDLabelB.HorizontalContentAlignment = HorizontalAlignment.Center;
            SoftwareIDLabelB.VerticalContentAlignment = VerticalAlignment.Center;
            SoftwareIDLabelB.Foreground = Brushes.White;
            Grid.SetRow(SoftwareIDLabelB, LEDsPrStrip + 1);
            Grid.SetColumn(SoftwareIDLabelB, 1);
            Grid.SetRowSpan(SoftwareIDLabelB, 2);
            Grid.SetColumnSpan(SoftwareIDLabelB, 2);
            NewGrid.Children.Add(SoftwareIDLabelB);

            while (NewGridTag.FlipDir != FlipDir)
            {
                int SenderWidth = (int)NewGrid.Width;
                int SenderHeight = (int)NewGrid.Height;
                NewGrid.Width = SenderHeight;
                NewGrid.Height = SenderWidth;

                if (NewGridTag.FlipDir == FlipDir.Up)
                {
                    FlipGridDir(NewGrid);
                    NewGridTag.FlipDir = FlipDir.Right;
                }
                else
                {
                    if (NewGridTag.FlipDir == FlipDir.Right)
                    {
                        FlipGridDir(NewGrid);
                        NewGridTag.FlipDir = FlipDir.Down;
                    }
                    else
                    {
                        if (NewGridTag.FlipDir == FlipDir.Down)
                        {
                            FlipGridDir(NewGrid);
                            NewGridTag.FlipDir = FlipDir.Left;
                        }
                        else
                        {
                            if (NewGridTag.FlipDir == FlipDir.Left)
                            {
                                FlipGridDir(NewGrid);
                                NewGridTag.FlipDir = FlipDir.Up;
                            }
                        }
                    }
                }
            }

            InnerSetupPanel.Children.Add(NewGridBorder);

            Polygon ConnectArrow = new Polygon();
            ConnectArrow.Points.Add(new Point(-1, 0));
            ConnectArrow.Points.Add(new Point(1, 0));
            ConnectArrow.Points.Add(new Point(0, 1));
            ConnectArrow.Fill = Brushes.Red;
            ConnectArrow.Stroke = Brushes.Red;
            NewGridTag.ConnectLineArrowObject = ConnectArrow;
            InnerSetupPanel.Children.Add(ConnectArrow);

            Line ConnectLine = new Line();
            ConnectLine.Stroke = Brushes.Black;
            ConnectLine.X1 = 1;
            ConnectLine.Y1 = 1;
            ConnectLine.X2 = 2;
            ConnectLine.Y2 = 2;
            ConnectLine.StrokeThickness = 5;
            ConnectLine.Cursor = Cursors.Cross;
            ConnectLine.Visibility = Visibility.Hidden;
            ConnectLine.Tag = ConnectArrow;
            NewGridTag.ConnectLineObject = ConnectLine;
            InnerSetupPanel.Children.Add(ConnectLine);

            NewGrid.Tag = NewGridTag;

            SetupAddStripFromIDTextBox.Text = (FromID + LEDsPrStrip).ToString();
        }

        private void SetupGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetupDragingObject = sender;
            Grid SenderGrid = sender as Grid;
            Point MousePos = SenderGrid.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
            SetupDragPoint = MousePos;
            SetupDraging = true;
        }

        private void SetupGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (SetupLineDraging)
            {
                Line SenderLine = SetupLineDragingObject as Line;
                Polygon SenderArrow = SenderLine.Tag as Polygon;
                Ellipse SenderEllipse = SetupIODragingObject as Ellipse;
                Grid SenderGrid = SenderEllipse.Parent as Grid;
                Border SenderGridBorder = SenderGrid.Parent as Border;
                Point MousePos = InnerSetupPanel.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                SenderLine.Stroke = Brushes.Red;
                if (SenderLine.Visibility == Visibility.Hidden)
                {
                    SenderLine.Visibility = Visibility.Visible;
                    SenderArrow.Visibility = Visibility.Visible;
                    InnerSetupPanel.Children.Remove(SenderLine);
                    InnerSetupPanel.Children.Add(SenderLine);
                    InnerSetupPanel.Children.Remove(SenderArrow);
                    InnerSetupPanel.Children.Add(SenderArrow);
                }
                Point LocationInGrid = SenderGrid.TranslatePoint(new Point(0, 0), SenderEllipse);
                if (MousePos.X > 0)
                {
                    if (MousePos.Y > 0)
                    {
                        SenderLine.X1 = SenderGridBorder.Margin.Left + (-LocationInGrid.X) + ButtonWidth / 2 - SenderGridBorder.BorderThickness.Left;
                        SenderLine.Y1 = SenderGridBorder.Margin.Top + (-LocationInGrid.Y) + ButtonHeight / 2 - SenderGridBorder.BorderThickness.Top;
                        SenderLine.X2 = MousePos.X;
                        SenderLine.Y2 = MousePos.Y;

                        SetArrowByLine(SenderArrow, SenderLine.X1, SenderLine.Y1, MousePos.X, MousePos.Y);
                    }
                }
            }
            else
            {
                if (SetupDraging)
                {
                    Grid SenderGrid = SetupDragingObject as Grid;
                    SetupStrip GridTag = (SetupStrip)SenderGrid.Tag;

                    if (GridTag.ConnectedFromID != 0)
                    {
                        Grid FoundByIDGrid = FindGridByID(GridTag.ConnectedFromID);
                        if (FoundByIDGrid != null)
                        {
                            SetupStrip InnerGridTag = (SetupStrip)FoundByIDGrid.Tag;
                            InnerGridTag.ConnectedToID = 0;
                            InnerGridTag.ConnectedFrom = 0;
                            InnerGridTag.DragingFrom = 0;
                            FoundByIDGrid.Tag = InnerGridTag;

                            Line SenderLine = InnerGridTag.ConnectLineObject as Line;
                            Polygon SenderArrow = SenderLine.Tag as Polygon;
                            SenderLine.Visibility = Visibility.Hidden;
                            SenderArrow.Visibility = Visibility.Hidden;
                        }

                        GridTag.ConnectedFromID = 0;
                    }
                    if (GridTag.ConnectedToID != 0)
                    {
                        Line SenderLine = GridTag.ConnectLineObject as Line;
                        Polygon SenderArrow = SenderLine.Tag as Polygon;
                        SenderLine.Visibility = Visibility.Hidden;
                        SenderArrow.Visibility = Visibility.Hidden;

                        Grid FoundByIDGrid = FindGridByID(GridTag.ConnectedToID);
                        if (FoundByIDGrid != null)
                        {
                            SetupStrip InnerGridTag = (SetupStrip)FoundByIDGrid.Tag;
                            InnerGridTag.ConnectedFrom = 0;
                            InnerGridTag.ConnectedFromID = 0;
                            FoundByIDGrid.Tag = InnerGridTag;
                        }

                        GridTag.ConnectedToID = 0;
                    }

                    GridTag.DragingFrom = 0;
                    GridTag.ConnectedFrom = 0;

                    SenderGrid.Tag = GridTag;

                    Border SenderGridBorder = SenderGrid.Parent as Border;
                    Point MousePos = InnerSetupPanel.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                    if ((MousePos.X - SetupDragPoint.X) > 0)
                        if ((MousePos.Y - SetupDragPoint.Y) > 0)
                            SenderGridBorder.Margin = new Thickness(
                                (MousePos.X - SetupDragPoint.X),
                                (MousePos.Y - SetupDragPoint.Y),
                                0,
                                0);
                }
            }
        }

        private void SetupGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SetupDraging)
            {
                SetupDraging = false;
                SetupDragingObject = null;
            }
            if (SetupLineDraging)
            {
                Ellipse SenderEllipse = SetupIODragingObject as Ellipse;
                Grid SenderGrid = SenderEllipse.Parent as Grid;
                Border SenderGridBorder = SenderGrid.Parent as Border;
                SetupStrip GridTag = (SetupStrip)SenderGrid.Tag;
                Line SenderLine = SetupLineDragingObject as Line;
                Polygon SenderArrow = SenderLine.Tag as Polygon;
                SenderLine.Visibility = Visibility.Hidden;
                SenderArrow.Visibility = Visibility.Hidden;

                bool ConnectionFound = false;

                foreach (UIElement InnerInnerElement in InnerSetupPanel.Children)
                {
                    if (ConnectionFound)
                        break;
                    if (InnerInnerElement is Border)
                    {
                        if (InnerInnerElement as Border != SenderGridBorder)
                        {
                            Border SenderGridBorderInner = InnerInnerElement as Border;
                            Grid SenderGridInner = SenderGridBorderInner.Child as Grid;
                            SetupStrip GridTagInner = (SetupStrip)SenderGridInner.Tag;
                            Point MousePos = SenderGridInner.PointFromScreen(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));

                            if (GridTagInner.FlipDir == FlipDir.Up)
                            {
                                if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                {
                                    if (MousePos.X > 0 && MousePos.X < ButtonWidth)
                                    {
                                        if (GridTagInner.DragingFrom <= 0)
                                        {
                                            ConnectionFound = true;
                                            GridTag.ConnectedToID = GridTagInner.ID;
                                            GridTagInner.ConnectedFrom = 1;
                                            GridTagInner.ConnectedFromID = GridTag.ID;
                                            SenderGridInner.Tag = GridTagInner;
                                        }
                                    }
                                }
                                if (MousePos.Y < SenderGridInner.Height && MousePos.Y > SenderGridInner.Height - ButtonHeight)
                                {
                                    if (MousePos.X > 0 && MousePos.X <= ButtonWidth)
                                    {
                                        if (GridTagInner.DragingFrom >= 0)
                                        {
                                            ConnectionFound = true;
                                            GridTag.ConnectedToID = -GridTagInner.ID;
                                            GridTagInner.ConnectedFrom = -1;
                                            GridTagInner.ConnectedFromID = GridTag.ID;
                                            SenderGridInner.Tag = GridTagInner;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (GridTagInner.FlipDir == FlipDir.Right)
                                {
                                    if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                    {
                                        if (MousePos.X > 0 && MousePos.X < ButtonWidth)
                                        {
                                            if (GridTagInner.DragingFrom <= 0)
                                            {
                                                ConnectionFound = true;
                                                GridTag.ConnectedToID = GridTagInner.ID;
                                                GridTagInner.ConnectedFrom = 1;
                                                GridTagInner.ConnectedFromID = GridTag.ID;
                                                SenderGridInner.Tag = GridTagInner;
                                            }
                                        }
                                    }
                                    if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                    {
                                        if (MousePos.X < SenderGridInner.Width && MousePos.X > SenderGridInner.Width - ButtonWidth)
                                        {
                                            if (GridTagInner.DragingFrom >= 0)
                                            {
                                                ConnectionFound = true;
                                                GridTag.ConnectedToID = -GridTagInner.ID;
                                                GridTagInner.ConnectedFrom = -1;
                                                GridTagInner.ConnectedFromID = GridTag.ID;
                                                SenderGridInner.Tag = GridTagInner;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (GridTagInner.FlipDir == FlipDir.Down)
                                    {
                                        if (MousePos.Y < SenderGridInner.Height && MousePos.Y > SenderGridInner.Height - ButtonHeight)
                                        {
                                            if (MousePos.X > 0 && MousePos.X < ButtonWidth)
                                            {
                                                if (GridTagInner.DragingFrom <= 0)
                                                {
                                                    ConnectionFound = true;
                                                    GridTag.ConnectedToID = GridTagInner.ID;
                                                    GridTagInner.ConnectedFrom = 1;
                                                    GridTagInner.ConnectedFromID = GridTag.ID;
                                                    SenderGridInner.Tag = GridTagInner;
                                                }
                                            }
                                        }
                                        if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                        {
                                            if (MousePos.X > 0 && MousePos.X <= ButtonWidth)
                                            {
                                                if (GridTagInner.DragingFrom >= 0)
                                                {
                                                    ConnectionFound = true;
                                                    GridTag.ConnectedToID = -GridTagInner.ID;
                                                    GridTagInner.ConnectedFrom = -1;
                                                    GridTagInner.ConnectedFromID = GridTag.ID;
                                                    SenderGridInner.Tag = GridTagInner;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (GridTagInner.FlipDir == FlipDir.Left)
                                        {
                                            if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                            {
                                                if (MousePos.X < SenderGridInner.Width && MousePos.X > SenderGridInner.Width - ButtonWidth)
                                                {
                                                    if (GridTagInner.DragingFrom <= 0)
                                                    {
                                                        ConnectionFound = true;
                                                        GridTag.ConnectedToID = GridTagInner.ID;
                                                        GridTagInner.ConnectedFrom = 1;
                                                        GridTagInner.ConnectedFromID = GridTag.ID;
                                                        SenderGridInner.Tag = GridTagInner;
                                                    }
                                                }
                                            }
                                            if (MousePos.Y > 0 && MousePos.Y < ButtonHeight)
                                            {
                                                if (MousePos.X > 0 && MousePos.X < ButtonWidth)
                                                {
                                                    if (GridTagInner.DragingFrom >= 0)
                                                    {
                                                        ConnectionFound = true;
                                                        GridTag.ConnectedToID = -GridTagInner.ID;
                                                        GridTagInner.ConnectedFrom = -1;
                                                        GridTagInner.ConnectedFromID = GridTag.ID;
                                                        SenderGridInner.Tag = GridTagInner;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (ConnectionFound)
                {
                    PlaceLineByFromToID(GridTag.ID, GridTag.DragingFrom, GridTag.ConnectedToID);
                }

                SenderGrid.Tag = GridTag;
                SetupLineDraging = false;
                SetupLineDragingObject = null;
            }
        }

        private void SetupLineGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse SenderEllipse = sender as Ellipse;
            Grid SenderGrid = SenderEllipse.Parent as Grid;
            SetupStrip NewGridTag = (SetupStrip)SenderGrid.Tag;
            if (NewGridTag.ConnectedFrom != (int)SenderEllipse.Tag)
            {
                NewGridTag.ConnectedToID = 0;
                NewGridTag.DragingFrom = (int)SenderEllipse.Tag;
                Line SenderLine = NewGridTag.ConnectLineObject as Line;
                SenderLine.Stroke = Brushes.Red;
                SenderLine.StrokeThickness = 5;
                SetupLineDragingObject = NewGridTag.ConnectLineObject;
                SetupIODragingObject = SenderEllipse;
                SetupLineDraging = true;
                SenderGrid.Tag = NewGridTag;
            }
        }

        #endregion

        #region GeneralPurposeFunctions

        UIElement FindChildByName(Grid _SenderGrid, string _Name)
        {
            foreach (UIElement Element in _SenderGrid.Children)
            {
                if (Element is Label)
                    if ((Element as Label).Name == _Name)
                        return Element;
                if (Element is Button)
                    if ((Element as Button).Name == _Name)
                        return Element;
                if (Element is Ellipse)
                    if ((Element as Ellipse).Name == _Name)
                        return Element;
            }
            return null;
        }

        void ChangeVisibilityByUID(Grid _StartingGrid, Visibility _Visibility, string _UID)
        {
            foreach (UIElement Element in GetAllControls(_StartingGrid))
            {
                if (Element.Uid == _UID)
                {
                    Element.Visibility = _Visibility;
                }
            }
        }

        void ResetAllAdvancedSettingsToDefault()
        {
            foreach (string Line in DefaultAdvancedValues)
            {
                LoadElement(Line);
            }
        }
        void SetDeviceDeviceName(int _Index, string _NewName)
        {
            DeviceList[_Index] = new TransferDevice(
                _NewName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceIsWireless(int _Index, bool _NewIsWireless)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                _NewIsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceIPAddress(int _Index, IPAddress _NewIPAddress)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                _NewIPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDevicePort(int _Index, int _NewPort)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                _NewPort,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceCOMPortName(int _Index, string _NewCOMPortName)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                _NewCOMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceBaudRate(int _Index, int _NewBaudRate)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                _NewBaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceSetupSaveFileName(int _Index, string _NewSetupSaveFileName)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                _NewSetupSaveFileName,
                DeviceList[_Index].TotalLEDCount);
        }

        void SetDeviceTotalLEDCount(int _Index, int _NewTotalLEDCount)
        {
            DeviceList[_Index] = new TransferDevice(
                DeviceList[_Index].DeviceName,
                DeviceList[_Index].SourceGrid,
                DeviceList[_Index].IsWireless,
                DeviceList[_Index].IPAddress,
                DeviceList[_Index].Port,
                DeviceList[_Index].COMPortName,
                DeviceList[_Index].BaudRate,
                DeviceList[_Index].Device,
                DeviceList[_Index].SetupSaveFileName,
                _NewTotalLEDCount);
        }

        int FindDeviceIndexByName(string _DeviceName)
        {
            for (int i = 0; i < DeviceList.Count; i++)
            {
                if (DeviceList[i].DeviceName == _DeviceName)
                {
                    return i;
                }
            }
            return 0;
        }

        int FindDeviceIndexByParentGrid(Grid _SenderGrid)
        {
            for (int i = 0; i < DeviceList.Count; i++)
            {
                if (DeviceList[i].SourceGrid == _SenderGrid)
                {
                    return i;
                }
            }
            return 0;
        }

        List<UIElement> GetAllControls(UIElement SenderControl)
        {
            List<UIElement> ReturnCollection = new List<UIElement>();
            if (SenderControl is Grid)
            {
                foreach (UIElement InnerControl in (SenderControl as Grid).Children)
                {
                    if (InnerControl is Grid)
                    {
                        List<UIElement> InnerReturnCollection = GetAllControls(InnerControl);
                        foreach (UIElement InnerInnerControl in InnerReturnCollection)
                            ReturnCollection.Add(InnerInnerControl);
                        ReturnCollection.Add(InnerControl);
                    }
                    else
                    {
                        ReturnCollection.Add(InnerControl);
                    }
                }
            }
            return ReturnCollection;
        }
        async Task SetRanges(int _FromID, int _ToID)
        {
            TransferMode Ranges = new TransferMode.Ranges(0, -1);
            if (ModesDeviceSelectionCombobox.SelectedIndex != 0)
            {
                Ranges = new TransferMode.Ranges(_FromID, _ToID);
            }
            await TransferToDeviceOrDevicesAsync((ModesDeviceSelectionCombobox.SelectedIndex == 0), ModesDeviceSelectionCombobox.SelectedItem.ToString(), Ranges);
        }

        async Task TransferToDeviceOrDevicesAsync(bool SendToAll, string DeviceName, TransferMode Data)
        {
            if (SendToAll)
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    await DeviceList[i].Device.WriteAsync(Data);
                }
            }
            else
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    if (DeviceList[i].DeviceName == DeviceName)
                    {
                        await DeviceList[i].Device.WriteAsync(Data);
                        break;
                    }
                }
            }
        }

        void TransferToDeviceOrDevices(bool SendToAll, string DeviceName, TransferMode Data)
        {
            if (SendToAll)
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    DeviceList[i].Device.Write(Data);
                }
            }
            else
            {
                for (int i = 0; i < DeviceList.Count; i++)
                {
                    if (DeviceList[i].DeviceName == DeviceName)
                    {
                        DeviceList[i].Device.Write(Data);
                        break;
                    }
                }
            }
        }

        private Color GammaCorrection(Color _InputColor, double _GammaValue)
        {
            int OutColorR = (int)(Math.Pow((double)_InputColor.R / (double)255, _GammaValue) * 255 + 0.5);
            if (OutColorR > 255)
                OutColorR = 0;
            if (OutColorR < 0)
                OutColorR = 0;

            int OutColorG = (int)(Math.Pow((double)_InputColor.G / (double)255, _GammaValue) * 255 + 0.5);
            if (OutColorG > 255)
                OutColorG = 0;
            if (OutColorG < 0)
                OutColorG = 0;

            int OutColorB = (int)(Math.Pow((double)_InputColor.B / (double)255, _GammaValue) * 255 + 0.5);
            if (OutColorB > 255)
                OutColorB = 0;
            if (OutColorB < 0)
                OutColorB = 0;

            return Color.FromArgb(255, (byte)OutColorR, (byte)OutColorG, (byte)OutColorB);
        }

        private void SetTextBoxToOnlyNumbers(object sender, TextChangedEventArgs e)
        {
            string OutString = "";
            TextBox SenderTextBox = sender as TextBox;
            foreach (char Cha in SenderTextBox.Text)
            {
                if (Cha >= '0' && Cha <= '9' || Cha == ',' && SenderTextBox.Text.Count(f => f == ',') == 1)
                {
                    OutString += Cha;
                }
            }
            SenderTextBox.Text = OutString;
            SenderTextBox.SelectionLength = 0;
            SenderTextBox.SelectionStart = SenderTextBox.Text.Length;
        }

        private void GetAllCOMPorts(object sender, EventArgs e)
        {
            ComboBox SenderComboBox = sender as ComboBox;
            SenderComboBox.Items.Clear();

            foreach (string Port in SerialPort.GetPortNames())
                SenderComboBox.Items.Add(Port);
        }

        Grid FindGridByID(int _ID)
        {
            foreach (UIElement InnerElement in InnerSetupPanel.Children)
            {
                if (InnerElement is Border)
                {
                    Border SenderBorder = InnerElement as Border;
                    Grid SenderGrid = SenderBorder.Child as Grid;
                    SetupStrip GridTag = (SetupStrip)SenderGrid.Tag;

                    if (_ID == Math.Abs(GridTag.ID))
                        return SenderGrid;
                }
            }
            return null;
        }

        #endregion

        #region LoadingAndSavingRegion
        void LoadSetup(string Location)
        {
            try
            {
                InnerSetupPanel.Children.Clear();
                StripIDCount = new List<int>(new int[50]);

                CurrentSaveFileName = Location.Split('\\')[Location.Split('\\').Length - 1].Replace(".txt", "");

                string[] Lines = File.ReadAllLines(Location, Encoding.UTF8);
                for (int i = 0; i < Lines.Length; i++)
                {
                    string[] Split = Lines[i].Split(';');
                    if (Split.Length > 0)
                    {
                        if (Split[0] == "S")
                        {
                            for (int j = 1; j < Split.Length; j++)
                            {
                                if (Split[j] != "")
                                {
                                    int ID = Int32.Parse(Split[j].Split(':')[0]);
                                    int From = Int32.Parse(Split[j].Split(':')[1].Split('>')[0]);
                                    int ToID = Int32.Parse(Split[j].Split(':')[1].Split('>')[1]);

                                    PlaceLineByFromToID(ID, From, ToID);
                                }
                            }
                        }
                        else
                        {
                            StripIDCount[Int32.Parse(Split[4])] = Int32.Parse(Split[3]) + Int32.Parse(Split[2]);
                            MakeNewStrip(
                                Int32.Parse(Split[6]),
                                Int32.Parse(Split[7]),
                                Int32.Parse(Split[8]),
                                Int32.Parse(Split[9]),
                                Int32.Parse(Split[10]),
                                new Point(Int32.Parse(Split[0]), Int32.Parse(Split[1])),
                                Int32.Parse(Split[2]),
                                Int32.Parse(Split[3]),
                                Int32.Parse(Split[4]),
                                (FlipDir)Enum.Parse(typeof(FlipDir), Split[5])
                                );
                            IDCount = Int32.Parse(Split[6]);
                        }
                    }
                }
                IDCount++;
            }
            catch { MessageBox.Show("Cannot access file!"); }
        }

        void SaveSetup(string Location)
        {
            try
            {
                using (StreamWriter SaveFile = new StreamWriter(Location, false))
                {
                    using (StreamWriter AutoSaveFile = new StreamWriter(Directory.GetCurrentDirectory() + "\\Setups\\0.txt", false))
                    {
                        string SerialOut = "";
                        foreach (UIElement c in InnerSetupPanel.Children)
                        {
                            if (c is Border)
                            {
                                Grid InnerGrid = (c as Border).Child as Grid;
                                SetupStrip OutGridTag = (SetupStrip)InnerGrid.Tag;
                                SerialOut = (c as Border).Margin.Left + ";" + (c as Border).Margin.Top + ";" + OutGridTag.LEDsPrStrip + ";" + OutGridTag.FromID + ";" + OutGridTag.PinID + ";" + OutGridTag.FlipDir + ";" + OutGridTag.ID + ";" + OutGridTag.ConnectedToID + ";" + OutGridTag.ConnectedFromID + ";" + OutGridTag.ConnectedFrom + ";" + OutGridTag.DragingFrom;
                                SaveFile.WriteLine(SerialOut);
                                AutoSaveFile.WriteLine(SerialOut);
                            }
                        }

                        SerialOut = "S;";
                        Grid StartGrid = FindGridByID(GetFirstInSeriesID());
                        if (StartGrid != null)
                        {
                            SetupStrip FindTags = (SetupStrip)StartGrid.Tag;
                            while (true)
                            {
                                SerialOut += FindTags.ID + ":" + FindTags.DragingFrom + ">" + FindTags.ConnectedToID + ";";
                                if (FindTags.ConnectedToID == 0)
                                    break;
                                FindTags = (SetupStrip)(FindGridByID(Math.Abs(FindTags.ConnectedToID)) as Grid).Tag;
                            }
                        }
                        SaveFile.WriteLine(SerialOut);
                        AutoSaveFile.WriteLine(SerialOut);
                    }
                }
            }
            catch { MessageBox.Show("Cannot access file!"); }
        }

        void LoadDeviceConfig()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\cfg.txt"))
            {
                int PositionCount = 0;
                try
                {
                    string[] Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\cfg.txt", Encoding.UTF8);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        string[] Split = Lines[i].Split(';');
                        if (Split[0] == "WIRELESSDEVICE")
                        {
                            if (PositionCount == 0)
                            {
                                AddWirelessDeviceButtons(ConnectionAddDeviceButton1, Split[1], IPAddress.Parse(Split[2]), Int32.Parse(Split[3]), Split[4], 0, 0, Int32.Parse(Split[5]));
                            }
                            else
                            {
                                if (PositionCount == 1)
                                {
                                    AddWirelessDeviceButtons(ConnectionAddDeviceButton2, Split[1], IPAddress.Parse(Split[2]), Int32.Parse(Split[3]), Split[4], 0, 1, Int32.Parse(Split[5]));
                                }
                                else
                                {
                                    if (PositionCount == 2)
                                    {
                                        AddWirelessDeviceButtons(ConnectionAddDeviceButton3, Split[1], IPAddress.Parse(Split[2]), Int32.Parse(Split[3]), Split[4], 1, 0, Int32.Parse(Split[5]));
                                    }
                                    else
                                    {
                                        AddWirelessDeviceButtons(ConnectionAddDeviceButton4, Split[1], IPAddress.Parse(Split[2]), Int32.Parse(Split[3]), Split[4], 1, 1, Int32.Parse(Split[5]));
                                    }
                                }
                            }
                            PositionCount++;
                        }
                        if (Split[0] == "SERIALDEVICE")
                        {
                            if (PositionCount == 0)
                            {
                                AddSerialDeviceButtons(ConnectionAddDeviceButton1, Split[1], Split[2], Int32.Parse(Split[3]), Split[4], 0, 0, Int32.Parse(Split[5]));
                            }
                            else
                            {
                                if (PositionCount == 1)
                                {
                                    AddSerialDeviceButtons(ConnectionAddDeviceButton1, Split[1], Split[2], Int32.Parse(Split[3]), Split[4], 0, 1, Int32.Parse(Split[5]));
                                }
                                else
                                {
                                    if (PositionCount == 2)
                                    {
                                        AddSerialDeviceButtons(ConnectionAddDeviceButton1, Split[1], Split[2], Int32.Parse(Split[3]), Split[4], 1, 0, Int32.Parse(Split[5]));
                                    }
                                    else
                                    {
                                        AddSerialDeviceButtons(ConnectionAddDeviceButton1, Split[1], Split[2], Int32.Parse(Split[3]), Split[4], 1, 1, Int32.Parse(Split[5]));
                                    }
                                }
                            }
                            PositionCount++;
                        }
                        LoadElement(Lines[i]);
                    }
                }
                catch { }
            }
        }

        void LoadApplicationConfigsForDevice(string DeviceName)
        {
            string UseName = "";
            if (DeviceName == " - All - ")
                UseName = "ALL";
            else
                UseName = DeviceName;
            if (File.Exists(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_cfg.txt"))
            {
                try
                {
                    string[] Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_cfg.txt", Encoding.UTF8);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        LoadElement(Lines[i]);
                    }
                }
                catch { }
            }
        }

        void LoadAdvancedApplicationConfigsForDevice(string DeviceName)
        {
            string UseName = "";
            if (DeviceName == " - All - ")
                UseName = "ALL";
            else
                UseName = DeviceName;
            if (File.Exists(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_cfg.txt"))
            {
                try
                {
                    string[] Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_ADVcfg.txt", Encoding.UTF8);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        LoadElement(Lines[i]);
                    }
                }
                catch { }
            }
        }

        void LoadElement(string Line)
        {
            Loading = true;
            try
            {
                string[] Split = Line.Split(';');
                if (Split[0] == "FADECOLORWHEEL")
                {
                    FadingGridColorWheelPointer.Margin = new Thickness(Convert.ToDouble(Split[1]), Convert.ToDouble(Split[2]), 0, 0);
                    Color ColorInColorCirle = GammaCorrection(
                        GetColorOfPixelInImage(
                            (BitmapSource)FadingGridColorWheelImage.Source,
                            (int)FadingGridColorWheelImage.Width,
                            (int)FadingGridColorWheelImage.Height,
                            (int)FadingGridColorWheelPointer.Margin.Left,
                            (int)FadingGridColorWheelPointer.Margin.Top),
                        FadingGammaCorrectionSlider.Value);

                    FadingRGBColorCodeLabel.Content = "RGB Color: " + ColorInColorCirle.R + ", " + ColorInColorCirle.G + ", " + ColorInColorCirle.B;
                    FadingColorRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, (byte)ColorInColorCirle.R, (byte)ColorInColorCirle.G, (byte)ColorInColorCirle.B));
                }
                if (Split[0] == "AMBILIGHTTOPSIDE")
                {
                    AmbilightSelectTopSideCheckbox.IsChecked = true;
                    TopSide.FromID = Convert.ToInt32(Split[1]);
                    TopSide.ToID = Convert.ToInt32(Split[2]);
                    SetSides();
                }
                if (Split[0] == "NOAMBILIGHTTOPSIDE")
                    AmbilightSelectTopSideCheckbox.IsChecked = false;
                if (Split[0] == "AMBILIGHTRIGHTSIDE")
                {
                    AmbilightSelectRightSideCheckbox.IsChecked = true;
                    RightSide.FromID = Convert.ToInt32(Split[1]);
                    RightSide.ToID = Convert.ToInt32(Split[2]);
                    SetSides();
                }
                if (Split[0] == "NOAMBILIGHTRIGHTSIDE")
                    AmbilightSelectRightSideCheckbox.IsChecked = false;
                if (Split[0] == "AMBILIGHTBOTTOMSIDE")
                {
                    AmbilightSelectBottomSideCheckbox.IsChecked = true;
                    BottomSide.FromID = Convert.ToInt32(Split[1]);
                    BottomSide.ToID = Convert.ToInt32(Split[2]);
                    SetSides();
                }
                if (Split[0] == "NOAMBILIGHTBOTTOMSIDE")
                    AmbilightSelectBottomSideCheckbox.IsChecked = false;
                if (Split[0] == "AMBILIGHTLEFTSIDE")
                {
                    AmbilightSelectLeftSideCheckbox.IsChecked = true;
                    LeftSide.FromID = Convert.ToInt32(Split[1]);
                    LeftSide.ToID = Convert.ToInt32(Split[2]);
                    SetSides();
                }
                if (Split[0] == "NOAMBILIGHTLEFTSIDE")
                    AmbilightSelectLeftSideCheckbox.IsChecked = false;
                if (Split[0] == "TEXTBOX")
                {
                    TextBox LoadTextBox = MainGrid.FindName(Split[1]) as TextBox;
                    LoadTextBox.Text = Split[2];
                }
                if (Split[0] == "SLIDER")
                {
                    Slider LoadSlider = MainGrid.FindName(Split[1]) as Slider;
                    double NewValue = Convert.ToDouble(Split[2]);
                    if (NewValue > LoadSlider.Maximum)
                    {
                        LoadSlider.Value = LoadSlider.Maximum;
                    }
                    else
                    {
                        if (NewValue < LoadSlider.Minimum)
                        {
                            LoadSlider.Value = LoadSlider.Minimum;
                        }
                        else
                        {
                            LoadSlider.Value = NewValue;
                        }
                    }
                }
                if (Split[0] == "CHECKBOX")
                {
                    CheckBox LoadCheckbox = MainGrid.FindName(Split[1]) as CheckBox;
                    LoadCheckbox.IsChecked = Convert.ToBoolean(Split[2]);
                }
                if (Split[0] == "COMBOBOX")
                {
                    ComboBox LoadCombobox = MainGrid.FindName(Split[1]) as ComboBox;
                    int NewValue = Int32.Parse(Split[2]);
                    if (NewValue < 0)
                    {
                        LoadCombobox.SelectedIndex = 0;
                    }
                    else
                    {
                        if (NewValue >= LoadCombobox.Items.Count)
                        {
                            LoadCombobox.SelectedIndex = LoadCombobox.Items.Count - 1;
                        }
                        else
                        {
                            LoadCombobox.SelectedIndex = NewValue;
                        }
                    }
                }
            }
            catch { Console.WriteLine("Could Not find: " + Line); }
            Loading = false;
        }

        void SaveRangesForMode(string DeviceName, string Mode)
        {
            string UseName = "";
            if (DeviceName == " - All - ")
                UseName = "ALL";
            else
                UseName = DeviceName;

            try
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt"))
                    File.Create(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt");

                bool IsThere = false;
                string[] Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt", Encoding.UTF8);
                for (int i = 0; i < Lines.Length; i++)
                {
                    string[] Split = Lines[i].Split(';');
                    if (Split[0] == Mode)
                    {
                        IsThere = true;
                        if (Split[1] == "FROM")
                        {
                            Split[2] = FromToIDSliderFrom.Value.ToString();
                        }
                        if (Split[1] == "TO")
                        {
                            Split[2] = FromToIDSliderTo.Value.ToString();
                        }
                    }
                    Lines[i] = string.Join(";", Split);
                }
                if (!IsThere)
                {
                    Array.Resize(ref Lines, Lines.Length + 2);
                    Lines[Lines.Length - 2] = Mode + ";FROM;" + FromToIDSliderFrom.Value.ToString();
                    Lines[Lines.Length - 1] = Mode + ";TO;" + FromToIDSliderTo.Value.ToString();
                    int Index = FindDeviceIndexByName(DeviceName);

                    FromToIDSliderFrom.Maximum = DeviceList[Index].TotalLEDCount - 1;
                    FromToIDSliderTo.Maximum = DeviceList[Index].TotalLEDCount - 1;
                    FromToIDSliderFrom.Value = 0;
                    FromToIDSliderTo.Value = DeviceList[Index].TotalLEDCount - 1;
                }
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt", Lines);
            }
            catch { }
        }

        void LoadRangesForMode(string DeviceName, string Mode)
        {
            string UseName = "";
            if (DeviceName == " - All - ")
                UseName = "ALL";
            else
                UseName = DeviceName;

            if (ModesDeviceSelectionCombobox.Items.Count > 0)
            {
                FromToIDSliderFrom.Maximum = 1;
                FromToIDSliderTo.Maximum = 1;
                FromToIDSliderFrom.Minimum = 0;
                FromToIDSliderTo.Minimum = 0;
                FromToIDSliderFrom.Value = 0;
                FromToIDSliderTo.Value = 1;
                if (ModesDeviceSelectionCombobox.SelectedIndex != 0)
                {
                    for (int i = 0; i < DeviceList.Count; i++)
                    {
                        if (DeviceList[i].DeviceName == DeviceName)
                        {
                            FromToIDSliderFrom.Maximum = DeviceList[i].TotalLEDCount - 1;
                            FromToIDSliderTo.Maximum = DeviceList[i].TotalLEDCount - 1;
                            FromToIDSliderFrom.Value = 0;
                            FromToIDSliderTo.Value = DeviceList[i].TotalLEDCount - 1;
                        }
                    }
                }
            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt"))
            {
                try
                {
                    string[] Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Ranges\\" + UseName + "_RANGES.txt", Encoding.UTF8);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        string[] Split = Lines[i].Split(';');
                        if (Split[0] == Mode)
                        {
                            if (Split[1] == "FROM")
                            {
                                FromToIDSliderFrom.Value = Convert.ToDouble(Split[2]);
                            }
                            if (Split[1] == "TO")
                            {
                                FromToIDSliderTo.Value = Convert.ToDouble(Split[2]);
                            }
                        }
                    }
                }
                catch { }
            }
        }
        void SaveGeneralConfigs()
        {
            try
            {
                if (DeviceSelectionCombobox.SelectedIndex != 0)
                {
                    if (CurrentSaveFileName == "")
                        CurrentSaveFileName = DeviceSelectionCombobox.SelectedItem.ToString();
                    string NewSaveFileLoc = Directory.GetCurrentDirectory() + "\\Setups\\" + CurrentSaveFileName + ".txt";
                    SaveSetup(NewSaveFileLoc);
                    SetDeviceSetupSaveFileName(FindDeviceIndexByName(DeviceSelectionCombobox.SelectedItem.ToString()), NewSaveFileLoc);
                }
                else
                {
                    SaveSetup(Directory.GetCurrentDirectory() + "\\Setups\\ALL.txt");
                }

                using (StreamWriter SaveFile = new StreamWriter(Directory.GetCurrentDirectory() + "\\cfg.txt", false))
                {
                    foreach (TransferDevice Device in DeviceList)
                    {
                        if (Device.IsWireless)
                        {
                            SaveFile.WriteLine(
                                "WIRELESSDEVICE;" +
                                Device.DeviceName + ";" +
                                Device.IPAddress + ";" +
                                Device.Port + ";" +
                                Device.SetupSaveFileName + ";" +
                                Device.TotalLEDCount
                                );
                        }
                        else
                        {
                            SaveFile.WriteLine(
                                "SERIALDEVICE;" +
                                Device.DeviceName + ";" +
                                Device.COMPortName + ";" +
                                Device.BaudRate + ";" +
                                Device.SetupSaveFileName + ";" +
                                Device.TotalLEDCount
                                );
                        }
                    }
                    foreach (UIElement Element in GetAllControls(MainGrid))
                    {
                        if (Element.Uid == "GeneralSetting")
                        {
                            if (Element is TextBox)
                            {
                                SaveFile.WriteLine("TEXTBOX;" + (Element as TextBox).Name + ";" + (Element as TextBox).Text);
                            }
                            if (Element is Slider)
                            {
                                SaveFile.WriteLine("SLIDER;" + (Element as Slider).Name + ";" + (Element as Slider).Value);
                            }
                            if (Element is CheckBox)
                            {
                                SaveFile.WriteLine("CHECKBOX;" + (Element as CheckBox).Name + ";" + (bool)(Element as CheckBox).IsChecked);
                            }
                            if (Element is ComboBox)
                            {
                                SaveFile.WriteLine("COMBOBOX;" + (Element as ComboBox).Name + ";" + (Element as ComboBox).SelectedIndex);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        void SaveApplicationConfig(string DeviceName)
        {
            string UseName = "";
            if (DeviceName == " - All - ")
                UseName = "ALL";
            else
                UseName = DeviceName;
            try
            {
                using (StreamWriter DeviceSaveFile = new StreamWriter(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_cfg.txt", false))
                {
                    DeviceSaveFile.WriteLine(
                        "FADECOLORWHEEL;" +
                        FadingGridColorWheelPointer.Margin.Left + ";" +
                        FadingGridColorWheelPointer.Margin.Top
                        );
                    if ((bool)AmbilightSelectTopSideCheckbox.IsChecked)
                    {
                        DeviceSaveFile.WriteLine(
                                "AMBILIGHTTOPSIDE;" +
                                TopSide.FromID + ";" +
                                TopSide.ToID
                                );
                    }
                    else
                        DeviceSaveFile.WriteLine("NOAMBILIGHTTOPSIDE;");
                    if ((bool)AmbilightSelectRightSideCheckbox.IsChecked)
                    {
                        DeviceSaveFile.WriteLine(
                                "AMBILIGHTRIGHTSIDE;" +
                                RightSide.FromID + ";" +
                                RightSide.ToID
                                );
                    }
                    else
                        DeviceSaveFile.WriteLine("NOAMBILIGHTRIGHTSIDE;");
                    if ((bool)AmbilightSelectBottomSideCheckbox.IsChecked)
                    {
                        DeviceSaveFile.WriteLine(
                                "AMBILIGHTBOTTOMSIDE;" +
                                BottomSide.FromID + ";" +
                                BottomSide.ToID
                                );
                    }
                    else
                        DeviceSaveFile.WriteLine("NOAMBILIGHTBOTTOMSIDE;");
                    if ((bool)AmbilightSelectLeftSideCheckbox.IsChecked)
                    {
                        DeviceSaveFile.WriteLine(
                                "AMBILIGHTLEFTSIDE;" +
                                LeftSide.FromID + ";" +
                                LeftSide.ToID
                                );
                    }
                    else
                        DeviceSaveFile.WriteLine("NOAMBILIGHTLEFTSIDE;");

                    foreach (UIElement Element in GetAllControls(MainGrid))
                    {
                        if (Element.Uid == "Setting")
                        {
                            if (Element is TextBox)
                            {
                                DeviceSaveFile.WriteLine("TEXTBOX;" + (Element as TextBox).Name + ";" + (Element as TextBox).Text);
                            }
                            if (Element is Slider)
                            {
                                DeviceSaveFile.WriteLine("SLIDER;" + (Element as Slider).Name + ";" + (Element as Slider).Value);
                            }
                            if (Element is CheckBox)
                            {
                                DeviceSaveFile.WriteLine("CHECKBOX;" + (Element as CheckBox).Name + ";" + (bool)(Element as CheckBox).IsChecked);
                            }
                            if (Element is ComboBox)
                            {
                                DeviceSaveFile.WriteLine("COMBOBOX;" + (Element as ComboBox).Name + ";" + (Element as ComboBox).SelectedIndex);
                            }
                        }
                    }
                }
                if ((bool)SettingsAdvancedSettingsCheckBox.IsChecked)
                {
                    using (StreamWriter DeviceSaveFile = new StreamWriter(Directory.GetCurrentDirectory() + "\\DeviceConfigs\\" + UseName + "_ADVcfg.txt", false))
                    {
                        foreach (UIElement Element in GetAllControls(MainGrid))
                        {
                            if (Element.Uid == "AdvancedSetting")
                            {
                                if (Element is TextBox)
                                {
                                    DeviceSaveFile.WriteLine("TEXTBOX;" + (Element as TextBox).Name + ";" + (Element as TextBox).Text);
                                }
                                if (Element is Slider)
                                {
                                    DeviceSaveFile.WriteLine("SLIDER;" + (Element as Slider).Name + ";" + (Element as Slider).Value);
                                }
                                if (Element is CheckBox)
                                {
                                    DeviceSaveFile.WriteLine("CHECKBOX;" + (Element as CheckBox).Name + ";" + (bool)(Element as CheckBox).IsChecked);
                                }
                                if (Element is ComboBox)
                                {
                                    DeviceSaveFile.WriteLine("COMBOBOX;" + (Element as ComboBox).Name + ";" + (Element as ComboBox).SelectedIndex);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}