namespace OtoParcam.API.Services;

public class LocalProductImageFileStorage : IProductImageFileStorage
{
    private const string UrlPrefix = "/uploads/products/";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    private readonly string _uploadsRoot;

    public LocalProductImageFileStorage(IWebHostEnvironment env)
    {
        _uploadsRoot = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", "products");
    }

    public async Task<string> SaveAsync(Guid productId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("Image file must be between 1 byte and 5 MB.");
        }

        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var extension))
        {
            throw new InvalidOperationException("Only JPEG, PNG, and WebP images are allowed.");
        }

        var productFolder = Path.Combine(_uploadsRoot, productId.ToString());
        Directory.CreateDirectory(productFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(productFolder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"{UrlPrefix}{productId}/{fileName}";
    }

    public void Delete(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith(UrlPrefix, StringComparison.Ordinal))
        {
            // Not a locally-stored file (e.g. an external URL from seed data) — nothing to delete.
            return;
        }

        var remainder = relativeUrl[UrlPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_uploadsRoot, remainder);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; the DB row is already gone, an orphaned file is not fatal.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
