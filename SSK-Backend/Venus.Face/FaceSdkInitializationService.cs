using Microsoft.Extensions.Hosting;

namespace Venus.Face
{
    public class FaceSdkInitializationService : IHostedService
    {
        private readonly FaceSdkWrapper _sdkWrapper;

        public FaceSdkInitializationService(FaceSdkWrapper sdkWrapper)
        {
            _sdkWrapper = sdkWrapper;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 执行初始化调用
                var result = _sdkWrapper.IsInitialized;
                Console.WriteLine($"人脸识别SDK初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "人脸识别SDK初始化失败");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
