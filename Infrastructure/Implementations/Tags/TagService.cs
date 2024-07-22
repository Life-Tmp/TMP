using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces;
using TMP.Application.Interfaces.Tags;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations.Tags
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            return _mapper.Map<TagDto>(tag);
        }

        public async Task<bool> UpdateTagAsync(int id, AddTagDto updatedTag)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null) return false;

            _mapper.Map(updatedTag, tag);
            _unitOfWork.Repository<Tag>().Update(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null) return false;

            _unitOfWork.Repository<Tag>().Delete(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();
            return true;
        }
    }
}
