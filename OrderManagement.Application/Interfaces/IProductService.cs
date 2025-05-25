using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken);
        Task<ProductDto?> GetByIdAsync(long productId, CancellationToken cancellationToken);
        Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> UpdateProductAsync(long productId, ProductDto productDto, CancellationToken cancellationToken);
        Task<bool> DeleteProductAsync(long productId, CancellationToken cancellationToken);
    }
}