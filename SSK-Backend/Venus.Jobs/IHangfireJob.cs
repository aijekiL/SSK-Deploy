using Venus.Tools;
using Venus.Tools.Response;

namespace MIS.Jobs
{
    public interface IHangfireJob: IDependency
    {
        /// <summary>
        /// Job执行任务
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        WebResponseContent Execute(params object[] args);
    }
}
