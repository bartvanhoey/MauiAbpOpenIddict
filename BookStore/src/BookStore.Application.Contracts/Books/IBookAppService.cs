using System;
using BookStore.Application.Contracts.Books.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BookStore.Application.Contracts.Books
{
    public interface IBookAppService : ICrudAppService<BookDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateBookDto>, IApplicationService
    {
        
    }
}
