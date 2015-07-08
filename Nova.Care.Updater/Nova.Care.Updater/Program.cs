using Newtonsoft.Json;
using Nova.Algorithms.CheckCodes;
using Nova.Care.Updater.Common;
using Nova.Database.DataBaseManager;
using Nova.Zip;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nova.Care.Updater
{
    class Program
    {
        private static readonly int RETRY_TIMES = 3;
        private static readonly string UPATE_DETAILS_FILE_NAME = "CheckUpdateList.db";
        private static readonly string BACKUP_FOLDER_NAME = "Backup";
        private static readonly string CURRENT_PATH = Environment.CurrentDirectory;
        private static readonly string UPDATE_PACKAGE_PATH = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Update_Package");
        //private static readonly string UPDATE_PACKAGE_FILE_PATH = Path.Combine(UPDATE_PACKAGE_PATH, "UpdatePackage.zip");
        private static readonly string UNZIP_UPDATE_PACKAGE_PATH = Path.Combine(Environment.GetEnvironmentVariable("TEMP"));
        private static readonly string UPATE_DETAILS_FILE_PATH = Path.Combine(UNZIP_UPDATE_PACKAGE_PATH, UPATE_DETAILS_FILE_NAME);
        private static readonly string BACKUP_SOFTWARE_PATH = Path.Combine(UPDATE_PACKAGE_PATH, BACKUP_FOLDER_NAME);

        private static string LCT_PROCCESS_ID = "NovaLCT-Mars";
        private static string M3_PROCCESS_ID = "MarsServerProvider";
        private static string MONITOR_PROCCESS_ID = "NovaMonitorManager";

        private static string UPDATE_STATE_FAILURE = "5";
        private static string UPDATE_STATE_SUCCESS = "4";

        private static readonly string SQL_GET_UPDATE_DETAILS = "select dictype,sourcedic,targerdic from checklist";

        private static string APPLICATION_PATH = AppDomain.CurrentDomain.BaseDirectory + @"..\MonitorManager\NovaMonitorManager.exe";

        //private static string _downloadLink = string.Empty;
        //private static string _checkDigit = string.Empty;
        //private static string _applicationPath = string.Empty;

        private static string _mac = string.Empty;
        private static string _serverAddress = string.Empty;

        private static List<VersionUpdateInfo> _systemVersionUpdateInfos = new List<VersionUpdateInfo>();
        private static List<UpdateDetails> _updateDetailsList = new List<UpdateDetails>();

        static void Main(string[] args)
        {
            //args = new string[2];
            //args[0] = "[{\"MAC\":\"000C298C94F3\",\"PROJECT\":\"3\",\"VERSION\":\"1.0.1506.30001\",\"MD5\":\"d36198aa75C2fdedefecc592684a840a\",\"URL\":\"http://nova-update.b0.upaiyun.com/demo/c6a8e734041309efb1620a01f9e4c822.zip\"},{\"MAC\":\"000C298C94F3\",\"PROJECT\":\"1\",\"VERSION\":\"1.0.1506.30001\",\"MD5\":\"d36198aa75C2fdedefecc592684a840a\",\"URL\":\"http://nova-update.b0.upaiyun.com/demo/c6a8e734041309efb1620a01f9e4c822.zip\"},{\"MAC\":\"000C298C94F3\",\"PROJECT\":\"2\",\"VERSION\":\"1.0.1506.30001\",\"MD5\":\"d36198aa75C2fdedefecc592684a840a\",\"URL\":\"http://nova-update.b0.upaiyun.com/demo/c6a8e734041309efb1620a01f9e4c822.zip\"}]";//"[{"VERSION":"1.0.1506.30001","MD5":"d36198aa75C2fdedefecc592684a840a","URL":"http://nova-update.b0.upaiyun.com/demo/c6a8e734041309efb1620a01f9e4c822.zip","PROJECT":1,"MAC":"000C298C94F3"}]";
            //args[1] = "t.novaicare.com";
            // args[1] = "807643A9052A1F450B7969A8EBB1C0C8";
            //args[2] = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "Test.exe");
            //args[2] = Path.Combine(par, "Nova.Monitoring.UI.MonitorMangager.exe");

            int successUpdateCount = 0;
            if (args == null && args.Length != 2)
            {
                return;
            }

            _systemVersionUpdateInfos = JsonConvert.DeserializeObject<List<VersionUpdateInfo>>(args[0]);
            if (_systemVersionUpdateInfos == null || _systemVersionUpdateInfos.Count == 0)
                return;
            _serverAddress = args[1];

            _mac = _systemVersionUpdateInfos[0].Mac;
            RestFulClient.Instance.Initialize(_serverAddress);

            InitialUpdatePath();
            var lctInfo = _systemVersionUpdateInfos.Find(i => i.Project == "2");
            if (lctInfo != null)
            {
                _systemVersionUpdateInfos.Remove(lctInfo);
                _systemVersionUpdateInfos.Insert(0, lctInfo);
            }
            foreach (var updateInfo in _systemVersionUpdateInfos)
            {

                if (FetchUpdatePackage(updateInfo))
                {
                    List<Process> relatedProcesses;
                    System.Console.WriteLine("Staring check is software running");
                    do
                    {
                        relatedProcesses = GetRelatedProcessesBy();
                        foreach (var item in relatedProcesses)
                        {
                            if (!item.HasExited)
                            {
                                System.Console.WriteLine("Software is running,kill it");
                                item.Kill();
                            }
                        }
                        foreach (var item in relatedProcesses)
                        {
                            if (!item.HasExited)
                            {
                                Thread.Sleep(2000);
                            }
                        }
                    } while (IsSoftWareRunning());

                    if (!IsSoftWareRunning())
                    {
                        if (BackupSoftwarePackage())
                        {
                            if (ApplyUpdatePackage(updateInfo))
                            {
                                successUpdateCount++;
                            }
                            else
                            {
                                RollbackSoftwarePackage();
                                PostUpdateStateToCare(UPDATE_STATE_FAILURE);
                                break;
                            }
                        }
                        else
                        {
                            PostUpdateStateToCare(UPDATE_STATE_FAILURE);
                            break;
                        }
                    }
                    else
                    {
                        PostUpdateStateToCare(UPDATE_STATE_FAILURE);
                        break;
                    }
                }
                else
                {
                    PostUpdateStateToCare(UPDATE_STATE_FAILURE);
                    break;
                }
            }

            if (successUpdateCount == _systemVersionUpdateInfos.Count)
            {
                try
                {
                    Process.Start(APPLICATION_PATH);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("StartOldSoftWare:Move directory error," + ex.ToString());
                }
                PostUpdateStateToCare(UPDATE_STATE_SUCCESS);
            }

        }

        private static void InitialUpdatePath()
        {
            if (!Directory.Exists(UPDATE_PACKAGE_PATH))
            {
                Directory.CreateDirectory(UPDATE_PACKAGE_PATH);
            }

        }

        private static bool RollbackSoftwarePackage()
        {
            foreach (var item in _updateDetailsList)
            {
                if (item.Type == TargetType.File)
                {
                    if (File.Exists(Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget)))
                    {
                        IOHelper.CopyFile(Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget), Path.Combine(CURRENT_PATH, item.UpdateTarget));
                    }
                    else
                    { }
                }
                else if (item.Type == TargetType.Folder)
                {
                    if (Directory.Exists(Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget)))
                    {
                        IOHelper.CopyFolder(Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget), Path.Combine(CURRENT_PATH, item.UpdateTarget));
                    }
                    else { }
                    
                }
            }

            //try
            //{
            //    Process.Start(APPLICATION_PATH);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine("StartOldSoftWare:Move directory error," + ex.ToString());
            //    return false;
            //}
            return true;


        }

        private static bool ApplyUpdatePackage(VersionUpdateInfo updateInfo)
        {
            try
            {
                foreach (var item in _updateDetailsList)
                {
                    if (item.Type == TargetType.File)
                    {
                        IOHelper.CopyFile(Path.Combine(Path.Combine(UNZIP_UPDATE_PACKAGE_PATH, updateInfo.MD5), item.UpdateSource), Path.Combine(CURRENT_PATH, item.UpdateTarget));
                    }
                    else if (item.Type == TargetType.Folder)
                    {
                        IOHelper.CopyFolder(Path.Combine(Path.Combine(UNZIP_UPDATE_PACKAGE_PATH, updateInfo.MD5), item.UpdateSource), Path.Combine(CURRENT_PATH, item.UpdateTarget));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("StartOldSoftWare:Move directory error," + ex.ToString());
                return false;
            }
            return true;
        }

        private static bool BackupSoftwarePackage()
        {
            try
            {
                foreach (var item in _updateDetailsList)
                {
                    if (item.Type == TargetType.File)
                    {
                        if (File.Exists(Path.Combine(CURRENT_PATH, item.UpdateTarget)))
                        {
                            IOHelper.CopyFile(Path.Combine(CURRENT_PATH, item.UpdateTarget), Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget));
                        }
                        else
                        {
                        }
                    }
                    else if (item.Type == TargetType.Folder)
                    {
                        if (Directory.Exists(Path.Combine(CURRENT_PATH, item.UpdateTarget)))
                        {
                            IOHelper.CopyFolder(Path.Combine(CURRENT_PATH, item.UpdateTarget), Path.Combine(BACKUP_SOFTWARE_PATH, item.UpdateTarget));
                        }
                        else
                        {

                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static bool FetchUpdatePackage(VersionUpdateInfo updateInfo)
        {
            string updatePackageLocalFile = Path.Combine(UPDATE_PACKAGE_PATH, string.Format("{0}.zip", updateInfo.MD5));
            bool downloadResult = false;
            bool resolveResult = false;
            int downloadTimes = 0;
            do
            {
                downloadResult = DownloadUpdatePackage(updateInfo.Url, updatePackageLocalFile);
                if (downloadResult)
                {
                    resolveResult = ResolveUpdatePackage(updateInfo, updatePackageLocalFile);
                    if (resolveResult)
                    {
                        break;
                    }
                }
                downloadTimes++;
            } while (downloadTimes < RETRY_TIMES);

            if (downloadResult && resolveResult)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void PostUpdateStateToCare(string updateState)
        {
            var commandResult = new CommandResult() { mac = _mac, commandType = 40, commandResult = updateState };
            RestFulClient.Instance.Post("Api/Cmd/cmd_result", commandResult);
        }

        private static bool ResolveUpdatePackage(VersionUpdateInfo updateInfo, string updatePackageLocalFile)
        {
            bool isMatch;
            MD5Helper.FileMD5ErrorMode errCode;
            isMatch = CheckUpdatePackageMD5(updatePackageLocalFile, updateInfo.MD5, out errCode);
            if (errCode != MD5Helper.FileMD5ErrorMode.OK || !isMatch)
            {
                return false;
            }
            string upzipUpdatePackagePath = Path.Combine(UNZIP_UPDATE_PACKAGE_PATH, updateInfo.MD5);
            if (!UnZipUpdatePackage(updatePackageLocalFile, upzipUpdatePackagePath))
            {
                return false;
            }

            if (GetUpdateDetailsList(Path.Combine(upzipUpdatePackagePath, UPATE_DETAILS_FILE_NAME)))
                return true;
            else
                return false;
        }

        private static bool GetUpdateDetailsList(string detailsFilePath)
        {
            try
            {
                var dbReader = new DbSqLiteHelper(detailsFilePath);
                dbReader.ConnectionInit();
                var dt = dbReader.ExecuteDataTable(SQL_GET_UPDATE_DETAILS);
                if (dt == null || dt.Rows.Count == 0)
                {
                    return false;
                }
                for (int index = 0; index < dt.Rows.Count; index++)
                {
                    var details = new UpdateDetails();
                    details.Type = (TargetType)dt.Rows[index][0];
                    details.UpdateSource = (string)dt.Rows[index][1];
                    details.UpdateTarget = (string)dt.Rows[index][2];
                    _updateDetailsList.Add(details);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        private static bool UnZipUpdatePackage(string sourceFilePath, string destFileDir)
        {
            ZipFile zip = new ZipFile();
            if (!zip.UnZipFiles(sourceFilePath, destFileDir, OperateType.Direct, true))
            {
                System.Console.WriteLine("Unzip the files failed");
                try
                {
                    if (Directory.Exists(destFileDir))
                    {
                        Directory.Delete(destFileDir, true);
                    }
                }
                catch { }
                return false;
            }

            System.Console.WriteLine("Unzip the files success");
            return true;
        }

        private static bool CheckUpdatePackageMD5(string filePath, string md5Code, out MD5Helper.FileMD5ErrorMode errCode)
        {
            string currentMD5Code;
            errCode = MD5Helper.CreateMD5(filePath, out currentMD5Code);
            if (errCode != MD5Helper.FileMD5ErrorMode.OK)
            {
                System.Console.WriteLine("Calculation file md5 error,Err:" + errCode.ToString());
                try
                {
                    File.Delete(filePath);
                }
                catch { }
                return false;
            }
            if (md5Code.ToLower() != currentMD5Code.ToLower())
            {
                System.Console.WriteLine("File md5 is not match!");
                try
                {
                    File.Delete(filePath);
                }
                catch { }
                return false;
            }
            else return true;
        }

        private static bool DownloadUpdatePackage(string downloadLink, string localFilePath)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            try
            {
                webClient.DownloadFile(downloadLink, localFilePath);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("下载链接为NULL");
                return false;
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine("下载数据时发生错误");
                return false;
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine("该方法已在多个线程上同时使用");
                return false;
            }
            return true;
        }

        private static bool CheckUpdatePackage(string checkDigit)
        {
            bool isMatch;
            MD5Helper.FileMD5ErrorMode errCode;
            isMatch = CheckUpdatePackageMD5(UPDATE_PACKAGE_PATH, checkDigit, out errCode);
            if (errCode != MD5Helper.FileMD5ErrorMode.OK)
            {
                return false;
            }
            return isMatch;
        }

        private static bool IsSoftWareRunning()
        {
            return IsLCTRunning() && IsMonitorRunning() && IsM3Running();
        }

        private static bool IsLCTRunning()
        {
            Process[] pList = Process.GetProcesses();
            return pList.FirstOrDefault(p => p.ProcessName == LCT_PROCCESS_ID) == null ? false : true;
        }

        private static bool IsM3Running()
        {

            Process[] pList = Process.GetProcesses();
            return pList.FirstOrDefault(p => p.ProcessName == M3_PROCCESS_ID) == null ? false : true;
        }

        private static bool IsMonitorRunning()
        {

            Process[] pList = Process.GetProcesses();
            return pList.FirstOrDefault(p => p.ProcessName == MONITOR_PROCCESS_ID) == null ? false : true;
        }
        private static List<Process> GetRelatedProcessesBy()
        {
            List<Process> proList;
            Process[] pList = Process.GetProcesses();
            proList = new List<Process>();
            foreach (var item in pList)
            {
                if (LCT_PROCCESS_ID == item.ProcessName || M3_PROCCESS_ID == item.ProcessName || MONITOR_PROCCESS_ID == item.ProcessName)
                {
                    proList.Add(item);
                }
            }
            // proList.Sort(CompareProcessById);
            return proList;
        }

    }

    public class RestFulClient
    {
        private static readonly object locker = new object();
        private static RestFulClient _restFulClient;
        private RestClient _client;
        private RestFulClient()
        {

        }
        public static RestFulClient Instance
        {
            get
            {
                if (_restFulClient == null)
                {
                    lock (locker)
                    {
                        if (_restFulClient == null)
                        {
                            _restFulClient = new RestFulClient();
                        }
                    }
                }
                return _restFulClient;
            }
        }

        //  public bool IsInitialized  { get; set; }

        public void Initialize(string baseUrl)
        {
            _client = new RestClient(string.Format("http://{0}", baseUrl));
            _client.Timeout = 60 * 1000;
        }

        public void Post(string resource, object body, Action<IRestResponse> responseHandler)
        {
            if (_client == null)
                return;

            var request = new RestRequest(resource, Method.POST);
            //request.Timeout = 60;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("EnablingCompression", "1");
            request.AddBody(body);
            _client.ExecuteAsync(request, response =>
            {
                if (responseHandler != null && response != null)
                    responseHandler.BeginInvoke(response, null, null);
            });

        }

        public string Post(string resource, object body)
        {
            if (_client == null)
                throw new TypeInitializationException("RestClient", new Exception());

            var request = new RestRequest(resource, Method.POST);
            // request.Timeout = 60;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(body);
            var response = _client.Execute(request);
            var content = response.Content;
            return content;
        }

    }


    public class CommandResult
    {
        public string mac { get; set; }
        public int commandType { get; set; }
        public string commandResult { get; set; }
    }
}
