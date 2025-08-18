using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using ReadVerse.Models.ViewModel;

namespace ReadVerseWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
            private readonly IUintOfWork _unitOFWork;
            private readonly IWebHostEnvironment _webHostEnvironment;
            public ProductController(IUintOfWork unitOFWork,IWebHostEnvironment webHostEnvironment)
            {
                _unitOFWork = unitOFWork;
                _webHostEnvironment=webHostEnvironment;
            }
            public IActionResult Index()
            {
                var objProductList = _unitOFWork.Product.GetAll(includeProperties:"Category").ToList();
           
                return View(objProductList);
            }
            public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOFWork.Category.GetAll()
            .Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;
            ProductVM productVM = new()
            {
                CategoryList = CategoryList,
                product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);

            }
            else
            {
                productVM.product=_unitOFWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }
        }
            [HttpPost]
            public IActionResult Upsert(ProductVM productVM,IFormFile? file)
            {
               
                if (ModelState.IsValid)
                {
                string wwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename=Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string ProductPath=Path.Combine(wwRootPath,@"Images/Product");
                    if (!string.IsNullOrEmpty(productVM.product.ImageUrl))
                    {
                        var oldpath=Path.Combine(wwRootPath,productVM.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldpath))
                        {
                            System.IO.File.Delete(oldpath); 
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(ProductPath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.product.ImageUrl = @"\Images\Product\" + filename;
                }
                if (productVM.product.Id == 0)
                {
                    _unitOFWork.Product.Add(productVM.product);
                }
                else
                    _unitOFWork.Product.Update(productVM.product);
                    _unitOFWork.Save();
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction("Index");
                }
            else
            {

                productVM.CategoryList = _unitOFWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
               
                return View(productVM);


            }
        }

        #region API CALLS for datatables.net
        [HttpGet]
        public IActionResult GetAll()
        {
            var objProductList = _unitOFWork.Product.GetAll(includeProperties: "Category").ToList();
                return Json(new {data=objProductList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var ProductToRemvoed = _unitOFWork.Product.Get(u => u.Id == id);
            if(ProductToRemvoed == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            var oldpath = Path.Combine(_webHostEnvironment.WebRootPath, ProductToRemvoed.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldpath))
            {
                System.IO.File.Delete(oldpath);
            }
            _unitOFWork.Product.Remove(ProductToRemvoed);
            _unitOFWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

    }
}
