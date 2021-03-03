using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShopifyWebhook.Data;
using ShopifyWebhook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyWebhook.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Orders> Get()
        {
            return _context.Orders.Where(o => o.fulfillment_id != null).ToList();
        }

        [HttpPost]
        [Route("OrderUpdate")]
        public IActionResult OrderUpdate()
        {
            Guid guid = Guid.NewGuid();
            DateTime start = DateTime.Now;
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var result = reader.ReadToEndAsync();

                    Orders order = JsonConvert.DeserializeObject<Orders>(result.Result);

                    Orders orderOriginal = _context.Orders.Find(order.id);

                    if (orderOriginal != null)
                    {
                        orderOriginal.updated_at = DateTime.Now;
                        orderOriginal.financial_status = order.financial_status;
                        orderOriginal.fulfillment_status = order.fulfillment_status;
                        if (order.fulfillments.Count > 0)
                            orderOriginal.fulfillment_id = order.fulfillments[0].id;

                        if (order.financial_status == "paid" && orderOriginal.status == "Pedido recibido")
                        {
                            orderOriginal.status = "Pago confirmado";
                            OrderStatus status = createState(orderOriginal.id, "Pago confirmado");
                            _context.OrderStatus.Add(status);
                        }

                        _context.Orders.Update(orderOriginal);                        
                        _context.SaveChanges();
                    }

                    DateTime end = DateTime.Now;
                    RegisterLog(start, end, guid, "Actualización de órdenes", $"Se ha actualizado la orden {order.id}", true);
                    return Ok();
                }
            }
            catch(Exception e)
            {
                RegisterLogError(e.Message, guid);
                DateTime end = DateTime.Now;
                RegisterLog(start, end, guid, "Actualización de órdenes", "No se pudo actualizar", false);
                return NotFound();
            }
        }

        [HttpPost]
        [Route("OrderCancel")]
        public IActionResult OrderCancel()
        {
            Guid guid = Guid.NewGuid();
            DateTime start = DateTime.Now;
            try
            {
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var result = reader.ReadToEndAsync();

                    Orders order = JsonConvert.DeserializeObject<Orders>(result.Result);

                    Orders orderOriginal = _context.Orders.Find(order.id);

                    if (orderOriginal != null)
                    {
                        orderOriginal.updated_at = order.cancelled_at;

                        orderOriginal.status = "Cancelado";

                        OrderStatus status = createState(orderOriginal.id, "Cancelado");
                        switch(order.cancel_reason)
                        {
                            case "customer":
                                orderOriginal.fechaEstimada = "Cancelado por el cliente";
                                break;
                            case "inventory":
                                orderOriginal.fechaEstimada = "Cancelado por falta de stock";
                                break;
                            case "fraud":
                                orderOriginal.fechaEstimada = "Cancelado por fraude";
                                break;
                            case "declined":
                                orderOriginal.fechaEstimada = "Cancelado";
                                break;
                        }

                        _context.Orders.Update(orderOriginal);
                        _context.OrderStatus.Add(status);
                        _context.SaveChanges();
                    }

                    DateTime end = DateTime.Now;
                    RegisterLog(start, end, guid, "Cancelar órden", $"Se canceló la orden {order.id}", true);
                    return Ok();
                }
            }
            catch (Exception e)
            {
                RegisterLogError(e.Message, guid);
                DateTime end = DateTime.Now;
                RegisterLog(start, end, guid, "Cancelar órden", "No se pudo cancelar", false);
                return NotFound();
            }
        }

        public OrderStatus createState(string orderId, string status)
        {
            OrderStatus stat = new OrderStatus();
            stat.CreateDate = DateTime.Now;
            stat.OrderId = orderId;
            stat.Status = status;
            return stat;
        }

        public void RegisterLog(DateTime start, DateTime end, Guid guid, string processName, string detail, bool status)
        {
            Logs log = new Logs();
            log.DateStart = start;
            log.DateEnd = end;
            log.Name = processName;
            log.Detail = detail;
            log.Id = guid;
            if (status)
                log.Status = "Completado";
            else
                log.Status = "Con errores";
            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void RegisterLogError(string error, Guid guid)
        {
            LogDetail detail = new LogDetail();
            detail.Error = error;
            detail.LogId = guid;
            _context.LogDetail.Add(detail);
            _context.SaveChanges();
        }
    }
}
