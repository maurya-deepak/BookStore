using Microsoft.AspNetCore.Mvc;
using BookStore.Repository;
using System.Collections.Generic;
using BookStore.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using BookStore.ViewModels;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly BookRepository _bookRepository = null;
        private readonly LanguageRepository _languageRepository = null;
        private readonly CategoryRepository _categoryRepository = null;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(BookRepository bookRepository,
        LanguageRepository languageRepository, IWebHostEnvironment webHostEnvironment,
        CategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _languageRepository = languageRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("getallbooks")]
        public async Task<ViewResult> GetAllBooks()
        {
            var data = await _bookRepository.GetAllBook();
            return View(data);

        }

        public async Task<ViewResult> GetBook(int id)
        {
            var bookVM = new BookVM();
            bookVM.book = await _bookRepository.GetBookById(id);
            if (bookVM.book != null)
            {
                bookVM.similarBooks = await _bookRepository.GetSimilarBook(bookVM.book.CategoryId, bookVM.book.Id);
            }
            return View(bookVM);
        }

        public List<BookModel> Search(string bookName, string authorName)
        {
            return _bookRepository.SearchBook(bookName, authorName);
        }

        [HttpGet]
        [Route("addnewbook")]
        public async Task<ViewResult> AddNewBook(bool isSuccess = false, int bookId = 0)
        {

            ViewBag.categories = new SelectList(await _categoryRepository.GetAllCategories(), "Id", "Name");

            ViewBag.languages = new SelectList(await _languageRepository.GetLanguages(), "Id", "Name");

            ViewBag.isSuccess = isSuccess;
            ViewBag.bookId = bookId;

            return View();
        }

        [HttpPost]
        [Route("addnewbook")]
        public async Task<IActionResult> AddNewBook(BookModel bookModel)
        {
            if (ModelState.IsValid)
            {
                if (bookModel.CoverPhoto != null)
                {
                    string path = "books/cover/";
                    bookModel.CoverImageUrl = await UploadFile(path, bookModel.CoverPhoto);
                }

                if (bookModel.GalaryImages != null)
                {
                    string path = "books/galary/";

                    bookModel.Galary = new List<GalaryModel>();

                    foreach (var image in bookModel.GalaryImages)
                    {
                        var galary = new GalaryModel
                        {
                            Name = image.FileName,
                            Url = await UploadFile(path, image)
                        };
                        bookModel.Galary.Add(galary);
                    }
                }

                if (bookModel.BookPdf != null)
                {
                    string path = "books/pdf/";
                    bookModel.BookPdfUrl = await UploadFile(path, bookModel.BookPdf);
                }

                int id = await _bookRepository.AddNewBook(bookModel);
                if (id > 0)
                {
                    return RedirectToAction(nameof(AddNewBook),
                    new { isSuccess = true, bookId = id });
                }
            }

            ViewBag.languages = new SelectList(await _languageRepository.GetLanguages(), "Id", "Name");

            return View();
        }

        private async Task<string> UploadFile(string location, IFormFile file)
        {
            location += Guid.NewGuid().ToString() + "_" + file.FileName;
            string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, location);
            await file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            return "/" + location;
        }
    }
}