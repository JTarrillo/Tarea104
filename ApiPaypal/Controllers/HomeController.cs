﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WebApplicationpaypal_1.Models.Paypal_Order;
using WebApplicationpaypal_1.Models.Paypal_Transaction;

namespace WebApplicationpaypal_1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {

            //id de la autorizacion para obtener el dinero
            string token = Request.QueryString["token"];

            bool status = false;


            using (var client = new HttpClient())
            {

                //INGRESA TUS CREDENCIALES AQUI -> CLIENT ID - SECRET
                var userName = "AXAfJ0vgxisAiBt0jDATa_ZSc0X-EiL9GFIbtrdfsv4wex6iM8G1VcWlMwIG1DsFKacE4aD2t1SiY8w7";
                var passwd = "EOxhMgmZFAgfy6r7XmIRocfCz4MFZ6TPe8XPP03YGd9DK2RlvT7D8hlBpHAV72jZAmWRio9XSQG3Oqk0";

                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");

                var authToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                var data = new StringContent("{}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"/v2/checkout/orders/{token}/capture", data);


                status = response.IsSuccessStatusCode;

                ViewData["Status"] = status;
                if (status)
                {
                    var jsonRespuesta = response.Content.ReadAsStringAsync().Result;

                    PaypalTransaction objeto = JsonConvert.DeserializeObject<PaypalTransaction>(jsonRespuesta);

                    ViewData["IdTransaccion"] = objeto.purchase_units[0].payments.captures[0].id;
                }

            }


            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        //public JsonResult Paypal(string precio) ---> EDITAR POR LA LINEA DE ABAJO
        public async Task<JsonResult> Paypal(string precio, string producto) {



            bool status = false;
            string respuesta = string.Empty;

            using (var client = new HttpClient()) {

                //INGRESA TUS CREDENCIALES AQUI -> CLIENT ID - SECRET
                var userName = "";
                var passwd = "";

                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");

                var authToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));


                var orden = new PaypalOrder()
                {
                    intent = "CAPTURE",
                    purchase_units = new List<Models.Paypal_Order.PurchaseUnit>() {

                        new Models.Paypal_Order.PurchaseUnit() {

                            amount = new Models.Paypal_Order.Amount() { 
                                currency_code = "USD",
                                value = precio 
                            },
                            description = producto
                        }
                    },
                    application_context = new ApplicationContext()
                    {
                        brand_name = "Mi Tienda",
                        landing_page = "NO_PREFERENCE",
                        user_action = "PAY_NOW", //Accion para que paypal muestre el monto de pago
                        return_url = "https://localhost:44321/Home/About",// cuando se aprovo la solicitud del cobro
                        cancel_url = "https://localhost:44321/Home/Index"// cuando cancela la operacion
                    }
                };


                var json = JsonConvert.SerializeObject(orden);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/v2/checkout/orders", data);


                status = response.IsSuccessStatusCode;


                if (status)
                {
                    respuesta = response.Content.ReadAsStringAsync().Result;
                }



            }

            return Json(new { status = status, respuesta = respuesta }, JsonRequestBehavior.AllowGet);

        }

          



    }
}