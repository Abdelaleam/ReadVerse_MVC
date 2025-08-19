using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using ReadVerse.Models.ViewModel;
using ReadVerse.Utility;

namespace ReadVerseWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
            private readonly IUintOfWork _unitOFWork;
            public CompanyController(IUintOfWork unitOFWork)
            {
                _unitOFWork = unitOFWork;
            }
            public IActionResult Index()
            {
                var objCompanyList = _unitOFWork.Company.GetAll().ToList();
           
                return View(objCompanyList);
            }
            public IActionResult Upsert(int? id)
             {
            if (id == null || id == 0)
            {
                //create
                return View(new Company());

            }
            else
            {
                Company company=_unitOFWork.Company.Get(u=>u.Id==id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {

                if (company.Id == 0)
                {
                    _unitOFWork.Company.Add(company);
                }
                else
                {
                    _unitOFWork.Company.Update(company);
                }
                _unitOFWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View(company);

        }

        #region API CALLS for datatables.net
        [HttpGet]
        public IActionResult GetAll()
        {
            var objCompanyList = _unitOFWork.Company.GetAll().ToList();
                return Json(new {data=objCompanyList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyToRemvoed = _unitOFWork.Company.Get(u => u.Id == id);
            if(CompanyToRemvoed == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            
            _unitOFWork.Company.Remove(CompanyToRemvoed);
            _unitOFWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

    }
}
