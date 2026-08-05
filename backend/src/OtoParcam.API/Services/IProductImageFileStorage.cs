namespace OtoParcam.API.Services;

public interface IProductImageFileStorage
{
    Task<string> SaveAsync(Guid productId, IFormFile file, CancellationToken cancellationToken = default);

    void Delete(string relativeUrl);
}
