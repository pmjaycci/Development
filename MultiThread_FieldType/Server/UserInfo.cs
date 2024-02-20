using System.Net.Sockets;

public class UserInfo
{
    public TcpClient? client { get; set; }
    public string? name { get; set; }
    public int fieldId { get; set; }
    public int state { get; set; }
    public string? message { get; set; }
}