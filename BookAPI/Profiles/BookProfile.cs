using AutoMapper;
using BookAPI.DTOs;
using BookAPI.Entity;

namespace BookAPI.Profiles
{
	public class BookProfile : Profile
	{
		public BookProfile() 
		{ 
			CreateMap<Book, BookDto>()
			.ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Name)); 
		
			CreateMap<CreateBookDto, Book>(); 
			
			CreateMap<UpdateBookDto, Book>(); 
		}
	}
}
