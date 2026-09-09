using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces
{
    public interface IStudentService
    {
        // Normal queries — HasQueryFilter hides IsDeleted students automatically
        Task<PagedResponse<StudentResponseDto>> GetAllAsync(PagedRequest request, CancellationToken ct);
        Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
        Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<StudentResponseDto>> GetAllIncludingDeletedAsync(CancellationToken ct);
        Task<bool> RestoreAsync(int id, CancellationToken ct);
        Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct);
        // Authentication → Student mapping
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct);


    }
}