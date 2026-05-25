using AutoMapper;
using InvenTrack.Application.DTOs.Auth;
using InvenTrack.Application.DTOs.Categories;
using InvenTrack.Application.DTOs.Import;
using InvenTrack.Application.DTOs.Products;
using InvenTrack.Application.DTOs.StockMovements;
using InvenTrack.Application.DTOs.Suppliers;
using InvenTrack.Domain.Entities;

namespace InvenTrack.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, o => o.Ignore());

        CreateMap<CreateCategoryDto, Category>();

        CreateMap<Supplier, SupplierDto>()
            .ForMember(d => d.ProductCount, o => o.Ignore());

        CreateMap<CreateSupplierDto, Supplier>();

        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductSku, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.Username))
            .ForMember(d => d.MovementType, o => o.MapFrom(s => s.MovementType.ToString()));

        CreateMap<ImportJob, ImportJobDto>()
            .ForMember(d => d.FileType, o => o.MapFrom(s => s.FileType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.Username));

        CreateMap<User, UserInfoDto>();
    }
}
