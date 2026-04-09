using WebApi.Domain.Interfaces;

namespace WebApi.Web;

public class LoggedInUser : ICurrentUser
{
    public string GetName()
    {
        return "TODO";
    }
}
