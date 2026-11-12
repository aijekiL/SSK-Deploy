using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        private readonly ILogger<StaffInfoController> _logger;
        public StaffInfoController(VenusDbContext dbContext, IAttachmentService attachmentService, CompreFaceClient compreFaceClient, FileStorageService fileStorageService, ILogger<StaffInfoController> logger)
        {
            _dbContext = dbContext;
            _attachmentService = attachmentService;
            _compreFaceClient = compreFaceClient;
            _fileStorageService = fileStorageService;
            _logger = logger;
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
            /// 手表IMEI
            /// </summary>
            public string WatchImei { get; set; } = string.Empty;

            /// <summary>
            /// 附件ID
            /// </summary>
            public Guid AttachmentId { get; set; }

            /// <summary>
            /// 已关联的人员信息台账ID
            /// </summary>
            public Guid? EmployeeProfileId { get; set; }

            /// <summary>
            /// 是否将本次登记同步到人员信息台账及人脸库
            /// </summary>
            public bool SyncEmployeeProfile { get; set; }

            /// <summary>
            /// 同步台账时是否同时替换台账登记照并刷新人脸库；为 false 时仅同步文字信息（台账无照片时仍会补挂）
            /// </summary>
            public bool SyncProfilePhoto { get; set; } = true;
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<WebResponseContent> ConfirmStaffInfo([FromBody] SaveStaffInfoDto dto, CancellationToken cancellationToken)
        {
            var normalizedName = dto.name?.Trim() ?? string.Empty;
            var normalizedPhone = dto.phone?.Trim() ?? string.Empty;
            var normalizedId = dto.id?.Trim() ?? string.Empty;
            var normalizedCompany = dto.company?.Trim() ?? string.Empty;
            var normalizedDepartment = dto.department?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return WebResponseContent.Instance.Error("姓名不能为空。");
            }

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                return WebResponseContent.Instance.Error("联系电话不能为空。");
            }

            if (!Regex.IsMatch(normalizedPhone, @"^(1[3-9]\d{9}|0\d{2,3}-?\d{7,8})$"))
            {
                return WebResponseContent.Instance.Error("联系电话格式不正确。");
            }

            if (string.IsNullOrWhiteSpace(normalizedCompany) && string.IsNullOrWhiteSpace(normalizedDepartment))
            {
                return WebResponseContent.Instance.Error("工作单位和工作部门至少填写一项。");
            }

            if (dto.AttachmentId == Guid.Empty)
            {
                return WebResponseContent.Instance.Error("登记照不能为空。");
            }

            dto.name = normalizedName;
            dto.phone = normalizedPhone;
            dto.id = normalizedId;
            dto.company = normalizedCompany;
            dto.department = normalizedDepartment;

            var staffPhoto = _dbContext.Set<Sys_Attachment>().FirstOrDefault(o =>
                o.BizObjectTable == "SM_StaffInfo" &&
                o.DataField == "djz" &&
                o.ObjectId == dto.AttachmentId);
            if (staffPhoto == null)
            {
                return WebResponseContent.Instance.Error("未找到登记照，请重新拍摄或上传。");
            }

            // 判断该用户是否已在场（仅检查状态为2的记录，表示已完成登记并入场）
            if (!string.IsNullOrWhiteSpace(normalizedId))
            {
                var checkData = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.ProjectId == dto.ParentId &&
                    o.id == normalizedId &&
                    o.Status == 2);
                if (checkData != null)
                {
                    return WebResponseContent.Instance.Error("该人员已在场。");
                }

                var duplicateId = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.ProjectId == dto.ParentId &&
                    o.id == normalizedId &&
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
            }

            if (!string.IsNullOrWhiteSpace(dto.Device1) && dto.Device1 != "未绑定")
            {
                var duplicateDevice = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.ProjectId == dto.ParentId &&
                    o.Device1 == dto.Device1 &&
                    o.ObjectId != dto.ObjectId &&
                    o.Status == 2);
                if (duplicateDevice != null)
                {
                    return WebResponseContent.Instance.Error("设备已绑定。");
                }
            }

            var normalizedWatchImei = string.IsNullOrWhiteSpace(dto.WatchImei) ? null : dto.WatchImei.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedWatchImei))
            {
                var duplicateWatch = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.ProjectId == dto.ParentId &&
                    o.WatchImei == normalizedWatchImei &&
                    o.ObjectId != dto.ObjectId &&
                    o.Status != 0);
                if (duplicateWatch != null)
                {
                    return WebResponseContent.Instance.Error("手表IMEI已绑定。");
                }
            }

            SM_StaffInfo? staffInfo = null;
            SM_EmployeeProfile? employeeProfile = null;
            var obsoleteProfileMongoIds = new List<string>();
            var profilePhotoSynced = false;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
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
                        WatchImei = normalizedWatchImei,
                        Status = 1,
                        CreateDate = DateTime.Now,
                        CreateID = UserContext.Current.UserId,
                        CurrentStep = 1
                    };
                    _dbContext.Set<SM_StaffInfo>().Add(staffInfo);
                }
                else
                {
                    staffInfo.ProjectId = dto.ParentId;
                    staffInfo.name = dto.name;
                    staffInfo.gender = dto.gender;
                    if (!string.IsNullOrWhiteSpace(normalizedId) || string.IsNullOrWhiteSpace(staffInfo.id))
                    {
                        staffInfo.id = normalizedId;
                    }
                    staffInfo.phone = dto.phone;
                    staffInfo.company = dto.company;
                    staffInfo.department = dto.department;
                    staffInfo.Device1 = dto.Device1;
                    staffInfo.WatchImei = normalizedWatchImei;
                    if (staffInfo.Status != 2)
                    {
                        staffInfo.Status = 1;
                    }
                    staffInfo.ModifyDate = DateTime.Now;
                    staffInfo.ModifyID = UserContext.Current.UserId;

                    if (staffInfo.CurrentStep < 1)
                    {
                        staffInfo.CurrentStep = 1;
                    }

                    _dbContext.Set<SM_StaffInfo>().Update(staffInfo);
                }

                staffPhoto.BizObjectId = staffInfo.ObjectId.ToString();
                staffPhoto.ModifyDate = DateTime.Now;
                staffPhoto.ModifyID = UserContext.Current.UserId;
                _dbContext.Set<Sys_Attachment>().Update(staffPhoto);

                if (dto.SyncEmployeeProfile)
                {
                    var profileId = dto.EmployeeProfileId ?? staffInfo.EmployeeProfileId;
                    if (profileId.HasValue)
                    {
                        employeeProfile = _dbContext.Set<SM_EmployeeProfile>()
                            .FirstOrDefault(x => x.ObjectId == profileId.Value && !x.Deleted);
                        if (employeeProfile == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return WebResponseContent.Instance.Error("关联的人员信息台账不存在。");
                        }
                    }

                    if (employeeProfile == null)
                    {
                        employeeProfile = new SM_EmployeeProfile
                        {
                            ObjectId = Guid.NewGuid(),
                            CreateDate = DateTime.Now,
                            CreateID = UserContext.Current.UserId
                        };
                        _dbContext.Set<SM_EmployeeProfile>().Add(employeeProfile);
                    }
                    else
                    {
                        employeeProfile.ModifyDate = DateTime.Now;
                        employeeProfile.ModifyID = UserContext.Current.UserId;
                        _dbContext.Set<SM_EmployeeProfile>().Update(employeeProfile);
                    }

                    employeeProfile.name = dto.name;
                    employeeProfile.gender = dto.gender;
                    employeeProfile.phone = dto.phone;
                    employeeProfile.company = dto.company;
                    employeeProfile.department = dto.department;
                    employeeProfile.Device1 = NormalizeProfileDeviceId(dto.Device1);
                    employeeProfile.WatchImei = normalizedWatchImei;
                    employeeProfile.projectId = dto.ParentId;
                    staffInfo.EmployeeProfileId = employeeProfile.ObjectId;

                    var oldProfilePhotos = _dbContext.Set<Sys_Attachment>()
                        .Where(o => o.BizObjectTable == "SM_EmployeeProfile" &&
                            o.DataField == "djz" &&
                            o.BizObjectId == employeeProfile.ObjectId.ToString())
                        .ToList();
                    profilePhotoSynced = dto.SyncProfilePhoto || oldProfilePhotos.Count == 0;
                    if (profilePhotoSynced)
                    {
                        employeeProfile.FaceSyncStatus = 0;
                        employeeProfile.FaceSyncMessage = "等待同步";
                        obsoleteProfileMongoIds = oldProfilePhotos
                            .Select(o => o.MongoId)
                            .Where(o => !string.IsNullOrWhiteSpace(o) && o != staffPhoto.MongoId)
                            .Distinct()
                            .ToList();
                        if (oldProfilePhotos.Count > 0)
                        {
                            _dbContext.Set<Sys_Attachment>().RemoveRange(oldProfilePhotos);
                        }

                        var profilePhoto = new Sys_Attachment
                        {
                            ObjectId = Guid.NewGuid(),
                            BizObjectId = employeeProfile.ObjectId.ToString(),
                            BizObjectTable = "SM_EmployeeProfile",
                            DataField = "djz",
                            FileName = staffPhoto.FileName,
                            ContentType = staffPhoto.ContentType,
                            ContentLength = staffPhoto.ContentLength,
                            FileExt = staffPhoto.FileExt,
                            MongoId = staffPhoto.MongoId,
                            CreateDate = DateTime.Now,
                            CreateID = UserContext.Current.UserId
                        };
                        _dbContext.Set<Sys_Attachment>().Add(profilePhoto);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "保存人员登记及同步人员台账失败，AttachmentId={AttachmentId}", dto.AttachmentId);
                return WebResponseContent.Instance.Error("人员信息保存失败，请稍后重试。");
            }

            foreach (var mongoId in obsoleteProfileMongoIds)
            {
                var hasOtherReference = _dbContext.Set<Sys_Attachment>().Any(o => o.MongoId == mongoId);
                if (!hasOtherReference)
                {
                    try
                    {
                        _fileStorageService.DeleteFile(mongoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "清理已替换的人员台账照片失败，MongoId={MongoId}", mongoId);
                    }
                }
            }

            bool? facePhotoSynced = null;
            var faceSyncMessage = string.Empty;
            if (employeeProfile != null && profilePhotoSynced)
            {
                var faceSyncResult = await SyncEmployeeProfileFaceAsync(employeeProfile, staffPhoto, cancellationToken);
                facePhotoSynced = faceSyncResult.Success;
                faceSyncMessage = faceSyncResult.Message;
            }

            var message = facePhotoSynced == false
                ? "确认成功，人员台账照片已同步，但人脸库同步失败。"
                : "确认成功";
            return WebResponseContent.Instance.OK(message, new
            {
                objectId = staffInfo!.ObjectId,
                employeeProfileId = employeeProfile?.ObjectId ?? staffInfo.EmployeeProfileId,
                profilePhotoSynced,
                facePhotoSynced,
                faceSyncMessage
            });
        }

        /// <summary>
        /// 人脸同步重试参数
        /// </summary>
        public class RetryEmployeeProfileFaceSyncDto
        {
            /// <summary>
            /// 人员信息台账ID
            /// </summary>
            public Guid EmployeeProfileId { get; set; }
        }

        /// <summary>
        /// 使用人员台账当前登记照重试人脸库同步
        /// </summary>
        [HttpPost]
        public async Task<WebResponseContent> RetryEmployeeProfileFaceSync(
            [FromBody] RetryEmployeeProfileFaceSyncDto dto,
            CancellationToken cancellationToken)
        {
            var employeeProfile = _dbContext.Set<SM_EmployeeProfile>()
                .FirstOrDefault(o => o.ObjectId == dto.EmployeeProfileId && !o.Deleted);
            if (employeeProfile == null)
            {
                return WebResponseContent.Instance.Error("人员信息台账不存在。");
            }

            var profilePhoto = _dbContext.Set<Sys_Attachment>()
                .Where(o => o.BizObjectTable == "SM_EmployeeProfile" &&
                    o.DataField == "djz" &&
                    o.BizObjectId == employeeProfile.ObjectId.ToString())
                .OrderByDescending(o => o.CreateDate)
                .FirstOrDefault();
            if (profilePhoto == null)
            {
                return WebResponseContent.Instance.Error("人员信息台账没有可同步的登记照。");
            }

            var result = await SyncEmployeeProfileFaceAsync(employeeProfile, profilePhoto, cancellationToken);
            return result.Success
                ? WebResponseContent.Instance.OK(result.Message, new
                {
                    employeeProfileId = employeeProfile.ObjectId,
                    facePhotoSynced = true,
                    faceSyncMessage = result.Message
                })
                : WebResponseContent.Instance.Error(result.Message);
        }

        private async Task<(bool Success, string Message)> SyncEmployeeProfileFaceAsync(
            SM_EmployeeProfile employeeProfile,
            Sys_Attachment photo,
            CancellationToken cancellationToken)
        {
            var oldFaceImageId = employeeProfile.FaceImageId;
            var success = false;
            var message = string.Empty;
            try
            {
                var attachmentResult = await _attachmentService.Get(photo.MongoId);
                if (!attachmentResult.Status || attachmentResult.Data.fileStream == null)
                {
                    throw new InvalidOperationException("无法读取登记照文件。");
                }

                await using var stream = attachmentResult.Data.fileStream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                var fileName = attachmentResult.Data.fileName ?? photo.FileName ?? "employee.jpg";
                var contentType = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType;
                var faceResult = await _compreFaceClient.AddFaceAsync(
                    employeeProfile.ObjectId.ToString(),
                    stream,
                    fileName,
                    contentType,
                    "zny",
                    cancellationToken);
                if (string.IsNullOrWhiteSpace(faceResult?.image_id))
                {
                    throw new InvalidOperationException("人脸库未返回图片ID。");
                }

                employeeProfile.FaceImageId = faceResult.image_id;
                employeeProfile.FaceSyncStatus = 1;
                employeeProfile.FaceSyncMessage = "同步成功";
                success = true;
                message = "同步成功";

                if (!string.IsNullOrWhiteSpace(oldFaceImageId) && oldFaceImageId != faceResult.image_id)
                {
                    try
                    {
                        await _compreFaceClient.DeleteFaceAsync(oldFaceImageId, "zny", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        message = "新照片同步成功，旧人脸照片清理失败。";
                        employeeProfile.FaceSyncMessage = message;
                        _logger.LogWarning(ex, "清理旧人脸照片失败，ProfileId={ProfileId}, ImageId={ImageId}", employeeProfile.ObjectId, oldFaceImageId);
                    }
                }
            }
            catch (Exception ex)
            {
                message = "人脸库同步失败，请稍后重试。";
                employeeProfile.FaceSyncStatus = 2;
                employeeProfile.FaceSyncMessage = message;
                _logger.LogError(ex, "同步人员登记照到人脸库失败，ProfileId={ProfileId}", employeeProfile.ObjectId);
            }

            employeeProfile.ModifyDate = DateTime.Now;
            employeeProfile.ModifyID = UserContext.Current.UserId;
            _dbContext.Set<SM_EmployeeProfile>().Update(employeeProfile);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存人脸同步状态失败，ProfileId={ProfileId}", employeeProfile.ObjectId);
            }

            return (success, message);
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

            // 同步更新进出登记日志绑定的设备（作业台账气体数据按设备取历史）
            var inOutLog = _dbContext.Set<SM_StaffInOutLog>().FirstOrDefault(x => x.StaffInfoId == StaffId);
            if (inOutLog != null)
            {
                inOutLog.Device1 = string.IsNullOrWhiteSpace(Device) ? null : Device.Trim();
                inOutLog.ModifyID = UserContext.Current.UserId;
                inOutLog.ModifyDate = DateTime.Now;
                _dbContext.Set<SM_StaffInOutLog>().Update(inOutLog);
            }

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
                o.WatchImei,
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

            // 回填进出登记日志的退场时间（作业台账数据源）
            var inOutLog = _dbContext.Set<SM_StaffInOutLog>()
                .FirstOrDefault(x => x.StaffInfoId == StaffId);
            if (inOutLog != null)
            {
                inOutLog.ExitTime = DateTime.Now;
                inOutLog.ModifyID = UserContext.Current.UserId;
                inOutLog.ModifyDate = DateTime.Now;
                _dbContext.Set<SM_StaffInOutLog>().Update(inOutLog);
            }

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
        /// <param name="RequireSignature">移动端入场时是否要求安全交底签名</param>
        /// <returns></returns>
        [HttpGet]
        public WebResponseContent CompleteRegistration(Guid StaffId, bool RequireSignature = false)
        {
            var staff = _dbContext.Set<SM_StaffInfo>().FirstOrDefault(o => !o.Deleted && o.ObjectId == StaffId);
            if (staff == null)
                return WebResponseContent.Instance.Error("不存在该员工。");

            if (RequireSignature)
            {
                var hasSafetyDisclosureSignature = _dbContext.Set<Sys_Attachment>().Any(o =>
                    !o.Deleted &&
                    o.BizObjectTable == "SM_StaffInfo" &&
                    o.DataField == "safetyDisclosureSignature" &&
                    o.BizObjectId == StaffId.ToString());
                if (!hasSafetyDisclosureSignature)
                    return WebResponseContent.Instance.Error("请先完成安全交底手写签名。");
            }

            // 将员工状态设置为在场，表示完成登记并正式入场
            staff.Status = 2;
            staff.CurrentStep = Math.Max(staff.CurrentStep, 3);
            _dbContext.Set<SM_StaffInfo>().Update(staff);

            // 记录进出登记日志（作业台账数据源），同一次入场登记只记录一次
            var inOutLog = _dbContext.Set<SM_StaffInOutLog>()
                .FirstOrDefault(x => x.StaffInfoId == staff.ObjectId);
            if (inOutLog == null)
            {
                var tools = _dbContext.Set<SM_StaffTool>()
                    .Where(o => !o.Deleted && o.ParentId == staff.ObjectId)
                    .Select(o => new { o.ToolName, o.ToolNumber })
                    .ToList();
                var toolSummary = string.Join("、", tools.Select(t => $"{t.ToolName}×{t.ToolNumber}"));
                _dbContext.Set<SM_StaffInOutLog>().Add(new SM_StaffInOutLog
                {
                    ObjectId = Guid.NewGuid(),
                    ProjectId = staff.ProjectId,
                    StaffInfoId = staff.ObjectId,
                    Name = staff.name,
                    Phone = staff.phone,
                    Company = staff.company,
                    Department = staff.department,
                    Tools = toolSummary,
                    Device1 = string.IsNullOrWhiteSpace(staff.Device1) ? null : staff.Device1.Trim(),
                    EnterTime = DateTime.Now,
                    CreateID = UserContext.Current.UserId,
                    CreateDate = DateTime.Now
                });
            }

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
            /// 设备Id（智能安全帽）
            /// </summary>
            public string? Device1 { get; set; }

            /// <summary>
            /// 手表IMEI
            /// </summary>
            public string? WatchImei { get; set; }

            /// <summary>
            /// 附件ID
            /// </summary>
            public Guid AttachmentId { get; set; }
        }

        private static string? NormalizeProfileDeviceId(string? deviceId)
        {
            var normalized = deviceId?.Trim() ?? string.Empty;
            return string.IsNullOrEmpty(normalized) || normalized == "未绑定" ? null : normalized;
        }

        /// <summary>
        /// 人员档案登记
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<WebResponseContent> SaveEmployeeProfileAsync([FromBody] SaveEmployeeProfileDto dto)
        {
            var normalizedId = dto.id?.Trim() ?? string.Empty;
            dto.id = normalizedId;

            // 判断该用户是否已存在
            if (!string.IsNullOrWhiteSpace(normalizedId))
            {
                var checkData = _dbContext.Set<SM_EmployeeProfile>().FirstOrDefault(o =>
                    !o.Deleted &&
                    o.id == normalizedId &&
                    o.ObjectId != dto.ObjectId);
                if (checkData != null)
                {
                    return WebResponseContent.Instance.Error($"人员身份已注册：[{checkData.name}][{checkData.id}]。");
                }
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
                    Device1 = NormalizeProfileDeviceId(dto.Device1),
                    WatchImei = string.IsNullOrWhiteSpace(dto.WatchImei) ? null : dto.WatchImei.Trim(),
                    CreateDate = DateTime.Now,
                    CreateID = UserContext.Current.UserId
                };
                _dbContext.Set<SM_EmployeeProfile>().Add(EmployeeProfile);
            }
            else
            {
                EmployeeProfile.name = dto.name;
                EmployeeProfile.gender = dto.gender;
                if (!string.IsNullOrWhiteSpace(normalizedId) || string.IsNullOrWhiteSpace(EmployeeProfile.id))
                {
                    EmployeeProfile.id = normalizedId;
                }
                EmployeeProfile.phone = dto.phone;
                EmployeeProfile.company = dto.company;
                EmployeeProfile.department = dto.department;
                EmployeeProfile.Device1 = NormalizeProfileDeviceId(dto.Device1);
                EmployeeProfile.WatchImei = string.IsNullOrWhiteSpace(dto.WatchImei) ? null : dto.WatchImei.Trim();
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

            // 台账登记照更新后，同步刷新该人员在场记录（SM_StaffInfo/djz）的附件指向，保证首页显示最新登记照
            if (att != null)
            {
                var onSiteStaffIds = _dbContext.Set<SM_StaffInfo>()
                    .Where(o => !o.Deleted && o.EmployeeProfileId == EmployeeProfile.ObjectId && o.Status == 2)
                    .Select(o => o.ObjectId.ToString())
                    .ToList();
                if (onSiteStaffIds.Count > 0)
                {
                    var staffPhotos = _dbContext.Set<Sys_Attachment>()
                        .Where(o => o.BizObjectTable == "SM_StaffInfo"
                            && o.DataField == "djz"
                            && onSiteStaffIds.Contains(o.BizObjectId))
                        .ToList();
                    foreach (var staffPhoto in staffPhotos)
                    {
                        staffPhoto.MongoId = att.MongoId;
                        staffPhoto.FileName = att.FileName;
                        staffPhoto.ContentType = att.ContentType;
                        staffPhoto.ContentLength = att.ContentLength;
                        staffPhoto.FileExt = att.FileExt;
                        staffPhoto.ModifyDate = DateTime.Now;
                        _dbContext.Set<Sys_Attachment>().Update(staffPhoto);
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
                        await _compreFaceClient.AddFaceAsync(EmployeeProfile.ObjectId.ToString(), stream, fileName, contentType, "zny", CancellationToken.None);
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
















