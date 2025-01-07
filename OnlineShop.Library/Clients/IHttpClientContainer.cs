namespace OnlineShop.Library.Clients;

public interface IHttpClientContainer
{
    public HttpClient HttpClient { get; }
}