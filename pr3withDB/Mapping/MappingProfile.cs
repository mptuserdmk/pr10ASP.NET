using AutoMapper;
using pr3withDB.DTOs;
using pr3withDB.Models;

namespace pr3withDB.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdProducts))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // Category mappings
            CreateMap<Category, CategoryReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdCategories))
                .ForMember(dest => dest.ProductsCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdOrderItems))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty));
            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.OrderItemQuantity, opt => opt.MapFrom(src => src.Quantity));
            CreateMap<CreateOrderItemFullDto, OrderItem>();
            CreateMap<UpdateOrderItemDto, OrderItem>();

            // Order mappings
            CreateMap<Order, OrderReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdOrders))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(dest => dest.UserLogin, opt => opt.MapFrom(src => src.User != null ? src.User.UserLogin : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<CreateOrderDto, Order>();
            CreateMap<CreateOrderFlatDto, Order>();

            // User mappings
            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdUsers))
                .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.UserLogin))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.UserCreatedAt));

            // Role mappings
            CreateMap<Role, RoleReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdRoles));
        }
    }
}
