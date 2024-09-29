using AutoMapper;
using MealSync.Application.UseCases.Dormitories.Models;
using MealSync.Application.UseCases.Buildings.Models;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Application.UseCases.Products.Models;
using MealSync.Domain.Entities;

namespace MealSync.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dormitory, DormitoryResponse>();
        CreateMap<Building, BuildingResponse>();
        CreateMap<OperatingSlot, OperatingDayResponse>();
        CreateMap<Location, LocationResponse>();
        CreateMap<ShopDormitory, ShopDormitoryResponse>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(
                    src => src.Dormitory != default ? src.Dormitory.Name : string.Empty));
        CreateMap<Shop, ShopConfigurationResponse>()
            .ForMember(dest => dest.OperatingDays,
                opt => opt.MapFrom(
                    src => src.OperatingSlots))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(
                    src => src.Location))
            .ForMember(dest => dest.ShopDormitoryies,
                opt => opt.MapFrom(
                    src => src.ShopDormitories));
        // CreateMap<Product, ProductDetailResponse>()
        //     .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ProductCategories))
        //     .ForMember(dest => dest.OperatingHours, opt => opt.MapFrom(src => src.ProductOperatingHours))
        //     .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.ProductQuestions));
        // CreateMap<ProductCategory, ProductDetailResponse.CategoryResponse>()
        //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
        //     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category.Name));
        // CreateMap<ProductOperatingHour, ProductDetailResponse.OperatingHourResponse>();
        // CreateMap<ProductVariant, ProductDetailResponse.QuestionResponse>()
        //     .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.ProductQuestionOptions));
        // CreateMap<ProductVariantOption, ProductDetailResponse.OptionResponse>();
    }
}