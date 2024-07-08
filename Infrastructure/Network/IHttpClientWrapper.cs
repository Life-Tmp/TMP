namespace TMPInfrastructure.Network
{
    public interface IHttpClientWrapper
    {
        void Post(string address, string json);
    }
}