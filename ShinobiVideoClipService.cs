using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Sample.ShinobiVideo.Lib.Models;

namespace Sample.ShinobiVideo.Lib
{
	public class ShinobiVideoClipService
	{
		public static ShinobiVideoClipService Do
		{
			get { return new ShinobiVideoClipService(); }
		}

		private ILog _log = LogManager.GetLogger(typeof(ShinobiVideoClipService));
		private static object _obj = new object();
		public bool DownloadMediaToDashboardMediaFolder(string ip, long eventId, string folderName)
		{
			var folderPath = ShinobiConfig.DashboardMediaFolder;
			if (!Directory.Exists(folderPath))
			{
				_log.Error($"failed to find shinobi media folder {folderPath}");
				return false;
			}


			folderPath = Path.Combine(folderPath, folderName);
			lock (_obj)
			{
				if (!Directory.Exists(folderPath))
				{
					lock (_obj)
					{
						Directory.CreateDirectory(folderPath);
					}
				}
			}

			var nowTime = DateTime.Now;
			Task.Run(() =>
			{
				try
				{
					var count = 20;
					var tried = 0;
					var snapshotUrl = $"{ShinobiConfig.ShinobiBaseUrl}/{ShinobiConfig.ShinobiApiKey}/jpeg/{ShinobiConfig.ShinobiGroupKey}/{ip.ToMonitorId()}/s.jpg";
					var snapShotName = Path.Combine(folderPath, eventId + ".jpg");
					while (tried < count)
					{
						using (var httpClient = new HttpClient())
						{
							var byteArr = httpClient.GetByteArrayAsync(snapshotUrl).Result;
							var imageGood = ImageChecker.IsGoodImage(byteArr);
							_log.Info($"[Video clip service] {ip} Image is good ? {imageGood}");
							if (imageGood)
							{
								using (var cl = new WebClient())
								{
									cl.DownloadFile(snapshotUrl, snapShotName);
									break;
								}
							}
							else
							{
								_log.Warn("Not a good image . retrying ..");
							}
						}

						tried++;
						Thread.Sleep(500);

					}

				}
				catch (Exception ex)
				{
					_log.Error(ex);
				}
			});

			Task.Run(() =>
			{
				var maxretry = 100;
				////download first video
				var videoPath1 = Path.Combine(folderPath, eventId + ".mp4");
				var videoTimeFrom = ShinobiConfig.FirstFootageAfterThisTime();

				var eventTime = nowTime.AddSeconds(ShinobiConfig.DeltaWindow);
				var time = RetryDownloadVideoAfterTime(ip, videoTimeFrom, videoPath1, maxretry, eventTime, 1).Result;
				if (time == null)
				{
					_log.Error("Get First video failed");
					return;
				}

				var firstVideoStartTime = time.Value.AddSeconds(1);

				////download 2nd video
				var videoPath2 = Path.Combine(folderPath, eventId + "_2.mp4");
				var r = RetryDownloadVideoAfterTime(ip, firstVideoStartTime, videoPath2, maxretry, nowTime, 2).Result;
			});
			return true;
		}

		private async Task<DateTime?> RetryDownloadVideoAfterTime(string ip, DateTime afterTime,
			string path, int maxretry, DateTime videoTime, int videoIndex)
		{
			try
			{
				var retried = 0;
				while (retried < maxretry)
				{
					try
					{
						var videoResp = ShinobiService.GetVideoFilesAfterTime(ip, afterTime);

						if (videoResp.videos?.Count > 0)
						{
							var videos = videoResp.videos;

							var theVideo = videoIndex == 1 ? videos.OrderByDescending(x => x.time).FirstOrDefault() : videos.OrderBy(x => x.time).FirstOrDefault();
							if (theVideo != null)
							{
								var startTime = DateTime.Parse(theVideo.time);
								var endtime = DateTime.Parse(theVideo.end);
								var timePointOfVideoWindow = videoIndex == 1 ? endtime : startTime;

								if (videoIndex == 1 && timePointOfVideoWindow < videoTime)
								{
									//_log.Info($"video time is {theVideo.time} but should after {videoShouldAfterTime}");
									Thread.Sleep(1000);
									retried++;
									continue;
								}
								else
								{
									_log.Info($"[GOT VIDEO] video time is {timePointOfVideoWindow} . should after {videoTime}");
									using (var cl = new WebClient())
									{
										var videoUrl = $"{ShinobiConfig.ShinobiBaseUrl}{theVideo.href}";
										cl.DownloadFile(videoUrl, path);
										_log.Info($"video has been saved into :{path}");
									}
								}

								return startTime;
							}
						}

						retried++;
						_log.Info($"[Get video clip shinobi] didnt get it {ip} {path}.retrying {retried} times");
						await Task.Delay(1000);
					}
					catch (Exception ex2)
					{
						_log.Error(ex2);
					}
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
			}

			return null;
		}

		public void DeleteTheFilesOnlyRemain10Mins()
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					try
					{
						var allRunning = ShinobiService.AllMonitoringObjs();
						//_log.Debug($"{allRunning.Count} monitors found running.");
						foreach (var shinobiMonitorObj in allRunning)
						{
							var mid = shinobiMonitorObj.mid;
							var ip = mid.ToIp();

							var tenMinsBack = ShinobiConfig.VideoStayMinutes();
							var videosToDelete = ShinobiService.GetVideoFilesBeforeWhen(ip, tenMinsBack);
							if (videosToDelete.videos?.Count > 0)
							{
								var videos = videosToDelete.videos;
								foreach (var shinobiResponseVideoVm in videos)
								{
									var ret = ShinobiService.DeleteVideoFile(shinobiResponseVideoVm);
									if (!ret)
									{
										_log.Error($"Delete video file error mid:{shinobiResponseVideoVm.mid}");
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						_log.Error(ex);
					}

					Thread.Sleep(10000);
				}
			}, TaskCreationOptions.LongRunning);
		}
	}
}
