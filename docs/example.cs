using DryFish.ILib;
using System.Threading.Tasks;

namespace MyApp
{
    class Program
    {
        static async Task Main()
        {
            // Enable debug mode
            ILib.ISetDebug(true);
            
            ILib.INotice("Application starting (async mode)...");
            
            // Async delay without blocking
            ILib.ILogInfo("Waiting 2 seconds asynchronously...");
            await ILib.IDelayAsync(2000);
            
            // Simulate async processing
            ILib.ILogInfo("Processing data asynchronously...");
            await ProcessDataAsync();
            
            // Get timezone info
            string vnTime = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
            ILib.ILogInfo($"Current time in Vietnam: {vnTime}");
            
            ILib.ILogComplete("Application completed!");
        }
        
        static async Task ProcessDataAsync()
        {
            for (int i = 0; i <= 100; i += 20)
            {
                ILib.ILogInfo($"Progress: {i}%");
                await ILib.IDelayAsync(300);
            }
        }
    }
}
