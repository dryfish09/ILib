using DryFish.ILib;

namespace myapp
{
    class Program
    {
        static void Main()
        {
            // 1. INotice - Thông báo thông thường
            ILib.INotice("Application starting...");
            
            // 2. ILogInfo - Info log
            ILib.ILogInfo("Initializing application components");
            
            // 3. IDelay - Delay (sleep)
            ILib.ILogInfo("Waiting 2 seconds...");
            ILib.IDelay(2000);
            
            // 4. Kiểm tra dependency
            string dependency = "no";
            string requirement = "ILib";
            
            if (dependency != requirement)
            {
                // 5. IWarn - Cảnh báo
                ILib.IWarn("Missing dependencies detected!");
                
                // 6. ILog - Custom log với prefix
                ILib.ILog("CHECK", "Dependency check failed");
                ILib.ILog("REQUIRED", requirement);
                ILib.ILog("FOUND", dependency);
                
                // 7. ILogDebug - Debug log (chỉ hiển thị trong DEBUG mode)
                ILib.ILogDebug("This debug message won't show in Release mode");
                
                // 8. IExit - Thoát với mã lỗi
                ILib.IExit(1);
            }
            
            // Nếu không có lỗi, tiếp tục
            ILib.ILogInfo("All dependencies satisfied!");
            
            // 9. INotice - Thông báo thành công
            ILib.INotice("Application running successfully");
            
            // 10. Delay async
            ILib.IDelayAsync(1000).Wait(); // Hoặc dùng await trong async method
            
            ILib.ILogInfo("Processing data...");
            ILib.IDelay(500);
            
            ILib.ILogInfo("Done!");
            ILib.INotice("Application finished");
        }
    }
}
