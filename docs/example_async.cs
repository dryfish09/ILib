using DryFish.ILib;
using System.Threading.Tasks;
namespace myapp
{
    class Program
    {
        static async Task Main()
        {
            ILib.INotice("=== DryFish.ILib Demo ===");
            
            // Log các loại khác nhau
            ILib.ILogInfo("This is an info message");
            ILib.IWarn("This is a warning message");
            ILib.INotice("This is a notice message");
            ILib.ILogDebug("This is a debug message (DEBUG mode only)");
            ILib.ILog("CUSTOM", "This is a custom log with prefix");
            
            // Delay demo
            ILib.ILogInfo("Waiting 1 second...");
            await ILib.IDelayAsync(1000);
            
            ILib.ILogInfo("Waiting another 500ms...");
            ILib.IDelay(500);
            
            // Mô phỏng kiểm tra lỗi
            bool hasError = false;
            
            if (hasError)
            {
                ILib.IWarn("Error detected!");
                ILib.IExit(1);
            }
            else
            {
                ILib.ILogInfo("No errors, exiting normally");
                ILib.IExit(0);
            }
        }
    }
}
