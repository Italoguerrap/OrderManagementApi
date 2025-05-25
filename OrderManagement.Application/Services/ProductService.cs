using AutoMapper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Services
{
    public class ProductService(IProductRepository productRepository, IMapper mapper) : IProductService
    {
        public async Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken cancellationToken)
        {
            Product product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price
            };

            product = await productRepository.AddAsync(product, cancellationToken);
            return mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(long productId, CancellationToken cancellationToken)
        {
            return await productRepository.DeleteAsync(productId, cancellationToken);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken)
        {
            List<Product> products = await productRepository.GetAllAsync(cancellationToken);
            return products.Select(p => mapper.Map<ProductDto>(p));
        }

        public async Task<ProductDto?> GetByIdAsync(long productId, CancellationToken cancellationToken)
        {
            Product product = await productRepository.GetByIdAsync(productId, cancellationToken);

            return product is null ? null : mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateProductAsync(long productId, ProductDto productDto, CancellationToken cancellationToken)
        {
            Product product;
            
            product = await productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new OrderManagementException("Produto não encontrado.");

            product.Name = productDto.Name;
            product.Price = productDto.Price;

            product = await productRepository.UpdateAsync(productId, product, cancellationToken);
            return mapper.Map<ProductDto>(product);
        }
    }
}