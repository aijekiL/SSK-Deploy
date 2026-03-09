using System.ComponentModel.DataAnnotations;

namespace Venus.Dto.User
{
    public class AddUserDto : UpdateUserDto
    {
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "用户登录名不能为空")]
        public string LoginName { get; set; } = string.Empty;

        [Required(ErrorMessage = "用户名不能为空")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "性别不能为空")]
        public string Sex { get; set; } = string.Empty;
    }

    public class DeleteUserDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public Guid UserId { get; set; }
    }
}

