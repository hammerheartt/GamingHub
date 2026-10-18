using System.Text.Encodings.Web;
using System.Text.Json;

namespace GamingHub.Api.Data.Paging;

public record PaginationMetadata(int TotalCount, int PageSize, int CurrentPage, int TotalPages,
                                 string? PreviousPageLink, string? NextPageLink);

public static class PaginationHeaderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Writ metadata as JSON into the pagination response header
    public static void AddPaginationHeader(this HttpResponse response, PaginationMetadata metadata)
    {
        response.Headers.Append("Pagination", JsonSerializer.Serialize(metadata, JsonOptions));
    }
}