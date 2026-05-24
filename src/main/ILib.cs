namespace DryFish.ILib
{
    public static class ILib
    {
        // Màu sắc cho console (tùy chọn)
        private static readonly object _consoleLock = new object();
        
        /// <summary>
        /// Hiển thị cảnh báo
        /// </summary>
        public static void IWarn(string message)
        {
            lock (_consoleLock)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] {message}");
                Console.ForegroundColor = originalColor;
            }
        }
        
        /// <summary>
        /// Hiển thị thông báo thông thường
        /// </summary>
        public static void INotice(string message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine($"[NOTICE] {message}");
            }
        }
        
        /// <summary>
        /// Thoát ứng dụng với mã lỗi
        /// </summary>
        public static void IExit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
        
        /// <summary>
        /// Ghi log với prefix tùy chỉnh
        /// </summary>
        public static void ILog(string prefix, string message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine($"[{prefix}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }
        
        /// <summary>
        /// Debug log
        /// </summary>
        public static void ILogDebug(string message)
        {
#if DEBUG
            lock (_consoleLock)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");
                Console.ForegroundColor = originalColor;
            }
#endif
        }
        
        /// <summary>
        /// Info log
        /// </summary>
        public static void ILogInfo(string message)
        {
            lock (_consoleLock)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Console.ForegroundColor = originalColor;
            }
        }
        
        /// <summary>
        /// Delay (sleep) milliseconds
        /// </summary>
        public static void IDelay(int milliseconds)
        {
            if (milliseconds > 0)
            {
                Thread.Sleep(milliseconds);
            }
        }
        
        /// <summary>
        /// Delay async version
        /// </summary>
        public static async Task IDelayAsync(int milliseconds)
        {
            if (milliseconds > 0)
            {
                await Task.Delay(milliseconds);
            }
        }
    }
}
