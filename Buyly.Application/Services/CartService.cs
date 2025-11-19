using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Cart;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;

namespace Buyly.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            if (product.Stock < dto.Quantity)
            {
                throw new ValidationException("Insufficient stock for the requested product");
            }

            var existingCartItem = await _cartRepository.GetUserCartItemByProductIdAsync(userId, dto.ProductId);
            if (existingCartItem != null)
            {
                var updatedQuantity = existingCartItem.Quantity + dto.Quantity;
                if (updatedQuantity > product.Stock)
                {
                    throw new ValidationException("Insufficient stock for the requested product");
                }

                existingCartItem.Quantity = updatedQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
                existingCartItem.Product = product;

                _cartRepository.Update(existingCartItem);
                await _unitOfWork.CommitAsync();
                return MapToCartItemDto(existingCartItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    PriceAtAdd = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                await _cartRepository.AddAsync(cartItem);
                await _unitOfWork.CommitAsync();

                cartItem.Product = product;
                return MapToCartItemDto(cartItem);
            }

        }

        public async Task ClearCartAsync(string userId)
        {
            await _cartRepository.ClearUserCartAsync(userId);
            await _unitOfWork.CommitAsync();
        }

        public async Task RestoreCartFromOrderAsync(string userId, IEnumerable<OrderItem> orderItems)
        {
            if (string.IsNullOrWhiteSpace(userId) || orderItems == null)
            {
                return;
            }

            var items = orderItems.ToList();
            if (!items.Any())
            {
                return;
            }

            var existingItems = (await _cartRepository.GetUserCartItemsAsync(userId))
                .ToDictionary(ci => ci.ProductId, ci => ci);

            foreach (var orderItem in items)
            {
                if (existingItems.TryGetValue(orderItem.ProductId, out var cartItem))
                {
                    cartItem.Quantity = orderItem.Quantity;
                    cartItem.PriceAtAdd = orderItem.UnitPrice;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                    _cartRepository.Update(cartItem);
                }
                else
                {
                    var newCartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = orderItem.ProductId,
                        Quantity = orderItem.Quantity,
                        PriceAtAdd = orderItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _cartRepository.AddAsync(newCartItem);
                }
            }

            await _unitOfWork.CommitAsync();
        }

        public async Task<CartDto?> GetUserCartAsync(string userId)
        {
            var cartItems = (await _cartRepository.GetUserCartItemsAsync(userId)).ToList();
            if (!cartItems.Any())
            {
                return new CartDto();
            }

            var productIds = cartItems.Select(ci => ci.ProductId).Distinct().ToList();
            var products = await _productRepository.GetProductsByIdsAsync(productIds);

            foreach (var cartItem in cartItems)
            {
                if (products.TryGetValue(cartItem.ProductId, out var product))
                {
                    cartItem.Product = product;
                }
            }

            var itemsDto = cartItems.Select(MapToCartItemDto).ToList();
            return new CartDto
            {
                Items = itemsDto,
                TotalItems = itemsDto.Sum(i => i.Quantity),
                Total = itemsDto.Sum(i => i.Subtotal)
            };
        }

        public async Task<bool> RemoveCartItemAsync(string userId, Guid cartItemId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.UserId != userId)
            {
                return false;
            }

            _cartRepository.Delete(cartItem);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<CartItemDto> UpdateCartItemAsync(string userId, Guid cartItemId, UpdateCartItemDto dto)
        {
            var cartItemsDict = await _cartRepository.GetUserCartItemsByIdsAsync(userId, new[] { cartItemId });
            if (!cartItemsDict.TryGetValue(cartItemId, out var cartItem))
            {
                throw new NotFoundException("Cart item not found");
            }

            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            if (dto.Quantity > product.Stock)
            {
                throw new ValidationException("Insufficient stock for the requested product");
            }

            cartItem.Quantity = dto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.Product = product;
            _cartRepository.Update(cartItem);

            await _unitOfWork.CommitAsync();
            return MapToCartItemDto(cartItem);
        }

        private static CartItemDto MapToCartItemDto(CartItem cartItem)
        {
            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product?.Name ?? string.Empty,
                ProductImageUrl = cartItem.Product?.ImageUrl,
                Price = cartItem.PriceAtAdd,
                Quantity = cartItem.Quantity,
                Subtotal = cartItem.PriceAtAdd * cartItem.Quantity
            };
        }
    }
}