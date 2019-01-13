using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using log4net;
using Sample.DataAccess.Tenant.DbModels;
using Sample.ShinobiVideo.Lib.Models;
using Newtonsoft.Json;

namespace Sample.ShinobiVideo.Lib
{
	/// <summary>
	/// A wrapper of shinobi video apis
	/// </summary>
	public static class ShinobiService
	{
		private static ILog _log = LogManager.GetLogger(typeof(ShinobiService));

		public static IList<ShinobiMonitorObj> AllMonitoringObjs()
		{
			try
			{
				using (var client = new HttpClient())
				{
					var urlGetAll =
						$"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/smonitor/{ShinobiConfig.ShinobiGroupKey}";
					var retGetAllStr = client.GetStringAsync(urlGetAll).Result;
					var retGetAll = JsonConvert.DeserializeObject<IList<ShinobiMonitorObj>>(retGetAllStr);
					return retGetAll;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return new List<ShinobiMonitorObj>();
			}

		}
		/*
         * Live view
         */
		public static bool AddCamera(CameraVm camera)
		{
			try
			{
				var url = RequestAddMonitor.Create(camera);
				_log.Info($"add camera url : [{url}]");
				using (var client = new HttpClient())
				{
					var retGetAll = AllMonitoringObjs();

					if (retGetAll.Count > 0)
					{
						var tryMatch = retGetAll.FirstOrDefault(x => x.mid == camera.IpAddress.ToMonitorId());
						if (tryMatch != null)
						{
							return true;
						}
					}


					var retStr = client.GetStringAsync(url).Result;
					var retObj = JsonConvert.DeserializeObject<ShinobiGeneralResponse>(retStr);
					if (!retObj.ok)
					{
						_log.Error($"Add Camera Failed. ip :{camera.IpAddress}. error :{retObj.msg}");
						return false;
					}

					return true;
				}

			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return false;
			}

		}
		public static bool DeleteCamera(string monitorId)
		{
			try
			{
				var url =
					$"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/configureMonitor/{ShinobiConfig.ShinobiGroupKey}/{monitorId}/delete";
				using (var client = new HttpClient())
				{
					var ret = client.GetStringAsync(url).Result;
					var retObj = JsonConvert.DeserializeObject<ShinobiGeneralResponse>(ret);
					if (!retObj.ok)
					{
						_log.Error($"Delete Camera Failed. monitorId :{monitorId}. error :{retObj.msg}");
						return false;
					}

					return true;
				}
			}
			catch (Exception ex)
			{
				return false;
			}
		}



		/*
         * Snapshot & Video clip
         */
		public static ShinobiResponseVideoFileVm GetVideoFilesBeforeWhen(string ip, DateTime datetime)
		{
			var dtStr = datetime.ToString("yyyy-MM-ddTHH:mm:ss");
			return GetVideoFilesBeforeWhen(ip, dtStr);
		}
		public static ShinobiResponseVideoFileVm GetVideoFilesBeforeWhen(string ip, string datetime)
		{
			try
			{
				var dtStr = datetime;
				var monitorId = ip.ToMonitorId();
				var url = $"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/videos/{ShinobiConfig.ShinobiGroupKey}/{monitorId}?end={dtStr}";
				using (var client = new HttpClient())
				{
					var json = client.GetStringAsync(url).Result;
					var videoResp = JsonConvert.DeserializeObject<ShinobiResponseVideoFileVm>(json);
					return videoResp;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return new ShinobiResponseVideoFileVm();
			}
		}


		public static ShinobiResponseVideoFileVm GetVideoFilesAfterTime(string ip, DateTime datetime)
		{
			try
			{
				var dtStr = datetime.ToString("yyyy-MM-ddTHH:mm:ss");
				var monitorId = ip.ToMonitorId();
				var url = $"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/videos/{ShinobiConfig.ShinobiGroupKey}/{monitorId}?start={dtStr}";
				using (var client = new HttpClient())
				{
					_log.Info($"[{ip} req shinobi time:] {url}");
					var json = client.GetStringAsync(url).Result;
					_log.Info($"[{ip} resp shinobi time:] ");
					var videos = JsonConvert.DeserializeObject<ShinobiResponseVideoFileVm>(json);
					return videos;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return new ShinobiResponseVideoFileVm();
			}
		}

		/// <summary>
		/// This is called by a batchjob every 1 minute .make sure only record 10 minutes video
		/// </summary>
		/// <param name="vm"></param>
		/// <returns></returns>
		public static bool DeleteVideoFile(ShinobiResponseVideoVm vm)
		{
			try
			{
				var url = $"{ShinobiConfig.ShinobiBaseUrl}{vm.links.deleteVideo}";
				using (var client = new HttpClient())
				{
					var ret = client.GetStringAsync(url).Result;
					var retObj = JsonConvert.DeserializeObject<ShinobiGeneralResponse>(ret);
					if (!retObj.ok)
					{
						_log.Error($"Delete Video Failed. camera :{vm.mid}. error :{retObj.msg}");
						return false;
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return false;
			}
		}

		/// <summary>
		/// Make sure the jepg api is enabled on shinobi
		/// </summary>
		/// <param name="camera"></param>
		/// <returns></returns>
		public static byte[] GetSnapshot(Camera camera)
		{
			try
			{
				var url =
					$"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/jpeg/{ShinobiConfig.ShinobiGroupKey}/{camera.IpAddress.ToMonitorId()}/s.jpg";
				using (var client = new HttpClient())
				{
					var byteArr = client.GetByteArrayAsync(url).Result;
					return byteArr;
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return null;
			}
		}
	}
}

