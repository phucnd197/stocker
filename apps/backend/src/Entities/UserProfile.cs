namespace Stocker.Entities;

public class UserProfile : ISoftDeletable
{
    public Guid UserId { get; set; }
    public string Image { get; set; }
    public string Nickname { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime DeletedAt { get; set; }
}