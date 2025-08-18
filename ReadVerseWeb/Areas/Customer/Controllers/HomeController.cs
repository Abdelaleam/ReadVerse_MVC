using Microsoft.AspNetCore.Mvc;
using ReadVerse.DataAccess.Repository;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using System.Diagnostics;

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
            Product product = _uintOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category");
            return View(product);
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
