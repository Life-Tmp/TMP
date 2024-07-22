using System.Collections.Generic;
using System.Threading.Tasks;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.DTOs.TaskDtos;

namespace TMP.Application.Interfaces.Tags
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<TagDto> GetTagByIdAsync(int id);
        Task<TagDto> AddTagAsync(AddTagDto newTag);
        Task<bool> UpdateTagAsync(int id, AddTagDto updatedTag);
        Task<bool> DeleteTagAsync(int id);
    }
}
