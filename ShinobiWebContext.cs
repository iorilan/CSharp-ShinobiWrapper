using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Sample.ShinobiVideo.Lib.Models;
using Task = System.Threading.Tasks.Task;


namespace Sample.ShinobiVideo.Lib
{
	/// <summary>
	/// Apis for liveview
	/// </summary>
    public static class ShinobiWebContext
    {
        private static volatile Dictionary<string, ShinobiMonitoringVm> CameraLastRequestTime ;

        static ShinobiWebContext()
        {
            CameraLastRequestTime = new Dictionary<string, ShinobiMonitoringVm>();
        }

        private static ILog _log = LogManager.GetLogger(typeof(ShinobiWebContext));

        /// <summary>
        /// client to ping this function every x seconds to keep the camera active
        /// </summary>
        public static void Ping(string ip)
        {
            var mid = ip.ToMonitorId();
            ////we use camera ip as identity
            if (CameraLastRequestTime.ContainsKey(mid))
            {
                CameraLastRequestTime[mid].LastPingTime = DateTime.Now;
            }
            else
            {
                var obj = new ShinobiMonitoringVm()
                {
                    CameraIp = ip,
                    LastPingTime = DateTime.Now,
                    MonitorId = mid
                };
                CameraLastRequestTime.Add(mid, obj);
            }
        }

        public static bool View(CameraVm camera)
        {
            Ping(camera.IpAddress);
            var ret = ShinobiService.AddCamera(camera);
            return ret;
        }


        public static void DeleteCamerasThoseAreIdle()
        {
            var t = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var activeWindow = DateTime.Now.AddSeconds(-ShinobiConfig.MaxIdleSecond);
                        var keys = CameraLastRequestTime.Keys.ToList();

                        if (keys.Count > 0)
                        {
                            ////delete those are idle too long
                            for (var i = 0; i < keys.Count; i++)
                            {
                                var camera = CameraLastRequestTime[keys[i]];

								////skip recording camera
	                            if (ShinobiConfig.RecordingArr().Contains(camera.MonitorId))
	                            {
									continue;
	                            }

                                if (camera.LastPingTime < activeWindow)
                                {
                                    var ret = ShinobiService.DeleteCamera(camera.MonitorId);
                                    _log.Info($"monitor idle for {ShinobiConfig.MaxIdleSecond} seconds, so delete it from monitor. result :{ret}");
                                    if (ret)
                                    {
                                        CameraLastRequestTime.Remove(keys[i]);
                                        keys = CameraLastRequestTime.Keys.ToList();
                                    }
                                }
                            }
                        }

						/*
						 * Removed this logic . as we want shinobi to record & provide snapshot
						 */
						//delete those are not in list(added from somewhere else)
						var allMonitored = ShinobiService.AllMonitoringObjs();
						var thoseNotInDictionay = allMonitored.Where(x => !keys.Contains(x.mid)).ToList();
						//_log.Info($"total {allMonitored.Count} and {thoseNotInDictionay.Count} not in dictionary");
						if (thoseNotInDictionay.Count > 0)
						{
							for (int i = 0; i < thoseNotInDictionay.Count; i++)
							{
								////skip recording camera
								if (ShinobiConfig.RecordingArr().Contains(thoseNotInDictionay[i].mid))
								{
									continue;
								}

								var ret = ShinobiService.DeleteCamera(thoseNotInDictionay[i].mid);
								_log.Warn($"Deleted monitor {thoseNotInDictionay[i].mid} created from somewhere else {ret}");
							}
						}

						Thread.Sleep(10000);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                  
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
