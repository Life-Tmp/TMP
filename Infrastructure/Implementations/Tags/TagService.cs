using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces;
using TMP.Application.Interfaces.Tags;
using TMPApplication.Interfaces;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations.Tags
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISearchService<TagDto> _searchService;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper, ISearchService<TagDto> searchService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _searchService = searchService;
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var tags = await _unitOfWork.Repository<Tag>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }

        public async Task<TagDto> GetTagByIdAsync(int id)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null) return null;

            return _mapper.Map<TagDto>(tag);
        }

        public async Task<TagDto> AddTagAsync(AddTagDto newTag)
        {
            var tag = _mapper.Map<Tag>(newTag);

            _unitOfWork.Repository<Tag>().Create(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();

            var tagDto = _mapper.Map<TagDto>(tag);
            await _searchService.IndexDocumentAsync(tagDto, "tags");

            return tagDto;
        }

        public async Task<bool> UpdateTagAsync(int id, AddTagDto updatedTag)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null) return false;

            _mapper.Map(updatedTag, tag);
            _unitOfWork.Repository<Tag>().Update(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();
            await _searchService.IndexDocumentAsync(_mapper.Map<TagDto>(tag), "tags");
            return true;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null) return false;

            _unitOfWork.Repository<Tag>().Delete(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();

            await _searchService.DeleteDocumentAsync(id.ToString(), "tags");

            return true;
        }
    }
}
