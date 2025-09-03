using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using ReadVerse.DataAccess.Repository.IRepository;
using ReadVerse.Models;
using ReadVerse.Models.ViewModel;
using ReadVerse.Utility;
using Stripe.Checkout;
using System.Security.Claims;

namespace ReadVerseWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize(Roles = SD.Role_Customer)]
    public class CartController : Controller
    {
        //Dip
        private readonly IUintOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUintOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                orderHeader=new()
            
            };
            foreach (var cart in ShoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += cart.Price * cart.Count;
                
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                orderHeader = new()

            };
            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);
            ShoppingCartVM.orderHeader.Name = ShoppingCartVM.orderHeader.ApplicationUser.Name;
            ShoppingCartVM.orderHeader.PhoneNumber = ShoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.orderHeader.StreetAddress = ShoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.orderHeader.City = ShoppingCartVM.orderHeader.ApplicationUser.City;
            ShoppingCartVM.orderHeader.State = ShoppingCartVM.orderHeader.ApplicationUser.State;
            ShoppingCartVM.orderHeader.PostalCode = ShoppingCartVM.orderHeader.ApplicationUser.PostalCode;
            foreach (var cart in ShoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += cart.Price * cart.Count;

            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            ShoppingCartVM.orderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.orderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);
          foreach (var cart in ShoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += cart.Price * cart.Count;

            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.orderHeader.OrderStatus = SD.StatusPending;
            }
            else
            { 
                ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.orderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.orderHeader.Add(ShoppingCartVM.orderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.shoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.orderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.orderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //customer
                var domain = "https://localhost:7033/";

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.orderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach(var item in ShoppingCartVM.shoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);

                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.orderHeader.UpdateStripePaymentId(ShoppingCartVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.orderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
             if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
          }
             List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cart=_unitOfWork.ShoppingCart.Get(u=>u.Id==cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cart);
            }
                _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId); 
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        private double GetPriceBasedOnQuantity (ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;
            else
            {
                if(shoppingCart.Count <=100)
                    return shoppingCart.Product.Price50 ;
                else
                    return shoppingCart.Product.Price100;
            }
            
        }
    }
}
