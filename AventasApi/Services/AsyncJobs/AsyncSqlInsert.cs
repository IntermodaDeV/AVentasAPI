using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AventasApi.ImageManager;
using DBData.Database;
//using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;

namespace AventasApi.Services.AsyncJobs
{
    public static class AsyncSqlInsert
    {
        public static TaskFactory factory;
        static AsyncSqlInsert()
        {
            factory = new TaskFactory();
        }
        public static  void IngresarPedido(PedidosxCliente pedido, string firma)
        {
            var pedidoTask = Task.Run(() =>
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    context.PedidosxCliente.Add(pedido);
                    context.SaveChanges();
                    ByteArrayImageConversion firmaConversion = new ByteArrayImageConversion(firma);
                    if (firmaConversion.IsSuccesful)
                    {

                        FirmasxPedido firmaAGuardar = new FirmasxPedido
                        {
                            PedidoId = pedido.PedidoId,
                            Firma = firmaConversion.ContentByteArray
                        };
                        context.FirmasxPedido.Add(firmaAGuardar);
                        context.SaveChanges();
                    }
                }

            });
        }
        public static  void IngresarRecibos(List<RecibosxClienteViewModel> recibos)
        {
            var reciboTask = Task.Run(() =>
            {

                var reciboAAgregar = recibos.Select(rec => new RecibosxCliente
                {
                    NumeroRecibo = rec.NumeroRecibo,
                    CodigoCliente = rec.CodigoCliente,
                    Fecha = rec.Fecha,
                    IdTipoPago = rec.IdTipoPago,
                    Referencia = rec.Referencia,
                    FechaCheque = rec.FechaPago,
                    IdBanco = rec.IdBanco,
                    Valor = rec.Valor,
                    IdMoneda = rec.IdMoneda,
                    Sincronizado = rec.Sincronizado,
                    CodigoAsesor = rec.CodigoAsesor,
                    IdFactura = rec.IdFactura,
                    Descuento = rec.Descuento,
                    Longitude = rec.Longitude,
                    Latitude = rec.Latitude,
                    SpecPago = rec.SpecPago,
                    RecibosDetalle = rec.DetalleRecibo.Select(recDet => new RecibosDetalle
                    {
                        IdReciboDetalle = recDet.IdReciboDetalle,
                        ReciboId = recDet.ReciboId,
                        IdSubFactura = recDet.IdSubFactura,
                        Valor = recDet.Valor,
                        Descuento = recDet.Descuento,
                        EsAbono = recDet.EsAbono,
                    }).ToList()
                }).ToList();

                using (AVentasEntities context = new AVentasEntities())
                {
                    context.RecibosxCliente.AddRange(reciboAAgregar);
                    context.SaveChanges();

                }

            });
        }
    }
}