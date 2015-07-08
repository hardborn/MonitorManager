using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Nova.Control;
using Nova.Convert;
using Nova.Equipment.Protocol.TGProtocol;
using Nova.LCT.GigabitSystem.Common;

namespace Nova.Monitoring.UI.ScannerStatusDisplay
{
    public partial class UC_StatusRectangleLayout : RectangularGridLayout
    {
        #region ˽���ֶ�
        private byte DrawOffset = 1;
        private Size _proposedSize = Size.Empty;
        //��ǰ��ʾ����
        private MonitorDisplayType _curType;
        //���Ƶ�Դ�����·���ָ��ߵ�Pen
        private Pen _switchPen = new Pen(Color.DeepSkyBlue, 1.0f);
        //���Ƶ�Դÿһ·Բ�߽��Pen
        private Pen _powerBorderPen = new Pen(Color.Black);
        //���Ƶ�Դ�����ÿһ·������Brush
        private SolidBrush _switchBrush = new SolidBrush(Color.Green);
        //������ֵ��ȡ��Ӧ����ɫ
        private ColorGradePartition _clrGradePartition = null;

        private bool _isDisplayAll = false;

        private MonitorConfigData _curMonitorConfigInfo = null;
        private uint _curMCFanCnt = 0;
        private uint _curMCPowerCnt = 0;

        private string _displayStr = "";
        private SizeF _strSize = new SizeF();
        private byte ImageAndStrInterval = 2;
        private byte MinImageSize = 5;
        private Pen _screenBoarderPen = null;
        #endregion

        public UC_StatusRectangleLayout()
        {
            InitializeComponent();

            _proposedSize  = new System.Drawing.Size(int.MaxValue, int.MaxValue);

            _switchPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            //Ĭ�ϱ����ɫ
            this.DefaultStyle.BoardColor = Color.Black;
            //Ĭ�ϱ�����ɫ
            this.DefaultStyle.BackColor = Color.Black;
            this.DefaultStyle.BoardWidth = 1;
            //Ĭ������
            this.DefaultFocusStyle.GridFont = new Font("����", 12, FontStyle.Regular);

            _screenBoarderPen = new Pen(Color.SpringGreen, 2.0f);

            _curMonitorConfigInfo = new MonitorConfigData();
        }

        #region ����
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
        public float HumidityThreshold
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
        public bool IsDisplayScanBoardValtage
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
        /// ��ǰ��ʾģʽ
        /// </summary>
        public MonitorDisplayType CurType
        {
            get { return _curType; }
            set { _curType = value; }
        }
        /// <summary>
        /// �Ƿ���ʾ���м����Ϣ
        /// </summary>
        public bool IsDiplayAllType
        {
            get { return _isDisplayAll; }
            set{  _isDisplayAll = value; }
        }
        /// <summary>
        /// �¶���ʾ����
        /// </summary>
        public TemperatureType TempDisplayType
        {
            get { return _curMonitorConfigInfo.TempDisplayType; }
            set {  _curMonitorConfigInfo.TempDisplayType = value; }
        }
        /// <summary>
        /// ��ȡ��ɫ����
        /// </summary>
        public ColorGradePartition CorGradePartition
        {
            get { return _clrGradePartition; }
            set { _clrGradePartition = value; }
        }
        #endregion

        /// <summary>
        /// ����Ƿ���Ҫ�����ַ���
        /// </summary>
        /// <param name="drawStr"></param>
        /// <param name="offsetX"></param>
        /// <param name="region"></param>
        /// <param name="strSize"></param>
        private void DrawStrForCenter(Graphics drawZoonGraphic, string drawStr,RectangleF drawRegion)
        {
            TextFormatFlags drawFlags = TextFormatFlags.NoPadding 
                                        | TextFormatFlags.HorizontalCenter 
                                        | TextFormatFlags.VerticalCenter;

            Rectangle drawRect = new Rectangle((int)drawRegion.X, (int)drawRegion.Y,
                                               (int)drawRegion.Width, (int)drawRegion.Height);

            TextRenderer.DrawText(drawZoonGraphic, drawStr, this.DefaultFocusStyle.GridFont,
                                  drawRect, Color.Black, drawFlags);
        }
        private void DrawStrForLeft(Graphics drawZoonGraphic, string drawStr, RectangleF drawRegion)
        {
            TextFormatFlags drawFlags = TextFormatFlags.NoPadding
                                       | TextFormatFlags.Left
                                       | TextFormatFlags.VerticalCenter;

            Rectangle drawRect = new Rectangle((int)drawRegion.X, (int)drawRegion.Y,
                                               (int)drawRegion.Width, (int)drawRegion.Height);

            TextRenderer.DrawText(drawZoonGraphic, drawStr, this.DefaultFocusStyle.GridFont,
                                  drawRect, Color.Black, drawFlags);
        }

        private SizeF GetStrSize(Graphics drawZoonGraphic, string drawStr)
        {
            TextFormatFlags measureFlags = TextFormatFlags.NoPadding;
            return TextRenderer.MeasureText(drawZoonGraphic, drawStr, this.DefaultFocusStyle.GridFont, _proposedSize, measureFlags);

        }

        private void GetDrawImage(ValueInfo valueInfo, PicType picType, float threshod, ref Image drawImage)
        {
            if (valueInfo.IsValid)
            {
                if (valueInfo.Value < threshod)
                {
                    //�澯
                    drawImage = FanAndPowerRepaintInfo.AlarmImage[(int)picType];
                    _switchBrush.Color = Color.Yellow;
                }
                else
                {
                    //����
                    drawImage = FanAndPowerRepaintInfo.OKImage[(int)picType];
                    _switchBrush.Color = Color.Green;
                }
            }
            else
            {
                //��Ч
                drawImage = FanAndPowerRepaintInfo.InvalidImage[(int)picType];
                _switchBrush.Color = Color.Gray;
            }
        }

        protected override void DrawGrid(Graphics drawZoonGraphic, RectangularGridInfo curGird)
        {
            if (curGird == null)
            {
                return;
            }
            if (_isDisplayAll)
            {
                return;
            }
            IGridBiningObject customObj = (IGridBiningObject)curGird.CustomObj;
            Color backColor = Color.Gray;
            int nValue = 0;
            bool bTempOrHumiInvalid = false;
            if (customObj.Type == GridType.ScanBoardGird)
            {
                ScanBoardGridBindObj scanBoardGridObj = (ScanBoardGridBindObj)customObj;
                #region ��ȡ���ӱ�����ɫ
                if (customObj != null)
                {
                    switch (_curType)
                    {
                        case MonitorDisplayType.SBStatus:
                            GetMonitorColorAndValue.GetSBStatusColorAndStr(scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                         ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.MCStatus:
                            GetMonitorColorAndValue.GetMCStatusColorAndStr(scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                         ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.Smoke:
                            GetMonitorColorAndValue.GetSmokeColorAndStr(scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                        ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.Temperature:
                            bTempOrHumiInvalid = !GetMonitorColorAndValue.DetectTempIsValidAndGetInfo(_clrGradePartition,
                                                                                          _curMonitorConfigInfo.TempDisplayType,
                                                                                          scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                                          ref nValue, ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.Humidity:
                            bTempOrHumiInvalid = !GetMonitorColorAndValue.DetectHumiValidAndGetInfo(_clrGradePartition,
                                                                                          scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                                          ref nValue, ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.Fan:
                            curGird.Style.BackColor = Color.Gray;
                            break;
                        case MonitorDisplayType.Power:
                            curGird.Style.BackColor = Color.Gray;
                            break;
                        case MonitorDisplayType.RowLine:
                            GetMonitorColorAndValue.GetRowLineCorAndStr(scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                        scanBoardGridObj.ScanBoardAndMonitorInfo.RowLineStatus,
                                                                        ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                        case MonitorDisplayType.GeneralSwitch:
                            GetMonitorColorAndValue.GetGeneralSwitchClr(scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData,
                                                                        scanBoardGridObj.ScanBoardAndMonitorInfo.GeneralSwitchList,
                                                                        ref backColor);
                            curGird.Style.BackColor = backColor;
                            break;
                    }
                }
                #endregion

                base.DrawGrid(drawZoonGraphic, curGird);

                #region ���Ƹ����ϵ��ַ�����ͼ��
                if (scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData == null)
                {
                    return;
                }
                RectangleF drawRect = new RectangleF(curGird.DrawRegion.X + this.DefaultStyle.BoardWidth / 2,
                                                   curGird.DrawRegion.Y + this.DefaultStyle.BoardWidth / 2,
                                                   curGird.DrawRegion.Width - this.DefaultStyle.BoardWidth,
                                                   curGird.DrawRegion.Height - this.DefaultStyle.BoardWidth);
                ScannerMonitorData monitorInfo = (ScannerMonitorData)scanBoardGridObj.ScanBoardAndMonitorInfo.MonitorData.Clone();
                if (customObj != null && monitorInfo != null)
                {
                    if (_curType == MonitorDisplayType.Temperature
                        || _curType == MonitorDisplayType.Humidity)
                    {
                        #region ������ʪ���ַ���
                        if (bTempOrHumiInvalid)
                        {
                            return;
                        }
                        _strSize = GetStrSize(drawZoonGraphic, nValue.ToString());
                        if (_strSize.Width > drawRect.Width || _strSize.Height > drawRect.Height)
                        {
                            //���ܽ��ַ������Ƴ���
                            return;
                        }

                        DrawStrForCenter(drawZoonGraphic, nValue.ToString(), drawRect);
                        #endregion
                    }
                    else if (_curType == MonitorDisplayType.Fan
                        || _curType == MonitorDisplayType.Power)
                    {
                        ValueInfo valueInfo = new ValueInfo();
                        float fSwitchHeight = 0;
                        Image drawImage = null;
                        bool isDrawStr = false;
                        float drawImageHeight = 0;
                        float imageSize = 0;
                        if (_curType == MonitorDisplayType.Fan)
                        {
                            #region ���Ʒ��ȷָ���
                            _curMCFanCnt = GetMonitorColorAndValue.GetMonitorFanCnt(_curMonitorConfigInfo.MCFanInfo, curGird.Key);
                            if (_curMCFanCnt <= 0)
                            {
                                return;
                            }
                            if ((curGird.DrawRegion.Height - DrawOffset * 2) < _curMCFanCnt * _switchPen.Width
                                || _curMCFanCnt <= 0)
                            {
                                //��ȡʵ�ʻ��Ƶķ��ȸ���ʧ��
                                return;
                            }
                            fSwitchHeight = (float)drawRect.Height / _curMCFanCnt;
                            drawImageHeight = fSwitchHeight - ImageAndStrInterval * 2;
                            for (int i = 0; i < _curMCFanCnt; i++)
                            {
                                isDrawStr = true;
                                if (monitorInfo.FanSpeedOfMonitorCardCollection == null
                                    || i >= monitorInfo.FanSpeedOfMonitorCardCollection.Count)
                                {
                                    valueInfo = new ValueInfo();
                                    valueInfo.IsValid = false;
                                }
                                else
                                {
                                    valueInfo = monitorInfo.FanSpeedOfMonitorCardCollection[i];
                                }
                                //��ȡ���ȵ�ͼƬ���ַ������ƴ�С
                                GetImageAndStrSize(drawZoonGraphic,drawRect,
                                                   valueInfo, fSwitchHeight, drawImageHeight,
                                                   out imageSize, out _strSize, out isDrawStr);
                                //��ȡ���Ƶ�ͼƬ
                                GetDrawImage(valueInfo, PicType.Fan,
                                             _curMonitorConfigInfo.MCFanInfo.AlarmThreshold,
                                             ref drawImage);
                                //����ͼƬ���ַ���
                                DrawImageAndStr(drawZoonGraphic, drawImage, valueInfo, drawRect,
                                                fSwitchHeight, imageSize, _strSize, isDrawStr, i);

                                #region ��������
                                #region ��ȡ�ַ�����ͼƬ���ƵĴ�С
                                //if (!valueInfo.IsValid)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //}
                                //else
                                //{
                                //    _strSize = GetStrSize(drawZoonGraphic, valueInfo.Value.ToString());
                                //}
                                //if (_strSize.Height > fSwitchHeight
                                //    || _strSize.Width + ImageAndStrInterval * 3 > drawRect.Width)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //}
                                //imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 3;
                                //imageSize = Math.Min(imageWidth, drawImageHeight);
                                //if (imageSize < MinImageSize)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //    imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 2;
                                //    imageSize = Math.Min(imageWidth, drawImageHeight);
                                //}
                                //if (imageSize < 0)
                                //{
                                //    imageSize = 0;
                                //}
                                #endregion

                                #region �����ַ�����ͼƬ
                                //imageRectF = new RectangleF(fRegionStartX + ImageAndStrInterval,
                                //  fRegionStartY + fSwitchHeight * i + (fSwitchHeight - imageSize) / 2,
                                //  imageSize, imageSize);

                                //lineStartPoint = new PointF(fRegionStartX, fRegionStartY + fSwitchHeight * i);
                                //lineEndPoint = new PointF(fRegionEndX, fRegionStartY + fSwitchHeight * i);

                                ////���Ʊ���
                                //drawZoonGraphic.FillRectangle(_switchBrush, fRegionStartX, fRegionStartY + fSwitchHeight * i,
                                //                              drawRect.Width - DrawOffset, fSwitchHeight);
                                //drawZoonGraphic.DrawImage(drawImage, imageRectF);
                                //if (i > 0)
                                //{
                                //    drawZoonGraphic.DrawLine(_switchPen, lineStartPoint, lineEndPoint);
                                //}
                                //if (isDrawStr)
                                //{
                                //    strRectF = new RectangleF(fRegionStartX + imageSize + ImageAndStrInterval * 2,
                                //                              fRegionStartY + fSwitchHeight * i,
                                //                              drawRect.Width - imageSize - ImageAndStrInterval * 2,
                                //                              fSwitchHeight);
                                //    DrawStrForLeft(drawZoonGraphic, valueInfo.Value.ToString(), strRectF);
                                //}
                                #endregion
                                #endregion
                            }
                            #endregion
                        }
                        else if (_curType == MonitorDisplayType.Power)
                        {
                            #region ���Ƶ�Դ�ָ���
                            _curMCPowerCnt = GetMonitorColorAndValue.GetMonitorPowerCnt(_curMonitorConfigInfo.MCPowerInfo, curGird.Key);
                            if (_curMCPowerCnt <= 0)
                            {
                                return;
                            }
                            if ((drawRect.Height - DrawOffset * 2) < _curMCPowerCnt * _switchPen.Width)
                            {
                                return;
                            }
                            fSwitchHeight = (float)drawRect.Height / _curMCPowerCnt;
                            drawImageHeight = fSwitchHeight - ImageAndStrInterval * 2;
                            int powerIndexInList = 0;
                            for (int i = 0; i < _curMCPowerCnt; i++)
                            {
                                powerIndexInList = i + 1;
                                isDrawStr = true;
                                if (monitorInfo.VoltageOfMonitorCardCollection == null
                                    || powerIndexInList >= monitorInfo.VoltageOfMonitorCardCollection.Count)
                                {
                                    valueInfo = new ValueInfo();
                                    valueInfo.IsValid = false;
                                }
                                else
                                {
                                    valueInfo = monitorInfo.VoltageOfMonitorCardCollection[powerIndexInList];
                                }
                                //��ȡ��Դ��ͼƬ���ַ������ƴ�С
                                GetImageAndStrSize(drawZoonGraphic, drawRect,
                                                   valueInfo, fSwitchHeight, drawImageHeight,
                                                   out imageSize, out _strSize, out isDrawStr);
                                //��ȡ���Ƶ�ͼƬ
                                GetDrawImage(valueInfo, PicType.Power,
                                             _curMonitorConfigInfo.MCPowerInfo.AlarmThreshold,
                                             ref drawImage);
                                //����ͼƬ���ַ���
                                DrawImageAndStr(drawZoonGraphic, drawImage, valueInfo, drawRect,
                                                fSwitchHeight, imageSize, _strSize, isDrawStr, i);

                                #region ��������
                                #region ��ȡ�ַ�����ͼƬ���ƵĴ�С
                                //if (!valueInfo.IsValid)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //}
                                //else
                                //{
                                //    _strSize = GetStrSize(drawZoonGraphic, valueInfo.Value.ToString("#0.00"));
                                //}
                                //if (_strSize.Height > fSwitchHeight
                                //    || _strSize.Width + ImageAndStrInterval * 3 > drawRect.Width)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //}
                                //imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 3;
                                //imageSize = Math.Min(imageWidth, drawImageHeight);
                                //if (imageSize < MinImageSize)
                                //{
                                //    isDrawStr = false;
                                //    _strSize.Width = 0;
                                //    imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 2;
                                //    imageSize = Math.Min(imageWidth, drawImageHeight);
                                //}
                                //if (imageSize < 0)
                                //{
                                //    imageSize = 0;
                                //}
                                #endregion

                                #region �����ַ�����ͼƬ
                                //imageRectF = new RectangleF(fRegionStartX + ImageAndStrInterval,
                                //  fRegionStartY + fSwitchHeight * i + (fSwitchHeight - imageSize) / 2,
                                //  imageSize, imageSize);

                                //lineStartPoint = new PointF(fRegionStartX, fRegionStartY + fSwitchHeight * i);
                                //lineEndPoint = new PointF(fRegionEndX, fRegionStartY + fSwitchHeight * i);

                                //GetDrawImage(valueInfo, PicType.Power,
                                //            _curMonitorConfigInfo.MCPowerInfo.AlarmThreshold,
                                //            ref drawImage);
                                ////���Ʊ���
                                //drawZoonGraphic.FillRectangle(_switchBrush, fRegionStartX, fRegionStartY + fSwitchHeight * i,
                                //                              drawRect.Width - DrawOffset, fSwitchHeight);
                                //drawZoonGraphic.DrawImage(drawImage, imageRectF);
                                //if (i > 0 && i < _curMCPowerCnt)
                                //{
                                //    drawZoonGraphic.DrawLine(_switchPen, lineStartPoint, lineEndPoint);
                                //}
                                //if (isDrawStr)
                                //{
                                //    strRectF = new RectangleF(fRegionStartX + imageSize + ImageAndStrInterval * 2,
                                //                              fRegionStartY + fSwitchHeight * i,
                                //                              drawRect.Width - imageSize - ImageAndStrInterval * 2,
                                //                              fSwitchHeight);
                                //    DrawStrForLeft(drawZoonGraphic, valueInfo.Value.ToString("#0.00"), strRectF);
                                //}
                                #endregion
                                #endregion
                            }
                            #endregion
                        }

                    }
                }
                #endregion
            }
            else if (customObj.Type == GridType.ComplexScreenGrid)
            {
                ComplexScreenGridBindObj complexScreenGridObj = (ComplexScreenGridBindObj)customObj;

                #region ��ȡ���ӱ�����ɫ
                backColor = Color.Gray;
                if (complexScreenGridObj.ComplexScreenLayout.IsAllMonitorDataIsValid)
                {
                    switch (_curType)
                    {
                        case MonitorDisplayType.SBStatus:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SBStatusErrCount > 0
                                || complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SBStatusErrCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.MCStatus:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.MCStatusErrCount > 0
                                || complexScreenGridObj.ComplexScreenLayout.AlarmInfo.MCStatusErrCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.Smoke:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SmokeAlarmCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SmokeAlarmCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.Temperature:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.TemperatureAlarmCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.TemperatureAlarmCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.Humidity:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.HumidityAlarmCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.HumidityAlarmCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.Fan:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.FanAlarmSwitchCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.FanAlarmSwitchCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.Power:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.PowerAlarmSwitchCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.PowerAlarmSwitchCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.RowLine:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.SoketAlarmCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.SoketAlarmCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                        case MonitorDisplayType.GeneralSwitch:
                            if (complexScreenGridObj.ComplexScreenLayout.FaultInfo.GeneralSwitchErrCount > 0)
                            {
                                backColor = Color.Red;
                            }
                            else if (complexScreenGridObj.ComplexScreenLayout.AlarmInfo.GeneralSwitchErrCount > 0)
                            {
                                backColor = Color.Yellow;
                            }
                            else
                            {
                                backColor = Color.Green;
                            }
                            break;
                    }
                }
                curGird.Style.BackColor = backColor;
                #endregion

                base.DrawGrid(drawZoonGraphic, curGird);

                #region ���Ƹ������ı߽�
                //drawZoonGraphic.DrawRectangle(_screenBoarderPen,
                //                              curGird.DrawRegion.X + _screenBoarderPen.Width / 2,
                //                              curGird.DrawRegion.Y + _screenBoarderPen.Width / 2,
                //                              curGird.DrawRegion.Width - _screenBoarderPen.Width,
                //                              curGird.DrawRegion.Height - _screenBoarderPen.Width);
                #endregion
            }
            else if (customObj.Type == GridType.ScreenGrid)
            {
                ScreenGridBindObj screenGridObj = (ScreenGridBindObj)customObj;
                if (!screenGridObj.ScreenIsValid)
                {
                    curGird.Style.BackColor = Color.Gray;
                    base.DrawGrid(drawZoonGraphic, curGird);
                }
                #region ���Ƹ��ӱ߽� --- ��ʾ���߽�
                drawZoonGraphic.DrawRectangle(_screenBoarderPen,
                                              curGird.DrawRegion.X + _screenBoarderPen.Width / 2,
                                              curGird.DrawRegion.Y + _screenBoarderPen.Width / 2,
                                              curGird.DrawRegion.Width - _screenBoarderPen.Width,
                                              curGird.DrawRegion.Height - _screenBoarderPen.Width);
                #endregion
            }
        }

        private void GetImageAndStrSize(Graphics drawZoonGraphic,RectangleF drawRect,
                                        ValueInfo valueInfo, float oneSwitchHeight, 
                                        float oldImageHeight, out float imageSize, 
                                        out SizeF strSize, out bool isDrawStr)
        {
            imageSize = 0;
            strSize = SizeF.Empty;
            isDrawStr = true;
            if (!valueInfo.IsValid)
            {
                isDrawStr = false;
                strSize.Width = 0;
            }
            else
            {
                strSize = GetStrSize(drawZoonGraphic, valueInfo.Value.ToString());
            }
            if (strSize.Height > oneSwitchHeight
                || strSize.Width + ImageAndStrInterval * 3 > drawRect.Width)
            {
                isDrawStr = false;
                strSize.Width = 0;
            }
            float imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 3;
            imageSize = Math.Min(imageWidth, oldImageHeight);
            if (imageSize < MinImageSize)
            {
                isDrawStr = false;
                strSize.Width = 0;
                imageWidth = drawRect.Width - _strSize.Width - ImageAndStrInterval * 2;
                imageSize = Math.Min(imageWidth, oldImageHeight);
            }
            if (imageSize < 0)
            {
                imageSize = 0;
            }
        }

        private void DrawImageAndStr(Graphics drawZoonGraphic, Image drawImage, ValueInfo valueInfo,
                                     RectangleF drawRect, float oneSwitchHeight, float imageSize, 
                                     SizeF strSize, bool isDrawStr, int switchIndex)
        {
            float fRegionStartX = drawRect.X;
            float fRegionStartY = drawRect.Y;
            float fRegionEndX = drawRect.X + drawRect.Width;
            float fRegionEndY = drawRect.Y + drawRect.Height;

            RectangleF imageRectF = new RectangleF(fRegionStartX + ImageAndStrInterval,
                                                   fRegionStartY + oneSwitchHeight * switchIndex + (oneSwitchHeight - imageSize) / 2,
                                                   imageSize, imageSize);

            PointF lineStartPoint = new PointF(fRegionStartX, fRegionStartY + oneSwitchHeight * switchIndex);
            PointF lineEndPoint = new PointF(fRegionEndX, fRegionStartY + oneSwitchHeight * switchIndex);

            //���Ʊ���
            drawZoonGraphic.FillRectangle(_switchBrush, fRegionStartX, fRegionStartY + oneSwitchHeight * switchIndex,
                                          drawRect.Width - DrawOffset, oneSwitchHeight);
            drawZoonGraphic.DrawImage(drawImage, imageRectF);
            if (switchIndex > 0)
            {
                drawZoonGraphic.DrawLine(_switchPen, lineStartPoint, lineEndPoint);
            }
            if (isDrawStr)
            {
                RectangleF strRectF = new RectangleF(fRegionStartX + imageSize + ImageAndStrInterval * 2,
                                                     fRegionStartY + oneSwitchHeight * switchIndex,
                                                     drawRect.Width - imageSize - ImageAndStrInterval * 2,
                                                     oneSwitchHeight);
                DrawStrForLeft(drawZoonGraphic, valueInfo.Value.ToString(), strRectF);
            }
        }

        private void ResourceDispose()
        {
            if (_switchPen != null)
            {
                _switchPen.Dispose();
            }
            if (_powerBorderPen != null)
            {
                _powerBorderPen.Dispose();
            }
            if (_switchBrush != null)
            {
                _switchBrush.Dispose();
            }
            if (_screenBoarderPen != null)
            {
                _screenBoarderPen.Dispose();
            }
        }
    }
}
