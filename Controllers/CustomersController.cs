using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vidly.Models;
using Vidly.ViewModels;
using System.Data.Entity;

namespace Vidly.Controllers
{
    public class CustomersController : Controller
    {
        private ApplicationDbContext _context;

        public CustomersController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult New()
        {
            var membershipTypes = _context.MembershipTypes.ToList();
            var customer = new Customer
            {
                Id = 0
            };
            var viewModel = new CustomerFormViewModel
            {
                MembershipTypes = membershipTypes,
                Customer = customer
            };
            return View("CustomerForm", viewModel);
        }

        //so it can only be used using HttpPost and not HttpGet (whatever that means)
        //USE WHEN YOUR ACTIONS MODIFY DATA IN THE DATABASE, NEVER USE HTTPGET WHEN MODIFYING DATABASE
        [HttpPost]
        //framework binds this viewModel to the request data
        [ValidateAntiForgeryToken]
        public ActionResult Save(Customer customer)
        {
            
            if (!ModelState.IsValid)
            {
                var viewModel = new CustomerFormViewModel
                {
                    Customer = customer,
                    MembershipTypes = _context.MembershipTypes.ToList()
                };
                return View("CustomerForm", viewModel);
            }

            //"if its a new customer"
            if (customer.Id == 0)
            {
                //not added yet, only in memory
                _context.Customers.Add(customer);
            }
            else //editing an existing customer
            {
                //retrieve the customer with the same Id
                var customerInDb = _context.Customers.Single(c => c.Id == customer.Id);

                //customerInDb is the customer in the dataset, customer holds the new values that should be passed to it
                customerInDb.Name = customer.Name;
                customerInDb.Birthdate = customer.Birthdate;
                customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;
                customerInDb.MembershipTypeId = customer.MembershipTypeId;
            }
            
            //now the add is committed to the database
            //generates SQL statements at runtime to add the item to the database
            _context.SaveChanges();
            //go to index action in customers controller
            return RedirectToAction("Index", "Customers");
        }

        public ActionResult Index()
        {
            //loads the MembershipType object associated with the customer together to put in the view
            var customers = _context.Customers.Include(c => c.MembershipType).ToList();

            return View(customers);
        }

        [Route("customers/details/{id}")]
        public ActionResult Details(int id)
        {
            //SingleOrDefault: returns a specific element in the sequence or a default value if that element is not found
            var customer = _context.Customers.Include(c => c.MembershipType).SingleOrDefault(c => c.Id == id);
            
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        public ActionResult Edit(int id)
        {
            var customer = _context.Customers.SingleOrDefault(c => c.Id == id);
             
            if (customer == null)
                return HttpNotFound();

            var viewModel = new CustomerFormViewModel
            {
                Customer = customer,
                MembershipTypes = _context.MembershipTypes.ToList()
            };

            //overwrites the standard behavior and goes to "CustomerForm" View instead
            return View("CustomerForm", viewModel);
            
        }
    }
}