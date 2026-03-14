using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Venus.Authentication;
using Venus.Authentication.User;
using Venus.Entitys;
using Venus.Entitys.SecurityMonitorService;
using Venus.Entitys.Sys;
using Venus.IService.Attachment;
using Venus.Service;
using Venus.Service.EfCore;
using Venus.Tools.Extension;
using Venus.Tools.Request;
using Venus.Tools.Response;
using Venus.WebApi.Services;

namespace Venus.WebApi.Controllers.SecurityMonitorService
{
    /// <summary>
    /// 员工信息
    /// </summary>
    public class StaffInfoController : VenusBaseController
    {
        private readonly VenusDbContext _dbContext;
        private readonly IAttachmentService _attachmentService;
        private readonly CompreFaceClient _compreFaceClient;
        private readonly FileStorageService _fileStorageService;
        public StaffInfoController(VenusDbContext dbContext, IAttachmentService attachmentService, CompreFaceClient compreFaceClient, FileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _attachmentService = attachmentService;
            _compreFaceClient = compreFaceClient;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// 人员信息保存DTO
        /// </summary>
        public class SaveStaffInfoDto
        {
            /// <summary>
            /// 人员信息ID
            /// </summary>
            public Guid? ObjectId { get; set; }

            /// <summary>
            /// 项目ID-SM_ProjectInfo-ObjectId
            /// </summary>
            public Guid ParentId { get; set; }

            /// <summary>
            /// 姓名
            /// </summary>
            public string name { get; set; } = string.Empty;

            /// <summary>
            /// 性别
            /// </summary>
            public string gender { get; set; } = string.Empty;

            /// <summary>
            /// 身份证号
            /// </summary>
            public string id { get; set; } = string.Empty;

            /// <summary>
            /// 联系方式
            /// </summary>
            public string phone { get; set; } = string.Empty;

            /// <summary>
            /// 工作单位
            /// </summary>
            public string company { get; set; } = string.Empty;

            /// <summary>
            /// 工作部门
            /// </summary>
            public string department { get; set; } = string.Empty;

            /// <summary>
            /// 设备1
            /// </summary>
            public string Device1 { get; set; } = string.Empty;

            /// <summary>
            /// 附件ID
            /// </summary>
            public Guid AttachmentId { get; set; }
        }

        /// <summary>
        /// 工器具DTO
        /// </summary>
        public class SaveStaffToolInfoDto
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            public Guid StaffId { get; set; }

            /// <summary>
            /// 工具名称
            /// </summary>
            public string ToolName { get; set; } = string.Empty;

            /// <summary>
            /// 工具数量
            /// </summary>
            public int ToolNumber { get; set; }

            /// <summary>
            /// 附件ID
            /// </summary>
            public Guid AttachmentId { get; set; }

        }

        /// <summary>
        /// 确认员工信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent ConfirmStaffInfo([FromBody] SaveStaffInfoDto dto)
        {
            // 判断该用户是否已在场（仅检查状态为2的记录，表示已完成登记并入场）
            var checkData = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => !o.Deleted && o.ProjectId == dto.ParentId && o.id == dto.id && o.Status == 2);
            if (checkData != null)
            {
                return WebResponseContent.Instance.Error("该人员已在场。");
            }

            var duplicateId = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                !o.Deleted &&
                o.ProjectId == dto.ParentId &&
                o.id == dto.id &&
                o.ObjectId != dto.ObjectId &&
                o.Status != 0);
            if (duplicateId != null)
            {
                // allow continuing an incomplete registration
                if (duplicateId.Status == 1)
                {
                    dto.ObjectId = duplicateId.ObjectId;
                }
                else
                {
                    return WebResponseContent.Instance.Error("身份证号已登记。");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Device1) && dto.Device1 != "未绑定")
            {
                var duplicateDevice = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.ProjectId == dto.ParentId &&
                    o.Device1 == dto.Device1 &&
                    o.ObjectId != dto.ObjectId &&
                    o.Status != 0);
                if (duplicateDevice != null)
                {
                    return WebResponseContent.Instance.Error("设备已绑定。");
                }
            }

            SM_StaffInfo? staffInfo = null;
            if (dto.ObjectId != null)
            {
                staffInfo = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(x => x.ObjectId == dto.ObjectId && !x.Deleted);
            }
            if (staffInfo == null)
            {
                staffInfo = new SM_StaffInfo()
                {
                    ObjectId = Guid.NewGuid(),
                    ProjectId = dto.ParentId,
                    name = dto.name,
                    gender = dto.gender,
                    id = dto.id,
                    phone = dto.phone,
                    company = dto.company,
                    department = dto.department,
                    Device1 = dto.Device1,
                    Status = 1, // 登记中状态
                    CreateDate = DateTime.Now,
                    CurrentStep = 1
                };
                _dbContext.Set<SM_StaffInfo>().Add(staffInfo);
            }
            else
            {
                staffInfo.ProjectId = dto.ParentId;
                staffInfo.name = dto.name;
                staffInfo.gender = dto.gender;
                staffInfo.id = dto.id;
                staffInfo.phone = dto.phone;
                staffInfo.company = dto.company;
                staffInfo.department = dto.department;
                staffInfo.Device1 = dto.Device1;
                // 只有在不是已完成登记状态时才设置为登记中
                if (staffInfo.Status != 2)
                {
                    staffInfo.Status = 1; // 登记中状态
                }
                staffInfo.ModifyDate = DateTime.Now;

                if (staffInfo.CurrentStep < 1)
                {
                    staffInfo.CurrentStep = 1;
                    // 当CurrentStep变为1时，不改变Status状态，除非Status不是2
                }

                _dbContext.Set<SM_StaffInfo>().Update(staffInfo);
            }

            // 赋值附件
            Sys_Attachment? att = null;
            if (dto.AttachmentId != Guid.Empty)
            {
                att = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.BizObjectTable == "SM_StaffInfo" && o.DataField == "djz" && o.ObjectId == dto.AttachmentId);
                if (att != null)
                {
                    att.BizObjectId = staffInfo.ObjectId.ToString();
                    _dbContext.Set<Sys_Attachment>().Update(att);
                }
            }

            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("确认成功", staffInfo);
        }

        /// <summary>
        /// 保存用户工具信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent AddStaffToolInfo([FromBody] SaveStaffToolInfoDto dto)
        {
            SM_StaffTool tool = new SM_StaffTool()
            {
                ObjectId = Guid.NewGuid(),
                ParentId = dto.StaffId,
                ToolName = dto.ToolName,
                ToolNumber = dto.ToolNumber
            };
            _dbContext.Set<SM_StaffTool>().Add(tool);

            // 赋值附件
            Sys_Attachment? att = null;
            if (dto.AttachmentId != Guid.Empty)
            {
                att = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.BizObjectTable == "SM_StaffInfo" && o.DataField == "SM_StaffTool.fj" && o.ObjectId == dto.AttachmentId);
                if (att != null)
                {
                    att.BizObjectId = tool.ObjectId.ToString();
                    _dbContext.Set<Sys_Attachment>().Update(att);
                }
            }
            _dbContext.SaveChanges();

            return WebResponseContent.Instance.OK("保存成功", new
            {
                tool,
                att
            });
        }

        /// <summary>
        /// 绑定设备
        /// </summary>
        /// <param name="StaffId"></param>
        /// <param name="Device"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent BindDevice(Guid StaffId, string? Device)
        {
            var staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => !o.Deleted && o.ObjectId == StaffId);
            if (staff == null)
                return WebResponseContent.Instance.Error("不存在该员工。");
            staff.Device1 = Device;
            _dbContext.Set<SM_StaffInfo>().Update(staff);
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("绑定成功", staff);
        }

        /// <summary>
        /// 删除员工工具信息
        /// </summary>
        /// <param name="ToolId"></param>
        /// <returns></returns>
        [HttpGet, Route("{ToolId}")]
        public WebResponseContent DelStaffToolInfo(Guid ToolId)
        {
            var tool = _dbContext.Set<SM_StaffTool>().FirstOrDefault(o => o.ObjectId == ToolId);
            if (tool != null)
            {
                tool.Deleted = true;
                _dbContext.Set<SM_StaffTool>().Update(tool);

                var att = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.BizObjectTable == "SM_StaffInfo" && o.DataField == "SM_StaffTool.fj" && o.BizObjectId == tool.ObjectId.ToString());
                if (att != null)
                {
                    att.Deleted = true;
                    _dbContext.Set<Sys_Attachment>().Update(att);
                }
                _dbContext.SaveChanges();
                return WebResponseContent.Instance.OK("删除成功");
            }
            else
            {
                return WebResponseContent.Instance.Error("未找到工具记录。");
            }
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetStaffList(Guid ProjectId)
        {
            var staffList = _dbContext.Set<SM_StaffInfo>().Where(o => !o.Deleted && o.ProjectId == ProjectId).ToList();
            return WebResponseContent.Instance.OK("获取员工列表成功", staffList);
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <param name="StaffId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent GetStaffData(Guid StaffId)
        {
            // 查询人员登记照
            var djzAtt = _dbContext.Set<Sys_Attachment>()
                .Where(o => !o.Deleted && o.BizObjectTable == "SM_StaffInfo" && o.DataField == "djz" && o.BizObjectId == StaffId.ToString())
                .FirstOrDefault();

            var staff = _dbContext.Set<SM_StaffInfo>().Where(o => !o.Deleted && o.ObjectId == StaffId).Select(o => new
            {
                o.ObjectId,
                o.ProjectId,
                o.name,
                o.gender,
                o.id,
                o.phone,
                o.company,
                o.department,
                o.Status,
                o.Device1,
                o.CreateDate,
                djzAtt = djzAtt
            }).FirstOrDefault();

            var tools = _dbContext.Set<SM_StaffTool>().Where(o => !o.Deleted && o.ParentId == StaffId).AsEnumerable().Select(o => new
            {
                tool = o,
                files = _dbContext.Set<Sys_Attachment>().Where(p => !p.Deleted && p.BizObjectId == o.ObjectId.ToString() && p.BizObjectTable == "SM_StaffInfo").AsEnumerable()
            }).ToList();
            return WebResponseContent.Instance.OK("获取成功", new
            {
                staff,
                tools
            });
        }

        /// <summary>
        /// 员工退场
        /// </summary>
        /// <param name="StaffId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent StaffExit(Guid StaffId)
        {
            var staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => o.ObjectId == StaffId);
            if (staff == null)
                return WebResponseContent.Instance.Error("不存在该员工。");

            var staffTools = _dbContext.Set<SM_StaffTool>().Where(o => o.ParentId == StaffId).ToList();
            var toolIds = staffTools.Select(o => o.ObjectId.ToString()).ToList();
            var toolAtts = _dbContext.Set<Sys_Attachment>()
                .Where(o => o.BizObjectTable == "SM_StaffInfo" && toolIds.Contains(o.BizObjectId))
                .ToList();
            var staffAtts = _dbContext.Set<Sys_Attachment>()
                .Where(o => o.BizObjectTable == "SM_StaffInfo" && o.BizObjectId == StaffId.ToString())
                .ToList();

            var mongoIds = toolAtts.Select(o => o.MongoId)
                .Concat(staffAtts.Select(o => o.MongoId))
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct()
                .ToList();
            var deleteAttachmentIds = toolAtts.Select(o => o.ObjectId)
                .Concat(staffAtts.Select(o => o.ObjectId))
                .ToList();
            foreach (var mongoId in mongoIds)
            {
                var hasOther = _dbContext.Set<Sys_Attachment>()
                    .Any(o => o.MongoId == mongoId && !deleteAttachmentIds.Contains(o.ObjectId));
                if (!hasOther)
                {
                    _fileStorageService.DeleteFile(mongoId);
                }
            }

            _dbContext.Set<Sys_Attachment>().RemoveRange(toolAtts);
            _dbContext.Set<Sys_Attachment>().RemoveRange(staffAtts);
            _dbContext.Set<SM_StaffTool>().RemoveRange(staffTools);
            _dbContext.Set<SM_StaffInfo>().Remove(staff);
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("人员退场成功");
        }

        /// <summary>
        /// 更新当前步骤
        /// </summary>
        /// <param name="StaffId"></param>
        /// <param name="curStep"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent UpdateCurStep(Guid StaffId, int curStep)
        {
            var staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => !o.Deleted && o.ObjectId == StaffId);
            if (staff == null)
                return WebResponseContent.Instance.Error("不存在该员工。");
            if (staff.CurrentStep < curStep)
            {
                staff.CurrentStep = curStep;
                _dbContext.Set<SM_StaffInfo>().Update(staff);
                _dbContext.SaveChanges();
            }
            return WebResponseContent.Instance.OK("操作成功");

        }

        /// <summary>
        /// 完成员工登记并正式入场
        /// </summary>
        /// <param name="StaffId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent CompleteRegistration(Guid StaffId)
        {
            var staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => !o.Deleted && o.ObjectId == StaffId);
            if (staff == null)
                return WebResponseContent.Instance.Error("不存在该员工。");
            
            // 将员工状态设置为在场，表示完成登记并正式入场
            staff.Status = 2;
            _dbContext.Set<SM_StaffInfo>().Update(staff);
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("员工正式入场成功");
        }

        #region  人员档案管理

        /// <summary>
        /// 人员信息保存DTO
        /// </summary>
        public class SaveEmployeeProfileDto
        {
            /// <summary>
            /// 人员信息ID
            /// </summary>
            public Guid? ObjectId { get; set; }

            /// <summary>
            /// 姓名
            /// </summary>
            public string name { get; set; } = string.Empty;

            /// <summary>
            /// 性别
            /// </summary>
            public string gender { get; set; } = string.Empty;

            /// <summary>
            /// 身份证号
            /// </summary>
            public string id { get; set; } = string.Empty;

            /// <summary>
            /// 联系方式
            /// </summary>
            public string phone { get; set; } = string.Empty;

            /// <summary>
            /// 工作单位
            /// </summary>
            public string company { get; set; } = string.Empty;

            /// <summary>
            /// 工作部门
            /// </summary>
            public string department { get; set; } = string.Empty;

            /// <summary>
            /// 附件ID
            /// </summary>
            public Guid AttachmentId { get; set; }
        }

        /// <summary>
        /// 人员档案登记
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<WebResponseContent> SaveEmployeeProfileAsync([FromBody] SaveEmployeeProfileDto dto)
        {
            // 判断该用户是否已存在
            var checkData = _dbContext.Set<SM_EmployeeProfile>().FirstOrDefault(o => !o.Deleted && o.id == dto.id && o.ObjectId != dto.ObjectId);
            if (checkData != null)
            {
                return WebResponseContent.Instance.Error($"人员身份已注册：[{checkData.name}][{checkData.id}]。");
            }

            SM_EmployeeProfile? EmployeeProfile = null;
            if (dto.ObjectId != null)
            {
                EmployeeProfile = _dbContext.Set<SM_EmployeeProfile>().FirstOrDefault(x => x.ObjectId == dto.ObjectId);
            }
            if (EmployeeProfile == null)
            {
                EmployeeProfile = new SM_EmployeeProfile()
                {
                    ObjectId = Guid.NewGuid(),
                    name = dto.name,
                    gender = dto.gender,
                    id = dto.id,
                    phone = dto.phone,
                    company = dto.company,
                    department = dto.department,
                    CreateDate = DateTime.Now,
                    CreateID = UserContext.Current.UserId
                };
                _dbContext.Set<SM_EmployeeProfile>().Add(EmployeeProfile);
            }
            else
            {
                EmployeeProfile.name = dto.name;
                EmployeeProfile.gender = dto.gender;
                EmployeeProfile.id = dto.id;
                EmployeeProfile.phone = dto.phone;
                EmployeeProfile.company = dto.company;
                EmployeeProfile.department = dto.department;
                EmployeeProfile.ModifyDate = DateTime.Now;
                _dbContext.Set<SM_EmployeeProfile>().Update(EmployeeProfile);
            }

            // 赋值附件
            Sys_Attachment? att = null;
            if (dto.AttachmentId != Guid.Empty)
            {
                att = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o => o.BizObjectTable == "SM_EmployeeProfile" && o.DataField == "djz" && o.ObjectId == dto.AttachmentId);
                if (att != null)
                {
                    att.BizObjectId = EmployeeProfile.ObjectId.ToString();
                    _dbContext.Set<Sys_Attachment>().Update(att);

                    // Replace mode: keep only the latest registration photo for this employee profile.
                    var obsoleteAtts = _dbContext.Set<Sys_Attachment>()
                        .Where(o => o.BizObjectTable == "SM_EmployeeProfile"
                            && o.DataField == "djz"
                            && o.BizObjectId == EmployeeProfile.ObjectId.ToString()
                            && o.ObjectId != att.ObjectId)
                        .ToList();
                    if (obsoleteAtts.Count > 0)
                    {
                        var deleteAttachmentIds = obsoleteAtts.Select(o => o.ObjectId).ToList();
                        var mongoIds = obsoleteAtts.Select(o => o.MongoId)
                            .Where(o => !string.IsNullOrWhiteSpace(o))
                            .Distinct()
                            .ToList();
                        foreach (var mongoId in mongoIds)
                        {
                            var hasOther = _dbContext.Set<Sys_Attachment>()
                                .Any(o => o.MongoId == mongoId && !deleteAttachmentIds.Contains(o.ObjectId));
                            if (!hasOther)
                            {
                                _fileStorageService.DeleteFile(mongoId);
                            }
                        }
                        _dbContext.Set<Sys_Attachment>().RemoveRange(obsoleteAtts);
                    }
                }
            }
            _dbContext.SaveChanges();

            if (att != null)
            {
                try
                {
                    var attMongo = await _attachmentService.Get(att.MongoId);
                    if (attMongo.Status && attMongo.Data.fileStream != null)
                    {
                        using var stream = attMongo.Data.fileStream;
                        if (stream.CanSeek)
                            stream.Position = 0;
                        var fileName = attMongo.Data.fileName ?? att.FileName ?? "employee.jpg";
                        var contentType = att.ContentType ?? "image/jpeg";
                        await _compreFaceClient.AddFaceAsync(EmployeeProfile.id, stream, fileName, contentType, "zny", CancellationToken.None);
                    }
                }
                catch
                {
                    // Best-effort sync; ignore failures to avoid blocking save.
                }
                //var attMongo = await _attachmentService.Get(att.MongoId);
                //if (!attMongo.Status)
                //{
                //    return WebResponseContent.Instance.OK("人员档案登记成功，同步人脸库异常-获取人脸照片失败：" + attMongo.Message);
                //}
                //if (attMongo.Data.fileStream == null)
                //{
                //    return WebResponseContent.Instance.OK("人员档案登记成功，同步人脸库异常-获取人脸照片数据流失败：" + attMongo.Message);
                //}

                //face_AddUserDto faceDto = new face_AddUserDto
                //{
                //    user_id = EmployeeProfile.id,
                //    group_id = "default_face_group",
                //    user_info = JsonSerializer.Serialize(new { EmployeeProfile.name, EmployeeProfile.gender, EmployeeProfile.id, EmployeeProfile.phone }),
                //};
                //using var memoryStream = new MemoryStream();
                //attMongo.Data.fileStream.CopyTo(memoryStream);
                //byte[] imageData = memoryStream.ToArray();
                //memoryStream.Dispose();
                //attMongo.Data.fileStream.Dispose();
                //faceDto.ImageBytes = imageData;

                //var str = _faceSdkWrapper.FaceManage_AddUser(faceDto);
                //if (str == null)
                //{
                //    return WebResponseContent.Instance.OK("人员档案登记成功，同步人脸库异常-未知异常");
                //}
                //if (str.errno != 0)
                //{
                //    return WebResponseContent.Instance.OK("人员档案登记成功，同步人脸库异常：" + str.Serialize());
                //}
            }
            else
            {
                return WebResponseContent.Instance.OK("人员档案登记成功，未上传照片，不进行人脸库同步。");
            }
            return WebResponseContent.Instance.OK("人员档案登记成功", EmployeeProfile);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost]
        public WebResponseContent GetEmployeeProfileList([FromBody] PageDataOptions options)
        {
            var query = _dbContext.Set<SM_EmployeeProfile>().Where(o => !o.Deleted);
            var data = query.GetPageDataOptions(options);
            return WebResponseContent.Instance.OK("成功", data);
        }

        /// <summary>
        /// 删除人员档案
        /// </summary>
        /// <param name="EmployeeProfileId"></param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent DeleteEmployeeProfile(Guid EmployeeProfileId)
        {
            var employee = _dbContext.Set<SM_EmployeeProfile>().FirstOrDefault(o => o.ObjectId == EmployeeProfileId);
            if (employee == null)
                return WebResponseContent.Instance.Error("未找到人员档案。");

            var attachments = _dbContext.Set<Sys_Attachment>()
                .Where(o => o.BizObjectTable == "SM_EmployeeProfile" && o.BizObjectId == EmployeeProfileId.ToString())
                .ToList();

            var mongoIds = attachments.Select(o => o.MongoId)
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct()
                .ToList();
            var deleteAttachmentIds = attachments.Select(o => o.ObjectId).ToList();
            foreach (var mongoId in mongoIds)
            {
                var hasOther = _dbContext.Set<Sys_Attachment>()
                    .Any(o => o.MongoId == mongoId && !deleteAttachmentIds.Contains(o.ObjectId));
                if (!hasOther)
                {
                    _fileStorageService.DeleteFile(mongoId);
                }
            }

            _dbContext.Set<Sys_Attachment>().RemoveRange(attachments);
            _dbContext.Set<SM_EmployeeProfile>().Remove(employee);
            _dbContext.SaveChanges();
            return WebResponseContent.Instance.OK("删除成功");
        }

#endregion

    }
}
















