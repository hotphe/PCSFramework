using System;

namespace PCS.Common
{
    public static class UnixTime
    {
        private static DateTime savedLocalTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary> Record the time when a response is received from the server. (Latest Response Time)/// </summary>
        private static long savedUnixTime;

        public static long Now
        {
            get { return savedUnixTime + (long)(DateTime.UtcNow - savedLocalTime).TotalSeconds; }
            set
            {
                savedLocalTime = DateTime.UtcNow;
                savedUnixTime = value;
            }
        }
    }
}
