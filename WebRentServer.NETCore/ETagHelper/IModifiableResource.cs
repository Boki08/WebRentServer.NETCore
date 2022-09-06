namespace WebRentServer.NETCore.ETagHelper
{
    public interface IModifiableResource
    {
        string ETag { get; }
    }
}