using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using ProgressControls;
using Nova.Control;
using Nova.LCT.GigabitSystem.Monitor;
using Nova.LCT.Message.Client;
using Nova.LCT.GigabitSystem.Common;
using Nova.Message.Common;
using Nova.Equipment.Protocol.TGProtocol;
using Nova.Process;
using Nova.LCT.GigabitSystem.CommonInfoAccessor;
using Nova.IO.Port;
using Nova.Convert;
using Nova.Control.DoubleBufferControl;
using Nova.Resource.Language;
using Nova.Drawing;
using Nova.Xml.Serialization;

namespace Nova.Monitoring.UI.ScannerStatusDisplay
{
    public partial class MarsScreenMonitorDataViewer : UserControl
    {
        #region ��̬����
        public static FontAdjuster AdjustFontObj = new FontAdjuster();
        private static Font CurrentFont = null;
        public static object _bindDataObj = new object();
        #endregion

        #region ˽���ֶ�
        private enum OperateStep
        {
            ConfigScanBoard = 0,
            UpdateMonitorData,
            RefreshStatisticsInfo,
            SetScaleRatio,
            SetGridFont,
            None
        }
        public class TempAndHumiInfo
        {
            public List<string> MinSBKeyList = new List<string>();
            public List<string> MaxSBKeyList = new List<string>();
        }

        #region ����UC
        private CustomToolTip _customToolTip = null;
        private ProgressIndicator _indicator = new ProgressIndicator();
        private UC_StatusRectangleLayout _statusRectangleLayout = null;
        /// <summary>
        /// �����ǰֻ��һ��������ʱ�������ø������ؼ���ʾ
        /// </summary>
        private UC_ComplexScreenLayout _complexScreenLayout = null;
        private UC_StatusInfo _sbStatusInfoUC = null;
        private UC_StatusInfo _mcStatusInfoUC = null;
        private UC_SmokeFanPowerInfo _smokeInfo = null;
        private UC_SmokeFanPowerInfo _fanInfoUC = null;
        private UC_SmokeFanPowerInfo _powerInfoUC = null;
        private UC_RowLineCabinetDoorInfo _rowLineInfoUC = null;
        private UC_RowLineCabinetDoorInfo _generalStatuInfoUC = null;
        private UC_TempAndHumiInfo _tempInfoUC = null;
        private UC_TempAndHumiInfo _humidityInfoUC = null;
        private UC_TempAndHumidityCorNotice _tempCorNoticeUC = null;
        private UC_TempAndHumidityCorNotice _humidityCorNoticeUC = null;
        private UC_StatusCorNotice _sbStatusCorNotice = null;
        private UC_SmokeFanPowerCorNotice _smokeOrPowerCorNotice = null;
        private UC_SmokeFanPowerCorNotice _fanCorNotice = null;
        private UC_RowLineCorNotice _rowLineCorNotice = null;
        private UC_StatusCorNotice _mcStatusCorNotice = null;
        private UC_SmokeFanPowerCorNotice _generalStatusCorNotice = null;
        #endregion

        private Frm_ComplexScreenDisplay _complexScreenPreviewFrm = null;
        //������ֵ��ȡ��Ӧ����ɫ
        private ColorGradePartition _clrGradePartition = null;
        private List<ILEDDisplayInfo> _ledInfos = new List<ILEDDisplayInfo>();
        private Dictionary<string, List<ScreenGridBindObj>> _curAllLedScreenRectDic = null;
        private static readonly char SCANBORDADDR_SEPERATE = '-';
        //ˢ���̵߳�Lock����
        private object _updateCurSreenObj = new object();
        //�¶ȶ�Ӧ�����ȼ�
        private static int MaxTempValue = 80;
        //�¶ȶ�Ӧ����С�ȼ�
        private static int MinTempValue = -20;
        //ʪ�ȶ�Ӧ�����ȼ�
        private static int MaxHumidityValue = 100;
        //ʪ�ȶ�Ӧ����С�ȼ�
        private static int MinHumidityValue = 0;
        private SimpleLEDDisplayInfo _simpleScreenInfo = null;
        private StandardLEDDisplayInfo _standardScreenInfo = null;
        private ComplexLEDDisplayInfo _complexScreenInfo = null;
        private ScanBoardRegionInfo _tempScanBoardInfo = null;
        // ��ǰ��ʾ������
        private MonitorDisplayType _curType = MonitorDisplayType.SBStatus;
        private bool _isDisplayAll = false;

        #region ������ʾ��Ϣ�洢����
        //��ǰ���տ�����
        private int _curScanBordCount = 0;

        //��ǰ��ʾ��������·��
        private uint _totalFanSwitchCount = 0;
        //��ǰ��ʾ����Դ��·��
        private uint _totalPowerSwitchCount = 0;

        private TempAndHumiStatisticsInfo _tempStatisInfo = null;
        private TempAndHumiStatisticsInfo _humiStatisInfo = null;

        private MonitorErrData _curFaultInfo = new MonitorErrData();
        private MonitorErrData _curAlarmInfo = new MonitorErrData();
        private MonitorInfoResult _monitorResType;
        #endregion

        //��׼��ʾ���ͼ���ʱ��ÿ��������������ű���
        private static float DefaultLengthOnePixel = 0.4f;

        #region �����ʴ洢����
        private int _allRatio = 40;
        private int _sbStatusRatio = 40;
        private int _temperatureRatio = 40;
        private int _humidityRatio = 40;
        private int _smokeRatio = 40;
        private int _fanRatio = 40;
        private int _powerRatio = 40;
        private int _rowLineRatio = 40;
        private int _mcStatusRatio = 40;
        private int _generalStatusRatio = 40;
        #endregion

        private object _configCurScreenObj = new object();
        private object _addScanBoardObj = new object();
        private object _updateMonitorInfoObj = new object();
        private object _curScreenInfoChangedObj = new object();
        private bool _bConfigScanBoardAgain = false;
        private bool _isNeedSetScaleRatio = false;

        private uint _curMCFanCnt = 0;
        private uint _curMCPowerCnt = 0;

        /// <summary>
        /// �������ؼ��󶨵ļ�����ݶ���
        /// </summary>
        private SBInfoAndMonitorInfo _complexScreenScanBoardMonitorInfo = null;
        private UC_ComplexScreenLayout _complexScreenLayoutTemp = null;

        /// <summary>
        /// �����ǰ�������ý��տ��ĸ�����Ϣ���첽�̣߳�������ͬʱ������UpdateMonitorInfo������
        /// ���ʱ��Ҫ��¼��Ҫ�󶨵ļ�����ݣ����ڸ�����Ϣ�����ɺ�����ñ�������ÿ�����Ӱ󶨼�����ݡ�
        /// </summary>
        private Dictionary<string, Dictionary<string, ScannerMonitorData>> _updateMonitorDataForNotUpdateDic = null;
        /// <summary>
        /// �����ǰ�������ø�����Ϣ������ͬʱ������RefreshDisplayAndStatisticsInfo������
        /// ���ʱ��Ҫ�øñ�־�����������ɺ���Ҫͳ�Ƽ�����ݡ�
        /// </summary>
        private bool _isNeedStatisticsMonitorData = false;
        private DateTime _lastUpdateTime;

        private bool _isNeedUpdateFont = false;
        private Font _needUpdateFont = null;

        /// <summary>
        /// �ؼ���ǰִ�еĲ���
        /// </summary>
        private OperateStep _curOperateStep = OperateStep.None;
        /// <summary>
        /// ��ǰ�Ƿ�ֻ��һ����ʾ������Ϊ������
        /// </summary>
        private bool _isOnlyOneComplexScreen = false;
        private bool _isOnlySimpleOrStandarScreen = false;
        /// <summary>
        /// ��ǰֻ��һ��������Ϊ������ʱ����ʾ������
        /// </summary>
        private string _oniyOneComplexScreenCommPort = "";
        public byte _onlyOneScreenIndex = 0;
        /// <summary>
        /// ��ǰֻ��һ��������Ϊ������ʱ����ʾ����Ϣ
        /// </summary>
        private ComplexLEDDisplayInfo _onlyOneComplexScreenInfo = null;
        private bool _isHasValidMonitorInfo = false;
        private CtrlSytemMode _curCtrlSystemMode = CtrlSytemMode.HasSenderMode;

        #region �����С��ʪ��������Ϣ
        private string _curScanBoardKey = "";
        private TempAndHumiInfo _curTempMaxMinKeyInfo = null;
        private TempAndHumiInfo _curHumiMaxMinKeyInfo = null;
        private ClickLinkLabelType _curHightLightType = ClickLinkLabelType.None;
        #endregion

        #region ������Ϣ���
        private static string NormalInfoBeginStr = ": >>>>";
        private static string SeriousErrorBeginStr = ":@@@@@@@@";
        private static string NormalClassNameSeperate = "--";
        private static string NormalInfoEnd = "=====";
        private static string SeriousErrorInfEnd = "XXXXXX";
        private readonly string CLASS_NAME = "UC_OneScreenMonitorInfo";
        #endregion

        #region �������
        private Font _customToolTipFont = null;
        private Font _simpleOrStandardScreenFont = null;
        private Font _complexScreenFont = null;
        #endregion
        #endregion

        #region ����
        /// <summary>
        /// ��׼���ͼ���ʱ��Ӹ��ӣ�������ʾ��ʱ���datagridview������Ϣ����ɵ�֪ͨ
        /// </summary>
        public event EventHandler CompeleteConfigScanBoardDisplayEvent = null;
        /// <summary>
        /// �Ƿ���ʾ���м����Ϣ
        /// </summary>
        public bool IsDiplayAllType
        {
            get { return false; }
        }
        /// <summary>
        /// ��ǰ��ʾģʽ
        /// </summary>
        public MonitorDisplayType CurType
        {
            get { return _curType; }
            set
            {
                if (value == MonitorDisplayType.SenderDVI)
                {
                    return;
                }
                _curType = value;
                if (_curType == MonitorDisplayType.Temperature)
                {
                    switch (_curMonitorConfigInfo.TempDisplayType)
                    {
                        case TemperatureType.Celsius:
                            if (_curType == MonitorDisplayType.Temperature)
                            {
                                _clrGradePartition = new ColorGradePartition(MinTempValue, _curMonitorConfigInfo.TempAlarmThreshold, MaxTempValue);
                            }
                            break;
                        case TemperatureType.Fahrenheit:
                            int nMaxTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MaxTempValue);
                            int nMinTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MinTempValue);
                            int nThreshold = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, _curMonitorConfigInfo.TempAlarmThreshold);
                            if (_curType == MonitorDisplayType.Temperature)
                            {
                                _clrGradePartition = new ColorGradePartition(nMinTemp, nThreshold, nMaxTemp);
                            }
                            break;
                    }
                }
                else if (_curType == MonitorDisplayType.Humidity)
                {
                    _clrGradePartition = new ColorGradePartition(MinHumidityValue, _curMonitorConfigInfo.HumiAlarmThreshold, MaxHumidityValue);
                }

                //���¸�����ʾ�ؼ�����ʾ����
                _statusRectangleLayout.CurType = _curType;
                _statusRectangleLayout.CorGradePartition = _clrGradePartition;
                //���¸�������ʾ�ؼ�����ʾ����
                _complexScreenLayout.CurType = _curType;
                _complexScreenLayout.CorGradePartition = _clrGradePartition;

                if (_curOperateStep == OperateStep.None)
                {
                    //TODO  ���õ�ǰҳ����ʾ��������
                    SetCurScaleRatio();
                }
                else
                {
                    _isNeedSetScaleRatio = true;
                }

                //���ý����ϲ�ͬ��ʾ���Ϳؼ�������
                SetDisplayTypeControl();
            }
        }
        /// <summary>
        /// �Ƿ���°��صĵ�ѹֵ
        /// </summary>
        public bool IsDisplaySBValtage
        {
            get { return _curMonitorConfigInfo.IsDisplayScanBoardVolt; }
            set
            {
                _curMonitorConfigInfo.IsDisplayScanBoardVolt = value;

                //���ø�������ʾ�ؼ��Ƿ���½��տ���ѹ
                _complexScreenLayout.IsDisplaySBValt = value;
                //���ø��ӻ��ƿؼ��Ƿ���½��տ���ѹ
                _statusRectangleLayout.IsDisplayScanBoardValtage = value;
            }
        }
        /// <summary>
        /// ��ǰ������ʾ��������
        /// </summary>
        public int CurZoomRation
        {
            get { return vScrollBar_PixelLength.Value; }
            set
            {
                if (value < vScrollBar_PixelLength.Minimum
                    || value > vScrollBar_PixelLength.Maximum)
                {
                    return;
                }
                if (_isDisplayAll)
                {
                    _allRatio = vScrollBar_PixelLength.Value;
                    return;
                }
                switch (_curType)
                {
                    case MonitorDisplayType.SBStatus:
                        _sbStatusRatio = value;
                        break;
                    case MonitorDisplayType.MCStatus:
                        _mcStatusRatio = value;
                        break;
                    case MonitorDisplayType.Smoke:
                        _smokeRatio = value;
                        break;
                    case MonitorDisplayType.Temperature:
                        _temperatureRatio = value;
                        break;
                    case MonitorDisplayType.Humidity:
                        _humidityRatio = value;
                        break;
                    case MonitorDisplayType.Fan:
                        _fanRatio = value;
                        break;
                    case MonitorDisplayType.Power:
                        _powerRatio = value;
                        break;
                    case MonitorDisplayType.RowLine:
                        _rowLineRatio = value;
                        break;
                    case MonitorDisplayType.GeneralSwitch:
                        _generalStatusRatio = value;
                        break;
                }
                if (vScrollBar_PixelLength.Value == value)
                {
                    return;
                }
                if (_curOperateStep == OperateStep.None)
                {
                    //TODO  ���õ�ǰҳ����ʾ��������
                    vScrollBar_PixelLength.Value = value;
                    vScrollBar_PixelLength_Scroll(null, null);
                }
                else
                {
                    _isNeedSetScaleRatio = true;
                }
            }
        }
        /// <summary>
        /// �����Ͽؼ�������
        /// </summary>
        public Font CtrlOfFormFont
        {
            get { return CurrentFont; }
            set
            {
                CurrentFont = value;
                AdjustFontObj.Attach(this);
                if (_complexScreenPreviewFrm != null
                    && !_complexScreenPreviewFrm.IsDisposed)
                {
                    _complexScreenPreviewFrm.CtrlOfFormFont = value;
                }
                AdjustFontObj.UpdateFont(value);
            }
        }
        /// <summary>
        /// �������׼������ʾ����
        /// </summary>
        public Font SimpleOrStandardScreenFont
        {
            get { return _simpleOrStandardScreenFont; }
            set
            {
                _simpleOrStandardScreenFont = value;
                _statusRectangleLayout.DefaultFocusStyle.GridFont = value;
                _statusRectangleLayout.DefaultSelectedStyle.GridFont = value;
                _statusRectangleLayout.DefaultStyle.GridFont = value;

                _needUpdateFont = value;
                if (_curOperateStep != OperateStep.None)
                {
                    _isNeedUpdateFont = true;
                }
                else
                {
                    SetGridFont(value);
                    _statusRectangleLayout.InvalidateDrawArea();
                }
            }
        }
        /// <summary>
        /// ����������ʾ����
        /// </summary>
        public Font ComplexScreenFont
        {
            get { return _complexScreenFont; }
            set
            {
                _complexScreenFont = value;
                if (_complexScreenPreviewFrm != null
                    && !_complexScreenPreviewFrm.IsDisposed)
                {
                    _complexScreenPreviewFrm.MonitorInfoListFont = value;
                }
                if (_complexScreenLayout != null)
                {
                    _complexScreenLayout.MonitorInfoListFont = value;
                }
            }
        }
        /// <summary>
        /// ��ʾ��Ϣ������
        /// </summary>
        public Font CustomToolTipFont
        {
            get
            {
                if (_customToolTip != null)
                {
                    return _customToolTip.TipContentFont;
                }
                return _customToolTipFont;
            }
            set
            {
                _customToolTipFont = value;
                if (_customToolTip != null)
                {
                    _customToolTip.TipContentFont = value;
                }
                if (_complexScreenPreviewFrm != null
                    && !_complexScreenPreviewFrm.IsDisposed)
                {
                    _complexScreenPreviewFrm.CustomToolTipFont = value;
                }
                if (_complexScreenLayout != null)
                {
                    _complexScreenLayout.CustomToolTipFont = value;
                }
            }
        }
        /// <summary>
        /// ��ǰ����ϵͳ��ģʽ
        /// </summary>
        public CtrlSytemMode CurCtrlSystemMode
        {
            get { return _curCtrlSystemMode; }
            set
            {
                _curCtrlSystemMode = value;
                if (_complexScreenPreviewFrm != null
                    && !_complexScreenPreviewFrm.IsDisposed)
                {
                    _complexScreenPreviewFrm.CurCtrlSystemMode = value;
                }
                if (_complexScreenLayout != null)
                {
                    _complexScreenLayout.CurCtrlSystemMode = value;
                }
            }
        }

        /// <summary>
        /// �������������Ϣ
        /// </summary>
        private MonitorConfigData _curMonitorConfigInfo = null;
        public MonitorConfigData CurMonitorConfigInfo
        {
            get
            {
                return _curMonitorConfigInfo;
            }
            set
            {
                _curMonitorConfigInfo = value;
                //if (value == null) return;
                float fThreshold = _curMonitorConfigInfo.TempAlarmThreshold;
                #region �¶�����
                if (_curMonitorConfigInfo.TempDisplayType != value.TempDisplayType)
                {
                    int nMaxTemp = MaxTempValue;
                    int nMinTemp = MinTempValue;
                    int nThreshold = (int)fThreshold;
                    switch (value.TempDisplayType)
                    {
                        case TemperatureType.Celsius:
                            _tempCorNoticeUC.UpdateUnit(nMinTemp, fThreshold, nMaxTemp, CommonStaticValue.CelsiusUnit);
                            _tempInfoUC.UpdateUnit(CommonStaticValue.CelsiusUnit);
                            break;
                        case TemperatureType.Fahrenheit:
                            nMaxTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MaxTempValue);
                            nMinTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MinTempValue);
                            nThreshold = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, fThreshold);

                            _tempCorNoticeUC.UpdateUnit(nMinTemp, nThreshold, nMaxTemp, CommonStaticValue.FahrenheitUnit);
                            _tempInfoUC.UpdateUnit(CommonStaticValue.FahrenheitUnit);
                            break;
                        default:
                            return;
                    }
                    if (_curType == MonitorDisplayType.Temperature)
                    {
                        _clrGradePartition = new ColorGradePartition(nMinTemp, nThreshold, nMaxTemp);
                        _statusRectangleLayout.CorGradePartition = _clrGradePartition;
                        _complexScreenLayout.CorGradePartition = _clrGradePartition;
                    }
                    //���ø�������ʾ�ؼ��¶���ʾ����
                    _complexScreenLayout.TempDisplayType = value.TempDisplayType;
                    //���ø��ӻ��ƿؼ����¶���ʾ����
                    _statusRectangleLayout.TempDisplayType = value.TempDisplayType;
                }
                #endregion
                #region �¶�
                if (_curType == MonitorDisplayType.Temperature)
                {
                    switch (_curMonitorConfigInfo.TempDisplayType)
                    {
                        case TemperatureType.Celsius:
                            if (_curType == MonitorDisplayType.Temperature)
                            {
                                _clrGradePartition = new ColorGradePartition(MinTempValue, fThreshold, MaxTempValue);
                            }
                            break;
                        case TemperatureType.Fahrenheit:
                            int nMaxTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MaxTempValue);
                            int nMinTemp = (int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, MinTempValue);
                            fThreshold = GetMonitorColorAndValue.GetDisplayTempValueByCelsius(TemperatureType.Fahrenheit, fThreshold);
                            if (_curType == MonitorDisplayType.Temperature)
                            {
                                _clrGradePartition = new ColorGradePartition(nMinTemp, (int)fThreshold, nMaxTemp);
                            }
                            break;
                    }
                }
                _tempCorNoticeUC.CurThreshold = (int)fThreshold;

                //���¸��ӻ��ƿؼ����¶ȸ澯��ֵ
                _statusRectangleLayout.TempAlarmThreshold = fThreshold;
                _statusRectangleLayout.CorGradePartition = _clrGradePartition;
                //���¸�������ʾ�ؼ����¶ȸ澯��ֵ
                _complexScreenLayout.TempAlarmThreshold = fThreshold;
                _complexScreenLayout.CorGradePartition = _clrGradePartition;
                #endregion
                #region ʪ��
                _humidityCorNoticeUC.CurThreshold = value.HumiAlarmThreshold;
                if (_curType == MonitorDisplayType.Humidity)
                {
                    _clrGradePartition = new ColorGradePartition(MinHumidityValue,
                        _curMonitorConfigInfo.HumiAlarmThreshold, MaxHumidityValue);
                }
                //���ø�������ʾ�ؼ�ʪ�ȸ澯��ֵ
                _complexScreenLayout.HumiAlarmThreshold = value.HumiAlarmThreshold;
                _complexScreenLayout.CorGradePartition = _clrGradePartition;
                //���ø��ӻ��ƿؼ���ʪ����ֵ
                _statusRectangleLayout.HumidityThreshold = value.HumiAlarmThreshold;
                _statusRectangleLayout.CorGradePartition = _clrGradePartition;
                #endregion
                #region ���ȵ�Դ
                //���ø�������ʾ�ؼ����ȼ��������Ϣ
                _complexScreenLayout.MCFanInfo = value.MCFanInfo;
                //���ø��ӻ��ƿؼ����ȼ��������Ϣ
                _statusRectangleLayout.MCFanInfo = value.MCFanInfo;
                //���ø�������ʾ�ؼ���Դ���������Ϣ
                _complexScreenLayout.MCPowerInfo = value.MCPowerInfo;
                //���ø��ӻ��ƿؼ���Դ���������Ϣ
                _statusRectangleLayout.MCPowerInfo = value.MCPowerInfo;
                #endregion
            }
        }
        private string _screenSN = string.Empty;
        public string ScreenSN
        {
            get { return _screenSN; }
            set { _screenSN = value; }
        }
        private string _screenSNDisplay = string.Empty;

        public string ScreenSNDisplay
        {
            get { return _screenSNDisplay; }
            set { _screenSNDisplay = value; }
        }
        #endregion

        #region ���캯��
        /// <summary>
        /// ��ʾ��������ݹ���ؼ�  --- ��������List<ILEDDisplayInfo>�仯ΪDictionary<string, List<ILEDDisplayInfo>>��
        ///                             keyֵΪ�������ƣ����Ҹ�������Ϣʵ��Ϊ��Ҫ��ص�������Ϣ
        /// </summary>
        public MarsScreenMonitorDataViewer()
        {
            InitializeComponent();
            CurrentFont = this.Font;
            _customToolTipFont = this.Font;
            _complexScreenFont = this.Font;
            _simpleOrStandardScreenFont = this.Font;
            _needUpdateFont = this.Font;

            _updateMonitorDataForNotUpdateDic = new Dictionary<string, Dictionary<string, ScannerMonitorData>>();
            CompeleteConfigScanBoardDisplayEvent += new EventHandler(MarsScreenMonitorDataViewer_CompeleteConfigScanBoardDisplayEvent);

            _curMonitorConfigInfo = new MonitorConfigData();
            _curAllLedScreenRectDic = new Dictionary<string, List<ScreenGridBindObj>>();
            //_physicalScreenInfo = new PhysicalDisplayInfo();
            _curTempMaxMinKeyInfo = new TempAndHumiInfo();
            _curHumiMaxMinKeyInfo = new TempAndHumiInfo();
            _tempStatisInfo = new TempAndHumiStatisticsInfo();
            _humiStatisInfo = new TempAndHumiStatisticsInfo();
            _curOperateStep = OperateStep.None;

            _indicator.BackColor = Color.White;
            _indicator.CircleColor = Color.Blue;
            _indicator.AnimationSpeed = 50;

            #region ��ʼ������ؼ�
            _complexScreenLayout = new UC_ComplexScreenLayout();
            _complexScreenLayout.Parent = panel_Display;
            _complexScreenLayout.Dock = DockStyle.Fill;
            _complexScreenLayout.ClearAllMonitorInfo();
            _complexScreenLayout.CurType = _curType;

            _statusRectangleLayout = new UC_StatusRectangleLayout();
            _statusRectangleLayout.DefaultFocusStyle.BackColor = Color.White;
            _statusRectangleLayout.DefaultFocusStyle.BoardColor = Color.Yellow;
            _statusRectangleLayout.IsCanSelect = false;
            _statusRectangleLayout.Parent = panel_Display;
            _statusRectangleLayout.Dock = DockStyle.Fill;
            //_statusRectangleLayout.BorderStyle = BorderStyle.FixedSingle;
            _statusRectangleLayout.BackColor = Color.Transparent;
            _statusRectangleLayout.ClearAllRectangularGrid();
            _statusRectangleLayout.CurType = _curType;

            _sbStatusInfoUC = new UC_StatusInfo(CommonStaticValue.SBStatusDisplayStr[0],
                                                CommonStaticValue.SBStatusDisplayStr[1],
                                                CommonStaticValue.SBStatusDisplayStr[2]);
            _sbStatusInfoUC.Parent = panel_Info;
            _sbStatusInfoUC.Dock = DockStyle.Fill;

            _fanInfoUC = new UC_SmokeFanPowerInfo(CommonStaticValue.FanDisplayStr[0], CommonStaticValue.FanDisplayStr[1]);
            _fanInfoUC.Parent = panel_Info;
            _fanInfoUC.Dock = DockStyle.Fill;

            _powerInfoUC = new UC_SmokeFanPowerInfo(CommonStaticValue.PowerDisplayStr[0], CommonStaticValue.PowerDisplayStr[1]);
            _powerInfoUC.Parent = panel_Info;
            _powerInfoUC.Dock = DockStyle.Fill;

            _smokeInfo = new UC_SmokeFanPowerInfo(CommonStaticValue.SmokeDisplayStr[0], CommonStaticValue.SmokeDisplayStr[1]);
            _smokeInfo.Parent = panel_Info;
            _smokeInfo.Dock = DockStyle.Fill;

            _tempInfoUC = new UC_TempAndHumiInfo(CommonStaticValue.TemperatureDisplayStr[0], CommonStaticValue.TemperatureDisplayStr[1],
                                                 CommonStaticValue.TemperatureDisplayStr[2], CommonStaticValue.TemperatureDisplayStr[3],
                                                 CommonStaticValue.TemperatureDisplayStr[4], CommonStaticValue.CelsiusUnit,
                                                 new ClickMaxMinValueEventHandler(ClickTempMaxMinValue));
            _tempInfoUC.Parent = panel_Info;
            _tempInfoUC.Dock = DockStyle.Fill;

            _humidityInfoUC = new UC_TempAndHumiInfo(CommonStaticValue.HumidityDisplayStr[0], CommonStaticValue.HumidityDisplayStr[1],
                                                     CommonStaticValue.HumidityDisplayStr[2], CommonStaticValue.HumidityDisplayStr[3],
                                                     CommonStaticValue.HumidityDisplayStr[4], CommonStaticValue.HUMIDITY_UNIT,
                                                     new ClickMaxMinValueEventHandler(ClickHumiMaxMinValue));
            _humidityInfoUC.Parent = panel_Info;
            _humidityInfoUC.Dock = DockStyle.Fill;

            _rowLineInfoUC = new UC_RowLineCabinetDoorInfo(CommonStaticValue.RowLineStr[0], CommonStaticValue.RowLineStr[1]);
            _rowLineInfoUC.Parent = panel_Info;
            _rowLineInfoUC.Dock = DockStyle.Fill;

            _mcStatusInfoUC = new UC_StatusInfo(CommonStaticValue.MCStatusDisplayStr[0],
                                                CommonStaticValue.MCStatusDisplayStr[1],
                                                CommonStaticValue.MCStatusDisplayStr[2]);
            _mcStatusInfoUC.Parent = panel_Info;
            _mcStatusInfoUC.Dock = DockStyle.Fill;

            _generalStatuInfoUC = new UC_RowLineCabinetDoorInfo(CommonStaticValue.GeneralStatusStr[0], CommonStaticValue.GeneralStatusStr[1]);
            _generalStatuInfoUC.Parent = panel_Info;
            _generalStatuInfoUC.Dock = DockStyle.Fill;

            _tempCorNoticeUC = new UC_TempAndHumidityCorNotice(MinTempValue, _curMonitorConfigInfo.TempAlarmThreshold,
                                                               MaxTempValue, CommonStaticValue.CelsiusUnit);
            _tempCorNoticeUC.Parent = panel_ColorMotice;
            _tempCorNoticeUC.Dock = DockStyle.Fill;

            _humidityCorNoticeUC = new UC_TempAndHumidityCorNotice(MinHumidityValue, _curMonitorConfigInfo.HumiAlarmThreshold,
                                                                   MaxHumidityValue, CommonStaticValue.HUMIDITY_UNIT);
            _humidityCorNoticeUC.Parent = panel_ColorMotice;
            _humidityCorNoticeUC.Dock = DockStyle.Fill;

            _sbStatusCorNotice = new UC_StatusCorNotice();
            _sbStatusCorNotice.Parent = panel_ColorMotice;
            _sbStatusCorNotice.Dock = DockStyle.Fill;

            _mcStatusCorNotice = new UC_StatusCorNotice();
            _mcStatusCorNotice.Parent = panel_ColorMotice;
            _mcStatusCorNotice.Dock = DockStyle.Fill;

            _smokeOrPowerCorNotice = new UC_SmokeFanPowerCorNotice(MonitorDisplayType.Smoke);
            _smokeOrPowerCorNotice.Parent = panel_ColorMotice;
            _smokeOrPowerCorNotice.Dock = DockStyle.Fill;

            _fanCorNotice = new UC_SmokeFanPowerCorNotice(MonitorDisplayType.Fan);
            _fanCorNotice.Parent = panel_ColorMotice;
            _fanCorNotice.Dock = DockStyle.Fill;

            _rowLineCorNotice = new UC_RowLineCorNotice();
            _rowLineCorNotice.Parent = panel_ColorMotice;
            _rowLineCorNotice.Dock = DockStyle.Fill;

            _generalStatusCorNotice = new UC_SmokeFanPowerCorNotice(MonitorDisplayType.GeneralSwitch);
            _generalStatusCorNotice.Parent = panel_ColorMotice;
            _generalStatusCorNotice.Dock = DockStyle.Fill;

            _statusRectangleLayout.GridMouseDoubleClick += new RectangularGridMouseEventHandler(StatusRectangleLayout_GridMouseDoubleClick);
            _statusRectangleLayout.GridMouseMove += new RectangularGridMouseEventHandler(StatusRectangleLayout_GridMouseMove);
            #endregion

            this.Disposed += new EventHandler(MarsScreenMonitorDataViewer_Disposed);
            vScrollBar_PixelLength.Value = (int)(DefaultLengthOnePixel * 100);
            vScrollBar_PixelLength_Scroll(null, null);

            _isNeedSetScaleRatio = false;
            _isHasValidMonitorInfo = false;
            //���ý����ϲ�ͬ��ʾ���Ϳؼ�������
            SetDisplayTypeControl();
            this.CurCtrlSystemMode = _curCtrlSystemMode;
        }
        #endregion

        #region �ӿ�
        public void SetScreenMessage(string sn, string sn10, List<ILEDDisplayInfo> leds)
        {
            //string[] str = sn.Split('-');
            //_screenSN = str[0] + "-" + (System.Convert.ToInt32(str[1], 16) + 1).ToString("00");
            _screenSN = sn;
            _screenSNDisplay = sn10;
            //TODO  ���ý��տ��ĸ�����Ϣ
            SetDisplayInfo(leds);
        }

        /// <summary>
        /// ˢ�¼��״̬��Ϣ���ýӿ���Ҫ���ڣ������Ϣ�仯����δ���ü�����ݴ洢�����ǵ���ˢ��  -- ֻ��ˢ��һ�����ڵļ�����ݣ�
        /// </summary>
        /// <param name="monitorDataDict"></param>
        public void UpdateMonitorInfo(DateTime updateCompleteTime, string commPort, SerializableDictionary<string, ScannerMonitorData> monitorDataDict)
        {
            _lastUpdateTime = updateCompleteTime;
            if (_curOperateStep != OperateStep.None)
            {
                if (!_updateMonitorDataForNotUpdateDic.ContainsKey(commPort))
                {
                    _updateMonitorDataForNotUpdateDic.Add(commPort, monitorDataDict);
                }
                else
                {
                    _updateMonitorDataForNotUpdateDic[commPort] = monitorDataDict;
                }
                return;
            }
            _curOperateStep = OperateStep.UpdateMonitorData;

            label_NoneUpdate.Visible = false;
            label_LastUpdateTimeValue.Visible = true;
            label_LastUpdateTimeValue.Text = _lastUpdateTime.ToLongTimeString();
            //TODO  ��ÿ�����տ��ĸ��Ӱ󶨼�����ݡ�
            BindingScanBoardMonitorInfo(commPort, monitorDataDict);
            _curOperateStep = OperateStep.None;
        }
        /// <summary>
        /// ˢ�¼��״̬��Ϣ���ýӿ���Ҫ���ڣ���������Ѿ����µ������ϣ�ֻ���л��������ʾ����ʱ����ˢ�¡�
        /// </summary>
        public void RefreshDisplayAndStatisticsInfo()
        {
            _curHightLightType = ClickLinkLabelType.None;
            if (_curOperateStep != OperateStep.None)
            {
                _isNeedStatisticsMonitorData = true;
                return;
            }
            _isNeedStatisticsMonitorData = false;
            _curOperateStep = OperateStep.RefreshStatisticsInfo;

            //TODO ͳ�ƽ��տ���Ӧ���ϸ澯��Ϣ
            StatisticsFaultAndAlarmInfo();
            if (_isNeedUpdateFont)
            {
                _curOperateStep = OperateStep.SetGridFont;
                _isNeedUpdateFont = false;
                SetGridFont(_needUpdateFont);
            }
            if (_isNeedSetScaleRatio)
            {
                _curOperateStep = OperateStep.SetScaleRatio;
                _isNeedSetScaleRatio = false;
                SetCurScaleRatio();
            }
            //TODO  ������ʾ�ĸ�����Ϣ
            if (!_isOnlyOneComplexScreen)
            {
                this._statusRectangleLayout.InvalidateDrawArea();
            }
            else
            {
                this._complexScreenLayout.InvalidateAllScanBoardInfo();
            }
            _curOperateStep = OperateStep.None;
        }
        /// <summary>
        /// ���������������
        /// </summary>
        /// <param name="physicalScreenInfo"></param>
        public void SetDisplayInfo(List<ILEDDisplayInfo> leds)
        {
            _curHightLightType = ClickLinkLabelType.None;
            _bConfigScanBoardAgain = true;
            Thread.Sleep(100);

            _ledInfos.Clear();
            if (leds != null || leds.Count > 0)
            {
                foreach (ILEDDisplayInfo led in leds)
                {
                    if (led == null)
                    {
                        continue;
                    }
                    _ledInfos.Add(led);
                }
            }
            label_NoneUpdate.Visible = true;
            label_LastUpdateTimeValue.Visible = false;
            //���ø�����Ϣ
            ConfigScanBoardDisplay();
        }
        /// <summary>
        /// ���½���������Դ
        /// </summary>
        /// <param name="hasTable"></param>
        public void UpdateLanguage(Hashtable hasTable)
        {
            CommonStaticValue.CelsiusUnit = GetLangControlText(hasTable, "CelsiusUnit",
                                                               CommonStaticValue.CelsiusUnit);
            CommonStaticValue.FahrenheitUnit = GetLangControlText(hasTable, "FahrenheitUnit",
                                                                  CommonStaticValue.FahrenheitUnit);

            CommonStaticValue.ColsHeaderText[0] = GetLangControlText(hasTable, "ColsHeaderTextInfo",
                                                             CommonStaticValue.ColsHeaderText[0]);
            CommonStaticValue.ColsHeaderText[1] = GetLangControlText(hasTable, "ColsHeaderTextSenderIndex",
                                                             CommonStaticValue.ColsHeaderText[1]);
            CommonStaticValue.ColsHeaderText[2] = GetLangControlText(hasTable, "ColsHeaderTextPortIndex",
                                                             CommonStaticValue.ColsHeaderText[2]);
            CommonStaticValue.ColsHeaderText[3] = GetLangControlText(hasTable, "ColsHeaderTextSBIndex",
                                                             CommonStaticValue.ColsHeaderText[3]);
            CommonStaticValue.ColsHeaderText[4] = GetLangControlText(hasTable, "ColsHeaderTextStartX",
                                                             CommonStaticValue.ColsHeaderText[4]);
            CommonStaticValue.ColsHeaderText[5] = GetLangControlText(hasTable, "ColsHeaderTextStartY",
                                                             CommonStaticValue.ColsHeaderText[5]);
            CommonStaticValue.ColsHeaderText[6] = GetLangControlText(hasTable, "ColsHeaderTextWidth",
                                                             CommonStaticValue.ColsHeaderText[6]);
            CommonStaticValue.ColsHeaderText[7] = GetLangControlText(hasTable, "ColsHeaderTextHeight",
                                                             CommonStaticValue.ColsHeaderText[7]);


            CommonStaticValue.DisplayTypeStr[0] = GetLangControlText(hasTable, "SBStatus", CommonStaticValue.DisplayTypeStr[0]);
            CommonStaticValue.DisplayTypeStr[1] = GetLangControlText(hasTable, "Temperature", CommonStaticValue.DisplayTypeStr[1]);
            CommonStaticValue.DisplayTypeStr[2] = GetLangControlText(hasTable, "MCStatus", CommonStaticValue.DisplayTypeStr[2]);
            CommonStaticValue.DisplayTypeStr[3] = GetLangControlText(hasTable, "Humidity", CommonStaticValue.DisplayTypeStr[3]);
            CommonStaticValue.DisplayTypeStr[4] = GetLangControlText(hasTable, "Smoke", CommonStaticValue.DisplayTypeStr[4]);
            CommonStaticValue.DisplayTypeStr[5] = GetLangControlText(hasTable, "Fan", CommonStaticValue.DisplayTypeStr[5]);
            CommonStaticValue.DisplayTypeStr[6] = GetLangControlText(hasTable, "Power", CommonStaticValue.DisplayTypeStr[6]);
            CommonStaticValue.DisplayTypeStr[7] = GetLangControlText(hasTable, "RowLine", CommonStaticValue.DisplayTypeStr[7]);
            CommonStaticValue.DisplayTypeStr[8] = GetLangControlText(hasTable, "GeneralStatus", CommonStaticValue.DisplayTypeStr[8]);
            CommonStaticValue.DisplayTypeStr[9] = GetLangControlText(hasTable, "AllInfo", CommonStaticValue.DisplayTypeStr[9]);

            CommonStaticValue.StatusNoticeStr[0] = GetLangControlText(hasTable, "Normal", CommonStaticValue.StatusNoticeStr[0]);
            CommonStaticValue.StatusNoticeStr[1] = GetLangControlText(hasTable, "Error", CommonStaticValue.StatusNoticeStr[1]);
            CommonStaticValue.StatusNoticeStr[2] = GetLangControlText(hasTable, "Alarm", CommonStaticValue.StatusNoticeStr[2]);
            CommonStaticValue.StatusNoticeStr[3] = GetLangControlText(hasTable, "Unknown", CommonStaticValue.StatusNoticeStr[3]);
            CommonStaticValue.StatusNoticeStr[4] = GetLangControlText(hasTable, "Invalid", CommonStaticValue.StatusNoticeStr[4]);
            CommonStaticValue.StatusNoticeStr[5] = GetLangControlText(hasTable, "VoltageAlarm", CommonStaticValue.StatusNoticeStr[5]);

            CommonStaticValue.ScanBordNameStr = GetLangControlText(hasTable, "ScanBordNameStr", CommonStaticValue.ScanBordNameStr);

            CommonStaticValue.MCStatusDisplayStr[0] = GetLangControlText(hasTable, "MCCount", CommonStaticValue.MCStatusDisplayStr[0]);
            CommonStaticValue.MCStatusDisplayStr[1] = GetLangControlText(hasTable, "FaultMCCount", CommonStaticValue.MCStatusDisplayStr[1]);
            CommonStaticValue.MCStatusDisplayStr[2] = GetLangControlText(hasTable, "AlarmMCCount", CommonStaticValue.MCStatusDisplayStr[2]);

            CommonStaticValue.SBStatusDisplayStr[0] = GetLangControlText(hasTable, "SBCount", CommonStaticValue.SBStatusDisplayStr[0]);
            CommonStaticValue.SBStatusDisplayStr[1] = GetLangControlText(hasTable, "FaultSBCount", CommonStaticValue.SBStatusDisplayStr[1]);
            CommonStaticValue.SBStatusDisplayStr[2] = GetLangControlText(hasTable, "AlarmSBCount", CommonStaticValue.SBStatusDisplayStr[2]);

            CommonStaticValue.SmokeDisplayStr[0] = GetLangControlText(hasTable, "SBCount", CommonStaticValue.SmokeDisplayStr[0]);
            CommonStaticValue.SmokeDisplayStr[1] = GetLangControlText(hasTable, "SmokeAlarmSBCount", CommonStaticValue.SmokeDisplayStr[1]);

            CommonStaticValue.FanDisplayStr[0] = GetLangControlText(hasTable, "FanCount", CommonStaticValue.FanDisplayStr[0]);
            CommonStaticValue.FanDisplayStr[1] = GetLangControlText(hasTable, "AlarmFanCount", CommonStaticValue.FanDisplayStr[1]);

            CommonStaticValue.PowerDisplayStr[0] = GetLangControlText(hasTable, "PowerCount", CommonStaticValue.PowerDisplayStr[0]);
            CommonStaticValue.PowerDisplayStr[1] = GetLangControlText(hasTable, "AlarmPowerCount", CommonStaticValue.PowerDisplayStr[1]);

            CommonStaticValue.TemperatureDisplayStr[0] = GetLangControlText(hasTable, "LowestTemp", CommonStaticValue.TemperatureDisplayStr[0]);
            CommonStaticValue.TemperatureDisplayStr[1] = GetLangControlText(hasTable, "HighestTemp", CommonStaticValue.TemperatureDisplayStr[1]);
            CommonStaticValue.TemperatureDisplayStr[2] = GetLangControlText(hasTable, "AverageTemp", CommonStaticValue.TemperatureDisplayStr[2]);
            CommonStaticValue.TemperatureDisplayStr[3] = GetLangControlText(hasTable, "OverAverageTempCnt", CommonStaticValue.TemperatureDisplayStr[3]);
            CommonStaticValue.TemperatureDisplayStr[4] = GetLangControlText(hasTable, "TempAlarmCount", CommonStaticValue.TemperatureDisplayStr[4]);

            CommonStaticValue.HumidityDisplayStr[0] = GetLangControlText(hasTable, "LowestHumi", CommonStaticValue.HumidityDisplayStr[0]);
            CommonStaticValue.HumidityDisplayStr[1] = GetLangControlText(hasTable, "HighestHumi", CommonStaticValue.HumidityDisplayStr[1]);
            CommonStaticValue.HumidityDisplayStr[2] = GetLangControlText(hasTable, "AverageHumi", CommonStaticValue.HumidityDisplayStr[2]);
            CommonStaticValue.HumidityDisplayStr[3] = GetLangControlText(hasTable, "OverAverageHumiCnt", CommonStaticValue.HumidityDisplayStr[3]);
            CommonStaticValue.HumidityDisplayStr[4] = GetLangControlText(hasTable, "HumidityAlarmCount", CommonStaticValue.HumidityDisplayStr[4]);

            CommonStaticValue.RowLineStr[0] = GetLangControlText(hasTable, "CabinetCount", CommonStaticValue.RowLineStr[0]);
            CommonStaticValue.RowLineStr[1] = GetLangControlText(hasTable, "CabinetFaultCount", CommonStaticValue.RowLineStr[1]);

            CommonStaticValue.SwitchSignStr[0] = GetLangControlText(hasTable, "Fan", CommonStaticValue.SwitchSignStr[0]);
            CommonStaticValue.SwitchSignStr[1] = GetLangControlText(hasTable, "Power", CommonStaticValue.SwitchSignStr[1]);
            CommonStaticValue.SwitchSignStr[2] = GetLangControlText(hasTable, "SwitchSignSBVoltage", CommonStaticValue.SwitchSignStr[2]);
            CommonStaticValue.SwitchSignStr[3] = GetLangControlText(hasTable, "SwitchSignMCSelefVoltage", CommonStaticValue.SwitchSignStr[3]);
            CommonStaticValue.SwitchSignStr[4] = GetLangControlText(hasTable, "SwitchSignMCVoltage", CommonStaticValue.SwitchSignStr[4]);


            CommonStaticValue.RowColNameStr = GetLangControlText(hasTable, "RowColNameStr", CommonStaticValue.RowColNameStr);
            //CommonStaticValue.CommPortScreen = GetLangControlText(hasTable, "CommPortScreen", CommonStaticValue.CommPortScreen);
            CommonStaticValue.DoubleClickToViewInfo = GetLangControlText(hasTable, "DoubleClickToViewInfo", CommonStaticValue.DoubleClickToViewInfo);
            CommonStaticValue.ScreenNotExist = GetLangControlText(hasTable, "ScreenNotExist", CommonStaticValue.ScreenNotExist);

            CommonStaticValue.NotConnectMC = GetLangControlText(hasTable, "NotConnectMC", CommonStaticValue.NotConnectMC);
            CommonStaticValue.SoketName = GetLangControlText(hasTable, "SoketName", CommonStaticValue.SoketName);
            CommonStaticValue.GroupName = GetLangControlText(hasTable, "GroupName", CommonStaticValue.GroupName);
            CommonStaticValue.ScanSignalName = GetLangControlText(hasTable, "ScanSignalName", CommonStaticValue.ScanSignalName);
            CommonStaticValue.SignalName = GetLangControlText(hasTable, "SignalName", CommonStaticValue.SignalName);

            CommonStaticValue.RGBSignalStr[0] = GetLangControlText(hasTable, "RGBSignalStrR", CommonStaticValue.RGBSignalStr[0]);
            CommonStaticValue.RGBSignalStr[1] = GetLangControlText(hasTable, "RGBSignalStrG", CommonStaticValue.RGBSignalStr[1]);
            CommonStaticValue.RGBSignalStr[2] = GetLangControlText(hasTable, "RGBSignalStrB", CommonStaticValue.RGBSignalStr[2]);
            CommonStaticValue.RGBSignalStr[3] = GetLangControlText(hasTable, "RGBSignalStrVR", CommonStaticValue.RGBSignalStr[3]);

            CommonStaticValue.GeneralStatusStr[0] = GetLangControlText(hasTable, "CabinetCount", CommonStaticValue.GeneralStatusStr[0]);
            CommonStaticValue.GeneralStatusStr[1] = GetLangControlText(hasTable, "CabinetCntOfOpenDoor", CommonStaticValue.GeneralStatusStr[1]);


            CommonStaticValue.CabinetDoorStatusStr[0] = GetLangControlText(hasTable, "DoorClose", CommonStaticValue.CabinetDoorStatusStr[0]);
            CommonStaticValue.CabinetDoorStatusStr[1] = GetLangControlText(hasTable, "DoorOpen", CommonStaticValue.CabinetDoorStatusStr[1]);

            CommonStaticValue.SupplyVoltage = GetLangControlText(hasTable, "SupplyVoltage", CommonStaticValue.SupplyVoltage);
            CommonStaticValue.PreviewComplexMonitorInfo = GetLangControlText(hasTable, "Frm_ComplexScreenDisplayText", CommonStaticValue.PreviewComplexMonitorInfo);
            CommonStaticValue.FaultInformation = GetLangControlText(hasTable, "FaultInformation", CommonStaticValue.FaultInformation);
            CommonStaticValue.SwitchName = GetLangControlText(hasTable, "SwitchName", CommonStaticValue.SwitchName);

            //ˢ�½�����ʾ��Ϣ
            _sbStatusInfoUC.UpdateLangusge(CommonStaticValue.SBStatusDisplayStr[0],
                                           CommonStaticValue.SBStatusDisplayStr[1],
                                           CommonStaticValue.SBStatusDisplayStr[2]);
            _mcStatusInfoUC.UpdateLangusge(CommonStaticValue.MCStatusDisplayStr[0],
                                           CommonStaticValue.MCStatusDisplayStr[1],
                                           CommonStaticValue.MCStatusDisplayStr[2]);
            _smokeInfo.UpdateLangusge(CommonStaticValue.SmokeDisplayStr[0], CommonStaticValue.SmokeDisplayStr[1]);
            _fanInfoUC.UpdateLangusge(CommonStaticValue.FanDisplayStr[0], CommonStaticValue.FanDisplayStr[1]);
            _powerInfoUC.UpdateLangusge(CommonStaticValue.PowerDisplayStr[0], CommonStaticValue.PowerDisplayStr[1]);
            _tempInfoUC.UpdateLangusge(CommonStaticValue.TemperatureDisplayStr[0], CommonStaticValue.TemperatureDisplayStr[1],
                                       CommonStaticValue.TemperatureDisplayStr[2], CommonStaticValue.TemperatureDisplayStr[3],
                                       CommonStaticValue.TemperatureDisplayStr[4]);
            _humidityInfoUC.UpdateLangusge(CommonStaticValue.HumidityDisplayStr[0], CommonStaticValue.HumidityDisplayStr[1],
                                           CommonStaticValue.HumidityDisplayStr[2], CommonStaticValue.HumidityDisplayStr[3],
                                           CommonStaticValue.HumidityDisplayStr[4]);
            _rowLineInfoUC.UpdateLangusge(CommonStaticValue.RowLineStr[0], CommonStaticValue.RowLineStr[1]);
            _generalStatuInfoUC.UpdateLangusge(CommonStaticValue.GeneralStatusStr[0], CommonStaticValue.GeneralStatusStr[1]);

            _mcStatusCorNotice.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK],
                                              CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown],
                                              CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Error],
                                              CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.VoltageException]);
            _rowLineCorNotice.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK],
                                             CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown],
                                             CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Error]);
            _tempCorNoticeUC.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
            _humidityCorNoticeUC.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
            _sbStatusCorNotice.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK],
                                            CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown],
                                            CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Error],
                                            CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.VoltageException]);
            _smokeOrPowerCorNotice.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK],
                                            CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Alarm],
                                            CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
            _fanCorNotice.UpdateLanguage(CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK],
                                         CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Alarm],
                                         CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
            _generalStatusCorNotice.UpdateLanguage(CommonStaticValue.CabinetDoorStatusStr[(int)CommonStaticValue.CabinetDoorStatusType.Close],
                                                   CommonStaticValue.CabinetDoorStatusStr[(int)CommonStaticValue.CabinetDoorStatusType.Open],
                                                   CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
            _complexScreenLayout.UpdateLanguage();
            if (_curMonitorConfigInfo != null)
            {
                switch (_curMonitorConfigInfo.TempDisplayType)
                {
                    case TemperatureType.Celsius:
                        _tempCorNoticeUC.UpdateUnit(CommonStaticValue.CelsiusUnit);
                        _tempInfoUC.UpdateUnit(CommonStaticValue.CelsiusUnit);
                        break;
                    case TemperatureType.Fahrenheit:
                        _tempCorNoticeUC.UpdateUnit(CommonStaticValue.FahrenheitUnit);
                        _tempInfoUC.UpdateUnit(CommonStaticValue.FahrenheitUnit);
                        break;
                }

            }
        }
        /// <summary>
        /// ��ȡ�ؼ���Text�ַ���
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static string GetLangControlText(Hashtable hasTable, string controlName, string defaultText)
        {
            if (hasTable != null
             && hasTable.Contains(controlName.ToLower()))
            {
                return (string)hasTable[controlName.ToLower()];
            }
            return defaultText;
        }
        #endregion

        #region ˽�к���
        private void ConfigScanBoardDisplay()
        {
            //���¹��������Ų�
            lock (_configCurScreenObj)
            {
                _isOnlySimpleOrStandarScreen = true;
                _isOnlyOneComplexScreen = false;

                //TODO  ���ͳ����Ϣ
                _isHasValidMonitorInfo = false;
                InitAllVariables();
                _curScanBordCount = 0;
                DisplayStatisticsInfo();
                _curAllLedScreenRectDic.Clear();
                _curOperateStep = OperateStep.ConfigScanBoard;

                #region �ж��Ƿ�ֻ��һ����ʾ�����Ҹ���Ϊ������
                //if (true) //�ж��Ƿ�Ϊ�������Ŀǰ��֧�������
                //{
                foreach (ILEDDisplayInfo led in _ledInfos)
                {
                    if (led.Type == LEDDisplyType.ComplexType)
                    {
                        _isOnlyOneComplexScreen = true;
                        _isOnlySimpleOrStandarScreen = false;
                        _oniyOneComplexScreenCommPort = _screenSN;
                        _onlyOneScreenIndex = 0;
                        _onlyOneComplexScreenInfo = (ComplexLEDDisplayInfo)led;
                        break;
                    }
                    else
                    {
                        _isOnlyOneComplexScreen = false;
                        _isOnlySimpleOrStandarScreen = true;
                        break;
                    }
                }
                //}
                //else
                //{
                //    _isOnlySimpleOrStandarScreen = false;
                //    _isOnlyOneComplexScreen = false;
                //}
                #endregion

                _bConfigScanBoardAgain = false;
                if (_indicator == null || _indicator.IsDisposed)
                {
                    _indicator = new ProgressIndicator();
                }
                if (_isOnlyOneComplexScreen)
                {
                    _indicator.Hide();
                    _indicator.Stop();

                    _indicator.Size = new Size(_complexScreenLayout.Width / 4, _complexScreenLayout.Height / 4);
                    _indicator.Parent = _complexScreenLayout;
                    _indicator.Show();
                    _indicator.Location = new Point((_complexScreenLayout.Width - _indicator.Width) / 2,
                                                    (_complexScreenLayout.Height - _indicator.Height) / 2);
                    _indicator.BringToFront();
                    _indicator.Start();

                    //TODO  ���ظ�����ʾ�غ������ʿؼ�
                    VisibleControlWhenOnlyOneComplexScreen(true);
                    //��ո�������ʾ�ؼ�����Ϣ
                    _complexScreenLayout.ClearAllMonitorInfo();
                    //�̳߳�
                    AddScanBdRegionInfToGridUIDelegate cs = new AddScanBdRegionInfToGridUIDelegate(ConfigComplexScreenInNarmalDisplay);
                    cs.BeginInvoke(null, null);
                }
                else
                {
                    _indicator.Size = new Size(_statusRectangleLayout.Width / 4, _statusRectangleLayout.Height / 4);
                    _indicator.Parent = _statusRectangleLayout;
                    _indicator.Show();
                    _indicator.Location = new Point((_statusRectangleLayout.Width - _indicator.Width) / 2,
                                                        (_statusRectangleLayout.Height - _indicator.Height) / 2);
                    _indicator.BringToFront();
                    _indicator.Start();


                    //TODO  ���ص�����������ʾ�ؼ�
                    VisibleControlWhenOnlyOneComplexScreen(false);
                    //��ո�����Ϣ
                    _statusRectangleLayout.ClearAllRectangularGrid();
                    //TODO  ����ÿ����ʾ���Ļ���λ��
                    GetAllScreenLocationAndSize();
                    //�̳߳�
                    AddScanBdRegionInfToGridUIDelegate cs = new AddScanBdRegionInfToGridUIDelegate(AddRegionInfToGridUI);
                    cs.BeginInvoke(null, null);
                }


            }
        }

        #region ����һ��������������Ϣ
        private void ConfigComplexScreenInNarmalDisplay()
        {
            //����������ʾ�ؼ�����м������
            AddScanBoardInfoForComplexControl(_oniyOneComplexScreenCommPort,
                                              _onlyOneScreenIndex,
                                              _onlyOneComplexScreenInfo,
                                              ref _complexScreenLayout);
            _curScanBordCount = _onlyOneComplexScreenInfo.ScannerCount;
            //���֪ͨ
            BeginNotifyAddGridComplate();
        }

        private void AddScanBoardInfoForComplexControl(string commPort, byte screenIndex,
                                                       ComplexLEDDisplayInfo complexScreen,
                                                       ref UC_ComplexScreenLayout complexScreenControl)
        {
            if (complexScreen == null)
            {
                return;
            }
            int complexScannerCnt = complexScreen.ScannerCount;
            VirtualModeType virModeType = complexScreen.VirtualMode;

            for (int i = 0; i < complexScannerCnt; i++)
            {
                if (_bConfigScanBoardAgain)
                {
                    //�������ø�����Ϣ
                    break;
                }
                if (complexScreen[i] == null)
                {
                    _curScanBordCount--;
                    continue;
                }
                _tempScanBoardInfo = (ScanBoardRegionInfo)complexScreen[i].Clone();
                _complexScreenScanBoardMonitorInfo = new SBInfoAndMonitorInfo();
                _complexScreenScanBoardMonitorInfo.CommPortName = _screenSNDisplay;
                _complexScreenScanBoardMonitorInfo.SBColIndex = -1;
                _complexScreenScanBoardMonitorInfo.SBRowIndex = -1;
                _complexScreenScanBoardMonitorInfo.ScreenIndex = screenIndex;
                _complexScreenScanBoardMonitorInfo.ScanBordInfo = _tempScanBoardInfo;
                _complexScreenScanBoardMonitorInfo.MonitorData = null;
                _complexScreenScanBoardMonitorInfo.RowLineStatus = null;
                _complexScreenScanBoardMonitorInfo.virModeType = virModeType;

                _complexScreenScanBoardMonitorInfo.SBRectKey = GetRectangleKey(commPort,
                                                                              _tempScanBoardInfo.SenderIndex,
                                                                              _tempScanBoardInfo.PortIndex,
                                                                              _tempScanBoardInfo.ConnectIndex);
                _complexScreenScanBoardMonitorInfo.SBAddress = GetScanBoardKey(_tempScanBoardInfo.SenderIndex,
                                                                              _tempScanBoardInfo.PortIndex,
                                                                              _tempScanBoardInfo.ConnectIndex);
                //����ÿһ�е���Ϣ
                complexScreenControl.AddOneScanBoardInfo(_complexScreenScanBoardMonitorInfo);
            }
        }
        #endregion

        #region ��������ͼ���ƿؼ��ĸ�����Ϣ
        /// <summary>
        /// ��ȡ������ʾ����λ�úʹ�С
        /// </summary>
        private void GetAllScreenLocationAndSize()
        {
            _curAllLedScreenRectDic.Clear();
            string commPortName = "";
            byte screenIndex = 0;

            Size screenSize;
            int nOffsetX = 0;
            int nOffsetY = 0;
            int nBaseScreenRight = 0;
            int nBaseScreenBottom = 0;
            ScreenGridBindObj screenBoarderInfo = null;
            if (_ledInfos != null || _ledInfos.Count > 0)
            {
                #region ��ȡ������ԭ�е�λ����Ϣ
                _curAllLedScreenRectDic.Add(_screenSN, new List<ScreenGridBindObj>());
                for (int i = 0; i < _ledInfos.Count; i++)//foreach (string commPort in _physicalScreenInfo.LedInfoDic.Keys)
                {
                    screenIndex = (byte)i;

                    screenBoarderInfo = new ScreenGridBindObj();
                    screenBoarderInfo.CommPortName = _screenSNDisplay;
                    screenBoarderInfo.ScreenIndex = screenIndex;
                    screenSize = _ledInfos[i].GetScreenSize();
                    screenBoarderInfo.ScreenRect = new Rectangle(0, 0, screenSize.Width, screenSize.Height);
                    _curAllLedScreenRectDic[_screenSN].Add(screenBoarderInfo);

                    //if ( �Ƿ�Ϊ�����== null)
                    //{
                    //    ��ִ������ģ�����ִ�������for
                    //}
                    //for (byte j = 0; j < _physicalScreenInfo.ScreenCombineInfo.ScreenCount; j++)
                    //{
                    //    if (_physicalScreenInfo.ScreenCombineInfo[j] == null)
                    //    {
                    //        continue;
                    //    }
                    //    if (commPort == _physicalScreenInfo.ScreenCombineInfo[j].CommPortName
                    //        && screenIndex == _physicalScreenInfo.ScreenCombineInfo[j].ScreenIndex)
                    //    {
                    //        screenBoarderInfo = new ScreenGridBindObj();
                    //        screenBoarderInfo.CommPortName = commPort;
                    //        screenBoarderInfo.ScreenIndex = screenIndex;
                    //        screenBoarderInfo.ScreenRect = new Rectangle(_physicalScreenInfo.ScreenCombineInfo[j].ScreenX,
                    //                                                     _physicalScreenInfo.ScreenCombineInfo[j].ScreenY,
                    //                                                     _physicalScreenInfo.ScreenCombineInfo[j].ScreenWidth,
                    //                                                     _physicalScreenInfo.ScreenCombineInfo[j].ScreenHeight);
                    //        _curAllLedScreenRectDic[commPort].Add(screenBoarderInfo);
                    //        break;
                    //    }
                    //}
                }
                #endregion

                #region ��ȡ���������Ƶ�λ����Ϣ
                foreach (string baseCommPort in _curAllLedScreenRectDic.Keys)
                {
                    for (int i = 0; i < _curAllLedScreenRectDic[baseCommPort].Count; i++)
                    {
                        screenSize = _ledInfos[i].GetScreenSize();
                        if (screenSize.Width == _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Width
                            && screenSize.Height == _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Height)
                        {
                            continue;
                        }
                        nOffsetX = screenSize.Width - _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Width;
                        nOffsetY = screenSize.Height - _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Height;
                        nBaseScreenRight = _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Width
                                            + _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.X;
                        nBaseScreenBottom = _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Height
                                            + _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Y;

                        _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Width = screenSize.Width;
                        _curAllLedScreenRectDic[baseCommPort][i].ScreenRect.Height = screenSize.Height;
                        foreach (string curCommPort in _curAllLedScreenRectDic.Keys)
                        {
                            for (int j = 0; j < _curAllLedScreenRectDic[curCommPort].Count; j++)
                            {
                                if (curCommPort == baseCommPort && i == j)
                                {
                                    continue;
                                }
                                if (_curAllLedScreenRectDic[curCommPort][j].ScreenRect.X >= nBaseScreenRight)
                                {
                                    _curAllLedScreenRectDic[curCommPort][j].ScreenRect.X += nOffsetX;
                                }
                                else if (_curAllLedScreenRectDic[curCommPort][j].ScreenRect.Y >= nBaseScreenBottom)
                                {
                                    _curAllLedScreenRectDic[curCommPort][j].ScreenRect.Y += nOffsetY;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// ��Ӹ�����Ϣ
        /// </summary>
        private delegate void AddScanBdRegionInfToGridUIDelegate();
        private void AddRegionInfToGridUI()
        {
            string key = "";
            Rectangle rectTemp;

            Rectangle screenRect;
            Point screenOffset;
            byte screenIndex = 0;

            ScanBoardGridBindObj scanBoardGridObj = null;
            ComplexScreenGridBindObj complexScreenGridObj = null;
            VirtualModeType virModeType = VirtualModeType.Unknown;

            #region ��ӽ��տ�������Ϣ
            try
            {
                //foreach (string commPort in _physicalScreenInfo.LedInfoDic.Keys)
                //{
                if (_bConfigScanBoardAgain)
                {
                    //�������ø�����Ϣ
                    return;
                }
                for (int i = 0; i < _ledInfos.Count; i++)
                {
                    virModeType = _ledInfos[i].VirtualMode;
                    screenRect = _curAllLedScreenRectDic[_screenSN][i].ScreenRect;
                    screenIndex = _curAllLedScreenRectDic[_screenSN][i].ScreenIndex;
                    screenOffset = _ledInfos[i].GetScreenPosition();
                    if (_ledInfos[i].Type == LEDDisplyType.SimpleSingleType)
                    {
                        #region ����
                        lock (_curScreenInfoChangedObj)
                        {
                            _simpleScreenInfo = (SimpleLEDDisplayInfo)_ledInfos[i];
                        }
                        _curScanBordCount += _simpleScreenInfo.ScannerCount;
                        lock (_addScanBoardObj)
                        {
                            for (int j = 0; j < _simpleScreenInfo.ScanBdRows; j++)
                            {
                                if (_bConfigScanBoardAgain)
                                {
                                    //�������ø�����Ϣ
                                    return;
                                }
                                for (int m = 0; m < _simpleScreenInfo.ScanBdCols; m++)
                                {
                                    if (_bConfigScanBoardAgain)
                                    {
                                        //�������ø�����Ϣ
                                        return;
                                    }
                                    if (_simpleScreenInfo[m, j] == null)
                                    {
                                        _curScanBordCount--;
                                        continue;
                                    }
                                    _tempScanBoardInfo = (ScanBoardRegionInfo)_simpleScreenInfo[m, j].Clone();
                                    key = GetRectangleKey(_screenSN, _tempScanBoardInfo.SenderIndex,
                                                          _tempScanBoardInfo.PortIndex, _tempScanBoardInfo.ConnectIndex);
                                    rectTemp = new Rectangle(_tempScanBoardInfo.X - screenOffset.X + screenRect.X,
                                                             _tempScanBoardInfo.Y - screenOffset.Y + screenRect.Y,
                                                             _tempScanBoardInfo.Width,
                                                             _tempScanBoardInfo.Height);
                                    scanBoardGridObj = new ScanBoardGridBindObj();
                                    scanBoardGridObj.ScanBoardAndMonitorInfo = new SBInfoAndMonitorInfo();
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.CommPortName = _screenSNDisplay;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.ScreenIndex = screenIndex;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBAddress = GetScanBoardKey(_tempScanBoardInfo.SenderIndex,
                                                                                                          _tempScanBoardInfo.PortIndex,
                                                                                                          _tempScanBoardInfo.ConnectIndex);
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBColIndex = m;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBRowIndex = j;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.ScanBordInfo = _tempScanBoardInfo;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData = null;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.RowLineStatus = null;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.virModeType = virModeType;
                                    AddOneRectangleInfo(rectTemp, key, scanBoardGridObj);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (_ledInfos[i].Type == LEDDisplyType.StandardType)
                    {
                        #region ��׼��
                        lock (_curScreenInfoChangedObj)
                        {
                            _standardScreenInfo = (StandardLEDDisplayInfo)_ledInfos[i];
                        }
                        _curScanBordCount += _standardScreenInfo.ScannerCount;
                        lock (_addScanBoardObj)
                        {
                            for (int j = 0; j < _standardScreenInfo.ScanBoardRows; j++)
                            {
                                if (_bConfigScanBoardAgain)
                                {
                                    //�������ø�����Ϣ
                                    return;
                                }
                                for (int m = 0; m < _standardScreenInfo.ScanBoardCols; m++)
                                {
                                    if (_bConfigScanBoardAgain)
                                    {
                                        //�������ø�����Ϣ
                                        return;
                                    }
                                    if (_standardScreenInfo[m, j] == null)
                                    {
                                        _curScanBordCount--;
                                        continue;
                                    }
                                    _tempScanBoardInfo = (ScanBoardRegionInfo)_standardScreenInfo[m, j].Clone();
                                    if (_tempScanBoardInfo.SenderIndex == 255)
                                    {
                                        _curScanBordCount--;
                                        continue;
                                    }
                                    key = GetRectangleKey(_screenSN, _tempScanBoardInfo.SenderIndex,
                                                          _tempScanBoardInfo.PortIndex, _tempScanBoardInfo.ConnectIndex);
                                    rectTemp = new Rectangle(_tempScanBoardInfo.X - screenOffset.X + screenRect.X,
                                                             _tempScanBoardInfo.Y - screenOffset.Y + screenRect.Y,
                                                             _tempScanBoardInfo.Width,
                                                             _tempScanBoardInfo.Height);
                                    scanBoardGridObj = new ScanBoardGridBindObj();
                                    scanBoardGridObj.ScanBoardAndMonitorInfo = new SBInfoAndMonitorInfo();
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.CommPortName = _screenSNDisplay;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.ScreenIndex = screenIndex;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBAddress = GetScanBoardKey(_tempScanBoardInfo.SenderIndex,
                                                                                                         _tempScanBoardInfo.PortIndex,
                                                                                                         _tempScanBoardInfo.ConnectIndex);
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBColIndex = m;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.SBRowIndex = j;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.ScanBordInfo = _tempScanBoardInfo;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData = null;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.RowLineStatus = null;
                                    scanBoardGridObj.ScanBoardAndMonitorInfo.virModeType = virModeType;
                                    AddOneRectangleInfo(rectTemp, key, scanBoardGridObj);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (_ledInfos[i].Type == LEDDisplyType.ComplexType)
                    {
                        #region ������
                        if (_bConfigScanBoardAgain)
                        {
                            //�������ø�����Ϣ
                            return;
                        }
                        lock (_curScreenInfoChangedObj)
                        {
                            _complexScreenInfo = (ComplexLEDDisplayInfo)_ledInfos[i];
                        }
                        _curScanBordCount += _complexScreenInfo.ScannerCount;
                        key = _screenSN + SCANBORDADDR_SEPERATE + screenIndex.ToString();
                        rectTemp = new Rectangle(screenRect.X, screenRect.Y, screenRect.Width, screenRect.Height);

                        complexScreenGridObj = new ComplexScreenGridBindObj();
                        complexScreenGridObj.CommPortName = _screenSNDisplay;
                        complexScreenGridObj.ScreenIndex = screenIndex;
                        ConstructOneComplexScreenObj();
                        complexScreenGridObj.ComplexScreenLayout = _complexScreenLayoutTemp;
                        AddScanBoardInfoForComplexControl(_screenSN, screenIndex, _complexScreenInfo,
                                                         ref complexScreenGridObj.ComplexScreenLayout);
                        AddOneRectangleInfo(rectTemp, key, complexScreenGridObj);
                        #endregion
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                OutputDebugInfoStr(true, "���ý��տ�������Ϣ�쳣:" + ex.Message);
            }

            #endregion

            #region ��Ӹ��ӱ߽�
            //TODO �����ʾ���ı߽������Ϣ
            try
            {
                if (!_isOnlySimpleOrStandarScreen)
                {
                    ScreenGridBindObj screenGridObj = null;
                    foreach (string commPort in _curAllLedScreenRectDic.Keys)
                    {
                        if (_bConfigScanBoardAgain)
                        {
                            //�������ø�����Ϣ
                            return;
                        }
                        for (int i = 0; i < _curAllLedScreenRectDic[commPort].Count; i++)
                        {
                            if (_bConfigScanBoardAgain)
                            {
                                //�������ø�����Ϣ
                                return;
                            }
                            screenGridObj = new ScreenGridBindObj();
                            if (_ledInfos[i] != null)
                            {
                                if (_ledInfos[i].Type == LEDDisplyType.ComplexType)
                                {
                                    continue;
                                }
                                screenGridObj.ScreenIsValid = true;
                            }
                            else
                            {
                                screenGridObj.ScreenIsValid = false;
                            }
                            key = commPort + SCANBORDADDR_SEPERATE + i.ToString();
                            rectTemp = new Rectangle(_curAllLedScreenRectDic[commPort][i].ScreenRect.X,
                                                     _curAllLedScreenRectDic[commPort][i].ScreenRect.Y,
                                                     _curAllLedScreenRectDic[commPort][i].ScreenRect.Width,
                                                     _curAllLedScreenRectDic[commPort][i].ScreenRect.Height);

                            screenGridObj.CommPortName = _screenSNDisplay;
                            screenGridObj.ScreenIndex = _curAllLedScreenRectDic[commPort][i].ScreenIndex;
                            screenGridObj.ScreenRect = rectTemp;
                            AddOneRectangleInfo(rectTemp, key, screenGridObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputDebugInfoStr(true, "���ø��ӱ���쳣��" + ex.Message);
            }
            #endregion
            //���֪ͨ
            BeginNotifyAddGridComplate();
        }
        private void AddOneRectangleInfo(Rectangle rect, string key, IGridBiningObject customObj)
        {
            try
            {
                if (!_statusRectangleLayout.GridDic.ContainsKey(key))
                {
                    OutputDebugInfoStr(false, "�������򲻰����ø�����Ϣ��keyֵΪ��" + key + "��ʼ��Ӹ�����Ϣ");
                    _statusRectangleLayout.AddRectangularGrid(rect, key, true, customObj);
                }
            }
            catch (Exception ex)
            {
                OutputDebugInfoStr(false, "��Ӹ�����Ϣʱ�����쳣��" + ex.Message);

            }
        }

        private delegate void ConstructComplexScreenObjDelegate();
        private void ConstructOneComplexScreenObj()
        {
            if (!this.InvokeRequired)
            {
                _complexScreenLayoutTemp = new UC_ComplexScreenLayout();
            }
            else
            {
                ConstructComplexScreenObjDelegate asb = new ConstructComplexScreenObjDelegate(ConstructOneComplexScreenObj);
                this.Invoke(asb, new object[] { });
            }
        }
        #endregion

        #region ���ý��տ�������Ϣ���
        private void BeginNotifyAddGridComplate()
        {
            foreach (Delegate del in CompeleteConfigScanBoardDisplayEvent.GetInvocationList())
            {
                //����ˢ������¼�
                EventHandler evt = (EventHandler)del;
                if (evt != null)
                {
                    evt.BeginInvoke(this, null, null, null);
                }
            }
        }
        private void MarsScreenMonitorDataViewer_CompeleteConfigScanBoardDisplayEvent(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler cd = new EventHandler(MarsScreenMonitorDataViewer_CompeleteConfigScanBoardDisplayEvent);
                this.BeginInvoke(cd, new object[] { sender, e });
            }
            else
            {
                _curOperateStep = OperateStep.None;
                _indicator.Stop();
                _indicator.Hide();
                if (_updateMonitorDataForNotUpdateDic.Count > 0)
                {
                    _curOperateStep = OperateStep.UpdateMonitorData;
                    foreach (string commPort in _updateMonitorDataForNotUpdateDic.Keys)
                    {
                        BindingScanBoardMonitorInfo(commPort, _updateMonitorDataForNotUpdateDic[commPort]);
                    }
                    _updateMonitorDataForNotUpdateDic.Clear();

                    label_NoneUpdate.Visible = false;
                    label_LastUpdateTimeValue.Visible = true;
                    label_LastUpdateTimeValue.Text = _lastUpdateTime.ToLongTimeString();
                }
                if (_isNeedStatisticsMonitorData)
                {
                    _curOperateStep = OperateStep.RefreshStatisticsInfo;

                    //TODO ͳ�ƽ��տ���Ӧ���ϸ澯��Ϣ
                    StatisticsFaultAndAlarmInfo();
                }
                else
                {
                    DisplayStatisticsInfo();
                }
                if (_isNeedUpdateFont)
                {
                    _curOperateStep = OperateStep.SetGridFont;
                    _isNeedUpdateFont = false;
                    SetGridFont(_needUpdateFont);
                }
                if (_isNeedSetScaleRatio)
                {
                    _curOperateStep = OperateStep.SetScaleRatio;
                    _isNeedSetScaleRatio = false;
                    SetCurScaleRatio();
                }
                //TODO  ������ʾ�ĸ�����Ϣ
                try
                {
                    if (!_isOnlyOneComplexScreen)
                    {
                        this._statusRectangleLayout.InvalidateDrawArea();
                    }
                    else
                    {
                        this._complexScreenLayout.InvalidateAllScanBoardInfo();
                    }
                }
                catch (Exception ex)
                {
                    OutputDebugInfoStr(false, "���������ɺ�ˢ�½��տ���Ϣ�쳣:" + ex.Message);
                }

                _isNeedStatisticsMonitorData = false;
                _curOperateStep = OperateStep.None;
            }
        }
        #endregion

        #region �󶨼������
        private delegate void UpdataScreenMonitorInfoDele(string commPort, Dictionary<string, ScannerMonitorData> monitorDataDict);
        private void BindingScanBoardMonitorInfo(string commPort, Dictionary<string, ScannerMonitorData> monitorDataDict)
        {
            if (this.InvokeRequired)
            {
                UpdataScreenMonitorInfoDele cd = new UpdataScreenMonitorInfoDele(BindingScanBoardMonitorInfo);
                this.Invoke(cd, new object[] { commPort, monitorDataDict });
            }
            else
            {
                lock (_bindDataObj)
                {
                    //CustomObj�д洢�Ľ��տ���ַ
                    string customSBAddress = "";
                    ScannerMonitorData monitorData = null;
                    SBInfoAndMonitorInfo sbInfAndMonitorInf = null;
                    if (!_isOnlyOneComplexScreen)
                    {
                        IGridBiningObject bindObj = null;
                        ScanBoardGridBindObj scanBdGridObj = null;
                        ComplexScreenGridBindObj complexScreenGridObj = null;
                        //�������ӣ��ж϶�Ӧ��CustomObj��CommPortName��ˢ�µĴ��ں���ͬʱ����������Ӧ�ļ������
                        foreach (string key in this._statusRectangleLayout.GridDic.Keys)
                        {
                            bindObj = (IGridBiningObject)_statusRectangleLayout[key].CustomObj;
                            if (bindObj.Type == GridType.ScanBoardGird)
                            {
                                scanBdGridObj = (ScanBoardGridBindObj)bindObj;
                                //if (scanBdGridObj.ScanBoardAndMonitorInfo.CommPortName != commPort)
                                //{
                                //    continue;
                                //}
                                customSBAddress = scanBdGridObj.ScanBoardAndMonitorInfo.SBAddress;
                                if (monitorDataDict != null && monitorDataDict.ContainsKey(customSBAddress))
                                {
                                    monitorData = (ScannerMonitorData)monitorDataDict[customSBAddress].Clone();
                                    sbInfAndMonitorInf = scanBdGridObj.ScanBoardAndMonitorInfo;
                                    GetNewSBInfoAndMonitoInf(monitorData, ref sbInfAndMonitorInf);
                                    _statusRectangleLayout[key].CustomObj = scanBdGridObj;
                                }
                                else
                                {
                                    scanBdGridObj.ScanBoardAndMonitorInfo.RowLineStatus = null;
                                    scanBdGridObj.ScanBoardAndMonitorInfo.MonitorData = null;
                                }
                            }
                            else if (bindObj.Type == GridType.ComplexScreenGrid)
                            {
                                complexScreenGridObj = (ComplexScreenGridBindObj)bindObj;
                                for (int i = 0; i < complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList.Count; i++)
                                {
                                    if (complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList[i].CommPortName != _screenSNDisplay)
                                    {
                                        continue;
                                    }
                                    customSBAddress = complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList[i].SBAddress;
                                    if (monitorDataDict != null && monitorDataDict.ContainsKey(customSBAddress))
                                    {
                                        monitorData = (ScannerMonitorData)monitorDataDict[customSBAddress].Clone();
                                        sbInfAndMonitorInf = complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList[i];
                                        GetNewSBInfoAndMonitoInf(monitorData, ref sbInfAndMonitorInf);
                                    }
                                    else
                                    {
                                        complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList[i].RowLineStatus = null;
                                        complexScreenGridObj.ComplexScreenLayout.CurAllSBMonitorInfList[i].MonitorData = null;
                                    }
                                }
                                _statusRectangleLayout[key].CustomObj = complexScreenGridObj;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _complexScreenLayout.CurAllSBMonitorInfList.Count; i++)
                        {
                            if (_complexScreenLayout.CurAllSBMonitorInfList[i].CommPortName != _screenSNDisplay)
                            {
                                continue;
                            }
                            customSBAddress = _complexScreenLayout.CurAllSBMonitorInfList[i].SBAddress;
                            if (monitorDataDict != null && monitorDataDict.ContainsKey(customSBAddress))
                            {
                                monitorData = (ScannerMonitorData)monitorDataDict[customSBAddress].Clone();
                                sbInfAndMonitorInf = _complexScreenLayout.CurAllSBMonitorInfList[i];
                                GetNewSBInfoAndMonitoInf(monitorData, ref sbInfAndMonitorInf);
                            }
                            else
                            {
                                _complexScreenLayout.CurAllSBMonitorInfList[i].RowLineStatus = null;
                                _complexScreenLayout.CurAllSBMonitorInfList[i].MonitorData = null;
                            }
                        }
                    }
                }
            }
        }

        private void GetNewSBInfoAndMonitoInf(ScannerMonitorData monitorData, ref SBInfoAndMonitorInfo info)
        {
            if (monitorData == null)
            {
                info.RowLineStatus = null;
                info.MonitorData = null;
                info.GeneralSwitchList = null;
                return;
            }
            byte[] moduleBytes = monitorData.ModuleStatusBytes;
            byte generalStatus = monitorData.GeneralStatusData;
            VirtualModeType virModeType = info.virModeType;
            ScanBoardRowLineStatus rowLineStatus = null;
            List<bool> _generalSwitchList = null;
            if (monitorData.IsConnectMC)
            {
                if (ParseRowLineData.ParseScanBoardRowLineData(moduleBytes, monitorData.IsConnectMC, virModeType, out rowLineStatus))
                {
                    info.RowLineStatus = rowLineStatus;
                }
                else
                {
                    info.RowLineStatus = null;
                }

                if (ParseGeneralSwitch.ParseMCGeneralSwitchStatus(generalStatus, monitorData.IsConnectMC, out _generalSwitchList))
                {
                    info.GeneralSwitchList = _generalSwitchList;
                }
                else
                {
                    info.GeneralSwitchList = null;
                }
            }
            else
            {
                info.RowLineStatus = null;
                info.GeneralSwitchList = null;
            }
            info.MonitorData = monitorData;
        }
        #endregion

        #region ͳ�ƹ��Ϻ͸澯��Ϣ
        private void StatisticsFaultAndAlarmInfo()
        {
            _isHasValidMonitorInfo = false;
            InitAllVariables();

            if (!_isOnlyOneComplexScreen)
            {
                IGridBiningObject bindObj = null;
                ScanBoardGridBindObj scanBdGridObj = null;
                ComplexScreenGridBindObj complexScreenGridObj = null;

                //�������ӣ��ж϶�Ӧ��CustomObj��CommPortName��ˢ�µĴ��ں���ͬʱ����������Ӧ�ļ������
                foreach (string key in this._statusRectangleLayout.GridDic.Keys)
                {
                    _curScanBoardKey = key;
                    bindObj = (IGridBiningObject)_statusRectangleLayout[key].CustomObj;
                    if (bindObj.Type == GridType.ScanBoardGird)
                    {
                        scanBdGridObj = (ScanBoardGridBindObj)bindObj;

                        GetOneScanBoardStatisticsInfo(key, scanBdGridObj.ScanBoardAndMonitorInfo);
                    }
                    else if (bindObj.Type == GridType.ComplexScreenGrid)
                    {
                        complexScreenGridObj = (ComplexScreenGridBindObj)bindObj;
                        GetComplexScreenStatisticsInfo(complexScreenGridObj.ComplexScreenLayout);
                    }
                }
            }
            else
            {
                GetComplexScreenStatisticsInfo(_complexScreenLayout);
            }
            if (_curType == MonitorDisplayType.Temperature
                || _curType == MonitorDisplayType.Humidity)
            {
                //�¶�ʪ��ʱ����ȡƽ��ֵ
                GetAverageValueAndBeyondCount();
            }
            //��ʾͳ����Ϣ
            DisplayStatisticsInfo();
        }
        private void GetComplexScreenStatisticsInfo(UC_ComplexScreenLayout complexLayout)
        {
            complexLayout.CurType = this._curType;
            complexLayout.IsDiplayAllType = this._isDisplayAll;
            complexLayout.CorGradePartition = this._clrGradePartition;
            complexLayout.TempAlarmThreshold = this._curMonitorConfigInfo.TempAlarmThreshold;
            complexLayout.HumiAlarmThreshold = this._curMonitorConfigInfo.HumiAlarmThreshold;
            complexLayout.MCFanInfo = this._curMonitorConfigInfo.MCFanInfo;
            complexLayout.MCPowerInfo = this._curMonitorConfigInfo.MCPowerInfo;
            complexLayout.IsDisplaySBValt = this._curMonitorConfigInfo.IsDisplayScanBoardVolt;
            complexLayout.GetStatisticsInfo();
            if (complexLayout.IsAllMonitorDataIsValid)
            {
                _isHasValidMonitorInfo = true;
            }
            switch (_curType)
            {
                case MonitorDisplayType.SBStatus:
                    //_curScanBordCount += complexLayout.ScanBoardCnt;
                    _curFaultInfo.SBStatusErrCount += complexLayout.FaultInfo.SBStatusErrCount;
                    _curAlarmInfo.SBStatusErrCount += complexLayout.AlarmInfo.SBStatusErrCount;
                    break;
                case MonitorDisplayType.MCStatus:
                    //_curScanBordCount += complexLayout.ScanBoardCnt;
                    _curFaultInfo.MCStatusErrCount += complexLayout.FaultInfo.MCStatusErrCount;
                    _curAlarmInfo.MCStatusErrCount += complexLayout.AlarmInfo.MCStatusErrCount;
                    break;
                case MonitorDisplayType.Smoke:
                    //_curScanBordCount += complexLayout.ScanBoardCnt;
                    //_curFaultInfo.SmokeAlarmCount += complexLayout.FaultInfo.SmokeAlarmCount;
                    _curAlarmInfo.SmokeAlarmCount += complexLayout.AlarmInfo.SmokeAlarmCount;
                    break;
                case MonitorDisplayType.Temperature:
                    #region ͳ���¶�
                    if (complexLayout.TempStatisticsInfo.ValidScanBoardCnt > 0)
                    {
                        if (_tempStatisInfo.ValidScanBoardCnt <= 0)
                        {
                            _tempStatisInfo.MinValue = complexLayout.TempStatisticsInfo.MinValue;
                            _tempStatisInfo.MaxValue = complexLayout.TempStatisticsInfo.MaxValue;

                            _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                            _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                        }
                        else
                        {
                            //��ȡ��ǰ��С�¶Ⱥ�����¶�
                            if (_tempStatisInfo.MinValue > complexLayout.TempStatisticsInfo.MinValue)
                            {
                                _curTempMaxMinKeyInfo.MinSBKeyList.Clear();
                                _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                                _tempStatisInfo.MinValue = complexLayout.TempStatisticsInfo.MinValue;
                            }
                            else if (_tempStatisInfo.MinValue == complexLayout.TempStatisticsInfo.MinValue)
                            {
                                _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                            }

                            if (_tempStatisInfo.MaxValue < complexLayout.TempStatisticsInfo.MaxValue)
                            {
                                _curTempMaxMinKeyInfo.MaxSBKeyList.Clear();
                                _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                                _tempStatisInfo.MaxValue = complexLayout.TempStatisticsInfo.MaxValue;
                            }
                            else if (_tempStatisInfo.MaxValue == complexLayout.TempStatisticsInfo.MaxValue)
                            {
                                _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                            }
                        }

                        //_curFaultInfo.TemperatureAlarmCount += complexLayout.FaultInfo.TemperatureAlarmCount;
                        _curAlarmInfo.TemperatureAlarmCount += complexLayout.AlarmInfo.TemperatureAlarmCount;
                        _tempStatisInfo.ValidScanBoardCnt += complexLayout.TempStatisticsInfo.ValidScanBoardCnt;
                        _tempStatisInfo.TotalValue += complexLayout.TempStatisticsInfo.TotalValue;
                        _tempStatisInfo.AllValueList.AddRange(complexLayout.TempStatisticsInfo.AllValueList);
                    }
                    #endregion
                    break;
                case MonitorDisplayType.Humidity:
                    if (complexLayout.HumiStatisticsInfo.ValidScanBoardCnt > 0)
                    {
                        if (_humiStatisInfo.ValidScanBoardCnt <= 0)
                        {
                            _humiStatisInfo.MinValue = complexLayout.HumiStatisticsInfo.MinValue;
                            _humiStatisInfo.MaxValue = complexLayout.HumiStatisticsInfo.MaxValue;

                            _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                            _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                        }
                        else
                        {
                            //��ȡ��ǰ��С�¶Ⱥ�����¶�
                            if (_humiStatisInfo.MinValue > complexLayout.HumiStatisticsInfo.MinValue)
                            {
                                _curHumiMaxMinKeyInfo.MinSBKeyList.Clear();
                                _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                                _humiStatisInfo.MinValue = complexLayout.HumiStatisticsInfo.MinValue;
                            }
                            else if (_humiStatisInfo.MinValue == complexLayout.HumiStatisticsInfo.MinValue)
                            {
                                _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
                            }

                            if (_humiStatisInfo.MaxValue < complexLayout.HumiStatisticsInfo.MaxValue)
                            {
                                _curHumiMaxMinKeyInfo.MaxSBKeyList.Clear();
                                _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                                _humiStatisInfo.MaxValue = complexLayout.HumiStatisticsInfo.MaxValue;
                            }
                            else if (_humiStatisInfo.MaxValue == complexLayout.HumiStatisticsInfo.MaxValue)
                            {
                                _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                            }
                        }

                        //_curFaultInfo.HumidityAlarmCount += complexLayout.FaultInfo.HumidityAlarmCount;
                        _curAlarmInfo.HumidityAlarmCount += complexLayout.AlarmInfo.HumidityAlarmCount;
                        _humiStatisInfo.ValidScanBoardCnt += complexLayout.HumiStatisticsInfo.ValidScanBoardCnt;
                        _humiStatisInfo.TotalValue += complexLayout.HumiStatisticsInfo.TotalValue;
                        _humiStatisInfo.AllValueList.AddRange(complexLayout.HumiStatisticsInfo.AllValueList);
                    }
                    break;
                case MonitorDisplayType.Fan:
                    _totalFanSwitchCount += complexLayout.FanTotalCnt;
                    //_curFaultInfo.FanAlarmSwitchCount += complexLayout.FaultInfo.FanAlarmSwitchCount;
                    _curAlarmInfo.FanAlarmSwitchCount += complexLayout.AlarmInfo.FanAlarmSwitchCount;
                    break;
                case MonitorDisplayType.Power:
                    _totalPowerSwitchCount += complexLayout.PowerTotalCnt;
                    //_curFaultInfo.PowerAlarmSwitchCount += complexLayout.FaultInfo.PowerAlarmSwitchCount;
                    _curAlarmInfo.PowerAlarmSwitchCount += complexLayout.AlarmInfo.PowerAlarmSwitchCount;
                    break;
                case MonitorDisplayType.RowLine:
                    _curFaultInfo.SoketAlarmCount += complexLayout.FaultInfo.SoketAlarmCount;
                    //_curAlarmInfo.SoketAlarmCount += complexLayout.AlarmInfo.SoketAlarmCount;
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    //_curFaultInfo.GeneralSwitchErrCount += complexLayout.FaultInfo.GeneralSwitchErrCount;
                    _curAlarmInfo.GeneralSwitchErrCount += complexLayout.AlarmInfo.GeneralSwitchErrCount;
                    break;
            }
        }
        /// <summary>
        /// ��ʾͳ����Ϣ
        /// </summary>
        private void DisplayStatisticsInfo()
        {
            switch (_curType)
            {
                case MonitorDisplayType.SBStatus:
                    _sbStatusInfoUC.TotalCount = _curScanBordCount.ToString();
                    _sbStatusInfoUC.FaultCount = _curFaultInfo.SBStatusErrCount.ToString();
                    _sbStatusInfoUC.AlarmCount = _curAlarmInfo.SBStatusErrCount.ToString();
                    break;
                case MonitorDisplayType.MCStatus:
                    _mcStatusInfoUC.TotalCount = _curScanBordCount.ToString();
                    _mcStatusInfoUC.FaultCount = _curFaultInfo.MCStatusErrCount.ToString();
                    _mcStatusInfoUC.AlarmCount = _curAlarmInfo.MCStatusErrCount.ToString();
                    break;
                case MonitorDisplayType.Smoke:
                    _smokeInfo.TotalCount = _curScanBordCount.ToString();
                    _smokeInfo.AlarmCount = _curAlarmInfo.SmokeAlarmCount.ToString();
                    break;
                case MonitorDisplayType.Temperature:
                    if (_isHasValidMonitorInfo)
                    {
                        _tempInfoUC.MinValueStr = ((int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(_curMonitorConfigInfo.TempDisplayType,
                                                                                                             _tempStatisInfo.MinValue)).ToString();
                        _tempInfoUC.MaxValueStr = ((int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(_curMonitorConfigInfo.TempDisplayType,
                                                                                                             _tempStatisInfo.MaxValue)).ToString();
                        _tempInfoUC.AverageValueStr = ((int)GetMonitorColorAndValue.GetDisplayTempValueByCelsius(_curMonitorConfigInfo.TempDisplayType,
                                                                                                                 _tempStatisInfo.AverageValue)).ToString();
                    }
                    else
                    {
                        _tempInfoUC.MinValueStr = ((int)_tempStatisInfo.MinValue).ToString();
                        _tempInfoUC.MaxValueStr = ((int)_tempStatisInfo.MaxValue).ToString();
                        _tempInfoUC.AverageValueStr = ((int)_tempStatisInfo.AverageValue).ToString();
                    }
                    _tempInfoUC.BeyondAverageCount = _tempStatisInfo.BeyondAverageCnt.ToString();
                    _tempInfoUC.TempAlarmCnt = _curAlarmInfo.TemperatureAlarmCount.ToString();
                    break;
                case MonitorDisplayType.Humidity:
                    _humidityInfoUC.MinValueStr = ((int)_humiStatisInfo.MinValue).ToString();
                    _humidityInfoUC.MaxValueStr = ((int)_humiStatisInfo.MaxValue).ToString();
                    _humidityInfoUC.AverageValueStr = ((int)_humiStatisInfo.AverageValue).ToString();
                    _humidityInfoUC.BeyondAverageCount = _humiStatisInfo.BeyondAverageCnt.ToString();
                    _humidityInfoUC.TempAlarmCnt = _curAlarmInfo.HumidityAlarmCount.ToString();
                    break;
                case MonitorDisplayType.Fan:
                    _fanInfoUC.TotalCount = _totalFanSwitchCount.ToString();
                    _fanInfoUC.AlarmCount = _curAlarmInfo.FanAlarmSwitchCount.ToString();
                    break;
                case MonitorDisplayType.Power:
                    _powerInfoUC.TotalCount = _totalPowerSwitchCount.ToString();
                    _powerInfoUC.AlarmCount = _curAlarmInfo.PowerAlarmSwitchCount.ToString();
                    break;
                case MonitorDisplayType.RowLine:
                    _rowLineInfoUC.TotalCount = _curScanBordCount.ToString();
                    _rowLineInfoUC.FaultCount = _curFaultInfo.SoketAlarmCount.ToString();
                    //_rowLineInfoUC.AlarmCount = _curAlarmInfo.SoketAlarmCount.ToString();
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    _generalStatuInfoUC.TotalCount = _curScanBordCount.ToString();
                    _generalStatuInfoUC.FaultCount = _curAlarmInfo.GeneralSwitchErrCount.ToString();
                    //_rowLineInfoUC.AlarmCount = _curAlarmInfo.SoketAlarmCount.ToString();
                    break;
            }
        }
        /// <summary>
        /// ��ȡ������ʾ���͵���ɫ��ֵ���ַ���
        /// </summary>
        /// <param name="curSBIndex"></param>
        /// <param name="monitorData"></param>
        /// <param name="backColor"></param>
        /// <param name="diplayObj"></param>
        /// <param name="errFanOrPower"></param>
        private void GetOneScanBoardStatisticsInfo(string monitorKey, SBInfoAndMonitorInfo sbInfAndMonitInf)
        {
            ScannerMonitorData monitorData = null;
            ScanBoardRowLineStatus rowLineStatus = null;
            List<bool> generalStatusList = null;

            if (sbInfAndMonitInf != null)
            {
                if (sbInfAndMonitInf.MonitorData != null)
                {
                    _isHasValidMonitorInfo = true;
                    monitorData = (ScannerMonitorData)sbInfAndMonitInf.MonitorData.Clone();
                }

                if (sbInfAndMonitInf.RowLineStatus != null)
                {
                    rowLineStatus = (ScanBoardRowLineStatus)sbInfAndMonitInf.RowLineStatus.Clone();
                }
                generalStatusList = sbInfAndMonitInf.GeneralSwitchList;
            }
            Color backColor = Color.Gray;
            bool bHasValidInfo = false;
            uint oneTypeFaultAlarmCnt = 0;
            switch (_curType)
            {
                #region ��ȡDataGridView��ʾ�ı�����ɫ
                case MonitorDisplayType.SBStatus:
                    StatisticsMonitorInfo.GetSBStatusFaultInfo(monitorData, out _monitorResType);
                    if (_monitorResType == MonitorInfoResult.Fault)
                    {
                        _curFaultInfo.SBStatusErrCount++;
                    }
                    else if (_monitorResType == MonitorInfoResult.Alarm)
                    {
                        _curAlarmInfo.SBStatusErrCount++;
                    }
                    break;
                case MonitorDisplayType.MCStatus:
                    StatisticsMonitorInfo.GetMCStatusFaultInfo(monitorData, out _monitorResType);
                    if (_monitorResType == MonitorInfoResult.Fault)
                    {
                        _curFaultInfo.MCStatusErrCount++;
                    }
                    else if (_monitorResType == MonitorInfoResult.Alarm)
                    {
                        _curAlarmInfo.MCStatusErrCount++;
                    }
                    break;
                case MonitorDisplayType.Smoke:
                    StatisticsMonitorInfo.GetSmokeAlarmInfo(monitorData, out _monitorResType);
                    if (_monitorResType == MonitorInfoResult.Alarm)
                    {
                        _curAlarmInfo.SmokeAlarmCount++;
                    }
                    break;
                case MonitorDisplayType.Temperature:
                    CaculateTempStatisticsInfo(monitorData);
                    break;
                case MonitorDisplayType.Humidity:
                    CaculateHumidityStatisticsInfo(monitorData);
                    break;
                case MonitorDisplayType.Fan:
                    _curMCFanCnt = GetMonitorColorAndValue.GetMonitorFanCnt(_curMonitorConfigInfo.MCFanInfo, monitorKey);
                    _totalFanSwitchCount += (uint)_curMCFanCnt;
                    StatisticsMonitorInfo.GetFanAlarmInfo(monitorData, _curMCFanCnt,
                                                          _curMonitorConfigInfo.MCFanInfo.AlarmThreshold,
                                                          out bHasValidInfo, out oneTypeFaultAlarmCnt);
                    _curAlarmInfo.FanAlarmSwitchCount += oneTypeFaultAlarmCnt;
                    break;
                case MonitorDisplayType.Power:
                    _curMCPowerCnt = GetMonitorColorAndValue.GetMonitorPowerCnt(_curMonitorConfigInfo.MCPowerInfo, monitorKey);
                    _totalPowerSwitchCount += (uint)_curMCPowerCnt;
                    StatisticsMonitorInfo.GetMCPowerAlarmInfo(monitorData, _curMCPowerCnt,
                                                              _curMonitorConfigInfo.MCPowerInfo.AlarmThreshold,
                                                              _curMonitorConfigInfo.MCPowerInfo.FaultThreshold,
                                                              out bHasValidInfo, out oneTypeFaultAlarmCnt);
                    _curAlarmInfo.PowerAlarmSwitchCount += oneTypeFaultAlarmCnt;
                    break;
                case MonitorDisplayType.RowLine:
                    StatisticsMonitorInfo.GetRowLineFaultInfo(monitorData, rowLineStatus, out _monitorResType);
                    if (_monitorResType == MonitorInfoResult.Fault)
                    {
                        _curFaultInfo.SoketAlarmCount++;
                    }
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    StatisticsMonitorInfo.GetGeneralSwitcAlarmInfo(monitorData, generalStatusList, out _monitorResType);
                    if (_monitorResType == MonitorInfoResult.Alarm)
                    {
                        _curAlarmInfo.GeneralSwitchErrCount++;
                    }
                    break;
                #endregion
            }
        }
        /// <summary>
        /// ��ȡ�¶ȶ�Ӧ����Ϣ
        /// </summary>
        /// <param name="monitorData"></param>
        /// <param name="curIndex"></param>
        /// <param name="backClr"></param>
        /// <param name="displayStr"></param>
        private void CaculateTempStatisticsInfo(ScannerMonitorData monitorData)
        {
            ValueCompareResult valueCompareRes = ValueCompareResult.Unknown;
            StatisticsMonitorInfo.CaculateTempStatisticsInfo(monitorData,
                                                             _curMonitorConfigInfo.TempAlarmThreshold,
                                                             out valueCompareRes,
                                                             out _monitorResType,
                                                             ref _tempStatisInfo);
            if (_monitorResType == MonitorInfoResult.Alarm)
            {
                _curAlarmInfo.TemperatureAlarmCount++;
            }
            if (valueCompareRes == ValueCompareResult.AboveMaxValue)
            {
                _curTempMaxMinKeyInfo.MaxSBKeyList.Clear();
                _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMaxValue)
            {
                _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.BelowMinValue)
            {
                _curTempMaxMinKeyInfo.MinSBKeyList.Clear();
                _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMinValue)
            {
                _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.IsFirstValidValue
                    || valueCompareRes == ValueCompareResult.EqualsBothValue)
            {
                _curTempMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                _curTempMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
        }
        /// <summary>
        /// ��ȡʪ�ȶ�Ӧ����Ϣ
        /// </summary>
        /// <param name="monitorData"></param>
        /// <param name="curIndex"></param>
        /// <param name="backClr"></param>
        /// <param name="displayStr"></param>
        private void CaculateHumidityStatisticsInfo(ScannerMonitorData monitorData)
        {
            ValueCompareResult valueCompareRes = ValueCompareResult.Unknown;
            StatisticsMonitorInfo.CaculateHumiStatisticsInfo(monitorData,
                                                             _curMonitorConfigInfo.HumiAlarmThreshold,
                                                             out valueCompareRes,
                                                             out _monitorResType,
                                                             ref _humiStatisInfo);
            if (_monitorResType == MonitorInfoResult.Alarm)
            {
                _curAlarmInfo.HumidityAlarmCount++;
            }
            if (valueCompareRes == ValueCompareResult.AboveMaxValue)
            {
                _curHumiMaxMinKeyInfo.MaxSBKeyList.Clear();
                _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMaxValue)
            {
                _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.BelowMinValue)
            {
                _curHumiMaxMinKeyInfo.MinSBKeyList.Clear();
                _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMinValue)
            {
                _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
            else if (valueCompareRes == ValueCompareResult.IsFirstValidValue
                    || valueCompareRes == ValueCompareResult.EqualsBothValue)
            {
                _curHumiMaxMinKeyInfo.MaxSBKeyList.Add(_curScanBoardKey);
                _curHumiMaxMinKeyInfo.MinSBKeyList.Add(_curScanBoardKey);
            }
        }
        /// <summary>
        /// ��ȡ��ƽ��ֵ�Ľ��տ�����
        /// </summary>
        /// <param name="averageTemp"></param>
        /// <returns></returns>
        private void GetAverageValueAndBeyondCount()
        {
            switch (_curType)
            {
                case MonitorDisplayType.Temperature:
                    StatisticsMonitorInfo.GetAverageValueAndBeyondCnt(ref _tempStatisInfo);
                    break;
                case MonitorDisplayType.Humidity:
                    StatisticsMonitorInfo.GetAverageValueAndBeyondCnt(ref _humiStatisInfo);
                    break;
            }
        }
        #endregion

        private void SetGridFont(Font newFont)
        {
            if (this._statusRectangleLayout == null
                || this._statusRectangleLayout.GridDic == null)
            {
                return;
            }
            foreach (string key in this._statusRectangleLayout.GridDic.Keys)
            {
                _statusRectangleLayout.GridDic[key].Style.GridFont = newFont;
                _statusRectangleLayout.GridDic[key].SelectStyle.GridFont = newFont;
            }
        }

        /// <summary>
        /// ����ÿ��������ʾ��Ӧ�ؼ��Ŀɼ���
        /// </summary>
        /// <param name="isComplexScreen"></param>
        private void SetDisplayTypeControl()
        {
            switch (_curType)
            {
                #region ���ý���ؼ���Visible����
                case MonitorDisplayType.SBStatus:
                    _sbStatusInfoUC.Visible = true;
                    _sbStatusCorNotice.Visible = true;
                    _fanInfoUC.Visible = false;
                    _powerInfoUC.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.MCStatus:
                    _mcStatusCorNotice.Visible = true;
                    _mcStatusInfoUC.Visible = true;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _powerInfoUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.Temperature:
                    _tempInfoUC.Visible = true;
                    _tempCorNoticeUC.Visible = true;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _powerInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.Humidity:
                    _humidityInfoUC.Visible = true;
                    _humidityCorNoticeUC.Visible = true;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _powerInfoUC.Visible = false;
                    _tempInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.Smoke:
                    _smokeInfo.Visible = true;
                    _smokeOrPowerCorNotice.Visible = true;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _powerInfoUC.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _fanCorNotice.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.Fan:
                    _fanCorNotice.Visible = true;
                    _fanInfoUC.Visible = true;
                    _smokeOrPowerCorNotice.Visible = false;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _powerInfoUC.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _smokeInfo.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.Power:
                    _powerInfoUC.Visible = true;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = true;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.RowLine:
                    _rowLineInfoUC.Visible = true;
                    _rowLineCorNotice.Visible = true;
                    _powerInfoUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    _generalStatuInfoUC.Visible = false;
                    _generalStatusCorNotice.Visible = false;
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    _generalStatuInfoUC.Visible = true;
                    _generalStatusCorNotice.Visible = true;
                    _rowLineInfoUC.Visible = false;
                    _rowLineCorNotice.Visible = false;
                    _powerInfoUC.Visible = false;
                    _smokeOrPowerCorNotice.Visible = false;
                    _fanInfoUC.Visible = false;
                    _sbStatusInfoUC.Visible = false;
                    _sbStatusCorNotice.Visible = false;
                    _tempInfoUC.Visible = false;
                    _humidityInfoUC.Visible = false;
                    _tempCorNoticeUC.Visible = false;
                    _humidityCorNoticeUC.Visible = false;
                    _fanCorNotice.Visible = false;
                    _smokeInfo.Visible = false;
                    _mcStatusCorNotice.Visible = false;
                    _mcStatusInfoUC.Visible = false;
                    break;
                #endregion
            }
        }
        /// <summary>
        /// ʹ���Ƿ�ֻ��һ���������Ŀؼ�����
        /// </summary>
        /// <param name="isOnlyOneComplexScreen"></param>
        private void VisibleControlWhenOnlyOneComplexScreen(bool isOnlyOneComplexScreen)
        {
            if (isOnlyOneComplexScreen)
            {
                //ֻ��һ��������
                _complexScreenLayout.Visible = true;
                _statusRectangleLayout.Visible = false;
                groupBox_ScalingRate.Visible = false;
            }
            else
            {
                //��׼��ʾ���ͼ���ʾ��
                _statusRectangleLayout.Visible = true;
                _complexScreenLayout.Visible = false;
                groupBox_ScalingRate.Visible = true;
            }
        }
        /// <summary>
        /// ��ȡ����keyֵ�ַ��������Ϊ���ں� + ���Ϳ���� + ������� + ���տ����
        /// </summary>
        /// <param name="commPort">���ں�</param>
        /// <param name="senderIndex">���Ϳ����</param>
        /// <param name="portIndex">�������</param>
        /// <param name="connectIndex">���տ����</param>
        /// <returns></returns>
        private string GetRectangleKey(string commPort, int senderIndex, int portIndex, int connectIndex)
        {
            return commPort + SCANBORDADDR_SEPERATE
                   + senderIndex.ToString() + SCANBORDADDR_SEPERATE
                   + portIndex.ToString() + SCANBORDADDR_SEPERATE
                   + connectIndex.ToString();
        }
        /// <summary>
        /// ��ȡ���տ���ַ�ַ��������Ϊ���Ϳ���� + ������� + ���տ����
        /// </summary>
        /// <param name="senderIndex">���Ϳ����</param>
        /// <param name="portIndex">�������</param>
        /// <param name="connectIndex">���տ����</param>
        /// <returns></returns>
        private string GetScanBoardKey(int senderIndex, int portIndex, int connectIndex)
        {
            return senderIndex.ToString() + SCANBORDADDR_SEPERATE
                   + portIndex.ToString() + SCANBORDADDR_SEPERATE
                   + connectIndex.ToString();
        }
        /// <summary>
        /// ���������Ϣ
        /// </summary>
        /// <param name="isSeriousError"></param>
        /// <param name="info"></param>
        private void OutputDebugInfoStr(bool isSeriousError, string info)
        {
            string beginStr = "";
            if (isSeriousError)
            {
                beginStr = AppDomain.CurrentDomain.FriendlyName + NormalClassNameSeperate + CLASS_NAME + SeriousErrorBeginStr;
                Trace.WriteLine(beginStr + info + SeriousErrorInfEnd);
            }
            else
            {
                beginStr = AppDomain.CurrentDomain.FriendlyName + NormalClassNameSeperate + CLASS_NAME + NormalInfoBeginStr;
                Debug.WriteLine(beginStr + info + NormalInfoEnd);
            }
        }
        /// <summary>
        /// ��ʼ�����м�ص���ʱ����
        /// </summary>
        private void InitAllVariables()
        {
            #region ��ʼ������
            _curHightLightType = ClickLinkLabelType.None;
            _curFaultInfo = new MonitorErrData();
            _curAlarmInfo = new MonitorErrData();

            _tempStatisInfo = new TempAndHumiStatisticsInfo();
            _humiStatisInfo = new TempAndHumiStatisticsInfo();

            _totalFanSwitchCount = 0;
            _totalPowerSwitchCount = 0;

            SetMaxTempGridStyle(false);
            SetMinTempGridStyle(false);
            SetMaxHumiGridStyle(false);
            SetMinHumiGridStyle(false);

            _curScanBoardKey = "";
            _curTempMaxMinKeyInfo.MaxSBKeyList.Clear();
            _curTempMaxMinKeyInfo.MinSBKeyList.Clear();

            _curHumiMaxMinKeyInfo.MaxSBKeyList.Clear();
            _curHumiMaxMinKeyInfo.MinSBKeyList.Clear();
            #endregion
        }
        /// <summary>
        /// ���������ʵ�������Сֵ
        /// </summary>
        private void SetCurScaleRatio()
        {
            if (_isDisplayAll)
            {
                return;
            }
            switch (_curType)
            {
                case MonitorDisplayType.SBStatus:
                    vScrollBar_PixelLength.Value = _sbStatusRatio;
                    break;
                case MonitorDisplayType.MCStatus:
                    vScrollBar_PixelLength.Value = _mcStatusRatio;
                    break;
                case MonitorDisplayType.Smoke:
                    vScrollBar_PixelLength.Value = _smokeRatio;
                    break;
                case MonitorDisplayType.Temperature:
                    vScrollBar_PixelLength.Value = _temperatureRatio;
                    break;
                case MonitorDisplayType.Humidity:
                    vScrollBar_PixelLength.Value = _humidityRatio;
                    break;
                case MonitorDisplayType.Fan:
                    vScrollBar_PixelLength.Value = _fanRatio;
                    break;
                case MonitorDisplayType.Power:
                    vScrollBar_PixelLength.Value = _powerRatio;
                    break;
                case MonitorDisplayType.RowLine:
                    vScrollBar_PixelLength.Value = _rowLineRatio;
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    vScrollBar_PixelLength.Value = _generalStatusRatio;
                    break;
            }
            vScrollBar_PixelLength_Scroll(null, null);
        }
        /// <summary>
        /// ��ȡ��ǰ��������
        /// </summary>
        private void GetCurScaleRatio()
        {
            if (_isDisplayAll)
            {
                _allRatio = vScrollBar_PixelLength.Value;
                return;
            }
            switch (_curType)
            {
                case MonitorDisplayType.SBStatus:
                    _sbStatusRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.MCStatus:
                    _mcStatusRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.Smoke:
                    _smokeRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.Temperature:
                    _temperatureRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.Humidity:
                    _humidityRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.Fan:
                    _fanRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.Power:
                    _powerRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.RowLine:
                    _rowLineRatio = vScrollBar_PixelLength.Value;
                    break;
                case MonitorDisplayType.GeneralSwitch:
                    _generalStatusRatio = vScrollBar_PixelLength.Value;
                    break;
            }
        }
        private void SetMaxTempGridStyle(bool isHightLight)
        {
            if (_statusRectangleLayout.GridDic == null)
            {
                return;
            }
            List<string> rowKeyList;
            string hightLightKey = "";
            if (isHightLight)
            {
                bool bIsScrollPosSet = false;
                if (_curTempMaxMinKeyInfo != null
                    && _curTempMaxMinKeyInfo.MaxSBKeyList != null)
                {
                    rowKeyList = _curTempMaxMinKeyInfo.MaxSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = HightLightScanPaintInfo.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = HightLightScanPaintInfo.BoardWidth;
                            if (!bIsScrollPosSet)
                            {
                                bIsScrollPosSet = true;
                                DetectAndSetScrollPosition(_statusRectangleLayout.GridDic[hightLightKey].DrawRegion);
                            }
                        }
                    }

                }
            }
            else
            {
                if (_curTempMaxMinKeyInfo != null
                   && _curTempMaxMinKeyInfo.MaxSBKeyList != null)
                {
                    rowKeyList = _curTempMaxMinKeyInfo.MaxSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = _statusRectangleLayout.DefaultStyle.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = _statusRectangleLayout.DefaultStyle.BoardWidth;
                        }
                    }
                }
            }
        }
        private void SetMinTempGridStyle(bool isHightLight)
        {
            if (_statusRectangleLayout.GridDic == null)
            {
                return;
            }
            List<string> rowKeyList;
            string hightLightKey = "";
            if (isHightLight)
            {
                bool bIsScrollPosSet = false;
                if (_curTempMaxMinKeyInfo != null
                    && _curTempMaxMinKeyInfo.MinSBKeyList != null)
                {
                    rowKeyList = _curTempMaxMinKeyInfo.MinSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = HightLightScanPaintInfo.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = HightLightScanPaintInfo.BoardWidth;
                            if (!bIsScrollPosSet)
                            {
                                bIsScrollPosSet = true;
                                DetectAndSetScrollPosition(_statusRectangleLayout.GridDic[hightLightKey].DrawRegion);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_curTempMaxMinKeyInfo != null
                   && _curTempMaxMinKeyInfo.MinSBKeyList != null)
                {
                    rowKeyList = _curTempMaxMinKeyInfo.MinSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = _statusRectangleLayout.DefaultStyle.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = _statusRectangleLayout.DefaultStyle.BoardWidth;
                        }
                    }
                }
            }
        }
        private void SetMaxHumiGridStyle(bool isHightLight)
        {
            if (_statusRectangleLayout.GridDic == null)
            {
                return;
            }
            List<string> rowKeyList;
            string hightLightKey = "";
            if (isHightLight)
            {
                bool bIsScrollPosSet = false;
                if (_curHumiMaxMinKeyInfo != null
                    && _curHumiMaxMinKeyInfo.MaxSBKeyList != null)
                {
                    rowKeyList = _curHumiMaxMinKeyInfo.MaxSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = HightLightScanPaintInfo.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = HightLightScanPaintInfo.BoardWidth;
                            if (!bIsScrollPosSet)
                            {
                                bIsScrollPosSet = true;
                                DetectAndSetScrollPosition(_statusRectangleLayout.GridDic[hightLightKey].DrawRegion);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_curHumiMaxMinKeyInfo != null
                   && _curHumiMaxMinKeyInfo.MaxSBKeyList != null)
                {
                    rowKeyList = _curHumiMaxMinKeyInfo.MaxSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = _statusRectangleLayout.DefaultStyle.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = _statusRectangleLayout.DefaultStyle.BoardWidth;
                        }
                    }
                }
            }
        }
        private void SetMinHumiGridStyle(bool isHightLight)
        {
            if (_statusRectangleLayout.GridDic == null)
            {
                return;
            }
            List<string> rowKeyList;
            string hightLightKey = "";
            if (isHightLight)
            {
                bool bIsScrollPosSet = false;
                if (_curHumiMaxMinKeyInfo != null
                    && _curHumiMaxMinKeyInfo.MinSBKeyList != null)
                {
                    rowKeyList = _curHumiMaxMinKeyInfo.MinSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = HightLightScanPaintInfo.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = HightLightScanPaintInfo.BoardWidth;
                            if (!bIsScrollPosSet)
                            {
                                bIsScrollPosSet = true;
                                DetectAndSetScrollPosition(_statusRectangleLayout.GridDic[hightLightKey].DrawRegion);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_curHumiMaxMinKeyInfo != null
                   && _curHumiMaxMinKeyInfo.MinSBKeyList != null)
                {
                    rowKeyList = _curHumiMaxMinKeyInfo.MinSBKeyList;
                    for (int i = rowKeyList.Count - 1; i >= 0; i--)
                    {
                        hightLightKey = rowKeyList[i];
                        if (_statusRectangleLayout.GridDic.ContainsKey(hightLightKey))
                        {
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardColor = _statusRectangleLayout.DefaultStyle.BoardColor;
                            _statusRectangleLayout.GridDic[hightLightKey].Style.BoardWidth = _statusRectangleLayout.DefaultStyle.BoardWidth;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// ��⵱ǰ��������λ�ò�����
        /// </summary>
        /// <param name="moduleRegion"></param>
        private void DetectAndSetScrollPosition(RectangleF griDrawRegion)
        {
            RectangleF effectRegion = this._statusRectangleLayout.EffectiveRegion;
            int lastScroll = this._statusRectangleLayout.HScrollPos;
            int diffValue = 10;
            if (griDrawRegion.X <= effectRegion.X)
            {
                lastScroll = this._statusRectangleLayout.HScrollPos;
                diffValue = (int)(effectRegion.X - griDrawRegion.X);
                this._statusRectangleLayout.SetScrollPosition(ScrollOrientation.HorizontalScroll,
                                                              lastScroll - diffValue);
            }
            else if (griDrawRegion.Right >= effectRegion.Right)
            {
                lastScroll = this._statusRectangleLayout.HScrollPos;
                diffValue = (int)(griDrawRegion.Right - effectRegion.Right);
                this._statusRectangleLayout.SetScrollPosition(ScrollOrientation.HorizontalScroll,
                                                              lastScroll + diffValue);
            }

            if (griDrawRegion.Y <= effectRegion.Y)
            {
                lastScroll = this._statusRectangleLayout.VScrollPos;
                diffValue = (int)(effectRegion.Y - griDrawRegion.Y);
                this._statusRectangleLayout.SetScrollPosition(ScrollOrientation.VerticalScroll,
                                                              lastScroll - diffValue);
            }
            else if (griDrawRegion.Bottom >= effectRegion.Bottom)
            {
                lastScroll = this._statusRectangleLayout.VScrollPos;
                diffValue = (int)(griDrawRegion.Bottom - effectRegion.Bottom);
                this._statusRectangleLayout.SetScrollPosition(ScrollOrientation.VerticalScroll,
                                                              lastScroll + diffValue);
            }
        }
        #endregion

        #region �����¼�
        /// <summary>
        /// ����ƶ��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusRectangleLayout_GridMouseMove(object sender, RectangularGridMouseEventArgs e)
        {
            if (_customToolTip == null)
            {
                _customToolTip = new CustomToolTip();
                _customToolTip.Owner = this.ParentForm;
                _customToolTip.TipContentFont = _customToolTipFont;
            }
            if (_isDisplayAll)
            {
                _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, null);
                return;
            }
            if (e.GridInfo == null)
            {
                //��ǰ����ƶ����������޾��θ���
                _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, null);
                return;
            }
            RectangularGridInfo scanBordGridInfo = _statusRectangleLayout[e.GridInfo.Key];
            IGridBiningObject bindObj = (IGridBiningObject)scanBordGridInfo.CustomObj;
            List<string> noticeStrList = null;
            if (bindObj.Type == GridType.ScanBoardGird)
            {
                #region ����Ϊ���տ�
                ScanBoardGridBindObj scanBdGridObj = (ScanBoardGridBindObj)bindObj;
                SBInfoAndMonitorInfo scanBoardMonitorInfo = (SBInfoAndMonitorInfo)scanBdGridObj.ScanBoardAndMonitorInfo.Clone();
                if (scanBoardMonitorInfo != null)
                {
                    if (scanBoardMonitorInfo.ScanBordInfo != null)
                    {
                        noticeStrList = new List<string>();
                        noticeStrList.Add(scanBoardMonitorInfo.CommPortName);// + "-"+ CommonStaticValue.CommPortScreen+ (scanBoardMonitorInfo.ScreenIndex + 1)
                        if (_curCtrlSystemMode == CtrlSytemMode.HasSenderMode)
                        {
                            noticeStrList.Add(CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SenderIndex] + ":"
                                             + (scanBoardMonitorInfo.ScanBordInfo.SenderIndex + 1));
                            noticeStrList.Add(CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.PortIndex] + ":"
                                              + (scanBoardMonitorInfo.ScanBordInfo.PortIndex + 1));
                        }
                        noticeStrList.Add(CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.ScanBordIndex] + ":"
                                          + (scanBoardMonitorInfo.ScanBordInfo.ConnectIndex + 1));
                        noticeStrList.Add(CommonStaticValue.RowColNameStr + ":(" + (scanBoardMonitorInfo.SBRowIndex + 1) + ","
                                         + (scanBoardMonitorInfo.SBColIndex + 1) + ")");
                        noticeStrList.Add("(X, Y):(" + e.GridInfo.Region.X + "," + e.GridInfo.Region.Y + ")");
                        noticeStrList.Add("(W, H):(" + e.GridInfo.Region.Width + "," + e.GridInfo.Region.Height + ")");
                        if (scanBoardMonitorInfo.MonitorData != null)
                        {
                            switch (_curType)
                            {
                                #region ��ȡ��Ҫ��ʾ�����ͺ�ֵ�ַ���
                                case MonitorDisplayType.SBStatus:
                                    #region ״̬
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetSBStatusNoticeStr(scanBoardMonitorInfo.MonitorData));
                                    #endregion
                                    break;
                                case MonitorDisplayType.MCStatus:
                                    #region ״̬
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetMCStatusNoticeStr(scanBoardMonitorInfo.MonitorData));
                                    #endregion
                                    break;
                                case MonitorDisplayType.Smoke:
                                    #region ����
                                    noticeStrList.Add(GetMonitorColorAndValue.GetSmokeNoticeStr(scanBoardMonitorInfo.MonitorData));
                                    #endregion
                                    break;
                                case MonitorDisplayType.Temperature:
                                    #region �¶�
                                    noticeStrList.Add(GetMonitorColorAndValue.GetTemperatureNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                      _curMonitorConfigInfo.TempDisplayType));
                                    #endregion
                                    break;
                                case MonitorDisplayType.Humidity:
                                    #region ʪ��
                                    noticeStrList.Add(GetMonitorColorAndValue.GetHumidityNoticeStr(scanBoardMonitorInfo.MonitorData));
                                    #endregion
                                    break;
                                case MonitorDisplayType.Fan:
                                    #region ����
                                    uint fanCnt = GetMonitorColorAndValue.GetMonitorFanCnt(_curMonitorConfigInfo.MCFanInfo, e.GridInfo.Key);
                                    if (fanCnt > 0)
                                    {
                                        float oneHeight = e.GridInfo.DrawRegion.Height / fanCnt;
                                        float startY = e.GridInfo.DrawRegion.Top;
                                        for (int i = 0; i < fanCnt; i++)
                                        {
                                            if (e.BaseMouseEventArgs.Y > i * oneHeight + startY
                                               && e.BaseMouseEventArgs.Y <= (i + 1) * oneHeight + startY)
                                            {
                                                noticeStrList.AddRange(GetMonitorColorAndValue.GetFanNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                               _curMonitorConfigInfo.MCFanInfo, i,
                                                                                                               e.GridInfo.Key));
                                                break;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        noticeStrList.AddRange(GetMonitorColorAndValue.GetFanNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                       _curMonitorConfigInfo.MCFanInfo,
                                                                                                       e.GridInfo.Key));
                                    }
                                    #endregion
                                    break;
                                case MonitorDisplayType.Power:
                                    #region ��Դ
                                    uint powerCnt = GetMonitorColorAndValue.GetMonitorPowerCnt(_curMonitorConfigInfo.MCPowerInfo, e.GridInfo.Key);
                                    if (powerCnt > 0)
                                    {
                                        float oneHeight = e.GridInfo.DrawRegion.Height / powerCnt;
                                        float startY = e.GridInfo.DrawRegion.Top;
                                        for (int i = 0; i < powerCnt; i++)
                                        {
                                            if (e.BaseMouseEventArgs.Y > i * oneHeight + startY
                                               && e.BaseMouseEventArgs.Y <= (i + 1) * oneHeight + startY)
                                            {
                                                noticeStrList.AddRange(GetMonitorColorAndValue.GetPowerNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                        _curMonitorConfigInfo.MCPowerInfo, i,
                                                                        e.GridInfo.Key));
                                                break;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        noticeStrList.AddRange(GetMonitorColorAndValue.GetPowerNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                               _curMonitorConfigInfo.MCPowerInfo,
                                                               _curMonitorConfigInfo.IsDisplayScanBoardVolt,
                                                                e.GridInfo.Key));
                                    }
                                    #endregion
                                    break;
                                case MonitorDisplayType.RowLine:
                                    #region ����״̬
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetRowLineNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                       scanBoardMonitorInfo.RowLineStatus));
                                    #endregion
                                    break;
                                case MonitorDisplayType.GeneralSwitch:
                                    #region ͨ�ÿ���״̬

                                    noticeStrList.Add(GetMonitorColorAndValue.GetGeneralSwitchNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                  scanBoardMonitorInfo.GeneralSwitchList));

                                    #endregion
                                    break;
                                default:
                                    _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, null);
                                    return;
                                #endregion
                            }
                        }
                    }
                    if (noticeStrList != null)
                    {
                        foreach(string str in noticeStrList)
                        {
                            System.Diagnostics.Debug.WriteLine("ToolTip:"+str);
                        }
                    }
                    _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, noticeStrList);
                    if (!_customToolTip.TopLevel)
                    {
                        _customToolTip.TopLevel = true;
                    }
                }
                #endregion
            }
            else if (bindObj.Type == GridType.ComplexScreenGrid)
            {
                #region ����Ϊ������
                noticeStrList = new List<string>();
                bool bHasFaultAlarmInfo = false;
                ComplexScreenGridBindObj complexScreenGridObj = (ComplexScreenGridBindObj)bindObj;
                noticeStrList.Add(complexScreenGridObj.CommPortName + "-"
                                  + CommonStaticValue.CommPortScreen
                                  + (complexScreenGridObj.ScreenIndex + 1).ToString());
                if (complexScreenGridObj.ComplexScreenLayout.IsAllMonitorDataIsValid)
                {
                    switch (_curType)
                    {
                        case MonitorDisplayType.SBStatus:
                            #region SBStatus
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SBStatusErrCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.SBStatusDisplayStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.FaultInfo.SBStatusErrCount);
                            }
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SBStatusErrCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.SBStatusDisplayStr[2] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SBStatusErrCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.MCStatus:
                            #region MCStatus
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.MCStatusErrCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.MCStatusDisplayStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.FaultInfo.MCStatusErrCount);
                            }
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.MCStatusErrCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.MCStatusDisplayStr[2] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.MCStatusErrCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.Smoke:
                            #region Smoke
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SmokeAlarmCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.SmokeDisplayStr[1] + complexScreenGridObj.ComplexScreenLayout.FaultInfo.SmokeAlarmCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SmokeAlarmCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.SmokeDisplayStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SmokeAlarmCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.Temperature:
                            #region Temperature
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.TemperatureAlarmCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.TemperatureDisplayStr[4] + complexScreenGridObj.ComplexScreenLayout.FaultInfo.TemperatureAlarmCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.TemperatureAlarmCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.TemperatureDisplayStr[4] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.TemperatureAlarmCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.Humidity:
                            #region Humidity
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.HumidityAlarmCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.HumidityDisplayStr[4] + complexScreenGridObj.ComplexScreenLayout.FaultInfo.HumidityAlarmCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.HumidityAlarmCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.HumidityDisplayStr[4] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.HumidityAlarmCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.Fan:
                            #region Fan
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.FanAlarmSwitchCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.FanDisplayStr[1] + complexScreenGridObj.ComplexScreenLayout.FaultInfo.FanAlarmSwitchCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.FanAlarmSwitchCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.FanDisplayStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.FanAlarmSwitchCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.Power:
                            #region Power
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.PowerAlarmSwitchCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.PowerDisplayStr[1] + complexScreenGridObj.ComplexScreenLayout.FaultInfo.PowerAlarmSwitchCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.PowerAlarmSwitchCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.PowerDisplayStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.PowerAlarmSwitchCount);
                            }
                            break;
                            #endregion
                        case MonitorDisplayType.RowLine:
                            #region RowLine
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SoketAlarmCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.RowLineStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.FaultInfo.SoketAlarmCount);
                            }
                            //if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SoketAlarmCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.RowLineStr[1] + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SoketAlarmCount);
                            //}
                            break;
                            #endregion
                        case MonitorDisplayType.GeneralSwitch:
                            #region GeneralSwitch
                            //if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.GeneralSwitchErrCount > 0)
                            //{
                            //    noticeStrList.Add(CommonStr.GeneralStatusStr[1] + ":"
                            //                      + complexScreenGridObj.ComplexScreenLayout.FaultInfo.GeneralSwitchErrCount);
                            //}
                            if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.GeneralSwitchErrCount > 0)
                            {
                                bHasFaultAlarmInfo = true;
                                noticeStrList.Add(CommonStaticValue.GeneralStatusStr[1] + ":"
                                                  + complexScreenGridObj.ComplexScreenLayout.AlarmInfo.GeneralSwitchErrCount);
                            }
                            else
                            {
                                noticeStrList.Add(CommonStaticValue.DisplayTypeStr[(int)_curType] + ":"
                                                 + CommonStaticValue.CabinetDoorStatusStr[(int)CommonStaticValue.CabinetDoorStatusType.Close]);
                            }
                            return;
                            #endregion
                        default:
                            _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, null);
                            return;
                    }
                    if (!bHasFaultAlarmInfo)
                    {
                        noticeStrList.Add(CommonStaticValue.DisplayTypeStr[(int)_curType] + ":"
                                             + CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.OK]);
                    }
                }
                else
                {
                    noticeStrList.Add(CommonStaticValue.DisplayTypeStr[(int)_curType] + ":"
                                      + CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown]);
                }
                noticeStrList.Add(CommonStaticValue.DoubleClickToViewInfo);
                _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, noticeStrList);
                if (!_customToolTip.TopLevel)
                {
                    _customToolTip.TopLevel = true;
                }
                #endregion
            }
            else if (bindObj.Type == GridType.ScreenGrid)
            {
                #region ����Ϊ����߽�
                ScreenGridBindObj screenGridObj = (ScreenGridBindObj)bindObj;
                if (screenGridObj.ScreenIsValid)
                {
                    _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, null);
                    return;
                }
                noticeStrList = new List<string>();
                noticeStrList.Add(screenGridObj.CommPortName + "-"
                                + CommonStaticValue.CommPortScreen + (screenGridObj.ScreenIndex + 1).ToString()
                                + CommonStaticValue.ScreenNotExist);
                _customToolTip.SetTipInfo(_statusRectangleLayout.DrawPanel, noticeStrList);
                if (!_customToolTip.TopLevel)
                {
                    _customToolTip.TopLevel = true;
                }
                #endregion
            }
        }
        /// <summary>
        /// ����˫���¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusRectangleLayout_GridMouseDoubleClick(object sender, RectangularGridMouseEventArgs e)
        {
            if (_isDisplayAll)
            {
                return;
            }
            if (e.GridInfo == null)
            {
                return;
            }

            RectangularGridInfo scanBordGridInfo = _statusRectangleLayout[e.GridInfo.Key];
            IGridBiningObject bindObj = (IGridBiningObject)scanBordGridInfo.CustomObj;
            if (bindObj.Type != GridType.ComplexScreenGrid)
            {
                return;
            }
            ClickLinkLabelType clickType = ClickLinkLabelType.None;

            #region ��ȡ����������ڸ������е�����
            if (_curType == MonitorDisplayType.Temperature)
            {
                //��ǰ��ʾ�¶�
                if (_curHightLightType == ClickLinkLabelType.MaxValue)
                {
                    //���������Ϊ���ֵ
                    if (_curTempMaxMinKeyInfo != null
                        && _curTempMaxMinKeyInfo.MaxSBKeyList.Contains(e.GridInfo.Key))
                    {
                        clickType = ClickLinkLabelType.MaxValue;
                    }
                }
                else if (_curHightLightType == ClickLinkLabelType.MinValue)
                {
                    //���������Ϊ��Сֵ
                    if (_curTempMaxMinKeyInfo != null
                        && _curTempMaxMinKeyInfo.MinSBKeyList.Contains(e.GridInfo.Key))
                    {
                        clickType = ClickLinkLabelType.MinValue;
                    }
                }
            }
            else if (_curType == MonitorDisplayType.Humidity)
            {
                //��ǰ��ʾʪ��
                if (_curHightLightType == ClickLinkLabelType.MaxValue)
                {
                    //������ֵ
                    if (_curHumiMaxMinKeyInfo != null
                        && _curHumiMaxMinKeyInfo.MaxSBKeyList.Contains(e.GridInfo.Key))
                    {
                        clickType = ClickLinkLabelType.MaxValue;
                    }
                }
                else if (_curHightLightType == ClickLinkLabelType.MinValue)
                {
                    //�����Сֵ
                    if (_curHumiMaxMinKeyInfo != null
                        && _curHumiMaxMinKeyInfo.MinSBKeyList.Contains(e.GridInfo.Key))
                    {
                        clickType = ClickLinkLabelType.MinValue;
                    }
                }
            }
            #endregion

            ComplexScreenGridBindObj screenGridObj = (ComplexScreenGridBindObj)bindObj;

            //TODO  �Ӹ��Ӱ󶨵Ķ����л�ȡ��������DataGridView���󣬲����ö�����򿪵Ĵ�����
            screenGridObj.ComplexScreenLayout.CurType = this._curType;
            screenGridObj.ComplexScreenLayout.IsDiplayAllType = this._isDisplayAll;
            screenGridObj.ComplexScreenLayout.CorGradePartition = this._clrGradePartition;
            screenGridObj.ComplexScreenLayout.TempAlarmThreshold = this._curMonitorConfigInfo.TempAlarmThreshold;
            screenGridObj.ComplexScreenLayout.HumiAlarmThreshold = this._curMonitorConfigInfo.HumiAlarmThreshold;
            screenGridObj.ComplexScreenLayout.MCFanInfo = this._curMonitorConfigInfo.MCFanInfo;
            screenGridObj.ComplexScreenLayout.MCPowerInfo = this._curMonitorConfigInfo.MCPowerInfo;
            screenGridObj.ComplexScreenLayout.IsDisplaySBValt = this._curMonitorConfigInfo.IsDisplayScanBoardVolt;
            screenGridObj.ComplexScreenLayout.TempDisplayType = this._curMonitorConfigInfo.TempDisplayType;
            screenGridObj.ComplexScreenLayout.CurCtrlSystemMode = this._curCtrlSystemMode;
            _complexScreenPreviewFrm = new Frm_ComplexScreenDisplay(screenGridObj.ComplexScreenLayout,
                                                                    clickType);
            _complexScreenPreviewFrm.MonitorInfoListFont = _complexScreenFont;
            _complexScreenPreviewFrm.CustomToolTipFont = _customToolTipFont;
            _complexScreenPreviewFrm.CtrlOfFormFont = CurrentFont;
            _complexScreenPreviewFrm.UpdateLanguage(CommonStaticValue.PreviewComplexMonitorInfo);
            _complexScreenPreviewFrm.ShowDialog(this.ParentForm);
        }
        /// <summary>
        /// �����ͷ��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarsScreenMonitorDataViewer_Disposed(object sender, EventArgs e)
        {
            _statusRectangleLayout.GridMouseDoubleClick -= new RectangularGridMouseEventHandler(StatusRectangleLayout_GridMouseDoubleClick);
            _statusRectangleLayout.GridMouseMove -= new RectangularGridMouseEventHandler(StatusRectangleLayout_GridMouseMove);
            if (_customToolTip != null)
            {
                _customToolTip.Close();
                _customToolTip.Dispose();
            }
            if (_complexScreenScanBoardMonitorInfo != null)
            {
                _complexScreenScanBoardMonitorInfo = null;
            }
            if (_curAllLedScreenRectDic != null)
            {
                _curAllLedScreenRectDic.Clear();
                _curAllLedScreenRectDic = null;
            }

            if (_complexScreenLayout != null)
            {
                _complexScreenLayout.ClearAllMonitorInfo();
            }

            this.Disposed -= new EventHandler(MarsScreenMonitorDataViewer_Disposed);
        }
        /// <summary>
        /// ���ڵ�Visible���Ա仯ʱ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarsScreenMonitorDataViewer_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (_curOperateStep == OperateStep.ConfigScanBoard)
                {
                    if (_isOnlyOneComplexScreen)
                    {
                        _indicator.Size = new Size(_complexScreenLayout.Width / 4, _complexScreenLayout.Height / 4);
                        _indicator.Parent = _complexScreenLayout;
                        _indicator.Show();
                        _indicator.Location = new Point((_complexScreenLayout.Width - _indicator.Width) / 2,
                                                        (_complexScreenLayout.Height - _indicator.Height) / 2);
                    }
                    else
                    {
                        _indicator.Size = new Size(_statusRectangleLayout.Width / 4, _statusRectangleLayout.Height / 4);
                        _indicator.Parent = _statusRectangleLayout;
                        _indicator.Show();
                        _indicator.Location = new Point((_statusRectangleLayout.Width - _indicator.Width) / 2,
                                                            (_statusRectangleLayout.Height - _indicator.Height) / 2);
                    }
                    _indicator.BringToFront();
                    _indicator.Start();
                }
            }
        }
        /// <summary>
        /// �����ʱ仯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vScrollBar_PixelLength_Scroll(object sender, ScrollEventArgs e)
        {
            _curOperateStep = OperateStep.SetScaleRatio;
            _isNeedSetScaleRatio = false;
            GetCurScaleRatio();
            _statusRectangleLayout.LengthForOnePixel = vScrollBar_PixelLength.Value / 100.0f;
            label_Value.Text = (vScrollBar_PixelLength.Value / 100.0f).ToString("f2");
            _curOperateStep = OperateStep.None;
        }
        private void ClickTempMaxMinValue(object sender, ClickMaxMinValueEventArgs e)
        {
            _curHightLightType = e.Type;
            if (e.Type == ClickLinkLabelType.None)
            {
                return;
            }
            if (_isOnlyOneComplexScreen)
            {
                _complexScreenLayout.ShowMaxMinRow(e.Type);
            }
            else
            {
                if (e.Type == ClickLinkLabelType.MaxValue)
                {
                    SetMinTempGridStyle(false);
                    SetMaxTempGridStyle(true);
                }
                else
                {
                    SetMaxTempGridStyle(false);
                    SetMinTempGridStyle(true);
                }
                _statusRectangleLayout.Refresh();
            }
        }
        private void ClickHumiMaxMinValue(object sender, ClickMaxMinValueEventArgs e)
        {
            _curHightLightType = e.Type;
            if (e.Type == ClickLinkLabelType.None)
            {
                return;
            }
            if (_isOnlyOneComplexScreen)
            {
                _complexScreenLayout.ShowMaxMinRow(e.Type);
            }
            else
            {
                if (e.Type == ClickLinkLabelType.MaxValue)
                {
                    SetMinHumiGridStyle(false);
                    SetMaxHumiGridStyle(true);
                }
                else
                {
                    SetMaxHumiGridStyle(false);
                    SetMinHumiGridStyle(true);
                }
                _statusRectangleLayout.Refresh();
            }
        }
        #endregion

    }
}