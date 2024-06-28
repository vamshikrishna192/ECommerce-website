using Bulky.data.data;
using Bulky.models;
using Microsoft.AspNetCore.Mvc;
using Bulky.data.Irepository.Repository;
using Bulky.data.Irepository;


namespace BlukyWeb.Controllers
{
    public class CategoryController : Controller
    {

        //private readonly AppDbContext _context;
        private readonly IunitOfWork _unitofwork;
        public CategoryController(IunitOfWork iunitOfWork)
        {
            //_context = db;
            _unitofwork = iunitOfWork;

        }
        public IActionResult Index()
        {
            //List<Category> objcategoryList = _context.categories.ToList();
            List<Category> objcategoryList = _unitofwork.Category.GetAll().ToList();

            return View(objcategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Editss(int id)
        {

            //var categoryfromdbs = _context.categories.Find(id);
            var categoryFromDb = _unitofwork.Category.Get(u => u.Id == id);

            // Check if the category exists
            //if (categoryfromdb == null)
            //{

            //	return NotFound();
            //}


            return View(categoryFromDb);
        }

        // HTTP POST action to handle the edit form submission
        [HttpPost]

        public IActionResult Editss(Category model)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Update the category in the database
                _unitofwork.Category.Update(model);
                _unitofwork.Save();
                TempData["success"] = "category edited succesfully";

                // Redirect to the index action (list of categories) upon successful update
                return RedirectToAction("Index");


            }


            return View(model);
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                //_context.categories.Add(obj);
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                //_context.SaveChanges();
                TempData["success"] = "category create succesfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Delete(int Id)
        {
            var categoryToDelete = _unitofwork.Category.Get(u => u.Id == Id);
            if (categoryToDelete == null)
            {
                return NotFound();
            }


            return View(categoryToDelete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int Id)
        {
            var categoryToDelete = _unitofwork.Category.Get(u => u.Id == Id);
            if (categoryToDelete == null)
            {
                return NotFound();
            }

            _unitofwork.Category.Remove(categoryToDelete);
            _unitofwork.Save();
            //

            return RedirectToAction("Index");
        }


    }
}
