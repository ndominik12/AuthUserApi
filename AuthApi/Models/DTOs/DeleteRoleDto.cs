namespace AuthApi.Models.DTOs
{
    public class DeleteRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
