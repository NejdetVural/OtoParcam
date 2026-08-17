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

        // The declared Content-Type is client-supplied and can be spoofed (e.g. an HTML/JS payload
        // labeled image/png) — verify the actual file bytes match a real image signature before
        // trusting it, since this ends up served back to browsers from wwwroot/uploads.
        if (!await HasValidSignatureAsync(file, file.ContentType, cancellationToken))
        {
            throw new InvalidOperationException("The file's content does not match a valid JPEG, PNG, or WebP image.");
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

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string contentType, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var totalRead = 0;
        while (totalRead < header.Length)
        {
            var read = await stream.ReadAsync(header.AsMemory(totalRead, header.Length - totalRead), cancellationToken);
            if (read == 0)
            {
                break;
            }
            totalRead += read;
        }

        if (totalRead < header.Length)
        {
            return false;
        }

        return contentType switch
        {
            "image/jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            "image/webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };
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
