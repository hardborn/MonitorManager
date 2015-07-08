using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Nova.LCT.GigabitSystem.Common;
using Nova.Convert;
using Nova.Control;
using Nova.LCT.GigabitSystem.Monitor;

namespace Nova.Monitoring.UI.ScannerStatusDisplay
{
    public partial class UC_ComplexScreenLayout : UserControl
    {
        #region ˽���ֶ�
        public class MaxMinInfoIndex
        {
            public List<int> MinSBIndexList = new List<int>();
            public List<int> MaxSBIndexList = new List<int>();
        }
        //��ǰ��ʾ����
        private MonitorDisplayType _curType;
        //������ֵ��ȡ��Ӧ����ɫ
        private ColorGradePartition _clrGradePartition = null;
        private CustomToolTip _customToolTip = null;

        private bool _isDisplayAll = false;
        private int _powerCntTemp = 0;
        private uint _curMCFanCnt = 0;
        private uint _curMCPowerCnt = 0;


        private MonitorInfoResult _tempMonitorRes = MonitorInfoResult.Ok;

        //DataGridView�����е���С���
        private static int ColDefaultWidth = 80;

        private ScannerMonitorData _monitorDataTemp = null;

        private List<SBInfoAndMonitorInfo> _curAllScanBoardMonitorInfList = null;

        private MonitorConfigData _curMonitorConfigInfo = null;

        private MonitorErrData _curFaultInfo = new MonitorErrData();
        private MonitorErrData _curAlarmInfo = new MonitorErrData();
        private MonitorInfoResult _monitorResType;

        //private int _scanCntOfInfoValid = 0;
        ////��ǰ���տ�����
        //private int _curScanBordCount = 0;

        //��ǰ��ʾ��������·��
        private uint _totalFanSwitchCount = 0;
        //��ǰ��ʾ����Դ��·��
        private uint _totalPowerSwitchCount = 0;

        ////��ǰ����¶�
        //private float _curMinTempValue = 0;
        ////��ǰ����¶�
        //private float _curMaxTempValue = 0;
        ////��ǰ����¶�
        //private float _curMinHumidityValue = 0;
        ////��ǰ����¶�
        //private float _curMaxHumidityValue = 0;
        ////��ǰ���н��տ������¶�
        //private float _curTotalTemp = 0;
        ////��ǰ���н��տ�����ʪ��
        //private float _curTotalHumidity = 0;
        //// ��ǰ��ʾ����Ӧ���¶��б�
        //private List<float> _curScreenTempList = new List<float>();
        ////��ǰ��ʾ����ʪ��List
        //private List<float> _curScreenHumidityList = new List<float>();

        private bool _allScanBoardMonitorDataIsValid = false;
        private int _curScanBoardIndex = -1;

        private TempAndHumiStatisticsInfo _tempStatisInfo = null;
        private TempAndHumiStatisticsInfo _humiStatisInfo = null;
        private MaxMinInfoIndex _curTempMaxMinIndexInfo = null;
        private MaxMinInfoIndex _curHumiMaxMinIndexInfo = null;
        private ClickLinkLabelType _curHightLightType = ClickLinkLabelType.None;

        private CtrlSytemMode _curCtrlSystemMode = CtrlSytemMode.HasSenderMode;
        #endregion

        #region ����
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
            }
        }
        /// <summary>
        /// �Ƿ���ʾ���м����Ϣ
        /// </summary>
        public bool IsDiplayAllType
        {
            get { return _isDisplayAll; }
            set { _isDisplayAll = value; }
        }
        /// <summary>
        /// ��ȡ��ɫ����
        /// </summary>
        public ColorGradePartition CorGradePartition
        {
            get { return _clrGradePartition; }
            set { _clrGradePartition = value; }
        }
        /// <summary>
        /// �¶���ֵ
        /// </summary>
        public float TempAlarmThreshold
        {
            get { return _curMonitorConfigInfo.TempAlarmThreshold; }
            set
            {
                if (_curMonitorConfigInfo.TempAlarmThreshold == value)
                {
                    return;
                }
                _curMonitorConfigInfo.TempAlarmThreshold = value;
            }
        }
        /// <summary>
        /// ʪ����ֵ
        /// </summary>
        public float HumiAlarmThreshold
        {
            get { return _curMonitorConfigInfo.HumiAlarmThreshold; }
            set
            {
                if (_curMonitorConfigInfo.HumiAlarmThreshold == value)
                {
                    return;
                }
                _curMonitorConfigInfo.HumiAlarmThreshold = value;
            }
        }
        /// <summary>
        /// �Ƿ���°��صĵ�ѹֵ
        /// </summary>
        public bool IsDisplaySBValt
        {
            get { return _curMonitorConfigInfo.IsDisplayScanBoardVolt; }
            set
            {
                if (_curMonitorConfigInfo.IsDisplayScanBoardVolt == value)
                {
                    return;
                }
                _curMonitorConfigInfo.IsDisplayScanBoardVolt = value;
            }
        }
        /// <summary>
        /// ��ؿ��ķ��ȼ����������
        /// </summary>
        public ScanBdMonitoredParamUpdateInfo MCFanInfo
        {
            get { return _curMonitorConfigInfo.MCFanInfo; }
            set
            {
                _curMonitorConfigInfo.MCFanInfo = value;
            }
        }
        /// <summary>
        /// ��ؿ��ĵ�Դ������ò���
        /// </summary>
        public ScanBdMonitoredPowerInfo MCPowerInfo
        {
            get { return _curMonitorConfigInfo.MCPowerInfo; }
            set
            {
                _curMonitorConfigInfo.MCPowerInfo = value;
            }
        }
        /// <summary>
        /// ��ǰ��ʾ���Ľ��տ���Ϣ�����Ӧ�ļ������
        /// </summary>
        public List<SBInfoAndMonitorInfo> CurAllSBMonitorInfList
        {
            get{ return _curAllScanBoardMonitorInfList; }
        }
        /// <summary>
        /// �¶���ʾ����
        /// </summary>
        public TemperatureType TempDisplayType
        {
            get { return _curMonitorConfigInfo.TempDisplayType; }
            set { _curMonitorConfigInfo.TempDisplayType = value; }
        }

        public uint FanTotalCnt
        {
            get { return _totalFanSwitchCount; }
        }
        public uint PowerTotalCnt
        {
            get { return _totalPowerSwitchCount; }
        }
        public MonitorErrData FaultInfo
        {
            get { return _curFaultInfo; }
        }
        public MonitorErrData AlarmInfo
        {
            get { return _curAlarmInfo; }
        }
        public bool IsAllMonitorDataIsValid
        {
            get { return _allScanBoardMonitorDataIsValid; }
        }
        public TempAndHumiStatisticsInfo TempStatisticsInfo
        {
            get { return _tempStatisInfo; }
        }
        public TempAndHumiStatisticsInfo HumiStatisticsInfo
        {
            get { return _humiStatisInfo; }
        }

        public Font MonitorInfoListFont
        {
            get { return dbfDataGridView_ComplexScreenInfo.DefaultCellStyle.Font; }
            set
            {
                dbfDataGridView_ComplexScreenInfo.DefaultCellStyle.Font = value;
                dbfDataGridView_ComplexScreenInfo.ColumnHeadersDefaultCellStyle.Font = value;
                dbfDataGridView_ComplexScreenInfo.Refresh();
            }
        }
        public Font CustomToolTipFont
        {
            get
            {
                if (_customToolTip != null)
                {
                    return _customToolTip.TipContentFont;
                }
                return this.Font;
            }
            set
            {
                if (_customToolTip != null)
                {
                    _customToolTip.TipContentFont = value;
                }
            }
        }

        public CtrlSytemMode CurCtrlSystemMode
        {
            get { return _curCtrlSystemMode; }
            set
            {
                _curCtrlSystemMode = value;
                int colIndex = 0;
                if (_curCtrlSystemMode == CtrlSytemMode.HasSenderMode)
                {
                    colIndex = (int)CommonStaticValue.ColsHeaderType.SenderIndex;
                    dbfDataGridView_ComplexScreenInfo.Columns[colIndex].Visible = true;

                    colIndex = (int)CommonStaticValue.ColsHeaderType.PortIndex;
                    dbfDataGridView_ComplexScreenInfo.Columns[colIndex].Visible = true;
                }
                else
                {
                    colIndex = (int)CommonStaticValue.ColsHeaderType.SenderIndex;
                    dbfDataGridView_ComplexScreenInfo.Columns[colIndex].Visible = false;

                    colIndex = (int)CommonStaticValue.ColsHeaderType.PortIndex;
                    dbfDataGridView_ComplexScreenInfo.Columns[colIndex].Visible = false;
                }
            }
        }
        #endregion
        
        public UC_ComplexScreenLayout()
        {
            InitializeComponent();

            _allScanBoardMonitorDataIsValid = false;
            _curMonitorConfigInfo = new MonitorConfigData();
            _curTempMaxMinIndexInfo = new MaxMinInfoIndex();
            _curHumiMaxMinIndexInfo = new MaxMinInfoIndex();

            _curAllScanBoardMonitorInfList = new List<SBInfoAndMonitorInfo>();

            dbfDataGridView_ComplexScreenInfo.ReadOnly = true;
            dbfDataGridView_ComplexScreenInfo.RowHeadersVisible = false;
            dbfDataGridView_ComplexScreenInfo.AllowUserToAddRows = false;
            dbfDataGridView_ComplexScreenInfo.AllowUserToDeleteRows = false;
            dbfDataGridView_ComplexScreenInfo.AllowUserToOrderColumns = false;
            dbfDataGridView_ComplexScreenInfo.AllowUserToResizeRows = false;
            dbfDataGridView_ComplexScreenInfo.AllowUserToResizeColumns = false;
            InitDataGridViewCols();
            CurCtrlSystemMode = _curCtrlSystemMode;
        }

        /// <summary>
        /// ���һ�����տ���Ϣ
        /// </summary>
        /// <param name="sbMonitorInfo"></param>
        public void AddOneScanBoardInfo(SBInfoAndMonitorInfo sbMonitorInfo)
        {
            _curAllScanBoardMonitorInfList.Add(sbMonitorInfo);
        }
        /// <summary>
        /// ������м������
        /// </summary>
        public void ClearAllMonitorInfo()
        {
            if (_customToolTip != null)
            {
                _customToolTip.Close();
                _customToolTip.Dispose();
            }
            _curHightLightType = ClickLinkLabelType.None;
            _allScanBoardMonitorDataIsValid = false;
            _curAllScanBoardMonitorInfList.Clear();
            InitAllVariables();
        }
        /// <summary>
        /// ˢ�����н��տ�����Ϣ
        /// </summary>
        public void InvalidateAllScanBoardInfo()
        {
            _curHightLightType = ClickLinkLabelType.None;
            int nCount = _curAllScanBoardMonitorInfList.Count;
            if (dbfDataGridView_ComplexScreenInfo.RowCount > nCount)
            {
                //�Ƴ��������
                int nDiffCount = dbfDataGridView_ComplexScreenInfo.RowCount - nCount;
                for (int i = 0; i < nDiffCount; i++)
                {
                    dbfDataGridView_ComplexScreenInfo.Rows.RemoveAt(0);
                }
            }
            else if (dbfDataGridView_ComplexScreenInfo.RowCount < nCount)
            {
                //��������
                dbfDataGridView_ComplexScreenInfo.RowCount = nCount;
            }

            //�޸���ͷ��ʾ�������ַ���
            dbfDataGridView_ComplexScreenInfo.Columns[0].HeaderText = CommonStaticValue.DisplayTypeStr[(int)_curType];
            //TODO  ˢ�������ж�Ӧ�Ľ��տ���Ϣ
            UpdateAllScanBoardInfo();
        }
        public void GetStatisticsInfo()
        {
            InitAllVariables();
            string key = "";
            _allScanBoardMonitorDataIsValid = false;
            for (int i = 0; i < _curAllScanBoardMonitorInfList.Count; i++)
            {
                _curScanBoardIndex = i;
                key = _curAllScanBoardMonitorInfList[i].SBRectKey;
                if (_curAllScanBoardMonitorInfList[i].MonitorData != null)
                {
                    _allScanBoardMonitorDataIsValid = true;
                }

                GetOneScanBoardStatisticsInfo(key, _curAllScanBoardMonitorInfList[i]);
            }
        }
        /// <summary>
        /// ��ʾ������Сֵ����
        /// </summary>
        /// <param name="clickType"></param>
        public void ShowMaxMinRow(ClickLinkLabelType clickType)
        {
            _curHightLightType = clickType;
            if (clickType == ClickLinkLabelType.None)
            {
                //��ʾ����ΪNone
                return;
            }
            this.dbfDataGridView_ComplexScreenInfo.Refresh();

            #region ���õ�ǰ��ʾ��
            int rowIndex = 0;
            if (_curHightLightType == ClickLinkLabelType.MaxValue)
            {
                if (_curType == MonitorDisplayType.Temperature)
                {
                    if (_curTempMaxMinIndexInfo != null
                        && _curTempMaxMinIndexInfo.MaxSBIndexList != null
                        && _curTempMaxMinIndexInfo.MaxSBIndexList.Count > 0)
                    {
                        //��ʾ�¶����ֵ�ĵ�һ����
                        rowIndex = _curTempMaxMinIndexInfo.MaxSBIndexList[0];
                        if (rowIndex < dbfDataGridView_ComplexScreenInfo.RowCount)
                        {
                            dbfDataGridView_ComplexScreenInfo.CurrentCell = dbfDataGridView_ComplexScreenInfo[0, rowIndex];
                        }
                    }
                }
                else if (_curType == MonitorDisplayType.Humidity)
                {
                    if (_curHumiMaxMinIndexInfo != null
                        && _curHumiMaxMinIndexInfo.MaxSBIndexList != null
                        && _curHumiMaxMinIndexInfo.MaxSBIndexList.Count > 0)
                    {                 
                        //��ʾʪ�����ֵ�ĵ�һ����
                        rowIndex = _curHumiMaxMinIndexInfo.MaxSBIndexList[0];
                        if (rowIndex < dbfDataGridView_ComplexScreenInfo.RowCount)
                        {
                            dbfDataGridView_ComplexScreenInfo.CurrentCell = dbfDataGridView_ComplexScreenInfo[0, rowIndex];
                        }
                    }
                }
            }
            else
            {
                if (_curType == MonitorDisplayType.Temperature)
                {
                    if (_curTempMaxMinIndexInfo != null
                        && _curTempMaxMinIndexInfo.MinSBIndexList != null
                        && _curTempMaxMinIndexInfo.MinSBIndexList.Count > 0)
                    {                 
                        //��ʾ�¶���Сֵ�ĵ�һ����
                        rowIndex = _curTempMaxMinIndexInfo.MinSBIndexList[0];
                        if (rowIndex < dbfDataGridView_ComplexScreenInfo.RowCount)
                        {
                            dbfDataGridView_ComplexScreenInfo.CurrentCell = dbfDataGridView_ComplexScreenInfo[0, rowIndex];
                        }
                    }
                }
                else if (_curType == MonitorDisplayType.Humidity)
                {
                    if (_curHumiMaxMinIndexInfo != null
                        && _curHumiMaxMinIndexInfo.MinSBIndexList != null
                        && _curHumiMaxMinIndexInfo.MinSBIndexList.Count > 0)
                    {         
                        //��ʾʪ����Сֵ�ĵ�һ����
                        rowIndex = _curHumiMaxMinIndexInfo.MinSBIndexList[0];
                        if (rowIndex < dbfDataGridView_ComplexScreenInfo.RowCount)
                        {
                            dbfDataGridView_ComplexScreenInfo.CurrentCell = dbfDataGridView_ComplexScreenInfo[0, rowIndex];
                        }
                    }
                }
            }
            #endregion
        }

        #region ͳ�ƹ��Ϻ͸澯��Ϣ
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
                    if (_curMonitorConfigInfo.IsDisplayScanBoardVolt)
                    {
                        _totalPowerSwitchCount++;
                        if (monitorData != null)
                        {
                            StatisticsMonitorInfo.GetOneFanOrPowerAlarmInfo(monitorData.VoltageOfScanCard,
                                                                            _curMonitorConfigInfo.MCPowerInfo.AlarmThreshold,
                                                                            out _monitorResType);
                            if (_monitorResType == MonitorInfoResult.Alarm)
                            {
                                _curAlarmInfo.PowerAlarmSwitchCount++;
                            }
                        }
                    }
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
                _curTempMaxMinIndexInfo.MaxSBIndexList.Clear();
                _curTempMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMaxValue)
            {
                _curTempMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.BelowMinValue)
            {
                _curTempMaxMinIndexInfo.MinSBIndexList.Clear();
                _curTempMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMinValue)
            {
                _curTempMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.IsFirstValidValue
                    || valueCompareRes == ValueCompareResult.EqualsBothValue)
            {
                _curTempMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
                _curTempMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
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
                _curHumiMaxMinIndexInfo.MaxSBIndexList.Clear();
                _curHumiMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMaxValue)
            {
                _curHumiMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.BelowMinValue)
            {
                _curHumiMaxMinIndexInfo.MinSBIndexList.Clear();
                _curHumiMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.EqualsMinValue)
            {
                _curHumiMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
            }
            else if (valueCompareRes == ValueCompareResult.IsFirstValidValue
                    || valueCompareRes == ValueCompareResult.EqualsBothValue)
            {
                _curHumiMaxMinIndexInfo.MinSBIndexList.Add(_curScanBoardIndex);
                _curHumiMaxMinIndexInfo.MaxSBIndexList.Add(_curScanBoardIndex);
            }
        }
        #endregion

        /// <summary>
        /// ��ʼ��DataGridView����
        /// </summary>
        /// <param name="dataGridView"></param>
        private void InitDataGridViewCols()
        {
            DataGridViewTextBoxColumn column0 = new DataGridViewTextBoxColumn();
            column0.DataPropertyName = "DataInfo";
            column0.HeaderText = CommonStaticValue.DisplayTypeStr[(int)_curType];
            column0.Name = "DataInfo";
            column0.ReadOnly = true;
            column0.MinimumWidth = ColDefaultWidth;
            column0.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column0.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column0);

            DataGridViewTextBoxColumn column1 = new DataGridViewTextBoxColumn();
            column1.DataPropertyName = "SenderIndex";
            column1.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SenderIndex];
            column1.Name = "SenderIndex";
            column1.ReadOnly = true;
            column1.Width = ColDefaultWidth;
            column1.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column1);

            DataGridViewTextBoxColumn column2 = new DataGridViewTextBoxColumn();
            column2.DataPropertyName = "PortIndex";
            column2.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.PortIndex];
            column2.Name = "PortIndex";
            column2.ReadOnly = true;
            column2.Width = ColDefaultWidth;
            column2.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column2);

            DataGridViewTextBoxColumn column3 = new DataGridViewTextBoxColumn();
            column3.DataPropertyName = "ConnectIndex";
            column3.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.ScanBordIndex];
            column3.Name = "ConnectIndex";
            column3.ReadOnly = true;
            column3.MinimumWidth = ColDefaultWidth;
            column3.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column3);

            DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn();
            column4.DataPropertyName = "StartX";
            column4.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.StartX];
            column4.Name = "StartX";
            column4.ReadOnly = true;
            column4.Width = ColDefaultWidth;
            column4.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column4);

            DataGridViewTextBoxColumn column5 = new DataGridViewTextBoxColumn();
            column5.DataPropertyName = "StartY";
            column5.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.StartY]; ;
            column5.Name = "StartY";
            column5.ReadOnly = true;
            column5.Width = ColDefaultWidth;
            column5.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column5);

            DataGridViewTextBoxColumn column6 = new DataGridViewTextBoxColumn();
            column6.DataPropertyName = "ScanBordWidth";
            column6.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SBWidth]; ;
            column6.Name = "ScanBordWidth";
            column6.ReadOnly = true;
            column6.Width = ColDefaultWidth;
            column6.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column6);

            DataGridViewTextBoxColumn column7 = new DataGridViewTextBoxColumn();
            column7.DataPropertyName = "ScanBordHeight";
            column7.HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SBHeight];
            column7.Name = "ScanBordHeight";
            column7.ReadOnly = true;
            column7.Width = ColDefaultWidth;
            column7.SortMode = DataGridViewColumnSortMode.NotSortable;
            dbfDataGridView_ComplexScreenInfo.Columns.Add(column7);
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

            _curScanBoardIndex = -1;
            _totalFanSwitchCount = 0;
            _totalPowerSwitchCount = 0;
            //_scanCntOfInfoValid = 0;
            //_curMinTempValue = 0;
            //_curMaxTempValue = 0;
            //_curMinHumidityValue = 0;
            //_curMaxHumidityValue = 0;
            //_curTotalTemp = 0;
            //_curTotalHumidity = 0;
            //_curScreenTempList.Clear();
            //_curScreenHumidityList.Clear();

            _curTempMaxMinIndexInfo.MaxSBIndexList.Clear();
            _curTempMaxMinIndexInfo.MinSBIndexList.Clear();

            _curHumiMaxMinIndexInfo.MaxSBIndexList.Clear();
            _curHumiMaxMinIndexInfo.MinSBIndexList.Clear();
            #endregion
        }
        /// <summary>
        /// ˢ�����н��տ���Ӧ������ʾ
        /// </summary>
        private void UpdateAllScanBoardInfo()
        {
            Color backColor = Color.Gray;
            int nValue = 0;
            string displayStr = "";
            ValueInfo valueInfo;
            bool hasErrFanOrPower = false;
            string monitorDataKey = "";
            ScannerMonitorData monitorData = null;
            ScanBoardRowLineStatus rowLineData = null;
            List<bool> generalStatus = null;
            for (int i = 0; i < _curAllScanBoardMonitorInfList.Count; i++)
            {
                if (_curAllScanBoardMonitorInfList[i] != null)
                {
                    hasErrFanOrPower = false;
                    monitorDataKey = _curAllScanBoardMonitorInfList[i].SBRectKey;
                    backColor = Color.Gray;
                    displayStr = "";
                    if (_curAllScanBoardMonitorInfList[i].MonitorData != null)
                    {
                        monitorData = (ScannerMonitorData)_curAllScanBoardMonitorInfList[i].MonitorData.Clone();
                    }
                    else
                    {
                        monitorData = null;
                    }
                    if (_curAllScanBoardMonitorInfList[i].RowLineStatus != null)
                    {
                        rowLineData = (ScanBoardRowLineStatus)_curAllScanBoardMonitorInfList[i].RowLineStatus.Clone();
                    }
                    else
                    {
                        rowLineData = null;
                    }
                    generalStatus = _curAllScanBoardMonitorInfList[i].GeneralSwitchList;
                    switch (_curType)
                    {
                        #region ��ȡ������ɫ����ʾ���ַ���
                        case MonitorDisplayType.SBStatus:
                            GetMonitorColorAndValue.GetSBStatusColorAndStr(monitorData, ref backColor);
                            break;
                        case MonitorDisplayType.MCStatus:
                            GetMonitorColorAndValue.GetMCStatusColorAndStr(monitorData, ref backColor);
                            break;
                        case MonitorDisplayType.Smoke:
                            GetMonitorColorAndValue.GetSmokeColorAndStr(monitorData, ref backColor);
                            break;
                        case MonitorDisplayType.Temperature:
                            #region Temperature
                            if (GetMonitorColorAndValue.DetectTempIsValidAndGetInfo(_clrGradePartition, _curMonitorConfigInfo.TempDisplayType, 
                                                                                    monitorData, ref nValue, ref backColor))
                            {
                                displayStr = nValue.ToString();
                            }
                            //else
                            //{
                            //    displayStr = CommonStr.StatusNoticeStr[(int)CommonStr.NoticeType.Unkown];
                            //}
                            #endregion
                            break;
                        case MonitorDisplayType.Humidity:
                            #region Humidity
                            if (GetMonitorColorAndValue.DetectHumiValidAndGetInfo(_clrGradePartition, monitorData, ref nValue, ref backColor))
                            {
                                displayStr = nValue.ToString();
                            }
                            //else
                            //{
                            //    displayStr = CommonStr.StatusNoticeStr[(int)CommonStr.NoticeType.Unkown];
                            //}
                            #endregion
                            break;
                        case MonitorDisplayType.Fan:
                            #region ����
                            if (monitorData == null)
                            {
                                break;
                            }
                            _curMCFanCnt = GetMonitorColorAndValue.GetMonitorFanCnt(_curMonitorConfigInfo.MCFanInfo, monitorDataKey);
                            if (_curMCFanCnt == 0)
                            {
                                break;
                            }
                            for (int j = 0; j < _curMCFanCnt; j++)
                            {
                                if (j > 0)
                                {
                                    displayStr += "-";
                                }
                                valueInfo = new ValueInfo();
                                if (monitorData.FanSpeedOfMonitorCardCollection == null
                                    || j >= monitorData.FanSpeedOfMonitorCardCollection.Count)
                                {
                                    valueInfo.IsValid = false;
                                }
                                else
                                {
                                    valueInfo = monitorData.FanSpeedOfMonitorCardCollection[j];
                                }
                                GetFanOneSwitchInfo(valueInfo, ref hasErrFanOrPower, ref backColor, ref displayStr);
                            }
                            #endregion
                            break;
                        case MonitorDisplayType.Power:
                            #region ��Դ
                            if (monitorData == null)
                            {
                                break;
                            }
                            _curMCPowerCnt = GetMonitorColorAndValue.GetMonitorPowerCnt(_curMonitorConfigInfo.MCPowerInfo, monitorDataKey);
                            if (_curMCPowerCnt == 0)
                            {
                                break;
                            }
                            int powerIndex = 0;
                            for (int j = 0; j < _curMCPowerCnt; j++)
                            {
                                if (j > 0)
                                {
                                    displayStr += "-";
                                }
                                powerIndex = j + 1;
                                //��ؿ���ѹ
                                valueInfo = new ValueInfo();
                                if (monitorData.VoltageOfMonitorCardCollection == null
                                    || powerIndex >= monitorData.VoltageOfMonitorCardCollection.Count)
                                {
                                    valueInfo.IsValid = false;
                                }
                                else
                                {
                                    valueInfo = monitorData.VoltageOfMonitorCardCollection[powerIndex];
                                }
                                GetPowerOneSwitchInfo(valueInfo, ref hasErrFanOrPower, ref backColor, ref displayStr);
                            }

                            #endregion
                            break;
                        case MonitorDisplayType.RowLine:
                            GetMonitorColorAndValue.GetRowLineCorAndStr(monitorData, rowLineData, ref backColor);
                            break;
                        case MonitorDisplayType.GeneralSwitch:
                            GetMonitorColorAndValue.GetGeneralSwitchClr(monitorData, generalStatus, ref backColor);
                            break;
                        #endregion
                    }
                    UpdateOneScanBoardInfo(i, backColor, displayStr, _curAllScanBoardMonitorInfList[i].ScanBordInfo);
                }
            }
        }
        /// <summary>
        /// ����һ�����տ��е���ʾ�ַ����ͱ�����ɫ
        /// </summary>
        /// <param name="index"></param>
        /// <param name="backClr"></param>
        /// <param name="displayStr"></param>
        /// <param name="ScanBordInfo"></param>
        private void UpdateOneScanBoardInfo(int index, Color backClr, string displayStr, ScanBoardRegionInfo ScanBordInfo)
        {
            dbfDataGridView_ComplexScreenInfo[0, index].Style.BackColor = backClr;
            dbfDataGridView_ComplexScreenInfo[0, index].Style.SelectionBackColor = backClr;
            dbfDataGridView_ComplexScreenInfo.Rows[index].SetValues(new object[] 
                                                                              { 
                                                                              displayStr,
                                                                              (ScanBordInfo.SenderIndex + 1),
                                                                              (ScanBordInfo.PortIndex+ 1),
                                                                              (ScanBordInfo.ConnectIndex+ 1),
                                                                              ScanBordInfo.X,
                                                                              ScanBordInfo.Y,
                                                                              ScanBordInfo.Width,
                                                                              ScanBordInfo.Height
                                                                              });
        }
        /// <summary>
        /// ��ȡ���Ⱥ͵�Դÿһ·��ֵ���ַ���
        /// </summary>
        /// <param name="valueInfo"></param>
        /// <param name="hasErrFanOrPower"></param>
        /// <param name="backClr"></param>
        /// <param name="strObj"></param>
        private void GetFanOneSwitchInfo(ValueInfo valueInfo, ref bool hasErrFanOrPower, ref Color backClr, ref string diplaystr)
        {
            if (valueInfo.IsValid)
            {
                if (valueInfo.Value < _curMonitorConfigInfo.MCFanInfo.AlarmThreshold)
                {
                    hasErrFanOrPower = true;
                    backClr = Color.Yellow;
                }
                else
                {
                    if (!hasErrFanOrPower)
                    {
                        backClr = Color.Green;
                    }
                }
                diplaystr += valueInfo.Value.ToString();
            }
            else
            {
                diplaystr += CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown];
                if (!hasErrFanOrPower)
                {
                    hasErrFanOrPower = true;
                    backClr = Color.Gray;
                }
            }
        }
        /// <summary>
        /// ��ȡ��Դÿһ·����Ϣ
        /// </summary>
        /// <param name="valueInfo"></param>
        /// <param name="hasErrFanOrPower"></param>
        /// <param name="backClr"></param>
        /// <param name="strObj"></param>
        private void GetPowerOneSwitchInfo(ValueInfo valueInfo, ref bool hasErrFanOrPower, ref Color backClr, ref string diplaystr)
        {
            if (valueInfo.IsValid)
            {
                if (valueInfo.Value < _curMonitorConfigInfo.MCPowerInfo.FaultThreshold)
                {
                    hasErrFanOrPower = true;
                    backClr = Color.Red;
                }
                else if (valueInfo.Value < _curMonitorConfigInfo.MCPowerInfo.AlarmThreshold)
                {
                    hasErrFanOrPower = true;
                    backClr = Color.Yellow;
                }
                else
                {
                    if (!hasErrFanOrPower)
                    {
                        backClr = Color.Green;
                    }
                }
                diplaystr += valueInfo.Value.ToString();
            }
            else
            {
                diplaystr += CommonStaticValue.StatusNoticeStr[(int)CommonStaticValue.NoticeType.Unkown];
                if (!hasErrFanOrPower)
                {
                    hasErrFanOrPower = true;
                    backClr = Color.Gray;
                }
            }
        }

        public void UpdateLanguage()
        {
            //ˢ��DataGridView����ͷ
            dbfDataGridView_ComplexScreenInfo.Columns[0].HeaderText = CommonStaticValue.DisplayTypeStr[(int)_curType];
            dbfDataGridView_ComplexScreenInfo.Columns[1].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SenderIndex];
            dbfDataGridView_ComplexScreenInfo.Columns[2].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.PortIndex];
            dbfDataGridView_ComplexScreenInfo.Columns[3].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.ScanBordIndex];
            dbfDataGridView_ComplexScreenInfo.Columns[4].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.StartX];
            dbfDataGridView_ComplexScreenInfo.Columns[5].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.StartY]; ;
            dbfDataGridView_ComplexScreenInfo.Columns[6].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SBWidth]; ;
            dbfDataGridView_ComplexScreenInfo.Columns[7].HeaderText = CommonStaticValue.ColsHeaderText[(int)CommonStaticValue.ColsHeaderType.SBHeight];
        }

        private void UC_ComplexScreenLayout_VisibleChanged(object sender, EventArgs e)
        {
            UpdateLanguage();
        }

        private void dbfDataGridView_ComplexScreenInfo_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_customToolTip != null)
            {
                _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
            }
        }

        private void dbfDataGridView_ComplexScreenInfo_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void dbfDataGridView_ComplexScreenInfo_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_customToolTip == null)
            {
                _customToolTip = new CustomToolTip();
                _customToolTip.Owner = this.ParentForm;
                _customToolTip.TipContentFont = this.Font;
            }
            if (e.ColumnIndex < 0 || e.ColumnIndex >= dbfDataGridView_ComplexScreenInfo.ColumnCount)
            {
                _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
                return;
            }
            if (e.RowIndex < 0 || e.RowIndex >= dbfDataGridView_ComplexScreenInfo.RowCount)
            {
                _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
                return;
            }

            int rowIndex = e.RowIndex;
            if (e.ColumnIndex == 0)
            {
                List<string> noticeStrList = null;
                if (rowIndex >= _curAllScanBoardMonitorInfList.Count)
                {
                    _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
                    return;
                }
                SBInfoAndMonitorInfo scanBoardMonitorInfo = _curAllScanBoardMonitorInfList[rowIndex];
                string fanOrPowerCntKey = scanBoardMonitorInfo.SBRectKey;
                if (scanBoardMonitorInfo != null)
                {
                    if (scanBoardMonitorInfo.ScanBordInfo != null)
                    {
                        noticeStrList = new List<string>();
                        noticeStrList.Add(scanBoardMonitorInfo.CommPortName);//+ "-"+ CommonStaticValue.CommPortScreen+ (scanBoardMonitorInfo.ScreenIndex + 1)
                        if (scanBoardMonitorInfo.MonitorData != null)
                        {
                            switch (_curType)
                            {
                                #region ��ȡ��Ҫ��ʾ�����ͺ�ֵ�ַ���
                                case MonitorDisplayType.SBStatus:
                                    #region ���տ�״̬
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetSBStatusNoticeStr(scanBoardMonitorInfo.MonitorData));
                                    #endregion
                                    break;
                                case MonitorDisplayType.MCStatus:
                                    #region ��ؿ�״̬
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
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetFanNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                   _curMonitorConfigInfo.MCFanInfo,
                                                                                                   fanOrPowerCntKey));
                                    #endregion
                                    break;
                                case MonitorDisplayType.Power:
                                    #region ��Դ
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetPowerNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                     _curMonitorConfigInfo.MCPowerInfo,
                                                                                                     _curMonitorConfigInfo.IsDisplayScanBoardVolt,
                                                                                                     fanOrPowerCntKey));
                                    #endregion
                                    break;
                                case MonitorDisplayType.RowLine:
                                    #region ����
                                    noticeStrList.AddRange(GetMonitorColorAndValue.GetRowLineNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                       scanBoardMonitorInfo.RowLineStatus));
                                    break;
                                    #endregion
                                case MonitorDisplayType.GeneralSwitch:
                                    #region ͨ�ÿ���״̬
                                    
                                    noticeStrList.Add(GetMonitorColorAndValue.GetGeneralSwitchNoticeStr(scanBoardMonitorInfo.MonitorData,
                                                                                                  scanBoardMonitorInfo.GeneralSwitchList));
                                    break;
                                    #endregion
                                default:
                                    _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
                                    return;
                                #endregion
                            }
                        }
                    }
                    _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, noticeStrList);
                    if (!_customToolTip.TopLevel)
                    {
                        _customToolTip.TopLevel = true;
                    }
                }
                else
                {
                    _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
                }
            }
            else
            {
                _customToolTip.SetTipInfo(dbfDataGridView_ComplexScreenInfo, null);
            }
        }

        private void dbfDataGridView_ComplexScreenInfo_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (_curHightLightType == ClickLinkLabelType.None)
            {
                //��ǰ��Ҫ����������Ϊ��
                return;
            }
            using (Pen bordPen = new Pen(HightLightScanPaintInfo.BoardColor, HightLightScanPaintInfo.BoardWidth))
            {
                if (_curHightLightType == ClickLinkLabelType.MaxValue)
                {
                    //��ǰ���ֵ����
                    if (_curType == MonitorDisplayType.Temperature)
                    {
                        //�¶�
                        if (_curTempMaxMinIndexInfo != null
                            && _curTempMaxMinIndexInfo.MaxSBIndexList != null)
                        {
                            if(_curTempMaxMinIndexInfo.MaxSBIndexList.Contains(e.RowIndex))
                            {
                                //�¶����ֵ����
                                e.Graphics.DrawRectangle(bordPen, e.RowBounds);
                            }
                        }
                    }
                    else if (_curType == MonitorDisplayType.Humidity)
                    {
                        //ʪ��
                        if (_curHumiMaxMinIndexInfo != null
                            && _curHumiMaxMinIndexInfo.MaxSBIndexList != null)
                        {
                            if (_curHumiMaxMinIndexInfo.MaxSBIndexList.Contains(e.RowIndex))
                            {
                                //ʪ�����ֵ����
                                e.Graphics.DrawRectangle(bordPen, e.RowBounds);
                            }
                        }
                    }
                }
                else
                {
                    //��ǰ��Сֵ����
                    if (_curType == MonitorDisplayType.Temperature)
                    {
                        //�¶�
                        if (_curTempMaxMinIndexInfo != null
                            && _curTempMaxMinIndexInfo.MinSBIndexList != null)
                        {
                            if (_curTempMaxMinIndexInfo.MinSBIndexList.Contains(e.RowIndex))
                            {
                                //�¶���Сֵ����
                                e.Graphics.DrawRectangle(bordPen, e.RowBounds);
                            }
                        }
                    }
                    else if (_curType == MonitorDisplayType.Humidity)
                    {
                        //ʪ��
                        if (_curHumiMaxMinIndexInfo != null
                            && _curHumiMaxMinIndexInfo.MinSBIndexList != null)
                        {
                            if (_curHumiMaxMinIndexInfo.MinSBIndexList.Contains(e.RowIndex))
                            {                 
                                //ʪ����Сֵ����
                                e.Graphics.DrawRectangle(bordPen, e.RowBounds);
                            }
                        }
                    }
                }
            }
        }
    }
}
