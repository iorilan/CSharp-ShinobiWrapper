namespace Sample.ShinobiVideo.Lib
{
   public static class StringEx
    {
        public static string ToMonitorId(this string ip)
        {
            return ip.Replace(".", "_");
        }

	    public static string ToIp(this string mid)
	    {
		    return mid.Replace("_", ".");
		}
    }
}
