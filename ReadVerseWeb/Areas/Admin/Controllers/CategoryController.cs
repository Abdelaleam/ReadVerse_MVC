using Microsoft.AspNetCore.Mvc;
using ReadVerse.DataAccess.Data;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;


namespace ReadVerseWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUintOfWork _unitOFWork;
        public CategoryController(IUintOfWork unitOFWork )
        {
            _unitOFWork = unitOFWork;
        }
        public IActionResult Index()
        {
            var objCategoryList = _unitOFWork.Category.GetAll().ToList();

            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if(obj.DisplayOrder.ToString()==obj.Name)
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOFWork.Category.Add(obj);
                _unitOFWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
             return View();  
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category= _unitOFWork.Category.Get(u=>u.Id==id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            
            if (ModelState.IsValid)
            {
                _unitOFWork.Category.Update(obj);
                _unitOFWork.Save();
                TempData["success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category = _unitOFWork.Category.Get(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category obj = _unitOFWork.Category.Get(u => u.Id == id);
          if (obj == null)
            {
                return NotFound();
            }
            _unitOFWork.Category.Remove(obj);
            _unitOFWork.Save();
            TempData["success"] = "Category deleted successfully";
           return RedirectToAction("Index");
        }
    }
}
