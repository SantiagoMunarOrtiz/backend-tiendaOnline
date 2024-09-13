using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnlineAPI.Data;
using TiendaOnlineAPI.Models;
using Microsoft.Extensions.Logging;

namespace TiendaOnlineAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products.Include(p => p.Category).AsNoTracking().ToListAsync();
                _logger.LogInformation("Listado de productos obtenido correctamente.");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado de productos.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Producto con ID {ProductId} no encontrado.", id);
                    return NotFound();
                }
                _logger.LogInformation("Producto con ID {ProductId} obtenido correctamente.", id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle del producto.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Agregar producto a lista de deseos
        [HttpPost("wishlist/{userId}")]
        public async Task<ActionResult> AddToWishList(int userId, [FromBody] int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Producto con ID {ProductId} no encontrado.", productId);
                    return NotFound();
                }

                var wishList = await _context.WishLists.Include(w => w.Products)
                                   .FirstOrDefaultAsync(w => w.UserId == userId);
                if (wishList == null)
                {
                    wishList = new WishList { UserId = userId, Products = new List<Product>() };
                    _context.WishLists.Add(wishList);
                }

                if (!wishList.Products.Contains(product))
                {
                    wishList.Products.Add(product);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Producto con ID {ProductId} agregado a la lista de deseos del usuario {UserId}.", productId, userId);
                }
                else
                {
                    _logger.LogWarning("El producto con ID {ProductId} ya está en la lista de deseos del usuario {UserId}.", productId, userId);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar el producto a la lista de deseos.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

     
        [HttpGet("wishlist/{userId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetWishList(int userId)
        {
            try
            {
                var wishList = await _context.WishLists
                                              .Include(w => w.Products)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wishList == null)
                {
                    _logger.LogWarning("Lista de deseos no encontrada para el usuario {UserId}.", userId);
                    return NotFound();
                }

                _logger.LogInformation("Lista de deseos obtenida correctamente para el usuario {UserId}.", userId);
                return Ok(wishList.Products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de deseos.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

       
        [HttpDelete("wishlist/{userId}/{productId}")]
        public async Task<ActionResult> RemoveFromWishList(int userId, int productId)
        {
            try
            {
                var wishList = await _context.WishLists
                                             .Include(w => w.Products)
                                             .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wishList == null)
                {
                    _logger.LogWarning("Lista de deseos no encontrada para el usuario {UserId}.", userId);
                    return NotFound();
                }

                var product = wishList.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    _logger.LogWarning("Producto con ID {ProductId} no encontrado en la lista de deseos del usuario {UserId}.", productId, userId);
                    return NotFound();
                }

                wishList.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Producto con ID {ProductId} eliminado de la lista de deseos del usuario {UserId}.", productId, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto de la lista de deseos.");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
