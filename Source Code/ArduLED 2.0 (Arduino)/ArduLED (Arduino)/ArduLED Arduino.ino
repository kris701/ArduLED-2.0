#include <NeoPixelBus.h>
#define UseWifi false
/*
 Feature Options:
  - NeoGrbFeature
  - NeoGrbwFeature
  - NeoRgbFeature
  - NeoRgbwFeature
  - NeoBrgFeature
  - NeoRbgFeature
  
 DotStar only features:

  - DotStarBgrFeature
  - DotStarLbgrFeature
  - DotStarGrbFeature
  - DotStarLgrbFeature
*/
#define ColorFeature NeoGrbFeature
/*
 Method Options:
  - Neo800KbpsMethod
  - Neo400KbpsMethod

 Model specific methods
  - NeoWs2812Method
  - NeoWs2812xMethod
  - NeoWs2813Method
  - NeoSk6812Method
  - NeoLc8812Method

 ESP8266 only methods:
  - NeoEsp8266Dma800KbpsMethod
  - NeoEsp8266Dma400KbpsMethod
  - NeoEsp8266DmaWs2813Method
  - NeoEsp8266Uart800KbpsMethod
  - NeoEsp8266Uart400KbpsMethod
  - NeoEsp8266UartWs2813Method
  - NeoEsp8266AsyncUart800KbpsMethod
  - NeoEsp8266AsyncUart400KbpsMethod
  - NeoEsp8266AsyncUartWs2813Method
  - NeoEsp8266BitBang800KbpsMethod
  - NeoEsp8266BitBang400KbpsMethod
  - NeoEsp8266BitBangWs2813Method

 DotStar only methods:

  - DotStarSpiMethod
  - DotStarMethod
*/
#define LEDMethod Neo800KbpsMethod
#define SplitS 128
#define LEDStripsS 16
#define SeriesS 32

#if UseWifi == true
	#include <ESP8266WiFi.h>
	#define WifiName "WiFimodem-5583-2.4Ghz"
	#define WifiPassword "c41b1479e1"
	IPAddress ServerIP(192, 168, 1, 90);
	IPAddress Gateway(192, 168, 1, 254);
	IPAddress Subnet(255, 255, 255, 0);
	#define ServerPort 8888
	WiFiServer WifiServer(ServerPort);
	WiFiClient Client;
#else
	#define BaudRate 1000000
#endif

 struct PixelSeries
{
	int FromID;
	int ToID;
	int PinID;

	PixelSeries(int _FromID = 0, int _ToID = 0, int _PinID = 0)
	{
		FromID = _FromID;
		ToID = _ToID;
		PinID = _PinID;
	};
};

enum Mode { None, FadeColors, VisualizerBeat, VisualizerWave, IndividualLEDs, VisualizerFullSpectrum, Ranges, Ambilight, Animation };

void Run(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount);
void Mode_F(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
void Mode_B(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
void Mode_W(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _CountToID, short _ShowFromPin, short _ShowToPin);
void Mode_I(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS]);
void Mode_S(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
void Mode_R(short *_DiscardFromIndex, short *_DiscardToIndex, short *_CountFromID, short *_CountToID, short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short *_FromID, short *_ToID, short _TotalLEDCount, short _Split[SplitS], short *_ShowFromPin, short *_ShowToPin);
void Mode_A(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
void Mode_N(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], PixelSeries _PixelSeries[SeriesS], short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountToID, short _ShowFromPin, short _ShowToPin);
static int AbsSeriesCount(PixelSeries PixelSeries[SeriesS], int _Index);
void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID);

void setup()
{
	void Run(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount);
	void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID);

	#if UseWifi == true
		WiFi.mode(WIFI_STA);
		WiFi.begin(WifiName, WifiPassword);
		WiFi.config(ServerIP, Gateway, Subnet);
	#else
		Serial.begin(BaudRate, SERIAL_8N1);
	#endif

	short NumberOfPixels[LEDStripsS];
	for (short i = 0; i < LEDStripsS; i++)
		NumberOfPixels[i] = 0;

	PixelSeries PixelSerie[SeriesS];
	uint8_t PreviousColor[3] = { 255,255,255 };
	short SeriesIndex = 0;
	short TotalLEDCount = 0;

	short Split[SplitS];

	#if UseWifi == true
		while (WiFi.status() != WL_CONNECTED) {
			delay(1000);
		}
		WifiServer.begin();
		Client = WifiServer.available();
		while (!Client) {
			delay(500);
			Client = WifiServer.available();
			Client.setTimeout(-1);
		}
	#endif

	SendToCurrentDevice("R");

	while (true)
	{
		if (ReadData(Split))
		{
			if (Split[1] != 9999)
			{
				NumberOfPixels[Split[2]] = Split[1];
			}
			else
				break;
			SendToCurrentDevice("0");
		}
	}
	SendToCurrentDevice("0");

	while (true)
	{
		if (ReadData(Split))
		{
			if (Split[1] != 9999)
			{
				PixelSerie[SeriesIndex] = PixelSeries(Split[1], Split[2], Split[3]);
				SeriesIndex++;
			}
			else
				break;
			SendToCurrentDevice("1");
		}
	}
	SendToCurrentDevice("1");

	NeoPixelBus<ColorFeature, LEDMethod>* LEDStrips[LEDStripsS];
	for (int i = 0; i < LEDStripsS; i++)
	{
		#if UseWifi == true
			LEDStrips[i] = new NeoPixelBus<ColorFeature, LEDMethod>(NumberOfPixels[i]);
		#else
			LEDStrips[i] = new NeoPixelBus<ColorFeature, LEDMethod>(NumberOfPixels[i], i);
		#endif
	}

	for (short i = 0; i < SeriesIndex; i++)
	{
		TotalLEDCount += AbsSeriesCount(PixelSerie, i);
	}

	for (short i = 0; i < LEDStripsS; i++)
	{
		if (LEDStrips[i]->PixelCount() > 0)
		{
			LEDStrips[i]->Begin();
			LEDStrips[i]->Show();
		}
	}

	ColorEntireStripFromTo(0, TotalLEDCount, 255, 255, 255, 5, LEDStrips, PixelSerie, -2, SeriesIndex, 0);

	SendToCurrentDevice("2");

	Run(LEDStrips, Split, PreviousColor, SeriesIndex, PixelSerie, TotalLEDCount);
}

void Run(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount)
{
	void Mode_F(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
	void Mode_B(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
	void Mode_W(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _CountToID, short _ShowFromPin, short _ShowToPin);
	void Mode_I(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS]);
	void Mode_S(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
	void Mode_R(short *_DiscardFromIndex, short *_DiscardToIndex, short *_CountFromID, short *_CountToID, short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short *_FromID, short *_ToID, short _TotalLEDCount, short _Split[SplitS], short *_ShowFromPin, short *_ShowToPin);
	void Mode_A(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin);
	void Mode_N(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], PixelSeries _PixelSeries[SeriesS], short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountToID, short _ShowFromPin, short _ShowToPin);

	short FromID = 0;
	short ToID = _TotalLEDCount;
	short DiscardFromIndex = 0;
	short DiscardToIndex = _SeriesIndex / 2;
	short CountToID = _TotalLEDCount;
	short CountFromID = 0;
	short ShowFromPin = 0;
	short ShowToPin = LEDStripsS - 1;

	while (true)
	{
		if (ReadData(_Split))
		{
			switch ((Mode)_Split[0]) {
			case FadeColors:
				Mode_F(_LEDStrips, _Split, _PreviousColor, _SeriesIndex, _PixelSeries, _TotalLEDCount, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountFromID, ShowFromPin, ShowToPin);
				break;
			case VisualizerBeat:
				Mode_B(_LEDStrips, _Split, _PreviousColor, _SeriesIndex, _PixelSeries, _TotalLEDCount, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountFromID, ShowFromPin, ShowToPin);
				break;
			case VisualizerWave:
				Mode_W(_LEDStrips, _Split, _SeriesIndex, _PixelSeries, _TotalLEDCount, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountFromID, CountToID, ShowFromPin, ShowToPin);
				break;
			case IndividualLEDs:
				Mode_I(_LEDStrips, _Split);
				break;
			case VisualizerFullSpectrum:
				Mode_S(_LEDStrips, _Split, _PreviousColor, _SeriesIndex, _PixelSeries, _TotalLEDCount, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountFromID, ShowFromPin, ShowToPin);
				break;
			case Ranges:
				Mode_R(&DiscardFromIndex, &DiscardToIndex, &CountFromID, &CountToID, _SeriesIndex, _PixelSeries, &FromID, &ToID, _TotalLEDCount, _Split, &ShowFromPin, &ShowToPin);
				break;
			case Ambilight:
				Mode_A(_LEDStrips, _Split, _SeriesIndex, _PixelSeries, _TotalLEDCount, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountFromID, ShowFromPin, ShowToPin);
				break;
			case Animation:
				Mode_N(_LEDStrips, _Split, _PixelSeries, FromID, ToID, DiscardFromIndex, DiscardToIndex, CountToID, ShowFromPin, ShowToPin);
				break;
			}
			SendToCurrentDevice("3");
		}
	}
}

#pragma region Fade Colors Section

void Mode_F(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin)
{
	void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID);
	void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin);

	float CurrentColor[3] = { 0,0,0 };
	float CurrentColorJump[3] = { 0,0,0 };
	float JumpValue = ((float)_Split[5] / (float)100);

	for (short i = 0; i < 3; i++)
	{
		CurrentColorJump[i] = (((float)_PreviousColor[i] - (float)_Split[i + 1]) * JumpValue);
		CurrentColor[i] = _PreviousColor[i];
	}

	while ((CurrentColor[0] == _Split[1]) + (CurrentColor[1] == _Split[2]) + (CurrentColor[2] == _Split[3]) < 3)
	{
		for (short i = 0; i < 3; i++)
		{
			CurrentColor[i] -= CurrentColorJump[i];
			CurrentColorJump[i] = ((CurrentColor[i] - (float)_Split[i + 1]) * JumpValue);
			if (CurrentColor[i] < 0)
				CurrentColor[i] = 0;
			if (CurrentColor[i] > 255)
				CurrentColor[i] = 255;
			if (CurrentColorJump[i] < 0)
			{
				if (CurrentColorJump[i] >= -1)
					CurrentColor[i] = _Split[i + 1];
			}
			else
			{
				if (CurrentColorJump[i] <= 1)
					CurrentColor[i] = _Split[i + 1];
			}
		}
		ColorEntireStripFromTo(_FromID, _ToID, CurrentColor[0], CurrentColor[1], CurrentColor[2], 0, _LEDStrips, _PixelSeries, _DiscardFromIndex, _DiscardToIndex, _CountFromID);

		ShowStrips(_LEDStrips, _ShowFromPin, _ShowToPin);

		delay(_Split[4]);
	}
	_PreviousColor[0] = _Split[1];
	_PreviousColor[1] = _Split[2];
	_PreviousColor[2] = _Split[3];
}

#pragma endregion

#pragma region Visualizer Beat Mode

void Mode_B(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin)
{
	void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID);
	void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin);

	float JumpValue = ((float)_Split[1] / (float)99);
	ColorEntireStripFromTo(_FromID, _ToID, _PreviousColor[0] * JumpValue, _PreviousColor[1] * JumpValue, _PreviousColor[2] * JumpValue, 0, _LEDStrips, _PixelSeries, _DiscardFromIndex, _DiscardToIndex, _CountFromID);

	ShowStrips(_LEDStrips, _ShowFromPin, _ShowToPin);
}

#pragma endregion

#pragma region Visualizer Wave Mode

void Mode_W(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _CountToID, short _ShowFromPin, short _ShowToPin)
{
	void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin);

	short CurrentIndex = _CountToID;
	for (short j = _DiscardToIndex; j >= _DiscardFromIndex - 1; j--)
	{
		int SeriesCount = AbsSeriesCount(_PixelSeries, j);

		for (short i = SeriesCount; i >= 0; i--)
		{
			if (CurrentIndex - (SeriesCount - i) >= _FromID)
			{
				if (CurrentIndex - (SeriesCount - i) <= _ToID)
				{
					if (_PixelSeries[j].ToID > _PixelSeries[j].FromID)
					{
						if (i == 0)
						{
							if (j != _DiscardFromIndex)
							{
								if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID + i, _LEDStrips[_PixelSeries[j - 1].PinID]->GetPixelColor(_PixelSeries[j - 1].ToID));
								else
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID - i, _LEDStrips[_PixelSeries[j - 1].PinID]->GetPixelColor(_PixelSeries[j - 1].ToID));
							}
						}
						else
							_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID + i, _LEDStrips[_PixelSeries[j].PinID]->GetPixelColor(_PixelSeries[j].FromID + i - 1));
					}
					else
					{
						if (i == 0)
						{
							if (j != _DiscardFromIndex)
							{
								if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID + i, _LEDStrips[_PixelSeries[j - 1].PinID]->GetPixelColor(_PixelSeries[j - 1].ToID));
								else
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID - i, _LEDStrips[_PixelSeries[j - 1].PinID]->GetPixelColor(_PixelSeries[j - 1].ToID));
							}
						}
						else
							_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID - i, _LEDStrips[_PixelSeries[j].PinID]->GetPixelColor(_PixelSeries[j].FromID - i + 1));
					}
				}
			}
		}

		CurrentIndex -= SeriesCount - 1;
	}

	if (_PixelSeries[_DiscardFromIndex].ToID > _PixelSeries[_DiscardFromIndex].FromID)
		_LEDStrips[_PixelSeries[_DiscardFromIndex].PinID]->SetPixelColor(_PixelSeries[_DiscardFromIndex].FromID + (_FromID - _CountFromID), RgbColor(_Split[1], _Split[2], _Split[3]));
	else
		_LEDStrips[_PixelSeries[_DiscardFromIndex].PinID]->SetPixelColor(_PixelSeries[_DiscardFromIndex].FromID - (_FromID - _CountFromID), RgbColor(_Split[1], _Split[2], _Split[3]));

	ShowStrips(_LEDStrips, _ShowFromPin, _ShowToPin);
}

#pragma endregion

#pragma region Individual LED Mode

void Mode_I(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS])
{
	_LEDStrips[_Split[1]]->SetPixelColor(_Split[2], RgbColor(_Split[3], _Split[4], _Split[5]));
	_LEDStrips[_Split[1]]->Show();
}

#pragma endregion

#pragma region Visualizer Spectrum Mode

void Mode_S(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], uint8_t _PreviousColor[3], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin)
{
	void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID);
	void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin);

	ColorEntireStripFromTo(_FromID, _ToID, 0, 0, 0, 0, _LEDStrips, _PixelSeries, _DiscardFromIndex, _DiscardToIndex, _CountFromID);

	short Count = 2;
	short CurrentIndex = _CountFromID;
	int OverCount = 0;

	for (short j = _DiscardFromIndex; j <= _DiscardToIndex; j++)
	{
		int SeriesCount = AbsSeriesCount(_PixelSeries, j);

		for (short i = OverCount; i <= SeriesCount; i += _Split[1])
		{
			OverCount = 0;
			if (CurrentIndex + i >= _FromID)
			{
				if (CurrentIndex + i <= _ToID)
				{
					for (short l = i; l < i + _Split[Count]; l++)
					{
						if (_PixelSeries[j].ToID > _PixelSeries[j].FromID)
						{
							if (_PixelSeries[j].FromID + l > _PixelSeries[j].ToID)
							{
								if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
									_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j + 1].FromID + OverCount, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								else
									_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j + 1].FromID - OverCount, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								OverCount++;
							}
							else
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID + l, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
						}
						else
						{
							if (_PixelSeries[j].FromID - l < _PixelSeries[j].ToID)
							{
								if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
									_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j + 1].FromID + OverCount, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								else
									_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j + 1].FromID - OverCount, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								OverCount++;
							}
							else
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(_PixelSeries[j].FromID - l, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
						}
					}
					CurrentIndex += _Split[1];
					if (Count > SplitS)
						break;
					Count++;
				}
			}
		}
		//CurrentIndex += SeriesCount - 1;

		/*if (_PixelSeries[j].ToID > _PixelSeries[j].FromID)
		{
			for (int i = _PixelSeries[j].FromID + abs(CurrentSplitIndex - CurrentIndex); i <= _PixelSeries[j].ToID + _Split[1]; i += _Split[1])
			{
				if (i >= _PixelSeries[j].FromID)
				{
					if (CurrentSplitIndex >= _FromID)
					{
						if (CurrentSplitIndex + _Split[1] <= _ToID)
						{
							for (short l = i; l < i + _Split[Count]; l++)
							{
								if (l > _PixelSeries[j].ToID)
								{
									if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
										_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j].FromID + abs(_PixelSeries[j].ToID - l) - 1, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
									else
										_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j].FromID - abs(_PixelSeries[j].ToID - l) + 1, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								}
								else
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
							}
						}
					}
				}
				CurrentSplitIndex += _Split[1];
				Count++;
				if (Count > SplitS)
					break;
			}
		}
		else
		{
			for (int i = _PixelSeries[j].FromID - abs(CurrentSplitIndex - CurrentIndex); i >= _PixelSeries[j].ToID - _Split[1]; i -= _Split[1])
			{
				if (i <= _PixelSeries[j].FromID)
				{
					if (CurrentSplitIndex >= _FromID)
					{
						if (CurrentSplitIndex + _Split[1] <= _ToID)
						{
							for (short l = i; l > i - _Split[Count]; l--)
							{
								if (l < _PixelSeries[j].ToID)
								{
									if (_PixelSeries[j + 1].ToID > _PixelSeries[j + 1].FromID)
										_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j].FromID + abs(_PixelSeries[j].ToID - l) - 1, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
									else
										_LEDStrips[_PixelSeries[j + 1].PinID]->SetPixelColor(_PixelSeries[j].FromID - abs(_PixelSeries[j].ToID - l) + 1, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
								}
								else
									_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(_PreviousColor[0], _PreviousColor[1], _PreviousColor[2]));
							}
						}
					}
				}
				CurrentSplitIndex += _Split[1];
				Count++;
				if (Count > SplitS)
					break;
			}
		}
		CurrentIndex += abs(_PixelSeries[j].FromID - _PixelSeries[j].ToID) + 1;*/
	}

	ShowStrips(_LEDStrips, _ShowFromPin, _ShowToPin);
}

#pragma endregion

#pragma region Ranging Section

void Mode_R(short *_DiscardFromIndex, short *_DiscardToIndex, short *_CountFromID, short *_CountToID, short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short *_FromID, short *_ToID, short _TotalLEDCount, short _Split[SplitS], short *_ShowFromPin, short *_ShowToPin)
{
	*_FromID = 0;
	*_ToID = _TotalLEDCount;
	*_CountToID = _TotalLEDCount;
	*_CountFromID = 0;
	*_ShowFromPin = LEDStripsS;
	*_ShowToPin = 0;

	if (_Split[1] >= 0)
		if (_Split[1] < _TotalLEDCount)
			*_FromID = _Split[1];
	if (_Split[2] > 0)
		if (_Split[2] <= _TotalLEDCount)
			*_ToID = _Split[2];
	if (_Split[2] == -1)
		*_ToID = _TotalLEDCount;

	for (short i = 0; i <= _SeriesIndex - 1; i++)
	{
		*_CountFromID += AbsSeriesCount(_PixelSeries, i);
		if (*_CountFromID > *_FromID)
		{
			*_CountFromID -= AbsSeriesCount(_PixelSeries, i);
			*_DiscardFromIndex = i;
			break;
		}
	}

	for (short i = _SeriesIndex - 1; i >= 0; i--)
	{
		*_CountToID -= AbsSeriesCount(_PixelSeries, i);
		if (*_CountToID <= *_ToID - 1)
		{
			*_CountToID += AbsSeriesCount(_PixelSeries, i);
			*_DiscardToIndex = i;
			break;
		}
	}

	for (short i = *_DiscardFromIndex; i <= *_DiscardToIndex; i++)
	{
		if (_PixelSeries[i].PinID < *_ShowFromPin)
			*_ShowFromPin = _PixelSeries[i].PinID;
		if (_PixelSeries[i].PinID > *_ShowToPin)
			*_ShowToPin = _PixelSeries[i].PinID;
	}
}

#pragma endregion

#pragma region Ambilight Section

void Mode_A(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], short _SeriesIndex, PixelSeries _PixelSeries[SeriesS], short _TotalLEDCount, short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID, short _ShowFromPin, short _ShowToPin)
{
	short Count = 4;
	short CurrentIndex = _CountFromID;
	short RVal, GVal, BVal;

	for (short j = _DiscardFromIndex; j <= _DiscardToIndex; j++)
	{
		if (_PixelSeries[j].ToID > _PixelSeries[j].FromID)
		{
			if (_Split[2] > _Split[1])
			{
				for (int i = _PixelSeries[j].FromID; i <= _PixelSeries[j].ToID; i++)
				{
					if (CurrentIndex + (i - _PixelSeries[j].FromID) >= _Split[1])
					{
						if (CurrentIndex + (i - _PixelSeries[j].FromID) + _Split[3] <= _Split[2])
						{
							int InputVal = _Split[Count];
							if (InputVal >= 111)
							{
								RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
								GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
								BVal = ((255 / 8) * ((InputVal % 10) - 1));
							}
							else
							{
								if (InputVal >= 11)
								{
									RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									BVal = (255 / 8) * ((InputVal % 10) - 1);
								}
								else
								{
									RVal = (255 / 8) * (InputVal - 1);
									GVal = (255 / 8) * (InputVal - 1);
									BVal = (255 / 8) * (InputVal - 1);
								}
							}
							for (int l = i; l < i + _Split[3]; l++)
							{
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(RVal, GVal, BVal));
							}
							Count++;
							i += _Split[3] - 1;
						}
						else
							break;
					}
				}
				if (CurrentIndex >= _Split[2])
					break;
			}
			else
			{
				for (int i = _PixelSeries[j].ToID; i >= _PixelSeries[j].FromID; i--)
				{
					if (CurrentIndex + (i - _PixelSeries[j].FromID) <= _Split[1])
					{
						if (CurrentIndex + (i - _PixelSeries[j].FromID) + _Split[3] >= _Split[2])
						{
							int InputVal = _Split[Count];
							if (InputVal >= 111)
							{
								RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
								GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
								BVal = ((255 / 8) * ((InputVal % 10) - 1));
							}
							else
							{
								if (InputVal >= 11)
								{
									RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									BVal = (255 / 8) * ((InputVal % 10) - 1);
								}
								else
								{
									RVal = (255 / 8) * (InputVal - 1);
									GVal = (255 / 8) * (InputVal - 1);
									BVal = (255 / 8) * (InputVal - 1);
								}
							}
							for (int l = i; l > i - _Split[3]; l--)
							{
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(RVal, GVal, BVal));
							}
							Count++;
							i -= _Split[3] - 1;
						}
						else
							break;
					}
				}
				if (CurrentIndex >= _Split[1])
					break;
			}
		}
		else
		{
			if (_Split[2] > _Split[1])
			{
				for (int i = _PixelSeries[j].FromID; i >= _PixelSeries[j].ToID; i--)
				{
					if (CurrentIndex + (_PixelSeries[j].FromID - i) >= _Split[1])
					{
						if (CurrentIndex + (_PixelSeries[j].FromID - i) + _Split[3] <= _Split[2])
						{
							int InputVal = _Split[Count];
							if (InputVal >= 111)
							{
								RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
								GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
								BVal = ((255 / 8) * ((InputVal % 10) - 1));
							}
							else
							{
								if (InputVal >= 11)
								{
									RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									BVal = (255 / 8) * ((InputVal % 10) - 1);
								}
								else
								{
									RVal = (255 / 8) * (InputVal - 1);
									GVal = (255 / 8) * (InputVal - 1);
									BVal = (255 / 8) * (InputVal - 1);
								}
							}
							for (int l = i; l > i - _Split[3]; l--)
							{
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(RVal, GVal, BVal));
							}
							Count++;
							i -= _Split[3] - 1;
						}
						else
							break;
					}
				}
				if (CurrentIndex >= _Split[2])
					break;
			}
			else
			{
				for (int i = _PixelSeries[j].ToID; i <= _PixelSeries[j].FromID; i++)
				{
					if (CurrentIndex + (_PixelSeries[j].FromID - i) <= _Split[1])
					{
						if (CurrentIndex + (_PixelSeries[j].FromID - i) - _Split[3] >= _Split[2])
						{
							int InputVal = _Split[Count];
							if (InputVal >= 111)
							{
								RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
								GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
								BVal = ((255 / 8) * ((InputVal % 10) - 1));
							}
							else
							{
								if (InputVal >= 11)
								{
									RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
									BVal = (255 / 8) * ((InputVal % 10) - 1);
								}
								else
								{
									RVal = (255 / 8) * (InputVal - 1);
									GVal = (255 / 8) * (InputVal - 1);
									BVal = (255 / 8) * (InputVal - 1);
								}
							}
							for (int l = i; l < i + _Split[3]; l++)
							{
								_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor(l, RgbColor(RVal, GVal, BVal));
							}
							Count++;
							i += _Split[3] - 1;
						}
						else
							break;
					}
				}
				if (CurrentIndex >= _Split[1])
					break;
			}
		}
		CurrentIndex += abs(_PixelSeries[j].FromID - _PixelSeries[j].ToID) + 1;
	}

	for (short i = _ShowFromPin; i <= _ShowToPin; i++)
	{
		if (_LEDStrips[i]->PixelCount() > 0)
			_LEDStrips[i]->Show();
	}
}

#pragma endregion

#pragma region Animation Region

void Mode_N(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _Split[SplitS], PixelSeries _PixelSeries[SeriesS], short _FromID, short _ToID, short _DiscardFromIndex, short _DiscardToIndex, short _CountToID, short _ShowFromPin, short _ShowToPin)
{
	void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin);

	short CurrentIndex = _CountToID;
	short Count = 4;
	short RVal, GVal, BVal;
	
	for (short i = _DiscardToIndex; i >= _DiscardFromIndex; i--)
	{
		if (_PixelSeries[i].ToID > _PixelSeries[i].FromID)
		{
			for (short j = _PixelSeries[i].ToID; j >= _PixelSeries[i].FromID; j--)
			{
				if (CurrentIndex - (_PixelSeries[i].ToID - j) >= _FromID)
				{
					if (CurrentIndex - (_PixelSeries[i].ToID - j) <= _ToID)
					{
						if (CurrentIndex - (_PixelSeries[i].ToID - j) <= _FromID + _Split[1])
						{
							if (_Split[2] == 1)
							{
								int InputVal = _Split[Count];
								if (InputVal >= 111)
								{
									RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
									GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
									BVal = ((255 / 8) * ((InputVal % 10) - 1));
								}
								else
								{
									if (InputVal >= 11)
									{
										RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
										GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
										BVal = (255 / 8) * ((InputVal % 10) - 1);
									}
									else
									{
										RVal = (255 / 8) * (InputVal - 1);
										GVal = (255 / 8) * (InputVal - 1);
										BVal = (255 / 8) * (InputVal - 1);
									}
								}
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, RgbColor(RVal, GVal, BVal));
								Count++;
							}
							else
							{
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, RgbColor(_Split[Count], _Split[Count + 1], _Split[Count + 2]));
								Count += 3;
							}
						}
						else
						{
							if (j - _Split[1] < _PixelSeries[i].FromID)
							{
								short NewSeriesIndex = i;
								short CurrentJump = 0;
								while ((_Split[1] - (j - _PixelSeries[i].FromID)) - CurrentJump > 0)
								{
									NewSeriesIndex -= 2;
									CurrentJump += AbsSeriesCount(_PixelSeries, NewSeriesIndex);
								}
								CurrentJump = (_Split[1] - (j - _PixelSeries[i].FromID)) - (CurrentJump - AbsSeriesCount(_PixelSeries, NewSeriesIndex));

								if (_PixelSeries[NewSeriesIndex].ToID > _PixelSeries[NewSeriesIndex].FromID)
									_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[NewSeriesIndex].PinID]->GetPixelColor(_PixelSeries[NewSeriesIndex].ToID - CurrentJump + 1));
								else
									_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[NewSeriesIndex].PinID]->GetPixelColor(_PixelSeries[NewSeriesIndex].ToID + CurrentJump - 1));
							}
							else
							{
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[i].PinID]->GetPixelColor(j - _Split[1]));
							}
						}
					}
				}
			}
		}
		else
		{
			for (short j = _PixelSeries[i].ToID; j <= _PixelSeries[i].FromID; j++)
			{
				if (CurrentIndex - (j - _PixelSeries[i].ToID) >= _FromID)
				{
					if (CurrentIndex - (j - _PixelSeries[i].ToID) <= _ToID)
					{
						if (CurrentIndex - (j - _PixelSeries[i].ToID) <= _FromID + _Split[1])
						{
							if (_Split[2] == 1)
							{
								int InputVal = _Split[Count];
								if (InputVal >= 111)
								{
									RVal = ((255 / 8) * (((InputVal / 100) % 10) - 1));
									GVal = ((255 / 8) * (((InputVal / 10) % 10) - 1));
									BVal = ((255 / 8) * ((InputVal % 10) - 1));
								}
								else
								{
									if (InputVal >= 11)
									{
										RVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
										GVal = (255 / 8) * (((InputVal / 10) % 10) - 1);
										BVal = (255 / 8) * ((InputVal % 10) - 1);
									}
									else
									{
										RVal = (255 / 8) * (InputVal - 1);
										GVal = (255 / 8) * (InputVal - 1);
										BVal = (255 / 8) * (InputVal - 1);
									}
								}
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, RgbColor(RVal, GVal, BVal));
								Count++;
							}
							else
							{
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, RgbColor(_Split[Count], _Split[Count + 1], _Split[Count + 2]));
								Count += 3;
							}
						}
						else
						{
							if (j + _Split[1] > _PixelSeries[i].FromID)
							{
								short NewSeriesIndex = i;
								short CurrentJump = 0;
								while ((_Split[1] - (_PixelSeries[i].FromID - j)) - CurrentJump > 0)
								{
									NewSeriesIndex -= 2;
									CurrentJump += AbsSeriesCount(_PixelSeries, NewSeriesIndex);
								}
								CurrentJump = (_Split[1] - (_PixelSeries[i].FromID - j)) - (CurrentJump - AbsSeriesCount(_PixelSeries, NewSeriesIndex));

								if (_PixelSeries[NewSeriesIndex].ToID > _PixelSeries[NewSeriesIndex].FromID)
									_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[NewSeriesIndex].PinID]->GetPixelColor(_PixelSeries[NewSeriesIndex].ToID - CurrentJump + 1));
								else
									_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[NewSeriesIndex].PinID]->GetPixelColor(_PixelSeries[NewSeriesIndex].ToID + CurrentJump - 1));
							}
							else
							{
								_LEDStrips[_PixelSeries[i].PinID]->SetPixelColor(j, _LEDStrips[_PixelSeries[i].PinID]->GetPixelColor(j + _Split[1]));
							}
						}
					}
				}
			}
		}
		CurrentIndex -= (abs(_PixelSeries[i].FromID - _PixelSeries[i].ToID) + 1);
	}

	if (_Split[3] == 1)
	{
		ShowStrips(_LEDStrips, _ShowFromPin, _ShowToPin);
	}
}

#pragma endregion

void ColorEntireStripFromTo(short _FromID, short _ToID, short _Red, short _Green, short _Blue, short _Delay, NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], PixelSeries _PixelSeries[SeriesS], short _DiscardFromIndex, short _DiscardToIndex, short _CountFromID)
{
	short CurrentIndex = _CountFromID;
	for (short j = _DiscardFromIndex; j <= _DiscardToIndex; j++)
	{
		int SeriesCount = AbsSeriesCount(_PixelSeries, j);

		for (short i = 0; i <= SeriesCount; i++)
		{
			if (CurrentIndex + i >= _FromID)
			{
				if (CurrentIndex + i <= _ToID)
				{
					if (_PixelSeries[j].ToID > _PixelSeries[j].FromID)
						_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor((_PixelSeries[j].FromID + i), RgbColor(_Red, _Green, _Blue));
					else
						_LEDStrips[_PixelSeries[j].PinID]->SetPixelColor((_PixelSeries[j].FromID - i), RgbColor(_Red, _Green, _Blue));

					if (_Delay > 0)
					{
						_LEDStrips[_PixelSeries[j].PinID]->Show();
						delay(_Delay);
					}
				}
			}
		}
		CurrentIndex += SeriesCount - 1;
	}
}

static int AbsSeriesCount(PixelSeries PixelSeries[SeriesS], int _Index)
{
	return abs(PixelSeries[_Index].ToID - PixelSeries[_Index].FromID) + 1;
}

void ShowStrips(NeoPixelBus<ColorFeature, LEDMethod> *_LEDStrips[LEDStripsS], short _ShowFromPin, short _ShowToPin)
{
	for (short i = _ShowFromPin; i <= _ShowToPin; i++)
	{
		if (_LEDStrips[i]->PixelCount() > 0)
			_LEDStrips[i]->Show();
	}
}

bool ReadData(short _Split[SplitS])
{
	#if UseWifi == true
		if (Client.connected())
		{
			if (Client.available() > 2)
			{
				for (short i = 0; i < SplitS; i++)
					_Split[i] = 0;

				short Step = 0;

				while (true)
				{
					if (Client.available() > 0)
					{
						int Value = Client.parseInt();
						if (Value == -1)
						{
							Client.read();
							break;
						}
						_Split[Step] = Value;
						Step++;
						if (Step >= SplitS)
						{
							Client.read();
							return false;
						}
					}
					else
						break;
				}
				return true;
			}
		}
		else
		{
			Client.stop();
			while (!Client) {
				delay(500);
				Client = WifiServer.available();
				Client.setTimeout(-1);
			}
		}
	#else
		if (Serial.available() > 2)
		{
			for (short i = 0; i < SplitS; i++)
				_Split[i] = 0;

			short Step = 0;

			while (true)
			{
				if (Serial.available() > 0)
				{
					int Value = Serial.parseInt();
					if (Value == -1)
					{
						Serial.read();
						break;
					}
					_Split[Step] = Value;
					Step++;
					if (Step >= SplitS)
					{
						Serial.read();
						return false;
					}
				}
				else
					break;
			}
			return true;
		}
	#endif
	return false;
}

void SendToCurrentDevice(char* _Input)
{
	#if UseWifi == true
		if (Client) {
			Client.write(_Input);
		}
	#else
		Serial.write(_Input);
	#endif
}

void loop() { }