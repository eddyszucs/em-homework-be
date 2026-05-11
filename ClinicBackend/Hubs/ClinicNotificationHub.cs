using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ClinicBackend.Hubs;

[Authorize]
public class ClinicNotificationHub : Hub
{
    public async Task JoinRoleGroup(string role)
    {
        Console.WriteLine($"[Hub] JoinRoleGroup: connectionId={Context.ConnectionId}, role={role}, user={Context.User?.Identity?.Name}, ident={Context.UserIdentifier}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}Group");
        Console.WriteLine($"[Hub] Added to group: {role}Group");
    }

    public async Task JoinDoctorGroup(Guid doctorId)
    {
        Console.WriteLine($"[Hub] JoinDoctorGroup: connectionId={Context.ConnectionId}, doctorId={doctorId}, user={Context.User?.Identity?.Name}, ident={Context.UserIdentifier}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"DoctorGroup_{doctorId}");
        Console.WriteLine($"[Hub] Added to group: DoctorGroup_{doctorId}");
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Hub] OnConnectedAsync: connectionId={Context.ConnectionId}, user={Context.User?.Identity?.Name}, ident={Context.UserIdentifier}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[Hub] OnDisconnectedAsync: connectionId={Context.ConnectionId}, exception={exception?.Message}");
        await base.OnDisconnectedAsync(exception);
    }
}
