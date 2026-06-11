using Stocker.Core.Domain;

namespace Stocker.Features.UserProfile;

public class UserProfile : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Auth0Sub { get; set; }
    public string Image { get; set; }
    public string Nickname { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime DeletedAt { get; set; }
}