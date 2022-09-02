using AutoMapper;
using BookStore.Application.Contracts.Books.Dtos;

namespace BookStore.Blazor;

public class BookStoreBlazorAutoMapperProfile : Profile
{
    public BookStoreBlazorAutoMapperProfile()
    {
       CreateMap<BookDto, CreateUpdateBookDto>();
    }
}
