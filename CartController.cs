using System.Runtime.CompilerServices;
using System.Security.Claims;
using Bulky.data.Irepository;
using Bulky.data.Irepository.Repository;
using Bulky.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Stripe.Checkout;


using ShopCartVm = Bulky.models.ShoppingCartVM;

namespace BlukyWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly IunitOfWork _unitOfWork;


        public CartController(IunitOfWork iunitOfWork)

        {
            //_context = db;
            _unitOfWork = iunitOfWork;
           
        }


        public IActionResult Index()
        {

            //List<Category> objcategoryList = _context.categories.ToList();



            List<ShoppingCart> objcategoryList = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").ToList();
            var totalSum = objcategoryList.Sum(item => item.Product.Price * item.Count);
            ViewBag.TotalSum = totalSum;
            return View(objcategoryList);
        }
       





        //if (user != null && user.CompanyId.HasValue && user.CompanyId.Value != 0)
        //{
        //    orderHeader.OrderStatus = "Approved";
        //    orderHeader.PaymentStatus = "Approved";
        //}
        //else
        //{
        //    orderHeader.OrderStatus = "Pending";
        //    orderHeader.PaymentStatus = "Pending";
        //}


        [HttpPost]
        public async Task<IActionResult> Summary(OrderHeader orderHeader)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

           

            // Retrieve the shopping cart items from the database
            var shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId, includeProperties: "Product"
            ).ToList();

            // Calculate the order total
            orderHeader.OrderTotal = shoppingCartItems.Sum(item => item.Product.Price * item.Count);

            // Set other order header properties
            orderHeader.ApplicationUserId = userId;
            orderHeader.OrderDate = DateTime.Now;
            orderHeader.ShippingDate = DateTime.Now.AddDays(7); // Example shipping date
            ApplicationUser user = _unitOfWork.applicationUser.Get(u => u.Id == userId);


            if (user != null && user.CompanyId.HasValue && user.CompanyId.Value != 0)
            {
                orderHeader.OrderStatus = "Approved";
                orderHeader.PaymentStatus = "Approved";
            }
            else
            {
                orderHeader.OrderStatus = "Pending";
                orderHeader.PaymentStatus = "Pending";
            }





            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = Url.Action("abcd", "Cart", null, Request.Scheme), // Adjust URL to your success action
                CancelUrl = Url.Action("Index", "Cart", null, Request.Scheme),
            };

            foreach (var item in shoppingCartItems)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100), // Convert price to cents
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            
            orderHeader.SessionId = session.Id;
            orderHeader.PaymentIntentId = session.PaymentIntentId;

            
            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Save();

            // Clear the shopping cart
            foreach (var item in shoppingCartItems)
            {
                _unitOfWork.ShoppingCart.Remove(item);
            }
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult abcd()
        {
            return View();
        }











        // Fetch shopping cart items including related Product entities for the user
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value; // Example method to retrieve UserId from Claims
            var shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(
                 u => u.ApplicationUserId == userId,includeProperties:"Product"
                
            ).ToList();

           
            var totalSum = shoppingCartItems.Sum(item => item.Product.Price * item.Count);
            ViewBag.TotalSum = totalSum;

            return View(shoppingCartItems);


        }
              








            public IActionResult pluss(int id)
        {
            var cartfromdb = _unitOfWork.ShoppingCart.Get(u=>u.Id==id);
            Console.WriteLine(cartfromdb);
            Console.WriteLine(id);
            Console.WriteLine(_unitOfWork.ShoppingCart);
            cartfromdb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartfromdb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult minuss(int cartId)
        {
            var cartfromdb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartfromdb.Count < 2)
            {
                _unitOfWork.ShoppingCart.Remove(cartfromdb);



            }
            else
            {
                cartfromdb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartfromdb);

            }
           
        
           
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult delete(int cartId)
        {
            var cartfromdb=_unitOfWork.ShoppingCart.Get(u=>u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartfromdb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

    }
}
