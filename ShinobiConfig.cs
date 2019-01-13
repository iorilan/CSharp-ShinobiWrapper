using System;
using System.Configuration;
using log4net;

namespace Sample.ShinobiVideo.Lib
{
   public static class ShinobiConfig
   {
	   private static ILog _log = LogManager.GetLogger(typeof(ShinobiConfig));

       public static string ShinobiBaseUrl = ConfigurationManager.AppSettings["ShinobiBaseUrl"];
       public static string ShinobiApiKey = ConfigurationManager.AppSettings["ShinobiApiKey"];
       public static string ShinobiGroupKey = ConfigurationManager.AppSettings["ShinobiGroupKey"];
       public static string ShinobiTemplateRtsp = ConfigurationManager.AppSettings["TemplateUrl_Rtsp"];
       public static string ShinobiTemplateHttp = ConfigurationManager.AppSettings["TemplateUrl_Http"];
       public static int MaxIdleSecond = int.Parse(ConfigurationManager.AppSettings["MaxIdleSecond"]);
	   private static string FirstFootageAfterSec = ConfigurationManager.AppSettings["FirstFootageAfterSecond"];
	   private static string VideoStayMin = ConfigurationManager.AppSettings["VideoRecordKeepInMinutes"];
	   private static string SecondFootageAfterSec = ConfigurationManager.AppSettings["SecondFootageAfterSecond"];
	   public static string DashboardMediaFolder = ConfigurationManager.AppSettings["DashboardMediaFolder"];
	   public static string DeltaWindowInSec = ConfigurationManager.AppSettings["DeltaWindowInSec"];



	   public static string[] RecordingArr()
	   {
		   var midList = ConfigurationManager.AppSettings["RecordingList"];
		   var midArr = midList.Split(',');
		   return midArr;
	   }
	   public static DateTime FirstFootageAfterThisTime()
	   {
		   _log.Info($"[Config FirstFootageAfterSec]: {FirstFootageAfterSec}");
		   return DateTime.Now.AddSeconds(int.Parse(FirstFootageAfterSec));
	   }

	   public static DateTime VideoStayMinutes()
	   {
		   return DateTime.Now.AddMinutes(-int.Parse(VideoStayMin));
	   }

	   public static DateTime SecondVideoBeforeThisTime()
	   {
		   return DateTime.Now.AddSeconds(int.Parse(SecondFootageAfterSec));
	   }

	   public static int DeltaWindow
	   {
		   get { return int.Parse(DeltaWindowInSec); }
	   }
   }
}
