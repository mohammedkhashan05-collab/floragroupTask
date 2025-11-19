using AutoMapper;
using FileStorage.Application.DTOs;
using FileStorage.Domain.Entities;

namespace FileStorage.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FileMetadata, FileResponseDto>();
    }
}
