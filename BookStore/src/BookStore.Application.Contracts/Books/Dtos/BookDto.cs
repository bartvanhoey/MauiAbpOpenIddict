using System;
using BookStore.Domain.Shared.Books;
using Volo.Abp.Application.Dtos;

namespace BookStore.Application.Contracts.Books.Dtos
{
    public class BookDto :  AuditedEntityDto<Guid>
    {
     public string Name { get; set; }

        public BookType Type { get; set; }

        public DateTime PublishDate { get; set; }

        public float Price { get; set; }
    }

    
}
