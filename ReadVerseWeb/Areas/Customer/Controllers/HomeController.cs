using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadVerse.DataAccess.Repository;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ReadVerseWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUintOfWork _uintOfWork;
        public HomeController(ILogger<HomeController> logger,IUintOfWork uintOfWork)
        {
            _logger = logger;
            _uintOfWork = uintOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _uintOfWork.Product.GetAll(includeProperties:"Category");
            return View(products);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _uintOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId=productId

            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart cartFromDb = _uintOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _uintOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _uintOfWork.ShoppingCart.Add(shoppingCart);
            }
            TempData["success"] = "cart updated successfully";

            _uintOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
