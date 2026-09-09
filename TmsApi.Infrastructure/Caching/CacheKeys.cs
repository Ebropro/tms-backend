namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    
    // Old entries become unreachable on the next read 
    private const string SchemaVersion = "v2";
    public const string CoursesTag = "courses";
    public static string Course(int id) => $"{SchemaVersion}:course:{id}";
    public static string CoursesPage(int page, int pageSize, string? search, string orderBy, bool descending) =>
        $"{SchemaVersion}:courses:page:{page}:size:{pageSize}:search:{search ?? "none"}:sort:{orderBy}:{(descending ? "desc" : "asc")}";
}
