using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db)

        {
            _db = db;
        }
        
        

        // GET api/<ProductController>/5
        [HttpGet("[action]")]
        [Authorize(Policy = "RequiredLogin")]
        public IActionResult GetProducts()
        {
            
            return Ok(_db.Products.ToList());
        }

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel formdata) {
            var newProduct = new ProductModel {
                Name=formdata.Name,
                ImgURL=formdata.ImgURL,
                Description=formdata.Description,
                OutOfStock=formdata.OutOfStock,
                Price=formdata.Price
            };

            await _db.Products.AddAsync(newProduct);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id,[FromBody] ProductModel formdata)
        {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var findproduct = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (findproduct == null) {
                return NotFound();
            }

            findproduct.Name = formdata.Name;
            findproduct.ImgURL = formdata.ImgURL;
            findproduct.Description = formdata.Description;
            findproduct.OutOfStock = formdata.OutOfStock;
            findproduct.Price = formdata.Price;

            _db.Entry(findproduct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok(new JsonResult ( "The product with Id"+id+" is updated"));


        }
        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var findproduct =await _db.Products.FindAsync(id);
            if (findproduct == null)
            {
                return NotFound();
            }

            _db.Products.Remove(findproduct);
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("The product with Id" + id + " is deleted"));

        }

        }
}
