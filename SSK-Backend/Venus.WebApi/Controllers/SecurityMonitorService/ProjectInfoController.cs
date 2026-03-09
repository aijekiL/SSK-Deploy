using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using Venus.Authentication;
using Venus.Authentication.User;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Tools.Response;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 项目信息接口
    /// </summary>
    public class ProjectInfoController : VenusBaseController
    {
        private readonly VenusDbContext _dbContext;
        public ProjectInfoController(VenusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取工程信息列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetList()
        {
            return new WebResponseContent()
            {
                Data = _dbContext.Set<SM_ProjectInfo>()
                    .Where(o => o.CreateID == UserContext.Current.UserId && !o.Deleted)
                    .ToList()
            };
        }


        public class SaveDto
        {
            public Guid? ObjectId { get; set; }
            public string ProjectName { get; set; } = string.Empty;
            public string ManagerName { get; set; } = string.Empty;
            public string ManagerPhone { get; set; } = string.Empty;
            public DateTime StartWorkDate { get; set; }
        }

        /// <summary>
        /// 保存工程信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent Save([FromBody] SaveDto dto)
        {
            SM_ProjectInfo? info = null;
            if (dto.ObjectId != null)
            {
                info = _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.ObjectId == dto.ObjectId);
            }
            if (info == null)
            {
                info = new SM_ProjectInfo();
                info.ObjectId = Guid.NewGuid();
                info.ProjectName = dto.ProjectName;
                info.ManagerName = dto.ManagerName;
                info.ManagerPhone = dto.ManagerPhone;
                info.StartWorkDate = dto.StartWorkDate;
                info.CreateDate = DateTime.Now;
                info.CreateID = UserContext.Current.UserId;
                info.Deleted = false;
                _dbContext.Set<SM_ProjectInfo>().Add(info);
            }
            else
            {
                info.ProjectName = dto.ProjectName;
                info.ManagerName = dto.ManagerName;
                info.ManagerPhone = dto.ManagerPhone;
                info.StartWorkDate = dto.StartWorkDate;
                _dbContext.Set<SM_ProjectInfo>().Add(info);
            }
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("保存成功", info);
        }

        /// <summary>
        /// 获取工程信息
        /// </summary>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetData(Guid ObjectId)
        {
            var data = _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.ObjectId == ObjectId);
            return WebResponseContent.Instance.OK("获取成功", data);
        }

        /// <summary>
        /// 完工
        /// </summary>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent Completed(Guid ObjectId)
        {
            var data = _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.ObjectId == ObjectId);
            if (data == null)
                return WebResponseContent.Instance.Error("未查询到此工程信息");
            data.Status = 1;
            _dbContext.Set<SM_ProjectInfo>().Update(data);
            _dbContext.SaveChanges();

            return WebResponseContent.Instance.OK("修改成功", data);
        }

        public class DeleteDto
        {
            public List<Guid> ObjectIds { get; set; } = new();
        }

        /// <summary>
        /// 删除工程
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent Delete([FromBody] DeleteDto dto)
        {
            if (dto?.ObjectIds == null || dto.ObjectIds.Count == 0)
            {
                return WebResponseContent.Instance.Error("请选择要删除的工程");
            }

            var projects = _dbContext.Set<SM_ProjectInfo>()
                .Where(o => dto.ObjectIds.Contains(o.ObjectId) && o.CreateID == UserContext.Current.UserId)
                .ToList();

            if (projects.Count == 0)
            {
                return WebResponseContent.Instance.Error("未查询到工程信息");
            }

            if (projects.Any(o => o.Status != 1))
            {
                return WebResponseContent.Instance.Error("只允许删除已完成工程");
            }

            var now = DateTime.Now;
            foreach (var project in projects)
            {
                project.Deleted = true;
                project.DeleteId = UserContext.Current.UserId;
                project.DeleteDate = now;
                project.IsCurrent = false;
            }

            _dbContext.Set<SM_ProjectInfo>().UpdateRange(projects);
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("删除成功");
        }

        /// <summary>
        /// 获取工程配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetConfig()
        {
            var configs = _dbContext.Set<SM_Config>().ToList();
            return WebResponseContent.Instance.OK("获取配置成功", configs);
        }

        /// <summary>
        /// 获取当前工程信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetCurPrjInfo()
        {
            var data = _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.IsCurrent && x.CreateID == UserContext.Current.UserId && !x.Deleted);
            return WebResponseContent.Instance.OK("获取成功", data);
        }

        /// <summary>
        /// 设置当前工程
        /// </summary>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent SetCurPrjId(Guid ObjectId)
        {
            var curData = _dbContext.Set<SM_ProjectInfo>().FirstOrDefault(x => x.ObjectId == ObjectId);
            if (curData == null) return WebResponseContent.Instance.Error("无此项目");

            var curs = _dbContext.Set<SM_ProjectInfo>().Where(o => o.IsCurrent && o.ObjectId != curData.ObjectId).ToList();
            foreach (var cur in curs)
            {
                cur.IsCurrent = false;
            }
            _dbContext.Set<SM_ProjectInfo>().UpdateRange(curs);
            curData.IsCurrent = true;
            _dbContext.Set<SM_ProjectInfo>().Update(curData);
            _dbContext.SaveChanges();
            this.SendMessage(curData.ObjectId);
            return WebResponseContent.Instance.OK("获取成功", curData);
        }

        // 存储所有已连接的 WebSocket 客户端
        private static readonly ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();

        /// <summary>
        /// 获取监听项目信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task GetMonitorProjectInfo()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    var connectionId = UserContext.Current.UserId.ToString() + "_" + Guid.NewGuid().ToString();
                    _webSockets.TryAdd(connectionId, webSocket);

                    try
                    {
                        await Echo(webSocket);
                    }
                    finally
                    {
                        _webSockets.TryRemove(connectionId, out _);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    }
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }

        private async void SendMessage(Guid ObjectId)
        {
            var ws = _webSockets.Where(o => o.Key.StartsWith(UserContext.Current.UserId.ToString())).ToList();
            var tasks = new List<Task>();
            foreach (var w in ws)
            {
                var socket = w.Value;
                if (socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(ObjectId.ToString());
                    tasks.Add(socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }
            await Task.WhenAll(tasks);
        }

    }
}

