using AutoMapper;
using BookAPI.DTOs;
using BookAPI.Entity;

namespace BookAPI.Profiles
{
	public class AuthorProfile : Profile
	{
		public AuthorProfile() 
		{ 
			CreateMap<Author, AuthorDto>(); 

			CreateMap<CreateAuthorDto, Author>(); 

			CreateMap<UpdateAuthorDto, Author>(); 
		}
	}
}
