using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IGenericRepository<Product> _productsRepo;
    private readonly IGenericRepository<ProductBrand> _productBrandsRepo;
    private readonly IGenericRepository<ProductType> _productTypesRepo;
    private readonly IMapper _mapper;


    public ProductsController(IGenericRepository<Product> productsRepo,
        IGenericRepository<ProductBrand> productBrandsRepo, IGenericRepository<ProductType> productTypesRepo, IMapper mapper)
    {
        _productsRepo = productsRepo;
        _productBrandsRepo = productBrandsRepo;
        _productTypesRepo = productTypesRepo;
        _mapper = mapper;
    }

    [HttpGet]
    //[FromQuery] - kiedy parametrem jest object to api domyslnie próbuje wyszukac parametrów w body, ale w GET nie ma body
    //dodanie [FromQuery] wymusza szukania parametrów w query
    public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery]ProductSpecParams productParams)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(productParams);

        var countSpec = new ProductsWithFiltersForCountSpecification(productParams);

        var totalItems = await _productsRepo.CountAsync(countSpec);
        
        var products = await _productsRepo.ListAsync(spec);

        var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
        return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems, data));
    }

    [HttpGet("{id}")]
    //info dla swaggera o typach
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(id);
        var product = await _productsRepo.GetEntityWithSpecAsync(spec);
        if (product == null) return NotFound(new ApiResponse(404));
        return _mapper.Map<Product, ProductToReturnDto>(product);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
    {
        return Ok(await _productBrandsRepo.ListAllAsync());
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
    {
        return Ok(await _productTypesRepo.ListAllAsync());
    }
}