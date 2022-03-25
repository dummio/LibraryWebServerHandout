﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryWebServer;
using LibraryWebServer.Models;

namespace LibraryWebServer.Controllers
{
    public class HomeController : Controller
    {
        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;
        private static Team68LibraryContext db = new Team68LibraryContext();

        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin(string name, int cardnum)
        {
            bool loginSuccessful = false;
            var query = from p in db.Patrons
                        where p.Name.Equals(name) && p.CardNum == cardnum select p;
            if (query.Count() > 0)
                loginSuccessful = true;
            if (!loginSuccessful)
            {
                return Json(new { success = false });
            }
            else
            {
                user = name;
                card = cardnum;
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json(new { success = true });
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {

            // TODO: Implement
            var query = from t in db.Titles
                        join i in db.Inventory on t.Isbn equals i.Isbn into ti
                        from x in ti.DefaultIfEmpty()
                        join c in db.CheckedOut on x.Serial equals c.Serial into co
                        from y in co.DefaultIfEmpty()
                        join p in db.Patrons on y.CardNum equals p.CardNum into pat
                        from k in pat.DefaultIfEmpty()
                        select new
                        {
                            isbn = x == null ? null : x.Isbn,
                            title = t == null ? null : t.Title,
                            author = t == null ? null : t.Author,
                            serial = y == null ? null : (uint?)y.Serial,
                            name = k == null ? null : k.Name
                        };

            return Json(query.ToArray());

        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            var query = from c in db.CheckedOut where c.CardNum == card
                        join i in db.Inventory on c.Serial equals i.Serial into co
                        from x in co.DefaultIfEmpty()
                        join t in db.Titles on x.Isbn equals t.Isbn into tit
                        from y in tit.DefaultIfEmpty()
                        select new
                        {
                            title = y == null ? null : y.Title,
                            author = y == null ? null : y.Author,
                            serial = c == null ? null : (uint?) c.Serial
                        };
            // TODO: Implement
            return Json(query.ToArray());
        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook(int serial)
        {
            // You may have to cast serial to a (uint)
            CheckedOut newBook = new CheckedOut();
            newBook.Serial = (uint)serial;
            newBook.CardNum = (uint)card;
            db.CheckedOut.Add(newBook);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }
           // var query = insert into db.CheckedOut


            
        }


        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook(int serial)
        {
            // You may have to cast serial to a (uint)
            var query = from co in db.CheckedOut
                        where co.CardNum == card && co.Serial == serial
                        select co;
            foreach(CheckedOut c in query)
            {
                db.CheckedOut.Remove(c);
            }
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }catch(Exception e){
                return Json(new { success = false });
            }
        }

        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/

        /// <summary>
        /// Return the home page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the About page.
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }


        /// <summary>
        /// Return the Contact page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Return the Error page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

