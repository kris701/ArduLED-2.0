using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ArduLED_Serial_Protocol
{
    public class ArduLEDSerialProtocol : TransferMode
    {
        public SerialPort SerialPort1;
        public TcpClient TCPClient1;
        public bool UnitReady = false;
        public bool Wait = false;
        public bool IsWireless = false;
        private bool ReadyToRecive = false;
        private int UnitTimeoutCounter = 0;
        private IPAddress LastIPAddress;
        private int LastPort;

        public ArduLEDSerialProtocol(bool _IsWireless)
        {
            IsWireless = _IsWireless;
            if (IsWireless)
            {
                TCPClient1 = new TcpClient();
                TCPClient1.ReceiveTimeout = -1;
                TCPClient1.SendTimeout = -1;
            }
            else
            {
                SerialPort1 = new SerialPort();
                SerialPort1.WriteTimeout = 500;
                SerialPort1.RtsEnable = true;
                SerialPort1.DtrEnable = true;
                SerialPort1.DataReceived += SerialRead;
            }
        }

        private void SerialRead(object sender, SerialDataReceivedEventArgs e)
        {
            if (!UnitReady)
                UnitReady = true;
            if (SerialPort1.BytesToRead > 0)
            {
                if (!ReadyToRecive)
                {
                    ReadyToRecive = true;
                    UnitTimeoutCounter = 0;
                }
                SerialPort1.ReadChar();
            }
        }

        public bool ConnectToWirelessDevice(IPAddress _IPAddress, int _Port)
        {
            try
            {
                if (TCPClient1 != null)
                    if (TCPClient1.Connected)
                        TCPClient1.Close();
                TCPClient1 = new TcpClient();
                TCPClient1.ReceiveTimeout = -1;
                TCPClient1.SendTimeout = -1;
                UnitReady = false;
                LastIPAddress = _IPAddress;
                LastPort = _Port;
                TCPClient1.Connect(_IPAddress, _Port);
                if (TCPClient1.Connected)
                {
                    UnitReady = true;
                }
                return true;
            }
            catch
            {
                Console.Write("Could not connect to wireless device!");
                return false;
            }
        }

        public async Task<bool> ConnectToWirelessDeviceAsync(IPAddress _IPAddress, int _Port)
        {
            try
            {
                if (TCPClient1 != null)
                    if (TCPClient1.Connected)
                        TCPClient1.Close();
                TCPClient1 = new TcpClient();
                TCPClient1.ReceiveTimeout = -1;
                TCPClient1.SendTimeout = -1;
                UnitReady = false;
                LastIPAddress = _IPAddress;
                LastPort = _Port;
                await TCPClient1.ConnectAsync(_IPAddress, _Port);
                if (TCPClient1.Connected)
                {
                    UnitReady = true;
                }
                return true;
            }
            catch
            {
                Console.Write("Could not connect to wireless device!");
                return false;
            }
        }

        public bool ConnectToCOMDevice(string _COMPort, int _BaudRate)
        {
            if (_COMPort != "")
            {
                try
                {
                    if (SerialPort1.IsOpen)
                        SerialPort1.Close();

                    UnitReady = false;

                    SerialPort1.PortName = _COMPort;
                    SerialPort1.BaudRate = _BaudRate;

                    SerialPort1.Open();
                    return true;
                }
                catch
                {
                    Console.Write("Could not connect to COM device");
                    return false;
                }
            }
            return false;
        }

        public void Write(TransferMode _Input)
        {
            if (_Input is NoneMode)
            {
                SendData("0;" + (_Input as NoneMode).Data);
            }
            if (_Input is FadeColorsMode)
            {
                FadeColorsMode Data = (_Input as FadeColorsMode);
                SendData("1;" + Data.Red + ";" + Data.Green + ";" + Data.Blue + ";" + Data.FadeSpeed + ";" + Math.Round(Data.FadeFactor * 100, 0));
            }
            if (_Input is VisualizerBeat)
            {
                VisualizerBeat Data = (_Input as VisualizerBeat);
                SendData("2;" + Data.BeatValue.ToString().Replace(',', '.'));
            }
            if (_Input is VisualizerWave)
            {
                VisualizerWave Data = (_Input as VisualizerWave);
                SendData("3;" + Data.Red + ";" + Data.Green + ";" + Data.Blue);
            }
            if (_Input is IndividualLEDs)
            {
                IndividualLEDs Data = (_Input as IndividualLEDs);
                SendData("4;" + Data.PinID + ";" + Data.HardwareID + ";" + Data.Red + ";" + Data.Green + ";" + Data.Blue);
            }
            if (_Input is VisualizerFullSpectrum)
            {
                VisualizerFullSpectrum Data = (_Input as VisualizerFullSpectrum);
                SendData("5;" + Data.SpectrumSplit + ";" + Data.SpectrumValues);
            }
            if (_Input is Ranges)
            {
                Ranges Data = (_Input as Ranges);
                SendData("6;" + Data.FromID + ";" + Data.ToID);
            }
            if (_Input is Ambilight)
            {
                Ambilight Data = (_Input as Ambilight);
                SendData("7;" + Data.FromID + ";" + Data.ToID + ";" + Data.LEDsPrBlock + ";" + Data.Values);
            }
            if (_Input is Animation)
            {
                Animation Data = (_Input as Animation);
                SendData("8;" + Data.LineCount + ";" + Convert.ToInt32(Data.UseCompression) + ";" + Convert.ToInt32(Data.ShowNow) + ";" + Data.Values);
            }
        }

        public async Task WriteAsync(TransferMode _Input)
        {
            if (_Input is NoneMode)
            {
                await SendDataAsync("0;" + (_Input as NoneMode).Data);
            }
            if (_Input is FadeColorsMode)
            {
                FadeColorsMode Data = (_Input as FadeColorsMode);
                await SendDataAsync("1;" + Data.Red + ";" + Data.Green + ";" + Data.Blue + ";" + Data.FadeSpeed + ";" + Math.Round(Data.FadeFactor * 100, 0));
            }
            if (_Input is VisualizerBeat)
            {
                VisualizerBeat Data = (_Input as VisualizerBeat);
                await SendDataAsync("2;" + Data.BeatValue.ToString().Replace(',', '.'));
            }
            if (_Input is VisualizerWave)
            {
                VisualizerWave Data = (_Input as VisualizerWave);
                await SendDataAsync("3;" + Data.Red + ";" + Data.Green + ";" + Data.Blue);
            }
            if (_Input is IndividualLEDs)
            {
                IndividualLEDs Data = (_Input as IndividualLEDs);
                await SendDataAsync("4;" + Data.PinID + ";" + Data.HardwareID + ";" + Data.Red + ";" + Data.Green + ";" + Data.Blue);
            }
            if (_Input is VisualizerFullSpectrum)
            {
                VisualizerFullSpectrum Data = (_Input as VisualizerFullSpectrum);
                await SendDataAsync("5;" + Data.SpectrumSplit + ";" + Data.SpectrumValues);
            }
            if (_Input is Ranges)
            {
                Ranges Data = (_Input as Ranges);
                await SendDataAsync("6;" + Data.FromID + ";" + Data.ToID);
            }
            if (_Input is Ambilight)
            {
                Ambilight Data = (_Input as Ambilight);
                await SendDataAsync("7;" + Data.FromID + ";" + Data.ToID + ";" + Data.LEDsPrBlock + ";" + Data.Values);
            }
            if (_Input is Animation)
            {
                Animation Data = (_Input as Animation);
                await SendDataAsync("8;" + Data.LineCount + ";" + Convert.ToInt32(Data.UseCompression) + ";" + Convert.ToInt32(Data.ShowNow) + ";" + Data.Values);
            }
        }

        private void SendData(string _Input)
        {
            if (!Wait)
            {
                if (IsWireless)
                {
                    if (UnitReady)
                    {
                        if (ReadyToRecive)
                        {
                            try
                            {
                                Stream Stm = TCPClient1.GetStream();

                                System.Text.ASCIIEncoding WithEncoding = new System.Text.ASCIIEncoding();
                                byte[] BytesToSend = WithEncoding.GetBytes(";" + _Input + ";-1;");

                                Stm.Write(BytesToSend, 0, BytesToSend.Length);
                            }
                            catch
                            {
                                if (!TCPClient1.Connected)
                                {
                                    TCPClient1.Close();
                                    TCPClient1 = new TcpClient();
                                    TCPClient1.ReceiveTimeout = -1;
                                    TCPClient1.SendTimeout = -1;
                                    ConnectToWirelessDevice(LastIPAddress, LastPort);
                                }
                            }
                            ReadyToRecive = false;
                        }
                        int TimeoutCounter = 0;
                        while (!ReadyToRecive)
                        {
                            Thread.Sleep(1);
                            TimeoutCounter++;
                            if (TimeoutCounter > 250)
                            {
                                UnitTimeoutCounter++;
                                if (UnitTimeoutCounter > 20)
                                {
                                    Console.WriteLine("Communication to Unit failed!");
                                    UnitTimeoutCounter = 0;
                                    break;
                                }
                                ReadyToRecive = true;
                                break;
                            }

                            if (TCPClient1.Available > 0)
                            {
                                Stream Stm = TCPClient1.GetStream();
                                byte[] bb = new byte[25];
                                Stm.Read(bb, 0, 25);

                                if (Convert.ToChar(bb[0]) != 0)
                                    ReadyToRecive = true;
                            }
                        }
                    }
                }
                else
                {
                    if (UnitReady)
                    {
                        int TimeoutCounter = 0;
                        while (!ReadyToRecive)
                        {
                            Thread.Sleep(1);
                            TimeoutCounter++;
                            if (TimeoutCounter > 250)
                            {
                                UnitTimeoutCounter++;
                                if (UnitTimeoutCounter > 20)
                                {
                                    Console.WriteLine("Communication to Unit failed!");
                                    UnitTimeoutCounter = 0;
                                    break;
                                }
                                ReadyToRecive = true;
                                break;
                            }
                        }
                    }
                    if (ReadyToRecive)
                    {
                        try
                        {
                            SerialPort1.WriteLine(";" + _Input + ";-1;");
                        }
                        catch { }
                        ReadyToRecive = false;
                    }
                }
            }
        }

        private async Task SendDataAsync(string _Input)
        {
            if (!Wait)
            {
                if (IsWireless)
                {
                    if (UnitReady)
                    {
                        if (ReadyToRecive)
                        {
                            try
                            {
                                Stream Stm = TCPClient1.GetStream();

                                System.Text.ASCIIEncoding WithEncoding = new System.Text.ASCIIEncoding();
                                byte[] BytesToSend = WithEncoding.GetBytes(";" + _Input + ";-1;");

                                await Stm.WriteAsync(BytesToSend, 0, BytesToSend.Length);
                            }
                            catch
                            {
                                if (!TCPClient1.Connected)
                                {
                                    TCPClient1.Close();
                                    TCPClient1 = new TcpClient();
                                    TCPClient1.ReceiveTimeout = -1;
                                    TCPClient1.SendTimeout = -1;
                                    await ConnectToWirelessDeviceAsync(LastIPAddress, LastPort);
                                }
                            }
                            ReadyToRecive = false;
                        }
                        int TimeoutCounter = 0;
                        while (!ReadyToRecive)
                        {
                            await Task.Delay(1);
                            TimeoutCounter++;
                            if (TimeoutCounter > 250)
                            {
                                UnitTimeoutCounter++;
                                if (UnitTimeoutCounter > 20)
                                {
                                    Console.WriteLine("Communication to Unit failed!");
                                    UnitTimeoutCounter = 0;
                                    break;
                                }
                                ReadyToRecive = true;
                                break;
                            }

                            if (TCPClient1.Available > 0)
                            {
                                Stream Stm = TCPClient1.GetStream();
                                byte[] bb = new byte[25];
                                await Stm.ReadAsync(bb, 0, 25);

                                if (Convert.ToChar(bb[0]) != 0)
                                    ReadyToRecive = true;
                            }
                        }
                    }
                }
                else
                {
                    if (UnitReady)
                    {
                        int TimeoutCounter = 0;
                        while (!ReadyToRecive)
                        {
                            await Task.Delay(1);
                            TimeoutCounter++;
                            if (TimeoutCounter > 250)
                            {
                                UnitTimeoutCounter++;
                                if (UnitTimeoutCounter > 20)
                                {
                                    Console.WriteLine("Communication to Unit failed!");
                                    UnitTimeoutCounter = 0;
                                    break;
                                }
                                ReadyToRecive = true;
                                break;
                            }
                        }
                    }
                    if (ReadyToRecive)
                    {
                        try
                        {
                            SerialPort1.WriteLine(";" + _Input + ";-1;");
                        }
                        catch { }
                        ReadyToRecive = false;
                    }
                }
            }
        }
    }

    public class TransferMode
    {
        public class NoneMode : TransferMode
        {
            public string Data;

            public NoneMode(string _Data)
            {
                Data = _Data;
            }
        }
        public class FadeColorsMode : TransferMode
        {
            public Int16 Red;
            public Int16 Green;
            public Int16 Blue;
            public int FadeSpeed;
            public double FadeFactor;

            public FadeColorsMode(Int16 _Red, Int16 _Green, Int16 _Blue, int _FadeSpeed, double _FadeFactor)
            {
                Red = _Red;
                Green = _Green;
                Blue = _Blue;
                FadeSpeed = _FadeSpeed;
                FadeFactor = _FadeFactor;
            }
        }

        public class VisualizerBeat : TransferMode
        {
            public int BeatValue;

            public VisualizerBeat(int _BeatValue)
            {
                BeatValue = _BeatValue;
            }
        }

        public class VisualizerWave : TransferMode
        {
            public Int16 Red;
            public Int16 Green;
            public Int16 Blue;

            public VisualizerWave(Int16 _Red, Int16 _Green, Int16 _Blue)
            {
                Red = _Red;
                Green = _Green;
                Blue = _Blue;
            }
        }

        public class IndividualLEDs : TransferMode
        {
            public Int16 Red;
            public Int16 Green;
            public Int16 Blue;
            public int PinID;
            public int HardwareID;

            public IndividualLEDs(Int16 _Red, Int16 _Green, Int16 _Blue, int _PinID, int _HardwareID)
            {
                Red = _Red;
                Green = _Green;
                Blue = _Blue;
                PinID = _PinID;
                HardwareID = _HardwareID;
            }
        }

        public class VisualizerFullSpectrum : TransferMode
        {
            public int SpectrumSplit;
            public string SpectrumValues;

            public VisualizerFullSpectrum(string _SpectrumValues, int _SpectrumSplit)
            {
                SpectrumValues = _SpectrumValues;
                SpectrumSplit = _SpectrumSplit;
            }
        }

        public class Ranges : TransferMode
        {
            public int FromID;
            public int ToID;

            public Ranges(int _FromID, int _ToID)
            {
                FromID = _FromID;
                ToID = _ToID;
            }
        }

        public class Ambilight : TransferMode
        {
            public int FromID;
            public int ToID;
            public int LEDsPrBlock;
            public string Values;

            public Ambilight(int _FromID, int _ToID, int _LEDsPrBlock, string _Values)
            {
                FromID = _FromID;
                ToID = _ToID;
                LEDsPrBlock = _LEDsPrBlock;
                Values = _Values;
            }
        }

        public class Animation : TransferMode
        {
            public int LineCount;
            public bool UseCompression;
            public bool ShowNow;
            public string Values;

            public Animation(int _LineCount, bool _UseCompression, bool _ShowNow, string _Values)
            {
                LineCount = _LineCount;
                UseCompression = _UseCompression;
                ShowNow = _ShowNow;
                Values = _Values;
            }
        }
    }
}
