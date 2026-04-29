using Microsoft.AspNetCore.SignalR;

namespace WEBDULICH.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }

        public async Task SendPrivateMessage(string toUserId, string message)
        {
            await Clients.User(toUserId).SendAsync("ReceivePrivateMessage", Context.ConnectionId, message, DateTime.Now);
        }

        public async Task JoinChatRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoinedRoom", Context.ConnectionId, roomId);
        }

        public async Task LeaveChatRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeftRoom", Context.ConnectionId, roomId);
        }

        public async Task SendMessageToRoom(string roomId, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveRoomMessage", Context.ConnectionId, message, DateTime.Now);
        }

        public async Task UserTyping(string roomId)
        {
            await Clients.OthersInGroup(roomId).SendAsync("UserIsTyping", Context.ConnectionId);
        }

        public async Task UserStoppedTyping(string roomId)
        {
            await Clients.OthersInGroup(roomId).SendAsync("UserStoppedTyping", Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
